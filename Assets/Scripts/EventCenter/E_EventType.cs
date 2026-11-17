using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 事件类型(事件名)
/// </summary>
public enum E_EventType 
{
    GemCollected, //宝石被收集
    LevelProgressChanged,//关卡进度改变
    UpdateHpUI, //更新玩家血条 <int> 传入当前血量
    PlayerDie, //玩家死亡
    PlayerDamaged, //玩家受伤
    EnemyDamaged, //敌人受伤
    LevelFinisned, //关卡完成
     DialogueStart, //参数:DialogueData
    DialogueNodeChanged, //参数:DialogueNode
    DialogueChoicesShown, //参数:List<DialogueChoice>
    DialogueEnd, //无参

}
