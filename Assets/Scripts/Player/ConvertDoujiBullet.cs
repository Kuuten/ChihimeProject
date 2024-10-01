using DG.Tweening;
using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerShotManager;

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
    private bool fullPower;

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

        if(fullPower)
        {
            //  ���U��SE�Đ�
            SoundManager.Instance.PlaySFX(
                (int)AudioChannel.SFX_CONVERT_SHOT,
                (int)SFXList.SFX_DOUJI_CONVERT_SHOT_FULL);
        }
        else
        {
            //  ���U��SE�Đ�
            SoundManager.Instance.PlaySFX(
                (int)AudioChannel.SFX_CONVERT_SHOT,
                (int)SFXList.SFX_DOUJI_CONVERT_SHOT_MIDDLE);
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

    //-----------------------------------------------------------
    //  �v���p�e�B
    //-----------------------------------------------------------
    public void SetVelocity(Vector3 v){ velocity = v; }
    public Vector3 GetVelocity(){ return velocity; }
    public void SetFullPower(bool b){ fullPower = b; }
    public bool GetFullPower(bool b){ return fullPower; }


}
