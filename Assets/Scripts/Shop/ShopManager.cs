using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

//--------------------------------------------------------------
//
//  ショップシーン管理クラス(買い物処理)
//
//--------------------------------------------------------------
public class ShopManager : MonoBehaviour
{
    enum eMenuList
    {
        RedHeart,           //  赤いハート
        DoubleupHeart,      //  ダブルアップハート
        GoldHeart,          //  金色のハート
        Bomb,               //  骨Gボム

    }
    private readonly string failedText = "魂が足らへんで～。";  //  魂が足りない時の文字列

    bool canContorol;   //  操作可能フラグ

    void Start()
    {
        StartCoroutine(StartInit());
    }

    //--------------------------------------------------------------
    //  初期化コルーチン
    //--------------------------------------------------------------
    IEnumerator StartInit()
    {
        //**************************************************************
        //  ここでショップの画面を表示しておく
        //**************************************************************

        //  白フェードイン
        //yield return StartCoroutine(WaitingForWhiteFadeIn());

        //  ショップBGM再生
        SoundManager.Instance.PlayBGM((int)MusicList.BGM_SHOP);

        //yield return new WaitForSeconds(1); //  1秒待つ

        //  操作可能にする
        canContorol = true;

        //  赤ハートを選択状態に設定
        //EventSystem.current.SetSelectedGameObject(menuButtonObj[(int)eMenuList.RedHeart]);


        yield return null;
    }

    void Update()
    {
        //  操作不能ならリターン
        if(!canContorol)return;
    }

    //--------------------------------------------------------------
    //  メッセージウィンドウを出す処理
    //--------------------------------------------------------------
    IEnumerator DisplayMessage(string msg)
    {
        ////  メッセージオブジェクトを表示
        //messageObj.SetActive(true);

        yield return null;
    }

    //--------------------------------------------------------------
    //  赤いハートボタンが押された時の処理
    //--------------------------------------------------------------
    public void OnRedHeartButtonDown()
    {
        
    }

    //--------------------------------------------------------------
    //  ダブルアップハートボタンが押された時の処理
    //--------------------------------------------------------------
    public void OnDoubleupHeartButtonDown()
    {
        
    }

    //--------------------------------------------------------------
    //  金色のハートボタンが押された時の処理
    //--------------------------------------------------------------
    public void OnGoldHeartButtonDown()
    {
        
    }

    //--------------------------------------------------------------
    //  骨Gのボムボタンが押された時の処理
    //--------------------------------------------------------------
    public void OnHoneGBombButtonDown()
    {
        
    }
}
