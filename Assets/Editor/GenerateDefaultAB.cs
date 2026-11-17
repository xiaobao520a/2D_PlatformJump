using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 生成默认AB包资源 到streamingAssets文件夹中 作默认资源
/// </summary>
public class GenerateDefaultAB
{
    /// <summary>
    /// 把选中的AB包复制到StreamingAssets的指定平台目录
    /// </summary>
    /// <param name="platform">目标平台目录名 PC / Android / IOS</param>
    public static void GenerateDefaultABToStreamingAssets(string platform)
    {
        //通过Editor的Selection类中的方法 获取在Project窗口中鼠标选中的资源 当然只选AB包
        Object[] selectedAssets = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);

        //如果没有选中资源 return;
        if (selectedAssets.Length == 0) return;

        //用窗口里选择的平台目录
        string abRootAsset = "Assets/StreamingAssets/AB/" + platform;
        string abRootFile = Application.streamingAssetsPath + "/AB/" + platform;

        //旧版扁平目录下 PC 是一个文件 会挡住同名文件夹
        if (File.Exists(abRootFile))
        {
            Debug.LogError("StreamingAssets/AB/" + platform + " 当前是一个文件，请先迁移成目录");
            return;
        }

        if (!AssetDatabase.IsValidFolder("Assets/StreamingAssets"))
            AssetDatabase.CreateFolder("Assets", "StreamingAssets");

        if (!AssetDatabase.IsValidFolder("Assets/StreamingAssets/AB"))
            AssetDatabase.CreateFolder("Assets/StreamingAssets", "AB");

        if (!AssetDatabase.IsValidFolder(abRootAsset))
            AssetDatabase.CreateFolder("Assets/StreamingAssets/AB", platform);

        StringBuilder stringBuilder = new StringBuilder();

        //遍历鼠标选中的资源
        foreach (Object asset in selectedAssets)
        {
            //得到资源路径
            string path = AssetDatabase.GetAssetPath(asset);

            //跳过文件夹
            if (AssetDatabase.IsValidFolder(path)) continue;

            //得到文件名 放入streamingAssets的取名就为文件名 例如/abCompareFile.txt 有/方便后面拼接路径
            string fileName = path.Substring(path.LastIndexOf('/'));

            //如果名字里面有.说明有后缀名 那就不是AB包 就不要复制 继续下一个
            if (fileName.IndexOf(".") != -1) continue;

            string destAssetPath = abRootAsset + fileName;
            string destFilePath = abRootFile + fileName;

            //重新生成时覆盖旧包
            if (File.Exists(destFilePath))
                AssetDatabase.DeleteAsset(destAssetPath);

            //把资源复制到StreamingAssets的当前平台目录
            if (!AssetDatabase.CopyAsset(path, destAssetPath))
            {
                Debug.LogError("复制AB包失败: " + path + " -> " + destAssetPath);
                continue;
            }

            //得到具体的文件信息
            FileInfo fileInfo = new FileInfo(destFilePath);

            //创建资源对比文件 只有没有后缀的时候 才是AB包
            stringBuilder.Append(fileInfo.Name + " " + fileInfo.Length + " " + CreateABCompare.GetMD5(fileInfo.FullName) + "|");
        }

        //如果长度为0 说明一个ab包也没有
        if (stringBuilder.Length == 0)
        {
            Debug.LogWarning("没有复制任何AB包");
            return;
        }

        //去掉最后一个|符号
        string abCompareInfo = stringBuilder.ToString();
        abCompareInfo = abCompareInfo.Substring(0, abCompareInfo.Length - 1);

        //存入当前平台的AB资源对比文件
        File.WriteAllText(abRootFile + "/ABCompareFile.txt", abCompareInfo);

        AssetDatabase.Refresh();
    }
}
