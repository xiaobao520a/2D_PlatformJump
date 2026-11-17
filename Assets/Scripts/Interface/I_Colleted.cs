using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 可被收集接口 实现Collect方法 如宝石等
/// </summary>
public interface I_Colleted
{
    public void Collect(Player player);
}
