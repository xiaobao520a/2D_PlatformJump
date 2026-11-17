using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_Move : PlayerState_Grounded
{
    public PlayerState_Move(string stateName, StateMachine stateMachine, Entity entity) : base(stateName, stateMachine, entity)
    {
    }


    public override void Update()
    {
        base.Update();
        
        //移动加翻转
        rb.velocity = new Vector2(player.moveInput.x * player.moveSpeed, rb.velocity.y);
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
        
        if(player.moveInput.x == 0f)
        {
            stateMachine.ChangeState(player.IdleState);
            return;
        }
    }
    
}
