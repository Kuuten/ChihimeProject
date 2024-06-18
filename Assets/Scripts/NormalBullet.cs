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

    // 下に動く
    void Update()
    {
        transform.position += -velocity * Time.deltaTime;
    }

    //  当たり判定
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //  壁に衝突
        if(collision.collider.CompareTag("WALL"))
        {
            Destroy(this.gameObject);
        }
    }
}
