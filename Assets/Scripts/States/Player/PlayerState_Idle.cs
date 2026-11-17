using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_Idle : PlayerState_Grounded
{
    public PlayerState_Idle(string stateName, StateMachine stateMachine, Entity entity) : base(stateName, stateMachine, entity)
    {
    }

    public override void Enter()
    {
        base.Enter();
        rb.velocity = Vector2.zero;
        player.jumpCount=0;
    }

    public override void Update()
    {
        base.Update();
        if (player.moveInput.x!= 0f&&player.isGrounded)
        {
            stateMachine.ChangeState(player.MoveState);
            return;
        }
    }
}
