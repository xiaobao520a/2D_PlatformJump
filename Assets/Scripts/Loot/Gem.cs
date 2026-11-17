using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gem : MonoBehaviour,I_Colleted
{
    public void Collect(Player player)
    {
       AudioMgr.Instance.PlaySoundEffect("laserSmall_003",false,true);
       EventCenter.Instance.Broadcast<int>(E_EventType.GemCollected,worth);
       Destroy(this.gameObject);
    }

    [SerializeField] private int worth;
    


}
