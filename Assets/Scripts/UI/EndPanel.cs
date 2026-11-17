using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndPanel : BasePanel
{
    private TextMeshProUGUI tmp_Survive;
    private TextMeshProUGUI tmp_GameOver;
    private Button btn_BackToMenu;
    private TextMeshProUGUI btnLabel;

    private string deathTitleText;
    private Color deathTitleColor;
    private bool isLevelComplete;
    private string nextSceneName;

    override protected void Init()
    {
        base.Init();
        tmp_Survive=GetControl<TextMeshProUGUI>("TMP_Survive");
        tmp_GameOver=GetControl<TextMeshProUGUI>("TMP_GameOver");
        btn_BackToMenu=GetControl<Button>("Btn_BackToMenu");
        btnLabel=btn_BackToMenu.GetComponentInChildren<TextMeshProUGUI>(true);

        deathTitleText=tmp_GameOver.text;
        deathTitleColor=tmp_GameOver.color;
    }
    
    public void SetDeath(int level)
    {
        isLevelComplete=false;
        nextSceneName=null;

        tmp_GameOver.gameObject.SetActive(true);
        tmp_GameOver.text=deathTitleText;
        tmp_GameOver.color=deathTitleColor;

        tmp_Survive.gameObject.SetActive(true);
        tmp_Survive.text="你完成了 " + level+"关!";
        btnLabel.text="回到主界面";
    }

    public void SetLevelComplete(string nextScene, bool isFinalLevel=false)
    {
        isLevelComplete=true;
        nextSceneName=nextScene;

        tmp_GameOver.gameObject.SetActive(true);
        tmp_GameOver.color=Color.white;

        if(isFinalLevel)
        {
            tmp_GameOver.text="恭喜通关";
            tmp_Survive.gameObject.SetActive(true);
            tmp_Survive.text="敬请期待后续哦!!";
            btnLabel.text="回到主界面";
        }
        else
        {
            tmp_GameOver.text="完成!";
            tmp_Survive.gameObject.SetActive(false);
            btnLabel.text="继续前进";
        }
    }

    protected override void ClickBtn(string btnName)
    {
        switch (btnName)
        {
            case "Btn_BackToMenu":
                Time.timeScale=1f;
                gameObject.SetActive(false);

                //隐藏EndPanel GamePanel 时间恢复并且回到开始场景 
                UIMgr.Instance.HidePanel<EndPanel>();
                UIMgr.Instance.HidePanel<GamePanel>();

                string targetScene=isLevelComplete?nextSceneName:"StartScene";
                if(string.IsNullOrEmpty(targetScene))
                    targetScene="StartScene";

                SceneMgr.Instance.LoadSceneAsync(targetScene);
                break;
        }
    }
}
