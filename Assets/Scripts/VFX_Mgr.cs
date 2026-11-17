using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFX_Mgr : Singleton_WithMono<VFX_Mgr>
{
    [Header("玩家受伤VFX相关")]
    [SerializeField] private float playerDamaged_VFX_Duration=0.2f;
    [SerializeField] private Color playerDamaged_VFX_Color=Color.red;
    private Color playerOriginalColor=Color.white;
    private Coroutine co_PlayerDamaged_VFX=null;

     [Header("敌人受伤VFX相关")]
    [SerializeField] private float enemyDamaged_VFX_Duration=0.2f;
    [SerializeField] private Color enemyDamaged_VFX_Color=Color.white;
    private Color enemyOriginalColor=Color.red;
    private Coroutine co_EnemyDamaged_VFX=null;
    

    void OnEnable()
    {
        EventCenter.Instance.AddListener<Player>(E_EventType.PlayerDamaged,PlayerDamaged_VFX);
        EventCenter.Instance.AddListener<Enemy>(E_EventType.EnemyDamaged,EnemyDamaged_VFX);

    }

    void OnDisable()
    {
        EventCenter.Instance.RemoveListener<Player>(E_EventType.PlayerDamaged,PlayerDamaged_VFX);
        EventCenter.Instance.RemoveListener<Enemy>(E_EventType.EnemyDamaged,EnemyDamaged_VFX);

    }

    private void PlayerDamaged_VFX(Player player)
    {
        if(player==null||co_PlayerDamaged_VFX!=null) return;
        SpriteRenderer sr=player.GetComponentInChildren<SpriteRenderer>();
        co_PlayerDamaged_VFX=StartCoroutine(Co_PlayerDamaged_VFX(sr));
    }

    private IEnumerator Co_PlayerDamaged_VFX(SpriteRenderer sr)
    {
        if(sr==null)
        {
            co_PlayerDamaged_VFX=null;
            yield break;
        }

        sr.color=playerDamaged_VFX_Color;
        yield return new WaitForSeconds(playerDamaged_VFX_Duration);
        if(sr!=null) sr.color=playerOriginalColor;
        co_PlayerDamaged_VFX=null;

    }

    private void EnemyDamaged_VFX(Enemy enemy)
    {
        if(enemy==null||co_EnemyDamaged_VFX!=null) return;
        SpriteRenderer sr=enemy.GetComponentInChildren<SpriteRenderer>();
        co_EnemyDamaged_VFX=StartCoroutine(Co_EnemyDamaged_VFX(sr));
    }

    private IEnumerator Co_EnemyDamaged_VFX(SpriteRenderer sr)
    {
        if(sr==null)
        {
            co_EnemyDamaged_VFX=null;
            yield break;
        }

        sr.color=enemyDamaged_VFX_Color;
        yield return new WaitForSeconds(enemyDamaged_VFX_Duration);
        if(sr!=null) sr.color=enemyOriginalColor;
        co_EnemyDamaged_VFX=null;

    }
}
