using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace ConvertConfigToShortUrlCore.Old
{
    public class DomainSetting
    {
        public UrlSetting Default;
        public string ManageKey;
        public string ReloadKey;
        public Dictionary<string, UrlSetting> Records;
        
        public static DomainSetting GetSetting(string fileName, out string domainName)
        {
            domainName = fileName.Substring(10, fileName.Length - 14);
            var fileContent = File.ReadAllText(fileName);
            var setting = JsonConvert.DeserializeObject<DomainSetting>(fileContent);
            return setting;
        }
    }

    public class UrlSetting
    {
        public string Url;
        public bool IsPermanent;
    }
}