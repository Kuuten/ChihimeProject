using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  小魂をプレイヤーに取得させるクラス
//
//--------------------------------------------------------------
public class SmallKon : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    //-------------------------------------------------------
    //  プレイヤーと当たったらお金を増やす
    //-------------------------------------------------------
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //  プレイヤーなら魂を消去してお金を増やす
        if(collision.CompareTag("Player"))
        {
            // アイテム獲得SE

            //  オブジェクトを消去
            Destroy(this. gameObject);

            //  お金を増やす
            int money = MoneyManager.Instance.GetKonNumGainedFromSmall();
            MoneyManager.Instance.AddMoney(money);
        }
    }
}
