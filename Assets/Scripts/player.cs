using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class player : MonoBehaviour
{
    // Playerを方向キー（十字キー）で動かす
    //  ・方向キーの入力を受け取る
    //  ・Playerの位置を変更する

    //  弾を撃つ
    //  ・弾を作る
    //  ・弾の動きを作る
    //  ・発射ポイントを作る

    //  敵の移動
    //  敵を生成
    //  敵に弾が当たったら爆発する
    //  敵とプレイヤーがぶつかったら爆発する

    //  移動スピード
    [SerializeField] private float moveSpeed = 3.0f;

    //  弾の発射点
    [SerializeField]private Transform firePoint;
    //  弾のプレハブ
    [SerializeField]private GameObject bulletPrefab;
    //  ショット間隔のカウント用
    private int shotCount = 0;
    //  弾を何フレーム毎に撃てるか
    private const int shotInterval = 10;

    void Start()
    {
        shotCount = 0;
    }

    void Update()
    {
        Move();

        Shot();
    }

    //-------------------------------------------
    //  移動処理
    //-------------------------------------------
    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        Vector3 moveVector = new Vector3(x, y, 0);
        moveVector.Normalize();
        transform.position += moveVector * moveSpeed * Time.deltaTime;
    }

    //-------------------------------------------
    //  移動処理
    //-------------------------------------------
    void Shot()
    {
        if( Input.GetKey(KeyCode.Z))    //  Zキー押下
        {
            shotCount++;
            if(shotCount % shotInterval == 0)
            {
                Instantiate( bulletPrefab, firePoint.position, bulletPrefab.transform.rotation);
            }
        }
    }
}
