using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : BaseState
{
    protected Player player;    
    public PlayerState(string stateName, StateMachine stateMachine, Entity entity) : base(stateName, stateMachine, entity)
    {
        player = entity as Player;
    }
}
