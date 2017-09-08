using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SecretNest.ShortUrl
{
    /// <summary>
    /// Summary description for Short
    /// </summary>
    public class Short : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            var host = context.Request.Url.Host;
            var setting = SettingHelper.GetSetting(host);
            if (setting == null)
            {
                context.Response.Redirect(SettingHelper.DefaultSite);
                return;
            }

            var code = context.Request["code"];

            if (string.IsNullOrWhiteSpace(code))
            {
                context.Response.Redirect(setting.Default);
            }
            else if (code == setting.ReloadKey)
            {
                SettingHelper.Reload(host);
                context.Response.Write("OK");
            }
            else if (code == setting.ManageKey)
            {
                ModifySetting(context, host, setting);
            }
            else if (setting.Records.TryGetValue(code, out var url))
            {
                context.Response.Redirect(url);
            }
            else
            {
                context.Response.Redirect(setting.Default);
            }
        }

        void ModifySetting(HttpContext context, string host, Setting currentSetting)
        {
            var settingDefault = context.Request["default"];
            var settingReload = context.Request["reload"];
            var settingManage = context.Request["manage"];
            var settingKey = context.Request["key"];
            var settingValue = context.Request["value"];
            var settingOperate = context.Request["operate"];
            Setting setting;

            if (settingOperate == "main")
            {
                setting = new Setting();
                if (!string.IsNullOrWhiteSpace(settingDefault))
                    setting.Default = settingDefault;
                if (!string.IsNullOrWhiteSpace(settingReload))
                    setting.ReloadKey = settingReload;
                if (!string.IsNullOrWhiteSpace(settingManage))
                    setting.ManageKey = settingManage;

                setting.Records = currentSetting.Records;

                SettingHelper.Save(host, setting);

                if (currentSetting.ManageKey != setting.ManageKey)
                {
                    context.Response.Redirect("/" + setting.ManageKey);
                    return;
                }
            }
            else if (settingOperate == "record")
            {
                setting = new Setting();
                setting.Default = currentSetting.Default;
                setting.ManageKey = currentSetting.ManageKey;
                setting.ReloadKey = currentSetting.ReloadKey;
                setting.Records = currentSetting.Records;

                var deletes = context.Request.Params.GetValues("delete");
                if (deletes != null)
                {
                    foreach(var key in deletes)
                    {
                        setting.Records.Remove(key);
                    }
                }

                if (!string.IsNullOrWhiteSpace(settingKey))
                {
                    if (string.IsNullOrWhiteSpace(settingValue))
                    {
                        setting.Records.Remove(settingKey);
                    }
                    else
                    {
                        setting.Records[settingKey] = settingValue;
                    }
                }

                SettingHelper.Save(host, setting);
            }
            else 
            {
                setting = currentSetting;
            }

            context.Response.Write("<html><head><title>");
            context.Response.Write(host);
            context.Response.Write("</title></head><body><form action=\"\" method=\"post\">These values cannot be null, empty or spaces only.<br /><table border=\"1\" style=\"width:100%\"><tr><th><p style=\"text - align:left; \">Setting</p></th><th><p style=\"text - align:left; \">Value</p></th></tr><tr><td>Default</td><td><input type=\"text\" name=\"default\" value=\"");
            context.Response.Write(setting.Default);
            context.Response.Write("\" style=\"width:100%\" /></td></tr><tr><td>Reload</td><td><input type=\"text\" name=\"reload\" value=\"");
            context.Response.Write(setting.ReloadKey);
            context.Response.Write("\" style=\"width:100%\" /></td></tr><tr><td>Manage</td><td><input type=\"text\" name=\"manage\" value=\"");
            context.Response.Write(setting.ManageKey);
            context.Response.Write("\" style=\"width:100%\" /></td></tr></table><input type=\"hidden\" name=\"operate\" value=\"main\" /><input type=\"hidden\" name=\"code\" value=\"");
            context.Response.Write(setting.ManageKey);
            context.Response.Write("\" /><input type=\"submit\" value=\"Save\" /></form><br /><form action=\"\" method=\"post\"><input type=\"hidden\" name=\"operate\" value=\"record\" /><input type=\"hidden\" name=\"code\" value=\"");
            context.Response.Write(setting.ManageKey);
            context.Response.Write("\" /><br />Records:<br /><table border=\"1\" style=\"width:100%\"><tr><th><p style=\"text - align:left; \">Operate</p></th><th><p style=\"text - align:left; \">Key</p></th><th><p style=\"text - align:left; \">Value</p></th></tr><tr><td><p style=\"text - align:left; \">Add / Change</p></td><td><input type=\"text\" name=\"key\" style=\"width:100%\" /></td><td><input type=\"text\" name=\"value\" style=\"width:100%\" /></td></tr>");
            var records = setting.Records.OrderBy(i => i.Key).ToArray();
            if (records.Any())
            {
                var deletePrefix = "<input type=\"checkbox\" name=\"delete\" value=\"";
                var deletePostfix = "\" />Delete";
                foreach (var record in records)
                {
                    context.Response.Write("<tr><td>");
                    context.Response.Write(deletePrefix);
                    context.Response.Write(HttpUtility.HtmlEncode(record.Key));
                    context.Response.Write(deletePostfix);
                    context.Response.Write("</td><td>");
                    context.Response.Write(HttpUtility.HtmlEncode(record.Key));
                    context.Response.Write("</td><td>");
                    context.Response.Write(HttpUtility.HtmlEncode(record.Value));
                    context.Response.Write("</td></tr>");
                }
            }
            context.Response.Write("</table><input type=\"submit\" value=\"Save\" /></form></body></html>");
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}