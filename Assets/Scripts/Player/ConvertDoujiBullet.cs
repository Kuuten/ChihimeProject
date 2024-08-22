using DG.Tweening;
using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  �v���C���[�̃h�E�W���o�[�g�e�N���X
//
//--------------------------------------------------------------
public class ConvertDoujiBullet : MonoBehaviour
{
    private Animator animator;
    private Vector3 velocity; 
    private int gamestatus;
    private bool fripY;

    void Start()
    {
        animator = this.GetComponent<Animator>();
        velocity = Vector3.zero;

        //  GameManager�����Ԃ��擾
        gamestatus = GameManager.Instance.GetGameState();

        //  �e�̌���
        switch(gamestatus)
        {
            case (int)eGameState.Zako:
                velocity = new Vector3(0,-5f,0);   //  �������֌���
                break;
            case (int)eGameState.Boss:
                velocity = new Vector3(0,5f,0);   //  ������֌���
                break;
            case (int)eGameState.Event:
                break;
        }

        //  ���݂̈ʒu����velocity���ړ�
        this.transform.DOMove(velocity,0.66f).SetRelative(true).SetEase(Ease.InOutQuint);

        //  �X�P�[�������񂾂񏬂�������
        this.transform.DOScale(new Vector3(0.1f,0.1f,0.1f),0.66f).SetEase(Ease.InExpo);
    }

    void Update()
    {
        //  �����P�Ȃ�A�j���[�V�������I�����Ă���(0�̓��C���[�ԍ�:BaseLayer)
        if(animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.66f)
        {
            Destroy(this.gameObject);
        }
    }

    //-------------------------------------------
    //  �v���p�e�B
    //-------------------------------------------
    public void SetVelocity(Vector3 v){ velocity = v; }
    public Vector3 GetVelocity(){ return velocity; }
    public void SetFripY(bool frip){ fripY = frip; }
    public bool GetFripY(){ return fripY; }


}
