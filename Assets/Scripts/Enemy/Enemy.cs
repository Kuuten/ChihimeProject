using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using static EnemyManager;

// 動きの種類
[SerializeField]  enum MOVE_TYPE {
    None,                       // 未設定
    Appear,                     // 現れるだけ
    ClampL,                     // カクカク移動左スタート
    ClampR,                     // カクカク移動右スタート
    ClampRandom,                // カクカク移動ランダムスタート
    ChargeToPlayer,             // 直進して現れてプレイヤーに突進
    Charge,                     // プレイヤーに突進
    RandomCharge,               // 適当なスポナーからプレイヤーに突進
    Straight,                   // 直線移動
    AdjustLine,                 // 軸合わせ
    Curve,                      // 放物線
    OssanMove,                  // おっさんムーブ
    DollSiege,                  // 人形包囲

    CurveAndShoot,              //  放物線移動&弾を撃って帰っていく
    SnipeShot3way,              //  自機狙い弾を撃つ
    WildlyShot3way,             //  バラマキ弾を撃つ
    CounterShot3way,            //  ３wayの撃ち返し弾を撃つ

    MidBoss_Appearance,         //  中ボス登場
    MidBoss_MoveSide            //  中ボス左右繰り返し移動
}

//--------------------------------------------------------------
//
//  ザコ敵の基本クラス
//
//--------------------------------------------------------------
public class Enemy : MonoBehaviour
{
    // 動きの種類
    [SerializeField] private MOVE_TYPE moveType = MOVE_TYPE.None;

    // ザコ敵の種類(名前をEnemySettingのIdと同じにする)
    public enum ENEMY_TYPE {
        None,                       //  未設定
        Kooni,                      //  子鬼
        Onibi,                      //  鬼火
        Ibarakidouji,               //  茨木童子
        Douji,                      //  ドウジ
        Ossan,                      //  ちっちゃいおっさん

        Doll,                       //  人形
        Chouchin,                   //  提灯
        Nuigurumi,                  //  ぬいぐるみ
        Tsukumo,                    //  ツクモ
    }
    // ザコ敵の種類
    public ENEMY_TYPE enemyType = ENEMY_TYPE.None;

    // 中ボスかどうかの設定
    public enum IS_MID_BOSS {
        No,                      //  ザコ
        Yes,                     //  中ボス
    }
    public IS_MID_BOSS isMidBoss = IS_MID_BOSS.No;

    // 打ち返し弾の設定
    public enum REVENGE_SHOT {
        None,                       //  なし
        Enable,                     //  あり
    }
    public REVENGE_SHOT revengeShot = REVENGE_SHOT.None;

    //  EnemyDataクラスからの情報取得用
    EnemyData enemyData;
    
    //  パラメータ
    private float hp;
    private bool bDeath;        //  死亡フラグ
    private bool bSuperMode;    //  無敵モードフラグ
    
    //  やられエフェクト
    [SerializeField] private GameObject explosion;

    //  カメラに写っているかの判定用
    private bool visible = false;

    //  点滅させるためのSpriteRenderer
    SpriteRenderer sp;
    //  点滅の間隔
    private float flashInterval;
    //  点滅させるときのループカウント
    private int loopCount;

    //  HPスライダー
    private Slider hpSlider;

    //  フェーズ切り替えフラグ
    private bool bSwitchPhase;

    //  ドロップパワーアップアイテム一覧
    private ePowerupItems powerupItems;

    //---------------------------------------------
    //  個別処理用の変数
    //---------------------------------------------
    private float dollAnimSpeed;    //  人形のAnimator再生速度
    
    private bool dollRotateFlag;    //  人形包囲陣の回転フラグ


    private void Start()
    {
        //  カメラに写っていない
        visible = false;
        //  死亡フラグOFF
        bDeath = false;
        //  最初は無敵モードON
        bSuperMode = true;
        //  ループカウントを設定
        loopCount = 1;
        //  点滅の間隔を設定
        flashInterval = 0.2f;
        //  人形のAnimator再生速度
        dollAnimSpeed = 0.3f;
        //  人形包囲陣の突撃フラグ
        dollRotateFlag = false;
        //  フェーズ切り替えフラグ
        bSwitchPhase = false;


        //  敵情報のアサーション（満たさなければいけない条件）
        Assert.IsTrue(enemyType.ToString() != ENEMY_TYPE.None.ToString(),
            "EnemyTypeがインスペクターで設定されていません！");

        //  SpriteRenderを取得
        sp = GetComponent<SpriteRenderer>();
    }

    //  敵のデータを設定 
    public void SetEnemyData(EnemySetting es, ePowerupItems item)
    {
        //  敵のデータを設定 
        enemyData = es.DataList
            .FirstOrDefault(enemy => enemy.Id == enemyType.ToString() );

        //  体力を設定
        hp = enemyData.Hp;

        Debug.Log( "タイプ: " + enemyType.ToString() + "\nHP: " + hp );
        Debug.Log( enemyType.ToString() + "の設定完了" );

        //Debug.Log($"ID：{enemyData.Id}");
        //Debug.Log($"HP：{enemyData.Hp}");
        //Debug.Log($"攻撃力：{enemyData.Attack}");
        //Debug.Log($"落魂：{enemyData.Money}");

        if(item == ePowerupItems.None)
        {
            powerupItems = ePowerupItems.None;
        }
        else
        {
            //  ドロップアイテムを設定
            powerupItems = item;    
        }


    }

    private void OnDestroy()
    {
        //  敵をリストから削除
        EnemyManager.Instance.DeleteEnemyFromList(this.gameObject);

        //  破壊された敵数を+1
        int destroyNum = EnemyManager.Instance.GetDestroyNum();
        destroyNum++;
        EnemyManager.Instance.SetDestroyNum(destroyNum);
        Debug.Log("破壊された敵数 : " + destroyNum);

        //  中ボスがやられたらザコ戦を終了する
        if(isMidBoss == IS_MID_BOSS.Yes)
        {
            EnemyManager.Instance.SetEndZakoStage(true);
        }
    }

    void Update()
    {
        //  スライダーを更新
        if(hpSlider != null)hpSlider.value = hp / enemyData.Hp;

        //  ボスのフェーズ切り替え監視
        SwitchBossPhase();

        //  行動パターン
        switch(moveType)
        {
            case MOVE_TYPE.None:
                break;
            //  低級ザコ
            case MOVE_TYPE.Appear:
                SetCoroutine( Appear() );
                break;
            case MOVE_TYPE.ClampL:
                SetCoroutine( Clamp(true) );
                break;
            case MOVE_TYPE.ClampR:
                SetCoroutine( Clamp(false) );
                break;
            case MOVE_TYPE.ClampRandom:
                SetCoroutine( ClampRandom() );
                break;
            case MOVE_TYPE.ChargeToPlayer:
                SetCoroutine( ChargeToPlayer() );
                break;
            case MOVE_TYPE.Charge:
                SetCoroutine( Charge() );
                break;
            case MOVE_TYPE.RandomCharge:
                SetCoroutine( RandomPopAndCharge() );
                break;
            case MOVE_TYPE.Straight:
                SetCoroutine( Straight() );
                break;
            case MOVE_TYPE.AdjustLine:
                SetCoroutine( AdjustLine() );
                break;
            case MOVE_TYPE.Curve:
                SetCoroutine( Curve() );
                break;
            case MOVE_TYPE.OssanMove:
                SetCoroutine( OssanMove() );
                break;
            case MOVE_TYPE.DollSiege:   //  人形包囲陣（特殊処理）
                SetCoroutine( FadeInAndChargeToPlayer(3.0f) );
                break;

            //  中級ザコ
            case MOVE_TYPE.CurveAndShoot:
                SetCoroutine( CurveAndShootRandom() );
                break;
            case MOVE_TYPE.SnipeShot3way:
                SetCoroutine( SnipeShot3way() );
                break;
            case MOVE_TYPE.WildlyShot3way:
                SetCoroutine( WildlyShot3way() );
                break;
            case MOVE_TYPE.CounterShot3way:
                SetCoroutine( CounterShot3way() );
                break;
            //  中ボス
            case MOVE_TYPE.MidBoss_Appearance:
                SetCoroutine( MidBoss_Appearance() );
                break;
            case MOVE_TYPE.MidBoss_MoveSide:
                SetCoroutine( MidBoss_MoveSide() );
                break;
        }

        //  敵個別処理
        if(enemyType.ToString() == "Doll")
        {
             this.GetComponent<Animator>().SetFloat("AnimSpeed", dollAnimSpeed);

            //  突撃フラグがTrueなら回転する
            if(dollRotateFlag)AxisRotation();
        }

        ////  仮処理
        //Vector3 pos = transform.position;

        ////  前進
        //pos += new Vector3(
        //    0,
        //    moveSpeed * Time.deltaTime,
        //    0
        //    );
        //transform.position = pos;
    }

    //  Rendererがカメラに写った時点で呼ばれる
    private void OnBecameVisible()
    {
        visible = true;
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
    public void SetSwitchPhase(bool flag){ bSwitchPhase = flag; }
    public bool GetSwitchPhase(){ return bSwitchPhase; }
    public void SetMoveType(int type){ moveType = (MOVE_TYPE)type; }

    //------------------------------------------------------
    //  中ボス用のHPスライダーをセットする
    //------------------------------------------------------
    public void SetHpSlider(Slider slider)
    {
        hpSlider = slider;
    }

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
        if(!visible || bDeath)return;

        if (collision.CompareTag("DeadWall"))
        {
            if(bSuperMode)return;
            if(isMidBoss == IS_MID_BOSS.Yes)return;

            Destroy(this.gameObject);
        }
        else if (collision.CompareTag("NormalBullet"))
        {
            //  弾の消去
            Destroy(collision.gameObject);

            //  無敵モードなら弾だけ消して返す
            if(bSuperMode)return;

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
                //  撃ち返し弾が有効なら撃ち返し
                if(revengeShot == REVENGE_SHOT.Enable)
                {
                    if(isMidBoss == IS_MID_BOSS.Yes)
                    {
                        StartCoroutine(MidBoss_CounterShot8way());
                    }
                    else StartCoroutine(RevengeShot());
                }
                Death();                       //  やられ演出
            }
        }
        else if (collision.CompareTag("DoujiConvert"))
        {
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
                //  撃ち返し弾が有効なら撃ち返し
                if(revengeShot == REVENGE_SHOT.Enable)
                {
                    if(isMidBoss == IS_MID_BOSS.Yes)
                    {
                        StartCoroutine(MidBoss_CounterShot8way());
                    }
                    else StartCoroutine(RevengeShot());
                }
                Death2();                      //  やられ演出2
            }
        }
        else if (collision.CompareTag("DoujiKonburst"))
        {
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
                //  撃ち返し弾が有効なら撃ち返し
                if(revengeShot == REVENGE_SHOT.Enable)
                {
                    if(isMidBoss == IS_MID_BOSS.Yes)
                    {
                        StartCoroutine(MidBoss_CounterShot8way());
                    }
                    else StartCoroutine(RevengeShot());
                }
                Death2();                      //  やられ演出2
            }
        }
        else if (collision.CompareTag("TsukumoConvert"))
        {
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
                //  撃ち返し弾が有効なら撃ち返し
                if(revengeShot == REVENGE_SHOT.Enable)
                {
                    if(isMidBoss == IS_MID_BOSS.Yes)
                    {
                        StartCoroutine(MidBoss_CounterShot8way());
                    }
                    else StartCoroutine(RevengeShot());
                }
                Death2();                      //  やられ演出2
            }
        }
        else if (collision.CompareTag("TsukumoKonburst"))
        {
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
                //  撃ち返し弾が有効なら撃ち返し
                if(revengeShot == REVENGE_SHOT.Enable)
                {
                    if(isMidBoss == IS_MID_BOSS.Yes)
                    {
                        StartCoroutine(MidBoss_CounterShot8way());
                    }
                    else StartCoroutine(RevengeShot());
                }
                Death2();                      //  やられ演出2
            }
        }
        else if (collision.CompareTag("Bomb"))
        {
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
                //  撃ち返し弾が有効なら撃ち返し
                if(revengeShot == REVENGE_SHOT.Enable)
                {
                    if(isMidBoss == IS_MID_BOSS.Yes)
                    {
                        StartCoroutine(MidBoss_CounterShot8way());
                    }
                    else StartCoroutine(RevengeShot());
                }
                Death();                        //  やられ演出
            }
        }
    }


    //-------------------------------------------
    //  当たり判定を抜けた時の処理
    //-------------------------------------------
    private void OnTriggerExit2D(Collider2D collision)
    {
        //bSuperMode = false;
    }

    //-------------------------------------------
    //  撃ち返し処理
    //-------------------------------------------
    public IEnumerator RevengeShot()
    {
         SetCoroutine( CounterShot3way() );

        yield return null;
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
        //if(super)bSuperMode = false;
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
        if (drop) drop.DropKon(enemyData.Money);

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
    //  ザコ敵の移動パターン
    //
    //******************************************************************

    private void SetCoroutine(IEnumerator func)
    {
        StartCoroutine(func);
        moveType = MOVE_TYPE.None;
    }

    //------------------------------------------------------------------
    //  指定座標まで直線移動する
    //------------------------------------------------------------------
    private IEnumerator LineMove(Vector3 pos, float duration)
    {
        transform.DOMove(pos, duration)
            .SetEase(Ease.Linear)
            .WaitForCompletion();

        yield return null;
    }

    //------------------------------------------------------------------
    //  現在位置から指定距離だけ直線移動する
    //------------------------------------------------------------------
    private IEnumerator LineMoveDistance(Vector3 distance, float duration)
    {
        transform.DOMove(distance, duration)
            .SetRelative(true)
            .SetEase(Ease.Linear)
            .WaitForCompletion();

        yield return null;
    }

    //------------------------------------------------------------------
    //  直進で登場する(デバッグ用など)
    //------------------------------------------------------------------
    private IEnumerator Appear()
    {
        const int Middle = 7;
        int Position = Middle;

        int rand = UnityEngine.Random.Range(0,3);

        float d = 5.0f;         //  移動距離
        float duration = 1.0f;  // 時間
        Vector3 dintance = new Vector3(0,d,0);

        //  スポナー[7]から出現
        Vector3 start = EnemyManager.Instance.GetSpawnerPos(Position);
        transform.position = start;

        //  一定距離直進
        yield return StartCoroutine(
            LineMoveDistance( dintance, duration ));

        //  処理時間待つ
        yield return new WaitForSeconds(duration);

        //  無敵モードOFF
        bSuperMode = false;

        yield return null;
    }

    //------------------------------------------------------------------
    //  上以外のランダムなスポナーから現れてプレイヤーに突撃
    //------------------------------------------------------------------
    private IEnumerator RandomPopAndCharge(ePowerupItems item = ePowerupItems.None)
    {
        //  [3]〜[11]のスポナーから出現する
        int rand = UnityEngine.Random.Range(3,12);
        Vector3 start = EnemyManager.Instance.GetSpawnerPos(rand);
        transform.position = start;

        //----------------------------------------------------------------------
        //  プレイヤーに突撃
        //----------------------------------------------------------------------
        float d = 5.0f;         //  移動距離
        float speed2 = 5.0f;    //  移動スピード２
        Vector3 dintance = new Vector3(0,d,0);

        //  敵の種類が人形の時はAnimatorスピードを1.5にする
        if (enemyType == ENEMY_TYPE.Doll)
        {
            dollAnimSpeed = 1.5f;
        }

        //  プレイヤーへのベクトルを計算
        Vector3 playerPos = GameManager.Instance.GetPlayer().transform.position;
        Vector3 vec = playerPos - transform.position;
        vec.Normalize();

        //  敵の種類が人形の時プレイヤーの方向へ向ける
        if (enemyType == ENEMY_TYPE.Doll)
        {
            //  上方向とプレイヤーへのベクトルの角度を計算(度)
            float angle = Vector3.SignedAngle(Vector3.up, vec, Vector3.forward);

            //  プレイヤーの方向に向ける
            this.transform.DOLocalRotate(new Vector3(0,0,angle),0f);
        }

        //  その方向にでたらめな距離伸ばした座標に移動する
        float dist = 30.0f;
        vec = vec * dist;

        //  一定距離直進
        StartCoroutine(LineMoveDistance( vec, vec.magnitude/speed2 ));

        //  画面内に入るまで待つ
        yield return new WaitForSeconds(0.5f);

        //  無敵モードOFF
        bSuperMode = false;

        //  処理時間待つ
        yield return new WaitForSeconds(vec.magnitude/speed2);
    }
    //------------------------------------------------------------------
    //  クランプ移動（スタート位置を左右切り替え可能）
    //------------------------------------------------------------------
    private IEnumerator Clamp(bool startFromLeft)
    {
        //  無敵モードOFF
        bSuperMode = false;

        //  開始点
        Vector3 start = Vector3.zero;

        //  経路は10点
        Vector3[] points = new Vector3[10];

        //  移動にかかる時間
        float duration = 2.0f;

        //  引数によって開始位置を初期化する
        if(startFromLeft)
        {
            start = EnemyManager.Instance.GetSpawnerPos(8);
            points[0] = EnemyManager.Instance.GetControlPointPos(6);
            points[1] = EnemyManager.Instance.GetControlPointPos(7);
            points[2] = EnemyManager.Instance.GetControlPointPos(8);
            points[3] = EnemyManager.Instance.GetControlPointPos(5);
            points[4] = EnemyManager.Instance.GetControlPointPos(4);
            points[5] = EnemyManager.Instance.GetControlPointPos(3);
            points[6] = EnemyManager.Instance.GetControlPointPos(0);
            points[7] = EnemyManager.Instance.GetControlPointPos(1);
            points[8] = EnemyManager.Instance.GetControlPointPos(2);
            points[9] = EnemyManager.Instance.GetSpawnerPos(2);
        }
        else
        {
            start = EnemyManager.Instance.GetSpawnerPos(6);
            points[0] = EnemyManager.Instance.GetControlPointPos(8);
            points[1] = EnemyManager.Instance.GetControlPointPos(7);
            points[2] = EnemyManager.Instance.GetControlPointPos(6);
            points[3] = EnemyManager.Instance.GetControlPointPos(3);
            points[4] = EnemyManager.Instance.GetControlPointPos(4);
            points[5] = EnemyManager.Instance.GetControlPointPos(5);
            points[6] = EnemyManager.Instance.GetControlPointPos(2);
            points[7] = EnemyManager.Instance.GetControlPointPos(1);
            points[8] = EnemyManager.Instance.GetControlPointPos(0);
            points[9] = EnemyManager.Instance.GetSpawnerPos(0);
        }

        //  開始座標を設定
        transform.position = start;

        Debug.Log("初期座標設定完了！");

        for(int i = 0; i < points.Length;i++)
        {
            yield return StartCoroutine(LineMove( points[i], duration ));

            yield return new WaitForSeconds(duration);
        }

        yield return null;
    }

    //  ランダム
    private IEnumerator ClampRandom()
    {
        //  無敵モードOFF
        bSuperMode = false;

        bool left = Convert.ToBoolean( UnityEngine.Random.Range(0,2) );

        SetCoroutine(Clamp(left));

        yield return null;
    }
    //------------------------------------------------------------------
    //  スポナー[6][7][8]からランダムに出現して一定距離進む
    //------------------------------------------------------------------
    private IEnumerator AppearFromSpawnerAndWalk()
    {
        const int Right = 6;
        const int Middle = 7;
        const int Left = 8;
        int[] Position = new int[3];
        Position[0] = Right;
        Position[1] = Middle;
        Position[2] = Left;

        int rand = UnityEngine.Random.Range(0,3);

        float d = 5.0f;         //  移動距離
        Vector3 dintance = new Vector3(0,d,0);

        //  スポナー[6][7][8]からランダムに出現
        Vector3 start = EnemyManager.Instance.GetSpawnerPos(Position[rand]);
        transform.position = start;

        //  一定距離直進
        yield return StartCoroutine(
            LineMoveDistance( dintance, 1.0f ));

        //  処理時間待つ
        yield return new WaitForSeconds(1.0f);

        //  無敵モードOFF
        bSuperMode = false;
    }
    //------------------------------------------------------------------
    //  自機狙いで突っ込む
    //------------------------------------------------------------------
    private IEnumerator Charge()
    {
        float d = 5.0f;         //  移動距離
        float speed2 = 5.0f;    //  移動スピード２
        Vector3 dintance = new Vector3(0,d,0);

        //  無敵モードOFF
        bSuperMode = false;

        //  敵の種類が人形の時はAnimatorスピードを1.5にする
        if (enemyType == ENEMY_TYPE.Doll)
        {
            dollAnimSpeed = 1.5f;
        }

        //  2秒待つ
        yield return new WaitForSeconds(2.0f);

        //  プレイヤーへのベクトルを計算
        Vector3 playerPos = GameManager.Instance.GetPlayer().transform.position;
        Vector3 vec = playerPos - transform.position;
        vec.Normalize();

        //  敵の種類が人形の時プレイヤーの方向へ向ける
        if (enemyType == ENEMY_TYPE.Doll)
        {
            //  上方向とプレイヤーへのベクトルの角度を計算(度)
            float angle = Vector3.SignedAngle(Vector3.up, vec, Vector3.forward);

            //  プレイヤーの方向に向ける
            this.transform.DOLocalRotate(new Vector3(0,0,angle),0f);
        }

        //  その方向にでたらめな距離伸ばした座標に移動する
        float dist = 30.0f;
        vec = vec * dist;

        //  一定距離直進
        yield return StartCoroutine(
            LineMoveDistance( vec, vec.magnitude/speed2 ));

        //  処理時間待つ
        yield return new WaitForSeconds(vec.magnitude/speed2);

        yield return null;
    }
    //------------------------------------------------------------------
    //  一定距離まで直進の後、自機狙いで突っ込む
    //------------------------------------------------------------------
    private IEnumerator ChargeToPlayer()
    {
        yield return StartCoroutine(AppearFromSpawnerAndWalk());

        yield return StartCoroutine(Charge());

    }
    //------------------------------------------------------------------
    //  フェードインで現れる(フェードインにかかる時間)
    //------------------------------------------------------------------
    private IEnumerator FadeInAndChargeToPlayer(float duration)
    {
        //  フェードイン
        this.GetComponent<SpriteRenderer>().DOFade(1.0f,3.0f);

        //  ３秒待つ
        yield return new WaitForSeconds(duration);

        //  ヒットボックス有効化
        this.GetComponent<BoxCollider2D>().enabled = true;

        //  無敵モードOFF
        bSuperMode = false;

        //  回転開始する
        dollRotateFlag = true;

        //  回転時間待つ
        yield return new WaitForSeconds(duration + 2.0f);

        //  回転を止める
        dollRotateFlag = false;

        //  プレイヤーに突っ込む
        yield return StartCoroutine( Charge() );
    }
    //------------------------------------------------------------------
    //  座標(0,2,0)を中心に回転する
    //------------------------------------------------------------------
    private void AxisRotation()
    {
        //　座標(0,2,0)を中心に回転する
        Vector3 center = new Vector3(0,2,0); // 回転の中心
        Vector3 axis = Vector3.forward;      // 回転軸
        float period = 2;                    // 円運動周期

        Transform tr = this.transform;

        // 回転のクォータニオン作成
        Quaternion angleAxis = Quaternion.AngleAxis(360 / period * Time.deltaTime, axis);

        // 円運動の位置計算
        Vector3 pos = tr.position;

        pos -= center;
        pos = angleAxis * pos;
        pos += center;

        tr.position = pos;
    }

    //------------------------------------------------------------------
    //  おっさんムーブ
    //------------------------------------------------------------------
    private IEnumerator OssanMove()
    {
        float speed = 3.0f;    //  移動スピード

        //  無敵モードOFF
        bSuperMode = false;

        //  2秒待つ
        yield return new WaitForSeconds(2.0f);

        //  プレイヤーへのベクトルを計算
        Vector3 playerPos = GameManager.Instance.GetPlayer().transform.position;
        Vector3 vec = playerPos - transform.position;
        vec.Normalize();

        //  その方向にでたらめな距離伸ばした座標に移動する
        float dist = 30.0f;
        vec = vec * dist;

        //  一定距離直進
        yield return StartCoroutine(
            LineMoveDistance( vec, vec.magnitude/speed ));

        yield return null;
    }
    //------------------------------------------------------------------
    //  直線移動(スポナーから反対側のスポナーへ移動)
    //------------------------------------------------------------------
    private IEnumerator Straight()
    {
        //  無敵モードOFF
        bSuperMode = false;

        //  開始座標を設定(3〜11)
        int rand = UnityEngine.Random.Range(3,12);
        int goalNum = 0;
        Vector3 start = EnemyManager.Instance.GetSpawnerPos(rand);
        transform.position = start;

        if(rand == 0 || rand == 1 || rand ==2)
            Debug.Log("randの値が不正です: " + rand);

        //  移動スピードを設定
        float speed = 3.0f;

        //  開始座標によって目標番号を設定
        if(rand == 3)goalNum = 11;
        else if(rand == 4)goalNum = 10;
        else if(rand == 5)goalNum = 9;
        else if(rand == 6)goalNum = 2;
        else if(rand == 7)goalNum = 1;
        else if(rand == 8)goalNum = 0;
        else if(rand == 9)goalNum = 5;
        else if(rand == 10)goalNum = 4;
        else if(rand == 11)goalNum = 3;

        //  目標座標を設定
        Vector3 goal = EnemyManager.Instance.GetSpawnerPos(goalNum);

        Vector3 distance = goal - start;

        //  目標に直線移動
        yield return StartCoroutine(
            LineMove(goal, distance.magnitude/speed));

        //  処理時間待つ
        yield return new WaitForSeconds(distance.magnitude/speed);

        yield return null;
    }
    //------------------------------------------------------------------
    //  プレイヤーとY軸が合えばTRUEを返す
    //------------------------------------------------------------------
    private bool AdjustAxisY()
    {
        GameObject player = GameManager.Instance.GetPlayer();
        float distanceY = player.transform.position.y - transform.position.y;
        if(Math.Abs(distanceY) <= 0.05f)
        {
            return true;
        }

        return false;  
    }
    //------------------------------------------------------------------
    //  プレイヤーとY軸が合えばTRUEを返す
    //------------------------------------------------------------------
    private bool AdjustAxisX()
    {
        GameObject player = GameManager.Instance.GetPlayer();
        float distanceX = player.transform.position.x - transform.position.x;
        if(Math.Abs(distanceX) <= 0.05f)
        {
            return true;
        }

        return false;  
    }
    //------------------------------------------------------------------
    //  直線移動中にプレイヤーとY軸が合えば自機に突撃
    //------------------------------------------------------------------
    private IEnumerator AdjustLineY()
    {
        // 0,1,2,6,7,8のリストを用意する
        List<int> spawmNo = new List<int>();
        spawmNo.Add(0);
        spawmNo.Add(1);
        spawmNo.Add(2);
        spawmNo.Add(6);
        spawmNo.Add(7);
        spawmNo.Add(8);

        int rand = spawmNo[UnityEngine.Random.Range(0,spawmNo.Count)];
        int goalNum = 0;
        Vector3 start = EnemyManager.Instance.GetSpawnerPos(rand);
        transform.position = start;

        //  移動スピードを設定
        float speed1 = 1.0f;
        float speed2 = 2.0f;

        //  開始座標によって目標番号を設定
        if(rand == 0)goalNum = 8;
        else if(rand == 1)goalNum = 7;
        else if(rand == 2)goalNum = 6;
        else if(rand == 6)goalNum = 2;
        else if(rand == 7)goalNum = 1;
        else if(rand == 8)goalNum = 0;

        //  目標座標を設定
        Vector3 goal = EnemyManager.Instance.GetSpawnerPos(goalNum);

        Vector3 distance = goal - start;

        //  目標に直線移動
        Tweener tweener = transform.DOMove(goal, distance.magnitude/speed1)
            .SetEase(Ease.Linear);

        //  Y軸が合うまで待つ
        yield return new WaitUntil(() => AdjustAxisY() == true);

        // DoMoveを途中終了
        tweener.Kill();

        //  1秒待つ
        yield return new WaitForSeconds(1);

        GameObject player = GameManager.Instance.GetPlayer();
        float dX =  player.transform.position.x - transform.position.x;

        if(dX > 0)
        {
            transform.DOMoveX(11f, Math.Abs(dX)/speed2)
                .SetEase(Ease.Linear);
        }
        else
        {
            transform.DOMoveX(-14f, Math.Abs(dX)/speed2)
                .SetEase(Ease.Linear);
        }

        //  待つ
        yield return new WaitForSeconds(dX/speed2);

        yield return null;
    }
    //------------------------------------------------------------------
    //  直線移動中にプレイヤーとX軸が合えば自機に突撃
    //------------------------------------------------------------------
    private IEnumerator AdjustLineX()
    {
        // 0,1,2,6,7,8のリストを用意する
        List<int> spawmNo = new List<int>();
        spawmNo.Add(3);
        spawmNo.Add(4);
        spawmNo.Add(5);
        spawmNo.Add(9);
        spawmNo.Add(10);
        spawmNo.Add(11);

        int rand = spawmNo[UnityEngine.Random.Range(0,spawmNo.Count)];
        int goalNum = 0;
        Vector3 start = EnemyManager.Instance.GetSpawnerPos(rand);
        transform.position = start;

        //  移動スピードを設定
        float speed1 = 1.0f;
        float speed2 = 2.0f;

        //  開始座標によって目標番号を設定
        if(rand == 3)goalNum = 11;
        else if(rand == 4)goalNum = 10;
        else if(rand == 5)goalNum = 9;
        else if(rand == 9)goalNum = 5;
        else if(rand == 10)goalNum = 4;
        else if(rand == 11)goalNum = 3;

        //  目標座標を設定
        Vector3 goal = EnemyManager.Instance.GetSpawnerPos(goalNum);

        Vector3 distance = goal - start;

        //  目標に直線移動
        Tweener tweener = transform.DOMove(goal, distance.magnitude/speed1)
            .SetEase(Ease.Linear);

        //  X軸が合うまで待つ
        yield return new WaitUntil(() => AdjustAxisX() == true);

        // DoMoveを途中終了
        tweener.Kill();

        //  1秒待つ
        yield return new WaitForSeconds(1);

        GameObject player = GameManager.Instance.GetPlayer();
        float dY =  player.transform.position.y - transform.position.y;

        if(dY > 0)
        {
            transform.DOMoveY(11f, Math.Abs(dY)/speed2)
                .SetEase(Ease.Linear);
        }
        else
        {
            transform.DOMoveY(-8f, Math.Abs(dY)/speed2)
                .SetEase(Ease.Linear);
        }

        //  待つ
        yield return new WaitForSeconds(dY/speed2);

        yield return null;
    }
    //------------------------------------------------------------------
    //  直線移動中にプレイヤーとX軸かY軸が合えば自機に突撃
    //------------------------------------------------------------------
    private IEnumerator AdjustLine()
    {
        //  無敵モードOFF
        bSuperMode = false;

        //  X軸かY軸かはランダム
        int rand = UnityEngine.Random.Range(0,2);

        if(rand == 0)StartCoroutine(AdjustLineY());
        else StartCoroutine(AdjustLineX());

        yield return null;
    }
    //------------------------------------------------------------------
    //  放物線移動ステップ１(スタート位置左)
    //------------------------------------------------------------------
    private IEnumerator CurveFromL1()
    {
        float duration = 3.0f; //  移動にかかる時間

        Vector3 start = EnemyManager.Instance.GetSpawnerPos(8);
        Vector3 middle1 = new Vector3(-4.0f,-1.65f,0);
        Vector3 middle2 = new Vector3(0f,-0.65f,0);
        Vector3 middle3 = new Vector3(4.5f,-1.65f,0);
        Vector3 end = EnemyManager.Instance.GetSpawnerPos(6);

        //  初期座標を設定
        transform.position = start;

        transform.DOLocalPath(
            new[]
            {
                start,
                middle1,
                middle2,
            },
            duration, PathType.CatmullRom)
            .SetEase(Ease.Linear)
            .WaitForCompletion();
         
        //  5秒待つ
        yield return new WaitForSeconds(duration);
    }
    //------------------------------------------------------------------
    //  放物線移動ステップ２(スタート位置左)
    //------------------------------------------------------------------
    private IEnumerator CurveFromL2()
    {
        float duration = 3.0f; //  移動にかかる時間

        Vector3 start = EnemyManager.Instance.GetSpawnerPos(8);
        Vector3 middle1 = new Vector3(-4.0f,-1.65f,0);
        Vector3 middle2 = new Vector3(0f,-0.65f,0);
        Vector3 middle3 = new Vector3(4.5f,-1.65f,0);
        Vector3 end = EnemyManager.Instance.GetSpawnerPos(6);

        //  初期座標を設定
        transform.position = middle2;

        transform.DOLocalPath(
            new[]
            {
                middle2,
                middle3,
                end,
            },
            duration, PathType.CatmullRom)
            .SetEase(Ease.Linear)
            .WaitForCompletion();
         
        //  5秒待つ
        yield return new WaitForSeconds(duration);
    }
    //------------------------------------------------------------------
    //  放物線移動(スタート位置左)
    //------------------------------------------------------------------
    private IEnumerator CurveFromL()
    {
        yield return StartCoroutine( CurveFromL1() );

        yield return StartCoroutine( CurveFromL2() );
        
        yield return null; 
    }
    //------------------------------------------------------------------
    //  放物線移動ステップ１(スタート位置右)
    //------------------------------------------------------------------
    private IEnumerator CurveFromR1()
    {
        float duration = 3.0f; //  移動にかかる時間

        Vector3 start = EnemyManager.Instance.GetSpawnerPos(6);
        Vector3 middle1 = new Vector3(4.5f,-1.65f,0);
        Vector3 middle2 = new Vector3(0f,-0.65f,0);
        Vector3 middle3 = new Vector3(-4.0f,-1.65f,0);
        Vector3 end = EnemyManager.Instance.GetSpawnerPos(8);

        //  初期座標を設定
        transform.position = start;

        transform.DOLocalPath(
            new[]
            {
                start,
                middle1,
                middle2,
            },
            duration, PathType.CatmullRom)
            .SetEase(Ease.Linear)
            .WaitForCompletion();
         
        //  5秒待つ
        yield return new WaitForSeconds(duration);
    }
    //------------------------------------------------------------------
    //  放物線移動ステップ２(スタート位置右)
    //------------------------------------------------------------------
    private IEnumerator CurveFromR2()
    {
        float duration = 3.0f; //  移動にかかる時間

        Vector3 start = EnemyManager.Instance.GetSpawnerPos(6);
        Vector3 middle1 = new Vector3(4.5f,-1.65f,0);
        Vector3 middle2 = new Vector3(0f,-0.65f,0);
        Vector3 middle3 = new Vector3(-4.0f,-1.65f,0);
        Vector3 end = EnemyManager.Instance.GetSpawnerPos(8);

        //  初期座標を設定
        transform.position = middle2;

        transform.DOLocalPath(
            new[]
            {
                middle2,
                middle3,
                end,
            },
            duration, PathType.CatmullRom)
            .SetEase(Ease.Linear)
            .WaitForCompletion();
         
        //  5秒待つ
        yield return new WaitForSeconds(duration);
    }
    //------------------------------------------------------------------
    //  放物線移動(スタート位置右)
    //------------------------------------------------------------------
    private IEnumerator CurveFromR()
    {
        yield return StartCoroutine( CurveFromR1() );

        yield return StartCoroutine( CurveFromR2() );
        
        yield return null; 
    }
    //------------------------------------------------------------------
    //  放物線移動(スタート位置はランダム)
    //------------------------------------------------------------------
    private IEnumerator Curve()
    {
        //  無敵モードOFF
        bSuperMode = false;

        //  スタート位置はランダム
        int rand = UnityEngine.Random.Range(0,2);

        if(rand == 0)StartCoroutine(CurveFromL());
        else StartCoroutine(CurveFromR());

        yield return null;
    }


    //------------------------------------------------------------------
    //  放物線移動の後自機狙い弾を撃って放物線移動で消えていく(中級ザコ)
    //------------------------------------------------------------------
    private IEnumerator CurveAndShootL()
    {
        yield return StartCoroutine( CurveFromL1() );

        //  ここで自機狙い弾を撃つ
        yield return StartCoroutine( SnipeShot3way() );

        //  2秒待つ
        yield return new WaitForSeconds(2);

        yield return StartCoroutine( CurveFromL2() );
        
        yield return null; 
    }
    //------------------------------------------------------------------
    //  放物線移動の後自機狙い弾を撃って放物線移動で消えていく(中級ザコ)
    //------------------------------------------------------------------
    private IEnumerator CurveAndShootR()
    {
        yield return StartCoroutine( CurveFromR1() );

        //  ここで自機狙い弾を撃つ
        yield return StartCoroutine( SnipeShot3way() );

        //  2秒待つ
        yield return new WaitForSeconds(2);

        yield return StartCoroutine( CurveFromR2() );

        yield return null;
    }
    //------------------------------------------------------------------
    //  放物線移動の後自機狙い弾を撃って放物線移動で消えていく(中級ザコ)
    //------------------------------------------------------------------
    private IEnumerator CurveAndShootRandom()
    {
        //  無敵モードOFF
        bSuperMode = false;

        //  スタート位置はランダム
        int rand = UnityEngine.Random.Range(0,2);

        if(rand == 0)StartCoroutine(CurveAndShootL());
        else StartCoroutine(CurveAndShootR());

        yield return null;
    }
    //------------------------------------------------------------------
    //  自機狙い弾を撃つ
    //------------------------------------------------------------------
    private IEnumerator SnipeShot3way()
    {
        //  通常自機狙い弾のプレハブを取得
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Snipe_Normal);

        float Degree = 30;              //  ずらす角度
        int wayNum = 3;                 //  弾のway数
        float speed = 2.0f;             //  弾速
        int chain = 1;                  //  連弾数
        float chainInterval = 0.3f;     //  連弾の間隔（秒）

        //  敵からプレイヤーへのベクトルを取得
        Vector3 playerPos = GameManager.Instance.GetPlayer().transform.position;
        Vector3 vector0 = (playerPos - transform.position).normalized;
        Vector3[] vector = new Vector3[wayNum];

        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < wayNum; i++)
            {
                vector[i] = Quaternion.Euler( 0, 0, -Degree + (i * Degree) ) * vector0;
                vector[i].z = 0f;

                //弾インスタンスを取得し、初速と発射角度を与える
                GameObject Bullet_obj = 
                    (GameObject)Instantiate(bullet, transform.position, transform.rotation);
                EnemyBullet enemyBullet = Bullet_obj.GetComponent<EnemyBullet>();
                enemyBullet.SetSpeed(speed);
                enemyBullet.SetVelocity(vector[i]);
                enemyBullet.SetPower(enemyData.Attack);

                //  発射SE再生
                SoundManager.Instance.PlaySFX(
                (int)AudioChannel.ENEMY_SHOT,
                (int)SFXList.SFX_ENEMY_SHOT);
            }
            yield return new WaitForSeconds(chainInterval);
        }


        yield return null;
    }
    //------------------------------------------------------------------
    //  バラマキ弾を撃つ
    //------------------------------------------------------------------
    private IEnumerator WildlyShot3way()
    {
        //  通常自機狙い弾のプレハブを取得
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Wildly_Normal);

        float Degree = 45;              //  ずらす角度
        int wayNum = 3;                 //  弾のway数
        float speed = 2.0f;             //  弾速
        int chain = 1;                  //  連弾数
        float chainInterval = 0.3f;     //  連弾の間隔（秒）

        //  敵の前方ベクトルを取得
        Vector3 vector0 = transform.up;
        Vector3[] vector = new Vector3[wayNum];
        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < wayNum; i++)
            {
                vector[i] = Quaternion.Euler( 0, 0, -Degree + (i * Degree) ) * vector0;
                vector[i].z = 0f;

                //弾インスタンスを取得し、初速と発射角度を与える
                GameObject Bullet_obj = 
                    (GameObject)Instantiate(bullet, transform.position, transform.rotation);
                EnemyBullet enemyBullet = Bullet_obj.GetComponent<EnemyBullet>();
                enemyBullet.SetSpeed(speed);
                enemyBullet.SetVelocity(vector[i]);
                enemyBullet.SetPower(enemyData.Attack);

                //  発射SE再生
                SoundManager.Instance.PlaySFX(
                (int)AudioChannel.ENEMY_SHOT,
                (int)SFXList.SFX_ENEMY_SHOT);
            }
            yield return new WaitForSeconds(chainInterval);
        }

        yield return null;
    }
    //------------------------------------------------------------------
    //  やられた時は３wayの撃ち返し弾を撃つ
    //------------------------------------------------------------------
    private IEnumerator CounterShot3way()
    {
        //  通常自機狙い弾のプレハブを取得
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Snipe_Normal);

        float Degree = 30;              //  ずらす角度
        int wayNum = 3;                 //  弾のway数
        float speed = 2.0f;             //  弾速
        int chain = 1;                  //  連弾数
        float chainInterval = 0.3f;     //  連弾の間隔（秒）

        //  敵からプレイヤーへのベクトルを取得
        Vector3 playerPos = GameManager.Instance.GetPlayer().transform.position;
        Vector3 vector0 = (playerPos - transform.position).normalized;
        Vector3[] vector = new Vector3[wayNum];

        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < wayNum; i++)
            {
                vector[i] = Quaternion.Euler( 0, 0, -Degree + (i * Degree) ) * vector0;
                vector[i].z = 0f;

                //弾インスタンスを取得し、初速と発射角度を与える
                GameObject Bullet_obj = 
                    (GameObject)Instantiate(bullet, transform.position, transform.rotation);
                EnemyBullet enemyBullet = Bullet_obj.GetComponent<EnemyBullet>();
                enemyBullet.SetSpeed(speed);
                enemyBullet.SetVelocity(vector[i]);
                enemyBullet.SetPower(enemyData.Attack);

                //  発射SE再生
                SoundManager.Instance.PlaySFX(
                (int)AudioChannel.ENEMY_SHOT,
                (int)SFXList.SFX_ENEMY_SHOT);
            }
            yield return new WaitForSeconds(chainInterval);
        }

        yield return null;
    }
    //------------------------------------------------------------------
    //  中ボス・中央から指定ポイントに移動する
    //------------------------------------------------------------------
    private IEnumerator MidBoss_Appearance()
    {
        Vector3 goal = new Vector3(0,-2.5f,0); //  目標座標
        float duration = 3.0f;                  //  移動にかかる時間
        float wait_time = 2.0f;                 //  待機時間

        //  初期座標を設定
        transform.position = new Vector3(0,-6,0);

        //  座標に直線移動する
        StartCoroutine(LineMove(goal, duration));

        //  移動時間中待機
        yield return new WaitForSeconds(duration);

        //  無敵モードOFF
        bSuperMode = false;

        //  コライダーを有効化
        this.GetComponent<BoxCollider2D>().enabled = true;

        //  移動後待機
        yield return new WaitForSeconds(wait_time);

        //  横移動モードに移行
        moveType = MOVE_TYPE.MidBoss_MoveSide;

        yield return null;
    }
    //------------------------------------------------------------------
    //  中ボス・左右に繰り返し移動する
    //------------------------------------------------------------------
    private IEnumerator MidBoss_MoveSide()
    {
        //  Phase1
        yield return StartCoroutine(MidBoss_Phase1());

        Debug.Log("***中ボス第２段階開始！***");

        //  Phase2
        yield return StartCoroutine(MidBoss_Phase2());

        yield return null;
    }
    //------------------------------------------------------------------
    //  中ボスの体力監視用
    //------------------------------------------------------------------
    private void SwitchBossPhase()
    {
        //  ザコ敵は除外
        if(isMidBoss == IS_MID_BOSS.No)return;

        //  フェーズ切り替えの体力閾値
        float hp_threshold = 0.7f;

        //  体力が最大体力x閾値かつフェーズ切り替えがFalseだったら
        if(hp <= enemyData.Hp * hp_threshold && !bSwitchPhase)
        {
            //  フェーズ切り替えフラグをTRUE
            bSwitchPhase = true;

            //  無敵フラグをTRUE
            bSuperMode = true;
        }
    }
    //------------------------------------------------------------------
    //  中ボス・Phase1
    //------------------------------------------------------------------
    private IEnumerator MidBoss_Phase1()
    {
        //  中ボスのタイプによって行動パターンを呼び分ける
        if(enemyType == ENEMY_TYPE.Ibarakidouji)
        {
            yield return StartCoroutine(Ibaraki_Phase1());
        }
        else if(enemyType == ENEMY_TYPE.Nuigurumi)
        {
            yield return StartCoroutine(Nuigurumi_Phase1());
        }
        else if(enemyType == ENEMY_TYPE.Ibarakidouji)
        {
            yield return StartCoroutine(Ibaraki_Phase1());
        }
        else if(enemyType == ENEMY_TYPE.Ibarakidouji)
        {
            yield return StartCoroutine(Ibaraki_Phase1());
        }
        else if(enemyType == ENEMY_TYPE.Ibarakidouji)
        {
            yield return StartCoroutine(Ibaraki_Phase1());
        }
        else if(enemyType == ENEMY_TYPE.Ibarakidouji)
        {
            yield return StartCoroutine(Ibaraki_Phase1());
        }
    }
    //------------------------------------------------------------------
    //  茨木童子・Phase1
    //------------------------------------------------------------------
    private IEnumerator Ibaraki_Phase1()
    {
        float duration = 2.5f;  //  移動にかかる時間

        while(true)
        {
            transform.DOLocalMoveX(5f, duration)
                .SetEase(Ease.OutBack);

            //  移動時間待つ
            yield return new WaitForSeconds(duration);

            //  360度バラマキ弾
            yield return StartCoroutine(MidBoss_WildlyShot360());

            //  HPが閾値を切ったら抜ける
            if(bSwitchPhase)break;

            transform.DOLocalMoveX(-5f, duration)
            .SetEase(Ease.OutBack);

            //  移動時間待つ
            yield return new WaitForSeconds(duration);

            //  360度バラマキ弾
            yield return StartCoroutine(MidBoss_WildlyShot360());

            //  HPが閾値を切ったら抜ける
            if(bSwitchPhase)break;
        }
    }
    //------------------------------------------------------------------
    //  ぬいぐるみ・Phase1
    //------------------------------------------------------------------
    private IEnumerator Nuigurumi_Phase1()
    {
        float duration = 2.5f;  //  移動にかかる時間

        while(true)
        {
            yield return StartCoroutine(MidBoss_WalkToPlayer(duration));

            //  移動時間待つ
            yield return new WaitForSeconds(duration);

            //  HPが閾値を切ったら抜ける
            if(bSwitchPhase)break;

            //  爪痕状攻撃
            yield return StartCoroutine(MidBoss_ClawShot());

            //  HPが閾値を切ったら抜ける
            if(bSwitchPhase)break;

            //  360度攻撃
            yield return StartCoroutine(MidBoss_WildlyShot360());

            //  HPが閾値を切ったら抜ける
            if(bSwitchPhase)break;
        }
    }
    //------------------------------------------------------------------
    //  中ボス・Phase2
    //------------------------------------------------------------------
    private IEnumerator MidBoss_Phase2()
    {
        //  中ボスのタイプによって行動パターンを呼び分ける
        if(enemyType == ENEMY_TYPE.Ibarakidouji)
        {
            yield return StartCoroutine(Ibaraki_Phase2());
        }
        else if(enemyType == ENEMY_TYPE.Nuigurumi)
        {
            yield return StartCoroutine(Nuigurumi_Phase2());
        }
        else if(enemyType == ENEMY_TYPE.Ibarakidouji)
        {
            yield return StartCoroutine(Ibaraki_Phase2());
        }
        else if(enemyType == ENEMY_TYPE.Ibarakidouji)
        {
            yield return StartCoroutine(Ibaraki_Phase2());
        }
        else if(enemyType == ENEMY_TYPE.Ibarakidouji)
        {
            yield return StartCoroutine(Ibaraki_Phase2());
        }
        else if(enemyType == ENEMY_TYPE.Ibarakidouji)
        {
            yield return StartCoroutine(Ibaraki_Phase2());
        }
    }
    //------------------------------------------------------------------
    //  茨木童子・Phase2
    //------------------------------------------------------------------
    private IEnumerator Ibaraki_Phase2()
    {
        int rand_move = 60;    //  ジャンプ移動の閾値
        int rand_shot = 100;   //  バラマキ弾自機狙い弾の閾値

        //  ジャンプ
        yield return StartCoroutine(MidBoss_Jump());

        //  無敵モードOFF
        bSuperMode = false;

        //  ２秒待つ
        yield return new WaitForSeconds(2);

        while(true)
        {
            int rand = UnityEngine.Random.Range(0,100);
            if(rand < rand_move)
            {
                yield return StartCoroutine(MidBoss_JumpAndMoveSide());
            }
            else if(rand <= rand_shot)
            {
                yield return StartCoroutine(MidBoss_WildlyShot5way());
                yield return StartCoroutine(MidBoss_SnipeShot3way());
            }

            //  1秒待つ
            yield return new WaitForSeconds(1);
        }
    }
    //------------------------------------------------------------------
    //  ぬいぐるみ・Phase2
    //------------------------------------------------------------------
    private IEnumerator Nuigurumi_Phase2()
    {
        //  プレイヤーに接近する時間
        float duration = 2f;

        //  無敵モードOFF
        bSuperMode = false;

        //  中央に戻る
        yield return StartCoroutine(MidBoss_ReturnToCenter());

        while(true)
        {
            //  人形ミサイル召喚
            yield return StartCoroutine(MidBoss_SummonDollMissile());

            //  人形包囲陣召喚
            yield return StartCoroutine(MidBoss_SummonDollGroup());

            //  プレイヤーに接近
            yield return StartCoroutine(MidBoss_WalkToPlayer(duration));

            //  移動時間待つ
            yield return new WaitForSeconds(duration);

            //  爪痕攻撃
            yield return StartCoroutine(MidBoss_ClawShot());
        }
    }
    //------------------------------------------------------------------
    //  中ボス・ジャンプ
    //------------------------------------------------------------------
    private IEnumerator MidBoss_Jump()
    {
        float duration = 0.3f;

        //  無敵フラグをTRUE
        bSuperMode = true;

        //  汽笛SE再生
        SoundManager.Instance.PlaySFX(
        (int)AudioChannel.SFX_SYSTEM,
        (int)SFXList.SFX_MIDBOSS_PHASE2);

        //  ジャンプする
        transform.DOLocalMoveY(2f, duration)
                .SetEase(Ease.OutExpo)
                .SetRelative(true);

        //  ジャンプSE再生
        SoundManager.Instance.PlaySFX(
        (int)AudioChannel.SFX_ENEMY,
        (int)SFXList.SFX_MIDBOSS_JUMP);

        yield return new WaitForSeconds(duration);

        transform.DOLocalMoveY(-2f, duration)
        .SetEase(Ease.InExpo)
        .SetRelative(true);

        yield return new WaitForSeconds(duration);

        //  ジャンプする
        transform.DOLocalMoveY(2f, duration)
                .SetEase(Ease.OutExpo)
                .SetRelative(true);

        //  ジャンプSE再生
        SoundManager.Instance.PlaySFX(
        (int)AudioChannel.SFX_ENEMY,
        (int)SFXList.SFX_MIDBOSS_JUMP);

        yield return new WaitForSeconds(duration);

        transform.DOLocalMoveY(-2f, duration)
        .SetEase(Ease.InExpo)
        .SetRelative(true);

        yield return new WaitForSeconds(duration);


        yield return null;
    }
    //------------------------------------------------------------------
    //  中ボス・ジャンプと横移動
    //------------------------------------------------------------------
    private IEnumerator MidBoss_JumpAndMoveSide()
    {
        float interval = 4.0f;  //  次の行動までの時間(秒)
        float duration = 0.5f;
        float jumpY = 9.0f;
        float jump_minX = -7.0f;
        float jump_maxX = 5.0f;

        //---------------------------------------------------------------------
        //  予備動作で２回小ジャンプする
        //---------------------------------------------------------------------
        
        //  ジャンプする
        transform.DOLocalMoveY(2f, duration)
                .SetEase(Ease.OutExpo)
                .SetRelative(true);

        //  ジャンプSE再生
        SoundManager.Instance.PlaySFX(
        (int)AudioChannel.SFX_ENEMY,
        (int)SFXList.SFX_MIDBOSS_JUMP);

        yield return new WaitForSeconds(duration);

        transform.DOLocalMoveY(-2f, duration)
        .SetEase(Ease.InExpo)
        .SetRelative(true);

        yield return new WaitForSeconds(duration);

        //  ジャンプする
        transform.DOLocalMoveY(2f, duration)
                .SetEase(Ease.OutExpo)
                .SetRelative(true);

        //  ジャンプSE再生
        SoundManager.Instance.PlaySFX(
        (int)AudioChannel.SFX_ENEMY,
        (int)SFXList.SFX_MIDBOSS_JUMP);

        yield return new WaitForSeconds(duration);

        transform.DOLocalMoveY(-2f, duration)
        .SetEase(Ease.InExpo)
        .SetRelative(true);

        yield return new WaitForSeconds(duration);
        //---------------------------------------------------------------------


        //  横移動する
        float rndX = UnityEngine.Random.Range(jump_minX, jump_maxX);
        transform.DOMoveX(rndX, interval)
            .SetEase(Ease.OutBack);

        //  ジャンプする
        transform.DOLocalMoveY(jumpY, interval/2)
                .SetEase(Ease.OutExpo)
                .SetRelative(true);

        //  ジャンプSE再生
        SoundManager.Instance.PlaySFX(
        (int)AudioChannel.SFX_ENEMY,
        (int)SFXList.SFX_MIDBOSS_JUMP);

        //  移動時間待つ
        yield return new WaitForSeconds(interval/2);

        //  ジャンプから戻る
        transform.DOLocalMoveY(-jumpY, interval/2)
                .SetEase(Ease.OutExpo)
                .SetRelative(true);

        //  戻り時間待つ
        yield return new WaitForSeconds(interval/2);

        //  横移動する
        rndX = UnityEngine.Random.Range(jump_minX, jump_maxX);
        transform.DOMoveX(rndX, interval)
            .SetEase(Ease.OutBack);

        //  ジャンプする
        transform.DOLocalMoveY(jumpY, interval/2)
                .SetEase(Ease.OutExpo)
                .SetRelative(true);

        //  ジャンプSE再生
        SoundManager.Instance.PlaySFX(
        (int)AudioChannel.SFX_ENEMY,
        (int)SFXList.SFX_MIDBOSS_JUMP);

        //  移動時間待つ
        yield return new WaitForSeconds(interval/2);

        //  ジャンプから戻る
        transform.DOLocalMoveY(-jumpY, interval/2)
                .SetEase(Ease.OutExpo)
                .SetRelative(true);

        //  戻り時間待つ
        yield return new WaitForSeconds(interval/2);
    }
    //------------------------------------------------------------------
    //  中ボス・プレイヤーに徐々に近づいてくる
    //------------------------------------------------------------------
    private IEnumerator MidBoss_WalkToPlayer(float duration)
    {
        //  １回の移動量
        float move_distance = 1.0f;

        //  プレイヤー座標を取得
        Vector3 playerPos = GameManager.Instance.GetPlayer().transform.position;

        //  プレイヤーへのベクトルを取得
        Vector3 vector = playerPos - transform.position;
        vector.Normalize(); //  正規化

        //  目標座標を計算
        Vector3 goal = move_distance * vector;

        //  移動
        this.transform.DOMove(playerPos, duration);

        yield return null;
    }
    //------------------------------------------------------------------
    //  中ボス・中央に戻る
    //------------------------------------------------------------------
    private IEnumerator MidBoss_ReturnToCenter()
    {
        Vector3 goal = new Vector3(0,-2.5f,0); //  目標座標
        float duration = 1.0f;                 //  移動にかかる時間

        //  座標に直線移動する
        StartCoroutine(LineMove(goal, duration));

        //  移動時間中待機
        yield return new WaitForSeconds(duration);
    }
    //------------------------------------------------
    //  中ボス・指定の座標に指定の敵セットを生成する
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
    //------------------------------------------------------------------
    //  中ボス・人形召喚
    //------------------------------------------------------------------
    private IEnumerator MidBoss_SummonDollMissile()
    {
        //  ステージ２が前提
        if(PlayerInfoManager.stageInfo != PlayerInfoManager.StageInfo.Stage02)
        {
            Debug.LogError("人形召喚はステージ２でなければできません！");
        }

        //  AnimatorのSummonをTrue
        this.GetComponent<Animator>().SetBool("Summon", true);

        for(int i=0;i<10;i++)
        {
            //  人形を生成してデータセット
            GameObject prefab = EnemyManager.Instance.GetEnemyPrefab((int)EnemyPattern.E01);

            //  人形オブジェクトを生成＆データをセット
            GameObject doll = SetEnemy(prefab, transform.position);
        
            //  moveTypeをRandomChargeにする
            doll.GetComponent<Enemy>().moveType = MOVE_TYPE.RandomCharge;
        }

        //  AnimatorのSummonをFalse
        this.GetComponent<Animator>().SetBool("Summon", false);

        //  １秒待つ
        yield return new WaitForSeconds(1);
    }
    //------------------------------------------------------------------
    //  中ボス・人形包囲陣召喚
    //------------------------------------------------------------------
    private IEnumerator MidBoss_SummonDollGroup()
    {
        //  確率1〜100
        int rand = UnityEngine.Random.Range(1, 101);

        //  ４の倍数なら人形包囲陣発動！
        if(rand % 4 == 0)EnemyManager.Instance.SetDollGroup();

        yield return null;
    }
    //------------------------------------------------------------------
    //  中ボス・５wayのバラマキ弾を撃つ/弾速・遅い
    //------------------------------------------------------------------
    private IEnumerator MidBoss_WildlyShot5way()
    {
        //  通常自機狙い弾のプレハブを取得
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Wildly_Normal);

        float totalDegree = 120;        //  撃つ範囲の総角  
        int wayNum = 5;                 //  弾のway数(必ず3way以上の奇数にすること)
        float Degree = totalDegree / (wayNum-1);     //  弾一発毎にずらす角度         
        float speed = 2.0f;             //  弾速
        int chain = 10;                 //  連弾数
        float chainInterval = 0.2f;     //  連弾の間隔（秒）

        //  敵の前方ベクトルを取得
        Vector3 vector0 = transform.up;
        Vector3[] vector = new Vector3[wayNum];
        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < wayNum; i++)
            {
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
    //  中ボス・３wayの自機狙い弾を撃つ/弾速・早い
    //------------------------------------------------------------------
    private IEnumerator MidBoss_SnipeShot3way()
    {
        //  通常自機狙い弾のプレハブを取得
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Snipe_Normal);

        float Degree = 15;              //  ずらす角度
        int wayNum = 3;                 //  弾のway数
        float speed = 7.0f;             //  弾速
        int chain = 1;                  //  連弾数
        float chainInterval = 0.1f;     //  連弾の間隔（秒）

        //  敵からプレイヤーへのベクトルを取得
        Vector3 playerPos = GameManager.Instance.GetPlayer().transform.position;
        Vector3 vector0 = (playerPos - transform.position).normalized;
        Vector3[] vector = new Vector3[wayNum];

        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < wayNum; i++)
            {
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
    //  中ボス・360度のバラマキ弾を撃つ/弾速・遅い
    //------------------------------------------------------------------
    private IEnumerator MidBoss_WildlyShot360()
    {
        //  通常自機狙い弾のプレハブを取得
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Wildly_Normal);

        float totalDegree = 360*5;        //  撃つ範囲の総角  
        int wayNum = 24*5;                //  弾のway数
        float Degree = totalDegree / wayNum;     //  弾一発毎にずらす角度         
        float speed = 4.0f;               //  弾速
        int chain = wayNum;               //  連弾数
        float chainInterval = 0.03f;      //  連弾の間隔（秒）
        float wait_time = 2f;             //  弾発射後の待ち時間（秒）

        //  敵の前方ベクトルを取得
        Vector3 vector0 = transform.up;
        Vector3[] vector = new Vector3[wayNum];
        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < 1; i++)
            {
                vector[i] = Quaternion.Euler
                    (0, 0, -Degree*j) * vector0;
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

        yield return new WaitForSeconds(wait_time);
    }
    //------------------------------------------------------------------
    //  中ボス・やられた時は８方向に撃ち返し弾を撃つ
    //------------------------------------------------------------------
    private IEnumerator MidBoss_CounterShot8way()
    {
        //  通常自機狙い弾のプレハブを取得
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Snipe_Normal);


        float totalDegree = 360;        //  撃つ範囲の総角  
        int wayNum = 8;                 //  弾のway数
        float Degree = totalDegree / (wayNum-1);     //  弾一発毎にずらす角度  
        float speed = 2.0f;             //  弾速
        int chain = 1;                  //  連弾数
        float chainInterval = 0.3f;     //  連弾の間隔（秒）

        //  敵からプレイヤーへのベクトルを取得
        Vector3 playerPos = GameManager.Instance.GetPlayer().transform.position;
        Vector3 vector0 = (playerPos - transform.position).normalized;
        Vector3[] vector = new Vector3[wayNum];

        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < wayNum; i++)
            {
                vector[i] = Quaternion.Euler( 0, 0, i * Degree ) * vector0;
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
    //  中ボス・爪痕状に弾を撃つ
    //------------------------------------------------------------------
    private IEnumerator MidBoss_ClawShot()
    {
        //  Animatorを取得
        Animator animator = this.GetComponent<Animator>();

        //  通常自機狙い弾のプレハブを取得
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Wildly_Normal);

        float totalDegree = 120;                //  撃つ範囲の総角  
        int wayNum = 10;                        //  弾のway数
        float Degree = totalDegree / wayNum;    //  弾一発毎にずらす角度
        float bias_degree = 30;                 //  発射座標の補正角 
        float speed = 6.0f;                     //  弾速
        float delay_time = 0.03f;               //  弾一発ごとのディレイ（秒）
        float wait_time = 2f;                   //  弾発射後の待ち時間（秒）

        //  プレイヤーへのベクトルを取得
        Vector3 playerPos = GameManager.Instance.GetPlayer().transform.position;
        Vector3 vec = playerPos - transform.position;
        vec.Normalize();

        //  敵の前方ベクトルを取得
        Vector3 vectorArmL = Quaternion.Euler(0, 0, totalDegree - bias_degree) * vec;
        Vector3 vectorArmR = Quaternion.Euler(0, 0, -totalDegree + bias_degree) * vec;
        Vector3[] vector = new Vector3[wayNum];

        //--------------------------------------------------
        //  左手攻撃
        //--------------------------------------------------
        //  Animatorの攻撃フラグをON
        animator.SetBool("Attack",true);

        //  待つ
        yield return new WaitForSeconds(0.3f);

        for (int i = 0; i < wayNum; i++)
        {
            vector[i] = Quaternion.Euler(0, 0, Degree * i) * vectorArmR;
            vector[i].z = 0f;

            //弾インスタンスを取得し、初速と発射角度を与える
            GameObject Bullet_obj = 
                (GameObject)Instantiate(bullet, transform.position, transform.rotation);
            EnemyBullet enemyBullet = Bullet_obj.GetComponent<EnemyBullet>();
            enemyBullet.SetSpeed(speed);
            enemyBullet.SetVelocity(vector[i]);
            enemyBullet.SetPower(enemyData.Attack);


            //  発射SE再生
            SoundManager.Instance.PlaySFX(
            (int)AudioChannel.ENEMY_SHOT,
            (int)SFXList.SFX_ENEMY_SHOT);

            //  一発毎にディレイをかける
            yield return new WaitForSeconds(delay_time);
        }

        //  Animatorの攻撃フラグをOFF(※アニメーション時間の0.29秒経過していること)
        animator.SetBool("Attack",false);

        //  1フレーム挟んでおく
        yield return null;

        //--------------------------------------------------
        //  右手攻撃
        //--------------------------------------------------
        //  Animatorの攻撃フラグをON
        animator.SetBool("Attack",true);

        //  待つ
        yield return new WaitForSeconds(0.3f);

        for (int i = 0; i < wayNum; i++)
        {
            vector[i] = Quaternion.Euler(0, 0, -Degree * i) * vectorArmL;
            vector[i].z = 0f;

            //弾インスタンスを取得し、初速と発射角度を与える
            GameObject Bullet_obj = 
                (GameObject)Instantiate(bullet, transform.position, transform.rotation);
            EnemyBullet enemyBullet = Bullet_obj.GetComponent<EnemyBullet>();
            enemyBullet.SetSpeed(speed);
            enemyBullet.SetVelocity(vector[i]);
            enemyBullet.SetPower(enemyData.Attack);


            //  発射SE再生
            SoundManager.Instance.PlaySFX(
            (int)AudioChannel.ENEMY_SHOT,
            (int)SFXList.SFX_ENEMY_SHOT);

            //  一発毎にディレイをかける
            yield return new WaitForSeconds(delay_time);
        }

        //  Animatorの攻撃フラグをOFF(※アニメーション時間の0.29秒経過していること)
        animator.SetBool("Attack",false);

        //  待つ
        yield return new WaitForSeconds(wait_time);
    }


}
