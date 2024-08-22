using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using System.Linq;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine.Experimental.GlobalIllumination;
using System.Threading;

//--------------------------------------------------------------
//
//  ザコ敵の基本クラス
//
//--------------------------------------------------------------
public class Enemy : MonoBehaviour
{
    // 動きの種類
    [SerializeField] private enum MOVE_TYPE {
        None,                       // 未設定
        Clamp,                      // カクカク移動
        ChargeToPlayer,             // プレイヤーに突進
        Straight,                   // 直線移動
        AdjustLine,                 // 軸合わせ
        Curve                       // 放物線
    }
    // 動きの種類
    [SerializeField] private MOVE_TYPE moveType = MOVE_TYPE.None;

    // ザコ敵の種類(名前をEnemySettingのIdと同じにする)
    public enum ENEMY_TYPE {
        None,                       //  未設定
        Kooni,                      //  子鬼
        Onibi,                      //  鬼火
        Ibarakidouji,               //  茨木童子
        Douji                       //  ドウジ
    }
    // ザコ敵の種類
    public ENEMY_TYPE enemyType = ENEMY_TYPE.None;

    //  EnemyDataクラスからの情報取得用
    EnemyData enemyData;

    //  出現ポイント(GameManagerからGetterで受け取る)
    private GameObject[] spawner;
    //  制御点(GameManagerからGetterで受け取る)
    private GameObject[] controlPoint;
    
    //  パラメータ
    [SerializeField] private float moveSpeed = 1.0f;
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

    //----------------------------------------------------
    //  移動経路用スポナー＆制御点のトランスフォーム
    //----------------------------------------------------
    private Transform[] spawners;       //  スポナー
    private Transform[] controlPoints;  //  制御点


    private void Start()
    {
        //  カメラに写っていない
        visible = false;
        //  死亡フラグOFF
        bDeath = false;
        //  最初は無敵モードOFF
        bSuperMode = false;
        //  ループカウントを設定
        loopCount = 1;
        //  点滅の間隔を設定
        flashInterval = 0.1f;

        //  敵情報のアサーション（満たさなければいけない条件）
        Assert.IsTrue(enemyType.ToString() != ENEMY_TYPE.None.ToString(),
            "EnemyTypeがインスペクターで設定されていません！");

        //  SpriteRenderを取得
        sp = GetComponent<SpriteRenderer>();
    }

    //  敵のデータを設定 
    public void SetEnemyData(EnemySetting es)
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
    }

    private void OnDestroy()
    {
        //  破壊された敵数を+1
        int destroyNum = EnemyManager.Instance.GetDestroyNum();
        destroyNum++;
        EnemyManager.Instance.SetDestroyNum(destroyNum);
        Debug.Log("破壊された敵数 : " + destroyNum);
    }

    void Update()
    {
        Vector3 pos = transform.position;

        //  前進
        pos += new Vector3(
            0,
            moveSpeed * Time.deltaTime,
            0
            );
        transform.position = pos;
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
            StartCoroutine(Blink(false,loopCount,flashInterval));

            //  死亡フラグON
            if(hp <= 0)
            {
                bDeath = true;
                Death();       //  やられ演出
            }
        }
        else if (collision.CompareTag("DoujiConvert"))
        {
            //  ダメージ処理
            float d = GameManager.Instance.GetPlayer()
                .GetComponent<PlayerShotManager>().GetConvertShotPower();
            Damage(d);

            //  点滅演出
            StartCoroutine(Blink(false,loopCount,flashInterval));

            //  死亡フラグON
            if(hp <= 0)
            {
                bDeath = true;
                Death2();       //  やられ演出2
            }
        }
        else if (collision.CompareTag("DoujiKonburst"))
        {
            //  ダメージ処理
            float d = GameManager.Instance.GetPlayer()
                .GetComponent<PlayerBombManager>().GetKonburstShotPower();
            Damage(d);

            //  点滅演出
            StartCoroutine(Blink(false,loopCount,flashInterval));

            //  死亡フラグON
            if(hp <= 0)
            {
                bDeath = true;
                Death2();       //  やられ演出2
            }
        }
        else if (collision.CompareTag("Bomb"))
        {
            //  ダメージ処理
            float d = GameManager.Instance.GetPlayer()
                .GetComponent<PlayerBombManager>().GetBombPower();
            Damage(d);

            //  点滅演出
            StartCoroutine(Blink(false,loopCount,flashInterval));

            //  死亡フラグON
            if(hp <= 0)
            {
                bDeath = true;
                Death();       //  やられ演出
            }
        }

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
    //  やられ演出(通常弾)
    //-------------------------------------------
    private void Death()
    {
        //  やられエフェクト
        Instantiate(explosion, transform.position, transform.rotation);

        //  アイテムドロップ判定
        DropItems drop = this.GetComponent<DropItems>();
        if (drop) drop.DropPowerupItem();

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
        if (drop) drop.DropPowerupItem();

        //  お金を生成(魂バートの時は2倍)
        int dropMoney = enemyData.Money;
        drop.DropKon(2 * dropMoney);

        //  オブジェクトを削除
        Destroy(this.gameObject);
    }

    //******************************************************************
    //
    //  敵の移動パターン
    //
    //******************************************************************

    //------------------------------------------------------------------
    //  クランプ移動（スタート位置を左右切り替え可能）
    //------------------------------------------------------------------

    //------------------------------------------------------------------
    //  一定距離まで直進の後、自機狙いで突っ込む
    //------------------------------------------------------------------

    //------------------------------------------------------------------
    //  直線移動(スポナーから反対側のスポナーへ移動)
    //------------------------------------------------------------------

    //------------------------------------------------------------------
    //  直線移動中にプレイヤーとXで軸かY軸が合えば自機に突撃
    //------------------------------------------------------------------

    //------------------------------------------------------------------
    //  放物線移動
    //------------------------------------------------------------------

    //------------------------------------------------------------------
    //  放物線移動の後弾を撃って放物線移動で消えていく(中級ザコ)
    //------------------------------------------------------------------
}
