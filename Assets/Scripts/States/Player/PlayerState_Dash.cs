using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//冲刺状态 冲刺期间无敌
public class PlayerState_Dash : PlayerState
{
    private float originalGravityScale;
    public PlayerState_Dash(string stateName, StateMachine stateMachine, Entity entity) : base(stateName, stateMachine, entity)
    {
    }

    public override void Enter()
    {
        base.Enter();

        //有方向输入就按输入方向冲刺 没有输入就按当前朝向冲刺
        float direction=player.flipDirection;
        if(Mathf.Abs(player.moveInput.x)>0.01f)
        {
            direction=Mathf.Sign(player.moveInput.x);
            if(direction!=player.flipDirection)
            {
                player.flipDirection=(int)direction;
                player.SetFlip();
            }
        }

        //冲刺期间暂时关闭重力
        rb.velocity=new Vector2(direction*player.dashSpeed,0);
        originalGravityScale=rb.gravityScale;
        rb.gravityScale=0;
        timer=0;
    }

    public override void Update()
    {
        base.Update();

        timer+=Time.deltaTime;
        if(timer>=player.dashDuration)
        {
            stateMachine.ChangeState(player.isGrounded?player.IdleState:player.FallState);
            return;
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.canDash=false;
        rb.gravityScale=originalGravityScale;
    }

}
