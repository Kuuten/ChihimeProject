using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using System.Linq;
using Cysharp.Threading.Tasks;

//--------------------------------------------------------------
//
//  ザコ敵の基本クラス
//
//--------------------------------------------------------------
public class Enemy : MonoBehaviour
{
    // 動きの種類
    public enum MOVE_TYPE {
        None,                       // 未設定
        Clamp,                      // カクカク移動
        ChargeToPlayer,             // プレイヤーに突進
        Straight,                   // 直線移動
        AdjustLine,                 // 軸合わせ
        Curve                       // 放物線
    }
    // 動きの種類
    public MOVE_TYPE moveType = MOVE_TYPE.None;

    // ザコ敵の種類(名前をEnemySettingのIdと同じにする)
    public enum ENEMY_TYPE {
        None,                       //  未設定
        Kooni,                      //  子鬼
        Onibi,                      //  鬼火
        Ibarakidouji,               //  茨木童子
        Douji                       //  ドウジ
    }
    // 動きの種類
    public ENEMY_TYPE enemyType = ENEMY_TYPE.None;

    //  敵情報
    EnemySetting enemySetting;

    //  出現ポイント(GameManagerからGetterで受け取る)
    private GameObject[] spawner;
    //  制御点(GameManagerからGetterで受け取る)
    private GameObject[] controlPoint;
    
    [SerializeField] private float moveSpeed = 1.0f;
    [SerializeField] private GameObject explosion;

    private float period = 20;  //  敵の寿命（秒）

    private float cycleCount = 0.1f; // １秒間に往復する回数
    private float curveLength = 1;   // カーブの最大距離
    private float cycleRadian = 0;   // サインに渡す値
    private float centerX;           // X座標の中心

    private bool visible = false;

    //  EnemyDataクラスからの情報取得用
    EnemyData enemyData;


    private async UniTask Start()
    {
        visible = false;

        // 初期Y座標を「X座標の中心」として保存
        centerX = transform.position.x;

        //  寿命を設定
        Destroy(this.gameObject, period);

        //  敵情報のアサーション（満たさなければいけない条件）
        Assert.IsTrue(enemyType.ToString() != ENEMY_TYPE.None.ToString(),
            "EnemyTypeがインスペクターで設定されていません！");

        //  敵データを取得
        enemySetting = await Addressables.LoadAssetAsync<EnemySetting>("EnemySetting");
        enemyData = enemySetting.DataList
            .FirstOrDefault(enemy => enemy.Id == enemyType.ToString() );
        //Debug.Log($"ID：{enemyData.Id}");
        //Debug.Log($"HP：{enemyData.Hp}");
        //Debug.Log($"攻撃力：{enemyData.Attack}");
        //Debug.Log($"落魂：{enemyData.Money}");
    }

    private void OnDestroy()
    {
                // 解放
        Addressables.Release(enemySetting);
    }

    void Update()
    {
        Vector3 pos = transform.position;

        //// 上下にカーブ
        //if (type == MOVE_TYPE.CURVE)
        //{
        //    if (cycleCount > 0)
        //    {
        //        cycleRadian += (cycleCount * 2 * Mathf.PI) / 50;
        //        pos.x = Mathf.Sin(cycleRadian) * curveLength + centerX;
        //    }
        //}

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


    //  敵に当たったら爆発する
    //  当たり判定の基礎知識：
    //  当たり判定を行うには、
    //  ・両者にColliderがついている
    //  ・どちらかにRigidBodyがついている
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!visible)return;

        if(collision.CompareTag("Player"))
        {
            Instantiate(
                    explosion,
                    collision.transform.position,
                    collision.transform.rotation
                );
        }
        else if(collision.CompareTag("NormalBullet"))
        {
            //  点滅させる

            //  ダメージ処理

            
            //  やられエフェクト
            Instantiate(explosion, transform.position, transform.rotation);

            //  オブジェクトを削除
            Destroy(this.gameObject);

            //  DropItemsがある場合はアイテムドロップ
            DropItems drop = this.GetComponent<DropItems>();
            if(drop)drop.DropPowerupItem();

            //  お金を生成
            DropMoneyItems(enemyData.Money);
        }
        else if(collision.CompareTag("ConverterBullet"))
        {
            //  点滅させる

            //  ダメージ処理

            //  やられエフェクト
            Instantiate(explosion, transform.position, transform.rotation);

            //  オブジェクトを削除
            Destroy(this.gameObject);

            //  DropItemsがある場合はアイテムドロップ
            DropItems drop = this.GetComponent<DropItems>();
            if(drop)drop.DropPowerupItem();

            //  お金を生成(魂バートの時は2倍)
            int dropMoney = enemyData.Money;
            DropMoneyItems(2 * dropMoney);
        }
        else return;
        
        

        //  プレイヤーの仮やられ演出
        Destroy(collision.gameObject);
    }

    //  お金を生成
    private void DropMoneyItems(int money)
    {
        DropItems drop = this.GetComponent<DropItems>();
        if(!drop)return;

        //  魂アイテムをドロップさせる
        drop.DropKon(money);
    }

    //  Wave移動
    //private void WaveMove()
    //{
    //    // 左右にウェーブ
    //    if (type == MOVE_TYPE.Wave)
    //    {
    //        if (cycleCount > 0)
    //        {
    //            cycleRadian += (cycleCount * 2 * Mathf.PI) / 50;
    //            pos.x = Mathf.Sin(cycleRadian) * curveLength + centerX;
    //        }
    //    }
    //}
}
