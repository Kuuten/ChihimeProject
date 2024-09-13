using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  敵の弾クラス
//
//--------------------------------------------------------------
public class EnemyBullet : MonoBehaviour
{
    //  外部から弾のベクトルと移動スピードを取得し、移動するだけ

    private Vector3 velocity;
    private float speed;

    private int power;
    
    void Awake()
    {
        speed = 0f;
        power = 0;
        velocity = Vector3.zero;
    }

    void Update()
    {
        transform.position += speed * velocity * Time.deltaTime;
    }

    //-------------------------------------------------
    //  当たり判定
    //-------------------------------------------------
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("DeadWall") ||
            collision.CompareTag("DeadWallBottom"))
        {
            Destroy(this.gameObject);
        }
    }


    //------------------------------------------------------
    //  プロパティ
    //------------------------------------------------------
    public void SetVelocity(Vector3 v){ velocity = v; }
    public void SetSpeed(float s){ speed = s; }
    public void SetPower(int p){ power = p; }
    public int GetPower(){ return power; }
}
