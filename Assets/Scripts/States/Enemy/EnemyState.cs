using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyState : BaseState
{
    protected Enemy enemy;
    protected Player player;
    public EnemyState(string stateName, StateMachine stateMachine, Entity entity) : base(stateName, stateMachine, entity)
    {
        enemy=(Enemy)entity;
        player=enemy.player;
    }
}
