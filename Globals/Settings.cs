using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Vulcain.Core.Utils;

namespace Vulcain.Core
{

    internal class SettingsData
    {
        public string Mode;
        public Dictionary<string, object> Stubs;
        public Dictionary<string, object> Config;
        public Dictionary<string, object> Alias;

        public SettingsData(Dictionary<string, object> obj=null)
        {
            Mode = obj?["Mode"] as string;
            Stubs = obj?["Stubs"] as Dictionary<string, object> ?? new Dictionary<string, object>();
            Config = obj?["Config"] as Dictionary<string, object> ?? new Dictionary<string, object>();
            Alias = obj?["Alias"] as Dictionary<string, object> ?? new Dictionary<string, object>();
        }
    }

    /**
     * Manage local file settings
     *
     * {
     *
     * }
     */
    internal class Settings
    {
        private SettingsData data;

        internal Settings()
        {
            this.ReadContext();
        }

        public string Stage
        {
            get {
                return this.data.Mode;
            }
        }

        internal Dictionary<string, dynamic> StubSessions
        {
            get
            {
                return this.data.Stubs;
            }
        }

        internal async Task SaveStubSessions(Dictionary<string, object> stubSessions) {
            if (this.data?.Stubs == null) // production
                return;

            try {
                this.data.Stubs["Sessions"] = stubSessions;
                var path = Files.FindConfigurationFile();
                if (path != null)
                    return;

                await System.IO.File.WriteAllTextAsync(path, JSON.Stringify( this.data  ));
            }
            catch (Exception e) {
                Service.Log.Error(null, e, () => "VULCAIN MANIFEST : Error when savings stub sessions.");
            }
        }

        /**
         * Read configurations from .vulcain file
         * Set env type from environment variable then .vulcain config
         *
         * @private
         */
        private void ReadContext()
        {
            this.data = new SettingsData();

            // Default mode is local
            this.data.Mode = Environment.GetEnvironmentVariable(Conventions.Instance.ENV_VULCAIN_STAGE) ?? "local";
            if (this.Stage == "production")
            {
                // Settings are ignored in production
                return;
            }

            try
            {
                var path = Files.FindConfigurationFile();
                if (path != null)
                {
                    var data = File.ReadAllText(path);
                    var obj = JSON.Parse(data) as Dictionary<string, object>;
                    if (obj != null)
                    {
                        var mode = this.data.Mode;
                        this.data = new SettingsData(obj);
                        this.data.Mode = (mode ?? this.data.Mode).ToLower();
                    }
                }
            }
            catch
            {
                throw new Exception("VULCAIN MANIFEST : Loading error");
            }

            if (this.Stage != "test" && this.Stage != "local")
            {
                throw new Exception("Invalid staging environment. Should be 'production', 'test' or 'local'");
            }
        }

        /**
         * Check if the service is running in local mode
         *
         * @readonly
         * @static
         *
         * @memberOf System
         */
        internal bool IsDevelopment
        { get {
                return this.Stage == "local";
            }
        }

        /**
         * Check if the current service is running in a test environnement (VULCAIN_TEST=true)
         *
         * @static
         * @returns
         *
         * @memberOf System
         */
        internal bool IsTestEnvironnment
        {
            get {
                return this.IsDevelopment || this.Stage == "test";
            }
        }

        /**
         * Resolve un alias (configuration key shared/$alternates/name-version)
         *
         * @param {string} name
         * @param {string} [version]
         * @returns null if no alias exists
         *
         * @memberOf System
         */
        internal object GetAlias(string name, string version = null) {
            if (name == null || !Service.IsDevelopment)
                return null;

            // Try to find an alternate uri
            var alias = this.data.Alias[name];
            if (alias != null) {
                if (alias is string) {
                    return alias;
                }
                var dic = alias as Dictionary<string, object>;
                alias = dic?[version];
                if (alias != null)
                    return alias;
            }

            return null;
        }
    }

}
