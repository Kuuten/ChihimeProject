using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  ボムアイテムクラス
//
//--------------------------------------------------------------
public class BombItem : MonoBehaviour
{
    //  プレイヤーのクラスを取得
    private GameObject player;

    void Start()
    {
        player = GameManager.Instance.GetPlayer();
    }

    void Update()
    {
        
    }

    //-------------------------------------------------------
    //  プレイヤーと当たったらボム数を１UPする
    //-------------------------------------------------------
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //---------------------------
        //  ボム1個分回復する
        //---------------------------

        //  タグがプレイヤー以外ならreturn
        if(!collision.CompareTag("Player"))return;

        PlayerBombManager pb = player.GetComponent<PlayerBombManager>();
        if (pb == null) return;

        //  ボムが最大じゃなければ増加
        if (pb.GetBombNum() < pb.GetBombMaxNum())
        {
            //  ボムを増加
            pb.AddBomb();

            Debug.Log($"プレイヤーのボム数が１回復して\n" +
                $"{pb.GetBombNum()}になりました！");
        }
        else
        {
            //  最大体力の時取ると魂獲得
            int money = MoneyManager.Instance.GetKonNumGainedFromPowerup();
            MoneyManager.Instance.AddMoney(money);
            Debug.Log($"ボムが最大なので魂{money}を獲得しました！");
        }

        //  アイテムを消去
        Destroy(this.gameObject);
    }
}
