using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// AB包管理器 管理资源的加载 卸载等
/// </summary>
public class ABMgr : Singleton_AutoWithMono<ABMgr>
{
    //ab包字典 因为ab包只能加载一次 防止多次加载报错 记录已经加载过的ab包
    private Dictionary<string, AssetBundle> abDic = new Dictionary<string, AssetBundle>();

    //主包 用来加载包的依赖
    private AssetBundle abMain = null;

    //主包清单 用来加载包的依赖
    private AssetBundleManifest abMainManifest = null;

    //主包加载失败的标记 用来打断"一直等主包"的等待 不然主包加载失败会一直死等
    private bool abMainLoadError = false;

    //打包时自带的AB包 在StreamingAssets下
    private string streamingPathUrl;

    //热更下载下来的AB包 在persistentDataPath下
    private string persistentPathUrl;

#if UNITY_EDITOR
    //编辑器直读模式：开着时跳过AB包 直接从工程目录里读资源
    //改完prefab存盘立刻生效 不用重新打包
    //出包时这段不参与编译 永远走真正的AB流程
    public static bool editorDirectLoad = false;

    //bundle名 -> 工程目录（一个包可以对应多个目录）
    //编辑器直读时按这张表找资源 没登记的包名会自动退回真正的AB流程
    //要注意：登记在这里的目录 要和打包时打了对应 bundle 名的那些资源是对应的
    private static readonly Dictionary<string, string[]> EditorBundleFolders =
        new Dictionary<string, string[]>
    {
        { "ui", new[] { "Assets/Prefabs/UI" } },
        {"player",new[]{"Assets/Prefabs/Player"}},
        {"music",new[]{"Assets/Audio/Music"}},
        {"sound",new[]{"Assets/Audio/Sound"}},
    };
#endif

    //初始化路径
    //ABMainName 必须和打包输出目录的最后一级名字一致
    //因为 BuildPipeline.BuildAssetBundles 生成的清单包会以输出目录的名字命名
    //所以这里把平台段拼进路径 和打包输出目录 Assets/StreamingAssets/AB/<平台名> 对齐
    //拼出来的实际结果
    //  GetABPath("PC") -> StreamingAssets/AB/PC/PC   清单包
    //  GetABPath("ui") -> StreamingAssets/AB/PC/ui   普通包
    private void Awake()
    {
        streamingPathUrl = Application.streamingAssetsPath + "/AB/" + ABMainName;
        persistentPathUrl = Application.persistentDataPath + "/AB/" + ABMainName;
    }

    /// <summary>
    /// 得到一个AB包的完整路径
    /// 热更下来的那份优先用 没有才用包体里自带的 这样热更过的包才会生效
    /// </summary>
    private string GetABPath(string abName)
    {
        string path = persistentPathUrl + "/" + abName;
        if (File.Exists(path))
            return path;

        return streamingPathUrl + "/" + abName;
    }

    //不同平台下的默认主包名
    private string ABMainName
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

    /// <summary>
    /// 加载ab包资源的方法 同步异步都在里面处理 永远异步加载包 而资源则选择是同步或异步
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="resName"></param>
    /// <param name="callBack"></param>
    /// <param name="isSync"></param>
    public void LoadRes(string abName, string resName, UnityAction<Object> callBack, bool isSync = false)
    {
        StartCoroutine(Co_LoadRes(abName, resName, callBack, isSync, null));
    }

    //2. 泛型版本
    public void LoadRes<T>(string abName, string resName, UnityAction<T> callBack, bool isSync = false) where T : Object
    {
        StartCoroutine(Co_LoadRes(abName, resName, obj => callBack(obj as T), isSync, typeof(T)));
    }


    //3. 指定类型版本
    public void LoadRes(string abName, string resName, System.Type type, UnityAction<Object> callBack, bool isSync = false)
    {
        StartCoroutine(Co_LoadRes(abName, resName, callBack, isSync, type));
    }
    //加载AB包的协程函数
    private IEnumerator Co_LoadRes(string abName, string resName, UnityAction<Object> callBack, bool isSync, System.Type assetType)
    {
#if UNITY_EDITOR
        //编辑器直读 跳过整个AB流程 直接读工程资源
        if (editorDirectLoad && EditorBundleFolders.ContainsKey(abName))
        {
            //异步模式保持"至少晚一帧"的时序 和真AB流程一致
            //不然会出现"编辑器里是同步的 出包后变成异步"这种很难查的差异
            if (!isSync)
                yield return null;

            callBack?.Invoke(FindAssetInEditor(abName, resName, assetType));
            yield break;
        }
#endif

        //1 先保证主包和主包清单加载好了 它是所有包加载的前置条件
        if (abMain == null || abMainManifest == null)
        {
            //占位 防止同时来了好几个加载请求 主包被重复加载
            if (!abDic.ContainsKey(ABMainName))
            {
                abDic[ABMainName] = null;
                abMainLoadError = false;
                StartCoroutine(Co_LoadABMain());
            }

            //一直等到包加载完毕 否则就一直等一帧检测一次加载完毕没有 加载失败就不等了
            while ((abMain == null || abMainManifest == null) && !abMainLoadError)
                yield return null;

            //主包没加载出来 就没法加载资源了 告诉外部加载失败
            if (abMain == null || abMainManifest == null)
            {
                Debug.LogError("AB主包加载失败 无法加载资源: " + abName);
                callBack?.Invoke(null);
                yield break;
            }
        }

        //2 加载当前包的所有依赖包
        string[] names = abMainManifest.GetAllDependencies(abName);
        foreach (string name in names)
        {
            if (!abDic.ContainsKey(name))
            {
                //占位
                abDic.Add(name, null);
                StartCoroutine(Co_LoadAB(name));
            }

            //一直等到包加载完毕 否则就一直等一帧检测一次加载完毕没有
            while (abDic.ContainsKey(name) && abDic[name] == null)
                yield return null;

            //加载失败会把占位删掉 这里检测到就直接结束 不然接着往下走会取到null报错
            if (!abDic.ContainsKey(name) || abDic[name] == null)
            {
                Debug.LogError("AB依赖包加载失败: " + name);
                callBack?.Invoke(null);
                yield break;
            }
        }

        //3 加载当前包
        if (!abDic.ContainsKey(abName))
        {
            abDic[abName] = null;
            StartCoroutine(Co_LoadAB(abName));
        }

        //一直等到包加载完毕 否则就一直等一帧检测一次加载完毕没有
        while (abDic.ContainsKey(abName) && abDic[abName] == null)
            yield return null;

        if (!abDic.ContainsKey(abName) || abDic[abName] == null)
        {
            Debug.LogError("AB包加载失败: " + abName);
            callBack?.Invoke(null);
            yield break;
        }

        //4 包都准备好了 开始加载资源
        //根据isAsync来选择是同步加载资源 还是异步
        //同步加载资源
        if (isSync)
        {
            Object obj = assetType == null ? abDic[abName].LoadAsset(resName) :
                abDic[abName].LoadAsset(resName, assetType);

            callBack?.Invoke(obj);
        }

        //异步加载资源
        else
        {
            StartCoroutine(Co_ReallyLoadRes(abName, resName, callBack, assetType));
        }
    }

#if UNITY_EDITOR
    //编辑器直读：先按 bundle 名找到登记的目录 再按资源名找具体资源
    //找不到会打出明确的错误 方便一眼看出是目录配错还是资源名写错
    private static Object FindAssetInEditor(string abName, string resName, System.Type assetType)
    {
        if (!EditorBundleFolders.TryGetValue(abName, out string[] folders))
            return null;

        System.Type type = assetType ?? typeof(Object);
        string searched = "";

        foreach (string folder in folders)
        {
            searched += folder + "  ";

            if (!AssetDatabase.IsValidFolder(folder))
            {
                Debug.LogWarning($"编辑器直读: 登记的目录不存在 {folder}（包 {abName}）");
                continue;
            }

            foreach (string guid in AssetDatabase.FindAssets(resName, new[] { folder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetDatabase.IsValidFolder(path)) continue;

                //FindAssets 是模糊匹配 这里要求文件名完全一致
                if (Path.GetFileNameWithoutExtension(path) != resName) continue;

                //主资源类型对得上 直接返回
                Object asset = AssetDatabase.LoadAssetAtPath(path, type);
                if (asset != null) return asset;

                //主资源类型对不上 就去子资源里找
                //比如一张多 sprite 的贴图 主资源不是 Sprite 真正的 Sprite 在子资源里
                foreach (Object sub in AssetDatabase.LoadAllAssetsAtPath(path))
                {
                    if (sub != null && type.IsAssignableFrom(sub.GetType()))
                        return sub;
                }

                Debug.LogWarning($"编辑器直读: 找到了 {path} 但里面没有 {type.Name} 类型的东西");
            }
        }

        Debug.LogError($"编辑器直读: 在 [{searched.Trim()}] 里找不到名为 {resName} 的 {type.Name}（包 {abName}）");
        return null;
    }
#endif


    //加载主包 主包Manifest 的异步协程函数
    private IEnumerator Co_LoadABMain()
    {
        string path = GetABPath(ABMainName);
        var abcr = AssetBundle.LoadFromFileAsync(path);
        yield return abcr;

        if (abcr.assetBundle == null)
        {
            Debug.LogError("AB主包加载失败: " + path);
            abDic.Remove(ABMainName);          //移除占位 让后续调用能重试
            abMainLoadError = true;            //标记失败 让正在等主包的加载请求别死等
            yield break;
        }

        abMain = abcr.assetBundle;
        abMainManifest = abMain.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        abDic[ABMainName] = abMain;
        abMainLoadError = false;
    }


    //加载AB包的 异步协程函数
    private IEnumerator Co_LoadAB(string abName)
    {
        string path = GetABPath(abName);
        var abcr = AssetBundle.LoadFromFileAsync(path);
        yield return abcr;

        if (abcr.assetBundle == null)
        {
            Debug.LogError("AB包加载失败: " + path);
            abDic.Remove(abName);          //移除占位 让后续调用能重试
            yield break;
        }

        abDic[abName] = abcr.assetBundle;
    }


    //真正的 拿到包之后 加载某一个资源的方法
    private IEnumerator Co_ReallyLoadRes(string abName, string resName, UnityAction<Object> callBack, System.Type assetType)
    {
        var abr = assetType == null ? abDic[abName].LoadAssetAsync(resName) :
            abDic[abName].LoadAssetAsync(resName, assetType);
        yield return abr;

        callBack?.Invoke(abr.asset);
    }


    //卸载的时候不需要异步 因为此时包已经在内存中 卸载很快 不需要硬盘IO 或者网络IO 而且可以放在加载条之类的地方卸载
    //同步卸载AB包 只卸载了目标包 没卸载对应的依赖包 false意味着只删包 true意味着把通过这个包加载出来的资源也全删了
    public void Unload(string abName, bool deleteRes = false)
    {
        if (!abDic.ContainsKey(abName))
            return;

        //可能还在加载中(占位是null) 那就只把记录删掉
        if (abDic[abName] != null)
            abDic[abName].Unload(deleteRes);

        abDic.Remove(abName);

        //卸载的是主包 主包引用也要清掉 下次加载会重新加载主包
        if (abName == ABMainName)
        {
            abMain = null;
            abMainManifest = null;
        }
    }


    //同步卸载所有AB包 false意味着只删包 true意味着把通过这个包加载出来的资源也全删了
    public void UnLoadAll(bool deleteRes = false)
    {
        StopAllCoroutines();
        AssetBundle.UnloadAllAssetBundles(deleteRes);
        abDic.Clear();

        abMain = null;
        abMainManifest = null;
        abMainLoadError = false;
    }

}
