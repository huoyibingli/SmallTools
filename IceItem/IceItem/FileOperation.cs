﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IceItem
{
    public class FileOperation
    {
        /// <summary>
        /// 判断文件的目录是否存,不存则创建
        /// </summary>
        /// <param name="targetPath">目录</param>
        public static void CreateDirectory(string targetPath)
        {
            string[] dirs = targetPath.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries); //解析出路径上所有的文件名
            string curDir = dirs[0];
            for (int i = 1; i < dirs.Length; i++)
            {
                curDir += "\\" + dirs[i];
                if (Directory.Exists(curDir) == false)
                {
                    Directory.CreateDirectory(curDir);//创建新路径
                }
            }
        }

        /// <summary>
        /// 文件是否存在
        /// </summary>
        /// <param name="sourcePath">源文件路径,文件存在</param>
        /// <param name="targetPath">目标文件路径</param>
        /// <param name="delete">目标文件与源文件不相同时，是否删除</param>
        public static bool FileExist(string targetPath, string sourcePath = null, bool delete = true)
        {
            bool isExist = false;
            if (sourcePath == null)
            {
                isExist = File.Exists(targetPath);
            }
            else
            {
                if (File.Exists(targetPath))
                {
                    //创建一个哈希算法对象 
                    using (HashAlgorithm hash = HashAlgorithm.Create())
                    {
                        using (FileStream sourceFile = new FileStream(sourcePath, FileMode.Open), targetFile = new FileStream(targetPath, FileMode.Open))
                        {
                            byte[] sourcehashByte = hash.ComputeHash(sourceFile);//哈希算法根据文本得到哈希码的字节数组 
                            byte[] targethashByte = hash.ComputeHash(targetFile);
                            string str1 = BitConverter.ToString(sourcehashByte);//将字节数组装换为字符串 
                            string str2 = BitConverter.ToString(targethashByte);
                            isExist = str1 == str2;//比较哈希码 
                        }
                    }
                    if (!isExist && delete) File.Delete(targetPath);
                }
            }

            return isExist;
        }

        /// <summary>
        /// 复制文件
        /// </summary>
        /// <param name="sourcePath">源文件路径</param>
        /// <param name="targetPath">目标文件路径</param>
        public static void FileCopy(string sourcePath, string targetPath)
        {
            if (!FileExist(targetPath, sourcePath))
            {
                string targetDic = Path.GetDirectoryName(targetPath);
                if (Directory.Exists(targetDic) == false)
                {
                    CreateDirectory(targetDic);
                }
                File.Copy(sourcePath, targetPath);
            }
        }


        /// <summary>
        /// 复制文件
        /// </summary>
        /// <param name="url">源文件路径</param>
        /// <param name="targetDic">目标文件路径</param>
        public static void FileUrlCopy(string url, string targetDic, string fileName = null)
        {
            fileName = fileName ?? Path.GetFileName(url);
            string fileTempPath = Path.Combine(targetDic, fileName);
            if (FileExist(fileTempPath)) return;

            using (HttpClient client = new HttpClient())
            {
                var t = client.GetByteArrayAsync(WebUtility.UrlDecode(url)).Result;
                FileByteCopy(t, targetDic, fileName);
            }
        }

        /// <summary>
        /// 复制文件
        /// </summary>
        /// <param name="bytes">文件字节</param>
        /// <param name="targetDic">目标路径</param>
        /// <param name="fileName">文件名</param>
        public static void FileByteCopy(byte[] bytes, string targetDic, string fileName)
        {
            if (Directory.Exists(targetDic) == false)
            {
                CreateDirectory(targetDic);
            }
            string fileTempPath = Path.Combine(targetDic, fileName);
            if (FileExist(fileTempPath)) return;

            Stream responseStream = new MemoryStream(bytes);

            long allLenght = responseStream.Length;
            int package = 10248;//特殊的包大小

            byte[] by = new byte[package];
            FileStream writeFile = new FileStream(fileTempPath, FileMode.Append);

            while (true)
            {
                int l = responseStream.Read(by, 0, (int)by.Length);
                if (l == 0) break;
                writeFile.Write(by, 0, l);
            }

            //全部上传完成
            if (allLenght == responseStream.Position)
            {
                writeFile.Flush();
                writeFile.Close();
            }
        }
    }
}
