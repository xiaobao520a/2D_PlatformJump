using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_Jump : PlayerState_Air
{
    public PlayerState_Jump(string stateName, StateMachine stateMachine, Entity entity) : base(stateName, stateMachine, entity)
    {
    }

    public override void Enter()
    {
        base.Enter();

        rb.velocity=new Vector2(rb.velocity.x,player.jumpSpeed);
        player.jumpCount++;
        player.smoke_VFX.Play();
    }

    public override void Update()
    {
        base.Update();

        //如果在空中 并且按下了跳跃键 并且当前跳跃次数小于最大跳跃次数 就可以二段跳
        if(player.playerControls.Player.Jump.WasPressedThisFrame()&&
        player.jumpCount<player.maxJumpCount)
        {
            rb.velocity=new Vector2(rb.velocity.x,player.jumpSpeed);
            player.jumpCount++;
            player.smoke_VFX.Play();
        }

        if(rb.velocity.y<0)
        {
            stateMachine.ChangeState(player.FallState);
            return;
        }

    }
}
