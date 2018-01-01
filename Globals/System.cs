using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Vulcain.Core.Configuration;
using Vulcain.Core.Stubs;
using Vulcain.Core.Utils;

namespace Vulcain.Core
{

    /**
     * Static class providing service helper methods
     *
     * @export
     * @class System
     */
    public class Service
    {
        private static Settings _settings;
        private static string _vulcainServer;
        private static string _vulcainToken;
        private static ILogger logger;
        private static string _serviceName;
        private static string _serviceVersion;
        private static string _domainName;
        private static CryptoHelper crypter;
        private static VulcainManifest _manifest;
        private static IStubManager _stubManager;
        static string defaultDomainName;

        internal static Settings Settings
        {
            get {
                if (Service._settings == null)
                {
                    Service._settings = new Settings();
                    Service.Log.Info(null, () => $"Running in { Service._settings.Stage} staging environment.");
                }
                return Service._settings;
            }
        }

        /**
         * Get the application manifest when the application runs in developement mode
         *
         * @readonly
         * @static
         *
         * @memberOf System
         */
        internal static VulcainManifest Manifest
        {
            get
            {
                if (Service._manifest == null)
                    Service._manifest = new VulcainManifest();
                return Service._manifest;
            }
        }

        /**
         * UTC date as string.
         *
         * @static
         * @returns
         *
         * @memberOf System
         */
        public static string nowAsString
        {
            get
            {
                return DateTime.UtcNow.ToString("o");
            }
        }

        /**
         * Acces to logger
         *
         * @static
         *
         * @memberOf System
         */
        public static ILogger Log
        {
            get
            {
                if (Service.logger == null)
                    Service.logger = new VulcainLogger();
                return Service.logger;
            }
        }

        /**
         * Default tenant
         *
         * @readonly
         * @static
         *
         * @memberOf System
         */
        public static string DefaultTenant { get {
                return Environment.GetEnvironmentVariable(Conventions.Instance.ENV_VULCAIN_TENANT) ?? "vulcain";
            }
        }

        internal static IStubManager GetStubManager(IServiceProvider container) {
            if (Service._stubManager == null) {
                if (Service.IsTestEnvironnment) {
                    var manager = Service._stubManager = container.GetService<IStubManager>();
                    manager.Initialize(Service.Settings.StubSessions, Service.Settings.SaveStubSessions);
                }
                else {
                    Service._stubManager = new DummyStubManager();
                }
            }
            return Service._stubManager; // TODO as service
        }

        /**
         * Check if the service is running in local mode (on developper desktop)
         * by checking if a '.vulcain' file exists in the working directory
         *
         * @readonly
         * @static
         *
         * @memberOf System
         */
        public static bool IsDevelopment
        {
            get
            {

                return Service.Settings.IsDevelopment;
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
        public static bool IsTestEnvironnment
        {
            get
            {
                return Service.Settings.IsTestEnvironnment;
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
        public static string ResolveAlias(string name, string version = null) {
            if (name == null)
                return null;

            // Try to find an alternate uri
            var alias = Service.Settings.GetAlias(name, version) as string;
            if (alias != null)
                return alias;

            var propertyName = "$alias." + name;
            if (version != null)
                propertyName = propertyName + "-" + version;

            var prop = DynamicConfiguration.GetProperty<string>(propertyName);
            if (prop?.Value != null) {
                var obj = JSON.Parse(prop.Value);
                var str = obj as string;
                if (str != null) return str;
                var dic = obj as Dictionary<string, object>;
                if (dic != null)
                {
                    name = dic["serviceName"] as string ?? name;
                    version = dic["version"] as string ?? version;
                    return Service.CreateContainerEndpoint(name, version);
                }
            }
            return null;
        }

        /**
         * Create container endpoint from service name and version
         *
         * @readonly
         * @static
         */
        public static string CreateContainerEndpoint(string serviceName, string version)
        {
            var s = (serviceName + version);
            s = s.Replace(".", "");
            s = s.Replace("-", "");
            return s.ToLower() + ":8080";
        }

        /**
         * Get current stage
         *
         * @readonly
         * @static
         *
         * @memberOf System
         */
        public static string StagingEnvironment
        {
            get
            {
                {
                    return Service.Settings.Stage;
                }
            }
        }

        /**
         * Get vulcain server used for getting configurations
         *
         * @readonly
         * @static
         *
         * @memberOf System
         */
        public static string VulcainServer { get
            {
                if (Service._vulcainServer == null)
                {
                    var env = DynamicConfiguration.GetPropertyValue<string>("vulcainServer");
                    Service._vulcainServer = env; // for dev
                }
                return Service._vulcainServer;
            }
        }

        /**
         * Get token for getting properties (must have configurations:read scope)
         *
         * @readonly
         * @static
         *
         * @memberOf System
         */
        public static string VulcainToken { get
            {
                if (Service._vulcainToken == null)
                {
                    Service._vulcainToken = DynamicConfiguration.GetPropertyValue<string>("vulcainToken");
                }
                return Service._vulcainToken;
            }
        }

        /**
         * Get service name
         *
         * @readonly
         * @static
         *
         * @memberOf System
         */
        public static string ServiceName
        {
            get
            {
                if (Service._serviceName == null)
                {
                    var env = Environment.GetEnvironmentVariable(Conventions.Instance.ENV_SERVICE_NAME);
                    if (env != null)
                        Service._serviceName = env;
                    else
                        return null;
                }
                return Service._serviceName;
            }
        }

        /**
         * Get service version
         *
         * @readonly
         * @static
         *
         * @memberOf System
         */
        public static string ServiceVersion
        {
            get
            {

                if (Service._serviceVersion == null)
                {
                    var env = Environment.GetEnvironmentVariable(Conventions.Instance.ENV_SERVICE_VERSION);
                    if (env != null)
                        Service._serviceVersion = env;
                    else
                        return null;
                }
                return Service._serviceVersion;
            }
        }

        public static string FullServiceName
        {
            get
            {

                return Service.ServiceName + "-" + Service.ServiceVersion;
            }
        }

        /**
         * Get current domain name
         *
         * @readonly
         * @static
         *
         * @memberOf System
         */
        public static string DomainName
        {
            get
            {

                if (Service._domainName == null)
                {
                    var env = Environment.GetEnvironmentVariable(Conventions.Instance.ENV_VULCAIN_DOMAIN);
                    if (env != null)
                        Service._domainName = env;
                    else
                        Service._domainName = Service.defaultDomainName;
                }
                return Service._domainName;
            }
        }

        private static CryptoHelper Crypto
        {
            get
            {

                if (Service.crypter == null)
                {
                    Service.crypter = new CryptoHelper();
                }
                return Service.crypter;
            }
        }

        /**
         * Encrypt a value
         *
         * @static
         * @param {any} value
         * @returns {string}
         *
         * @memberOf System
         */
        public static string Encrypt(string value) {
            return Service.Crypto.Encrypt(value);
        }

        /**
         * Decrypt a value
         *
         * @static
         * @param {string} value
         * @returns
         *
         * @memberOf System
         */
        public static string Decrypt(string value)
        {
            return Service.Crypto.Decrypt(value);
        }

        public static void RegisterPropertyAsDependency<T>(string name, T defaultValue=default(T))
        {
            var prefix = (Service.ServiceName + "." + Service.ServiceVersion);

            var p = Service.Manifest.Configurations[name];
            if (p != null && p != "any")
                return;
            var schema = "any";
           schema = typeof(T).Name;
            Service.Manifest.Configurations[name] = schema;
        }

        /**
         * create an url from segments
         * Segments of type string are concatened to provide the path
         * Segments of type object are appending in the query string
         * Null segments are ignored.
         * @protected
         * @param {string} base url
         * @param {(...Array<string|any>)} urlSegments
         * @returns an url
         */
        public static string CreateUrl(string baseurl, params object[] urlSegments)
        {

            var hasQueryPoint = baseurl.IndexOf("?") >= 0;

            if (urlSegments.Length > 0)
            {
                if (!hasQueryPoint && baseurl[baseurl.Length - 1] != '/')
                    baseurl += "/";

                var vpath = "";
                var paths = urlSegments.Where(s => s is String).Cast<string>().ToArray();

                if (hasQueryPoint && paths.Length >= 1)
                {
                    throw new ApplicationException("You can't have a path on your url after a query string");
                }
                else
                {
                    vpath = String.Concat(paths, "/");
                }


                var query = urlSegments.Where(s => s != null && !(s is string)).ToArray();
                if (query.Length > 0)
                {
                    var sep = hasQueryPoint ? "&" : "?";
                    foreach (var obj in query)
                    {
                        foreach (var (p,val) in JSObject.PropertiesOf(obj))
                        {
                            if (val != null && val is string)
                            {
                                vpath += String.Concat(sep, p, '=', val);
                                sep = "&";
                            }
                        }
                    }
                    baseurl += HttpUtility.UrlEncode(vpath);
                }

                return baseurl;
            }
            else
            {
                return baseurl;
            }
        }

        public static string RemovePasswordFromUrl(string url)
        {
            var regex = new Regex(@"/(\/[^:]*:[^@]*@)/g");
            var subst = "****@";

            // The substituted value will be contained in the result variable
            return regex.Replace(url, subst);
        }
    }
}
