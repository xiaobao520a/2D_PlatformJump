using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//被弹起来 不是主动跳跃 进入的弹跳状态
public class PlayerState_Bounce : PlayerState_Air
{
    public PlayerState_Bounce(string stateName, StateMachine stateMachine, Entity entity) : base(stateName, stateMachine, entity)
    {
    }

    public override void Enter()
    {
        base.Enter();
        rb.velocity=new Vector2(rb.velocity.x,player.bounceSpeed);
    }
    public override void Update()
    {
        base.Update();

        if(rb.velocity.y<0)
        {
            stateMachine.ChangeState(player.FallState);
            return;
        }
    }
    

}
