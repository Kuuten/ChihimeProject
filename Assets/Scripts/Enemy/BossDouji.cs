using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


//--------------------------------------------------------------
//
//  ボス・ドウジのクラス
//
//--------------------------------------------------------------
public class BossDouji : MonoBehaviour
{
    //  EnemyDataクラスからの情報取得用
    EnemyData enemyData;
    
    //  パラメータ
    private float hp;
    private bool bDeath;        //  死亡フラグ
    private bool bSuperMode;    //  無敵モードフラグ

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

    // 敵の弾の種類
    public enum BULLET_TYPE {
        //  青弾
        Snipe_Normal,        //  自機狙い弾・通常
        Snipe_RotationL,     //  自機狙い弾・左回転
        Snipe_RotationR,     //  自機狙い弾・右回転
        Snipe_Long,          //  自機狙い弾・ロング
        Snipe_Big,           //  自機狙い弾・大玉
        //  赤弾
        Wildly_Normal,       //  バラマキ弾・通常
        Wildly_RotationL,    //  バラマキ弾・左回転
        Wildly_RotationR,    //  バラマキ弾・右回転
        Wildly_Long,         //  バラマキ弾・ロング
        Wildly_Big,          //  バラマキ弾・大玉
        //  ドウジギミック弾
        Douji_Gimmick_Top,
        Douji_Gimmick_Bottom,
        Douji_Gimmick_Left,
        Douji_Gimmick_Right,
        //  ギミック警告
        Douji_Warning_Top,
        Douji_Warning_Bottom,
        Douji_Warning_Left,
        Douji_Warning_Right,
        //  ドウジ発狂弾
        Douji_Berserk_Bullet
    }

    //  ドロップパワーアップアイテム一覧
    private ePowerupItems powerupItems;

    //  ギミック弾の使用済み番号格納用
    private int[] kooniNum = new int[(int)DoujiPhase2Bullet.KooniDirection.MAX];

    //------------------------------------------------------------
    //  Phase2用
    //------------------------------------------------------------
    private GameObject[] warningObject;

    void Start()
    {
        //  警告オブジェクトを取得
        warningObject = new GameObject[(int)DoujiPhase2Bullet.KooniDirection.MAX];
        warningObject[(int)DoujiPhase2Bullet.KooniDirection.TOP] =
            EnemyManager.Instance.GetBulletPrefab((int)BULLET_TYPE.Douji_Warning_Top);
        warningObject[(int)DoujiPhase2Bullet.KooniDirection.BOTTOM] =
            EnemyManager.Instance.GetBulletPrefab((int)BULLET_TYPE.Douji_Warning_Bottom);
        warningObject[(int)DoujiPhase2Bullet.KooniDirection.LEFT] =
            EnemyManager.Instance.GetBulletPrefab((int)BULLET_TYPE.Douji_Warning_Left);
        warningObject[(int)DoujiPhase2Bullet.KooniDirection.RIGHT] =
            EnemyManager.Instance.GetBulletPrefab((int)BULLET_TYPE.Douji_Warning_Right);

        //  ギミック弾の使用済み番号を初期化
        for(int i=0;i<(int)DoujiPhase2Bullet.KooniDirection.MAX;i++)
        {
            kooniNum[i] = -1;
        }

        //  死亡フラグOFF
        bDeath = false;
        //  最初は無敵モードOFF
        bSuperMode = false;
        //  ループカウントを設定
        loopCount = 1;
        //  点滅の間隔を設定
        flashInterval = 0.2f;
        //  SpriteRenderを取得
        sp = GetComponent<SpriteRenderer>();

        //  行動開始
        StartCoroutine(WaitDoujiAction(1));
    }

    private void OnDestroy()
    {
        Debug.Log("ボス撃破！ステージクリア！");


        //  ボス戦やられたらステージクリア
        GameManager.Instance.SetStageClearFlag(true);
    }

    void Update()
    {
        //  スライダーを更新
        hpSlider.value = hp / enemyData.Hp;
    }

    //  敵のデータを設定 
    public void SetBossData(EnemySetting es, ePowerupItems item)
    {
        string boss_id = "Douji";

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
        if(bDeath)
        {
            return;
        }

        if (collision.CompareTag("NormalBullet"))
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
                Death();                       //  やられ演出
            }
        }
        else if (collision.CompareTag("DoujiConvert"))
        {
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
    //  ドウジの移動パターン
    //
    //******************************************************************

    //------------------------------------------------------------------
    //  数秒待ってからドウジの行動開始
    //------------------------------------------------------------------
    private IEnumerator WaitDoujiAction(float duration)
    {
        yield return new WaitForSeconds(duration);

        //  行動開始
        StartCoroutine(Douji_Action());
    }

    //------------------------------------------------------------------
    //  ドウジの行動管理関数
    //------------------------------------------------------------------
    private IEnumerator Douji_Action()
    {
        float phase1_end = 0.66f;   //  フェーズ１終了条件のHP割合の閾値
        float phase2_end = 0.33f;   //  フェーズ２終了条件のHP割合の閾値

        Debug.Log("***ドウジ弾幕フェーズ開始！***");
        Debug.Log("フェーズ１開始");
        //  フェーズ１
        while (true)
        {
            yield return StartCoroutine(Douji_LoopMove(1.5f, 0.5f));

            yield return StartCoroutine(Shot());


            //yield return StartCoroutine(Douji_BerserkBarrage());

            //yield return StartCoroutine(Douji_LoopMoveBerserk(3, 0.6f, 1.0f));

            //yield return StartCoroutine(Douji_BerserkGatling());

            //yield return StartCoroutine(Douji_LoopMoveBerserk(3, 0.6f, 1.0f));

            //yield return StartCoroutine(Douji_BerserkGatling());



            //  HPが三分の二を切ったら抜ける
            if (hp <= enemyData.Hp*phase1_end)
            {
                //  アイテムドロップ判定
                DropItems drop = this.GetComponent<DropItems>();
                if (drop) drop.DropRandomPowerupItem();

                //  ３秒待つ
                yield return new WaitForSeconds(3);
                break;
            }
        }
        Debug.Log("フェーズ２へ移行");
        //  フェーズ２
        while (true)
        {
            yield return StartCoroutine(Douji_LoopMove(1.0f,1.0f));

            StartCoroutine(KooniParty());
            StartCoroutine(KooniParty());
            StartCoroutine(KooniParty());
            yield return StartCoroutine(KooniParty());

            //  HPが三分の一を切ったら抜ける
            if(hp <= enemyData.Hp*phase2_end)
            {
                //  アイテムドロップ判定
                DropItems drop = this.GetComponent<DropItems>();
                if (drop) drop.DropRandomPowerupItem();

                //  ３秒待つ
                yield return new WaitForSeconds(3);
                break;
            }
        }
        Debug.Log("フェーズ３へ移行");
        //  フェーズ３
        while (true)
        {
            yield return StartCoroutine(Douji_LoopMoveBerserk(3, 0.6f, 1.0f));

            yield return StartCoroutine(Douji_BerserkBarrage());
            yield return StartCoroutine(Douji_BerserkGatling());

            yield return StartCoroutine(Douji_LoopMoveBerserk(3, 0.6f, 1.0f));

            yield return StartCoroutine(Douji_BerserkBarrage());
            yield return StartCoroutine(Douji_BerserkGatling());
        }
    }

    //------------------------------------------------------------------
    //  ドウジの移動
    //------------------------------------------------------------------
    private IEnumerator Douji_LoopMove(float duration,float interval)
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
    //  通常弾をランダムに選択して撃つ
    //------------------------------------------------------------------
    private IEnumerator Shot()
    {
        int rand = Random.Range(0,100);

        //  発生確率の閾値で呼び分ける
        if (rand <= 66f)
        {
            yield return StartCoroutine(WildlyShot());

            yield return StartCoroutine(SnipeShot());
        }
        else if (rand <= 99f)
        {
            yield return StartCoroutine(OriginalShot());
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
        int wayNum = 9;                 //  弾のway数(必ず3way以上の奇数にすること)
        float Degree = totalDegree / (wayNum-1);     //  弾一発毎にずらす角度         
        float speed = 3.0f;             //  弾速
        int chain = 10;                  //  連弾数
        float chainInterval = 0.4f;     //  連弾の間隔（秒）

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
    //  自機狙い弾
    //------------------------------------------------------------------
    private IEnumerator SnipeShot()
    {
        //  通常自機狙い弾のプレハブを取得
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Snipe_Big);

        float Degree = 15;              //  ずらす角度
        int wayNum = 3;                 //  弾のway数
        float speed = 5.0f;            //  弾速
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
    //  オリジナル弾・ガトリングショット
    //------------------------------------------------------------------
    private IEnumerator OriginalShot()
    {
        //  弾のプレハブを取得
        //GameObject bulletL = EnemyManager.Instance
        //    .GetBulletPrefab((int)BULLET_TYPE.Wildly_RotationL);

        //GameObject bulletR = EnemyManager.Instance
        //    .GetBulletPrefab((int)BULLET_TYPE.Wildly_RotationR);
        GameObject bulletL = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Wildly_Big);

        GameObject bulletR = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Wildly_Big);

        float totalDegree = 180;        //  撃つ範囲の総角  
        int wayNum = 1;                 //  弾のway数(必ず3way以上の奇数にすること)
        int chain = 5;                 //  連弾数
        float Degree = totalDegree/chain;     //  弾一発毎にずらす角度         
        float speed = 10.0f;             //  弾速
        float chainInterval = 0.05f;     //  連弾の間隔（秒）

        //  敵の前方ベクトルを取得
        Vector3[] vector = new Vector3[wayNum];
        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < wayNum; i++)
            {
                Vector3 vector0 = Quaternion.Euler(0,0,90) * -transform.up;

                vector[i] = Quaternion.Euler(0,0,j * -Degree) * vector0;
                vector[i].z = 0f;

                //弾インスタンスを取得し、初速と発射角度を与える
                GameObject Bullet_obj = 
                    (GameObject)Instantiate(bulletL, transform.position, transform.rotation);
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

        //  １秒間隔を空ける
        yield return new WaitForSeconds(1.0f);

        //  敵の前方ベクトルを取得
        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < wayNum; i++)
            {
                Vector3 vector0 = Quaternion.Euler(0,0,-90) * -transform.up;

                vector[i] = Quaternion.Euler(0,0,j * Degree) * vector0;
                vector[i].z = 0f;

                //弾インスタンスを取得し、初速と発射角度を与える
                GameObject Bullet_obj = 
                    (GameObject)Instantiate(bulletL, transform.position, transform.rotation);
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

        //  敵の前方ベクトルを取得
        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < wayNum; i++)
            {
                Vector3 vector0 = Quaternion.Euler(0,0,-90) * -transform.up;

                vector[i] = Quaternion.Euler(0,0,j * Degree) * vector0;
                vector[i].z = 0f;

                //弾インスタンスを取得し、初速と発射角度を与える
                GameObject Bullet_obj = 
                    (GameObject)Instantiate(bulletL, transform.position, transform.rotation);
                EnemyBullet enemyBullet = Bullet_obj.GetComponent<EnemyBullet>();
                enemyBullet.SetSpeed(speed*2);
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

            for (int i = 0; i < wayNum; i++)
            {
                Vector3 vector0 = Quaternion.Euler(0,0,90) * -transform.up;

                vector[i] = Quaternion.Euler(0,0,j * -Degree) * vector0;
                vector[i].z = 0f;

                //弾インスタンスを取得し、初速と発射角度を与える
                GameObject Bullet_obj = 
                    (GameObject)Instantiate(bulletL, transform.position, transform.rotation);
                EnemyBullet enemyBullet = Bullet_obj.GetComponent<EnemyBullet>();
                enemyBullet.SetSpeed(speed*2);
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
    //  Phase2:警告を出す
    //------------------------------------------------------------------
    private IEnumerator Warning(DoujiPhase2Bullet.KooniDirection direction, Vector2 pos)
    {
        int loop_count = 2;
        float flash_interval = 0.5f;

        //  SEを再生
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX_SYSTEM,
            (int)SFXList.SFX_DOUJI_WARNING);

        //点滅ループ開始
        for (int i = 0; i < loop_count; i++)
        {
            //  座標を設定
            warningObject[(int)direction].GetComponent<RectTransform>()
                .anchoredPosition = pos;

            //  有効化
            warningObject[(int)direction].SetActive(true);

            //  flashInterval待ってから
            yield return new WaitForSeconds(flash_interval);

            //  無効化
            warningObject[(int)direction].SetActive(false);

            //  flashInterval待ってから
            yield return new WaitForSeconds(flash_interval);
        }

        yield return null;
    }

    //------------------------------------------------------------------
    //  Phase2:ランダムなスポナーから子鬼が突撃してくる
    //------------------------------------------------------------------
    private IEnumerator KooniParty()
    {
        int fourDirection = -1;

        while(true)
        {
            //  まずは４方向で抽選
            fourDirection  = Random.Range(0,4);

            //  番号が使用済みではなかったら
            if(kooniNum[fourDirection] == -1)
            {
                //  今回の番号を記録
                kooniNum[fourDirection] = fourDirection;
                break;
            }
        }

        //  発生確率の閾値で呼び分ける
        if (fourDirection == 0)         //  上方向とする
        {
            //  ギミック弾のプレハブを取得
            GameObject bullet = EnemyManager.Instance
                .GetBulletPrefab((int)BULLET_TYPE.Douji_Gimmick_Top);
            //  ３箇所で抽選
            int rand = Random.Range(0,3);
            Vector3 pos = default;
            if(rand == 0)       //  左
            {
                pos = EnemyManager.Instance.GetSpawnerPos(0);

                //  Warning!表示
                yield return StartCoroutine(
                    Warning(
                        DoujiPhase2Bullet.KooniDirection.TOP,
                        new Vector2(-375,300)
                        ));
            }
            else if(rand == 1)  //  中
            {
                pos = EnemyManager.Instance.GetSpawnerPos(1);

                //  Warning!表示
                yield return StartCoroutine(
                    Warning(
                        DoujiPhase2Bullet.KooniDirection.TOP,
                        new Vector2(-60,300)
                        ));
            }
            else if(rand == 2)  //  右
            {
                pos = EnemyManager.Instance.GetSpawnerPos(2);

                //  Warning!表示
                yield return StartCoroutine(
                    Warning(
                        DoujiPhase2Bullet.KooniDirection.TOP,
                        new Vector2(250,300)
                        ));
            }
            GameObject obj = Instantiate(bullet,pos,Quaternion.identity);
            DoujiPhase2Bullet bulletComp =  obj.GetComponent<DoujiPhase2Bullet>();
            bulletComp.SetPower(enemyData.Attack);
            bulletComp.SetDirection(DoujiPhase2Bullet.KooniDirection.TOP);

            //  子鬼が突撃する
            yield return StartCoroutine(bulletComp.KooniRush());

            //  リセット
            kooniNum[fourDirection] = -1;
        }
        else if (fourDirection == 1)    //  下方向とする
        {
            //  ギミック弾のプレハブを取得
            GameObject bullet = EnemyManager.Instance
                .GetBulletPrefab((int)BULLET_TYPE.Douji_Gimmick_Bottom);
            //  ３箇所で抽選
            int rand = Random.Range(0,3);
            Vector3 pos = default;
            if(rand == 0)       //  左
            {
                pos = EnemyManager.Instance.GetSpawnerPos(8);

                //  Warning!表示
                yield return StartCoroutine(
                    Warning(
                        DoujiPhase2Bullet.KooniDirection.BOTTOM,
                        new Vector2(-375,-300)
                        ));
            }
            else if(rand == 1)  //  中
            {
                pos = EnemyManager.Instance.GetSpawnerPos(7);

                //  Warning!表示
                yield return StartCoroutine(
                    Warning(
                        DoujiPhase2Bullet.KooniDirection.BOTTOM,
                        new Vector2(-60,-300)
                        ));
            }
            else if(rand == 2)  //  右
            {
                pos = EnemyManager.Instance.GetSpawnerPos(6);

                //  Warning!表示
                yield return StartCoroutine(
                    Warning(
                        DoujiPhase2Bullet.KooniDirection.BOTTOM,
                        new Vector2(250,-300)
                        ));
            }
            GameObject obj = Instantiate(bullet,pos,Quaternion.identity);
            DoujiPhase2Bullet bulletComp =  obj.GetComponent<DoujiPhase2Bullet>();
            bulletComp.SetPower(enemyData.Attack);
            bulletComp.SetDirection(DoujiPhase2Bullet.KooniDirection.BOTTOM);

            //  子鬼が突撃する
            yield return StartCoroutine(bulletComp.KooniRush());

            //  リセット
            kooniNum[fourDirection] = -1;
        }
        else if (fourDirection == 2)    //  左方向とする
        {
            //  ギミック弾のプレハブを取得
            GameObject bullet = EnemyManager.Instance
                .GetBulletPrefab((int)BULLET_TYPE.Douji_Gimmick_Left);
            //  ３箇所で抽選
            int rand = Random.Range(0,3);
            Vector3 pos = default;
            if(rand == 0)       //  上
            {
                pos = EnemyManager.Instance.GetSpawnerPos(11);

                //  Warning!表示
                yield return StartCoroutine(
                    Warning(
                        DoujiPhase2Bullet.KooniDirection.LEFT,
                        new Vector2(-430,180)
                        ));
            }
            else if(rand == 1)  //  中
            {
                pos = EnemyManager.Instance.GetSpawnerPos(10);

                //  Warning!表示
                yield return StartCoroutine(
                    Warning(
                        DoujiPhase2Bullet.KooniDirection.LEFT,
                        new Vector2(-430,-15)
                        ));
            }
            else if(rand == 2)  //  下
            {
                pos = EnemyManager.Instance.GetSpawnerPos(9);

                //  Warning!表示
                yield return StartCoroutine(
                    Warning(
                        DoujiPhase2Bullet.KooniDirection.LEFT,
                        new Vector2(-430,-220)
                        ));
            }
            GameObject obj = Instantiate(bullet,pos,Quaternion.identity);
            DoujiPhase2Bullet bulletComp =  obj.GetComponent<DoujiPhase2Bullet>();
            bulletComp.SetPower(enemyData.Attack);
            bulletComp.SetDirection(DoujiPhase2Bullet.KooniDirection.LEFT);

            //  子鬼が突撃する
            yield return StartCoroutine(bulletComp.KooniRush());

            //  リセット
            kooniNum[fourDirection] = -1;
        }
        else if (fourDirection == 3)    //  右方向とする
        {
            //  ギミック弾のプレハブを取得
            GameObject bullet = EnemyManager.Instance
                .GetBulletPrefab((int)BULLET_TYPE.Douji_Gimmick_Right);
            //  ３箇所で抽選
            int rand = Random.Range(0,3);
            Vector3 pos = default;
            if(rand == 0)       //  上
            {
                pos = EnemyManager.Instance.GetSpawnerPos(3);

                //  Warning!表示
                yield return StartCoroutine(
                    Warning(
                        DoujiPhase2Bullet.KooniDirection.RIGHT,
                        new Vector2(300,185)
                        ));
            }
            else if(rand == 1)  //  中
            {
                pos = EnemyManager.Instance.GetSpawnerPos(4);

                //  Warning!表示
                yield return StartCoroutine(
                    Warning(
                        DoujiPhase2Bullet.KooniDirection.RIGHT,
                        new Vector2(300,-15)
                        ));
            }
            else if(rand == 2)  //  下
            {
                pos = EnemyManager.Instance.GetSpawnerPos(5);

                //  Warning!表示
                yield return StartCoroutine(
                    Warning(
                        DoujiPhase2Bullet.KooniDirection.RIGHT,
                        new Vector2(300,-215)
                        ));
            }
            GameObject obj = Instantiate(bullet,pos,Quaternion.identity);
            DoujiPhase2Bullet bulletComp =  obj.GetComponent<DoujiPhase2Bullet>();
            bulletComp.SetPower(enemyData.Attack);
            bulletComp.SetDirection(DoujiPhase2Bullet.KooniDirection.RIGHT);

            //  子鬼が突撃する
            yield return StartCoroutine(bulletComp.KooniRush());

            //  リセット
            kooniNum[fourDirection] = -1;
        }

        yield return null;
    }

   //-------------------------------------------------------------------
    //  ドウジの発狂弾生成処理
    //------------------------------------------------------------------
    private IEnumerator GenerateBerserkBullet(float duration)
    {
        //  発狂弾のプレハブを取得
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Douji_Berserk_Bullet);

        //  弾を生成
        GameObject obj = Instantiate(bullet,transform.position,Quaternion.identity);
        DoujiPhase3Bullet enemyBullet = obj.GetComponent<DoujiPhase3Bullet>();

        enemyBullet.SetPower(enemyData.Attack);

        //  SEを再生
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.ENEMY_SHOT,
            (int)SFXList.SFX_DOUJI_SHOT1);

        yield return new WaitForSeconds(duration);
    }

    //------------------------------------------------------------------
    //  ドウジの発狂弾
    //------------------------------------------------------------------
    private IEnumerator Douji_LoopMoveBerserk(int bulletNum,float duration,float interval)
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

        //  発狂弾のプレハブを取得
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Douji_Berserk_Bullet);

        //  弾を生成
        yield return StartCoroutine(GenerateBerserkBullet(duration/bulletNum));

        yield return StartCoroutine(GenerateBerserkBullet(duration/bulletNum));

        yield return StartCoroutine(GenerateBerserkBullet(duration/bulletNum));

        //  現在の番号を更新
        currentlNum = targetNum;

        //  次の移動まで待つ
        yield return new WaitForSeconds(interval);
    }

    //------------------------------------------------------------------
    //  ドウジの発狂弾幕
    //------------------------------------------------------------------
    private IEnumerator Douji_BerserkBarrage()
    {
        //  通常バラマキ弾のプレハブを取得
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Wildly_Big);

        float totalDegree = 180;        //  撃つ範囲の総角  
        int wayNum = 9;                 //  弾のway数(必ず3way以上の奇数にすること)
        float Degree = totalDegree / (wayNum-1);     //  弾一発毎にずらす角度         
        float speed = 10.0f;             //  弾速
        int chain = 5;                  //  連弾数
        float chainInterval = 0.3f;     //  連弾の間隔（秒）

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

        yield return null;
    }

    //------------------------------------------------------------------
    //  自機狙い発狂ガトリングショット
    //------------------------------------------------------------------
    private IEnumerator Douji_BerserkGatling()
    {
        //  弾のプレハブを取得
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Snipe_Big);

        int wayNum = 3;                 //  弾のway数
        float Degree = 20;              //  ずらす角度
        int chain = 2;                 //  連弾数         
        float speed = 10.0f;            //  弾速
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
