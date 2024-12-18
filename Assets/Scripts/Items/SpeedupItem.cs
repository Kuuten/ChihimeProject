using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//--------------------------------------------------------------
//
//  スピードアップアイテムクラス
//
//--------------------------------------------------------------
public class SpeedupItem : MonoBehaviour
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
        //  タグがプレイヤー以外ならreturn
        if(!collision.CompareTag("Player"))return;

        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if(pm == null)return;

        //  スピードレベルが最大じゃなければレベルアップ
        if(pm.GetSpeedLevel() < (int)eSpeedLevel.Lv3 )
        {
            // アイテム獲得SE
            SoundManager.Instance.PlaySFX(
                (int)AudioChannel.SFX,
                (int)SFXList.SFX_GET_POWERUP);

            //  スピードアップ
            pm.LevelupMoveSpeed();
            Debug.Log("自機がスピードレベル" + pm.GetSpeedLevel() + "になりました！");
        }
        else
        {
            // アイテム獲得SE
            SoundManager.Instance.PlaySFX(
                (int)AudioChannel.SFX,
                (int)SFXList.SFX_GET_KON);

            //  最大レベルの時取ると魂獲得
            int money = MoneyManager.Instance.GetKonNumGainedFromPowerup();
            MoneyManager.Instance.AddMoney( money );
            Debug.Log("自機スピードが最大レベルなので魂" + money + "を獲得しました！");
        }
        
        //  アイテムを消去
        Destroy(this.gameObject);
    }
}
