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
    private Vector3 velocity;
    //  弾の寿命
    [SerializeField] private float lifetime = 3;

    private void Awake()
    {
        velocity = new Vector3(0,-20,0);
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

    // 上下に動く
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
