using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

/// <summary>
/// UnityWebRequest的管理器 简单封装 加载texture string assetBundle
/// </summary>
public class UnityWebRequestMgr : Singleton_AutoWithMono<UnityWebRequestMgr>
{
    /// <summary>
    /// 异步加载资源 string texture assetBundle
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="pathUrl">资源路径</param>
    /// <param name="callBack">加载成功之后的回调函数</param>
    /// <param name="failCallBack">加载失败后的回调函数</param>
    public void LoadRes<T>(string pathUrl,UnityAction<T>callBack,UnityAction failCallBack)where T: class
    {
        //开启协程
        StartCoroutine(ReallyLoadRes<T>(pathUrl, callBack, failCallBack));
    }

    private IEnumerator ReallyLoadRes<T>(string pathUrl, UnityAction<T> callBack, UnityAction failCallBack) where T : class
    {
        Type type = typeof(T);
        UnityWebRequest req = null;
        try
        {
            //根据类型加载
            if (type == typeof(string) || type == typeof(byte[]))
                req = UnityWebRequest.Get(pathUrl);

            else if (type == typeof(Texture))
                req = UnityWebRequestTexture.GetTexture(pathUrl);

            else if (type == typeof(AssetBundle))
                req = UnityWebRequestAssetBundle.GetAssetBundle(pathUrl);

            else
            {
                Debug.LogError($"UnityWebRequest不能加载{typeof(T)}类型的资源");
                failCallBack?.Invoke();
                yield break;
            }
            yield return req.SendWebRequest();

            //如果加载成功了
            if (req.result == UnityWebRequest.Result.Success)
            {
                //根据类型加载
                if (type == typeof(string))
                    callBack?.Invoke(req.downloadHandler.text as T);

                else if (type == typeof(byte[]))
                    callBack?.Invoke(req.downloadHandler.data as T);

                else if (type == typeof(Texture))
                    callBack?.Invoke(DownloadHandlerTexture.GetContent(req) as T);

                else if (type == typeof(AssetBundle))
                    callBack?.Invoke(DownloadHandlerAssetBundle.GetContent(req) as T);
            }

            //加载失败
            else
            {
                Debug.LogError($"加载失败: {pathUrl}, 状态码: {req.responseCode}, 错误: {req.error}");
                failCallBack?.Invoke();
            }
        }

        finally
        {
            //释放UnityWebRequest对象
            req?.Dispose();
        }

    }

}
