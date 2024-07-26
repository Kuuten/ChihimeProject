using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using System;
using DG.Tweening;
using Unity.VisualScripting;
using System.ComponentModel;
using Cysharp.Threading.Tasks;
using static Unity.Collections.AllocatorManager;
using System.Threading;

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
    private bool bSuperMode;
    private bool bDeath;

    //  �_�ł����邽�߂�SpriteRenderer
    SpriteRenderer sp;

    //  �_�ł̊Ԋu
    private float flashInterval;

    //  �_�ł�����Ƃ��̃��[�v�J�E���g
    private int loopCount;

    //  �v���C���[�̎��S�G�t�F�N�g
    [SerializeField] private GameObject playerDeathEffect;
    //  �f�B���N�V���i�����C�g
    [SerializeField] private GameObject directionalLight;


    void Start()
    {
        //  �ŏ��̓n�[�g�R��
        currentMaxHealth = 6;
        currentHealth = 6;

        //  SpriteRender���擾
        sp = GetComponent<SpriteRenderer>();

        //  ���[�v�J�E���g��ݒ�
        loopCount = 30;

        //  �_�ł̊Ԋu��ݒ�
        flashInterval = 0.02f;

        //  ���S�t���OOFF
        bDeath = false;

        //  �ŏ��͖��G���[�hOFF
        bSuperMode = false;
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

    //----------------------------------------------------------------
    //  �v���C���[�̓����蔻��
    //----------------------------------------------------------------
    private async void OnTriggerEnter2D(Collider2D collision)
    {
        //  ���G�����S���Ă���Ȃ��΂�
        if(bSuperMode || bDeath)return;

        if(collision.CompareTag("Enemy") ||         //  �G����
            collision.CompareTag("EnemyBullet"))    //  �G�e
        {
            //  �v���C���[�̃_���[�W����
            EnemyData ed = collision.GetComponent<Enemy>().GetEnemyData();
            Damage( ed.Attack );

            //  ���S�t���OON
            if(currentHealth <= 0)
            {
                bDeath = true;
                StartCoroutine(Death());       //  ���ꉉ�o
                return;
            }

            //  �S�����P�i�K�_�E��
            PlayerShotManager ps = this.GetComponent<PlayerShotManager>();
            ps.LeveldownNormalShot();
            PlayerMovement pm = this.GetComponent<PlayerMovement>();
            pm.LeveldownMoveSpeed();

            //  �f�o�b�O�\��
            Debug.Log("�S�����P�i�K�_�E���I");
            Debug.Log("�V���b�g���� :" + ps.GetNormalShotLevel() 
                +"" + "�X�s�[�h����" + pm.GetSpeedLevel());
            Debug.Log("Player�̗̑� :" + currentHealth);

            //  �_�ŉ��o
            var task = Blink();
            await task;

        }
    }

    //-------------------------------------------
    //  �_���[�W���̓_�ŉ��o
    //-------------------------------------------
    private async UniTask Blink()
    {
        //  ���G���[�hON
        bSuperMode = true;

        //GameObject���j�����ꂽ���ɃL�����Z�����΂��g�[�N�����쐬
        var token = this.GetCancellationTokenOnDestroy();

        //�_�Ń��[�v�J�n
        for (int i = 0; i < loopCount; i++)
        {
            //flashInterval�҂��Ă���
            await UniTask.Delay (TimeSpan.FromSeconds(flashInterval))
                .AttachExternalCancellation(token);

            //spriteRenderer���I�t
            sp.enabled = false;
            
            //flashInterval�҂��Ă���
            await UniTask.Delay (TimeSpan.FromSeconds(flashInterval))
                .AttachExternalCancellation(token);

            //spriteRenderer���I��
            sp.enabled = true;
        }

        //  ���G���[�hOFF
        bSuperMode = false;
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
    //  �v���p�e�B
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
        //  ���C�gOFF
        directionalLight.gameObject.SetActive(false);

        //  �q�I�u�W�F�N�g�̋z���t�B�[���h���A�N�e�B�u��
        this.gameObject.transform.Find("Field").gameObject.SetActive(false);

        //  �v���C���[���~�߂�
        this.GetComponent<CircleCollider2D>().enabled = false;
        this.GetComponent<SpriteRenderer>().enabled = false;
        this.GetComponent<PlayerMovement>().enabled = false;
        this.GetComponent<PlayerShotManager>().enabled = false;

        //  �v���C���[�̃A�j���I����҂�(�ň��b���ő҂�await�Ƃ�)

        //  �v���C���[�̂���G�t�F�N�g
        GameObject obj = Instantiate(
            playerDeathEffect, 
            this.transform.position,
            Quaternion.identity);

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
