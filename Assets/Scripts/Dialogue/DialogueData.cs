using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Dialogue",menuName ="Game/Dialogue/Dialogue")]
public class DialogueData :ScriptableObject
{
    public string startNodeId="start";
    public List<DialogueNode>nodes=new List<DialogueNode>();

    //按id找节点
    public DialogueNode GetNode(string id)
    {
        for(int i=0;i<nodes.Count;i++)
        {
            if(nodes[i].id==id) 
            return nodes[i];
        }
        return null;
    }
}

[System.Serializable]
public class DialogueNode
{
    public string id; //节点唯一的名字 比如start
    public string speaker; //说话人
    [TextArea(2,6)] public string text; //内容
    public string next; //下一句的Id 空就是结束
    public List<DialogueChoice>choices=new List<DialogueChoice>();
}

[System.Serializable]
public class DialogueChoice
{
    public string text; //选项按钮上显示的字
    public string next; //选完跳到哪个节点
}
