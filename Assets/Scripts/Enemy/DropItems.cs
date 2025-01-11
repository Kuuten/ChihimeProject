using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using UnityEngine.ResourceManagement.AsyncOperations;
using static EnemyManager;

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
    Ossan,       //  ちっちゃいおっさん
    
    Max
}

public class DropItems : MonoBehaviour
{  
    //  アイテムのプレハブ格納用
    GameObject SmallKonPrefab;
    GameObject LargeKonPrefab;
    List<GameObject> powerupPrefabs;
    List<GameObject> speedupPrefabs;
    List<GameObject> bombPrefabs;
    List<GameObject> healPrefabs;

    //  パワーアップアイテムを落とすかどうか
    [SerializeField] EnemyManager.DROP_TYPE dropType;

    void Start()
    {
        //SmallKonPrefab = new GameObject();
        //LargeKonPrefab = new GameObject();
        powerupPrefabs = new List<GameObject>();
        speedupPrefabs = new List<GameObject>();
        bombPrefabs = new List<GameObject>();
        healPrefabs = new List<GameObject>();


        SmallKonPrefab = EnemyManager.Instance.GetSmallKon();
        LargeKonPrefab = EnemyManager.Instance.GetLargeKon();
        powerupPrefabs = EnemyManager.Instance.GetPowerupItems();
        speedupPrefabs = EnemyManager.Instance.GetSpeedupItems();
        bombPrefabs = EnemyManager.Instance.GetBombItems();
        healPrefabs = EnemyManager.Instance.GetHealItems();
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
        int rand = Random.Range(0,100);

        Vector3 pos = this.transform.position;

        if(rand == 0)    //  確率１％でちっちゃいおっさんが出る
        {
            EnemyManager.Instance.SetEnemy(
                EnemyManager.Instance.GetEnemyPrefab((int)EnemyPattern.EX),
                pos
            );
        }
        else // その他は確率なしのランダム
        {
            int num = Random.Range(
                (int)ePowerupItems.PowerUp,
                (int)ePowerupItems.Ossan
            );

            if(num == (int)ePowerupItems.PowerUp)
            {
                //  敵がやられた場所に生成する
                Instantiate(powerupPrefabs[0], pos, Quaternion.identity);
            }
            else if(num == (int)ePowerupItems.SpeedUp)
            {
                //  敵がやられた場所に生成する
                Instantiate(speedupPrefabs[0], pos, Quaternion.identity);
            }
            else if(num == (int)ePowerupItems.Bomb)
            {
                //  敵がやられた場所に生成する
                Instantiate(bombPrefabs[0], pos, Quaternion.identity);
            }
            else if(num == (int)ePowerupItems.Heal)
            {
                //  敵がやられた場所に生成する
                Instantiate(healPrefabs[0], pos, Quaternion.identity);
            }

        }
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

        //  ちっちゃいおっさんのセット
        if(item == ePowerupItems.Ossan)
        {            
            EnemyManager.Instance.SetEnemy(
                EnemyManager.Instance.GetEnemyPrefab((int)EnemyPattern.EX),
                pos
            );
        }
        else if(item == ePowerupItems.PowerUp)
        {
            Instantiate(powerupPrefabs[0], pos, Quaternion.identity);
        }
        else if(item == ePowerupItems.SpeedUp)
        {
            Instantiate(speedupPrefabs[0], pos, Quaternion.identity);
        }
        else if(item == ePowerupItems.Bomb)
        {
            Instantiate(bombPrefabs[0], pos, Quaternion.identity);
        }
        else if(item == ePowerupItems.Heal)
        {
            Instantiate(healPrefabs[0], pos, Quaternion.identity);
        }
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
        int largeKon = MoneyManager.Instance.GetKonNumGainedFromLarge(); // 500 
        int smallKon = MoneyManager.Instance.GetKonNumGainedFromSmall(); // 100

        if (money < largeKon)
        {
            //  smallKonの数だけ小魂を生成
            int small = money / smallKon;
            Debug.Log("小魂の数 :" + small);
            DropKonPrefab(SmallKonPrefab, small);
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
            DropKonPrefab(LargeKonPrefab, largeKonNum);

            //  remainderの数だけ小魂を生成
            DropKonPrefab(SmallKonPrefab, small);
        }
    }
}
