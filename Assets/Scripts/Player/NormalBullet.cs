using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//---------------------------------------------------
//
//  通常弾の挙動クラス
//
//---------------------------------------------------
public class NormalBullet : MonoBehaviour
{
    //  弾のスピード
    private Vector3 velocity = new Vector3(0,20,0);
    //  弾の寿命
    [SerializeField] private float lifetime = 3;

    // 下に動く
    void Update()
    {
        transform.position += -velocity * Time.deltaTime;

        lifetime -= Time.deltaTime;
        if(lifetime <= 0f)Destroy(this.gameObject);
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
