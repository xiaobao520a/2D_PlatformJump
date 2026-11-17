using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Transform pointStart;
    [SerializeField] private Transform pointEnd;
    [SerializeField] private float moveSpeed=3f;
    [SerializeField] private float waitTime=1f;

    private Vector3 targetPosition;
    private bool isWaiting=false;
    private bool movingToEnd=true;
    private float waitTimer=0f;

    //站在这块平台上的Rigidbody2D 只记录乘客 不修改父子关系
    private readonly HashSet<Rigidbody2D> passengers=new HashSet<Rigidbody2D>();

    void Awake()
    {
        targetPosition=pointEnd.position;
    }

    void FixedUpdate()
    {
        //到达一端后原地等待 再切换目标点
        if(isWaiting)
        {
            waitTimer+=Time.fixedDeltaTime;
            if(waitTimer<waitTime) return;

            waitTimer=0f;
            isWaiting=false;
            targetPosition=movingToEnd?pointStart.position:pointEnd.position;
            movingToEnd=!movingToEnd;
            return;
        }

        Vector3 previousPosition=transform.position;
        transform.position=Vector3.MoveTowards(
            previousPosition,
            targetPosition,
            moveSpeed*Time.fixedDeltaTime);

        Vector3 delta=transform.position-previousPosition;
        if(delta.sqrMagnitude>0f)
            MovePassengers(delta);

        if((transform.position-targetPosition).sqrMagnitude<=0.000001f)
        {
            transform.position=targetPosition;
            isWaiting=true;
            waitTimer=0f;
        }
    }

    private void MovePassengers(Vector3 delta)
    {
        passengers.RemoveWhere(rb => rb==null);

        foreach(Rigidbody2D passenger in passengers)
            passenger.position+=(Vector2)delta;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(!collision.collider.CompareTag("Player")) return;

        Rigidbody2D passenger=collision.collider.attachedRigidbody;
        if(passenger!=null)
            passengers.Add(passenger);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if(!collision.collider.CompareTag("Player")) return;

        Rigidbody2D passenger=collision.collider.attachedRigidbody;
        if(passenger!=null)
            passengers.Remove(passenger);
    }

    void OnDisable()
    {
        passengers.Clear();
    }
}
