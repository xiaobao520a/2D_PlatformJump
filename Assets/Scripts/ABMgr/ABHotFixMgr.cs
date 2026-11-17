using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

/// <summary>
/// AB更新管理器 用来从远端仓库下载 更新ab包
/// </summary>
//因为可能要异步加载 做成继承Mono的单例模式基类
public class ABHotFixMgr : Singleton_AutoWithMono<ABHotFixMgr>
{
    //远端服务器地址 可以修改 目前测试就是本地回环地址
    private string remoteServerPathUrl = "http://127.0.0.1:8000";

    //远端资源对比文件具体存的string 资源对比文件的格式是 包名 大小 md5|包名 大小 md5|.....
    private string remoteABCompareFile = null;

    //远端和本地的 ab包名字和对应md5码的dic 用来比对ab资源对比文件来决定是否更新AB包
    private Dictionary<string, string> remoteABDic = new Dictionary<string, string>();
    private Dictionary<string, string> localABDic = new Dictionary<string, string>();

    private bool remoteABCompareFileLoadFinish = false;
    private bool localABCompareFileLoadFinish = false;
    private bool compareFileLoadError = false;
    private bool downloadError = false;

    //设置远端服务器地址
    public void SetRemoteServerPathUrl(string remoteServerPathUrl) => this.remoteServerPathUrl = remoteServerPathUrl;

    //当前平台目录名 必须和打包输出目录/ABMgr保持一致
    private string PlatformName
    {
        get
        {
#if UNITY_ANDROID
            return "Android";
#elif UNITY_IOS
            return "IOS";
#else
            return "PC";
#endif
        }
    }

    //热更资源本地目录 persistentDataPath/AB/<平台名>
    private string LocalABPath
    {
        get { return Application.persistentDataPath + "/AB/" + PlatformName; }
    }

    //安装包内资源目录 StreamingAssets/AB/<平台名>
    private string StreamingABPath
    {
        get { return Application.streamingAssetsPath + "/AB/" + PlatformName; }
    }

    //远端服务器上的平台目录
    private string RemoteABPath
    {
        get { return remoteServerPathUrl + "/AB/" + PlatformName; }
    }

    public void HotFix(UnityAction<bool> callBack=null)
    {
        StartCoroutine(HotFixAsync(callBack));
    }
    private IEnumerator HotFixAsync(UnityAction<bool> callBack)
    {
        //重置
        remoteABCompareFileLoadFinish = false;
        localABCompareFileLoadFinish = false;
        compareFileLoadError = false;
        localABDic.Clear();
        remoteABDic.Clear();
        remoteABCompareFile = null;
        downloadError = false;

        //得到本地和远端的AB资源对比文件
        GetRemoteABCompareFile();
        GetLocalABCompareFile();

        //没加载完就一直等
        while ((!remoteABCompareFileLoadFinish || !localABCompareFileLoadFinish) && !compareFileLoadError)
        {
            yield return null;
        }

        if(compareFileLoadError)
        {
            Debug.LogError("AB资源对比文件加载或解析失败,热更中断");
            callBack?.Invoke(false);
            yield break;
        }

        //写入磁盘的路径 persistentDataPath/AB/<平台名>
        string writePath = LocalABPath;
        //防止第一次加载的时候没有这个路径 先创建 有的话就不变
        Directory.CreateDirectory(writePath);

        //写入资源的路径 远端服务器也按平台分目录
        string resPath = RemoteABPath;
        //加载完之后开始对比 更新AB包
        foreach (string name in remoteABDic.Keys)
        {
            //如果本地dic没有远端dic的键 这个AB包要加载
            if (!localABDic.ContainsKey(name))
            {
                yield return DownloadFile(resPath + "/" + name, writePath + "/" + name);
                if (downloadError)
                {
                    Debug.LogError("有AB包没下成功,这次热更中断,本地对比文件不写,下次进游戏会重新来");
                    callBack?.Invoke(false);
                    yield break;       // 写在 HotFixAsync 里，结束的才是整个热更流程
                }
            }
            //如果有 分情况
            else
            {
                //md5码不同 重新写入 相同不管
                if (remoteABDic[name] != localABDic[name])
                {
                    yield return DownloadFile(resPath + "/" + name, writePath + "/" + name);
                    if (downloadError)
                    {
                        Debug.LogError("有AB包没下成功,这次热更中断,本地对比文件不写,下次进游戏会重新来");
                        callBack?.Invoke(false);
                        yield break;       
                    }
                }
                localABDic.Remove(name);
            }
        }

        //最后看本地dic还有没有剩的 有剩的也要删
        foreach (string name in localABDic.Keys)
        {
            File.Delete(writePath + "/" + name);
        }

        //热更完毕之后要更新本地的ab资源对比文件
        File.WriteAllText(writePath + "/ABCompareFile.txt",remoteABCompareFile);

        remoteABDic.Clear();
        localABDic.Clear();

        print("热更完成");
        callBack?.Invoke(true);

    }

    //得到远端服务器上的AB资源对比文件
    private void GetRemoteABCompareFile()
    {
        UnityWebRequestMgr.Instance.LoadRes<string>(
            RemoteABPath + "/ABCompareFile.txt",
            HandleRemoteABCompareFile,
            HandleCompareFileLoadError);
    }

    //得到本地服务器上的AB资源对比文件 
    private void GetLocalABCompareFile()
    {
        //先在persistentDataPath里面找 找到了直接用 如果没找到 说明是第一次加载 此时的AB资源对比文件在StreamingAssets里
        string path = LocalABPath + "/ABCompareFile.txt";
        if (File.Exists(path))
        {
            try
            {
                HandleLocalABCompareFile(File.ReadAllText(path));
            }
            catch(System.Exception e)
            {
                Debug.LogError("读取本地AB资源对比文件失败: " + e.Message);
                HandleCompareFileLoadError();
            }
        }

        else
        {
            path = StreamingABPath + "/ABCompareFile.txt";
            UnityWebRequestMgr.Instance.LoadRes<string>(
                path,
                HandleLocalABCompareFile,
                HandleCompareFileLoadError);
        }
    }

    //处理远端服务器存储的AB资源对比文件
    private void HandleRemoteABCompareFile(string text)
    {
        try
        {
            remoteABCompareFile = text;

            string[] infos = text.Split("|");
            foreach (string info in infos)
            {
                if(string.IsNullOrWhiteSpace(info)) continue;

                string[] strs = info.Split(new[]{' '},System.StringSplitOptions.RemoveEmptyEntries);
                if(strs.Length<3)
                    throw new System.FormatException("远端AB资源对比文件格式错误: " + info);

                remoteABDic.Add(strs[0], strs[2]);
            }

            remoteABCompareFileLoadFinish = true;
        }
        catch(System.Exception e)
        {
            Debug.LogError("解析远端AB资源对比文件失败: " + e.Message);
            HandleCompareFileLoadError();
        }
    }

    //处理本地服务器存储的AB资源对比文件
    private void HandleLocalABCompareFile(string text)
    {
        try
        {
            string[] infos = text.Split("|");
            foreach (string info in infos)
            {
                if(string.IsNullOrWhiteSpace(info)) continue;

                string[] strs = info.Split(new[]{' '},System.StringSplitOptions.RemoveEmptyEntries);
                if(strs.Length<3)
                    throw new System.FormatException("本地AB资源对比文件格式错误: " + info);

                localABDic.Add(strs[0], strs[2]);
            }

            localABCompareFileLoadFinish = true;
        }
        catch(System.Exception e)
        {
            Debug.LogError("解析本地AB资源对比文件失败: " + e.Message);
            HandleCompareFileLoadError();
        }
    }

    private void HandleCompareFileLoadError()
    {
        compareFileLoadError = true;
    }

    // 下载一个文件到本地，下完了才继续往下走 下载失败直接 别走了
    private IEnumerator DownloadFile(string url, string savePath)
    {
        UnityWebRequest req = UnityWebRequest.Get(url);
        req.downloadHandler = new DownloadHandlerFile(savePath);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("下载失败: " + url + " " + req.error);
            downloadError = true;
        }

        req.Dispose();
    }
}
