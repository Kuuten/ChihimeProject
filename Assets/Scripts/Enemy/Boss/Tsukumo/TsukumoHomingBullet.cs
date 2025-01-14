using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  ツクモのホーミング弾クラス
//
//--------------------------------------------------------------
public class TsukumoHomingBullet : MonoBehaviour
{
    //  外部から弾のベクトルと移動スピードを取得し、移動するだけ

    private Vector3 velocity;
    private float speed;
    private int power;

    private int step;

    private static readonly float radius = 1.0f;
    [SerializeField] Vector3 controlPos;

    //  繰り返す時間(秒)
    private float timer = 3;
    // 回す角度(度)
    float degree;

    // 回転軸
    [SerializeField] private Vector3 axis = Vector3.forward;

    // 円運動周期
    [SerializeField] private float period = 2;
    
    void Awake()
    {
        step = 0;
        speed = 0f;
        power = 0;
        velocity = Vector3.zero;
        controlPos = Vector3.zero;
        degree = 0;
    }

    private void Start()
    {
        //  ランダムなコントロールポイントに飛ぶ
        FlyToRandomControlPoint();
    }

    void Update()
    {
        switch(step)
        {
            case 0:
                CaculateDistance();
                //  座標を更新
                transform.position += speed * velocity * Time.deltaTime;
                break;
            case 1:
                RorateAroundControlPoint();
                break;
            case 2:
                //  座標を更新
                transform.position += speed * velocity * Time.deltaTime;
                break;
        }


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
    //  目標地点との距離を測る
    //------------------------------------------------------
    private void CaculateDistance()
    {
        //  距離を計算する
        float distance = Vector3.Distance( controlPos, transform.position ); 

        //  一定距離になると到達フラグをTrue
        if(distance <= radius)
        {
            //  色変え
            StartCoroutine(ColorAnimation());
            step++;
        }
    }

    //------------------------------------------------------
    //  ランダムなコントロールポイントに飛んでいく
    //------------------------------------------------------
    private void FlyToRandomControlPoint()
    {
        //  0～8番までをランダムに抽出
        int rand = UnityEngine.Random.Range( 3, 9 );
        
        //  ランダムなコントロールポイントの座標を取得
        controlPos = EnemyManager.Instance.GetControlPointPos(rand);

        //  その座標へのベクトルを計算
        Vector3 vec = controlPos - this.transform.position;
        vec.Normalize();

        //  velocityにそのベクトルを設定
        velocity = vec;
    }

    //------------------------------------------------------
    //  コントロールポイントの周りを回転する
    //------------------------------------------------------
    private void RorateAroundControlPoint()
    {
        var tr = transform;
        // 回転のクォータニオン作成
        var angleAxis = Quaternion.AngleAxis(360 / period * Time.deltaTime, axis);

        // 円運動の位置計算
        var pos = tr.position;

        pos -= controlPos;
        pos = angleAxis * pos;
        pos += controlPos;

        tr.position = pos;

        //  カウントダウン
        if( timer <= 0 )
        {
            timer = 0;
            FlyToPlayer();  //  プレイヤーに向かって飛ばす
            step++;
        }
        else
        {
            timer -= Time.deltaTime;
        }
    }

    //------------------------------------------------------
    //  弾の色替えアニメーション
    //------------------------------------------------------
    private IEnumerator ColorAnimation()
    {
        SpriteRenderer fadeSprite = this.GetComponent<SpriteRenderer>();
        fadeSprite.enabled = true;
        var c = fadeSprite.color;
        c.a = 1.0f; // 初期値
        fadeSprite.color = c;

        DOTween.ToAlpha(
        ()=> fadeSprite.color,
        color => fadeSprite.color = color,
        0.0f, // 目標値
        1.5f // 所要時間
        );

        yield return new WaitForSeconds(1.5f);

        //  スプライトを差し替え
        SpriteRenderer s = EnemyManager.Instance.GetHomingBulletSprite();
        this.GetComponent<SpriteRenderer>().sprite = s.sprite;
        this.GetComponent<SpriteRenderer>().color = new Color(1,1,1,0);

        DOTween.ToAlpha(
        ()=> fadeSprite.color,
        color => fadeSprite.color = color,
        1.0f, // 目標値
        1.5f // 所要時間
        );

        yield return new WaitForSeconds(1.5f);
    }

    //------------------------------------------------------
    //  プレイヤーに向かって飛んでいく
    //------------------------------------------------------
    private void FlyToPlayer()
    {
        //  プレイヤーの座標
        Vector3 playerPos = GameManager.Instance.GetPlayer().transform.position;

        //  プレイヤーへのベクトル
        Vector3 vector = playerPos - this.transform.position;
        vector.Normalize();

        //  方向を設定
        velocity = vector;
    }
}
