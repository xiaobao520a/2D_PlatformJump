using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//对话管理器
public class DialogueMgr : Singleton_WithoutMono<DialogueMgr>
{
    private DialogueMgr(){}

    public bool IsPlaying{get;private set;}
    public DialogueNode CurrentNode{get;private set;}

    private DialogueData data; //正在播放的对话数据
    private float timeScaleBeforeDialogue=1f;
    private bool gameplayPaused;

    /// <summary>
    /// 开始一段对话
    /// </summary>
    /// <param name="dialogue"></param>
    public void Play(DialogueData dialogue)
    {
        if(dialogue==null)
        {
            Debug.Log("对话数据为空");
            return;
        }

        //如果已经在播 静默重置上一段(不广播结束 避免把面板关掉)
        if(IsPlaying) ResetState();

        PauseGameplay();
        data=dialogue;
        IsPlaying=true;

        EventCenter.Instance.Broadcast(E_EventType.DialogueStart,dialogue);
        Goto(data.startNodeId);
    }

    /// <summary>
    /// 下一句(当前节点有选项时不会推进)
    /// </summary> <summary>
    /// 
    /// </summary>
    public void Advance()
    {
        if(!IsPlaying||CurrentNode==null) return;

        //有选项就停下来让玩家选
        if(HasChoices(CurrentNode)) return;

        if(string.IsNullOrEmpty(CurrentNode.next))
        {
            Finish();
            return;
        }

        Goto(CurrentNode.next);
    }


    /// <summary>
    /// 选择第index个选项
    /// </summary>
    /// <param name="index"></param>
    public void Choose(int index)
    {
        if(!IsPlaying||CurrentNode==null) return;
        if(!HasChoices(CurrentNode)) return;

        if(index<0||index>=CurrentNode.choices.Count)
        {
            Debug.LogError("选择下标越界"+index);
            return;
        }

        Goto(CurrentNode.choices[index].next);
    }

    /// <summary>
    /// 主动中断对话
    /// </summary>
    public void Stop()
    {
        if(!IsPlaying) return;

        ResetState();
        EventCenter.Instance.Broadcast(E_EventType.DialogueEnd);
    }


    /// <summary>
    /// 跳到指定节点
    /// </summary> <summary>
    /// 
    /// </summary>
    private void Goto(string id)
    {
        //空id表示这段对话结束 (选项里next留空就是这种情况)
        if(string.IsNullOrEmpty(id))
        {
            Finish();
            return;
        }

        DialogueNode node=data.GetNode(id);
        if(node==null)
        {
            Debug.Log($"对话{data.name}里找不到节点:{id}");
            Finish();
            return;
        }
        
        CurrentNode=node;

        //广播 : 换了一句
        EventCenter.Instance.Broadcast(E_EventType.DialogueNodeChanged,node);

        //广播 :有选项需要显示
        if(HasChoices(node))
            EventCenter.Instance.Broadcast(E_EventType.DialogueChoicesShown,node.choices);
    }

    private bool HasChoices(DialogueNode node)
    {
        return node.choices!=null&&node.choices.Count>0;
    }
    private void Finish()
    {
        ResetState();
        EventCenter.Instance.Broadcast(E_EventType.DialogueEnd);
    }

    private void ResetState()
    {
        ResumeGameplay();
        IsPlaying=false;
        CurrentNode=null;
        data=null;
    }

    private void PauseGameplay()
    {
        if(gameplayPaused) return;

        timeScaleBeforeDialogue=Time.timeScale;
        Time.timeScale=0f;
        gameplayPaused=true;
    }

    private void ResumeGameplay()
    {
        if(!gameplayPaused) return;

        Time.timeScale=timeScaleBeforeDialogue;
        gameplayPaused=false;
    }

}
