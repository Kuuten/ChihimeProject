using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//--------------------------------------------------------------
//
//  �v���C���[�̃{���̃t�F�[�h�����N���X
//
//--------------------------------------------------------------
public class BombFade : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {
        
    }

    // �t�F�[�h�C�� 
    public IEnumerator StartFadeIn(GameObject obj, float time)
    {
        //  �Q�[���I�u�W�F�N�g���Ȃ��ꍇ�͔�����
        if(obj == null)yield break;

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
            time // ���v����
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
    public IEnumerator StartFadeOut(GameObject obj, float time)
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
            time // ���v����
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

    // �����폜 
    public IEnumerator DestroyObject()
    {
        //  �Q�[���I�u�W�F�N�g���Ȃ��ꍇ�͔�����
        if(this.gameObject == null)yield break;

        DestroyImmediate (this.gameObject, true);

        yield return null;
    }
}
