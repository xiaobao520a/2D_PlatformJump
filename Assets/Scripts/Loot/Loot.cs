using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//战利品
[Serializable]
public class Loot
{
    public GameObject lootObj; //具体的战利品 gem bigGem healthItem....
    [Range(0,100)] public float dropChance; //掉落几率
}
