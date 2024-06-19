using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HormingBullet : MonoBehaviour
{
    //  弾のスピード
    private Vector3 velocity;
    [SerializeField] GameObject target;
    [SerializeField] float period;   //  目標到達にかかる時間（秒）
    
    void Update()
    {
        Vector3 acceleration = Vector3.zero;
        target = GameObject.Find("Target");

        //  物理の公式
        Vector3 diff = target.transform.position - transform.position;
        acceleration += (diff - velocity*period) * 2f / (period*period);

        period -= Time.deltaTime;
        if(period < 0f)Destroy(this.gameObject);

        velocity += 3 * acceleration * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;
    }

    ////  当たり判定
    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    //  壁に衝突
    //    if(collision.CompareTag("ENEMY"))
    //    {
    //        Destroy(this.gameObject);
    //    }
    //}
}
