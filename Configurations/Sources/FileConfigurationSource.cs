using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vulcain.Core.Configuration;

namespace Vulcain.Core.Configuration.Sources
{
    public enum ConfigurationDataType
    {
        KeyValue,
        Json,
        VulcainConfig
    }

    public class FileConfigurationSource : ILocalConfigurationSource
    {
        private Dictionary<string, ConfigurationItem> _values = new Dictionary<string, ConfigurationItem>();

        private bool _disabled = false;
        private string _path;
        private ConfigurationDataType _mode;

        public FileConfigurationSource(string path, ConfigurationDataType mode = ConfigurationDataType.Json)
        {
            _path = path;
            _mode = mode;

            if (path == null)
            {
                this._disabled = true;
                return;
            }

            try
            {
                if (!File.Exists(this._path))
                {
                    Service.Log.Info(null, () => "CONFIGURATIONS : File " + path + " doesn't exist.");
                    this._disabled = true;
                }
            }
            catch (Exception e)
            {
                Service.Log.Error(null, e, () => "Invalid path when reading file configuration source at " + this._path + ". Are you using an unmounted docker volume ?");
            }
        }

        public PropertyValue Get(string name)
        {
            if (!this._values.TryGetValue(name, out ConfigurationItem item) && !item.Deleted)
            {
                return new PropertyValue(item.Value);
            }
            return PropertyValue.Undefined;
        }

        protected async Task<bool> ReadJsonValues(bool vulcainConfig)
        {
            try
            {
                var data = await File.ReadAllTextAsync(this._path);
                var obj = JSON.Parse(data) as Dictionary<string,object>;
                obj = obj != null && vulcainConfig ? obj["config"] as Dictionary<string,object> : obj;
                if (obj != null)
                {
                    foreach (var kv in obj)
                    {
                        try
                        {
                            if (kv.Value is string)
                            {
                                this.UpdateValue(kv.Key, kv.Value as string, false);
                            }
                            else
                            {
                                var dic = kv.Value as Dictionary<string, object>;
                                if (dic != null)
                                {
                                    var val = dic["value"];
                                    var encrypted = dic["encrypted"] == true;
                                    this.UpdateValue(kv.Key, val, encrypted);
                                }
                            }
                        }
                        catch
                        {
                        }

                    }
                }
                return true;
            }
            catch (Exception err)
            {
                Service.Log.Error(null, err, () => "File configuration source - Error when reading json values");
            }

            return false;
        }


        protected async Task<bool> ReadKeyValues()
        {
            var re = new Regex(@"/^\s*([\w\$_][\d\w\._\-\$] *)\s *=\s * (.*) /");

            try
            {
                var lines = await File.ReadAllLinesAsync(this._path);
                foreach (var line in lines)
                {
                    if (String.IsNullOrWhiteSpace(line))
                        continue;

                    try
                    {
                        var m = re.Match(line);
                        if (m.Success)
                        {
                            if (m.Groups[2]?.Value == null)
                                continue;

                            var encrypted = false;
                            var val = m.Groups[2].Value.Trim().Trim('"');
                            if (val != null && val[0] == '!')
                            {
                                val = val.Substring(1);
                                encrypted = true;
                            }

                            this.UpdateValue(m.Groups[1].Value, val, encrypted);
                        }
                    }
                    catch (Exception err)
                    {
                        Service.Log.Error(null, err, () => $"File configuration source - Error when reading key values line { line}");
                    }
                }
                return true;
            }
            catch (Exception err)
            {
                Service.Log.Error(null, err, () => "File configuration source - Error when reading key values");
                return false;
            }
        }

        protected void UpdateValue(string name, object value, bool encrypted)
        {
            this._values.TryAdd(name, new ConfigurationItem { Value = encrypted ? Service.Decrypt((string)value) : value, Encrypted = encrypted, Key = name });
            var v = encrypted ? "********" : value;
            Service.Log.Info(null, () => $"CONFIG: Setting property value '{v}' for key { name}");
        }

        public async Task<DataSource> ReadProperties(int timeout = 0)
        {
            if (!this._disabled)
            {
                if (File.Exists(this._path))
                {
                    if (this._mode == ConfigurationDataType.KeyValue)
                        await this.ReadKeyValues();
                    else
                        await this.ReadJsonValues(this._mode == ConfigurationDataType.VulcainConfig);

                    return new DataSource(this._values.Values);
                }
            }
            return null;
        }
    }
}
