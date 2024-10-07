using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//--------------------------------------------------------------
//
//  �{�X�̏�C�̃A�j���[�V�����N���X
//
//--------------------------------------------------------------
public class FogAnimation : MonoBehaviour
{
    //  ��C�̎��
    public enum Fog
    {
        Left,
        Right,

        Max
    }
    public Fog fog;

    private const float targetX_L = -515.3f; 
    private const float startX_L = -633; 
    private const float targetX_R = 396f; 
    private const float startX_R = 517; 

    void Start()
    {

    }

    private void OnEnable()
    {
        float duration = 7.0f;

        //  �t�H�O�������痈��^�C�v��������
        if(fog == Fog.Left)
        {
            this.GetComponent<RectTransform>().anchoredPosition = new Vector2 (startX_L, 0f);

            this.GetComponent<RectTransform>().DOAnchorPosX(targetX_L,duration);
        }
        //  �t�H�O���E���痈��^�C�v��������
        else if(fog == Fog.Right)
        {
            this.GetComponent<RectTransform>().anchoredPosition = new Vector2 (startX_R, 0f);

            this.GetComponent<RectTransform>().DOAnchorPosX(targetX_R,duration);
        }
    }

    void Update()
    {
        
    }
}
