using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileUpdate
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public static bool _initialized = false;
        public static log4net.ILog log;
        private int fileCount;

        private void Form1_Load(object sender, EventArgs e)
        {
            InitLog();
            fileCount = 0;
            string filePath = System.Configuration.ConfigurationManager.AppSettings["FilePath"];
            string backupPath = System.Configuration.ConfigurationManager.AppSettings["BackupPath"];
            string updatePath = System.Configuration.ConfigurationManager.AppSettings["UpdataPath"];
            txtFileDir.Text = string.IsNullOrEmpty(filePath) ? @"G:\huoyibingli\pictures" : filePath;
            txtBackupDir.Text = string.IsNullOrEmpty(backupPath) ? @"G:\backup\huoyibingli\pictures" : backupPath;
            txtUpdateDir.Text = updatePath;
        }

        #region "Select Dictionary"
        
        private void Button2_Click(object sender, EventArgs e)
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
                this.txtFileDir.Text = dialog.SelectedPath;
            }
        }

        private void Button3_Click(object sender, EventArgs e)
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
                this.txtBackupDir.Text = dialog.SelectedPath;
            }
        }

        private void Button4_Click(object sender, EventArgs e)
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
                this.txtUpdateDir.Text = dialog.SelectedPath;
            }
        }

        #endregion

        private void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                SyncFile(txtFileDir.Text);
                MessageBox.Show("更新完毕!共 " + fileCount + " 个文件");
                fileCount = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("同步出现错误：" + ex.Message + "\r\n已更新 " + fileCount + " 个文件");
            }

        }

        /// <summary>
        /// 同步文件
        /// </summary>
        /// <param name="fileSavePath">需要同步的初始文件夹</param>
        public void SyncFile(string fileSavePath)
        {
            string newDic = fileSavePath.Replace(txtFileDir.Text, ""); // 文件保存路径下的文件夹路径
            TransferFile(newDic.TrimStart('\\'));
            DirectoryInfo root = new DirectoryInfo(fileSavePath);
            foreach (DirectoryInfo directory in root.GetDirectories())
            {
                SyncFile(directory.FullName);
            }
        }

        /// <summary>
        /// 转移文件
        /// </summary>
        /// <param name="dic">以文件保存路径为起始的文件夹路径</param>
        private void TransferFile(string dic)
        {
            DirectoryInfo root = new DirectoryInfo(Path.Combine(txtFileDir.Text, dic));
            foreach (FileInfo file in root.GetFiles())
            {
                string backupFilePath = Path.Combine(txtBackupDir.Text, dic, file.Name);
                bool isVerifyConsistency = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["IsVerifyConsistency"]);
                bool isExist = isVerifyConsistency ? IceItem.FileOperation.FileExist(backupFilePath, file.FullName) : IceItem.FileOperation.FileExist(backupFilePath);

                if (isExist == false)
                {
                    IceItem.FileOperation.FileCopy(file.FullName, backupFilePath);
                    if (!string.IsNullOrEmpty(txtUpdateDir.Text))
                    {
                        string updateFilePath = Path.Combine(txtUpdateDir.Text, dic, file.Name);
                        IceItem.FileOperation.FileCopy(file.FullName, updateFilePath);
                    }
                    fileCount++;
                    log.Info("Copy File : " + file.FullName);
                }
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
