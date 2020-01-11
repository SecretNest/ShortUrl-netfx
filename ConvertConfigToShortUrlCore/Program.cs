using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConvertConfigToShortUrlCore
{
    class Program
    {
        static void Main(string[] args)
        {
            var filesSetting = Directory.GetFiles(".", "Setting.*.txt");
            var filesRedirect = Directory.GetFiles(".", "Redirect.*.txt");

            var newSetting = New.ServiceSetting.Load();

            foreach (var fileSetting in filesSetting)
            {
                var oldDomainSetting = Old.DomainSetting.GetSetting(fileSetting, out string domainName);

                Console.WriteLine("Processing domain " + domainName);

                New.DomainSetting newDomainSetting;
                if (newSetting.Domains.TryGetValue(domainName, out newDomainSetting))
                {
                    if (newDomainSetting.IgnoreCaseWhenMatching)
                    {
                        newDomainSetting.IgnoreCaseWhenMatching = false;
                        newDomainSetting.Redirects = new System.Collections.Generic.Dictionary<string, New.RedirectTarget>(newDomainSetting.Redirects);
                    }
                }
                else
                {
                    newDomainSetting = New.DomainSetting.CreateEmpty(oldDomainSetting.ManageKey, oldDomainSetting.Default.Url, oldDomainSetting.Default.IsPermanent);
                    newSetting.Domains.Add(domainName, newDomainSetting);
                }

                foreach (var redirect in oldDomainSetting.Records)
                {
                    var matched = newDomainSetting.Redirects
                        .FirstOrDefault(i => i.Value.Target == redirect.Value.Url && i.Value.Permanent == redirect.Value.IsPermanent);
                    if (matched.Equals(default(KeyValuePair<string, New.RedirectTarget>)))
                    {
                        newDomainSetting.Redirects[redirect.Key] = New.RedirectTarget.Create(redirect.Value.Url, redirect.Value.IsPermanent);
                    }
                    else
                    {
                        newDomainSetting.Redirects[redirect.Key] = New.RedirectTarget.Create(">" + matched.Key, false);
                    }
                }
            }

            foreach (var fileRedirect in filesRedirect)
            {
                var target = Old.RedirectSetting.GetSetting(fileRedirect, out string alias);

                Console.WriteLine("Process redirect " + alias);

                newSetting.Aliases[alias] = target;
            }

            newSetting.SaveSetting();
        }
    }
}
