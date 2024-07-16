using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Numerics;

//--------------------------------------------------------------
//
//  �t�F�[�h�C���E�A�E�g�N���X
//
//--------------------------------------------------------------
public class FadeIO : MonoBehaviour
{

    void Start()
    {

    }

    public IEnumerator StartAnimation()
    {
        bool complete = false;

        //  �A�j���[�V��������
        Image fadeImage = GetComponent<Image>();
        fadeImage.enabled = true;

        Color c = fadeImage.color;
        c.a = 1.0f; // �����l
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
                    .OnStart(() => {

                    })
            );
        sequence.Append
            (
                DOTween.ToAlpha
                (
                    () => fadeImage.color,
                    color => fadeImage.color = color,
                    0f, // �ڕW�l
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

    // �t�F�[�h�C�� 
    public IEnumerator StartFadeIn()
    {
        bool complete = false;

        //  �A�j���[�V��������
        Image fadeImage = GetComponent<Image>();
        fadeImage.enabled = true;
        Color c = fadeImage.color;
        c.a = 1.0f; // �����l
        fadeImage.color = c;

        Sequence sequence = DOTween.Sequence();
        sequence.Append
    (
        DOTween.ToAlpha
        (
            () => fadeImage.color,
            color => fadeImage.color = color,
            0f, // �ڕW�l
            1.0f // ���v����
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

    // �t�F�[�h�A�E�g 
    public IEnumerator StartFadeOut()
    {
        bool complete = false;

        //  �A�j���[�V��������
        Image fadeImage = GetComponent<Image>();
        fadeImage.enabled = true;
        Color c = fadeImage.color;
        c.a = 1.0f; // �����l
        fadeImage.color = c;

        Sequence sequence = DOTween.Sequence();

        sequence.Append
            (
                    DOTween.ToAlpha
                    (
                        () => fadeImage.color,
                        color => fadeImage.color = color,
                        1f, // �ڕW�l
                        1.0f // ���v����
                    )
                    .OnStart(() => {

                    })
            );

        //  �����܂ő҂�
        yield return new WaitUntil(() => complete == true);
    }
}