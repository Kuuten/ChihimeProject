using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//--------------------------------------------------------------
//
//  ツクモのPhase3の弾クラス
//
//--------------------------------------------------------------
public class TsukumoPhase3Bullet : MonoBehaviour
{
    private Vector3 velocity;
    private float speed;
    private int power;
    private Vector3 vec;    //  弾配置時のベクトル

    private int step;
    private int hp;
    private static readonly int hp_max = 3;

    private float timer;                //  回転を繰り返す時間(秒)
    private Transform parentTransform;  //  ツクモのTransform

    private int homingFrame;            //  ホーミングの計算頻度用

    void Awake()
    {
        speed = 0f;
        power = 0;
        velocity = Vector3.zero;
        step = 0;
        hp = hp_max;
        vec = Vector3.zero;
        timer = 1;
        parentTransform = default;
    }

    void Start()
    {
        //  展開開始
        StartCoroutine( Expand() );

        //  ５秒たったら消す
        Destroy(this.gameObject, 5f);
    }

    void Update()
    {
        //  発狂弾の移動
        BerserkBullet();

        //  ステージクリアしたら残らないように消す
        if(GameManager.Instance.GetStageClearFlag())
        {
            Destroy(this.gameObject);
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
    public void SetVec(Vector3 v){ vec = v; }
    public void SetPower(int p){ power = p; }
    public int GetPower(){ return power; }
    public void SetParentTransform(Transform t){ parentTransform = t; }

    //------------------------------------------------------
    //  発狂弾の移動
    //------------------------------------------------------
    private void BerserkBullet()
    {
        switch(step)
        {
            case 0: //  展開
                break;
            case 1: //  回転
                RotationAxis();
                break;
            case 2: //  ホーミング
                Homing();
                break;
            case 3: //  座標だけ更新する
                //  座標を更新
                transform.position += speed * velocity * Time.deltaTime;
                break;
        }
    }

    //------------------------------------------------------
    //  展開
    //------------------------------------------------------
    private IEnumerator Expand()
    {
        float duration = 0.5f;   //  移動にかかる時間（秒）

        //  回転する
        float rotationTime = 0.5f;
        Tweener tweener = transform.DOLocalRotate(new Vector3(0, 0, -360f), rotationTime, RotateMode.FastBeyond360)  
            .SetEase(Ease.Linear)  
            .SetLoops(-1, LoopType.Restart);

        //  目標座標を設定する
        Vector3 targetPos = transform.position+ vec;

        //  移動開始
        transform.DOMove(targetPos,duration)
            .OnComplete(()=>{
                tweener.Kill();     //  回転停止
                step++;
            });

        //  移動時間待つ
        yield return new WaitForSeconds(duration);
    }
    //------------------------------------------------------
    //  ツクモを中心に回転
    //------------------------------------------------------
    private void RotationAxis()
    {
        // 回転軸
        Vector3 axis = parentTransform.forward;
        // 円運動周期
        float period = 1;
        //  中心点
        Vector3 center = parentTransform.position;

        var tr = transform;
        // 回転のクォータニオン作成
        var angleAxis = Quaternion.AngleAxis(360 / period * Time.deltaTime, axis);

        // 円運動の位置計算
        var pos = tr.position;

        pos -= center;
        pos = angleAxis * pos;
        pos += center;

        tr.position = pos;

        //  カウントダウン
        if( timer <= 0 )
        {
            timer = 0;
            step++;
        }
        else
        {
            timer -= Time.deltaTime;
        }
    }
    //------------------------------------------------------
    //  ホーミング(少し角度補正あり)
    //------------------------------------------------------
    private void Homing()
    {
        Debug.Log("ホーミングを始めます！！");

        //  プレイヤーの座標を取得する
        Vector3 pPos = GameManager.Instance.GetPlayer().transform.position;

        //  弾からプレイヤーへのベクトルを計算する
        Vector3 v = pPos - transform.position;
        v.Normalize();

        //  velocityにそのベクトルを設定する
        velocity = v;

        //  座標を更新
        transform.position += speed * velocity * Time.deltaTime;

        //  フレームを更新
        if(homingFrame < 60 * 1)homingFrame++;
        else
        {
            //  homingFrameが1秒経ったらホーミングをやめる
            step++;
        };
        
    }
}
