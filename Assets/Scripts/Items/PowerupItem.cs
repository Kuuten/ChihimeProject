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
    //[SerializeField] private 

    void Start()
    {
        
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

        //  パワーアップ
        Debug.Log("弾がパワーアップしました！");
        
        //  アイテムを消去
        Destroy(this.gameObject);


    }
}
