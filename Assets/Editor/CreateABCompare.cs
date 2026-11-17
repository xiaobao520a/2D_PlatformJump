using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 创建AB包资源对比文件 用来进行AB包的资源热更
/// </summary>
public class CreateABCompare
{
    /// <summary>
    /// 具体创建资源对比文件的静态方法
    /// </summary>
    /// <param name="platform">目标平台目录名 PC / Android / IOS</param>
    public static void CreateABCompareFile(string platform)
    {
        string abRoot = Application.dataPath + "/ArtRes/AB/" + platform;
        DirectoryInfo directoryInfo = Directory.CreateDirectory(abRoot);
        FileInfo[] infos = directoryInfo.GetFiles();
        StringBuilder stringBuilder = new StringBuilder();

        //拿到具体存的AB包资源文件
        foreach (FileInfo info in infos)
        {
            //没有后缀 .meta .manifest的文件才是需要的AB包文件
            if (info.Extension == "")
            {
                //具体存 AB包名字 大小(多少字节) MD5码|AB包名字 大小(多少字节) MD5码......
                //频繁拼接字符串用stringBuilder 减少垃圾 gc
                stringBuilder.Append(info.Name);
                stringBuilder.Append(" ");
                stringBuilder.Append(info.Length);
                stringBuilder.Append(" ");
                stringBuilder.Append(GetMD5(info.FullName)); //直接传文件的完整路径
                stringBuilder.Append("|");
            }
        }

        if (stringBuilder.Length == 0)
        {
            Debug.LogWarning("没有找到AB包: " + abRoot);
            return;
        }

        //转成字符串 并且删掉最后一个|
        string abCompareInfo = stringBuilder.ToString();
        abCompareInfo = abCompareInfo.Substring(0, abCompareInfo.Length - 1);

        Debug.Log(abCompareInfo);

        //创好之后 要把它写进对应存ab包的文件夹里
        File.WriteAllText(abRoot + "/ABCompareFile.txt", abCompareInfo);

        //写进去后自动刷新
        AssetDatabase.Refresh();
    }

    //得到文件的MD5码 128bit 16byte
    public static string GetMD5(string filePath)
    {
        //文件流 读取指定文件
        using (FileStream fs = new FileStream(filePath, FileMode.Open))
        {
            using (MD5 md5 = MD5.Create()) //跨平台的推荐写法
            {
                byte[] bytes = md5.ComputeHash(fs);
                fs.Close(); //读完文件流之后关掉

                StringBuilder stringBuilder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    stringBuilder.Append(b.ToString("x2")); //把每一个字节都转成两位十六进制 减少长度
                }

                return stringBuilder.ToString();
            }
        }
    }
}
