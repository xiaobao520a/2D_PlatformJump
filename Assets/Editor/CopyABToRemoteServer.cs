using System.IO;
using UnityEditor;
using UnityEngine;

public static class CopyABToRemoteServer
{
    public static void Copy(string platform)
    {
        string sourcePath=Application.dataPath+"/ArtRes/AB/"+platform;
        if(!Directory.Exists(sourcePath))
        {
            EditorUtility.DisplayDialog(
                "复制失败",
                "找不到AB构建目录:\n"+sourcePath,
                "确定");
            return;
        }

        string compareFile=Path.Combine(sourcePath,"ABCompareFile.txt");
        if(!File.Exists(compareFile))
        {
            EditorUtility.DisplayDialog(
                "复制失败",
                "没有找到ABCompareFile.txt，请先执行第1步。",
                "确定");
            return;
        }

        string serverRoot=EditorUtility.OpenFolderPanel(
            "选择服务器根目录（将自动创建AB/"+platform+"）",
            "",
            "");

        if(string.IsNullOrEmpty(serverRoot)) return;

        string targetPath=Path.Combine(serverRoot,"AB",platform);
        Directory.CreateDirectory(targetPath);

        int copyCount=0;
        DirectoryInfo sourceInfo=new DirectoryInfo(sourcePath);

        //先复制正式AB包 无后缀文件才是AB包
        foreach(FileInfo file in sourceInfo.GetFiles())
        {
            if(file.Extension!="") continue;

            File.Copy(
                file.FullName,
                Path.Combine(targetPath,file.Name),
                true);
            copyCount++;
        }

        //比较文件最后复制 避免客户端提前看到新清单
        File.Copy(
            compareFile,
            Path.Combine(targetPath,"ABCompareFile.txt"),
            true);
        copyCount++;

        EditorUtility.DisplayDialog(
            "复制完成",
            "已复制 "+copyCount+" 个文件到:\n"+targetPath,
            "确定");
    }
}
