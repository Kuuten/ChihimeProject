using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//--------------------------------------------------------------
//
//  �{�^�����X�P�[�����O������
//
//--------------------------------------------------------------
public class ButtonCommon : MonoBehaviour
{
    protected ButtonScaling _buttonScaling;
    protected EventTrigger _Trigger; 

    void Start()
    {
        ////  �R���|�[�l���g��ݒ�
        //_buttonScaling = this.GetComponent<ButtonScaling>();
        //_Trigger = this.GetComponent<EventTrigger>();
        //// �}�E�X�I�[�o�[�C�x���g
        //var entry = new EventTrigger.Entry();
        //entry.eventID = EventTriggerType.PointerEnter;
        //// �}�E�X�I�[�o�[���O���C�x���g
        //var away = new EventTrigger.Entry();
        //away.eventID = EventTriggerType.PointerExit;

        //// �C�x���g�o�^
        //entry.callback.AddListener((data) => 
        //{
        //    //  �}�E�X�I�[�o�[
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
