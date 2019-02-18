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

namespace TwitterPicRenameTool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.txtDicPath.Text = @"G:\Twitter";
        }

        private void btnSelectPath_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "请选择Txt所在文件夹";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    MessageBox.Show(this, "文件夹路径不能为空", "提示");
                    return;
                }
                this.txtDicPath.Text = dialog.SelectedPath;
            }
        }

        private void btnRename_Click(object sender, EventArgs e)
        {
            string folderName = Path.GetFileName(this.txtDicPath.Text);
            int count = 0;
            string folders = System.Configuration.ConfigurationManager.AppSettings["Folders"];
            if (folders.Contains(folderName.ToLower()))
            {
                DirectoryInfo root = new DirectoryInfo(this.txtDicPath.Text);
                foreach (FileInfo file in root.GetFiles())
                {
                    try
                    {
                        string name = file.Name;
                        string fullName = file.FullName;
                        if (name.EndsWith("_large"))
                        {
                            string newName = name.Substring(0, name.Length - 6);
                            string newFullName = Path.Combine(this.txtDicPath.Text, newName);
                            if (File.Exists(newFullName))
                            {
                                FileInfo newFile = new FileInfo(newFullName);
                                if (newFile.Length > file.Length)
                                {
                                    File.Delete(newFullName);
                                    file.MoveTo(newFullName);
                                }
                                else
                                {
                                    File.Delete(fullName);
                                }
                            }
                            else
                            {
                                file.MoveTo(newFullName);
                            }
                            count++;
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }

                MessageBox.Show("处理完成，共处理 " + count + "个文件");
            }
            else
            {
                MessageBox.Show("文件夹不正确！");
            }
        }
    }
}
