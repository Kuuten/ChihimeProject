using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

//--------------------------------------------------------------
//
//  �|�b�v�A�b�v�E�B���h�E�N���X
//
//--------------------------------------------------------------
public class PopupWindow : MonoBehaviour
{
    //  �A�j���[�V�����̊����t���O
    private bool isComplete;

    void Start()
    {

    }

    private void OnEnable()
    {
        Init();
    }

    private void Init()
    {
        //  ������
        isComplete = false;

        //  �|�b�v�A�b�v�J�n
        StartCoroutine(Popup());
    }

    void Update()
    {
        
    }

    //--------------------------------------------------------------
    //  �|�b�v�A�b�v�A�j���[�V����
    //--------------------------------------------------------------
    private IEnumerator Popup()
    {
        float openDuration = 0.4f;   //  �A�j���[�V��������(�b)
        float closeDuration = 0.2f;  //  �A�j���[�V��������(�b)

        //  ��U�X�P�[��������������
        transform.localScale = new Vector3(0f,0f,0f);

        //  �A�j���[�V�����J�n
        yield return StartCoroutine(PopupOpen(openDuration));

        //  �ҋ@����
        yield return new WaitForSeconds(1f);

        //  �A�j���[�V�����J�n
        yield return StartCoroutine(PopupClose(closeDuration));

        yield return null;
    }

    //--------------------------------------------------------------
    //  �|�b�v�A�b�v�I�[�v��
    //--------------------------------------------------------------
    private IEnumerator PopupOpen(float duration)
    {
        transform.DOScale(1f,duration)
            .SetEase(Ease.InOutElastic);

        yield return null;
    }

    //--------------------------------------------------------------
    //  �|�b�v�A�b�v�E�N���[�Y
    //--------------------------------------------------------------
    private IEnumerator PopupClose(float duration)
    {
        transform.DOScale(0f,duration)
            .OnComplete(()=>
            {
                isComplete = true;  //  �A�j���[�V��������
                this.gameObject.SetActive(false);
            });

        yield return null;
    }
}
