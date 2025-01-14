using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

//--------------------------------------------------------------
//
//  ボス・ツクモのクラス
//
//--------------------------------------------------------------
public class BossTsukumo : MonoBehaviour
{
    //  EnemyDataクラスからの情報取得用
    EnemyData enemyData;
    
    //  パラメータ
    private float hp;
    private bool bDeath;            //  死亡フラグ
    private bool bSuperMode;        //  無敵モードフラグ
    private bool bSuperModeInterval;//  フェーズ切り替え時の無敵モードフラグ

    //  点滅させるためのSpriteRenderer
    SpriteRenderer sp;
    //  点滅の間隔
    private float flashInterval;
    //  点滅させるときのループカウント
    private int loopCount;

    //  やられエフェクト
    [SerializeField] private GameObject explosion;

    //  HPスライダー
    private Slider hpSlider;

    //  制御点
    enum Control
    {
        Left,
        Center,
        Right,

        Max
    };

    //  ドロップパワーアップアイテム一覧
    private ePowerupItems powerupItems;

    //  ギミック弾の使用済み番号格納用
    private int[] buelletNum = new int[(int)TsukumoPhase2Bullet.Direction.MAX];

    //------------------------------------------------------------
    //  Phase2用
    //------------------------------------------------------------
    private GameObject warningObject;

    private bool bWarningFirst;

    //  WARNING時の予測ライン
    private GameObject[] dangerLineObject;

    //  コルーチン停止用フラグ
    Coroutine phase1_Coroutine;
    Coroutine phase2_Coroutine;
    private bool bStopPhase1;
    private bool bStopPhase2;

    private const float phase1_end = 0.66f;   //  フェーズ１終了条件のHP割合の閾値
    private const float phase2_end = 0.33f;   //  フェーズ２終了条件のHP割合の閾値



    void Start()
    {
        //  警告オブジェクトを取得
        warningObject = new GameObject();
        warningObject =
            EnemyManager.Instance.GetBulletPrefab((int)BULLET_TYPE.Douji_Warning);

        //  フラグ初期化
        bStopPhase1 = false;
        bStopPhase2 = false;

        //  WARNING時の予測ラインオブジェクトを取得
        dangerLineObject = new GameObject[(int)TsukumoPhase2Bullet.Direction.MAX];
        dangerLineObject[(int)TsukumoPhase2Bullet.Direction.TOP] =
            EnemyManager.Instance.GetBulletPrefab((int)BULLET_TYPE.Douji_DangerLine_Top);
        dangerLineObject[(int)TsukumoPhase2Bullet.Direction.BOTTOM] =
            EnemyManager.Instance.GetBulletPrefab((int)BULLET_TYPE.Douji_DangerLine_Bottom);
        dangerLineObject[(int)TsukumoPhase2Bullet.Direction.LEFT] =
            EnemyManager.Instance.GetBulletPrefab((int)BULLET_TYPE.Douji_DangerLine_Left);
        dangerLineObject[(int)TsukumoPhase2Bullet.Direction.RIGHT] =
            EnemyManager.Instance.GetBulletPrefab((int)BULLET_TYPE.Douji_DangerLine_Right);

        //  ギミック弾の使用済み番号を初期化
        for(int i=0;i<(int)TsukumoPhase2Bullet.Direction.MAX;i++)
        {
            buelletNum[i] = -1;
        }

        //  死亡フラグOFF
        bDeath = false;
        //  最初は無敵モードOFF
        bSuperMode = false;
        bSuperModeInterval = false;
        //  ループカウントを設定
        loopCount = 1;
        //  点滅の間隔を設定
        flashInterval = 0.2f;
        //  SpriteRenderを取得
        sp = GetComponent<SpriteRenderer>();
        //  Warningの初回フラグ
        bWarningFirst = false;

        //  行動開始
        StartCoroutine(WaitDoujiAction(1));
    }

    private void OnDestroy()
    {
        Debug.Log("ボス撃破！ステージクリア！");

        //  ボス戦やられたらステージクリア
        GameManager.Instance.SetStageClearFlag(true);
    }

    //*********************************************************************************
    //
    //  更新
    //
    //*********************************************************************************
    void Update()
    {
        //  HPが閾値を切ったら抜ける
        if (hp <= enemyData.Hp*phase1_end)
        {
            if(!bStopPhase1)
            {
                hp = enemyData.Hp*phase1_end;
                bStopPhase1 = true;
            }
        }
        if(hp <= enemyData.Hp*phase2_end)
        {
            if(!bStopPhase2)
            {
                hp = enemyData.Hp*phase2_end;
                bStopPhase2 = true;
            }
        }
        
        //  スライダーを更新
        hpSlider.value = hp / enemyData.Hp;
    }

    //  敵のデータを設定 
    public void SetBossData(EnemySetting es, ePowerupItems item)
    {
        string boss_id = "Tsukumo";

        //  敵のデータを設定 
        enemyData = es.DataList
            .FirstOrDefault(enemy => enemy.Id == boss_id );

        //  体力を設定
        hp = enemyData.Hp;

        Debug.Log( "タイプ: " + boss_id + "\nHP: " + hp );
        Debug.Log( boss_id + "の設定完了" );

        //Debug.Log($"ID：{enemyData.Id}");
        //Debug.Log($"HP：{enemyData.Hp}");
        //Debug.Log($"攻撃力：{enemyData.Attack}");
        //Debug.Log($"落魂：{enemyData.Money}");

        if(item == ePowerupItems.None)
        {
            powerupItems = default;
        }
        else
        {
            //  ドロップアイテムを設定
            powerupItems = item;    
        }
    }

    //----------------------------------------------------------------------
    //  プロパティ
    //----------------------------------------------------------------------
    public EnemyData GetEnemyData(){ return enemyData; }
    public void SetEnemyData(EnemyData ed){ enemyData = ed; }
    public void SetHp(float health){ hp = health; }
    public float GetHp(){ return hp; }
    public void SetSuperMode(bool flag){ bSuperMode = flag; }
    public bool GetSuperMode(){ return bSuperMode; }
    public void SetHpSlider(Slider s){ hpSlider = s; }

    //------------------------------------------------------
    //  ダメージSEを再生した後無敵モードをオフにする
    //------------------------------------------------------
    IEnumerator PlayDamageSFXandSuperModeOff()
    {
        float interval = 0.1f;  //  無敵が解除されるまでの時間（秒）

        //  ダメージSE再生
        SoundManager.Instance.PlaySFX(
        (int)AudioChannel.SFX_ENEMY,
        (int)SFXList.SFX_ENEMY_DAMAGE);

        //  何秒か待つ
        yield return new WaitForSeconds(interval);

        //  無敵モードOFF
        bSuperMode = false;
    }

    //  敵に当たったら爆発する
    //  当たり判定の基礎知識：
    //  当たり判定を行うには、
    //  ・両者にColliderがついている
    //  ・どちらかにRigidBodyがついている
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(bDeath)return;


        if (collision.CompareTag("NormalBullet"))
        {
            //  弾の消去
            Destroy(collision.gameObject);

            //  無敵モードなら弾だけ消して返す
            if(bSuperMode || bSuperModeInterval)return;

            //  ダメージ処理
            float d = GameManager.Instance.GetPlayer()
                .GetComponent<PlayerShotManager>().GetNormalShotPower();
            Damage(d);

            //  点滅演出
            StartCoroutine(Blink(true,loopCount,flashInterval));

            //  ダメージSE再生
            StartCoroutine(PlayDamageSFXandSuperModeOff());

            //  死亡フラグON
            if(hp <= 0)
            {
                bDeath = true;
                Death();                       //  やられ演出
            }
        }
        else if (collision.CompareTag("DoujiConvert"))
        {
            //  フェーズ切替時の無敵モードなら弾だけ消して返す
            if(bSuperModeInterval)
            {
                //  弾の消去
                Destroy(collision.gameObject);
                return;
            }

            //  ダメージ処理
            float d = collision.GetComponent<ConvertDoujiBullet>().GetInitialPower();
            Damage(d);

            //  中攻撃か強攻撃か判定
            bool isFullPower = GameManager.Instance.GetPlayer()
                .GetComponent<PlayerShotManager>().IsConvertFullPower();

            //  魂バーストゲージを増やす
            if(isFullPower)
            {
                PlayerBombManager.Instance.PlusKonburstGauge(true);
            }
            //  魂バーストゲージを少し増やす
            else
            {
                PlayerBombManager.Instance.PlusKonburstGauge(false);  
            }

            //  点滅演出
            StartCoroutine(Blink(true,loopCount,flashInterval));

            //  ダメージSE再生
            StartCoroutine(PlayDamageSFXandSuperModeOff());

            //  死亡フラグON
            if(hp <= 0)
            {
                bDeath = true;
                Death2();                      //  やられ演出2
            }
        }
        else if (collision.CompareTag("DoujiKonburst"))
        {
            //  フェーズ切替時の無敵モードなら弾だけ消して返す
            if(bSuperModeInterval)return;

            //  ダメージ処理
            float d = GameManager.Instance.GetPlayer()
                .GetComponent<PlayerBombManager>().GetKonburstShotPower();
            Damage(d);

            //  点滅演出
            StartCoroutine(Blink(true,loopCount,flashInterval));

            //  ダメージSE再生
            StartCoroutine(PlayDamageSFXandSuperModeOff());

            //  死亡フラグON
            if(hp <= 0)
            {
                bDeath = true;
                Death2();                      //  やられ演出2
            }
        }
        else if (collision.CompareTag("TsukumoConvert"))
        {
            //  フェーズ切替時の無敵モードなら弾だけ消して返す
            if(bSuperModeInterval)
            {
                //  弾の消去
                Destroy(collision.gameObject);
                return;
            }

            //  弾を消す
            Destroy(collision.gameObject);

            //  ダメージ処理
            float d = GameManager.Instance.GetPlayer()
                .GetComponent<PlayerShotManager>().GetConvertShotPower();
            Damage(d);

            //  中攻撃か強攻撃か判定
            bool isFullPower = GameManager.Instance.GetPlayer()
                .GetComponent<PlayerShotManager>().IsConvertFullPower();

            //  魂バーストゲージを増やす
            if(isFullPower)
            {
                PlayerBombManager.Instance.PlusKonburstGauge(true);
            }
            //  魂バーストゲージを少し増やす
            else
            {
                PlayerBombManager.Instance.PlusKonburstGauge(false);  
            }

            //  点滅演出
            StartCoroutine(Blink(true,loopCount,flashInterval));

            //  ダメージSE再生
            StartCoroutine(PlayDamageSFXandSuperModeOff());

            //  死亡フラグON
            if(hp <= 0)
            {
                bDeath = true;
                Death2();                      //  やられ演出2
            }
        }
        else if (collision.CompareTag("TsukumoKonburst"))
        {
            //  フェーズ切替時の無敵モードなら弾だけ消して返す
            if(bSuperModeInterval)
            {
                //  弾の消去
                Destroy(collision.gameObject);
                return;
            }

            //  弾を消す
            Destroy(collision.gameObject);

            //  ダメージ処理
            float d = GameManager.Instance.GetPlayer()
                .GetComponent<PlayerBombManager>().GetKonburstShotPower();
            Damage(d);

            //  点滅演出
            StartCoroutine(Blink(true,loopCount,flashInterval));

            //  ダメージSE再生
            StartCoroutine(PlayDamageSFXandSuperModeOff());

            //  死亡フラグON
            if(hp <= 0)
            {
                bDeath = true;
                Death2();                      //  やられ演出2
            }
        }
        else if (collision.CompareTag("Bomb"))
        {
            //  フェーズ切替時の無敵モードなら返す
            if(bSuperModeInterval)return;

            //  ダメージ処理
            float d = GameManager.Instance.GetPlayer()
                .GetComponent<PlayerBombManager>().GetBombPower();
            Damage(d);

            //  点滅演出
            StartCoroutine(Blink(true,loopCount,flashInterval));

            //  ダメージSE再生
            StartCoroutine(PlayDamageSFXandSuperModeOff());

            //  死亡フラグON
            if(hp <= 0)
            {
                bDeath = true;
                Death();                        //  やられ演出
            }
        }

        // 残りHP表示
        //Debug.Log("残りHP: " + hp);
    }

    //-------------------------------------------
    //  ダメージ処理
    //-------------------------------------------
    public void Damage(float value)
    {
        if(hp > 0.0f)
        {
            hp -= value;
        }
        else
        {
            hp = 0.0f;
        }
    }

   //-------------------------------------------
    //  ダメージ時の点滅演出
    //-------------------------------------------
    public IEnumerator Blink(bool super, int loop_count, float flash_interval)
    {
        //  無敵モードON
        if(super)bSuperMode = true;

        //点滅ループ開始
        for (int i = 0; i < loop_count; i++)
        {
            //flashInterval待ってから
            yield return new WaitForSeconds(flash_interval);

            //spriteRendererをオフ
            if(sp)sp.enabled = false;

            //flashInterval待ってから
            yield return new WaitForSeconds(flash_interval);

            //spriteRendererをオン
            if(sp)sp.enabled = true;
        }
        //  無敵モードOFF
        if(super)bSuperMode = false;
    }

    //-------------------------------------------
    //  やられ演出(通常弾・ボム)
    //-------------------------------------------
    private void Death()
    {
        //  やられエフェクト
        Instantiate(explosion, transform.position, transform.rotation);

        // やられSE
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX_ENEMY,
            (int)SFXList.SFX_ENEMY_DEATH);

        //  アイテムドロップ判定
        DropItems drop = this.GetComponent<DropItems>();
        if (powerupItems == ePowerupItems.Random)
        {
            if (drop) drop.DropRandomPowerupItem();
        }
        else
        {
            if (drop) drop.DropPowerupItem(powerupItems);
        }

        //  お金を生成
        drop.DropKon(enemyData.Money);

        //  オブジェクトを削除
        Destroy(this.gameObject);
    }

    //-------------------------------------------
    //  やられ演出(魂バート)
    //-------------------------------------------
    private void Death2()
    {
        //  やられエフェクト
        Instantiate(explosion, transform.position, transform.rotation);

        // やられSE
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX_ENEMY,
            (int)SFXList.SFX_ENEMY_DEATH);

        //  アイテムドロップ判定
        DropItems drop = this.GetComponent<DropItems>();
        if (powerupItems == ePowerupItems.Random)
        {
            if (drop) drop.DropRandomPowerupItem();
        }
        else
        {
            if (drop) drop.DropPowerupItem(powerupItems);
        }

        //  お金を生成(魂バートの時は2倍)
        int dropMoney = enemyData.Money;
        drop.DropKon(2 * dropMoney);

        //  オブジェクトを削除
        Destroy(this.gameObject);
    }

    //******************************************************************
    //
    //  ツクモの移動パターン
    //
    //******************************************************************

    //------------------------------------------------------------------
    //  数秒待ってからツクモの行動開始
    //------------------------------------------------------------------
    private IEnumerator WaitDoujiAction(float duration)
    {
        yield return new WaitForSeconds(duration);

        //  行動開始
        StartCoroutine(StartDoujiAction());
    }

    //------------------------------------------------------------------
    //  ツクモの行動管理関数
    //------------------------------------------------------------------
    private IEnumerator StartDoujiAction()
    {
        Debug.Log("***ツクモ弾幕フェーズ開始！***");

        //  フェーズ１開始
        phase1_Coroutine = StartCoroutine(Tsukumo_Phase1());

        //  フラグがTRUEになるまで待つ
        yield return new WaitUntil(()=>bStopPhase1 == true);

        //  フラグでコルーチン停止
        StopCoroutine(phase1_Coroutine);

        //  フェーズ変更
        yield return StartCoroutine(Tsukumo_PhaseChange());

        //  フェーズ２開始
        phase2_Coroutine = StartCoroutine(Tsukumo_Phase2());

        //  フラグがTRUEになるまで待つ
        yield return new WaitUntil(()=>bStopPhase2 == true);

        //  フラグでコルーチン停止
        StopCoroutine(phase2_Coroutine);

        //  フェーズ変更
        yield return StartCoroutine(Tsukumo_PhaseChange());

        //  フェーズ３開始
        StartCoroutine(Douji_Phase3());
    }

    //------------------------------------------------------------------
    //  ツクモのフェーズチェンジ時の行動
    //------------------------------------------------------------------
    private IEnumerator Tsukumo_PhaseChange()
    {
        //  移動にかかる時間(秒)
        float duration = 1.5f;
        //  移動後に待機する時間(秒)
        float interval = 5.0f;

        //  真ん中に移動する
        yield return StartCoroutine(Tsukumo_MoveToCenter(duration));

        //  次のフェーズまで待つ
        yield return new WaitForSeconds(interval);

        //  無敵モードOFF
        bSuperModeInterval = false;
    }

    //------------------------------------------------------------------
    //  ツクモのPhase1
    //------------------------------------------------------------------
    private IEnumerator Tsukumo_Phase1()
    {
        Debug.Log("フェーズ１開始");

        //  フェーズ１
        while (!bStopPhase1)
        {
            yield return StartCoroutine(Tsukumo_LoopMove(1.5f, 0.5f));

            yield return StartCoroutine(Shot());


            //yield return StartCoroutine(Tsukumo_LoopMove(1.0f, 1.0f));
            //yield return StartCoroutine(Warning());
            //StartCoroutine(TatamiSand());
            //yield return StartCoroutine(TatamiSand());


            //yield return StartCoroutine(Douji_BerserkBarrage());

            //yield return StartCoroutine(Douji_LoopMoveBerserk(3, 0.6f, 1.0f));

            //yield return StartCoroutine(Douji_BerserkGatling());

            //yield return StartCoroutine(Douji_LoopMoveBerserk(3, 0.6f, 1.0f));

            //yield return StartCoroutine(Douji_BerserkGatling());
        }
    }

    //------------------------------------------------------------------
    //  ツクモのPhase2
    //------------------------------------------------------------------
    private IEnumerator Tsukumo_Phase2()
    {
        Debug.Log("フェーズ２へ移行");

        //  フェーズ２
        while (!bStopPhase2)
        {
            StartCoroutine(WildlyShotSmall());

            //  Warning!(初回のみ)
            yield return StartCoroutine(Warning());

            StartCoroutine(TatamiSand());
            //StartCoroutine(TatamiSand());
            //StartCoroutine(TatamiSand());

            yield return StartCoroutine(TatamiSand());

            yield return StartCoroutine(Tsukumo_LoopMove(1.0f,1.0f));
        }
    }

    //------------------------------------------------------------------
    //  ツクモのPhase3
    //------------------------------------------------------------------
    private IEnumerator Douji_Phase3()
    {
        Debug.Log("フェーズ３へ移行");

        //  フェーズ３
        while (true)
        {
            yield return StartCoroutine(Tsukumo_LoopMove(1.5f, 0.5f));

            yield return StartCoroutine(Shot());

            //yield return StartCoroutine(Douji_LoopMoveBerserk(3, 0.6f, 1.0f));

            //yield return StartCoroutine(Douji_BerserkBarrage());
            //yield return StartCoroutine(Douji_BerserkGatling());

            //yield return StartCoroutine(Douji_LoopMoveBerserk(3, 0.6f, 1.0f));

            //yield return StartCoroutine(Douji_BerserkBarrage());
            //yield return StartCoroutine(Douji_BerserkGatling());
        }
    }

    //------------------------------------------------------------------
    //  ツクモの移動
    //------------------------------------------------------------------
    private IEnumerator Tsukumo_LoopMove(float duration,float interval)
    {
        int currentlNum = (int)Control.Center;      //  現在位置
        List<int> targetList = new List<int>();     //  目標位置候補リスト
        int targetNum = (int)Control.Center;        //  目標位置

        //  現在位置を求める（一番近い位置とする）
        Vector3 p1 = EnemyManager.Instance.GetControlPointPos((int)Control.Left);
        Vector3 p2 = EnemyManager.Instance.GetControlPointPos((int)Control.Center);
        Vector3 p3 = EnemyManager.Instance.GetControlPointPos((int)Control.Right);
        float d1 = Vector3.Distance(p1,this.transform.position);
        float d2 = Vector3.Distance(p2,this.transform.position);
        float d3 = Vector3.Distance(p3,this.transform.position);
        List<float> dList = new List<float>();
        dList.Clear();
        dList.Add(d1);
        dList.Add(d2);
        dList.Add(d3);
        
        //  並び替え
        dList.Sort();

        if(dList[0] == d1)currentlNum = (int)Control.Left;
        if(dList[0] == d2)currentlNum = (int)Control.Center;
        if(dList[0] == d3)currentlNum = (int)Control.Right;

        //  リストをクリア
        targetList.Clear();

        //  目標の番号を抽選
        if(currentlNum ==(int)Control.Left)
        {
            targetList.Add((int)Control.Center);
            targetList.Add((int)Control.Right);
        }
        else if(currentlNum ==(int)Control.Center)
        {
            targetList.Add((int)Control.Left);
            targetList.Add((int)Control.Right);
        }
        else if(currentlNum ==(int)Control.Right)
        {
            targetList.Add((int)Control.Left);
            targetList.Add((int)Control.Center); 
        }

        //  目標番号を抽選
        targetNum = targetList[Random.Range(0, targetList.Count)];

        //  目標座標を取得
        Vector3 targetPos = EnemyManager.Instance.GetControlPointPos(targetNum);

        //  横移動開始
        transform.DOLocalMoveX(targetPos.x, duration)
            .SetEase(Ease.Linear);

        //  縦移動開始
        transform.DOLocalMoveY(-2f, duration/2)
            .SetEase(Ease.Linear)
            .SetRelative(true);

        //  移動時間待つ
        yield return new WaitForSeconds(duration/2);

        //  縦移動開始
        transform.DOLocalMoveY(2f, duration/2)
            .SetEase(Ease.Linear)
            .SetRelative(true);

        //  移動時間待つ
        yield return new WaitForSeconds(duration/2);

        //  現在の番号を更新
        currentlNum = targetNum;

        //  次の移動まで待つ
        yield return new WaitForSeconds(interval);
    }

    //------------------------------------------------------------------
    //  フェーズの切り替え時にツクモが真ん中に移動する
    //------------------------------------------------------------------
    private IEnumerator Tsukumo_MoveToCenter(float duration)
    {
        int targetNum = (int)Control.Center;        //  目標位置

        //  無敵モードON
        bSuperModeInterval = true;

        //  目標座標を取得
        Vector3 targetPos = EnemyManager.Instance.GetControlPointPos(targetNum);

        //  横移動開始
        transform.DOLocalMoveX(targetPos.x, duration)
            .SetEase(Ease.Linear);

        //  縦移動開始
        transform.DOLocalMoveY(targetPos.y, duration)
            .SetEase(Ease.Linear);

        //  移動時間待つ
        yield return new WaitForSeconds(duration);

        //  アイテムドロップ判定
        DropItems drop = this.GetComponent<DropItems>();
        //  確定ドロップでショット強化を落とす
        if (drop) drop.DropPowerupItem(ePowerupItems.PowerUp);
    }

    //------------------------------------------------------------------
    //  通常弾をランダムに選択して撃つ
    //------------------------------------------------------------------
    private IEnumerator Shot()
    {
        yield return StartCoroutine(WildlyShot());

        yield return StartCoroutine(SnipeShot());

        yield return StartCoroutine(OriginalShot());
    }

    //------------------------------------------------------------------
    //  バラマキ弾(小)
    //------------------------------------------------------------------
    private IEnumerator WildlyShotSmall()
    {
        //  通常バラマキ弾のプレハブを取得
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Wildly_Normal);

        float totalDegree = 180;        //  撃つ範囲の総角  
        int wayNum = 5;                 //  弾のway数(必ず3way以上の奇数にすること)
        float Degree = totalDegree / (wayNum-1);     //  弾一発毎にずらす角度         
        float speed = 3.0f;             //  弾速
        int chain = 5;                  //  連弾数
        float chainInterval = 0.8f;     //  連弾の間隔（秒）

        //  敵の前方ベクトルを取得
        Vector3[] vector = new Vector3[wayNum];
        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < wayNum; i++)
            {
                Vector3 vector0 = Quaternion.Euler(0,0,Random.Range(-10,11)) * -transform.up;

                vector[i] = Quaternion.Euler(
                        0, 0, -Degree * ((wayNum-1)/2) + (i * Degree)
                    ) * vector0;
                vector[i].z = 0f;

                //弾インスタンスを取得し、初速と発射角度を与える
                GameObject Bullet_obj = 
                    (GameObject)Instantiate(bullet, transform.position, transform.rotation);
                EnemyBullet enemyBullet = Bullet_obj.GetComponent<EnemyBullet>();
                enemyBullet.SetSpeed(speed);
                enemyBullet.SetVelocity(vector[i]);
                enemyBullet.SetPower(enemyData.Attack);

                if(i == 0)
                {
                    //  発射SE再生
                    SoundManager.Instance.PlaySFX(
                    (int)AudioChannel.ENEMY_SHOT,
                    (int)SFXList.SFX_ENEMY_SHOT);
                }
            }
            yield return new WaitForSeconds(chainInterval);
        }

        yield return null;
    }

    //------------------------------------------------------------------
    //  バラマキ弾
    //------------------------------------------------------------------
    private IEnumerator WildlyShot()
    {
        //  通常バラマキ弾のプレハブを取得
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Wildly_Big);

        float totalDegree = 180;        //  撃つ範囲の総角  
        int wayNum = 15;                //  弾のway数(必ず3way以上の奇数にすること)
        float Degree = totalDegree / (wayNum-1);     //  弾一発毎にずらす角度         
        float speed = 7.0f;             //  弾速
        float chainInterval = 0.03f;    //  連弾の間隔（秒）
        float AttackInterval = 0.5f;    //  弾幕毎の間隔（秒）
        Vector3[] vector = new Vector3[wayNum];

        //-----------------------------------------------
        //  右から左へバラマキ
        //-----------------------------------------------
        for (int i = 0; i < wayNum; i++)
        {
            Vector3 vector0 = Quaternion.Euler(0,0,90) * -transform.up;

            vector[i] = Quaternion.Euler(0, 0, (i * -Degree)) * vector0;
            vector[i].z = 0f;

            //弾インスタンスを取得し、初速と発射角度を与える
            GameObject Bullet_obj = 
                (GameObject)Instantiate(bullet, transform.position, transform.rotation);
            EnemyBullet enemyBullet = Bullet_obj.GetComponent<EnemyBullet>();
            enemyBullet.SetSpeed(speed);
            enemyBullet.SetVelocity(vector[i]);
            enemyBullet.SetPower(enemyData.Attack);

            if(i == 0)
            {
                //  発射SE再生
                SoundManager.Instance.PlaySFX(
                (int)AudioChannel.ENEMY_SHOT,
                (int)SFXList.SFX_ENEMY_SHOT);
            }

            yield return new WaitForSeconds(chainInterval);
        }

        //  攻撃間隔を開ける
        yield return new WaitForSeconds(AttackInterval);

        //-----------------------------------------------
        //  左から右へバラマキ
        //-----------------------------------------------
        for (int i = 0; i < wayNum; i++)
        {
            Vector3 vector0 = Quaternion.Euler(0,0,-90) * -transform.up;

            vector[i] = Quaternion.Euler(0, 0, (i * Degree)) * vector0;
            vector[i].z = 0f;

            //弾インスタンスを取得し、初速と発射角度を与える
            GameObject Bullet_obj = 
                (GameObject)Instantiate(bullet, transform.position, transform.rotation);
            EnemyBullet enemyBullet = Bullet_obj.GetComponent<EnemyBullet>();
            enemyBullet.SetSpeed(speed);
            enemyBullet.SetVelocity(vector[i]);
            enemyBullet.SetPower(enemyData.Attack);

            if(i == 0)
            {
                //  発射SE再生
                SoundManager.Instance.PlaySFX(
                (int)AudioChannel.ENEMY_SHOT,
                (int)SFXList.SFX_ENEMY_SHOT);
            }

            yield return new WaitForSeconds(chainInterval);
        }

        //  攻撃間隔を開ける
        yield return new WaitForSeconds(AttackInterval);

        //-----------------------------------------------
        //  左右からバラマキ
        //-----------------------------------------------
        for (int i = 0; i < wayNum; i++)
        {
            Vector3 vectorR = Quaternion.Euler(0,0,90) * -transform.up;
            Vector3 vectorL = Quaternion.Euler(0,0,-90) * -transform.up;

            //弾インスタンスを取得し、初速と発射角度を与える
            vector[i] = Quaternion.Euler(0, 0, (i * -Degree)) * vectorR;
            vector[i].z = 0f;

            GameObject Bullet_objR = 
                (GameObject)Instantiate(bullet, transform.position, transform.rotation);
            EnemyBullet enemyBulletR = Bullet_objR.GetComponent<EnemyBullet>();
            enemyBulletR.SetSpeed(speed);
            enemyBulletR.SetVelocity(vector[i]);
            enemyBulletR.SetPower(enemyData.Attack);

            vector[i] = Quaternion.Euler(0, 0, (i * Degree)) * vectorL;
            vector[i].z = 0f;

            GameObject Bullet_objL = 
                (GameObject)Instantiate(bullet, transform.position, transform.rotation);
            EnemyBullet enemyBulletL = Bullet_objL.GetComponent<EnemyBullet>();
            enemyBulletL.SetSpeed(speed);
            enemyBulletL.SetVelocity(vector[i]);
            enemyBulletL.SetPower(enemyData.Attack);

            if(i == 0)
            {
                //  発射SE再生
                SoundManager.Instance.PlaySFX(
                (int)AudioChannel.ENEMY_SHOT,
                (int)SFXList.SFX_ENEMY_SHOT);
            }

            yield return new WaitForSeconds(chainInterval);
        }

        //  攻撃間隔を開ける
        yield return new WaitForSeconds(AttackInterval);
    }

    //------------------------------------------------------------------
    //  自機狙い弾
    //------------------------------------------------------------------
    private IEnumerator SnipeShot()
    {
        //  通常自機狙い弾のプレハブを取得
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Snipe_Big);

        float Degree = 15;              //  ずらす角度
        int wayNum = 3;                 //  弾のway数
        float speed = 9.0f;            //  弾速
        int chain = 3;                  //  連弾数
        float chainInterval = 1f;       //  連弾の間隔（秒）



        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < wayNum; i++)
            {
                //  敵からプレイヤーへのベクトルを取得
                Vector3 playerPos = GameManager.Instance.GetPlayer().transform.position;
                Vector3 vector0 = (playerPos - transform.position).normalized;
                Vector3[] vector = new Vector3[wayNum];

                vector[i] = Quaternion.Euler( 0, 0, -Degree + (i * Degree) ) * vector0;
                vector[i].z = 0f;

                //弾インスタンスを取得し、初速と発射角度を与える
                GameObject Bullet_obj = 
                    (GameObject)Instantiate(bullet, transform.position, transform.rotation);
                EnemyBullet enemyBullet = Bullet_obj.GetComponent<EnemyBullet>();
                enemyBullet.SetSpeed(speed);
                enemyBullet.SetVelocity(vector[i]);
                enemyBullet.SetPower(enemyData.Attack);

                if(i == 0)
                {
                    //  発射SE再生
                    SoundManager.Instance.PlaySFX(
                    (int)AudioChannel.ENEMY_SHOT,
                    (int)SFXList.SFX_ENEMY_SHOT);
                }
            }
            yield return new WaitForSeconds(chainInterval);
        }

        yield return null;
    }

    //------------------------------------------------------------------
    //  オリジナル弾・ホーミングショット
    //------------------------------------------------------------------
    private IEnumerator OriginalShot()
    {
        //  通常自機狙い弾のプレハブを取得
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Wildly_Big);

        //  ランダムなコントロールポイントに向かって弾が飛んでいき、
        //  一定距離まで近づくとその周りを回転し始める。
        //  一定時間回転した弾はプレイヤーに向かって飛んでいく。

        int wayNum = 5;                 //  一度に撃つ弾数
        float speed = 7.0f;             //  弾速
        int chain = 1;                  //  連弾数
        float chainInterval = 2f;       //  連弾の間隔（秒）
        float Interval = 3f;            //  次の行動までの間隔（秒）

        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < wayNum; i++)
            {
                //  弾インスタンスを取得し、初速と発射角度を与える
                GameObject Bullet_obj =
                    (GameObject)Instantiate(bullet, transform.position, transform.rotation);

                //  弾にデフォルトでEnemyBulletコンポーネントがあるのでそれを削除する
                Destroy(Bullet_obj.GetComponent<EnemyBullet>());

                //  代わりにTsukumoHomingBulletコンポーネントを追加する
                Bullet_obj.AddComponent<TsukumoHomingBullet>();

                //  必要な情報をセットする
                TsukumoHomingBullet enemyBullet = Bullet_obj.GetComponent<TsukumoHomingBullet>();
                enemyBullet.SetSpeed(speed);
                enemyBullet.SetPower(enemyData.Attack);

                if (i == 0)
                {
                    //  発射SE再生
                    SoundManager.Instance.PlaySFX(
                    (int)AudioChannel.ENEMY_SHOT,
                    (int)SFXList.SFX_ENEMY_SHOT);
                }
            }
            yield return new WaitForSeconds(chainInterval);
        }

        yield return new WaitForSeconds(Interval);;
    }


    //------------------------------------------------------------------
    //  Phase2:子鬼の群れの進路を表示する
    //------------------------------------------------------------------
    private IEnumerator DisplayDirection(TsukumoPhase2Bullet.Direction direction, Vector2 pos)
    {
        GameObject line = null;

        //  SEを再生
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX_SYSTEM,
            (int)SFXList.SFX_DOUJI_WARNING);

        //  予測進路のスライダーを生成
        GameObject canvas = EnemyManager.Instance.GetDangerLineCanvas();
        line = Instantiate(dangerLineObject[(int)direction]);
        line.transform.SetParent(canvas.transform);
        line.GetComponent<RectTransform>().anchoredPosition = pos;

        yield return new WaitForSeconds(1);

        if(line.gameObject)Destroy(line.gameObject);
    }
    //------------------------------------------------------------------
    //  Phase2:警告を出す
    //------------------------------------------------------------------
    private IEnumerator Warning()
    {
        float duration = 3.0f;

        if(bWarningFirst)yield break;

        //  SEを再生
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX_SYSTEM,
            (int)SFXList.SFX_DOUJI_WARNING);

        //  テスト
        GameObject canvas = GameObject.Find("BossCanvas");

        if(!canvas.activeSelf)
        {
            Debug.LogError("BossCanvasが有効化されていません！");
        }


        //  WARNINGを有効化
        warningObject.SetActive(true);

        //  初回ではなくなったのでTRUE
        bWarningFirst = true;

        //  演出が３秒なのでその分待つ
        yield return new WaitForSeconds(duration);

        //  WARNINGを無効化
        warningObject.SetActive(false);
    }

    //------------------------------------------------------------------
    //  Phase2:ランダムなスポナーから畳が挟んでくる
    //------------------------------------------------------------------
    private IEnumerator TatamiSand()
    {
        int fourDirection = -1;             //  抽選する方向
        int fourDirection_mirror = -1;      //  その反対側の方向

        //  両端から挟み込むので抽選する方向は上と左だけあればよい

        while(true)
        {
            //  まずは２方向で抽選
            int rand  = Random.Range(0,2);

            if (rand == 0)
            {
                fourDirection = 0;  //  上方向
            }
            else
            {
               fourDirection = 2;   //  左方向
            }

            //  抽選した方向に応じて反対側の方向を設定する
            if (fourDirection == 0)fourDirection_mirror = 1;        //  上下
            else if(fourDirection == 2)fourDirection_mirror = 3;    //  左右

            //  番号が使用済みではなかったら
            if(buelletNum[fourDirection] == -1)
            {
                //  今回の番号を記録
                buelletNum[fourDirection] = fourDirection;

                //  反対側の番号も登録
                buelletNum[fourDirection_mirror] = fourDirection_mirror;

                break;
            }
        }

        //  発生確率の閾値で呼び分ける
        if (fourDirection == 0)         //  上方向とする
        {
            //  ギミック弾のプレハブを取得
            /*プレハブの種類が増える予定*/
            GameObject bullet = EnemyManager.Instance
                .GetBulletPrefab((int)BULLET_TYPE.Tsukumo_Gimmick_Top);

            GameObject bullet_mirror = EnemyManager.Instance
                .GetBulletPrefab((int)BULLET_TYPE.Tsukumo_Gimmick_Bottom);

            //  ３箇所で抽選
            int rand = Random.Range(0,3);
            Vector3 pos = default;
            Vector3 pos_mirror = default;
            
            if(rand == 0)       //  左
            {
                pos = EnemyManager.Instance.GetSpawnerPos(0);

                //  子鬼の群れの進路を表示する
                StartCoroutine(
                    DisplayDirection(
                        TsukumoPhase2Bullet.Direction.TOP,
                        new Vector2(-375,300)
                        ));

                pos_mirror = EnemyManager.Instance.GetSpawnerPos(8);

                //  子鬼の群れの進路を表示する
                yield return StartCoroutine(
                    DisplayDirection(
                        TsukumoPhase2Bullet.Direction.BOTTOM,
                        new Vector2(-375,-300)
                        ));
            }
            else if(rand == 1)  //  中
            {
                pos = EnemyManager.Instance.GetSpawnerPos(1);

                //  子鬼の群れの進路を表示する
                StartCoroutine(
                    DisplayDirection(
                        TsukumoPhase2Bullet.Direction.TOP,
                        new Vector2(-60,300)
                        ));

                pos_mirror = EnemyManager.Instance.GetSpawnerPos(7);

                //  子鬼の群れの進路を表示する
                yield return StartCoroutine(
                    DisplayDirection(
                        TsukumoPhase2Bullet.Direction.BOTTOM,
                        new Vector2(-60,-300)
                        ));
            }
            else if(rand == 2)  //  右
            {
                pos = EnemyManager.Instance.GetSpawnerPos(2);

                //  子鬼の群れの進路を表示する
                StartCoroutine(
                    DisplayDirection(
                        TsukumoPhase2Bullet.Direction.TOP,
                        new Vector2(250,300)
                        ));

                pos_mirror = EnemyManager.Instance.GetSpawnerPos(6);

                //  子鬼の群れの進路を表示する
                yield return StartCoroutine(
                    DisplayDirection(
                        TsukumoPhase2Bullet.Direction.BOTTOM,
                        new Vector2(250,-300)
                        ));
            }

            //  0.5秒のディレイをかける
            yield return new WaitForSeconds(0.5f);

            /************************/
            /* 上方向のセットアップ */
            /************************/

            //  子鬼を生成
            GameObject obj_up = Instantiate(bullet,pos,Quaternion.identity);
            TsukumoPhase2Bullet bulletComp_up =  obj_up.GetComponent<TsukumoPhase2Bullet>();
            bulletComp_up.SetPower(enemyData.Attack);
            bulletComp_up.SetDirection(TsukumoPhase2Bullet.Direction.TOP);

            //  子鬼が突撃する
            StartCoroutine(bulletComp_up.BulletMove());

            /************************/
            /* 下方向のセットアップ */
            /************************/

            //  子鬼を生成
            GameObject obj_bottom = Instantiate(bullet,pos_mirror,Quaternion.identity);
            TsukumoPhase2Bullet bulletComp_bottom =  obj_bottom.GetComponent<TsukumoPhase2Bullet>();
            bulletComp_bottom.SetPower(enemyData.Attack);
            bulletComp_bottom.SetDirection(TsukumoPhase2Bullet.Direction.BOTTOM);

            //  子鬼が突撃する
            yield return StartCoroutine(bulletComp_bottom.BulletMove());

            //  リセット
            buelletNum[fourDirection] = -1;
            buelletNum[fourDirection_mirror] = -1;
        }
        else if (fourDirection == 2)    //  左方向とする
        {
            //  ギミック弾のプレハブを取得
            GameObject bullet = EnemyManager.Instance
                .GetBulletPrefab((int)BULLET_TYPE.Tsukumo_Gimmick_Left);

            GameObject bullet_mirror = EnemyManager.Instance
                .GetBulletPrefab((int)BULLET_TYPE.Tsukumo_Gimmick_Right);

            //  ３箇所で抽選
            int rand = Random.Range(0,3);
            Vector3 pos = default;
            Vector3 pos_mirror = default;

            if(rand == 0)       //  上
            {
                pos = EnemyManager.Instance.GetSpawnerPos(11);

                //  子鬼の群れの進路を表示する
                StartCoroutine(
                    DisplayDirection(
                        TsukumoPhase2Bullet.Direction.LEFT,
                        new Vector2(-430,180)
                        ));

                pos_mirror = EnemyManager.Instance.GetSpawnerPos(3);

                //  子鬼の群れの進路を表示する
                yield return StartCoroutine(
                    DisplayDirection(
                        TsukumoPhase2Bullet.Direction.RIGHT,
                        new Vector2(300,180)
                        ));
            }
            else if(rand == 1)  //  中
            {
                pos = EnemyManager.Instance.GetSpawnerPos(10);

                //  子鬼の群れの進路を表示する
                StartCoroutine(
                    DisplayDirection(
                        TsukumoPhase2Bullet.Direction.LEFT,
                        new Vector2(-430,-15)
                        ));

                pos_mirror = EnemyManager.Instance.GetSpawnerPos(4);

                //  子鬼の群れの進路を表示する
                yield return StartCoroutine(
                    DisplayDirection(
                        TsukumoPhase2Bullet.Direction.RIGHT,
                        new Vector2(300,-15)
                        ));
            }
            else if(rand == 2)  //  下
            {
                pos = EnemyManager.Instance.GetSpawnerPos(9);

                //  子鬼の群れの進路を表示する
                StartCoroutine(
                    DisplayDirection(
                        TsukumoPhase2Bullet.Direction.LEFT,
                        new Vector2(-430,-220)
                        ));

                pos_mirror = EnemyManager.Instance.GetSpawnerPos(5);

                //  子鬼の群れの進路を表示する
                yield return StartCoroutine(
                    DisplayDirection(
                        TsukumoPhase2Bullet.Direction.RIGHT,
                        new Vector2(300,-215)
                        ));
            }

            //  0.5秒のディレイをかける
            yield return new WaitForSeconds(0.5f);

            /************************/
            /* 左方向のセットアップ */
            /************************/

            //  子鬼を生成
            GameObject obj_left = Instantiate(bullet,pos,Quaternion.identity);
            TsukumoPhase2Bullet bulletComp_left =  obj_left.GetComponent<TsukumoPhase2Bullet>();
            bulletComp_left.SetPower(enemyData.Attack);
            bulletComp_left.SetDirection(TsukumoPhase2Bullet.Direction.LEFT);

            //  子鬼が突撃する
            StartCoroutine(bulletComp_left.BulletMove());

            /************************/
            /* 右方向のセットアップ */
            /************************/

            //  子鬼を生成
            GameObject obj_right = Instantiate(bullet,pos_mirror,Quaternion.identity);
            TsukumoPhase2Bullet bulletComp_right =  obj_right.GetComponent<TsukumoPhase2Bullet>();
            bulletComp_right.SetPower(enemyData.Attack);
            bulletComp_right.SetDirection(TsukumoPhase2Bullet.Direction.RIGHT);

            //  子鬼が突撃する
            yield return StartCoroutine(bulletComp_right.BulletMove());

            //  リセット
            buelletNum[fourDirection] = -1;
            buelletNum[fourDirection_mirror] = -1;


        }
        yield return null;
    }
}
