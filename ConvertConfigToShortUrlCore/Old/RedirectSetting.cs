using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ConvertConfigToShortUrlCore.Old
{
    class RedirectSetting
    {
        public static string GetSetting(string fileName, out string alias)
        {
            alias = fileName.Substring(11, fileName.Length - 15);
            var fileContent = File.ReadAllText(fileName);
            return fileContent;
        }
    }
}
