using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_Fall : PlayerState_Air
{
    private float originalGravityScale;
    public PlayerState_Fall(string stateName, StateMachine stateMachine, Entity entity) : base(stateName, stateMachine, entity)
    {
    }

    public override void Enter()
    {
        base.Enter();

        //下落的时候增加重力 让下落更快
        originalGravityScale=rb.gravityScale;
        rb.gravityScale*=player.fallGravityMultiplier;

        //如果是在空中的话 只能最多挑一次 只有从地面可以二段跳
        if(player.jumpCount<2) player.jumpCount=1;
    }

    public override void Update()
    {
        base.Update();

        //下落过程中只能跳一次
        if(player.playerControls.Player.Jump.WasPressedThisFrame()&&
        player.jumpCount<player.maxJumpCount)
        {
            stateMachine.ChangeState(player.JumpState);
            return;
        }

        //接触到地面就重置跳跃次数 又可以二段跳了 同时看情况切换状态
        if(player.isGrounded)
        {
            player.jumpCount=0;
            if(player.moveInput.x==0) stateMachine.ChangeState(player.IdleState);
            else stateMachine.ChangeState(player.MoveState);
            return;
        }

        //限制下落的最大速度
        if(rb.velocity.y>player.maxJumpSpeed) 
        rb.velocity=new Vector2(rb.velocity.x,player.maxJumpSpeed);
    }

    public override void Exit()
    {
        base.Exit();

        //退出时恢复重力
        rb.gravityScale=originalGravityScale;

        //跳跃次数只能在真正落地时重置
        //离开Fall进入Jump时重置会导致空中无限跳
    }
}
