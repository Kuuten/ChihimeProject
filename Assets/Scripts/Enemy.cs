using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  敵の基本クラス
//
//--------------------------------------------------------------
public class Enemy : MonoBehaviour
{

    //  敵を左右に揺らす
    //  スコアの表示
    //  敵を倒した時にスコアを表示させる
    //  リスタートの実装

    [SerializeField] float moveSpeed = 1.0f;
    [SerializeField] GameObject explosion;

    void Start()
    {
        
    }

    void Update()
    {
        transform.position += new Vector3(0,moveSpeed * Time.deltaTime,0);
    }

    //  敵に当たったら爆発する
    //  当たり判定の基礎知識：
    //  当たり判定を行うには、
    //  ・両者にColliderがついている
    //  ・どちらかにRigidBodyがついている
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Instantiate(explosion, transform.position, transform.rotation);
        Destroy(this.gameObject);
        Destroy(collision.gameObject);
    }
}
