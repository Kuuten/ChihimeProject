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
    //  Animator.speed�݂����Ȃ���擾����
    //  �����Dotween�ł��񂾂����������
    //  DOTween.To���g���΂���͂�

    //  �X�P�[����DoTween�ł��񂾂�Ə���������

    private Animator animator;

    void Start()
    {
        animator = this.GetComponent<Animator>();

        //  ShotManager���Őݒ肷��悤�ɂ���
        this.GetComponent<SpriteRenderer>().flipY = true;

        //  �����P�Ȃ�A�j���[�V�������I�����Ă���(0�̓��C���[�ԍ�:BaseLayer)
        //animator.GetCurrentAnimatorStateInfo(0).normalizedTime
    }

    void Update()
    {
        
    }
}
