using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class ConvertTsukumoBullet : MonoBehaviour
{
    GameObject target;
    private int gamestatus;
    private bool fullPower;
    private bool isL;
    private bool startHoming;
    private float speed = 5.0f;


   void Start()
    {
        target = null;
        startHoming = false;

        //  SE再生
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX_CONVERT_SHOT,
            (int)SFXList.SFX_TSUKUMO_CONVERT_SHOT_01);

        //  コルーチン開始
        StartCoroutine(Move());
    }
    
    void Update()
    {
        //  GameManagerから状態を取得
        gamestatus = GameManager.Instance.GetGameState();

        if(startHoming)Homing();
    }

    //-----------------------------------------------------------
    //  プロパティ
    //-----------------------------------------------------------
    public void SetFullPower(bool b){ fullPower = b; }
    public bool GetFullPower(bool b){ return fullPower; }
    public void SetIsL(bool b){ isL = b; }
    public bool GetIsL(){ return isL; }

    //-----------------------------------------------------------
    //  移動
    //-----------------------------------------------------------
    private IEnumerator Move()
    {
        //  回転する
        float rotationTime = 0.5f;
        Tweener tweener = transform.DOLocalRotate(new Vector3(0, 0, -360f), rotationTime, RotateMode.FastBeyond360)  
            .SetEase(Ease.Linear)  
            .SetLoops(-1, LoopType.Restart); 

        //  回転SE再生
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX_LOOP,
            (int)SFXList.SFX_TSUKUMO_CONVERT_SHOT_02);

        //  プレイヤーの座標
        Vector3 playerPos = GameManager.Instance.GetPlayer().transform.position;

        //  プレイヤーの左右ベクトルを求める
        Vector3 left = -this.transform.right;
        Vector3 right = this.transform.right;

        //  左右に１離れた座標を求める
        float duration = 1.0f;
        float bias = 2.0f;
        Vector3 posL = this.transform.position + left * bias;
        Vector3 posR = this.transform.position + right * bias;

        //  そこへ移動する
        if(isL)
        {
            transform.DOMove(posL,duration);
        }
        else
        {
            transform.DOMove(posR,duration);
        }

        //  移動時間待つ
        yield return new WaitForSeconds(duration);

        //  待つ
        yield return new WaitForSeconds(3);

        //  回転アニメを終了
        tweener.Kill();

        //  ホーミング開始
        startHoming = true;

        //  ホーミングSE再生
        SoundManager.Instance.PlayLoopSFX(
            (int)AudioChannel.SFX_CONVERT_SHOT,
            (int)SFXList.SFX_TSUKUMO_CONVERT_SHOT_03);
    }

    //-----------------------------------------------------------
    //  必中ホーミング(当たるまで追いかけ続ける)
    //-----------------------------------------------------------
    private void Homing()
    {
        //  一番近い敵オブジェクトを取得する
        if(gamestatus == (int)eGameState.Zako)
        {
            target = EnemyManager.Instance.GetNearestEnemyFromPlayer();
        }
        else if(gamestatus == (int)eGameState.Boss)
        {
            target = EventSceneManager.Instance.GetBossObject();
        }else if(gamestatus == (int)eGameState.Event)
        {
            startHoming = false;
            Destroy(this.gameObject);
            return;
        }

        //  敵がいなければ弾を消去して戻る
        if(target == null)
        {
            Debug.Log("敵がいない！");
            Destroy(this.gameObject);
            return;
        }

        //  弾から敵へのベクトルを求める
        Vector3 vec = target.transform.position - this.transform.position;

        //  ベクトルのオイラー角を求める
        Quaternion q = Quaternion.Euler(vec);
        float degree = q.eulerAngles.z;

        //  弾の向きを敵に向ける
        this.transform.Rotate(0,0,degree);

        //  一番近い敵へ突撃
        this.transform.position += vec * speed * Time.deltaTime;
    }
}
