using DG.Tweening;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

//--------------------------------------------------------------
//
//  ボスに基本のクラス
//
//--------------------------------------------------------------
public class BossBase : MonoBehaviour
{
    //  EnemyDataクラスからの情報取得用
    protected EnemyData enemyData;
    
    //  パラメータ
    protected string boss_id;         //  EnemyData検索ID
    protected float hp;
    protected bool bDeath;            //  死亡フラグ
    protected bool bSuperMode;        //  無敵モードフラグ
    protected bool bSuperModeInterval;//  フェーズ切り替え時の無敵モードフラグ

    //  点滅させるためのSpriteRenderer
    protected SpriteRenderer sp;
    //  点滅の間隔
    protected float flashInterval;
    //  点滅させるときのループカウント
    protected int loopCount;

    //  やられエフェクト
    [SerializeField,ShowAssetPreview]
    public GameObject explosion;

    //  HPスライダー
    protected Slider hpSlider;

    //  制御点
    protected enum Control
    {
        Left,
        Center,
        Right,

        Max
    };

    //  ドロップパワーアップアイテム一覧
    protected ePowerupItems powerupItems;

    //  弾オブジェクトのリスト
    protected List<GameObject> bulletList;
    //  弾オブジェクトのコピーリスト
    protected List<GameObject> copyBulletList;

    //  コルーチン停止用フラグ
    protected Coroutine phase1_Coroutine;
    protected Coroutine phase2_Coroutine;
    protected bool bStopPhase1;
    protected bool bStopPhase2;

    protected const float phase1_end = 0.66f;   //  フェーズ１終了条件のHP割合の閾値
    protected const float phase2_end = 0.33f;   //  フェーズ２終了条件のHP割合の閾値
    
    /// <summary>
    /// 初期化
    /// </summary>
    protected virtual void Awake()
    {
        boss_id = "";

        //  フラグ初期化
        bStopPhase1 = false;
        bStopPhase2 = false;

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

        //  弾のリスト
        bulletList = new List<GameObject>();

        //※単純に代入すると参照渡し（変更が相互に作用する）になりコピーの意味がなくなるが、
        //　こうやってコンストラクタの引数でリストを渡せば値渡し（同じデータを持つ別物）になる
        copyBulletList = new List<GameObject>(bulletList);
    }

    protected virtual void Start()
    {

    }

    /// <summary>
    /// 更新
    /// </summary>
    protected virtual void Update()
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
    }

    /// <summary>
    /// オブジェクトが破棄された時の処理
    /// </summary>
    protected void OnDestroy()
    {
        Debug.Log("ボス撃破！ステージクリア！");

        //  弾を全削除
        DeleteAllBullet();

        //  ボス戦やられたらステージクリア
        GameManager.Instance.SetStageClearFlag(true);
    }

    /// <summary>
    ///  敵のデータを設定 
    /// </summary>
    /// <param name="es">敵設定ファイル</param>
    /// <param name="item">ドロップアイテム</param>
    public void SetBossData(EnemySetting es, ePowerupItems item)
    {
        //  敵のデータを設定 
        if(boss_id == "")Debug.LogError("boss_idが空になっています！");
        else Debug.Log($"boss_id:{boss_id}で設定が完了しました！");


        Debug.Log($"boss_id: {boss_id}");


        //  IDでデータをenemyDataに設定
        enemyData = es.DataList
            .FirstOrDefault(enemy => enemy.Id == boss_id );
        if(enemyData == null)Debug.LogError("enemyDataの取得に失敗しました" +
            "！\nボス派生クラスのboss_idを確認してください");
        else Debug.Log("enemyDataの設定が完了しました！");


        //  体力を設定
        if(enemyData.Hp <= 0)Debug.LogError("ボスHPが最初から0以下に設定されています！");
        else Debug.Log("ボスのHPの設定が完了しました！");
        hp = enemyData.Hp;

        Debug.Log( "タイプ: " + boss_id + "\nHP: " + hp );

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
    protected IEnumerator PlayDamageSFXandSuperModeOff()
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
    protected void OnTriggerEnter2D(Collider2D collision)
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
    protected void Death()
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
    protected void Death2()
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
    //  移動パターン
    //
    //******************************************************************

    //------------------------------------------------------------------
    //  行動管理関数
    //------------------------------------------------------------------
    protected IEnumerator StartAction()
    {
        Debug.Log("***弾幕フェーズ開始！***");

        //  フェーズ１開始
        phase1_Coroutine = StartCoroutine(Phase1());

        //  フラグがTRUEになるまで待つ
        yield return new WaitUntil(()=>bStopPhase1 == true);

        //  フラグでコルーチン停止
        if(phase1_Coroutine != null)StopCoroutine(phase1_Coroutine);

        //  フェーズ変更
        yield return StartCoroutine(PhaseChange());

        //  フェーズ２開始
        phase2_Coroutine = StartCoroutine(Phase2());

        //  フラグがTRUEになるまで待つ
        yield return new WaitUntil(()=>bStopPhase2 == true);

        //  フラグでコルーチン停止
        if(phase2_Coroutine != null)StopCoroutine(phase2_Coroutine);

        //  フェーズ変更
        yield return StartCoroutine(PhaseChange());

        //  フェーズ３開始
        StartCoroutine(Phase3());
    }

    //------------------------------------------------------------------
    //  フェーズチェンジ時の行動
    //------------------------------------------------------------------
    protected IEnumerator PhaseChange()
    {
        //  移動にかかる時間(秒)
        float duration = 1.5f;
        //  移動後に待機する時間(秒)
        float interval = 5.0f;

        //  真ん中に移動する
        yield return StartCoroutine(MoveToCenterOnPhaseChange(duration));

        //  次のフェーズまで待つ
        yield return new WaitForSeconds(interval);

        //  無敵モードOFF
        bSuperModeInterval = false;
    }

    /// <summary>
    ///  ツクモのPhase1
    /// </summary>
    protected virtual IEnumerator Phase1()
    {
        yield return null;
    }

    /// <summary>
    ///  ツクモのPhase2
    /// </summary>
    protected virtual IEnumerator Phase2()
    {
        yield return null;
    }

    /// <summary>
    ///  ツクモのPhase3
    /// </summary>
    protected virtual IEnumerator Phase3()
    {
        yield return null;
    }

    //------------------------------------------------------------------
    //  移動
    //------------------------------------------------------------------
    protected virtual IEnumerator LoopMove(float duration,float interval)
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
    //  真ん中への移動
    //------------------------------------------------------------------
    protected virtual IEnumerator MoveToCenter()
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
    //  フェーズの切り替え時にボスが真ん中に移動する
    //------------------------------------------------------------------
    protected virtual IEnumerator MoveToCenterOnPhaseChange(float duration)
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
    //  通常攻撃パターン(Phase1)
    //------------------------------------------------------------------
    protected virtual IEnumerator Shot()
    {
        yield return null;        
    }

    //------------------------------------------------------------------
    //  バラマキ弾(小)
    //------------------------------------------------------------------
    protected virtual IEnumerator WildlyShotSmall()
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
    protected virtual IEnumerator WildlyShot(float speed)
    {
        yield return null;
    }

    //------------------------------------------------------------------
    //  自機狙い弾
    //------------------------------------------------------------------
    protected virtual IEnumerator SnipeShot()
    {
        yield return null;
    }

    //------------------------------------------------------------------
    //  オリジナル弾・ホーミングショット
    //------------------------------------------------------------------
    protected virtual IEnumerator OriginalShot()
    {
        yield return null;
    }

    //-------------------------------------------------------------------
    //  発狂弾生成処理
    //------------------------------------------------------------------
    protected virtual IEnumerator GenerateBerserkBullet(float duration)
    {
        yield return null;
    }

    //------------------------------------------------------------------
    //  発狂移動
    //------------------------------------------------------------------
    protected virtual IEnumerator LoopMoveBerserk(float duration,float interval)
    {
        yield return null;
    }

    //------------------------------------------------------------------
    //  発狂ガトリングショット
    //------------------------------------------------------------------
    protected virtual IEnumerator BerserkGatling()
    {
        yield return null;
    }

}
