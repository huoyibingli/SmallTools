using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace Pixiv_Download
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private string urlFormat;
        public static bool _initialized = false;
        public static log4net.ILog log;

        private void Form1_Load(object sender, EventArgs e)
        {
            InitLog();
            // https://pixiv.cat/{0}.png 
            urlFormat = System.Configuration.ConfigurationManager.AppSettings["proxyUrlFormat"];
            textBox2.Text = System.Configuration.ConfigurationManager.AppSettings["defaultPath"];
            label3.Text = string.Empty;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text) || string.IsNullOrEmpty(textBox2.Text)) 
            {
                MessageBox.Show("路径和id不能为空！");
                return;
            }

            if (IceItem.FileOperation.FileExist(textBox2.Text)) 
            {
                IceItem.FileOperation.CreateDirectory(textBox2.Text);
            }

            string[] pidArray = textBox1.Text.Split(new string[] { ",", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            string url = string.Empty;
            string filePath = string.Empty;
            int picTotal = pidArray.Length;
            int successCount = 0;
            int faliCount = 0;
            string processFormat = "共 {0} 张，已下载 {1} 张";
            label3.Text = string.Format(processFormat, picTotal, successCount + faliCount);

            using (HttpClient client = new HttpClient())
            {
                foreach (string pid in pidArray)
                {
                    url = string.Format(this.urlFormat, pid);
                    filePath = Path.Combine(textBox2.Text, pid + ".png");
                    if (IceItem.FileOperation.FileExist(filePath)) continue;

                    try
                    {
                        IceItem.FileOperation.FileUrlCopy(url, textBox2.Text);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        if (checkBox1.Checked) 
                        {
                            try
                            {
                                var t = client.GetAsync(url).Result;
                                string result = t.Content.ReadAsStringAsync().Result;
                                HtmlDocument document = new HtmlDocument();
                                document.LoadHtml(result);
                                HtmlNode paragraph = document.DocumentNode.SelectSingleNode("//p");
                                string content = paragraph.InnerText;
                                string picCount = Regex.Match(content, @"\d+").Value;
                                picTotal += int.Parse(picCount);
                                // Displays a message box asking the user to choose Yes or No.
                                if (MessageBox.Show("是否下载 " + pid + " 里面的 " + picCount + " 张图片", "Continue", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                {
                                    // Do something after the No button was clicked by user.
                                    for (int i = 1; i < int.Parse(picCount) + 1; i++)
                                    {
                                        url = string.Format(this.urlFormat, pid + "-" + i);
                                        filePath = Path.Combine(textBox2.Text, pid + "-" + i + ".png");
                                        if (IceItem.FileOperation.FileExist(filePath)) continue;
                                        IceItem.FileOperation.FileUrlCopy(url, textBox2.Text);
                                        successCount++;
                                    }
                                }
                                else
                                {
                                    // Do something after the Yes button was clicked by user.
                                }
                            }
                            catch (Exception)
                            { }
                        }

                        log.Error($"图片下载错误！pid：{pid},{ex.Message}");
                        faliCount++;
                    }
                    label3.Text = string.Format(processFormat, picTotal, successCount + faliCount);
                }
            }
            MessageBox.Show($"成功 {successCount},失败 {faliCount}!");
            
        }


        private void button2_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "请选择所在文件夹";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    MessageBox.Show(this, "文件夹路径不能为空", "提示");
                    return;
                }
                this.textBox2.Text = dialog.SelectedPath;
            }
        }

        /// <summary>
        /// 初始化日志
        /// </summary>
        private static void InitLog()
        {
            //初始化日志文件 
            if (!_initialized)
            {
                var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
                var fi = new System.IO.FileInfo(path);
                log4net.Config.XmlConfigurator.Configure(fi);

                log4net.Appender.RollingFileAppender appender = new log4net.Appender.RollingFileAppender();
                appender.Encoding = System.Text.Encoding.UTF8;
                appender.File = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log", DateTime.Now.ToString("yyyyMMddHHmmss") + ".log");
                appender.AppendToFile = true;
                appender.MaxSizeRollBackups = 3;
                appender.MaximumFileSize = "1MB";
                appender.RollingStyle = log4net.Appender.RollingFileAppender.RollingMode.Size;
                appender.StaticLogFileName = true;
                appender.Layout = new log4net.Layout.PatternLayout("%date [%thread] %-5level - %message%newline");
                appender.ActivateOptions();
                log4net.Config.BasicConfigurator.Configure(appender);
                log = log4net.LogManager.GetLogger("RollingLogFile");
                _initialized = true;
            }

        }

    }
}
