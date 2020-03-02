using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] pidArray = textBox1.Text.Split(new string[] { ",", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            string url = string.Empty;
            string filePath = string.Empty;
            int successCount = 0;
            int faliCount = 0;
            using (System.Net.WebClient webClient = new System.Net.WebClient())
            {
                foreach (string pid in pidArray)
                {
                    url = string.Format(this.urlFormat, pid);
                    filePath = Path.Combine(textBox2.Text, pid + ".png");
                    try
                    {
                        webClient.DownloadFile(url, filePath);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        log.Error($"图片下载错误！pid：{pid},{ex.Message}");
                        faliCount++;
                    }
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
