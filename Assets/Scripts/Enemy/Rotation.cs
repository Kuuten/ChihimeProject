using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  �I�u�W�F�N�g����]������N���X
//
//--------------------------------------------------------------
public class Rotaion : MonoBehaviour
{
    // ����]���E��]���ǂ����̐ݒ�
    public enum LeftOrRight {
        Left,                      //  ����]
        Right,                     //  �E��]
    }
    public LeftOrRight leftOrRight = LeftOrRight.Left;

    //  ��]����
    [SerializeField] private float duraiton;

    // Start is called before the first frame update
    void Start()
    {
        // �X�v���C�g����]������
        if( leftOrRight == LeftOrRight.Left)
        {
            transform.DORotate(new Vector3(0, 0, 360f), duraiton, RotateMode.FastBeyond360)  
                .SetEase(Ease.Linear)  
                .SetLoops(-1, LoopType.Incremental);
        }
        else
        {
            transform.DORotate(new Vector3(0, 0, -360f), duraiton, RotateMode.FastBeyond360)  
                .SetEase(Ease.Linear)  
                .SetLoops(-1, LoopType.Incremental);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
