using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthItem : MonoBehaviour,I_Colleted
{
    [SerializeField] private int addHp=1; //增加的血量
    public void Collect(Player player)
    {
        AudioMgr.Instance.PlaySoundEffect("forceField_003",false,true);
        player.Healed(addHp);
        Destroy(gameObject);
    }

    


}
