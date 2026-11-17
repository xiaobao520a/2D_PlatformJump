using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyState_Jump : EnemyState
{
    private int jumpDirection=1;

    public EnemyState_Jump(string stateName, StateMachine stateMachine, Entity entity) : base(stateName, stateMachine, entity)
    {
    }

    public override void Enter()
    {
        base.Enter();

        float dx=player!=null?player.transform.position.x-enemy.transform.position.x:enemy.flipDirection;
        jumpDirection=Mathf.Abs(dx)<0.05f?enemy.flipDirection:(dx>0f?1:-1);

        enemy.FaceDirection(jumpDirection);
        rb.velocity=new Vector2(jumpDirection*enemy.speed,enemy.jumpForce);
    }

    public override void Update()
    {
        base.Update();

        //确认已经开始下落并落地后再回追逐 防止刚起跳就立刻切回追逐
        if(enemy.isGrounded&&rb.velocity.y<=0.1f)
        {
            stateMachine.ChangeState(enemy.ChaseState);
            return;
        }
    }
}
