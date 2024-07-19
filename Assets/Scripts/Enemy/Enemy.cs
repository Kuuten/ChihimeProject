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
    
    [SerializeField] private float moveSpeed = 1.0f;
    [SerializeField] private GameObject explosion;

    private float period = 20;  //  敵の寿命（秒）

    private bool visible = false;

    //  EnemyDataクラスからの情報取得用
    EnemyData enemyData;


    private async UniTask Start()
    {
        visible = false;

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
            //  やられエフェクト
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

            //  アイテムドロップ判定
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

            //  アイテムドロップ判定
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
}
