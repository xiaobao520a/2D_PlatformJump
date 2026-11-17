using UnityEngine;

//贴墙下滑状态
public class PlayerState_WallSlide : PlayerState
{
    public PlayerState_WallSlide(string stateName,StateMachine stateMachine,Entity entity)
        : base(stateName,stateMachine,entity)
    {
    }

    public override void Enter()
    {
        base.Enter();

        player.SetFacing(player.wallDirection);
        rb.velocity=new Vector2(0,-player.wallSlideSpeed);
    }

    public override void Update()
    {
        if(player.isGrounded)
        {
            player.jumpCount=0;
            stateMachine.ChangeState(player.moveInput.x==0f?player.IdleState:player.MoveState);
            return;
        }

        //主动往离墙方向按 也会自然脱离墙壁
        if(!player.isTouchingWall||player.moveInput.x*player.wallDirection<0f)
        {
            stateMachine.ChangeState(player.FallState);
            return;
        }

        if(player.playerControls.Player.Jump.WasPressedThisFrame())
        {
            stateMachine.ChangeState(player.WallJumpState);
            return;
        }

        rb.velocity=new Vector2(0,-player.wallSlideSpeed);
    }
}
