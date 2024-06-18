using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HormingBullet : MonoBehaviour
{
    //  弾のスピード
    private Vector3 velocity;
    private float lifeTime = 0;
    private const float lifeMax = 60*3;
    [SerializeField] GameObject target;
    [SerializeField] float period;   //  目標到達にかかる時間（秒）
    
    void Update()
    {
        //  時間経過で死亡も入れとく
        if(lifeTime >= lifeMax)
        {
            lifeTime = 0;
            Destroy(this.gameObject);
        }
        else lifeTime += Time.deltaTime;

        Vector3 acceleration = Vector3.zero;
        target = GameObject.Find("Target");

        //  物理の公式
        Vector3 diff = target.transform.position - transform.position;
        acceleration += (diff - velocity*period) * 2f / (period*period);

        period -= Time.deltaTime;
        if(period < 0f)return;

        velocity += acceleration * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;
    }

    //  当たり判定
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //  壁に衝突
        if(collision.collider.CompareTag("WALL") ||
           collision.collider.CompareTag("ENEMY")
          )
        {
            Destroy(this.gameObject);
        }
    }
}
