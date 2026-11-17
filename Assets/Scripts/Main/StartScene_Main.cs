using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartScene_Main : MonoBehaviour
{
    void Start()
    {
        ABHotFixMgr.Instance.HotFix();
        Debug.Log(Application.persistentDataPath);
        
        //显示开始界面
        UIMgr.Instance.ShowPanel<StartPanel>(E_UILayer.Middle,null,true);

        //播放背景音乐
        AudioMgr.Instance.PlayBackgroundMusic("BackgroundMusic");
    }

}
