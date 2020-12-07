using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
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
        private static string processFormat = "共 {0} 张，已下载 {1} 张";
        private string urlFormat;
        private static bool _initialized = false;
        private static log4net.ILog log;
        private int picTotal = 0;
        private int successCount = 0;
        private int faliCount = 0;

        private void Form1_Load(object sender, EventArgs e)
        {
            InitLog();
            // https://pixiv.cat/{0}.png 
            urlFormat = System.Configuration.ConfigurationManager.AppSettings["proxyUrlFormat"];
            textBox2.Text = System.Configuration.ConfigurationManager.AppSettings["defaultPath"];
            label3.Text = string.Empty;
            checkBox1.Checked = true;
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

            picTotal = pidArray.Length;
            successCount = 0;
            faliCount = 0;
            label3.Text = string.Format(processFormat, picTotal, successCount + faliCount);

            foreach (string pid in pidArray)
            {
                if (pid.StartsWith("http"))
                {
                    string fileName = Path.GetFileName(pid);
                    IceItem.FileOperation.FileUrlCopy(pid, textBox2.Text, fileName);
                    successCount++;
                }
                else
                {
                    //PidDownload(pid);

                    // 先请求一次确定图片数量，然后返回下载列表
                    PidDownloadTest(pid);
                }
            }
            MessageBox.Show($"成功 {successCount},失败 {faliCount}!");

        }

        #region "下载1"

        private void PidDownload(string pid)
        {
            string url = string.Format(this.urlFormat, pid);
            string filePath = Path.Combine(textBox2.Text, pid + ".png");
            if (IceItem.FileOperation.FileExist(filePath))
            {
                successCount++;
            }
            else
            {
                try
                {
                    IceItem.FileOperation.FileUrlCopy(url, textBox2.Text);
                    successCount++;
                }
                catch (Exception ex)
                {
                    bool mutiDownload = false;
                    if (checkBox1.Checked)
                    {
                        mutiDownload = MutiDownload(pid);
                    }

                    if (!mutiDownload)
                    {
                        log.Error($"图片下载错误！pid：{pid},{ex.Message}");
                        faliCount++;
                    }
                }
            }
            label3.Text = string.Format(processFormat, picTotal, successCount + faliCount);
        }

        private bool MutiDownload(string pid)
        {
            bool mutiDownload = false;
            string pidNo = string.Empty;

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = string.Format(this.urlFormat, pid);
                    var t = client.GetAsync(url).Result;
                    string result = t.Content.ReadAsStringAsync().Result;
                    HtmlDocument document = new HtmlDocument();
                    document.LoadHtml(result);
                    HtmlNode paragraph = document.DocumentNode.SelectSingleNode("//p");
                    string content = paragraph.InnerText;
                    string picCount = Regex.Match(content, @"\d+").Value;
                    // Displays a message box asking the user to choose Yes or No.
                    if (MessageBox.Show("是否下载 " + pid + " 里面的 " + picCount + " 张图片", "Continue", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        picTotal += int.Parse(picCount) - 1;
                        mutiDownload = true;
                        // Do something after the No button was clicked by user.
                        for (int i = 1; i < int.Parse(picCount) + 1; i++)
                        {
                            pidNo = pid + "-" + i;
                            url = string.Format(this.urlFormat, pidNo);
                            string filePath = Path.Combine(textBox2.Text, pidNo + ".png");
                            if (!IceItem.FileOperation.FileExist(filePath))
                            {
                                IceItem.FileOperation.FileUrlCopy(url, textBox2.Text);
                            }
                            successCount++;
                            label3.Text = string.Format(processFormat, picTotal, successCount + faliCount);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (mutiDownload)
                    {
                        log.Error($"图片下载错误！pid：{pidNo},{ex.Message}");
                        faliCount++;
                    }
                }
            }
            return mutiDownload;
        }

        #endregion

        #region "下载2"

        private byte[] fileBytes = null;
        private void PidDownloadTest(string singlePid)
        {
            List<string> pidList = GetPidList(singlePid);

            if (pidList.Count == 1)
            {
                IceItem.FileOperation.FileByteCopy(fileBytes, textBox2.Text, pidList[0] + "_p0.png");
                successCount++;
                label3.Text = string.Format(processFormat, picTotal, successCount + faliCount);
                fileBytes = null;
            }
            else 
            {
                for (int i = 0; i < pidList.Count; i++)
                {
                    try
                    {
                        string fileName = pidList[i].Replace($"-{(i + 1)}", $"_p{i}.png");
                        string url = string.Format(this.urlFormat, pidList[i]);
                        IceItem.FileOperation.FileUrlCopy(url, textBox2.Text, fileName);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        log.Error($"图片下载错误！pid：{pidList[i]},{ex.Message}");
                        faliCount++;
                    }
                    label3.Text = string.Format(processFormat, picTotal, successCount + faliCount);
                }
            }
            label3.Text = string.Format(processFormat, picTotal, successCount + faliCount);
        }

        private List<string> GetPidList(string singlePid)
        {
            List<string> pidList = new List<string>() { singlePid };

            using (HttpClient client = new HttpClient())
            {
                string url = string.Format(this.urlFormat, singlePid);
                try
                {
                    var t = client.GetAsync(url).Result;
                    if (t.StatusCode == HttpStatusCode.OK)
                    {
                        fileBytes = t.Content.ReadAsByteArrayAsync().Result;
                    }
                    else
                    {
                        pidList = new List<string>();
                        if (checkBox1.Checked)
                        {
                            string result = t.Content.ReadAsStringAsync().Result;
                            MessageBox.Show("请求地址：" + url + "\r\n 返回信息：" + result);
                            HtmlDocument document = new HtmlDocument();
                            document.LoadHtml(result);
                            HtmlNode paragraph = document.DocumentNode.SelectSingleNode("//p");
                            string content = paragraph.InnerText;
                            string picCount = Regex.Match(content, @"\d+").Value;

                            // Displays a message box asking the user to choose Yes or No.
                            if (MessageBox.Show("是否下载 " + singlePid + " 里面的 " + picCount + " 张图片", "Continue", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {
                                picTotal += int.Parse(picCount) - 1;
                                // Do something after the No button was clicked by user.
                                for (int i = 1; i < int.Parse(picCount) + 1; i++)
                                {
                                    pidList.Add(singlePid + "-" + i);
                                }
                            }
                            else
                            {
                                log.Error($"图片下载错误！pid：{singlePid},{result}");
                                faliCount++;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error($"图片下载错误！pid：{singlePid},{ex.Message}");
                    faliCount++;
                }
            }

            return pidList;
        }

        #endregion

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
