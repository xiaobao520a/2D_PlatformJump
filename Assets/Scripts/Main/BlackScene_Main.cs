using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackScene_Main : MonoBehaviour
{
    [SerializeField] private DialogueData dialogueData;
    private bool isLoadingLevel1=false;

    void OnEnable()
    {
        EventCenter.Instance.AddListener(E_EventType.DialogueEnd,LoadLevel1);
    }

    void OnDisable()
    {
        EventCenter.Instance.RemoveListener(E_EventType.DialogueEnd,LoadLevel1);
    }
    void Start()
    {
        AudioMgr.Instance.PlayBackgroundMusic("Floating Cities");

        UIMgr.Instance.ShowPanel<DialoguePanel>(E_UILayer.Top, (panel) =>
        {
            DialogueMgr.Instance.Play(dialogueData);
        });
    }

    private void LoadLevel1()
    {
        if(isLoadingLevel1) return;
        isLoadingLevel1=true;
        SceneMgr.Instance.LoadSceneAsync("Level_Tutorial");
    }

    
}
