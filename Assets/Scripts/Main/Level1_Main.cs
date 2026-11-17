using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level1_Main : MonoBehaviour
{
    [SerializeField] private Player player;
    void Start()
    {
        GameMgr.Instance.ResetGame();
        UIMgr.Instance.ShowPanel<GamePanel>(E_UILayer.Middle,(panel)=>
        {
            panel.InitHp(player.maxHp,player.nowHp);
        },false);
        GameMgr.Instance.ResetGame();
    }

    
}
