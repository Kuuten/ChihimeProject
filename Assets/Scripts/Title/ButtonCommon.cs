using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//--------------------------------------------------------------
//
//  ボタンをスケーリングさせる
//
//--------------------------------------------------------------
public class ButtonCommon : MonoBehaviour
{
    protected ButtonScaling _buttonScaling;
    protected EventTrigger _Trigger; 

    void Start()
    {
        ////  コンポーネントを設定
        //_buttonScaling = this.GetComponent<ButtonScaling>();
        //_Trigger = this.GetComponent<EventTrigger>();
        //// マウスオーバーイベント
        //var entry = new EventTrigger.Entry();
        //entry.eventID = EventTriggerType.PointerEnter;
        //// マウスオーバーが外れるイベント
        //var away = new EventTrigger.Entry();
        //away.eventID = EventTriggerType.PointerExit;

        //// イベント登録
        //entry.callback.AddListener((data) => 
        //{
        //    //  マウスオーバー
        //    _buttonScaling.OnOver(); 
        //});
        //_Trigger.triggers.Add(entry);

        //away.callback.AddListener((data) => { _buttonScaling.OnAway(); });
        //_Trigger.triggers.Add(away);  
    }

    void Update()
    {
        
    }
}
