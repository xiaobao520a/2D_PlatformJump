using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour,I_Poolable
{
    [SerializeField] private float bulletSpeed=5f;
    [SerializeField] private float destroyTime=3f;
    [SerializeField] private int damage=1;

    private Rigidbody2D rb;
    private float timer=0;
    private bool hasHitEnemy;

    public int MaxPoolSize => 20;

    void Awake()
    {
        rb=GetComponent<Rigidbody2D>();
    }


    void Update()
    {
        timer+=Time.deltaTime;
        if(timer>=destroyTime)
        {
            //放入对象池中 来复用
            PoolMgr.Instance.Push("player","Bullet",gameObject);
        }
        
    }

    public void SetVelocity(Vector2 direction)
    {
        rb.velocity=direction*bulletSpeed;
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        if(hasHitEnemy||!collision.CompareTag("Enemy")) return;

        hasHitEnemy=true;
        AudioMgr.Instance.PlaySoundEffect("impactMetal_000",false,true);
        collision.GetComponent<I_Damaged>()?.TakeDamage(damage);
        PoolMgr.Instance.Push("player","Bullet",gameObject);
    }

    public void OnPush()
    {
        timer=0;
        hasHitEnemy=false;
    }

    public void OnPop()
    {
        timer=0;
        hasHitEnemy=false;
    }
}
