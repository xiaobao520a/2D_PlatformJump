using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHitBox : MonoBehaviour
{
    private Enemy enemy;
    void Awake()
    {
        enemy=GetComponentInParent<Enemy>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(!other.gameObject.CompareTag("Player")) return;
        
        other.gameObject.GetComponent<I_Damaged>()?.TakeDamage(enemy.damage);
    }
    
}
