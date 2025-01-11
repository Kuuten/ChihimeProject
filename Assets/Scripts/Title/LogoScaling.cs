using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using static UnityEngine.EventSystems.EventTrigger;
using System;

//--------------------------------------------------------------
//
//  �^�C�g�����S���X�P�[�����O������
//
//--------------------------------------------------------------
public class LogoScaling : MonoBehaviour
{

    void Start()
    {
        transform.localScale = new Vector3(1f,1f,1f);
        transform.DOScale(0.9f,0.35f)
            .SetEase(Ease.OutElastic)
            .SetLoops(-1); 
    }
}
