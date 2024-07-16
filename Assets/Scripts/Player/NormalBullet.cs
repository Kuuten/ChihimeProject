using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

//---------------------------------------------------
//
//  通常弾の挙動クラス
//
//---------------------------------------------------
public class NormalBullet : MonoBehaviour
{
    //  弾のスピード
    private Vector3 velocity = new Vector3(0,-20,0);
    //  弾の寿命
    [SerializeField] private float lifetime = 3;
    //  弾の向き
    private Quaternion rotation = Quaternion.identity;

    private void Start()
    {

    }

    //  最初から画面内にいれば画面外に行った時に呼ばれる
    void OnBecameInvisible() {
        Destroy (this.gameObject);
    }

    // 下に動く
    void Update()
    {
        transform.position += velocity * Time.deltaTime;

        //  消えなかった時の為の保険
        lifetime -= Time.deltaTime;
        if (lifetime <= 0f) Destroy(this.gameObject);
    }

    //-----------------------------------------------------
    //  プロパティ
    //-----------------------------------------------------
    public Vector3 GetVelocity(){ return velocity; }
    public void SetVelocity(Vector3 v){ velocity = v; }
    public Quaternion GetRotation(){ return transform.rotation; }
    public void SetRotation(Quaternion q){ transform.rotation = q; }
}
