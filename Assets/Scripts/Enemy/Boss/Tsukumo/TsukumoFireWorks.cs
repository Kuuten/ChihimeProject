using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  敵の弾クラス
//
//--------------------------------------------------------------
public class TsukumoFireworks : MonoBehaviour
{
    //  外部から弾のベクトルと移動スピードを取得し、移動するだけ

    private Vector3 velocity;
    private float speed;
    private float accel;

    private int power;
    
    void Awake()
    {
        speed = 0f;
        power = 0;
        accel = 0.1f;
        velocity = Vector3.zero;
    }

    private void Start()
    {
        Debug.Log($"ベクトルの大きさ:{velocity.magnitude}");

        StartCoroutine(Disapeear(5f, 1f));
    }

    void Update()
    {
        transform.position += speed * velocity * Time.deltaTime;
        speed -= accel * Time.deltaTime;
        accel += 0.01f * Time.deltaTime;
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

    //------------------------------------------------------
    //  当たり判定を無効化してアルファアニメで消す
    //------------------------------------------------------
    private IEnumerator Disapeear(float duration,float fade_time)
    {
        Debug.Log("Ready");

        yield return new WaitForSeconds(duration);

        Debug.Log("Go");

        //  当たり判定を消す
        this.GetComponent<CircleCollider2D>().enabled = false;

        //  アルファアニメーション
        this.GetComponent<SpriteRenderer>().DOFade(0f,fade_time)
            .OnComplete(()=>{ Destroy(this.gameObject);});
    }

}
