using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Vulcain.Core
{

    /**
     * Conventions values
     * You can override this values before instanciating application
     *
     * @export
     * @class Conventions
     */
    public class Conventions
    {

        private static Conventions _instance;

        public static string GetRandomId()
        {
            return Utils.getRandom64().toString("hex");
        }

        static object Clone(object source, dynamic target=null)
        {
            if (source == null )
            {
                return source;
            }

            target = target ?? new { };
            foreach(var (key,val) in JSObject.PropertiesOf(source))
            {
                var dic = val as Dictionary<string, object>;
                if (dic != null)
                {
                    JSObject.Set(target, key, new Dictionary<string, object>());
                    foreach (var kv in dic)
                    {
                        target[kv.Key] = kv.Value;
                    }
                }
                else
                {
                    target[key] = Conventions.Clone(val, JSObject.Get(target, key));
                }
            }
            return target;
        }

        public static Conventions Instance
        {
            get
            {
                if (Conventions._instance == null)
                {
                    Conventions._instance = new Conventions();
                    try
                    {
                        if (File.Exists("vulcain.conventions"))
                        {
                            var data = JSON.Parse<object>(File.ReadAllText("vulcain.conventions"));
                            Conventions.Clone(data, Conventions._instance);
                        }
                    }
                    catch (Exception e)
                    {
                        Service.Log.error(null, e, () => "Error when reading vulcain.conventions file. Custom conventions are ignored.");
                    }
                }
                return Conventions._instance;
            }
        }

    /**
     * Naming
     *
     */
    public string DefaultApplicationFolder = "api";
        public string DefaultHystrixPath = "/hystrix.stream";
        public string DefaultUrlprefix = "/api";
        public string VulcainFileName = "vulcain.json";

        public int DefaultStatsdDelayInMs = 10000;
        public string DefaultSecretKey = "Dn~BnCG7*fjEX@Rw5uN^hWR4*AkRVKMe"; // 32 length random string
        public string DefaultTokenExpiration = "20m"; // in moment format

        public string VULCAIN_SECRET_KEY = "vulcainSecretKey";
        public string TOKEN_ISSUER = "vulcainTokenIssuer";
        public string TOKEN_EXPIRATION = "vulcainTokenExpiration";
        public string ENV_VULCAIN_TENANT = "VULCAIN_TENANT";
        public string ENV_VULCAIN_DOMAIN = "VULCAIN_DOMAIN";
        public string ENV_SERVICE_NAME = "VULCAIN_SERVICE_NAME";
        public string ENV_SERVICE_VERSION = "VULCAIN_SERVICE_VERSION";
        public string ENV_VULCAIN_STAGE = "VULCAIN_STAGE"; // 'production', 'test' or 'local'

        public Dictionary<string, object> hystrix = new Dictionary<string, object> {
            { "hystrix.health.snapshot.validityInMilliseconds", 500 },
        {"hystrix.force.circuit.open", false },
        {"hystrix.force.circuit.closed",false },
        {"hystrix.circuit.enabled", true },
        {"hystrix.circuit.sleepWindowInMilliseconds", 5000 },
        {"hystrix.circuit.errorThresholdPercentage", 50 },
        {"hystrix.circuit.volumeThreshold", 10},
        {"hystrix.execution.timeoutInMilliseconds", 1500},
        {"hystrix.metrics.statistical.window.timeInMilliseconds", 10000},
        {"hystrix.metrics.statistical.window.bucketsNumber", 10},
        {"hystrix.metrics.percentile.window.timeInMilliseconds", 10000},
        {"hystrix.metrics.percentile.window.bucketsNumber", 10},
        {"hystrix.isolation.semaphore.maxConcurrentRequests", 10},
        {"hystrix.fallback.semaphore.maxConcurrentRequests", 10}
    };
    }

}
