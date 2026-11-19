using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//抽象的实体类 Player和Enemy继承
public abstract class Entity : MonoBehaviour, I_Damaged
{
    public StateMachine stateMachine; //状态机
    protected Animator animator; //animator组件
    public Rigidbody2D rb; //rigidBody2D组件
    protected SpriteRenderer sr; //spriteRenderer组件
    public BoxCollider2D boxCollider; //boxCollider2D组件

    public int maxHp = 5; //最大生命值
    public int nowHp = 5; //当前生命值
    public int flipDirection = 1; //1表示朝右 -1表示朝左

    protected virtual void Awake()
    {
        stateMachine = new StateMachine();
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        boxCollider=GetComponent<BoxCollider2D>();

        flipDirection = sr.flipX ? -1 : 1;
    }

    public virtual void TakeDamage(int damage)
    {
    }

    public virtual void Die()
    {
    }

    public void SetFlip()
    {
        sr.flipX = !sr.flipX;
    }

    protected virtual void Update()
    {
        stateMachine.Update();
    }
}
