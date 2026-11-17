using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyState_Chase : EnemyState
{
    public EnemyState_Chase(string stateName, StateMachine stateMachine, Entity entity) : base(stateName, stateMachine, entity)
    {
    }

    public override void Enter()
    {
        base.Enter();

        rb.velocity=new Vector2(0,rb.velocity.y);

    }
    public override void Update()
    {
        base.Update();

        timer+=Time.deltaTime;
        if(player==null)
        {
            rb.velocity=new Vector2(0,rb.velocity.y);
            return;
        }

        float dx=player.transform.position.x-enemy.transform.position.x;
        if(Mathf.Abs(dx)<0.05f)
        {
            rb.velocity=new Vector2(0,rb.velocity.y);
            return;
        }

        int direction=dx>0f?1:-1;
        enemy.FaceDirection(direction);

        bool hasGroundAhead=enemy.HasGroundAhead(direction);

        //只有确实该跳时才跳 否则在平台边缘停住
        if(timer>=enemy.transitionTime&&enemy.ShouldJump(direction,hasGroundAhead))
        {
            stateMachine.ChangeState(enemy.JumpState);
            return;
        }

        if(!hasGroundAhead)
        {
            rb.velocity=new Vector2(0,rb.velocity.y);
            return;
        }

        rb.velocity=new Vector2(direction*enemy.speed,rb.velocity.y);
    }
}
