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
        static ConcurrentDictionary<string, string> Redirected = new ConcurrentDictionary<string, string>();
        static ConcurrentDictionary<string, Setting> Settings = new ConcurrentDictionary<string, Setting>();
        public static string DefaultSite = Properties.Settings.Default.DefaultSite;
        static string RedirectNameFormat = Properties.Settings.Default.RedirectNameFormat;
        static string SettingNameFormat = Properties.Settings.Default.SettingNameFormat;

        static string GetRedirectFileName(string hostName)
        {
            return HttpContext.Current.Server.MapPath(string.Format(RedirectNameFormat, hostName));
        }

        static string GetSettingFileName(string hostName)
        {
            return HttpContext.Current.Server.MapPath(string.Format(SettingNameFormat, hostName));
        }

        static string GetRealHostName(string hostName)
        {
            return Redirected.GetOrAdd(hostName, host =>
            {
                string fileName = GetRedirectFileName(host);
                if (File.Exists(fileName))
                {
                    var fileContent = File.ReadAllText(fileName);
                    return fileContent;
                }
                else
                {
                    return host;
                }
            });
        }

        public static Setting GetSetting(string hostName)
        {
            hostName = GetRealHostName(hostName);

            return Settings.GetOrAdd(hostName, host =>
            {
                string fileName = GetSettingFileName(host);
                if (File.Exists(fileName))
                {
                    var fileContent = File.ReadAllText(fileName);
                    var setting = JsonConvert.DeserializeObject<Setting>(fileContent);
                    return setting;
                }
                else
                {
                    return null;
                }
            });
        }

        public static void Reload(string hostName)
        {
            string fileName = GetRedirectFileName(hostName);
            string fileContent;
            if (File.Exists(fileName))
            {
                fileContent = File.ReadAllText(fileName);
                Redirected[hostName] = fileContent;
                hostName = fileContent;
            }
            else
            {
                Redirected[hostName] = hostName;
            }
            fileName = GetSettingFileName(hostName);
            fileContent = File.ReadAllText(fileName);
            var setting = JsonConvert.DeserializeObject<Setting>(fileContent);
            Settings[hostName] = setting;
        }

        public static void Save(string hostName, Setting setting)
        {
            hostName = GetRealHostName(hostName);

            Settings[hostName] = setting;

            string fileName = GetSettingFileName(hostName);
            var fileContent = JsonConvert.SerializeObject(setting);
            File.WriteAllText(fileName, fileContent);
        }
    }
}