using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using System;
using DG.Tweening;
using Unity.VisualScripting;

//--------------------------------------------------------------
//
//  �v���C���[�̗̑͊Ǘ��N���X
//
//--------------------------------------------------------------

//  ��{�̗͂̓n�[�g�R���i���n�[�g�U���j
//  �ő�̗͂̓n�[�g�U���i���n�[�g�P�Q���j
//  �ŏ��P�ʂ̔��n�[�g���Z�Ńv���O��������
public class PlayerHealth : MonoBehaviour
{
    private int currentMaxHealth;
    private int currentHealth;
    private const int limitHealth = 12;

    [SerializeField]private GameObject enemyGenerator;



    void Start()
    {
        currentMaxHealth = 6;
        currentHealth = 6;
    }

    void Update()
    {
        //if (test.WasPressedThisFrame())
        //{
        //    testSwitch = !testSwitch;

        //    //  �G�̃W�F�l���[�^�[�𖳌���
        //    SetGeneratorActive(testSwitch);

        //    if(testSwitch == false)
        //    {
        //        //  Pauser���t�����I�u�W�F�N�g���|�[�Y
        //        Pauser.Pause();
        //        //  �|�[�Y��ɓ��͂������Ȃ��Ȃ�̂Ń��Z�b�g
        //        this.GetComponent<PlayerInput>().enabled = true;
        //    }
        //    else
        //    {
        //        //  Pauser���t�����I�u�W�F�N�g���|�[�Y
        //        Pauser.Resume(); 
        //    }
        //}
    }

    //-------------------------------------------
    //  �_���[�W����
    //-------------------------------------------
    public void Damage(int value)
    {
        if(currentHealth > 0)
        {
            currentHealth -= value;
        }
        else
        {
            currentHealth = 0;

            //  �v���C���[���~�߂�
            this.GetComponent<PlayerMovement>().enabled = false;
            this.GetComponent<PlayerShotManager>().enabled = false;
            this.GetComponent<BoxCollider2D>().enabled = false;

            //  ���ꉉ�o
            StartCoroutine(Death());
        }
        
    }

    //-------------------------------------------
    //  �񕜏���
    //-------------------------------------------
    public void Heal(int value)
    {
        int target = currentHealth + value;

        // ���l�̕ύX
        DOTween.To(
            () => currentHealth,          // ����Ώۂɂ���̂�
            num => currentHealth = num,   // �l�̍X�V
            target,                       // �ŏI�I�Ȓl
            value/2                       // �A�j���[�V��������
        );
    }

    //-------------------------------------------
    //  �̗͂̃v���p�e�B
    //-------------------------------------------
    public void SetCurrentHealth(int value)
    {
        Assert.IsFalse((value < 0 || value > currentMaxHealth),
            "�̗͂ɔ͈͊O�̐����ݒ肳��Ă��܂��I");

        if(currentMaxHealth != value)currentMaxHealth = value;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public void SetCurrentMaxHealth(int value)
    {
        Assert.IsFalse((value < 0 || value > limitHealth),
            "�ő�̗͂ɔ͈͊O�̐����ݒ肳��Ă��܂��I");

        if(currentMaxHealth != value)currentMaxHealth = value;
    }

    public int GetCurrentMaxHealth()
    {
        return currentMaxHealth;
    }

    //-------------------------------------------
    //  ���ꉉ�o
    //-------------------------------------------
    private IEnumerator Death()
    {
        //  Pauser���t�����I�u�W�F�N�g���|�[�Y
        Pauser.Pause();


        yield return null;
    }


    private IEnumerator Death2()
    {
        //-------------------------------------------
        //  ��֔�яオ������A���֗����Ă���
        //-------------------------------------------
        DG.Tweening.Sequence sequence = DOTween.Sequence();
        bool complete = false;

        //  �w��ʒu�ֈړ�
        sequence.Append
            (
                    transform.DOLocalMoveY(2,0.5f)
                    .SetEase(Ease.OutCubic)
                    .SetRelative(true)  //  ���Βl�ړ�
                    .OnStart(() => {
                        //Sound.Play( (int)AudioChannel.SFX, (int)SFXList.SFX_DASH);
                    })
                    .OnComplete(() =>{

                    })

            )
            .Append
            (
                transform.DOMoveY(-13,1.5f)
                .SetEase(Ease.InSine)  
                .OnStart(() => {
                    
                })
                .OnComplete(() =>{

                })
            );

        //  �����܂ő҂�
        yield return new WaitUntil(() => complete == true);
    }
}
