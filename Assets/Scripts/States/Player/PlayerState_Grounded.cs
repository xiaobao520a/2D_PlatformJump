using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class PlayerState_Grounded : PlayerState
{
    public PlayerState_Grounded(string stateName, StateMachine stateMachine, Entity entity) : base(stateName, stateMachine, entity)
    {
        
    }

    public override void Update()
    {
        base.Update();

        
        //如果在地面上 还可以跳跃并且按下了跳跃键 就跳起来
        if(player.isGrounded&&player.playerControls.Player.Jump.WasPressedThisFrame()
        &&player.jumpCount<player.maxJumpCount)
        {
            stateMachine.ChangeState(player.JumpState);
            return;
        }

        //如果此时在空中下落 进入Fallstate
        if(!player.isGrounded&&rb.velocity.y<0)
        {
            stateMachine.ChangeState(player.FallState);
            return;
        }

        

        
    }

}
