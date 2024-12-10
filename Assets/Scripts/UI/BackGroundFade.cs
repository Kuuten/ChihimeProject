using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//--------------------------------------------------------------
//
//  �w�i�摜�̃t�F�[�h�N���X
//
//--------------------------------------------------------------
public class BackGroundFade : MonoBehaviour
{
    void Start()
    {
        
    }

    private void OnEnable()
    {
        StartCoroutine(StartFadeOut());
    }

    // �t�F�[�h�A�E�g 
    public IEnumerator StartFadeOut()
    {
        bool complete = false;
        float duration = 3.0f;  //  �A�j���[�V��������

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
                        duration // ���v����
                    )
                    .OnStart(() => {

                    })
                    .OnComplete(
                    () =>
                    {
                        complete = true;
                    })
            );

        //  �����܂ő҂�
        yield return new WaitUntil(() => complete == true);
    }
}
