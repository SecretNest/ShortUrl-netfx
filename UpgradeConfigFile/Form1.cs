using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace UpgradeConfigFile
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text)) return;

            Setting1 setting1 = JsonConvert.DeserializeObject<Setting1>(textBox1.Text);

            Setting11 setting11 = new Setting11();
            setting11.Default = new UrlSetting() { Url = setting1.Default, IsPermanent = false };
            setting11.ManageKey = setting1.ManageKey;
            setting11.ReloadKey = setting1.ReloadKey;
            setting11.Records = new Dictionary<string, UrlSetting>();
            foreach(var item in setting1.Records)
            {
                setting11.Records.Add(item.Key, new UrlSetting() { Url = item.Value, IsPermanent = false });
            }

            textBox2.Text = JsonConvert.SerializeObject(setting11);
        }
    }
}
