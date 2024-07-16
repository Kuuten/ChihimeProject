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

        //  スピードアップ
        Debug.Log("自機がスピードアップしました！");
        
        //  アイテムを消去
        Destroy(this.gameObject);


    }
}
