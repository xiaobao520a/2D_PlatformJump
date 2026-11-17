using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_Air : PlayerState
{
    public PlayerState_Air(string stateName, StateMachine stateMachine, Entity entity) : base(stateName, stateMachine, entity)
    {
    }

    public override void Update()
    {
        base.Update();

        //空中的时候也可以左右移动 翻转
        rb.velocity=new Vector2(player.moveInput.x*player.moveSpeed*player.inAirSpeedMultiplier,rb.velocity.y);
        if(player.moveInput.x > 0f&&player.flipDirection<0)
        {
            player.flipDirection = 1;
            player.SetFlip();
            //翻转的同时添加粒子特效
            player.smoke_VFX.Play();
        }
        else if(player.moveInput.x < 0f&&player.flipDirection > 0)
        {
            player.flipDirection = -1;
            player.SetFlip();
            player.smoke_VFX.Play();
        }
        
    }
}
