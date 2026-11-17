using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player_StartScene : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer sr;

    [SerializeField] private float maxSpeed;
    [SerializeField] private float minDistance;

    void Awake()
    {
        animator=GetComponentInChildren<Animator>();
        sr=GetComponentInChildren<SpriteRenderer>();
    }

    void Start()
    {
        animator.SetBool("Idle",true);
        animator.SetBool("Move",false);
    }
    

    // Update is called once per frame
    void Update()
    {
        Vector2 screenMousePosition=Mouse.current.position.ReadValue();
        Vector3 mousePosition=Camera.main.ScreenToWorldPoint(screenMousePosition);
        mousePosition.y=transform.position.y;
        mousePosition.z=transform.position.z;
        float direction=Mathf.Sign(mousePosition.x-transform.position.x);
        if(direction>0)
        {
            sr.flipX=false;
        }
        else if(direction<0)
        {
            sr.flipX=true;
        }
        float distance=Mathf.Abs(mousePosition.x-transform.position.x);
        if(distance<=minDistance)
        {
            animator.SetBool("Move",false);
            animator.SetBool("Idle",true);
            return;
        }
        else
        {
            animator.SetBool("Idle",false);
            animator.SetBool("Move",true);
            transform.position=Vector3.MoveTowards(transform.position,mousePosition,Mathf.Min(distance*Time.deltaTime,maxSpeed*Time.deltaTime));
        }


    }
}
