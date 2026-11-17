using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//游戏UI面板
public class GamePanel : BasePanel
{
    private Slider slider_Level;

    //Hp相关
    [SerializeField]private GameObject img_Hp;
    public Transform hpTrans; //HP的transform

    private List<GameObject>listHp=new List<GameObject>(); //当前有多少血量icon的列表

    protected override void Init()
    {
        base.Init();

        slider_Level=GetControl<Slider>("Slider_Level");


    }

    void OnEnable()
    {
        EventCenter.Instance.AddListener<int>(E_EventType.LevelProgressChanged,ChangeLevelProgress);
        EventCenter.Instance.AddListener<int>(E_EventType.UpdateHpUI,UpdateHp);

    }

    void OnDisable()
    {
        EventCenter.Instance.RemoveListener<int>(E_EventType.LevelProgressChanged,ChangeLevelProgress);
        EventCenter.Instance.RemoveListener<int>(E_EventType.UpdateHpUI,UpdateHp);

    }


    private void ChangeLevelProgress(int nowWorth)
    {
        slider_Level.value=nowWorth;
    }

    //初始化血量UI
    public void InitHp(int maxHp,int nowHp)
    {
            //先清空所有Hp图标
        ClearAllHpIcons();
        
        //创建Hp
        for(int i=0;i<maxHp;i++)
        {
            GameObject hp_Icon=Instantiate(img_Hp,hpTrans);
            listHp.Add(hp_Icon); //添加一个就加入列表
            if(i>=nowHp)
                hp_Icon.GetComponent<Image>().color=Color.white;
        }
        }

    private void UpdateHp(int nowHp)
    {
        for(int i=0;i<nowHp;i++)
            listHp[i].GetComponent<Image>().color=Color.red;

        for(int i=nowHp;i<listHp.Count;i++)
            listHp[i].GetComponent<Image>().color=Color.white;

    }


    //先清空所有Hp图标
    private void ClearAllHpIcons()
    {
        listHp.Clear();
        for(int i=hpTrans.childCount-1;i>=0;i--)
        {
            Destroy(hpTrans.GetChild(i).gameObject);
        }
    }


}
