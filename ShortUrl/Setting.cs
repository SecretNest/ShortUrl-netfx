using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SecretNest.ShortUrl
{
    public class Setting
    {
        public string Default;
        public string ManageKey;
        public string ReloadKey;
        public Dictionary<string, string> Records;
    }
}