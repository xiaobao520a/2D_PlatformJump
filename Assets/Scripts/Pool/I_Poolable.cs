using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 要使用对象池的具体物体去继承 定义最大容量 以及Push进去时重置状态的方法 以及Pop出来初始化状态的方法
/// </summary>
public interface I_Poolable
{
    int MaxPoolSize{get;}
    void OnPush();
    void OnPop();

}
