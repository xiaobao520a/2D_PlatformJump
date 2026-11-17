using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class Player : Entity,I_Healed
{
    //角色的InputSystem 输入控制相关
    public PlayerControls playerControls;

    //各种状态
    public PlayerState_Idle IdleState{ get; private set; }
    public PlayerState_Move MoveState{ get; private set; }
    public PlayerState_Jump JumpState{ get; private set; }
    public PlayerState_Fall FallState{ get; private set; }
    public PlayerState_Bounce BounceState{get;private set;}
    public PlayerState_Dash DashState{get;private set;}
    public PlayerState_WallSlide WallSlideState{get;private set;}
    public PlayerState_WallJump WallJumpState{get;private set;}

    [Header("移动相关")]
    public Vector2 moveInput; //移动输入
    public float moveSpeed = 5f; //移动速度

    [Header("跳跃相关")]
    public float jumpSpeed = 10f; //跳跃力度
    public float maxJumpSpeed=18f;
    public int maxJumpCount = 2; //最大跳跃次数 支持二段跳
    public int jumpCount = 0; //当前跳跃次数
    public float inAirSpeedMultiplier=0.8f; //在空中水平速度慢一点
    public float bounceSpeed; //踩到trap的弹跳力度 由具体陷阱决定

    [Header("冲刺相关")]
    public float dashSpeed=10f; //冲刺速度
    public float dashDuration=0.5f; //持续时间
    public float dashCoolDownTime=1f; //冷却时间
    private float dashTimer=0f; //计时器
    public bool canDash=true; //能否dash

    [Header("墙壁相关")]
    [SerializeField] private float wallCheckDistance=0.08f;
    [SerializeField] private LayerMask wallLayer;
    public float wallSlideSpeed=2f;
    public float wallJumpForceX=8f;
    public float wallJumpForceY=10f;
    public float wallJumpControlLockTime=0.12f;
    public bool isTouchingWall{get;private set;}
    public int wallDirection{get;private set;}


    [Header("重力相关")]
    public float fallGravityMultiplier =2f; //下落的时候的重力增长倍数  下落的更快

    [Header("检测相关")]
    [SerializeField] private Transform groundCheck; //地面检测点
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.5f, 0.1f); //地面检测的大小
    public LayerMask groundLayer; //地面检测的图层
    public bool isGrounded; //是否在地面上

    [Header("粒子特效")]
    public ParticleSystem smoke_VFX;

    [Header("长按进入下一关相关")]
    [SerializeField]private float holdTime=1f; //长按时间
    [SerializeField]private Image img_HoldCircle; //填充的圆形
    private float nowHoldTime=0f; //目前按住的时间
    private bool gameOver=false; //游戏结束
    private bool isDead=false; //防止死亡事件重复广播

    [Header("子弹相关")]
    [SerializeField] private GameObject bulletObj;
    private bool bulletPoolReady;

    [Header("下平台相关")]
    [SerializeField] private float dropSpeed=2f;
    [SerializeField] private float dropIgnoreTime=0.35f;

    [Header("结束游戏相关")]
    [SerializeField] private Transform endTrans;

    protected override void Awake()
    {
        base.Awake();
        playerControls=new PlayerControls();

        //初始化各种状态
        IdleState = new PlayerState_Idle("Idle", stateMachine, this);
        MoveState = new PlayerState_Move("Move", stateMachine, this);
        JumpState = new PlayerState_Jump("Jump", stateMachine, this);
        FallState = new PlayerState_Fall("Fall", stateMachine, this);
        BounceState=new PlayerState_Bounce("Bounce",stateMachine,this);
        DashState=new PlayerState_Dash("Dash",stateMachine,this);
        WallSlideState=new PlayerState_WallSlide("WallSlide",stateMachine,this);
        WallJumpState=new PlayerState_WallJump("WallJump",stateMachine,this);

        //没有手动指定墙层时 自动查找名为Wall的Layer
        if(wallLayer.value==0)
        {
            int wallLayerIndex=LayerMask.NameToLayer("Wall");
            if(wallLayerIndex>=0)
                wallLayer=1<<wallLayerIndex;
        }

        //重置长按相关 
        ResetHold();
    }

    void OnEnable()
    {
        //启用输入控制
        playerControls.Enable();
    }

    void Start()
    {
        //初始化一开始的状态
        stateMachine.Init(IdleState);

        //预加载子弹 对象池优化 避免频繁GC卡顿游戏
        PoolMgr.Instance.Preload("player", "Bullet", ()=>bulletPoolReady=true);
    }

    protected override void Update()
    {
        if(gameOver||Time.timeScale==0f) return; 

        //检测是否掉下去死亡
        if(transform.position.y<=endTrans.transform.position.y)
        {
            Die();
            return;
        }
        //获取移动输入
        moveInput = playerControls.Player.Move.ReadValue<Vector2>();

        //地面检测
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
        DetectWall();

        //dash的冷却时间
        if(!canDash)
        {
            dashTimer+=Time.deltaTime;
            if(dashTimer>=dashCoolDownTime)
            {
                canDash=true;
                dashTimer=0;
            }
        }

        //冲刺输入统一在这里处理 避免切到Dash后又被当前状态的逻辑覆盖
        if(TryDash())
        {
            base.Update();
            return;
        }

        if(TryWallSlide())
        {
            base.Update();
            return;
        }

        //如果按下了攻击键 就发射子弹
        Fire();

        //如果按两下S键 并且在地面上 就可以掉到下面去
        if(playerControls.Player.Drop.WasPerformedThisFrame()&&isGrounded)
        {
            StartCoroutine(Co_Drop());
        }
        //获取完各种各样的输入之后 再进入状态更新
        base.Update();

        //如果当前关卡已经完成 就可以长按E去进入下一关
        if(GameMgr.Instance.finishLevel)
        {
            //处理具体长按逻辑
            HoldToNextLevel();
        }

    }


    void OnDisable()
    {
        //禁用输入控制
        playerControls.Disable();
    }

    void OnDestroy()
    {
        //销毁输入控制
        playerControls.Dispose();
    }

    //拾取可收集物体的方法
    public void OnTriggerEnter2D(Collider2D collision)
    {
        collision.GetComponent<I_Colleted>()?.Collect(this);
    }

    //开火的方法 用对象池优化
    public void Fire()
    {
        //如果按下了攻击键 就发射子弹 对象池优化版
        if(!playerControls.Player.Attack.WasPressedThisFrame()) return;
        if(!bulletPoolReady) return;

        GameObject obj=PoolMgr.Instance.Pop("player","Bullet");
        if(obj==null) return;

        obj.transform.position=transform.position;
        obj.transform.rotation=quaternion.identity;

        //设置发射方向和速度
        Vector2 direction=((Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition)-(Vector2)transform.position).normalized;
        Bullet bullet=obj.GetComponent<Bullet>();
        if(bullet==null)
        {
            PoolMgr.Instance.Push("player","Bullet",obj);
            return;
        }

        bullet.SetVelocity(direction);
        AudioMgr.Instance.PlaySoundEffect("laserSmall_000",false,true);
    }

    //重置长按进入下一关相关字段
    private void ResetHold()
    {
        nowHoldTime=0f;
        img_HoldCircle.fillAmount=0f;
    }

    //长按进入下一关的具体逻辑
    private void HoldToNextLevel()
    {
        //完成当前关卡
        if(nowHoldTime>=holdTime)
        {
            gameOver=true;
            GameMgr.Instance.level++;
            EventCenter.Instance.Broadcast(E_EventType.LevelFinisned);
            return;
        }

        //如果E键正在被按下
        if(playerControls.Player.Interact.IsPressed())
        {
            nowHoldTime+=Time.deltaTime;
            img_HoldCircle.fillAmount=nowHoldTime/holdTime;
        }
        //松开就重置
        else
        {
            ResetHold();
        }

    }

    //受伤 调用特效 更新UI
    public override void TakeDamage(int damage)
    {
        if(isDead) return;

        //冲刺状态的时候无敌
        if(stateMachine.CurrentState==DashState) return;

        AudioMgr.Instance.PlaySoundEffect("impactMetal_004",false,true);
        nowHp-=damage;
        if (nowHp <= 0)
        {
            nowHp=0;
            EventCenter.Instance.Broadcast<int>(E_EventType.UpdateHpUI,nowHp);
            Die();
            return;
        }

        EventCenter.Instance.Broadcast<int>(E_EventType.UpdateHpUI,nowHp);
        EventCenter.Instance.Broadcast<Player>(E_EventType.PlayerDamaged,this);
    }

    public override void Die()
    {
        if(isDead) return;

        isDead=true;
        AudioMgr.Instance.PlaySoundEffect("lowFrequency_explosion_000",false,true);

        //禁用Player的输入
        playerControls.Disable();
        EventCenter.Instance.Broadcast(E_EventType.PlayerDie);
    }

    //画出检测地面射线
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
    }

    public void Healed(int value)
    {
        nowHp+=value;
        if(nowHp>=maxHp) nowHp=maxHp;

        EventCenter.Instance.Broadcast<int>(E_EventType.UpdateHpUI,nowHp);
    }

    private IEnumerator Co_Drop()
    {
        if(boxCollider!=null) boxCollider.enabled=false;
        isGrounded=false;
        stateMachine.ChangeState(FallState);
        rb.velocity=new Vector2(rb.velocity.x,-dropSpeed);
        yield return new WaitForSeconds(dropIgnoreTime);
        if(boxCollider!=null) boxCollider.enabled=true;
    }

    public void SetBounceSpeed(float speed)=>bounceSpeed=speed;

    public void SetFacing(int direction)
    {
        if(direction==0||direction==flipDirection) return;

        flipDirection=direction>0?1:-1;
        SetFlip();
    }

    private void DetectWall()
    {
        isTouchingWall=false;
        wallDirection=0;

        if(boxCollider==null) return;

        Vector2 origin=boxCollider.bounds.center;
        float distance=boxCollider.bounds.extents.x+wallCheckDistance;

        RaycastHit2D rightHit=Physics2D.Raycast(origin,Vector2.right,distance,wallLayer);
        RaycastHit2D leftHit=Physics2D.Raycast(origin,Vector2.left,distance,wallLayer);

        if(rightHit.collider!=null&&leftHit.collider!=null)
            wallDirection=moveInput.x>0f?1:-1;
        else if(rightHit.collider!=null)
            wallDirection=1;
        else if(leftHit.collider!=null)
            wallDirection=-1;

        isTouchingWall=wallDirection!=0;
    }

    private bool TryWallSlide()
    {
        if(isGrounded||!isTouchingWall||rb.velocity.y>0f) return false;
        if(moveInput.x*wallDirection<0f) return false;
        if(stateMachine.CurrentState==WallSlideState||
           stateMachine.CurrentState==WallJumpState||
           stateMachine.CurrentState==DashState) return false;

        stateMachine.ChangeState(WallSlideState);
        return stateMachine.CurrentState==WallSlideState;
    }

    private bool TryDash()
    {
        if(!canDash||stateMachine.CurrentState==DashState||stateMachine.CurrentState==WallSlideState) return false;
        if(!playerControls.Player.Dash.WasPressedThisFrame()) return false;

        stateMachine.ChangeState(DashState);
        return stateMachine.CurrentState==DashState;
    }
}
