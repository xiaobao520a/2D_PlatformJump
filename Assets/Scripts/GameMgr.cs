using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMgr : Singleton_WithMono<GameMgr>
{
    [Header("关卡相关")]
    [SerializeField] private int finishWorth=100; //完成关卡的最终收集价值
    private int nowWorth=0;
    public bool finishLevel=false; //是否完成关卡
    public int level=0; //当前通过关卡数
    private bool endPanelShown=false; //结束面板只显示一次

    void OnEnable()
    {
        EventCenter.Instance.AddListener<int>(E_EventType.GemCollected,AddWorth);
        EventCenter.Instance.AddListener(E_EventType.PlayerDie,PlayerDie);
        EventCenter.Instance.AddListener(E_EventType.LevelFinisned,LevelFinisned);

    }

    void OnDisable()
    {
        EventCenter.Instance.RemoveListener<int>(E_EventType.GemCollected,AddWorth);
        EventCenter.Instance.RemoveListener(E_EventType.PlayerDie,PlayerDie);
        EventCenter.Instance.RemoveListener(E_EventType.LevelFinisned,LevelFinisned);

    }
    

    //增加关卡进度的方法
    private void AddWorth(int worth)
    {
        nowWorth+=worth;
        EventCenter.Instance.Broadcast<int>(E_EventType.LevelProgressChanged,nowWorth);

        //如果收集的价值超过了完成价值
        if(nowWorth>=finishWorth)
        {
            finishLevel=true;
        }
    }

    //就是Player死亡时调用的方法
    private void PlayerDie()
    {
        if(endPanelShown) return;

        endPanelShown=true;
        Time.timeScale=0;

        UIMgr.Instance.ShowPanel<EndPanel>(E_UILayer.Middle,
            (panel)=>
            {
                panel.SetDeath(level);
            },true);
    }

    private void LevelFinisned()
    {
        if(endPanelShown) return;

        AudioMgr.Instance.PlaySoundEffect("doorOpen_001",false,true);
        endPanelShown=true;
        Time.timeScale=0;

         UIMgr.Instance.ShowPanel<EndPanel>(E_UILayer.Middle,
            (panel)=>
            {
                bool isFinalLevel=SceneManager.GetActiveScene().name=="Level1";
                panel.SetLevelComplete(GetNextSceneName(),isFinalLevel);
            },true);
        
    }

    public void ResetGame()
    {
        nowWorth=0;
        finishLevel=false;
        level=0;
        endPanelShown=false;
    }

    private string GetNextSceneName()
    {
        string currentScene=SceneManager.GetActiveScene().name;
        if(currentScene=="Level_Tutorial")
            return "Level1";

        return "StartScene";
    }
}
