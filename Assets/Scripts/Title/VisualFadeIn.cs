using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//--------------------------------------------------------------
//
//  �^�C�g�����B�W���A���̃t�F�[�h�C���N���X
//
//--------------------------------------------------------------
public class VisualFadeIn : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    //--------------------------------------------------------------
    //  �^�C�g���r�W���A���̃t�F�[�h�C��
    //--------------------------------------------------------------
    public IEnumerator FadeInTitleVisual()
    {
        bool complete = false;

        //  �A�j���[�V��������
        Image fadeImage = GetComponent<Image>();
        fadeImage.enabled = true;
        Color c = fadeImage.color;
        c.a = 0.0f; // �����l
        fadeImage.color = c;

        Sequence sequence = DOTween.Sequence();
        sequence.Append
    (
        DOTween.ToAlpha
        (
            () => fadeImage.color,
            color => fadeImage.color = color,
            1f, // �ڕW�l
            0.5f // ���v����
        )
    )
    .OnStart(
    () =>
    {

    })
    .OnComplete(
    () =>
    {
        complete = true;
    });

      //  �����܂ő҂�
      yield return new WaitUntil(() => complete == true);
    }
}
