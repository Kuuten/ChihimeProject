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

    //  ステータス-----------------------------------------------------------
    private string id;    //  ID
    private float hp;     //  HP
    private float attack; //  攻撃力
    private int money;    //  落とす金額（魂）
    //-----------------------------------------------------------------------


    private async UniTask Start()
    {
        visible = false;

        // 初期Y座標を「X座標の中心」として保存
        centerX = transform.position.x;

        //  寿命を設定
        Destroy(this.gameObject, period);

        //  ステータスを設定(EnemySettingから取得)
        id = "none";
        hp = 1f;
        attack = 1f;
        money = -1;

        //  敵データを取得
        enemySetting = await Addressables.LoadAssetAsync<EnemySetting>("EnemySetting");

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

    //----------------------------------------------------------------------
    //  プロパティ
    //----------------------------------------------------------------------
    public string GetId(){ return id; }
    public void SetId(string id){ this.id = id; }
    public float GetHp(){ return this.hp; }
    public void SetHp(float hp){ this.hp = hp; }
    public float GetAttack(){ return this.attack; }
    public void SetAttack(float attack){  this.attack = attack; }
    public int GetMoney(){ return this.money; }
    public void SetMoney(int money){ this.money = money; }


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
            DropMoneyItems();

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
        DropItems drop = this.GetComponent<DropItems>();
        if(!drop)return;

        //  敵の落とす金額を取得
        drop.DropKon(100);

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
