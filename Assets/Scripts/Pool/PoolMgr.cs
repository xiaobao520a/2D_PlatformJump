using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 对象池管理器
/// </summary> <summary>
/// 
/// </summary>
public class PoolMgr : Singleton_WithoutMono<PoolMgr>
{
    private PoolMgr() {}

    //一个池子：预制体 + 回收的实例
    private class Pool
    {
        public GameObject prefab;
        public Stack<GameObject> stack = new Stack<GameObject>(); //具体放物体的池子
        public List<UnityAction>callBacks = new List<UnityAction>(); //加载完的回调
        public bool isLoading; //是否正在加载
    }

    private Dictionary<string, Pool> poolDic = new Dictionary<string, Pool>();

    //用"包名/资源名"当key 防止不同包有同名资源
    private string GetKey(string abName, string resName)
    {
        return abName + "/" + resName;
    }

    //① 预加载：异步加载预制体 加载完回调
    public void Preload(string abName, string resName, UnityAction onReady = null)
    {
        string key = GetKey(abName, resName);
        if(poolDic.TryGetValue(key, out Pool pool))
        {
            //已经加载好了 直接调回调函数就行
            if (pool.prefab != null)
                onReady?.Invoke();

            //如果是正在加载 并且回调不为空 就加入callBacks列表 等加载完再调用
            else if(pool.isLoading&&onReady!=null)
                pool.callBacks.Add(onReady);

            return;
        }

        poolDic[key] = new Pool{isLoading=true};   //先占位 表示正在加载
        if(onReady!=null)
            poolDic[key].callBacks.Add(onReady);

        ABMgr.Instance.LoadRes<GameObject>(abName, resName, (prefab) =>
        {
            if (prefab == null)
            {
                poolDic.Remove(key);
                Debug.LogError("预制体加载失败: " + key);
                return;
            }
            
            if (!poolDic.TryGetValue(key, out var p) || !p.isLoading) return;
            poolDic[key].prefab = prefab;
            poolDic[key].isLoading = false;
            //加载完了 调用回调函数
            foreach (var callback in poolDic[key].callBacks)
                callback?.Invoke();

            poolDic[key].callBacks.Clear(); //调用完之后清空委托 防止内存泄漏
        });
    }

    //② 取：有就复用 没有就用预制体创建
    public GameObject Pop(string abName, string resName)
    {
        string key = GetKey(abName, resName);

        if (!poolDic.TryGetValue(key, out Pool pool))
        {
            Debug.LogError("池子没有注册: " + key);
            return null;
        }

        //预制体还在异步加载时直接返回null 由调用方安全跳过这一帧
        if(pool.prefab==null)
            return null;

        while (pool.stack.Count > 0)
        {
            GameObject obj = pool.stack.Pop();
            if (obj == null) continue;   //被场景销毁的跳过

            obj.SetActive(true);
            obj.GetComponent<I_Poolable>()?.OnPop(); //初始化状态
            return obj;
        }

        GameObject obj1=GameObject.Instantiate(pool.prefab);
        obj1.GetComponent<I_Poolable>()?.OnPop(); //初始化状态
        return obj1;
    }

    //③ 还：失活后放回池子
    public void Push(string abName, string resName, GameObject obj)
    {
        if (obj == null) return;

        string key = GetKey(abName, resName);
        if (!poolDic.TryGetValue(key, out Pool pool))
        {
            Debug.LogWarning("没有对应的池子 直接销毁: " + key);
            GameObject.Destroy(obj);
            return;
        }

        var poolAble=obj.GetComponent<I_Poolable>(); 
        if(poolAble!=null)
            poolAble.OnPush(); //重置状态
        
        obj.SetActive(false);
        
        if(poolAble!=null&&pool.stack.Count>=poolAble.MaxPoolSize) //池子满了 直接销毁
        {
            GameObject.Destroy(obj);
            return;
        }
        pool.stack.Push(obj);
    }

    //清空所有池子 切场景/退出时调用
    public void Clear()
    {
        foreach (var pool in poolDic.Values)
        {
            while (pool.stack.Count > 0)
            {
                GameObject obj = pool.stack.Pop();
                if (obj != null)
                    GameObject.Destroy(obj);
            }
            pool.callBacks.Clear();
        }
        poolDic.Clear();
    }
}
