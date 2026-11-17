using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 对话面板 只负责显示和接收点击
/// </summary> <summary>
/// 
/// </summary>
public class DialoguePanel : BasePanel
{
    [SerializeField] private GameObject choiceButtonPrefab;
    [SerializeField] private float charDelay=0.03f;

    private TextMeshProUGUI tmp_Speaker;
    private TextMeshProUGUI tmp_Content;
    private Button btn_Next;
    private Transform choiceRoot;
    
    private Coroutine typing;
    private WaitForSecondsRealtime wait;
    private bool currentHasChoices;
    private AudioSource typingAudioSource;
    private int typingAudioRequest;

    private readonly List<GameObject> choicesObjs=new List<GameObject>();

    protected override void Init()
    {
        base.Init();

        tmp_Speaker=GetControl<TextMeshProUGUI>("Tmp_Speaker");
        tmp_Content=GetControl<TextMeshProUGUI>("Tmp_Content");
        btn_Next=GetControl<Button>("Btn_Next");
        choiceRoot=transform.Find("ChoiceRoot");

        //对话期间Time.timeScale为0 打字机必须使用不受时间缩放影响的等待
        wait=new WaitForSecondsRealtime(charDelay);
    }

    void OnEnable()
    {
        EventCenter.Instance.AddListener<DialogueNode>(
            E_EventType.DialogueNodeChanged, ShowNode);

        EventCenter.Instance.AddListener<List<DialogueChoice>>(
            E_EventType.DialogueChoicesShown, ShowChoices);

        EventCenter.Instance.AddListener(
            E_EventType.DialogueEnd, ClosePanel);
    }

    void OnDisable()
    {
        EventCenter.Instance.RemoveListener<DialogueNode>(
            E_EventType.DialogueNodeChanged, ShowNode);

        EventCenter.Instance.RemoveListener<List<DialogueChoice>>(
            E_EventType.DialogueChoicesShown, ShowChoices);

        EventCenter.Instance.RemoveListener(
            E_EventType.DialogueEnd, ClosePanel);
    }

    public override void Show()
    {
        //清掉上一次的残留
        ClearChoices();
        StopTypingSound();

        if(typing!=null)
        {
            StopCoroutine(typing);
            typing=null;
        }

    }

    public override void Hide()
    {
        ClearChoices();
        StopTypingSound();

        if (typing != null)
        {
            StopCoroutine(typing);
            typing = null;
        }
    }

    //收到 换了一句的事件
    private void ShowNode(DialogueNode node)
    {
        ClearChoices();

        tmp_Speaker.text=node.speaker;
        currentHasChoices=node.choices!=null&&node.choices.Count>0;

        //打字期间保留"继续"按钮 用来点击直接显示完整句子
        btn_Next.gameObject.SetActive(true);

        if(typing!=null)
        StopCoroutine(typing);

        typing=StartCoroutine(TypeText(node.text));
    }

    //打字机
    private IEnumerator TypeText(string text)
    {
        StartTypingSound();

        tmp_Content.text=text;
        tmp_Content.ForceMeshUpdate(); //先让TMP算出字符数

        int total=tmp_Content.textInfo.characterCount;
        tmp_Content.maxVisibleCharacters=0;

        for(int i=1;i<=total;i++)
        {
            tmp_Content.maxVisibleCharacters=i;
            yield return wait;
        }

        tmp_Content.maxVisibleCharacters=total;
        typing=null;
        StopTypingSound();

        //有选项时 隐藏掉"继续"按钮 避免玩家以为能跳过选项
        if(currentHasChoices)
        btn_Next.gameObject.SetActive(false);
    }

    //收到 要显示选项的事件
    public void ShowChoices(List<DialogueChoice>choices)
    {
        ClearChoices();

        for(int i=0;i<choices.Count;i++)
        {
            int index=i; //闭包要复制一份

            GameObject obj=Instantiate(choiceButtonPrefab,choiceRoot);
            obj.GetComponentInChildren<TextMeshProUGUI>().text = choices[i].text;
            obj.GetComponent<Button>().onClick.AddListener(() =>
                DialogueMgr.Instance.Choose(index));

            choicesObjs.Add(obj);

        }
    }

    protected override void ClickBtn(string btnName)
    {
        if(btnName!="Btn_Next") return;

        //还在打字 先把这一句显示完整
        if(typing!=null)
        {
            StopCoroutine(typing);
            typing=null;
            StopTypingSound();
            tmp_Content.maxVisibleCharacters=tmp_Content.textInfo.characterCount;

            if(currentHasChoices)
            btn_Next.gameObject.SetActive(false);

            return;
        }

        //已经打完 下一句(有选项时 Advance会自动不做事)
        DialogueMgr.Instance.Advance();
    }

    //对话结束
    private void ClosePanel()
    {
        ClearChoices();
        StopTypingSound();

        if(typing!=null)
        {
            StopCoroutine(typing);
            typing=null;
        }

        gameObject.SetActive(false);
    }

    private void StartTypingSound()
    {
        StopTypingSound();

        int request=++typingAudioRequest;

        AudioMgr.Instance.PlaySoundEffect(
            "dialogue_typewriter_loop",
            true,
            true,
            source =>
            {
                //资源加载期间可能已经跳过当前句或关闭了面板
                if(request!=typingAudioRequest||source==null)
                {
                    if(source!=null)
                        AudioMgr.Instance.StopSoundEffect(source);
                    return;
                }

                typingAudioSource=source;
            });
    }

    private void StopTypingSound()
    {
        typingAudioRequest++;

        if(typingAudioSource!=null)
        {
            AudioMgr.Instance.StopSoundEffect(typingAudioSource);
            typingAudioSource=null;
        }
    }

        private void ClearChoices()
    {
        for(int i=0;i<choicesObjs.Count;i++)
            Destroy(choicesObjs[i]);

        choicesObjs.Clear();
    }

}
