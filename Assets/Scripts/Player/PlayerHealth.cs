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
using UnityEditor.Animations;
using DG.Tweening.Core.Easing;

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
    //  �n�[�g�̃^�C�v
    enum HeartType
    {
        Half,   //  ����
        Full,   //  �n�[�g�P��

        Max
    }

    [SerializeField] private int currentMaxHealth;  //  �K������
    [SerializeField] private int currentHealth;
    private const int limitHealth = 12;
    private bool bSuperMode;
    private bool bDeath;
    private bool bDamage;

    //  �_���[�W���o�p��Animator
    [SerializeField] private AnimatorController animPlayerFront;
    [SerializeField] private AnimatorController animPlayerFrontDamage;
    [SerializeField] private AnimatorController animPlayerBack;
    [SerializeField] private AnimatorController animPlayerBackDamage;

    //  ���S���o�p��Animator
    [SerializeField] private AnimatorController animPlayerDeath1;
    [SerializeField] private AnimatorController animPlayerDeath2;

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
    //  �n�[�g�摜�̐e�I�u�W�F�N�g�̈ʒu�擾�p
    [SerializeField] private GameObject heartRootObj;
    //  �n�[�g�t���[���̃v���n�u
    [SerializeField] private GameObject heartFrameObj;
    //  �n�[�g�t���[���I�u�W�F�N�g�̃��X�g
    private List<GameObject> heartList = new List<GameObject>();

    void Awake()
    {
 
    }

    void Start()
    {
        //  PlayerInfoManager���珉����
        currentMaxHealth = PlayerInfoManager.g_MAXHP;
        if(currentMaxHealth > limitHealth)
            Debug.LogError("�ő�̗͒l�������𒴉߂��Ă��܂��I");
        if(currentMaxHealth % 2 !=0)
            Debug.LogError("currentMaxHealth�͕K�������łȂ���΂����܂���I");
        currentHealth = PlayerInfoManager.g_CURRENTHP;
        if(currentHealth > currentMaxHealth)
            Debug.LogError("���ݑ̗͒l���ő�̗͒l�𒴉߂��Ă��܂��I");

        //  SpriteRender���擾
        sp = GetComponent<SpriteRenderer>();

        //  ���[�v�J�E���g��ݒ�
        loopCount = 30;

        //  �_�ł̊Ԋu��ݒ�
        flashInterval = 0.02f;

        //  �_���[�W�t���OOFF
        bDamage = false;

        //  ���S�t���OOFF
        bDeath = false;

        //  �ŏ��͖��G���[�hOFF
        bSuperMode = false;

        //  �e�I�u�W�F�N�g�̎q�I�u�W�F�N�g�Ƃ��ăn�[�g�t���[���𐶐�
        for( int i=0; i<currentMaxHealth/2;i++ )
        {
            GameObject obj = Instantiate(heartFrameObj);
            obj.transform.SetParent( heartRootObj.transform );
            obj.transform.GetChild((int)HeartType.Half).gameObject.SetActive(true);
            obj.transform.GetChild((int)HeartType.Full).gameObject.SetActive(true);
            obj.transform.localScale = Vector3.one;
            obj.GetComponent<RectTransform>().transform.localPosition = Vector3.zero;


            heartList.Add( obj );   //  ���X�g�ɒǉ�
        }
    }

    void Update()
    {
        //  �v���C���[������ł����珈�����Ȃ�
        if(bDeath)return;

        //  �Q�[���i�K�ʂ�Animator�̐؂�ւ�
       int gamestatus = GameManager.Instance.GetGameState();
        switch(gamestatus)
        {
            case (int)eGameState.Zako:
                ChangePlayerSpriteToFront(true);     //  ��O����
                break;
            case (int)eGameState.Boss:
                ChangePlayerSpriteToFront(false);    //  ������
                break;
            case (int)eGameState.Event:
                break;
        }

        //  �n�[�g�摜���X�V
        CalculateHealthUI(currentHealth);
    }

    //----------------------------------------------------------------
    //  �v���C���[�̓����蔻��
    //----------------------------------------------------------------
    private async void OnTriggerEnter2D(Collider2D collision)
    {
        //  ���G�����S���Ă���Ȃ��΂�
        if(bSuperMode || bDeath)return;

        if(collision.CompareTag("Enemy") )    //  �G�{�̂�HIT�I
        {
            //  �v���C���[�̃_���[�W����
            EnemyData ed = collision.GetComponent<Enemy>().GetEnemyData();
            Damage( ed.Attack );

            //  ���S�t���OON
            if(currentHealth <= 0)
            {
                bDamage = false;
                bDeath = true;
                //  HitCircle���\���ɂ���
                this.transform.GetChild(6).gameObject.SetActive(false);
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

            //  �_���[�W���̐Ԃ��Ȃ�_�ŉ��o�J�n
            var task1 = DamageAnimation();
            await task1;

            //  ���G���o�J�n
            var task2 = Blink();
            await task2;

        }
        else if(collision.CompareTag("EnemyBullet"))    //  �G�e��HIT�I
        {
            //  �v���C���[�̃_���[�W����

            /* �����Œe��Enemy���t���ĂȂ��̂ŃG���[���o��n�Y */
            /* �G�̒e�p�̃N���X���쐬���ĊO���Œe�������ɈЗ͂�ݒ肵�� */
            /* ������Q�b�^�[�ł����Ŏ擾�ł���悤�ɂ��������@*/
            //EnemyData ed = collision.GetComponent<Enemy>().GetEnemyData();
            //Damage( ed.Attack );
            Damage( 2 );    //  ���_���[�W����


            //  ���S�t���OON
            if(currentHealth <= 0)
            {
                bDamage = false;
                bDeath = true;
                //  HitCircle���\���ɂ���
                this.transform.GetChild(6).gameObject.SetActive(false);
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

            //  �_���[�W���̐Ԃ��Ȃ�_�ŉ��o�J�n
            var task1 = DamageAnimation();
            await task1;

            //  ���G���o�J�n
            var task2 = Blink();
            await task2;
        }
    }

    //-------------------------------------------
    //  �_���[�W���̐Ԃ��Ȃ�_�ŉ��o
    //-------------------------------------------
    private async UniTask DamageAnimation()
    {
        //GameObject���j�����ꂽ���ɃL�����Z�����΂��g�[�N�����쐬
        var token = this.GetCancellationTokenOnDestroy();

        //  ��u�F���ς��
        await UniTask.Delay (TimeSpan.FromSeconds(0.3f))
        .AttachExternalCancellation(token);

        //  �_���[�W�t���OOFF
        bDamage = false;

    }

    //-------------------------------------------
    //  �_���[�W���̖��G�_�ŉ��o
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
    //  �v���C���[�̃X�v���C�g�������ւ���
    //-------------------------------------------
   private void ChangePlayerSpriteToFront(bool front)
    {
        if(front)   //  �U�R�풆
        {
            if(bDamage) //  �_���[�W���I
            {
                this.GetComponent<Animator>().runtimeAnimatorController =
                    animPlayerFrontDamage;
            }
            else // �ʏ�
            {
                this.GetComponent<Animator>().runtimeAnimatorController =
                    animPlayerFront;
            }

        }
        else        //  �{�X�풆
        {
            if(bDamage) //  �_���[�W���I
            {
                this.GetComponent<Animator>().runtimeAnimatorController =
                    animPlayerBackDamage;
            }
            else // �ʏ�
            {
                this.GetComponent<Animator>().runtimeAnimatorController =
                    animPlayerBack;
            }
        }
    }

    //-------------------------------------------
    //  �_���[�W����
    //-------------------------------------------
    public void Damage(int value)
    {
        //  �_���[�W�I
        bDamage = true;

        int target = currentHealth - value;
        //  �Œ�̗͂Ŏ~�߂�
        if (target <= 0)
        {
            currentHealth = 0;
        }

        currentHealth = target;

        //// ���l�̕ύX
        //DOTween.To(
        //    () => currentHealth,          // ����Ώۂɂ���̂�
        //    num => currentHealth = num,   // �l�̍X�V
        //    target,                       // �ŏI�I�Ȓl
        //    value/2                       // �A�j���[�V��������
        //);

        //  �f�o�b�O�\��
        Debug.Log($"�v���C���[�̗̑͂�{value}��������\n" +
            $"{currentHealth}�ɂȂ�܂����I");
        
    }

    //-------------------------------------------
    //  �񕜏���
    //-------------------------------------------
    public void Heal(int value)
    {
        int target = currentHealth + value;
        //  �ő�̗͂Ŏ~�߂�
        if (target >= currentMaxHealth)
        {
            target = currentMaxHealth;
        }

        currentHealth = target;

        //// ���l�̕ύX
        //DOTween.To(
        //    () => currentHealth,          // ����Ώۂɂ���̂�
        //    num => currentHealth = num,   // �l�̍X�V
        //    target,                       // �ŏI�I�Ȓl
        //    value/2                       // �A�j���[�V��������
        //);

        //  �f�o�b�O�\��
        Debug.Log($"�v���C���[�̗̑͂�{value}�񕜂���\n" +
            $"{currentHealth}�ɂȂ�܂����I");
    }

    //-------------------------------------------
    //  �ő�̗͑�������
    //-------------------------------------------
    public void IncreaseHP(int value)
    {
        int target = currentMaxHealth + value;
        //  �ő�̗͂Ŏ~�߂�
        if (target >= limitHealth)
        {
            target = limitHealth;
        }

        currentMaxHealth = target;

        //// ���l�̕ύX
        //DOTween.To(
        //    () => currentMaxHealth,         // ����Ώۂɂ���̂�
        //    num => currentMaxHealth = num,  // �l�̍X�V
        //    target,                         // �ŏI�I�Ȓl
        //    value                           // �A�j���[�V��������
        //);

        //  �f�o�b�O�\��
        Debug.Log($"�v���C���[�̍ő�̗͂�{value}��������\n" +
            $"{currentMaxHealth}�ɂȂ�܂����I");
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

    public void SetDamageFlag(bool flag)
    {
        bDamage = flag;
    }

    public bool GetDamageFlag()
    {
        return  bDamage;
    }

    public void SetSuperMode( bool flag ){ bSuperMode = flag; }

    public bool GetSuperMode(){ return bSuperMode; }

    //-------------------------------------------
    //  ���ꉉ�o
    //-------------------------------------------
    private IEnumerator Death()
    {
        //  �f�o�b�O�\��
        Debug.Log("�v���C���[�����S���܂����I");

        //  ���C�gOFF
        directionalLight.gameObject.SetActive(false);

        //  �q�I�u�W�F�N�g�̋z���t�B�[���h���A�N�e�B�u��
        this.gameObject.transform.Find("Field").gameObject.SetActive(false);

        //  �v���C���[���~�߂�
        this.GetComponent<CircleCollider2D>().enabled = false;
        this.GetComponent<PlayerMovement>().enabled = false;
        this.GetComponent<PlayerShotManager>().enabled = false;

        //  Animator���udeath1�v�ɍ����ւ�
        this.GetComponent<Animator>().runtimeAnimatorController =
            animPlayerDeath1;

        //  0.8�b�҂�
        yield return new WaitForSeconds(0.8f);

        //  Animator���udeath2�v�ɍ����ւ�
        this.GetComponent<Animator>().runtimeAnimatorController =
            animPlayerDeath2;

        //  1�b�҂�
        yield return new WaitForSeconds(1.0f);

        //  �v���C���[���\���ɂ���
        this.GetComponent<SpriteRenderer>().enabled = false;

        //  �v���C���[�̂���G�t�F�N�g
        GameObject obj = Instantiate(
            playerDeathEffect, 
            this.transform.position,
            Quaternion.identity);

        //  ����G�t�F�N�g�̏I����҂�
        yield return new WaitForSeconds(0.583f);

        //  GameOver�\����҂�

        //  GameOver�փV�[���J��
        LoadingScene.Instance.LoadNextScene("GameOver");

        //  Pauser���t�����I�u�W�F�N�g���|�[�Y
        //Pauser.Pause();

        yield return null;
    }

    //---------------------------------------------------
    //  ���ݑ̗͂��󂯎���đ̗�UI���v�Z����
    //---------------------------------------------------
    private void CalculateHealthUI(int health)
    {
        if(health < 0)Debug.LogError("health�Ƀ}�C�i�X�̒l�������Ă��܂��I");

        //  �̗�0�Ȃ�n�[�g��S����\���ɂ���
        if(health == 0)
        {
            for(int i=0;i<heartList.Count;i++)
            {
                heartList[i].transform.GetChild((int)HeartType.Half)
                    .gameObject.SetActive(false);
                heartList[i].transform.GetChild((int)HeartType.Full)
                    .gameObject.SetActive(false);
            }
        }
        else if(health == 1)
        {
            for(int i=0;i<heartList.Count;i++)
            {
                if(i==0)
                {
                    heartList[i].transform.GetChild((int)HeartType.Half)
                        .gameObject.SetActive(true);
                    heartList[i].transform.GetChild((int)HeartType.Full)
                        .gameObject.SetActive(false);
                }

                //  �c����\���ɂ���
                for(int j=1;j<heartList.Count;j++)
                {
                    heartList[j].transform.GetChild((int)HeartType.Half)
                        .gameObject.SetActive(false);
                    heartList[j].transform.GetChild((int)HeartType.Full)
                        .gameObject.SetActive(false);
                }

            } 
        }
        else // �̗͂��Q�ȏ�̎�
        {
            //  ��U���ݑ̗͂̂Ƃ��܂őS���t���Ŗ��߂�
            int fullNum = health / 2;
            for(int i=0;i<fullNum;i++)
            {
                heartList[i].transform.GetChild((int)HeartType.Half)
                    .gameObject.SetActive(true);
                heartList[i].transform.GetChild((int)HeartType.Full)
                    .gameObject.SetActive(true);
            }

            //  ��������ꍇ�͍Ō�̔ԍ������n�[�t�ɂ���
            int taegetNum = health - fullNum;
            if(health % 2 != 0)
            {
                heartList[taegetNum-1].transform.GetChild((int)HeartType.Half)
                    .gameObject.SetActive(true);
                heartList[taegetNum-1].transform.GetChild((int)HeartType.Full)
                    .gameObject.SetActive(false);
            }

            //  �c����\���ɂ���
            for(int i=taegetNum;i<heartList.Count;i++)
            {
                heartList[i].transform.GetChild((int)HeartType.Half)
                    .gameObject.SetActive(false);
                heartList[i].transform.GetChild((int)HeartType.Full)
                    .gameObject.SetActive(false);
            }
        }
    }
}
