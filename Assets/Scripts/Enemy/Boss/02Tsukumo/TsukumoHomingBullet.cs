using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static EnemyManager;

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

    // 回転軸
    [SerializeField] private Vector3 axis = Vector3.forward;

    // 円運動周期
    [SerializeField] private float period = 2;

    //  目標番号抽出用
    private int targetNum;
    
    void Awake()
    {
        step = 0;
        speed = 0f;
        power = 0;
        velocity = Vector3.zero;
        controlPos = Vector3.zero;
        targetNum = 0;
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
            case 0: //  コントロールポイントへ近づく
                CaculateDistance();
                //  座標を更新
                transform.position += speed * velocity * Time.deltaTime;
                break;
            case 1: //  回転しながらオブジェクトが変わる
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
    public void SetTargetNum(int n){ targetNum = n; }
    public int GetTargetNum(){ return targetNum; }

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
            StartCoroutine(FadeoutBullet(1.5f));
            step++;
        }
    }

    //------------------------------------------------------
    //  ランダムなコントロールポイントに飛んでいく
    //------------------------------------------------------
    private void FlyToRandomControlPoint()
    {
        //  コントロールポイントの座標を取得
        controlPos = EnemyManager.Instance.GetControlPointPos(targetNum);

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
        Transform tr = this.transform;

        // 回転のクォータニオン作成
        var angleAxis = Quaternion.AngleAxis(360 / period * Time.deltaTime, axis);

        // 円運動の位置計算
        var pos = tr.position;

        pos -= controlPos;
        pos = angleAxis * pos;
        pos += controlPos;

        tr.position = pos;
    }

    //------------------------------------------------
    //  指定の座標に指定の敵セットを生成する
    //------------------------------------------------
    public GameObject SetEnemy(GameObject prefab,Vector3 pos,ePowerupItems item = ePowerupItems.None)
    {
        //  敵オブジェクトの生成
        EnemyManager.Instance.GetEnemyObjectList()
            .Add(Instantiate(prefab,pos,transform.rotation));

        //  敵情報の設定
        EnemySetting es = EnemyManager.Instance.GetEnemySetting();
        EnemyManager.Instance.GetEnemyObjectList()
            .Last().GetComponent<Enemy>().SetEnemyData(es, item);

        return EnemyManager.Instance.GetEnemyObjectList().Last();
    }

    //------------------------------------------------------
    //  弾がフェードアウトする
    //------------------------------------------------------
    private IEnumerator FadeoutBullet(float duration)
    {
        SpriteRenderer fadeSprite = this.GetComponent<SpriteRenderer>();
        fadeSprite.enabled = true;
        var c = fadeSprite.color;
        c.a = 1.0f; // 初期値
        fadeSprite.color = c;

        DOTween.ToAlpha(
        () => fadeSprite.color,
        color => fadeSprite.color = color,
        0.0f, // 目標値
        duration // 所要時間
        );

        yield return new WaitForSeconds(duration);

        //  敵がフェードインする
        StartCoroutine(FadeinEnemy(1.5f));
    }

    //------------------------------------------------------
    //  敵がフェードインする
    //------------------------------------------------------
    private IEnumerator FadeinEnemy(float duration)
    {
        /*
           オブジェクトのSpriteの画像だけ人形のものにして
           それをフェードインさせる。
        　　完了後本物を生成してこのオブジェクトは破棄する。
        */

        //  透明度を0にする
        SpriteRenderer s = this.GetComponent<SpriteRenderer>();
        SpriteRenderer s2 = EnemyManager.Instance.GetHomingBulletSprite();
        s.sprite = s2.sprite;
        s.color = new Color(1,1,1,0);

        //  Animatorを追加する
        this.AddComponent<Animator>();

        //  人形のAnimatorを適用する
        Animator animator = EnemyManager.Instance.GetHomingBulletAnimator();
        this.GetComponent<Animator>().runtimeAnimatorController
            = animator.runtimeAnimatorController;

        //  スケールを1.4倍に合わせる
        this.transform.localScale = new Vector3(1.4f,1.4f,1.4f);

        //  フェードイン開始
        DOTween.ToAlpha(
        () => s.color,
        color => s.color = color,
        1.0f,       // 目標値
        duration    // 所要時間
        );

        //  フェードインの時間待つ
        yield return new WaitForSeconds(duration);

        //  人形のプレハブを取得
        GameObject prefab = EnemyManager.Instance.GetEnemyPrefab((int)EnemyPattern.E01);

        //  人形オブジェクトを生成＆データをセット
        GameObject obj = SetEnemy(prefab, transform.position);

        //  moveTypeをChargeにする
        obj.GetComponent<Enemy>().SetMoveType((int)MOVE_TYPE.Charge);

        //  このオブジェクトを削除
        Destroy(this.gameObject);
    }
}
