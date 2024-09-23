using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  �I�u�W�F�N�g���g��E�k�����J��Ԃ�����N���X
//
//--------------------------------------------------------------
public class RepeatScaling : MonoBehaviour
{
    //  �ő�X�P�[��
    [SerializeField] private float maxScale = 1.2f;
    //  �ŏ��X�P�[��
    [SerializeField] private float minScale = 0.8f;
    //  ��̃A�j���[�V�����ɂ����鎞��
    [SerializeField] private float duration = 0.5f;

    void Start()
    {
        GetComponent<RectTransform>().localScale = 
            new Vector3(minScale,minScale,minScale);

        //  �g�傷��
        GetComponent<RectTransform>().DOScale(maxScale,duration)
                .SetEase(Ease.Linear)  
                .SetLoops(-1, LoopType.Yoyo); 
    }

    void Update()
    {
        
    }
}
