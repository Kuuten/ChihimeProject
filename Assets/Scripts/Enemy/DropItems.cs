using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using UnityEngine.ResourceManagement.AsyncOperations;

//--------------------------------------------------------------
//
//  敵にアイテムをドロップさせるクラス
//
//--------------------------------------------------------------

// 魂アイテムの種類 
public enum KonItems
{
    smallKon,
    largeKon,
    
    Max
}

// パワーアップアイテムの種類 
public enum ePowerupItems
{
    None = -2,
    Random,      // ランダム
    PowerUp,     //  弾のレベルアップ
    SpeedUp,     //  自機のスピードアップ
    Heal,        //  ハートを回復
    Bomb,        //  ボムが１UP
    
    Max
}

public class DropItems : MonoBehaviour
{
    //  プレハブのアドレス
    private string[] adress =
    {
        "item_smallKon",    //  Items.smallKon
        "item_largeKon",    //  Items.largeKon
        "item_powerup",     //  Items.PowerUp
        "item_speedup",     //  Items.SpeedUp
        "item_heart",       //  Items.Heal
        "item_bomb"         //  Items.Bomb
    };
    
    //  アイテムのプレハブ格納用
    List<GameObject> konPrefabs = new List<GameObject>();
    List<GameObject> powerupPrefabs = new List<GameObject>();

    //  パワーアップアイテムを落とすかどうか
    [SerializeField] EnemyManager.DROP_TYPE dropType;

    private void Start()
    {
        konPrefabs = EnemyManager.Instance.GetKonItems();
        powerupPrefabs = EnemyManager.Instance.GetPowerupItems();
    }

    //------------------------------------------------------------
    //  ドロップ持ちの敵が死んだ時にランダムな
    //  パワーアップアイテムをドロップする
    //------------------------------------------------------------
    public void DropRandomPowerupItem()
    {
        //  ドロップなしならリターン
        if(dropType == EnemyManager.DROP_TYPE.None)return;

        //  ランダムなアイテムを生成する
        int rand = Random.Range(
            (int)ePowerupItems.PowerUp,
            (int)ePowerupItems.Max);

        //  敵がやられた場所に生成する
        Vector3 pos = this.transform.position;
        Instantiate(powerupPrefabs[rand], pos, Quaternion.identity);
    }

    //------------------------------------------------------------
    //  ドロップ持ちの敵が死んだ時に指定されたs
    //  パワーアップアイテムをドロップする
    //------------------------------------------------------------
    public void DropPowerupItem(ePowerupItems item)
    {
        //  ドロップなしならリターン
        if(dropType == EnemyManager.DROP_TYPE.None)return;

        if(item == ePowerupItems.None || item == ePowerupItems.Random)
            return;

        //  敵がやられた場所に生成する
        Vector3 pos = this.transform.position;
        Instantiate(powerupPrefabs[(int)item], pos, Quaternion.identity);
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
        int smallKon = MoneyManager.Instance.GetKonNumGainedFromSmall();

        if (money < largeKon)
        {
            //  smallKonの数だけ小魂を生成
            int small = money / smallKon;
            Debug.Log("小魂の数 :" + small);
            DropKonPrefab(konPrefabs[(int)KonItems.smallKon], small);
        }
        else // moneyがlargeKon以上の場合
        {
            //  大魂から計算する
            int largeKonNum = money / largeKon;

            //  余りで小魂を計算する
            int remainder = money % largeKon;
            int small = remainder / smallKon;

            Debug.Log("大魂の数 :" + largeKonNum);
            Debug.Log("小魂の数 :" + small);

            //  largeKonNumの数だけ大魂を生成
            DropKonPrefab(konPrefabs[(int)KonItems.largeKon], largeKonNum);

            //  remainderの数だけ小魂を生成
            DropKonPrefab(konPrefabs[(int)KonItems.smallKon], small);
        }
    }
}
