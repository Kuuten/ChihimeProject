using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.UI;


//--------------------------------------------------------------
//
//  �v���C���[�̍��o�[�X�g�̃J�b�g�C�����o�N���X
//
//--------------------------------------------------------------
public class KonburstCutin : MonoBehaviour
{
    void Start()
    {
        //  �������W�E�X�P�[���ݒ�
        this.GetComponent<RectTransform>().anchoredPosition =
            new Vector3 (1140, 0, 0);
        this.transform.localScale =
            new Vector3(1,0.2f,1);

        //  �J�b�g�C���J�n
        StartCoroutine(CutinAnimation());
    }

    void Update()
    {
        
    }

    //--------------------------------------------------
    //  �A�j���[�V����������
    //--------------------------------------------------
    private IEnumerator CutinAnimation()
    {
        //  ���v2.3�b

        //  �C�[�Y�C��
        this.GetComponent<RectTransform>().DOAnchorPos(new Vector3(-59.25f,0),0.4f)
            .SetEase(Ease.InExpo);

        yield return new WaitForSeconds(0.4f);

        //  �X�P�[�����O
        this.GetComponent<RectTransform>().DOScale(1f, 0.4f)
            .SetEase(Ease.InExpo);

        yield return new WaitForSeconds(0.4f);

        //  0.5�b�ҋ@
        yield return new WaitForSeconds(0.5f);

        //  �A���t�@�A�j���[�V����
        var  fadeImage = GetComponent<Image>();
        fadeImage.enabled = true;
        var c = fadeImage.color;
        c.a = 1.0f; // �����l
        fadeImage.color = c;

        DOTween.ToAlpha(
	        ()=> fadeImage.color,
	        color => fadeImage.color = color,
	        0f, // �ڕW�l
	        1f // ���v����
        );

        //  0.5�b�ҋ@
        yield return new WaitForSeconds(1.0f);
    }
}
