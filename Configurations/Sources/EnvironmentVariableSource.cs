using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Vulcain.Core.Configuration.Sources
{
    internal class EnvironmentVariableSource : IConfigurationSource
    {
        private Dictionary<string, PropertyValue> cache = new Dictionary<string, PropertyValue>();

        public PropertyValue Get(string name)
        {
            if (cache.TryGetValue(name, out PropertyValue pv))
                return pv;

            // As is
            var env = Environment.GetEnvironmentVariable(name);
            if (env == null)
            {
                // Replace dot
                env = Environment.GetEnvironmentVariable(name.Replace('.', '_'));
                if (env == null)
                {
                    // Replace dot with uppercases
                    env = Environment.GetEnvironmentVariable(name.ToUpper().Replace('.', '_'));
                    if (env == null)
                    {
                        // Transform camel case to upper case
                        // ex: myProperty --> MY_PROPERTY
                        var regex = new Regex(@"/([A-Z])|(\.)/g");
                        var subst = @"_\$1";
                        var res = regex.Replace(name, subst);
                        env = Environment.GetEnvironmentVariable(res.ToUpper());

                        // Otherwise as a docker secret
                        if (env == null)
                        {
                            try
                            {
                                // Using sync method here is assumed
                                env = File.ReadAllText("/run/secrets/" + name);
                            }
                            catch
                            {
                                // ignore error
                            }
                        }
                    }
                }
            }

            pv = env == null ? PropertyValue.Undefined : new PropertyValue(env);
            return pv;
        }
    }
}
