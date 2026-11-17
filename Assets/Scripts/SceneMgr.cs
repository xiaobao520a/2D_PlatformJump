using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// 场景管理器 用来切换场景
/// </summary>
public class SceneMgr : Singleton_AutoWithMono<SceneMgr>
{
    /// <summary>
    /// 同步加载场景
    /// </summary>
    /// <param name="name"></param>
    /// <param name="callBack"></param> <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="callBack"></param>
   public void LoadScene(string name,UnityAction callBack=null)
    {
        //切换场景
        SceneManager.LoadScene(name);

        callBack?.Invoke();
    }

    /// <summary>
    /// 异步加载场景
    /// </summary>
    /// <param name="name"></param>
    /// <param name="callBack"></param>
    public void LoadSceneAsync(string name,UnityAction callBack=null)
    {
        StartCoroutine(ReaalyLoadSceneAsync(name,callBack));
    }

    private IEnumerator ReaalyLoadSceneAsync(string name,UnityAction callBack)
    {
        var ao=SceneManager.LoadSceneAsync(name);
        yield return ao;
        callBack?.Invoke();
    }

}
