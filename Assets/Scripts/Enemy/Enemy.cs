using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy :Entity
{
    public EnemyState_Chase ChaseState{get;private set;}
    public EnemyState_Jump JumpState{get;private set;}
    public Player player; //为了追逐Player得拿到Player的引用

    //伤害
    public int damage=1;

    [Header("移动跳跃相关")]
    public float jumpForce=5f; //跳跃力

    public float transitionTime=1f; //跳跃之后回到追逐再回到跳跃需要等1f

    public float speed=2f;

    [Header("检测相关")]
    [SerializeField]private LayerMask groundLayer; //地面检测的图层
    public bool isGrounded; //是否在地面上
    [SerializeField] private float groundDetectDistance=1f;
    [SerializeField] private float aboveDistance=3f; //向上检测的距离
    [SerializeField] private float jumpHorizontalRange=3f; //玩家在这个水平距离内才考虑跳
    [SerializeField] private float samePlatformHeightTolerance=0.35f; //地面高度差在这个范围内算同一平台

    [Header("战利品相关")]
    [SerializeField] private List<Loot>lootLists;
    [SerializeField] private float maxDropChance=100f; //最大掉落几率 0-100


    void Start()
    {
        GameObject playerObj=GameObject.FindGameObjectWithTag("Player");
        if(playerObj!=null)
            player=playerObj.GetComponent<Player>();

        ChaseState=new EnemyState_Chase("Chase",stateMachine,this);
        JumpState=new EnemyState_Jump("Jump",stateMachine,this);

        stateMachine.Init(ChaseState);
    }

    protected override void Update()
    {
        if(Time.timeScale==0f) return;

        isGrounded=Physics2D.Raycast(transform.position,Vector2.down,groundDetectDistance,groundLayer);
        base.Update();
    }

    public void FaceDirection(int direction)
    {
        if(direction==0||direction==flipDirection) return;

        flipDirection=direction>0?1:-1;
        SetFlip();
    }

    public bool HasGroundAhead(int direction)
    {
        if(boxCollider==null) return true;

        Bounds bounds=boxCollider.bounds;
        Vector2 origin=new Vector2(
            bounds.center.x+direction*(bounds.extents.x+0.06f),
            bounds.min.y+0.03f);

        return Physics2D.Raycast(origin,Vector2.down,groundDetectDistance,groundLayer);
    }

    public bool ShouldJump(int direction,bool hasGroundAhead)
    {
        if(!isGrounded||player==null||player.boxCollider==null) return false;

        float dx=player.transform.position.x-transform.position.x;
        float dy=player.transform.position.y-transform.position.y;

        if(Mathf.Abs(dx)<0.05f||Mathf.Abs(dx)>jumpHorizontalRange) return false;

        float gravity=Mathf.Abs(Physics2D.gravity.y*rb.gravityScale);
        float maxJumpHeight=gravity>0f?jumpForce*jumpForce/(2f*gravity):aboveDistance;

        //玩家明显在上方 且高度在敌人一次跳跃能到达的范围内
        bool playerIsAbove=dy>0.6f&&dy<=Mathf.Min(aboveDistance,maxJumpHeight*0.85f);

        //同一平台上的玩家不触发向上跳
        if(playerIsAbove&&IsPlayerOnSamePlatform()) return false;

        //前方是坑时 玩家在同一高度附近也可以跳过去追
        bool jumpOverGap=!hasGroundAhead&&dy>-0.5f;
        return playerIsAbove||jumpOverGap;
    }

    private bool IsPlayerOnSamePlatform()
    {
        RaycastHit2D enemyGround=GetGroundHit(boxCollider);
        RaycastHit2D playerGround=GetGroundHit(player.boxCollider);

        if(enemyGround.collider==null||playerGround.collider==null) return false;
        return Mathf.Abs(enemyGround.point.y-playerGround.point.y)<=samePlatformHeightTolerance;
    }

    private RaycastHit2D GetGroundHit(BoxCollider2D targetCollider)
    {
        if(targetCollider==null) return default;

        Bounds bounds=targetCollider.bounds;
        Vector2 origin=new Vector2(bounds.center.x,bounds.min.y+0.03f);
        float distance=Mathf.Max(groundDetectDistance+1f,aboveDistance+2f);
        return Physics2D.Raycast(origin,Vector2.down,distance,groundLayer);
    }

    public override void TakeDamage(int damage)
    {
        AudioMgr.Instance.PlaySoundEffect("impactMetal_001",false,true);
        nowHp-=damage;
        if(nowHp<=0)
        {
            Die();
            return;
        }
        
        EventCenter.Instance.Broadcast<Enemy>(E_EventType.EnemyDamaged,this);
    }

    public override void Die()
    {
        AudioMgr.Instance.PlaySoundEffect("explosionCrunch_000",false,true);

        //掉落战利品 只掉一个 也可能什么都没掉
        foreach(var loot in lootLists)
        {
            if(Random.Range(0,maxDropChance)<loot.dropChance)
            {
                Instantiate(loot.lootObj,transform.position,Quaternion.identity);
                break;
            }
        }
        Destroy(gameObject);

    }

    void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position,transform.position+new Vector3(0,-groundDetectDistance,0));
    }


}
