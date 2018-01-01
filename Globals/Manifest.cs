using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Vulcain.Core
{

    internal class ServiceDependencyInfo
    {
        public string Service;
        public string Version;
        public string DiscoveryAddress;
    }

    internal class ConfigurationInfo
    {
        public string Key;
        public string Schema;
    }

    internal class DatabaseDependencyInfo
    {
        public string Address;
        public string Schema;
    }

    internal class ExternalDependencyInfo
    {
        public string Uri;
    }

    internal class DependenciesInfo
    {
        public List<ServiceDependencyInfo> Services;
        public List<ExternalDependencyInfo> Externals;
        public List<DatabaseDependencyInfo> Databases;
        public Dictionary<string, string> Packages;
    }
    /**
     * Contains all service dependencies
     *
     * @export
     * @class VulcainManifest
     */
    internal class VulcainManifest
    {
        public DependenciesInfo Dependencies;

        public Dictionary<string, string> Configurations;
        public string Domain;

        public VulcainManifest() {
            this.Domain = Service.DomainName;
            this.Dependencies = new DependenciesInfo() {
                Services = new List<ServiceDependencyInfo>(),
                Externals = new List<ExternalDependencyInfo>(),
                Databases = new List<DatabaseDependencyInfo>(),
                Packages = this.RetrievePackage()
            };
            this.Configurations = new Dictionary<string, string>();
        }

        public void RegisterExternal(string uri)
        {
            var exists = this.Dependencies.Externals.Any(x => x.Uri == uri);
            if (!exists)
            {
                this.Dependencies.Externals.Add(new ExternalDependencyInfo() { Uri = uri });
            }
        }

        public void RegisterProvider(string address, string schema)
        {
            var exists = this.Dependencies.Databases.Any(db => db.Address == address && db.Schema == schema);
            if (!exists)
            {
                this.Dependencies.Databases.Add(new DatabaseDependencyInfo() { Address = address, Schema = schema });
            }
        }

        public void RegisterService(string targetServiceName, string targetServiceVersion)
        {
            if (targetServiceName == null)
                throw new ArgumentNullException("You must provide a service name");

            if (targetServiceVersion == null || !targetServiceVersion.Match(@"/[0-9]+\.[0-9]+/"))
                throw new ArgumentException("Invalid version number. Must be on the form major.minor");

            var exists = this.Dependencies.Services.Any(svc => svc.Service == targetServiceName && svc.Version == targetServiceVersion);
            if (!exists)
            {
                this.Dependencies.Services.Add(new ServiceDependencyInfo() { Service = targetServiceName, Version = targetServiceVersion });
            }
        }

        private Dictionary<string, string> RetrievePackage()
        {
            var packages = new Dictionary<string, string>();
            try
            {
                // TODO
                //var packageFilePath = Path.Combine(process.cwd(), "package.json");
                //var json = fs.readFileSync(packageFilePath, "utf8");
                //var pkg = JSON.Parse(json);
                //var dependencies = pkg.dependencies;

                //var nodeModulesPath = Path.Combine(Path.GetDirectoryName(packageFilePath), "node_modules");
                //foreach (var packageName in JSObject.KeysOf(dependencies))
                //{
                //    try
                //    {
                //        json = fs.readFileSync(Path.Combine(nodeModulesPath, packageName, "package.json"), "utf8");
                //        pkg = JSON.Parse(json);
                //    }
                //    catch {/*ignore*/}
                //    yield return { packageName name, version: (pkg && pkg.version) || "???" };
                //}
            }
            catch
            {
                //console.info("Can not read packages version. Skip it", e.message);
            }
            return packages;
        }
    }

    /**
     * Declare a vulcain service dependencie for the current service
     *
     * @export
     * @param {string} service Name of the called service
     * @param {string} version Version of the called service
     * @param {string} discoveryAddress Discovery address of the called service (ex:http://..:30000/api/_servicedesctipyion)
     * @returns
     */
    public class ServiceDependencyAttribute : VulcainAttribute {
        string service;
        string version;
        string discoveryAddress;
        public ServiceDependencyAttribute(string service, string version, string discoveryAddress)
        {
            this.service = service;
            this.version = version;
            this.discoveryAddress = discoveryAddress;
        }

        internal override void Apply()
        {
            //target["$dependency:service"] = { service targetServiceName, version targetServiceVersion };

            var exists = Service.Manifest.Dependencies.Services.Any(svc => svc.Service == service && svc.Version == version);
            if (!exists)
            {
                Service.Manifest.Dependencies.Services.Add(new ServiceDependencyInfo() { Service = service, Version = version, DiscoveryAddress = discoveryAddress });
            }
        }
    }

    /**
     * Declare an external http call dependencie for the current service
     *
     * @export
     * @param {string} uri External uri
     * @returns
     */
    public class HttpDependencyAttribute : VulcainAttribute
    {
        string uri;

        public HttpDependencyAttribute(string uri)
        {
            this.uri = uri;
        }

        internal override void Apply()
        {
            //        target["$dependency:external"] = { uri };
            var exists = Service.Manifest.Dependencies.Externals.Any(ex => ex.Uri == uri);
            if (!exists)
            {
                Service.Manifest.Dependencies.Externals.Add(new ExternalDependencyInfo() { Uri = uri });
            }
        }
    }

    /**
     * Declare a dynamic property configuration for the current service.
     *
     * @export
     * @param {string} propertyName Property name
     * @param {string} schema Property schema (can be a model or a native js type)
     * @returns
     */
    public class ConfigurationPropertyAttribute : VulcainAttribute
    {
        string propertyName;
        string schema;

        public ConfigurationPropertyAttribute(string propertyName, string schema)
        {
            this.propertyName = propertyName;
            this.schema = schema;
        }

        internal override void Apply()
        {
            if (propertyName == null)
                throw new ArgumentNullException("Invalid property propertyName");

            if (schema == null)
                throw new ArgumentNullException("Invalid property schema");

            schema = schema.ToLower();
            var existingSchema = Service.Manifest.Configurations[propertyName];
            if (existingSchema != null && existingSchema != "any")
            {
                if (existingSchema != schema)
                    throw new ApplicationException($"Inconsistant schema({ schema } <> { existingSchema}) for configuration property { propertyName}");
                return;
            }
            Service.Manifest.Configurations[propertyName] = schema;
        }
    }
}
