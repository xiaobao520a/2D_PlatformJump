using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//开始面板
public class StartPanel : BasePanel
{
    protected override void ClickBtn(string btnName)
    {
        switch (btnName)
        {
            //按下开始游戏后 隐藏StartPanel 切换场景同时关掉背景音乐
            case "Btn_Start":
                UIMgr.Instance.HidePanel<StartPanel>();
                SceneMgr.Instance.LoadScene("BlackScene");
                break;

            //按下退出游戏按键就退出游戏嘛
            case "Btn_Exit":
            Application.Quit();
                break;

            //按下设置按键就打开设置面板
            case "Btn_Setting":
            UIMgr.Instance.HidePanel<StartPanel>(false);
            UIMgr.Instance.ShowPanel<SettingPanel>(E_UILayer.Middle);
            break;
        }
    }
}
