using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//坠落平台
public class FallingPlatform : MonoBehaviour
{
    [SerializeField] private float delayTime=1f; //碰到之后等一秒下落
    [SerializeField] private float destroyTime=2f; //下落之后 2s删除自己
    private Rigidbody2D rb;
    private Coroutine co_Fall=null; //下落相关的协程

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        rb=GetComponentInChildren<Rigidbody2D>();
    }
    //如果是Player碰到了 就开启下落
    void OnCollisionEnter2D(Collision2D other)
    {
        if(co_Fall!=null) return;

        if(other.transform.CompareTag("Player")&&other.gameObject.GetComponent<Player>()?.isGrounded==true)
            co_Fall=StartCoroutine(Co_Fall());
    }

    private IEnumerator Co_Fall()
    {
        yield return new WaitForSeconds(delayTime);
        rb.bodyType=RigidbodyType2D.Dynamic;
        Destroy(this.transform.parent.gameObject,destroyTime);
    }
}
