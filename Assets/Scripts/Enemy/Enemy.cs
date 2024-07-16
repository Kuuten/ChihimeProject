using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;

//--------------------------------------------------------------
//
//  ザコ敵の基本クラス
//
//--------------------------------------------------------------
public class Enemy : MonoBehaviour
{
    // 動きの種類
    public enum MOVE_TYPE {
        Clamp,                      // カクカク移動
        ChargeToPlayer,             // プレイヤーに突進
        Straight,                   // 直線移動
        AdjustLine,                 // 軸合わせ
        Curve                       // 放物線
    }
    // 動きの種類
    public MOVE_TYPE type = MOVE_TYPE.Clamp;

    //  出現ポイント
    [SerializeField] private GameObject[] spawner;
    //  制御点
    [SerializeField] private GameObject[] controlPoint;
    
    [SerializeField] private float moveSpeed = 1.0f;
    [SerializeField] private GameObject explosion;

    //  もらえるお金
    [SerializeField] private int money = 50;

    private float period = 20;  //  敵の寿命（秒）

    private float cycleCount = 0.1f; // １秒間に往復する回数
    private float curveLength = 1;   // カーブの最大距離
    private float cycleRadian = 0;   // サインに渡す値
    private float centerX;           // X座標の中心

    private bool visible = false;

    //  ステータス
    private int Id;     //  ID
    private int Hp;     //  HP
    private int Attack; //  攻撃力
    private int Money;  //  落とす金額（魂）


    void Start()
    {
        visible = false;

        // 初期Y座標を「X座標の中心」として保存
        centerX = transform.position.x;

        //  寿命を設定
        Destroy(this.gameObject, period);

        //  ステータスを設定(EnemySettingから取得)
        Id = -1;
        Hp = -1;
        Attack = -1;
        Money = -1;

        ////  敵データを取得
        //enemySetting = await Addressables.LoadAssetAsync<EnemySetting>("EnemySetting");

        //var EnemyData = enemySetting.DataList
        //    .FirstOrDefault(enemy => enemy.Id == "Kooni");
        //Debug.Log($"ID：{EnemyData.Id}");
        //Debug.Log($"HP：{EnemyData.Hp}");
        //Debug.Log($"攻撃力：{EnemyData.Attack}");
        //Debug.Log($"落魂：{EnemyData.Money}");
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
            //  DropItemsがある場合はアイテムドロップ
            DropItems drop = this.GetComponent<DropItems>();
            if(drop)drop.DropPowerupItem();

            //  魂は常にドロップ

            //  お金を生成
            DropMoneyItems();

            //  プレイヤーのお金を加算
            //scoreManager.AddMoney(100);

            


        }
        else if(collision.CompareTag("ConverterBullet"))
        {
            //  DropItemsがある場合はアイテムドロップ
            DropItems drop = this.GetComponent<DropItems>();
            if(drop)drop.DropPowerupItem();

            //  お金を生成

            //  プレイヤーのお金を加算
            //scoreManager.AddMoney(100);

        }
        else return;

        //  爆発
        Instantiate(explosion, transform.position, transform.rotation);
        
        Destroy(this.gameObject);
        Destroy(collision.gameObject);
    }

    //  お金を生成
    private void DropMoneyItems()
    {
        //  敵の落とす金額を取得

        //  魂アイテムをドロップさせる
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
