using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UI;
using static EnemyManager;

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

    //  弾オブジェクトのリスト
    List<GameObject> bulletList;
    //  弾オブジェクトのコピーリスト
    List<GameObject> copyBulletList;


    //------------------------------------------------------------
    //  Phase2用
    //------------------------------------------------------------
    private GameObject warningObject;

    private bool bWarningFirst;

    //  WARNING時の予測ライン
    private GameObject[] dangerLineObject;

    //  ギミック弾の使用済み番号格納用
    private int[] buelletNum = new int[(int)TsukumoPhase2Bullet.Direction.MAX];


    //------------------------------------------------------------
    //  Phase2用
    //------------------------------------------------------------


    //------------------------------------------------------------
    //  Phase3用
    //------------------------------------------------------------
    TsukumoPhase3Bullet enemyPhase3Bullet;

    //  コルーチン停止用フラグ
    Coroutine phase1_Coroutine;
    Coroutine phase2_Coroutine;
    private bool bStopPhase1;
    private bool bStopPhase2;

    private const float phase1_end = 0.66f;   //  フェーズ１終了条件のHP割合の閾値
    private const float phase2_end = 0.33f;   //  フェーズ２終了条件のHP割合の閾値





    void Start()
    {
        //  警告オブジェクトを取得;
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
        //  Phase3の弾クラス初期化
        enemyPhase3Bullet = null;
        //  弾のリスト
        bulletList = new List<GameObject>();
        //※単純に代入すると参照渡し（変更が相互に作用する）になりコピーの意味がなくなるが、
        //　こうやってコンストラクタの引数でリストを渡せば値渡し（同じデータを持つ別物）になる
        copyBulletList = new List<GameObject>(bulletList);

        //  行動開始
        StartCoroutine(WaitTsukumoAction(1));
    }

    private void OnDestroy()
    {
        Debug.Log("ボス撃破！ステージクリア！");

        //  弾を全削除
        DeleteAllBullet();

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

        //  弾リストを監視して空なら削除        {
        DeleteBulletFromList();
        
        //  スライダーを更新
        hpSlider.value = hp / enemyData.Hp;

        //  Phase3用の座標更新
        if(enemyPhase3Bullet != null)
        {
            enemyPhase3Bullet.SetParentTransform(this.transform);
        }
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

    //------------------------------------------------
    //  オブジェクトが空になっていたらリストから削除
    //------------------------------------------------
    public void DeleteBulletFromList()
    {
        //  コピーでループを回す
        foreach(GameObject bullet in copyBulletList)
        {
            if(bullet == null)
            {
                //  本体のリストから削除
                bulletList.Remove(bullet);
            } 
        }
    }

    //------------------------------------------------
    //  弾を全削除
    //------------------------------------------------
    public void DeleteAllBullet()
    {
        foreach(GameObject obj in bulletList)
        {
            if(obj)Destroy(obj);
        }
    }

    //------------------------------------------------
    //  リストに敵オブジェクトを追加
    //------------------------------------------------
    public void AddBulletFromList(GameObject obj)
    {
        if(obj != null)
        {
            bulletList.Add(obj);
        }
        else
        {
            Debug.LogError("空のオブジェクトが引数に指定されています！");
        }
    }

    //******************************************************************
    //
    //  ツクモの移動パターン
    //
    //******************************************************************

    //------------------------------------------------------------------
    //  数秒待ってからツクモの行動開始
    //------------------------------------------------------------------
    private IEnumerator WaitTsukumoAction(float duration)
    {
        yield return new WaitForSeconds(duration);

        //  行動開始
        StartCoroutine(StartTsukumoAction());
    }

    //------------------------------------------------------------------
    //  ツクモの行動管理関数
    //------------------------------------------------------------------
    private IEnumerator StartTsukumoAction()
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
        StartCoroutine(Tsukumo_Phase3());
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

    /// <summary>
    ///  ツクモのPhase1
    /// </summary>
    private IEnumerator Tsukumo_Phase1()
    {
        Debug.Log("フェーズ１開始");

        //  フェーズ１
        while (!bStopPhase1)
        {
            yield return StartCoroutine(Tsukumo_LoopMove(1.5f, 0.5f));
            yield return StartCoroutine(Shot());



            //yield return StartCoroutine(Warning());
            //StartCoroutine(WildlyShot(7.0f));
            //yield return StartCoroutine(ShoujiKekkai());
            //yield return StartCoroutine(Tsukumo_LoopMove(1.0f,1.0f));



            //StartCoroutine(Tsukumo_BerserkFireworks());
            //yield return StartCoroutine(GenerateBerserkBullet());
            //yield return StartCoroutine(Tsukumo_LoopMoveBerserk(0.6f, 3.0f));
            //StartCoroutine(GenerateBerserkBullet());
            //StartCoroutine(SummonDolls());
            //yield return StartCoroutine(WildlyShot(9.0f));
            //yield return StartCoroutine(Tsukumo_MoveToCenter());
        }
    }

    /// <summary>
    ///  ツクモのPhase2
    /// </summary>
    private IEnumerator Tsukumo_Phase2()
    {
        Debug.Log("フェーズ２へ移行");

        //  フェーズ２
        while (!bStopPhase2)
        {
            //yield return StartCoroutine(Tsukumo_LoopMove(1.5f, 0.5f));
            //yield return StartCoroutine(Shot());




            //  Warning!(初回のみ)
            yield return StartCoroutine(Warning());
            StartCoroutine(WildlyShot(7.0f));
            yield return StartCoroutine(ShoujiKekkai());
            yield return StartCoroutine(Tsukumo_LoopMove(1.0f, 1.0f));
        }
    }

    /// <summary>
    ///  ツクモのPhase3
    /// </summary>
    private IEnumerator Tsukumo_Phase3()
    {
        Debug.Log("フェーズ３へ移行");

        //  フェーズ３
        while (true)
        {
            StartCoroutine(Tsukumo_BerserkFireworks());
            yield return StartCoroutine(GenerateBerserkBullet());
            yield return StartCoroutine(Tsukumo_LoopMoveBerserk(0.6f, 3.0f));
            StartCoroutine(GenerateBerserkBullet());
            StartCoroutine(SummonDolls());
            yield return StartCoroutine(WildlyShot(9.0f));
            yield return StartCoroutine(Tsukumo_MoveToCenter());
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
    //  ツクモの真ん中への移動
    //------------------------------------------------------------------
    private IEnumerator Tsukumo_MoveToCenter()
    {
        float duration = 1.0f;   // 移動にかかる時間
        int controlPointId = 1;  // 中央のコントロールポイント

        //  ド真ん中のコントロールポイントを目標とする
        Vector3 targetPos = EnemyManager.Instance.GetControlPointPos(controlPointId);

        //  移動開始
        transform.DOMove(targetPos, duration);

        //  移動時間待つ
        yield return new WaitForSeconds(duration);
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
        yield return StartCoroutine(WildlyShot(7.0f));

        //yield return StartCoroutine(SnipeShot());

        //yield return StartCoroutine(OriginalShot());

        StartCoroutine(FlowShouji());
        StartCoroutine(FlowShouji());
        StartCoroutine(FlowShouji());
        StartCoroutine(FlowShouji());
        
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
                //  リストに追加
                AddBulletFromList(Bullet_obj);

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
    private IEnumerator WildlyShot(float speed)
    {
        //  通常バラマキ弾のプレハブを取得
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Wildly_Big);

        float totalDegree = 180;        //  撃つ範囲の総角  
        int wayNum = 15;                //  弾のway数(必ず3way以上の奇数にすること)
        float Degree = totalDegree / (wayNum-1);     //  弾一発毎にずらす角度         
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
            //  リストに追加
            AddBulletFromList(Bullet_obj);

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
            //  リストに追加
            AddBulletFromList(Bullet_obj);

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
            //  リストに追加
            AddBulletFromList(Bullet_objR);

            EnemyBullet enemyBulletR = Bullet_objR.GetComponent<EnemyBullet>();
            enemyBulletR.SetSpeed(speed);
            enemyBulletR.SetVelocity(vector[i]);
            enemyBulletR.SetPower(enemyData.Attack);

            vector[i] = Quaternion.Euler(0, 0, (i * Degree)) * vectorL;
            vector[i].z = 0f;

            GameObject Bullet_objL = 
                (GameObject)Instantiate(bullet, transform.position, transform.rotation);
            //  リストに追加
            AddBulletFromList(Bullet_objL);

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
                //  リストに追加
                AddBulletFromList(Bullet_obj);

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
        float chainInterval = 2f;       //  連弾の間隔（秒）
        float Interval = 3f;            //  次の行動までの間隔（秒）

        //  目標のコントロールポイント番号格納用
        List<int> targetNum = new List<int>();

        //  3～8までセット
        for(int i=3;i<3+(wayNum+1);i++)
        {
            targetNum.Add(i);
        }

        /* 弾生成する */
        for (int i = 0; i < wayNum; i++)
        {
            //  弾インスタンスを取得し、初速と発射角度を与える
            GameObject Bullet_obj =
                (GameObject)Instantiate(bullet, transform.position, transform.rotation);
            //  リストに追加
            AddBulletFromList(Bullet_obj);

            //  弾にデフォルトでEnemyBulletコンポーネントがあるのでそれを削除する
            Destroy(Bullet_obj.GetComponent<EnemyBullet>());

            //  代わりにTsukumoHomingBulletコンポーネントを追加する
            Bullet_obj.AddComponent<TsukumoHomingBullet>();

            //  3～8番までを重複なしでランダムに抽出
            if(targetNum.Count > 6/*目標座標の数*/-wayNum)
            {
                int index = Random.Range(0, targetNum.Count);
 
                int ransu = targetNum[index];

                //  弾に目標番号をセット
                Bullet_obj.GetComponent<TsukumoHomingBullet>().SetTargetNum(ransu);
 
                targetNum.RemoveAt(index);
            }

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

        yield return new WaitForSeconds(Interval);;
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

    /// <summary>
    /// Phase2:ランダムなスポナーから障子が流れてくる
    /// </summary>
    private IEnumerator FlowShouji()
    {
        int fourDirection = -1;

        while (true)
        {
            //  まずは４方向で抽選
            fourDirection = Random.Range(0, 4);

            //  番号が使用済みではなかったら
            if (buelletNum[fourDirection] == -1)
            {
                //  今回の番号を記録
                buelletNum[fourDirection] = fourDirection;
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

            //  ３箇所で抽選
            int rand = Random.Range(0, 3);
            Vector3 pos = default;
            if (rand == 0)       //  左
            {
                pos = EnemyManager.Instance.GetSpawnerPos(0);
            }
            else if (rand == 1)  //  中
            {
                pos = EnemyManager.Instance.GetSpawnerPos(1);
            }
            else if (rand == 2)  //  右
            {
                pos = EnemyManager.Instance.GetSpawnerPos(2);
            }

            //  0.5秒のディレイをかける
            yield return new WaitForSeconds(0.5f);

            //  子鬼を生成
            GameObject obj = Instantiate(bullet, pos, Quaternion.identity);
            //  リストに追加
            AddBulletFromList(obj);

            TsukumoPhase2Bullet bulletComp = obj.GetComponent<TsukumoPhase2Bullet>();
            if(bulletComp == null)Debug.LogError("TsukumoPhase2Bulletが取得できません！");
            bulletComp.SetPower(enemyData.Attack);
            bulletComp.SetDirection(TsukumoPhase2Bullet.Direction.TOP);

            //  子鬼が突撃する
            yield return StartCoroutine(bulletComp.BulletMove());

            //  リセット
            buelletNum[fourDirection] = -1;
        }
        else if (fourDirection == 1)    //  下方向とする
        {
            //  ギミック弾のプレハブを取得
            GameObject bullet = EnemyManager.Instance
                .GetBulletPrefab((int)BULLET_TYPE.Tsukumo_Gimmick_Bottom);
            //  ３箇所で抽選
            int rand = Random.Range(0, 3);
            Vector3 pos = default;
            if (rand == 0)       //  左
            {
                pos = EnemyManager.Instance.GetSpawnerPos(8);
            }
            else if (rand == 1)  //  中
            {
                pos = EnemyManager.Instance.GetSpawnerPos(7);
            }
            else if (rand == 2)  //  右
            {
                pos = EnemyManager.Instance.GetSpawnerPos(6);
            }

            //  0.5秒のディレイをかける
            yield return new WaitForSeconds(0.5f);

            //  子鬼を生成
            GameObject obj = Instantiate(bullet, pos, Quaternion.identity);
            //  リストに追加
            AddBulletFromList(obj);

            TsukumoPhase2Bullet bulletComp = obj.GetComponent<TsukumoPhase2Bullet>();
            if(bulletComp == null)Debug.LogError("TsukumoPhase2Bulletが取得できません！");
            bulletComp.SetPower(enemyData.Attack);
            bulletComp.SetDirection(TsukumoPhase2Bullet.Direction.BOTTOM);

            //  子鬼が突撃する
            yield return StartCoroutine(bulletComp.BulletMove());

            //  リセット
            buelletNum[fourDirection] = -1;
        }
        else if (fourDirection == 2)    //  左方向とする
        {
            //  ギミック弾のプレハブを取得
            GameObject bullet = EnemyManager.Instance
                .GetBulletPrefab((int)BULLET_TYPE.Tsukumo_Gimmick_Left);
            //  ３箇所で抽選
            int rand = Random.Range(0, 3);
            Vector3 pos = default;
            if (rand == 0)       //  上
            {
                pos = EnemyManager.Instance.GetSpawnerPos(11);

                //  座標をセット
                //pos2 = new Vector2(-560,190);
            }
            else if (rand == 1)  //  中
            {
                pos = EnemyManager.Instance.GetSpawnerPos(10);

                //  座標をセット
                //pos2 = new Vector2(-560,-18);
            }
            else if (rand == 2)  //  下
            {
                pos = EnemyManager.Instance.GetSpawnerPos(9);

                //  座標をセット
                //pos2 = new Vector2(-560,-215);
            }

            //  0.5秒のディレイをかける
            yield return new WaitForSeconds(0.5f);

            //  子鬼を生成
            GameObject obj = Instantiate(bullet, pos, Quaternion.identity);
            //  リストに追加
            AddBulletFromList(obj);

            TsukumoPhase2Bullet bulletComp = obj.GetComponent<TsukumoPhase2Bullet>();
            if(bulletComp == null)Debug.LogError("TsukumoPhase2Bulletが取得できません！");
            bulletComp.SetPower(enemyData.Attack);
            bulletComp.SetDirection(TsukumoPhase2Bullet.Direction.LEFT);

            //  子鬼が突撃する
            yield return StartCoroutine(bulletComp.BulletMove());

            //  リセット
            buelletNum[fourDirection] = -1;
        }
        else if (fourDirection == 3)    //  右方向とする
        {
            //  ギミック弾のプレハブを取得
            GameObject bullet = EnemyManager.Instance
                .GetBulletPrefab((int)BULLET_TYPE.Tsukumo_Gimmick_Right);
            //  ３箇所で抽選
            int rand = Random.Range(0, 3);
            Vector3 pos = default;
            if (rand == 0)       //  上
            {
                pos = EnemyManager.Instance.GetSpawnerPos(3);

                //  座標をセット
                //pos2 = new Vector2(440,190);
            }
            else if (rand == 1)  //  中
            {
                pos = EnemyManager.Instance.GetSpawnerPos(4);

                //  座標をセット
                //pos2 = new Vector2(440,-18);
            }
            else if (rand == 2)  //  下
            {
                pos = EnemyManager.Instance.GetSpawnerPos(5);

                //  座標をセット
                //pos2 = new Vector2(440,-215);
            }

            //  0.5秒のディレイをかける
            yield return new WaitForSeconds(0.5f);

            //  子鬼を生成
            GameObject obj = Instantiate(bullet, pos, Quaternion.identity);
            //  リストに追加
            AddBulletFromList(obj);

            TsukumoPhase2Bullet bulletComp = obj.GetComponent<TsukumoPhase2Bullet>();
            if(bulletComp == null)Debug.LogError("TsukumoPhase2Bulletが取得できません！");
            bulletComp.SetPower(enemyData.Attack);
            bulletComp.SetDirection(TsukumoPhase2Bullet.Direction.RIGHT);

            //  子鬼が突撃する
            yield return StartCoroutine(bulletComp.BulletMove());

            //  リセット
            buelletNum[fourDirection] = -1;
        }

        yield return null;
    }
    /// <summary>
    /// Phase2:障子の二重結界
    /// </summary>
    private IEnumerator ShoujiKekkai()
    {
        //  通常自機狙い弾のプレハブを取得
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Tsukumo_Gimmick);

        float totalDegree = 360;                 //  撃つ範囲の総角  
        int wayNum = 4;                          //  弾のway数
        float Degree = totalDegree / wayNum;     //  弾一発毎にずらす角度     
        float wait_time = 5f;                    //  弾発射後の待ち時間（秒）            

        /***************************************************************
            弾を生成してプレイヤーの四方に配置する
         ***************************************************************/
        //  敵の前方ベクトルを取得
        Vector3 centerPos = GameManager.Instance.GetPlayer().transform.position;
        Vector3 pos2 = centerPos + new Vector3(0,-1,0);
        Vector3 vector0 = (pos2 - centerPos).normalized;
        Vector3[] vector = new Vector3[wayNum];
        float distance = 3.0f;

        for (int i = 0; i < wayNum; i++)
        {
            vector[i] = Quaternion.Euler
                (0, 0, Degree*i) * vector0;
            vector[i].z = 0f;

            //弾インスタンスを取得し、オブジェクトを生成
            GameObject Bullet_obj = 
                (GameObject)Instantiate(bullet, transform.position, transform.rotation);
            //  リストに追加
            AddBulletFromList(Bullet_obj);

            TsukumoPhase2Bullet_B enemyBullet
                = Bullet_obj.GetComponent<TsukumoPhase2Bullet_B>();

            //  向きを修正
            Bullet_obj.transform.rotation
                = Quaternion.Euler(0, 0, Degree * i) * Bullet_obj.transform.rotation;

            //  四方に配置する
            Bullet_obj.transform.position = centerPos + vector[i] * distance;

            //  情報をセット
            enemyBullet.SetDegree(Degree * i);
            enemyBullet.SetVelocity(vector[i]);
            enemyBullet.SetPower(enemyData.Attack);

            //  弾のフェードイン音
            if(i == 0)
            {
                //  発射SE再生
                SoundManager.Instance.PlaySFX(
                (int)AudioChannel.ENEMY_SHOT,
                (int)SFXList.SFX_ENEMY_SHOT);
            }
        }
        //  間で人形召喚
        StartCoroutine(SummonDolls());

        //  待つ
        yield return new WaitForSeconds(wait_time);
    }
    /// <summary>
    /// Phase2:人形召喚
    /// </summary>
    private IEnumerator SummonDolls()
    {
        int summonNum = 10;     //  召喚する数

        for(int i=0;i<summonNum;i++)
        {
            //  人形を生成してデータセット
            GameObject prefab = EnemyManager.Instance.GetEnemyPrefab((int)EnemyPattern.E01);

            //  人形オブジェクトを生成＆データをセット
            GameObject doll = EnemyManager.Instance.SetEnemy(prefab, transform.position);

            //  リストに追加
            AddBulletFromList(doll);
        
            //  moveTypeをRandomChargeにする
            doll.GetComponent<Enemy>().SetMoveType((int)MOVE_TYPE.RandomCharge);
        }

        yield return null;
    }
    //-------------------------------------------------------------------
    //  ツクモの発狂弾生成処理
    //------------------------------------------------------------------
    private IEnumerator GenerateBerserkBullet()
    {
        float speed = 7.0f;                     //  弾速
        float totalDegree = 360;                //  撃つ範囲の総角  
        int wayNum = 5;                         //  弾のway数
        float Degree = totalDegree / wayNum;    //  弾一発毎にずらす角度
        float bulletDistance = 2.0f;            //  プレイヤーと展開する弾の最大距離
        float duration = 2.5f;

        //  発狂弾のプレハブを取得
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Tsukumo_Berserk_Bullet);

        //  ツクモの前方ベクトルを取得
        Vector3 vector0 = -transform.up;

        Vector3[] vector = new Vector3[wayNum];

        //  弾を生成
        for (int i = 0; i < wayNum; i++)
        {
            GameObject Bullet_obj = Instantiate(bullet,transform.position,Quaternion.identity);
            //  リストに追加
            AddBulletFromList(Bullet_obj);

            enemyPhase3Bullet = Bullet_obj.GetComponent<TsukumoPhase3Bullet>();
            enemyPhase3Bullet.SetParentTransform(transform);

            //  ベクトルを角度で回す
            vector[i] = Quaternion.Euler
                (0, 0, Degree * i) * vector0 * bulletDistance;
            vector[i].z = 0f;

            //  弾速と攻撃力を設定
            enemyPhase3Bullet.SetVec(vector[i]);
            enemyPhase3Bullet.SetSpeed(speed);
            enemyPhase3Bullet.SetPower(enemyData.Attack);

            //  SEを再生
            SoundManager.Instance.PlaySFX(
                (int)AudioChannel.ENEMY_SHOT,
                (int)SFXList.SFX_TSUKUMO_SHOT1);
        }

        yield return new WaitForSeconds(duration);
    }

    //------------------------------------------------------------------
    //  ツクモの発狂移動
    //------------------------------------------------------------------
    private IEnumerator Tsukumo_LoopMoveBerserk(float duration,float interval)
    {
        int currentlNum = (int)Control.Left;       //  現在位置
        List<int> targetList = new List<int>();    //  目標位置候補リスト
        int targetNum = (int)Control.Right;        //  目標位置

        Vector3 vec = Vector3.down;     //  弾のベクトル

        //  現在位置を求める（一番近い位置とする）
        Vector3 p1 = EnemyManager.Instance.GetControlPointPos((int)Control.Left);
        Vector3 p2 = EnemyManager.Instance.GetControlPointPos((int)Control.Right);
        float d1 = Vector3.Distance(p1,this.transform.position);
        float d2 = Vector3.Distance(p2,this.transform.position);
        List<float> dList = new List<float>();
        dList.Clear();
        dList.Add(d1);
        dList.Add(d2);
        
        //  並び替え
        dList.Sort();

        if(dList[0] == d1)currentlNum = (int)Control.Left;
        if(dList[0] == d2)currentlNum = (int)Control.Right;

        //  リストをクリア
        targetList.Clear();

        //  目標の番号を設定
        if(currentlNum ==(int)Control.Left)
        {
            targetList.Add((int)Control.Right);
        }
        else if(currentlNum ==(int)Control.Right)
        {
            targetList.Add((int)Control.Left);
        }

        //  目標番号を設定
        targetNum = targetList[Random.Range(0, targetList.Count)];

        //  目標座標を取得
        Vector3 targetPos = EnemyManager.Instance.GetControlPointPos(targetNum);

        //  横移動開始
        transform.DOLocalMoveX(targetPos.x, duration)
            .SetEase(Ease.Linear);

        //  現在の番号を更新
        currentlNum = targetNum;

        //  次の移動まで待つ
        yield return new WaitForSeconds(interval);
    }

    //------------------------------------------------------------------
    //  ツクモの発狂花火弾幕
    //------------------------------------------------------------------
    private IEnumerator Tsukumo_BerserkFireworks()
    {
        //  発狂花火弾幕のプレハブを取得
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Wildly_Big);

        float totalDegree = 90;         //  撃つ範囲の総角  
        int wayNum = 5;                 //  弾のway数(必ず3way以上の奇数にすること)
        float Degree = totalDegree / (wayNum-1);     //  弾一発毎にずらす角度         
        float speed = 2.0f;             //  弾速

        //  敵の前方ベクトルを取得
        Vector3[] vector = new Vector3[wayNum];

        for (int i = 0; i < wayNum; i++)
        {
            Vector3 vector0 = Quaternion.Euler(0,0,-totalDegree/2) * -transform.up;

            vector[i] = Quaternion.Euler(0,0,Degree*i) * vector0;
            vector[i].z = 0f;

            //弾インスタンスを取得し、初速と発射角度を与える
            Vector3 center_pos = transform.position + vector[i] * 1.0f;
            int bulletNum = 18;             // 弾の数
            float deg = 360/bulletNum;     //  角度
            Vector3 dir = Quaternion.Euler(0,0,-deg/2) * -Vector3.up;
            

            //  弾一発ごとの処理
            for(int j=0;j<bulletNum;j++)
            {
                //  そこから60度ずつずらして配置する
                Vector3 bulletPos = center_pos + Quaternion.Euler(0,0,deg*j) * dir * 1.0f;

                //  オブジェクトを生成
                GameObject Bullet_obj = 
                Instantiate(bullet, bulletPos, Quaternion.identity);

                //  リストに追加
                AddBulletFromList(Bullet_obj.gameObject);

                //  進む方向を計算
                Vector3 direction = center_pos - bulletPos;
                direction.Normalize();

                //  EnemyBulletをデタッチして代わりにTsukumoFireworksを付与する
                Destroy(Bullet_obj.GetComponent<EnemyBullet>());
                TsukumoFireworks fw = Bullet_obj.AddComponent<TsukumoFireworks>();
                fw.SetSpeed(speed);
                fw.SetVelocity((vector[i] + direction));
                fw.SetPower(enemyData.Attack);
            }

            if(i == 0)
            {
                //  発射SE再生
                SoundManager.Instance.PlaySFX(
                (int)AudioChannel.ENEMY_SHOT,
                (int)SFXList.SFX_ENEMY_SHOT);
            }
        }

        yield return new WaitForSeconds(7);
    }

    //------------------------------------------------------------------
    //  自機狙い発狂ガトリングショット
    //------------------------------------------------------------------
    private IEnumerator Tsukumo_BerserkGatling()
    {
        //  弾のプレハブを取得
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Snipe_Big);

        int wayNum = 3;                 //  弾のway数
        float Degree = 20;              //  ずらす角度
        int chain = 2;                  //  連弾数         
        float speed = 8.0f;             //  弾速
        float chainInterval = 0.5f;     //  連弾の間隔（秒）

        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < wayNum; i++)
            {
                //  敵からプレイヤーへのベクトルを取得
                Vector3 playerPos = GameManager.Instance.GetPlayer().transform.position;
                Vector3 vector0 = (playerPos - transform.position).normalized;
                Vector3[] vector = new Vector3[wayNum];

                vector[i] = Quaternion.Euler( 0, 0, -Degree + i * Degree ) * vector0;
                vector[i].z = 0f;

                //弾インスタンスを取得し、初速と発射角度を与える
                GameObject Bullet_obj = 
                    (GameObject)Instantiate(bullet, transform.position, transform.rotation);
                //  リストに追加
                AddBulletFromList(Bullet_obj);

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

        //  3秒待つ
        yield return new WaitForSeconds(1.0f);

        yield return null;
    }
}
