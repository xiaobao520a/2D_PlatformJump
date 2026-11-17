using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level_Tutorial_Main : MonoBehaviour
{
    [SerializeField] private DialogueData data;
    [SerializeField] private Player player;

    void Start()
    {
        GameMgr.Instance.ResetGame();

        if(player==null)
        {
            GameObject playerObj=GameObject.FindGameObjectWithTag("Player");
            if(playerObj!=null)
                player=playerObj.GetComponent<Player>();
        }

        if(player!=null)
        {
            player.nowHp=Mathf.Max(1,player.maxHp-1);

            UIMgr.Instance.ShowPanel<GamePanel>(E_UILayer.Middle,
                panel => panel.InitHp(player.maxHp,player.nowHp));
        }

        if(data!=null)
        {
            UIMgr.Instance.ShowPanel<DialoguePanel>(E_UILayer.Top, (panel) =>
            {
                DialogueMgr.Instance.Play(data);
            });
        }
    }
}
