using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  回復アイテムクラス
//
//--------------------------------------------------------------
public class HealItem : MonoBehaviour
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
    //  プレイヤーと当たったら回復する
    //-------------------------------------------------------
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //---------------------------
        //  ハート1個分回復する
        //---------------------------

        //  タグがプレイヤー以外ならreturn
        if(!collision.CompareTag("Player"))return;

        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if(ph == null)return;

        //  体力が最大じゃなければ回復
        if(ph.GetCurrentHealth() < ph.GetCurrentMaxHealth() )
        {
            //  回復
            int heal_health = 2;
            ph.Heal(heal_health);
            int health = ph.GetCurrentHealth();
        }
        else
        {
            //  最大体力の時取ると魂獲得
            int money = MoneyManager.Instance.GetKonNumGainedFromPowerup();
            MoneyManager.Instance.AddMoney( money );
            Debug.Log($"体力が最大なので魂{money}を獲得しました！");
        }

        //  アイテムを消去
        Destroy(this.gameObject);
    }
}
