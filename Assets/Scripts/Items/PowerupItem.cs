using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//--------------------------------------------------------------
//
//  パワーアップアイテムクラス
//
//--------------------------------------------------------------
public class PowerupItem : MonoBehaviour
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
    //  プレイヤーと当たったらパワーアップする
    //-------------------------------------------------------
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //  タグがプレイヤーならパワーアップする
        if(collision.CompareTag("Player"))
        {
            PlayerShotManager ps = player.GetComponent<PlayerShotManager>();
            if(ps == null)return;

            //  ショットレベルが最大じゃなければレベルアップ
            if(ps.GetNormalShotLevel() < (int)eNormalShotLevel.Lv3 )
            {
                //  パワーアップ
                ps.LevelupNormalShot();
                Debug.Log("通常弾のパワーレベルが" + ps.GetNormalShotLevel() + "になりました！");
            }
            else
            {
                //  最大レベルの時取ると魂獲得
                int money = MoneyManager.Instance.GetKonNumGainedFromPowerup();
                MoneyManager.Instance.AddMoney( money );
                Debug.Log("ショットが最大レベルなので魂" + money + "を獲得しました！");
            }

            //  アイテムを消去
            Destroy(this.gameObject);
        }
    }
}
