using UnityEngine;

//蹬墙跳状态
public class PlayerState_WallJump : PlayerState_Air
{
    public PlayerState_WallJump(string stateName,StateMachine stateMachine,Entity entity)
        : base(stateName,stateMachine,entity)
    {
    }

    public override void Enter()
    {
        base.Enter();

        int jumpDirection=-player.wallDirection;
        player.SetFacing(jumpDirection);
        player.jumpCount=1;
        timer=0f;

        rb.velocity=new Vector2(
            jumpDirection*player.wallJumpForceX,
            player.wallJumpForceY);
    }

    public override void Update()
    {
        timer+=Time.deltaTime;

        //短暂保持反向蹬墙速度 之后恢复正常空中控制
        if(timer>=player.wallJumpControlLockTime)
            base.Update();

        if(player.isGrounded)
        {
            player.jumpCount=0;
            stateMachine.ChangeState(player.moveInput.x==0f?player.IdleState:player.MoveState);
            return;
        }

        if(rb.velocity.y<0f)
            stateMachine.ChangeState(player.FallState);
    }
}
