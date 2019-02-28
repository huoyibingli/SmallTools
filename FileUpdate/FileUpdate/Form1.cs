using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
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
            txtBackupDir.Text = string.IsNullOrEmpty(filePath) ? @"G:\backup\huoyibingli\pictures" : backupPath;
            txtUpdateDir.Text = string.IsNullOrEmpty(filePath) ? @"G:\FileUpdate\huoyibingli\pictures" : updatePath;
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
            CompareDirectory(txtFileDir.Text);
            MessageBox.Show("更新完毕!共 " + fileCount + " 个文件");
            fileCount = 0;
        }

        public void CompareDirectory(string dic)
        {
            string newDic = dic.Replace(txtFileDir.Text, "");
            CompareFile(newDic.TrimStart('\\'));
            DirectoryInfo root = new DirectoryInfo(Path.Combine(dic));
            foreach (DirectoryInfo directory in root.GetDirectories())
            {
                CompareDirectory(directory.FullName);
            }
        }

        private void CompareFile(string dic)
        {
            DirectoryInfo root = new DirectoryInfo(Path.Combine(txtFileDir.Text, dic));
            foreach (FileInfo file in root.GetFiles())
            {
                string backupFilePath = Path.Combine(txtBackupDir.Text, dic, file.Name);
                if (File.Exists(backupFilePath) == false)
                {
                    CopyFile(dic, file.Name);
                }
            }
        }

        private void CopyFile(string dic, string fileName)
        {
            if (Directory.Exists(Path.Combine(txtBackupDir.Text, dic)) == false)
            {
                CreateDirectory(txtBackupDir.Text, dic);
            }

            if (Directory.Exists(Path.Combine(txtUpdateDir.Text, dic)) == false)
            {
                CreateDirectory(txtUpdateDir.Text, dic);
            }

            string filePath = Path.Combine(txtFileDir.Text, dic, fileName);
            string backupPath = Path.Combine(txtBackupDir.Text, dic, fileName);
            string updatePath = Path.Combine(txtUpdateDir.Text, dic, fileName);

            File.Copy(filePath, backupPath);
            File.Copy(filePath, updatePath);
            fileCount++;
            log.Info("Copy File : " + filePath);
        }

        /// <summary>
        /// 判断文件的目录是否存,不存则创建
        /// </summary>
        /// <param name="destFilePath">本地文件目录</param>
        private void CreateDirectory(string path, string dic)
        {
            string[] dirs = dic.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries); //解析出路径上所有的文件名
            string curDir = path;
            foreach (string dir in dirs)
            {
                curDir = Path.Combine(curDir, dir);
                if (Directory.Exists(curDir) == false)
                {
                    Directory.CreateDirectory(curDir);//创建新路径
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
