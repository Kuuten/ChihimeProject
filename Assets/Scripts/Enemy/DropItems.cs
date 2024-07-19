using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.ResourceManagement.AsyncOperations;

//--------------------------------------------------------------
//
//  敵にアイテムをドロップさせるクラス
//
//--------------------------------------------------------------

// ドロップアイテムの種類 
public enum Items
{
    smallKon,
    largeKon,
    PowerUp,     //  弾のレベルアップ
    SpeedUp,     //  自機のスピードアップ
    
    Max,
}

public class DropItems : MonoBehaviour
{
    //  プレハブのアドレス
    private string[] adress =
    {
        "item_smallKon",    //  Items.smallKon
        "item_largeKon",    //  Items.largeKon
        "item_powerup",     //  Items.PowerUp
        "item_speedup"      //  Items.SpeedUp
    };
    
    //  アイテムのプレハブ格納用
    List<GameObject> Prefabs = new List<GameObject>();

    private void Start()
    {
        Prefabs = EnemyManager.Instance.GetDropItems();
    }

    //------------------------------------------------------------
    //  ドロップ持ちの敵が死んだ時にランダムな
    //  パワーアップアイテムをドロップする
    //------------------------------------------------------------
    public void DropPowerupItem()
    {
        //  ドロップなしならリターン
        if(EnemyManager.Instance.GetDropType() == EnemyManager.DROP_TYPE.None)return;

        //  ランダムなアイテムを生成する
        int rand = Random.Range(
            (int)Items.PowerUp,
            (int)Items.Max);

        //  敵がやられた場所に生成する
        Vector3 pos = this.transform.position;
        Instantiate(Prefabs[rand], pos, Quaternion.identity);
    }

    //------------------------------------------------------------
    //  num個分魂をドロップする
    //------------------------------------------------------------
    public void DropKonPrefab(GameObject Prefab, int num)
    {
        //  敵がやられた場所に生成する
        Vector3 pos = this.transform.position;

        for(int i=0;i<num;i++)
        {
            GameObject obj = Instantiate(
                Prefab,
                pos,
                Quaternion.identity);

            //  実際のシーン上での横幅を取得
            SpriteRenderer sprite = obj.GetComponent<SpriteRenderer>();
            float spriteWidth = sprite.bounds.size.x;
            
            //  横幅分ずつずらして配置する
            pos.y = this.transform.position.y + Random.Range(-2,2);
            pos.x = (transform.position.x - num * spriteWidth / 2) + i * spriteWidth;

            //  再配置
            obj.transform.position = pos;
        }
    }

    //------------------------------------------------------------
    //  落とす魂の金額から計算して魂を生成する
    //------------------------------------------------------------
    public void DropKon(int money)
    {
        //  大魂１個あたりの魂獲得量を取得
        int largeKon = MoneyManager.Instance.GetKonNumGainedFromLarge();

        if (money < largeKon)
        {
            //  smallKonの数だけ小魂を生成
            int smallKon = money;
            Debug.Log("小魂の数でばっぐ :" + smallKon);
            DropKonPrefab(Prefabs[(int)Items.smallKon], smallKon);
        }
        else // moneyがlargeKon以上の場合
        {
            //  大魂から計算する
            int largeKonNum = money / largeKon;

            //  余りで小魂を計算する
            int remainder = money % largeKon;

            Debug.Log("大魂の数 :" + largeKonNum);
            Debug.Log("小魂の数 :" + remainder);

            //  largeKonNumの数だけ大魂を生成
            DropKonPrefab(Prefabs[(int)Items.largeKon], largeKonNum);

            //  remainderの数だけ小魂を生成
            DropKonPrefab(Prefabs[(int)Items.smallKon], remainder);
        }
    }
}
