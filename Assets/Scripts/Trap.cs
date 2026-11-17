using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    [SerializeField]private float bounceVelocity; //弹力
    [SerializeField] private int damage; //伤害

    //如果碰到了陷阱 就设置弹力伤害 切换player的状态
    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.transform.CompareTag("Player"))
        {
            Player player=collision.transform.GetComponent<Player>();

            if(damage==0)
                AudioMgr.Instance.PlaySoundEffect("forceField_001",false,true);
            else
                AudioMgr.Instance.PlaySoundEffect("impactMetal_003",false,true);

            player.SetBounceSpeed(bounceVelocity);
            player.stateMachine.ChangeState(player.BounceState);
            if(damage!=0) player.TakeDamage(damage);
        }
    }
}
