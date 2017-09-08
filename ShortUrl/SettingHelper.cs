using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace SecretNest.ShortUrl
{
    public class SettingHelper
    {
        static ConcurrentDictionary<string, Setting> Settings = new ConcurrentDictionary<string, Setting>();
        public static string DefaultSite = Properties.Settings.Default.DefaultSite;
        static string SettingNameFormat = Properties.Settings.Default.SettingNameFormat;

        static string GetFileName(string hostName)
        {
            return HttpContext.Current.Server.MapPath(String.Format(SettingNameFormat, hostName));
        }

        public static Setting GetSetting(string hostName)
        {
            try
            {
                return Settings.GetOrAdd(hostName, host =>
                {
                    string fileName = GetFileName(host);
                    var fileContent = File.ReadAllText(fileName);
                    var setting = JsonConvert.DeserializeObject<Setting>(fileContent);
                    return setting;
                });
            }
            catch
            {
                return null;
            }
        }

        public static void Reload(string hostName)
        {
            string fileName = GetFileName(hostName);
            var fileContent = File.ReadAllText(fileName);
            var setting = JsonConvert.DeserializeObject<Setting>(fileContent);
            Settings[hostName] = setting;
        }

        public static void Save(string hostName, Setting setting)
        {
            Settings[hostName] = setting;

            string fileName = GetFileName(hostName);
            var fileContent = JsonConvert.SerializeObject(setting);
            File.WriteAllText(fileName, fileContent);
        }
    }
}