using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

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
    //  ドロップするアイテムのプレハブ達
    [SerializeField] private GameObject[] dropItems;
    //  小魂で得られる魂の数
    private const int konNumGainedFromSmallKon = 1;
    //  大魂で得られる魂の数
    private const int konNumGainedFromLargelKon = 10;

    void Start()
    {
        Assert.IsFalse((int)Items.Max != dropItems.GetLength(0),
            "配列の要素数と列挙の数が合いません！");
    }

    //------------------------------------------------------------
    //  ドロップ持ちの敵が死んだ時にランダムな
    //  パワーアップアイテムをドロップする
    //------------------------------------------------------------
    public void DropPowerupItem()
    {
        //  ランダムなアイテムを生成する
        int rand = Random.Range(
            (int)Items.PowerUp,
            (int)Items.Max);

        //  敵がやられた場所に生成する
        Vector3 pos = this.transform.position;
        Instantiate(dropItems[rand], pos, Quaternion.identity);
    }

    //------------------------------------------------------------
    //  小魂をドロップする
    //------------------------------------------------------------
    public void DropSmallKon(int num)
    {
        //  敵がやられた場所に生成する
        float bias = 5.0f;
        Vector3 pos = this.transform.position;

        for(int i=0;i<num;i++)
        {
            GameObject obj = Instantiate(
                dropItems[(int)Items.smallKon],
                pos,
                Quaternion.identity);

            //  実際のシーン上での横幅を取得
            SpriteRenderer sprite = obj.GetComponent<SpriteRenderer>();
            float spriteWidth = sprite.bounds.size.x;
            
            //  横幅分ずつずらして配置する
            pos.y = this.transform.position.y + 1.0f;
            pos.x = ((transform.position.x - spriteWidth * i / 2) + i * spriteWidth );

            //  再配置
            obj.transform.position = pos;
        }
    }

    //------------------------------------------------------------
    //  大魂をドロップする
    //------------------------------------------------------------
    public void DropLargeKon()
    {
        //  敵がやられた場所に生成する
        float bias = 5.0f;
        Vector3 pos = this.transform.position;
        pos.y = this.transform.position.y + 1.0f;
        pos.x = Random.Range( transform.position.x-bias, transform.position.x + bias );

        Instantiate(dropItems[(int)Items.largeKon], pos, Quaternion.identity);
    }

    //------------------------------------------------------------
    //  落とす魂の金額から計算して魂を生成する
    //------------------------------------------------------------
    public void DropKon(int money)
    {
        int largeKon = konNumGainedFromLargelKon;
        int smallKon = konNumGainedFromSmallKon;

        if (money < largeKon)
        {
            //  smallKonの数だけ小魂を生成
            smallKon = money;
            DropSmallKon(smallKon);
        }
        else // moneyがlargeKon以上の場合
        {
            //  大魂から計算する
            int largeKonNum = money / largeKon;

            //  余りで小魂を計算する
            int remainder = money % largeKon;

            Debug.Log("小魂の数 :" + remainder);

            //  largeKonNumの数だけ大魂を生成
            for(int i=0;i<largeKonNum;i++)
            {
                DropLargeKon();
            }

            //  remainderの数だけ小魂を生成
            for(int i=0;i<remainder;i++)
            {
                DropSmallKon(i);
            }
        }
    }
}
