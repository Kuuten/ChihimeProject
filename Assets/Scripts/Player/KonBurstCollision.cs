using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  プレイヤーの魂バーストの弾消し当たり判定クラス
//
//--------------------------------------------------------------
public class KonBurstCollision : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    //-------------------------------------------------------
    //  敵の弾との当たり判定
    //-------------------------------------------------------
    private void OnTriggerEnter2D(Collider2D collision)
    {   
        if(collision.CompareTag("EnemyBullet"))
        {
            //  敵弾と当たったら敵弾を消す
            Destroy(collision.gameObject);
        }
    }
}
