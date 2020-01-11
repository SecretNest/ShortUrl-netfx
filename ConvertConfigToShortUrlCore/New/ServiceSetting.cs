using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ConvertConfigToShortUrlCore.New
{
    public class ServiceSetting
    {
        public bool EnableStaticFiles { get; set; }
        public string UserSpecifiedStaticFileFolder { get; set; }
        public bool PreferXForwardedHost { get; set; }
        public RedirectTarget DefaultTarget { get; set; }

        public string GlobalManagementKey { get; set; }
        public const string DefaultGlobalManagementKey = "$$$$GlobalManagement$$$$";
        public HashSet<string> GlobalManagementEnabledHosts { get; set; }
        public Dictionary<string, DomainSetting> Domains { get; set; }
        public Dictionary<string, string> Aliases { get; set; }

        const string fileName = "SecretNest.ShortUrl.Setting.json";

        public static ServiceSetting Load()
        {
            if (File.Exists(fileName))
            {
                var fileData = File.ReadAllText(fileName);
                ServiceSetting serviceSetting = JsonConvert.DeserializeObject<ServiceSetting>(fileData);
                serviceSetting.FixAfterDeserializing();
                return serviceSetting;
            }
            else
            {
                return CreateDefault();
            }
        }

        static DefaultContractResolver contractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        };

        static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = contractResolver,
            Formatting = Formatting.Indented
        };

        public void SaveSetting()
        {
            var fileData = JsonConvert.SerializeObject(this, jsonSerializerSettings);
            if (File.Exists(fileName))
            {
                File.Move(fileName, fileName + ".bak", true);
            }
            File.WriteAllText(fileName, fileData);
        }

        static ServiceSetting CreateDefault()
        {
            ServiceSetting item = new ServiceSetting
            {
                //KestrelUrl = "http://localhost:40020",
                EnableStaticFiles = true,
                PreferXForwardedHost = true,
                DefaultTarget = RedirectTarget.Create(DefaultGlobalManagementKey, false),
                GlobalManagementKey = DefaultGlobalManagementKey,
                GlobalManagementEnabledHosts = new HashSet<string>(StringComparer.OrdinalIgnoreCase),
                Domains = new Dictionary<string, DomainSetting>(StringComparer.OrdinalIgnoreCase),
                Aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            };
            return item;
        }

        void FixAfterDeserializing()
        {
            GlobalManagementEnabledHosts = new HashSet<string>(GlobalManagementEnabledHosts, StringComparer.OrdinalIgnoreCase); 

            Domains = new Dictionary<string, DomainSetting>(Domains, StringComparer.OrdinalIgnoreCase);
            foreach (var domain in Domains.Values)
            {
                if (domain.IgnoreCaseWhenMatching)
                {
                    domain.Redirects = new Dictionary<string, RedirectTarget>(domain.Redirects, StringComparer.OrdinalIgnoreCase);
                }
            }

            Aliases = new Dictionary<string, string>(Aliases, StringComparer.OrdinalIgnoreCase);
        }
    }

    public class DomainSetting
    {
        public string ManagementKey { get; set; }
        public RedirectTarget DefaultTarget { get; set; }
        public bool IgnoreCaseWhenMatching { get; set; }
        public Dictionary<string, RedirectTarget> Redirects { get; set; }

        public static DomainSetting CreateEmpty(string managementKey, string defaultTarget, bool permanent)
        {
            DomainSetting item = new DomainSetting
            {
                ManagementKey = managementKey,
                DefaultTarget = RedirectTarget.Create(defaultTarget, permanent),
                IgnoreCaseWhenMatching = false,
                Redirects = new Dictionary<string, RedirectTarget>()
            };
            return item;
        }
    }

    public class RedirectTarget
    {
        public string Target { get; set; }
        public bool Permanent { get; set; }

        [DefaultValue(RedirectQueryProcess.Ignored)]
        public RedirectQueryProcess QueryProcess { get; set; }

        public static RedirectTarget Create(string target, bool permanent)
        {
            RedirectTarget item = new RedirectTarget();
            item.Target = target;
            item.Permanent = permanent;
            item.QueryProcess = RedirectQueryProcess.Ignored;
            return item;
        }
    }

    [DefaultValue(Ignored)]
    public enum RedirectQueryProcess : int
    {
        Ignored = 0,
        AppendDirectly = 1,
        AppendRemovingLeadingQuestionMark = 2
    }
}
