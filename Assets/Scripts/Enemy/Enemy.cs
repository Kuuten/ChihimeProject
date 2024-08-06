using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using System.Linq;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine.Experimental.GlobalIllumination;

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

    //  敵情報
    EnemySetting enemySetting;

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

    //  EnemyDataクラスからの情報取得用
    EnemyData enemyData;

    //  点滅させるためのSpriteRenderer
    SpriteRenderer sp;
    //  点滅の間隔
    private float flashInterval;
    //  点滅させるときのループカウント
    private int loopCount;


    private async UniTask Start()
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
        flashInterval = 0.01f;

        //  敵情報のアサーション（満たさなければいけない条件）
        Assert.IsTrue(enemyType.ToString() != ENEMY_TYPE.None.ToString(),
            "EnemyTypeがインスペクターで設定されていません！");

        //GameObjectが破棄された時にキャンセルを飛ばすトークンを作成
        var token = this.GetCancellationTokenOnDestroy();

        //  SpriteRenderを取得
        sp = GetComponent<SpriteRenderer>();

        //  敵データを取得
        enemySetting = await Addressables.LoadAssetAsync<EnemySetting>("EnemySetting")
            .WithCancellation(token);
        enemyData = enemySetting.DataList
            .FirstOrDefault(enemy => enemy.Id == enemyType.ToString() );

        //  体力を設定
        hp = enemyData.Hp;

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

         // 解放
         if(enemySetting != null)
        {
            Addressables.Release(enemySetting);
            enemySetting = null;
        }
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
    public void SetHp(float health){ hp = health; }
    public float GetHp(){ return hp; }

    //  敵に当たったら爆発する
    //  当たり判定の基礎知識：
    //  当たり判定を行うには、
    //  ・両者にColliderがついている
    //  ・どちらかにRigidBodyがついている
    private async void OnTriggerEnter2D(Collider2D collision)
    {
        if(!visible || bSuperMode || bDeath)return;

        if (collision.CompareTag("DeadWall"))
        {
            Destroy(this.gameObject);
        }
        else if (collision.CompareTag("NormalBullet"))
        {
            //  弾の消去
            Destroy(collision.gameObject);

            //  ダメージ処理
            float d = GameManager.Instance.GetPlayer()
                .GetComponent<PlayerShotManager>().GetNormalShotPower();
            Damage(d);

            //  点滅演出
            var task = Blink();
            await task;

            //  死亡フラグON
            if(hp <= 0)
            {
                bDeath = true;
                Death();       //  やられ演出
            }
        }
        else if (collision.CompareTag("ConverterBullet"))
        {
            //  ダメージ処理

            //  点滅演出
            var task = Blink();
            await task;

            //  死亡フラグON
            if(hp <= 0)
            {
                bDeath = true;
                Death2();       //  やられ演出2
            }
        }

    }

    //  お金を生成
    private void DropMoneyItems(int money)
    {
        DropItems drop = this.GetComponent<DropItems>();
        if(!drop)return;

        //  魂アイテムをドロップさせる
        drop.DropKon(money);
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
    private async UniTask Blink()
    {
        //  無敵モードON
        bSuperMode = true;

        //GameObjectが破棄された時にキャンセルを飛ばすトークンを作成
        var token = this.GetCancellationTokenOnDestroy();

        //点滅ループ開始
        for (int i = 0; i < loopCount; i++)
        {
            //flashInterval待ってから
            await UniTask.Delay (TimeSpan.FromSeconds(flashInterval))
                .AttachExternalCancellation(token);

            //spriteRendererをオフ
            sp.enabled = false;

            //flashInterval待ってから
            await UniTask.Delay(TimeSpan.FromSeconds(flashInterval))
                .AttachExternalCancellation(token);

            //spriteRendererをオン
            sp.enabled = true;
        }
        //  無敵モードOFF
        bSuperMode = false;
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
        DropMoneyItems(enemyData.Money);

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
        DropMoneyItems(2 * dropMoney);

        //  オブジェクトを削除
        Destroy(this.gameObject);
    }


}
