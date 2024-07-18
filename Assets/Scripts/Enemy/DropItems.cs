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
    public void DropSmallKon()
    {
        //  敵がやられた場所に生成する
        float bias = 5.0f;
        Vector3 pos = this.transform.position;
        pos.y = this.transform.position.y + 1.0f;
        pos.x = Random.Range( transform.position.x-bias, transform.position.x + bias );

        Instantiate(dropItems[(int)Items.smallKon], pos, Quaternion.identity);
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
        
    }
}
