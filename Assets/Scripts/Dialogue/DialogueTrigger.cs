using UnityEngine;

//进入触发区域后播放一段对话
[DisallowMultipleComponent]
public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueData dialogueData;
    [SerializeField] private bool triggerOnce=true;

    private bool hasTriggered;
    private Collider2D triggerCollider;

    void Awake()
    {
        triggerCollider=GetComponent<Collider2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(hasTriggered||DialogueMgr.Instance.IsPlaying) return;
        if(!other.CompareTag("Player")) return;

        if(dialogueData==null)
        {
            Debug.LogWarning("DialogueTrigger没有配置DialogueData: "+name);
            return;
        }

        if(triggerOnce)
        {
            hasTriggered=true;
            if(triggerCollider!=null)
                triggerCollider.enabled=false;
        }

        UIMgr.Instance.ShowPanel<DialoguePanel>(
            E_UILayer.Top,
            panel => DialogueMgr.Instance.Play(dialogueData));
    }
}
