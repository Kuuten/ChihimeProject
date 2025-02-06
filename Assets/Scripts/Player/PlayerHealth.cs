using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
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
    //  �n�[�g�̃^�C�v
    enum HeartType
    {
        Half,         //  ����
        Full,         //  �n�[�g�P��
        ShieldNone,   //  �V�[���h�n�[�g�O��
        ShieldHalf,   //  �V�[���h����
        ShieldFull,   //  �V�[���h�n�[�g�P��

        Max
    }

    [SerializeField] private int currentMaxHealth;  //  �K������
    [SerializeField] private int currentHealth;
    private const int limitHealth = 10;
    //[Watch]public bool bSuperMode;
    private bool bSuperMode;
    private bool bDeath;
    private bool bDamage;

    //  �V���O���g���ȃC���X�^���X
    public static PlayerHealth Instance
    {
        get; private set;
    }

    //  �_���[�W���o�p��Animator
    [SerializeField] private RuntimeAnimatorController animPlayerFront;
    [SerializeField] private RuntimeAnimatorController animPlayerFrontLeft;
    [SerializeField] private RuntimeAnimatorController animPlayerFrontRight;
    [SerializeField] private RuntimeAnimatorController animPlayerFrontDamage;
    [SerializeField] private RuntimeAnimatorController animPlayerBack;
    [SerializeField] private RuntimeAnimatorController animPlayerBackLeft;
    [SerializeField] private RuntimeAnimatorController animPlayerBackRight;
    [SerializeField] private RuntimeAnimatorController animPlayerBackDamage;

    //  ���S���o�p��Animator
    [SerializeField] private RuntimeAnimatorController animPlayerDeath1;
    [SerializeField] private RuntimeAnimatorController animPlayerDeath2;

    //  �V�[���h���o�p��Animator
    [SerializeField] private RuntimeAnimatorController animPlayerShieldFront;
    [SerializeField] private RuntimeAnimatorController animPlayerShieldFrontLeft;
    [SerializeField] private RuntimeAnimatorController animPlayerShieldFrontRight;
    [SerializeField] private RuntimeAnimatorController animPlayerShieldBack;
    [SerializeField] private RuntimeAnimatorController animPlayerShieldBackLeft;
    [SerializeField] private RuntimeAnimatorController animPlayerShieldBackRight;


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

    //  �V�[���h�t���O
    private bool isShielded;

    //-------------------------------------------------------------
    //  FaceUI����
    //-------------------------------------------------------------
    //  ��UI�I�u�W�F�N�g
    [SerializeField] private GameObject faceObject;
    //  ��UI�I�u�W�F�N�g��Image�R���|�[�l���g
    private Image faceImage;
    //  ��P�ʏ��X�v���C�g
    [SerializeField] private Sprite faceNormal;
    //  ��P�_���[�W��X�v���C�g
    [SerializeField] private Sprite faceDamage;
    //  ��UI���J�n�p�P
    [SerializeField] private GameObject faceBand1;
    //  ��UI���J�n�p�Q
    [SerializeField] private GameObject faceBand2;

    //  GameOver�\��
    [SerializeField] private GameObject gameOver;

    //  �h���b�v�p���[�A�b�v�A�C�e���ꗗ
    [SerializeField] private GameObject DropItem;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {

        //  PlayerInfoManager���珉����
        currentMaxHealth = PlayerInfoManager.g_MAXHP;
        if (currentMaxHealth > limitHealth)
            Debug.LogError("�ő�̗͒l�������𒴉߂��Ă��܂��I");
        if (currentMaxHealth % 2 != 0)
            Debug.LogError("currentMaxHealth�͕K�������łȂ���΂����܂���I");
        currentHealth = PlayerInfoManager.g_CURRENTHP;
        if (currentHealth > currentMaxHealth)
            Debug.LogError("���ݑ̗͒l���ő�̗͒l�𒴉߂��Ă��܂��I");

        //  ��UI��Image�R���|�[�l���g���擾
        faceImage = faceObject.GetComponent<Image>();

        //  ��UI�̏�����
        faceImage.sprite = faceNormal;

        //  �J�n�p�P�𖳌���
        faceBand1.SetActive(false);

        //  �J�n�p�Q�𖳌���
        faceBand2.SetActive(false);

        //  SpriteRender���擾
        sp = GetComponent<SpriteRenderer>();

        //  ���[�v�J�E���g��ݒ�
        loopCount = 10;

        //  �_�ł̊Ԋu��ݒ�
        flashInterval = 0.1f;

        //  �_���[�W�t���OOFF
        bDamage = false;

        //  ���S�t���OOFF
        bDeath = false;

        //  �ŏ��͖��G���[�hOFF
        bSuperMode = false;

        //  �ŏ���GameOver����\��
        gameOver.SetActive(false);

        //  �ŏ��̓V�[���h�Ȃ�
        isShielded = PlayerInfoManager.g_IS_SHIELD;

        //  �e�I�u�W�F�N�g�̎q�I�u�W�F�N�g�Ƃ��ăn�[�g�t���[���𐶐�
        for (int i = 0; i < currentMaxHealth / 2; i++)
        {
            GameObject obj = Instantiate(heartFrameObj);
            obj.GetComponent<RectTransform>().SetParent(heartRootObj.transform);
            obj.GetComponent<RectTransform>().localScale = Vector3.one;
            obj.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);
            obj.transform.GetChild((int)HeartType.Half).gameObject.SetActive(true);
            obj.transform.GetChild((int)HeartType.Full).gameObject.SetActive(true);


            heartList.Add(obj);   //  ���X�g�ɒǉ�
        }
    }

    void Update()
    {
        //  �n�[�g�摜���X�V
        CalculateHealthUI(currentHealth);

        //  �v���C���[������ł����珈�����Ȃ�
        if (bDeath) return;

        //  ��UI���J�n�p���X�V
        ChangeDamageBand();

        //  �Q�[���i�K�ʂ�Animator�̐؂�ւ�
        int gamestatus = GameManager.Instance.GetGameState();
        switch (gamestatus)
        {
            case (int)eGameState.Zako:
                ChangePlayerSpriteToFront(true);     //  ��O����
                break;
            case (int)eGameState.Boss:
                ChangePlayerSpriteToFront(false);    //  ������
                break;
            case (int)eGameState.Event:
                ChangePlayerSpriteToFront(false);    //  ������
                break;
        }
    }

    //----------------------------------------------------------------
    //  �v���C���[�̓����蔻��
    //----------------------------------------------------------------
    private async void OnTriggerEnter2D(Collider2D collision)
    {
        //  ���S���Ă���Ȃ��΂�
        if (bDeath) return;

        //  �V�[���h���
        if (isShielded)
        {
            //  ���G���[�h�Ȃ��΂�
            if (bSuperMode) return;

            if (collision.CompareTag("Enemy") ||    //  �U�R�G�{�̂�HIT�I
                collision.CompareTag("Boss") ||     //  �{�X�{�̂�HIT�I
                collision.CompareTag("EnemyBullet") //  �G�e��HIT�I
            )
            {
                //  ���G���[�hON
                bSuperMode = true;

                //  �V�[���h���폜
                isShielded = false;

                //  �V�[���h�j��SE�Đ�
                SoundManager.Instance.PlaySFX(
                    (int)AudioChannel.SFX_DAMAGE,
                    (int)SFXList.SFX_SHIELDBREAK);

                //  ���G���o�J�n
                var taskBlink = Blink(flashInterval, loopCount);
                await taskBlink;
            }
        }
        //  �ʏ���
        else
        {
            if (collision.CompareTag("Enemy"))    //  �G�{�̂�HIT�I
            {
                //  ���G���[�h�Ȃ��΂�
                if (bSuperMode) return;

                //  ���G���[�hON
                bSuperMode = true;

                //  �v���C���[�̃_���[�W����
                EnemyData ed = collision.GetComponent<Enemy>().GetEnemyData();


                //  �_���[�W�֘A����
                await AfterDamage(ed.Attack,flashInterval, loopCount);

            }
            else if (collision.CompareTag("Boss"))    //  �G�{�̂�HIT�I
            {
                //  �m�b�N�o�b�N�I
                KnockBack(collision);

                //  ���G���[�h�Ȃ��΂�
                if (bSuperMode) return;

                //  ���G���[�hON
                bSuperMode = true;

                //  �v���C���[�̃_���[�W����
                EnemyData ed = null;

                BossBase boss = null;
                switch (PlayerInfoManager.stageInfo)
                {
                    case PlayerInfoManager.StageInfo.Stage01:
                        boss = collision.GetComponent<BossDouji>();
                        break;
                    case PlayerInfoManager.StageInfo.Stage02:
                        boss = collision.GetComponent<BossTsukumo>();
                        break;
                    case PlayerInfoManager.StageInfo.Stage03:
                       boss = collision.GetComponent<BossKuchinawa>();
                       break;
                    //case PlayerInfoManager.StageInfo.Stage04:
                    //    boss = collision.GetComponent<BossKurama>();
                    //    break;
                    //case PlayerInfoManager.StageInfo.Stage05:
                    //    boss = collision.GetComponent<BossWadatsumi>();
                    //    break;
                    //case PlayerInfoManager.StageInfo.Stage06:
                    //    boss = collision.GetComponent<BossHakumen>();
                    //    break;
                    default:
                        Debug.LogError("PlayerInfoManager.stageInfo�ɔ͈͊O�̒l�������Ă��܂�");
                        break;
                }
                if (!boss) Debug.LogError("BossBase�N���X��'boss'��null�ɂȂ��Ă��܂�");


                //  �_���[�W�ʂ̐ݒ�
                ed = boss.GetEnemyData();
                if (ed == null) Debug.LogError("EnemyData�̎擾�Ɏ��s���܂���");


                //  �_���[�W�֘A����
                await AfterDamage(ed.Attack,flashInterval, loopCount);

            }
            else if (collision.CompareTag("EnemyBullet"))    //  �G�e��HIT�I
            {
                //  ���G���[�h�Ȃ��΂�
                if (bSuperMode) return;

                //  �v���C���[�̃_���[�W����
                int power = 0;

                if (collision.GetComponent<EnemyBullet>())
                {
                    power = collision.GetComponent<EnemyBullet>().GetPower();
                }
                else if (collision.GetComponent<TsukumoFireworks>())
                {
                    power = collision.GetComponent<TsukumoFireworks>().GetPower();
                }
                else if (collision.GetComponent<DoujiPhase2Bullet>())
                {
                    power = collision.GetComponent<DoujiPhase2Bullet>().GetPower();
                }
                else if (collision.GetComponent<DoujiPhase3Bullet>())
                {
                    power = collision.GetComponent<DoujiPhase3Bullet>().GetPower();
                }
                else if (collision.GetComponent<TsukumoHomingBullet>())
                {
                    power = collision.GetComponent<TsukumoHomingBullet>().GetPower();
                }
                else if (collision.GetComponent<TsukumoPhase2Bullet>())
                {
                    power = collision.GetComponent<TsukumoPhase2Bullet>().GetPower();
                }
                else if (collision.GetComponent<TsukumoPhase3Bullet>())
                {
                    power = collision.GetComponent<TsukumoPhase3Bullet>().GetPower();
                }
                // else if (collision.GetComponent<KuchinawaPhase1Bullet>())
                // {
                //     power = collision.GetComponent<KuchinawaPhase1Bullet>().GetPower();
                // }
                // else if (collision.GetComponent<KuchinawaPhase2Bullet>())
                // {
                //     power = collision.GetComponent<KuchinawaPhase2Bullet>().GetPower();
                // }
                else if (collision.GetComponent<KuchinawaPhase3Bullet>())
                {
                    power = collision.GetComponent<KuchinawaPhase3Bullet>().GetPower();

                    //  �_�Ŏ��ԗp�̕ϐ�
                    float interval = 0.01f;
                    int loop_count = 1;

                    //  �_���[�W�֘A����
                    await AfterDamage(power,interval, loop_count);

                    return;
                }



                //  �_���[�W�֘A����
                await AfterDamage(power,flashInterval, loopCount);

            }
            else if (collision.CompareTag("Obstacles"))    //  ��Q����HIT�I
            {

            }
        }
    }

    //----------------------------------------------------------------
    //  ���[�U�[�Ȃǂ̓���ȓ����蔻��
    //----------------------------------------------------------------
    private async void OnTriggerStay2D(Collider2D collision)
    {
        //  �_�Ŏ��ԗp�̕ϐ�
        float interval = 0.01f;
        int loop_count = 1;

        if (collision.CompareTag("EnemyBullet"))    //  �G�e��HIT�I
        {
            //  ���G���[�h�Ȃ��΂�
            if (bSuperMode) return;

            //  �v���C���[�̃_���[�W����
            int power = 0;

            //  �N�`�i�����[�U�[��HIT�I
            if (collision.GetComponent<KuchinawaPhase3Bullet>())
            {
                power = collision.GetComponent<KuchinawaPhase3Bullet>().GetPower();
            }

            //  �_���[�W�֘A����
            await AfterDamage(power,interval, loop_count);
        }
    }

    //-------------------------------------------
    //  �_���[�W�̈�A�̏���
    //-------------------------------------------
    private async UniTask AfterDamage(int damage_value,float flashInterval, int loopCount)
    {
        //  ���G���[�hON
        bSuperMode = true;

        //  �_���[�W����
        Damage(damage_value);

        //  ���S�t���OON
        if (currentHealth <= 0)
        {
            bDamage = false;
            bDeath = true;
            //  HitCircle���\���ɂ���
            this.transform.GetChild(6).gameObject.SetActive(false);

            //  �V���b�g�̖�����
            PlayerShotManager psm = this.GetComponent<PlayerShotManager>();
            psm.DisableShot();

            //  �{���̖�����
            this.GetComponent<PlayerBombManager>().enabled = false;

            StartCoroutine(Death());       //  ���ꉉ�o
            return;
        }

        //  �_���[�W��UI�ɕύX
        StartCoroutine(ChangeToDmageFace());

        //  �_���[�WSE�Đ�
        SoundManager.Instance.PlaySFX(
        (int)AudioChannel.SFX_DAMAGE,
        (int)SFXList.SFX_PLAYER_DAMAGE);

        //  �S�����P�i�K�_�E��
        PlayerShotManager ps = this.GetComponent<PlayerShotManager>();
        ps.LeveldownNormalShot();
        //PlayerMovement pm = this.GetComponent<PlayerMovement>();
        //pm.LeveldownMoveSpeed();

        //  �f�o�b�O�\��
        Debug.Log("�V���b�g�����P�i�K�_�E���I");
        Debug.Log("�V���b�g���� :" + ps.GetNormalShotLevel());
        Debug.Log("Player�̗̑� :" + currentHealth);

        //  �_���[�W���̐Ԃ��Ȃ�_�ŉ��o�J�n
        var task1 = DamageAnimation();
        await task1;

        //  ���G���o�J�n
        var taskBlink = Blink(flashInterval, loopCount);
        await taskBlink;
    }

    //-------------------------------------------
    //  �_���[�W���̐Ԃ��Ȃ�_�ŉ��o
    //-------------------------------------------
    private async UniTask DamageAnimation()
    {
        //GameObject���j�����ꂽ���ɃL�����Z�����΂��g�[�N�����쐬
        var token = this.GetCancellationTokenOnDestroy();

        //  ��u�F���ς��
        await UniTask.Delay(TimeSpan.FromSeconds(0.3f))
        .AttachExternalCancellation(token);

        //  �_���[�W�t���OOFF
        bDamage = false;

    }

    //-------------------------------------------
    //  �_���[�W���̖��G�_�ŉ��o
    //-------------------------------------------
    private async UniTask Blink(float flashInterval,int loopCount)
    {
        //GameObject���j�����ꂽ���ɃL�����Z�����΂��g�[�N�����쐬
        var token = this.GetCancellationTokenOnDestroy();

        //�_�Ń��[�v�J�n
        for (int i = 0; i < loopCount; i++)
        {
            //flashInterval�҂��Ă���
            await UniTask.Delay(TimeSpan.FromSeconds(flashInterval))
                .AttachExternalCancellation(token);

            //spriteRenderer���I�t
            sp.enabled = false;

            //flashInterval�҂��Ă���
            await UniTask.Delay(TimeSpan.FromSeconds(flashInterval))
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
        if (front)   //  �U�R�풆
        {
            if (bDamage) //  �_���[�W���I
            {
                this.GetComponent<Animator>().runtimeAnimatorController =
                    animPlayerFrontDamage;
            }
            else // �ʏ�
            {
                //  ���������̓��͒l�`�F�b�N
                int check = this.GetComponent<PlayerMovement>().GetHorizontalCheck();

                if (check == 1)          //  ���͂�+�����Ȃ�
                {
                    if (isShielded)  //  �V�[���h��Ԏ�
                    {
                        this.GetComponent<Animator>().runtimeAnimatorController =
                            animPlayerShieldFrontRight;
                    }
                    else
                    {
                        this.GetComponent<Animator>().runtimeAnimatorController =
                            animPlayerFrontRight;
                    }

                }
                else if (check == -1)    //  ���͂�-�����Ȃ�
                {
                    if (isShielded)  //  �V�[���h��Ԏ�
                    {
                        this.GetComponent<Animator>().runtimeAnimatorController =
                            animPlayerShieldFrontLeft;
                    }
                    else
                    {
                        this.GetComponent<Animator>().runtimeAnimatorController =
                            animPlayerFrontLeft;
                    }
                }
                else   //   ���͂Ȃ��Ȃ�
                {
                    if (isShielded)  //  �V�[���h��Ԏ�
                    {
                        this.GetComponent<Animator>().runtimeAnimatorController =
                            animPlayerShieldFront;
                    }
                    else
                    {
                        this.GetComponent<Animator>().runtimeAnimatorController =
                            animPlayerFront;
                    }
                }
            }

        }
        else        //  �{�X�풆
        {
            if (bDamage) //  �_���[�W���I
            {
                this.GetComponent<Animator>().runtimeAnimatorController =
                    animPlayerBackDamage;
            }
            else // �ʏ�
            {
                //  ���������̓��͒l�`�F�b�N
                int check = this.GetComponent<PlayerMovement>().GetHorizontalCheck();

                if (check == 1)          //  ���͂�+�����Ȃ�
                {
                    if (isShielded)  //  �V�[���h��Ԏ�
                    {
                        this.GetComponent<Animator>().runtimeAnimatorController =
                            animPlayerShieldBackRight;
                    }
                    else
                    {
                        this.GetComponent<Animator>().runtimeAnimatorController =
                            animPlayerBackRight;
                    }
                }
                else if (check == -1)    //  ���͂�-�����Ȃ�
                {
                    if (isShielded)  //  �V�[���h��Ԏ�
                    {
                        this.GetComponent<Animator>().runtimeAnimatorController =
                            animPlayerShieldBackLeft;
                    }
                    else
                    {
                        this.GetComponent<Animator>().runtimeAnimatorController =
                            animPlayerBackLeft;
                    }
                }
                else   //   ���͂Ȃ��Ȃ�
                {
                    if (isShielded)  //  �V�[���h��Ԏ�
                    {
                        this.GetComponent<Animator>().runtimeAnimatorController =
                            animPlayerShieldBack;
                    }
                    else
                    {
                        this.GetComponent<Animator>().runtimeAnimatorController =
                            animPlayerBack;
                    }
                }
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

        ////  �v���C���[�̒ʏ�e���x�����P����Ȃ��Ȃ�
        //if(GetComponent<PlayerShotManager>().GetNormalShotLevel() > 1)
        //{
        //    Debug.Log("�h���b�v�I");

        //    //  �V���b�g�����A�C�e���𗎂Ƃ�
        //    Instantiate(DropItem,this.transform.position,Quaternion.identity);
        //}


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

        //  �n�[�g�摜���X�V
        CalculateHealthUI(currentHealth);

        //  ��UI���J�n�p���X�V
        ChangeDamageBand();

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
        //  �ő�̗�10�Ŏ~�߂�
        if (target >= limitHealth)
        {
            //  11�ȏ�͏������Ȃ�
            if (target > limitHealth)
            {
                return;
            }
            //  10�ȏ��10�Ŏ~�߂�
            target = limitHealth;
        }

        currentMaxHealth = target;

        //  �e�I�u�W�F�N�g�̎q�I�u�W�F�N�g�Ƃ��ăn�[�g�t���[���𐶐�
        GameObject obj = Instantiate(heartFrameObj);
        obj.GetComponent<RectTransform>().SetParent(heartRootObj.transform);
        obj.GetComponent<RectTransform>().localScale = Vector3.one;
        obj.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);
        //obj.transform.GetChild((int)HeartType.Half).gameObject.SetActive(true);
        //obj.transform.GetChild((int)HeartType.Full).gameObject.SetActive(true);
        heartList.Add(obj);   //  ���X�g�ɒǉ�

        //  �n�[�g�摜���X�V
        CalculateHealthUI(currentHealth);

        //  ��UI���J�n�p���X�V
        ChangeDamageBand();

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

        if (currentMaxHealth != value) currentMaxHealth = value;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public void SetCurrentMaxHealth(int value)
    {
        Assert.IsFalse((value < 0 || value > limitHealth),
            "�ő�̗͂ɔ͈͊O�̐����ݒ肳��Ă��܂��I");

        if (currentMaxHealth != value) currentMaxHealth = value;
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
        return bDamage;
    }

    public void SetSuperMode(bool flag) { bSuperMode = flag; }
    public bool GetSuperMode() { return bSuperMode; }
    public void SetIsShielded(bool flag) { isShielded = flag; }
    public bool GetIsShielded() { return isShielded; }

    //-------------------------------------------
    //  ���ꉉ�o
    //-------------------------------------------
    private IEnumerator GameOverAnimation()
    {
        //  �ŏI�ҋ@����
        float procedural_time = 2.0f;
        //  �A�j���[�V�����ɂ����鎞��
        float duration = 1.0f;

        //  �������W�̐ݒ�
        gameOver.GetComponent<RectTransform>().anchoredPosition
            = new Vector2(-17, 563);

        //  GameOver�\����L����
        gameOver.SetActive(true);

        //  �ړ��J�n
        gameOver.GetComponent<RectTransform>()
            .DOAnchorPos(new Vector2(-17f, 0f), duration)
            .SetEase(Ease.OutElastic);

        //  GameOver�W���O����炷
        SoundManager.Instance.PlaySFX(
        (int)AudioChannel.SFX_SYSTEM,
        (int)SFXList.SFX_GAMEOVER);

        //  �P�b�҂�
        yield return new WaitForSeconds(duration);

        //  ��]�J�n
        gameOver.GetComponent<RectTransform>()
            .DOLocalRotate(new Vector3(0, 0, -35f), duration)
            .SetEase(Ease.InCubic);

        //  �P�b�҂�
        yield return new WaitForSeconds(duration);

        //  �ړ��J�n
        gameOver.GetComponent<RectTransform>()
            .DOAnchorPos(new Vector2(-17f, -700f), duration)
            .SetEase(Ease.OutElastic);

        //  �P�b�҂�
        yield return new WaitForSeconds(duration);

        //  GameOver�\���𖳌���
        gameOver.SetActive(false);

        //  �ŏI�ҋ@
        yield return new WaitForSeconds(procedural_time);

        yield return null;
    }

    //-------------------------------------------
    //  ���ꉉ�o
    //-------------------------------------------
    private IEnumerator Death()
    {
        //  �f�o�b�O�\��
        Debug.Log("�v���C���[�����S���܂����I");

        //  ���݂�BGM���X�g�b�v
        SoundManager.Instance.Stop((int)AudioChannel.MUSIC);

        //  ���݃X�e�[�W�����Z�b�g
        PlayerInfoManager.stageInfo = PlayerInfoManager.StageInfo.Stage01;

        //  �_���[�W��ɕύX����
        faceImage.sprite = faceDamage;

        //  ���SSE�Đ�
        SoundManager.Instance.PlaySFX(
        (int)AudioChannel.SFX_DAMAGE,
        (int)SFXList.SFX_PLAYER_DEATH);

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

        //  �v���C���[�̉e(�V�Ԗڂ̎q�I�u�W�F�N�g)����\���ɂ���
        this.transform.GetChild(7).gameObject.SetActive(false);

        //  �v���C���[�̂���G�t�F�N�g
        GameObject obj = Instantiate(
            playerDeathEffect,
            this.transform.position,
            Quaternion.identity);

        //  ����G�t�F�N�g�̏I����҂�
        yield return new WaitForSeconds(0.583f);

        //  GameOver�A�j���[�V�����̏I����҂�
        yield return StartCoroutine(GameOverAnimation());

        //  GameOver�փV�[���J��
        LoadingScene.Instance.LoadNextScene("GameOver");

        yield return null;
    }

    //---------------------------------------------------
    //  ���ݑ̗͂��󂯎���đ̗�UI���v�Z����
    //---------------------------------------------------
    public void CalculateHealthUI(int health)
    {
        if (health < 0)
        {
            health = 0;
        }

        //  �̗�0�Ȃ�n�[�g��S����\���ɂ���
        if (health == 0)
        {
            for (int i = 0; i < heartList.Count; i++)
            {
                heartList[i].transform.GetChild((int)HeartType.Half)
                    .gameObject.SetActive(false);
                heartList[i].transform.GetChild((int)HeartType.Full)
                    .gameObject.SetActive(false);
                heartList[i].transform.GetChild((int)HeartType.ShieldNone)
                    .gameObject.SetActive(false);
                heartList[i].transform.GetChild((int)HeartType.ShieldHalf)
                    .gameObject.SetActive(false);
                heartList[i].transform.GetChild((int)HeartType.ShieldFull)
                    .gameObject.SetActive(false);
            }
        }
        else if (health == 1)
        {
            for (int i = 0; i < heartList.Count; i++)
            {
                if (i == 0)
                {
                    //  �V�[���h��Ԏ��̏���
                    if (isShielded)
                    {
                        heartList[i].transform.GetChild((int)HeartType.Half)
                            .gameObject.SetActive(true);
                        heartList[i].transform.GetChild((int)HeartType.Full)
                            .gameObject.SetActive(false);
                        heartList[i].transform.GetChild((int)HeartType.ShieldNone)
                            .gameObject.SetActive(true);
                        heartList[i].transform.GetChild((int)HeartType.ShieldHalf)
                            .gameObject.SetActive(true);
                        heartList[i].transform.GetChild((int)HeartType.ShieldFull)
                            .gameObject.SetActive(false);
                    }
                    //  �ʏ��Ԏ��̏���
                    else
                    {
                        heartList[i].transform.GetChild((int)HeartType.Half)
                            .gameObject.SetActive(true);
                        heartList[i].transform.GetChild((int)HeartType.Full)
                            .gameObject.SetActive(false);
                        heartList[i].transform.GetChild((int)HeartType.ShieldNone)
                            .gameObject.SetActive(false);
                        heartList[i].transform.GetChild((int)HeartType.ShieldHalf)
                            .gameObject.SetActive(false);
                        heartList[i].transform.GetChild((int)HeartType.ShieldFull)
                            .gameObject.SetActive(false);
                    }
                }

                //  �c����\���ɂ���
                for (int j = 1; j < heartList.Count; j++)
                {
                    //  �V�[���h��Ԏ��̏���
                    if (isShielded)
                    {
                        heartList[j].transform.GetChild((int)HeartType.Half)
                            .gameObject.SetActive(false);
                        heartList[j].transform.GetChild((int)HeartType.Full)
                            .gameObject.SetActive(false);
                        heartList[j].transform.GetChild((int)HeartType.ShieldNone)
                            .gameObject.SetActive(true);
                        heartList[j].transform.GetChild((int)HeartType.ShieldHalf)
                            .gameObject.SetActive(false);
                        heartList[j].transform.GetChild((int)HeartType.ShieldFull)
                            .gameObject.SetActive(false);
                    }
                    //  �ʏ��Ԏ��̏���
                    else
                    {
                        heartList[j].transform.GetChild((int)HeartType.Half)
                            .gameObject.SetActive(false);
                        heartList[j].transform.GetChild((int)HeartType.Full)
                            .gameObject.SetActive(false);
                        heartList[j].transform.GetChild((int)HeartType.ShieldNone)
                            .gameObject.SetActive(false);
                        heartList[j].transform.GetChild((int)HeartType.ShieldHalf)
                            .gameObject.SetActive(false);
                        heartList[j].transform.GetChild((int)HeartType.ShieldFull)
                            .gameObject.SetActive(false);
                    }
                }

            }
        }
        else // �̗͂��Q�ȏ�̎�
        {
            //  ��U���ݑ̗͂̂Ƃ��܂őS���t���Ŗ��߂�
            int fullNum = health / 2;
            for (int i = 0; i < fullNum; i++)
            {
                //  �V�[���h��Ԏ��̏���
                if (isShielded)
                {
                    heartList[i].transform.GetChild((int)HeartType.Half)
                        .gameObject.SetActive(true);
                    heartList[i].transform.GetChild((int)HeartType.Full)
                        .gameObject.SetActive(true);
                    heartList[i].transform.GetChild((int)HeartType.ShieldNone)
                        .gameObject.SetActive(true);
                    heartList[i].transform.GetChild((int)HeartType.ShieldHalf)
                        .gameObject.SetActive(true);
                    heartList[i].transform.GetChild((int)HeartType.ShieldFull)
                        .gameObject.SetActive(true);
                }
                //  �ʏ��Ԏ��̏���
                else
                {
                    heartList[i].transform.GetChild((int)HeartType.Half)
                        .gameObject.SetActive(true);
                    heartList[i].transform.GetChild((int)HeartType.Full)
                        .gameObject.SetActive(true);
                    heartList[i].transform.GetChild((int)HeartType.ShieldNone)
                        .gameObject.SetActive(false);
                    heartList[i].transform.GetChild((int)HeartType.ShieldHalf)
                        .gameObject.SetActive(false);
                    heartList[i].transform.GetChild((int)HeartType.ShieldFull)
                        .gameObject.SetActive(false);
                }
            }

            //  ��������ꍇ�͍Ō�̔ԍ������n�[�t�ɂ���
            int taegetNum = health - fullNum;
            if (health % 2 != 0)
            {
                //  �V�[���h��Ԏ��̏���
                if (isShielded)
                {
                    heartList[taegetNum - 1].transform.GetChild((int)HeartType.Half)
                        .gameObject.SetActive(true);
                    heartList[taegetNum - 1].transform.GetChild((int)HeartType.Full)
                        .gameObject.SetActive(false);
                    heartList[taegetNum - 1].transform.GetChild((int)HeartType.ShieldNone)
                        .gameObject.SetActive(true);
                    heartList[taegetNum - 1].transform.GetChild((int)HeartType.ShieldHalf)
                        .gameObject.SetActive(true);
                    heartList[taegetNum - 1].transform.GetChild((int)HeartType.ShieldFull)
                        .gameObject.SetActive(false);
                }
                //  �ʏ��Ԏ��̏���
                else
                {
                    heartList[taegetNum - 1].transform.GetChild((int)HeartType.Half)
                        .gameObject.SetActive(true);
                    heartList[taegetNum - 1].transform.GetChild((int)HeartType.Full)
                        .gameObject.SetActive(false);
                    heartList[taegetNum - 1].transform.GetChild((int)HeartType.ShieldNone)
                        .gameObject.SetActive(false);
                    heartList[taegetNum - 1].transform.GetChild((int)HeartType.ShieldHalf)
                        .gameObject.SetActive(false);
                    heartList[taegetNum - 1].transform.GetChild((int)HeartType.ShieldFull)
                        .gameObject.SetActive(false);
                }

            }

            //  �c����\���ɂ���
            for (int i = taegetNum; i < heartList.Count; i++)
            {
                //  �V�[���h��Ԏ��̏���
                if (isShielded)
                {
                    heartList[i].transform.GetChild((int)HeartType.Half)
                        .gameObject.SetActive(false);
                    heartList[i].transform.GetChild((int)HeartType.Full)
                        .gameObject.SetActive(false);
                    heartList[i].transform.GetChild((int)HeartType.ShieldNone)
                        .gameObject.SetActive(true);
                    heartList[i].transform.GetChild((int)HeartType.ShieldHalf)
                        .gameObject.SetActive(false);
                    heartList[i].transform.GetChild((int)HeartType.ShieldFull)
                        .gameObject.SetActive(false);
                }
                //  �ʏ��Ԏ��̏���
                else
                {
                    heartList[i].transform.GetChild((int)HeartType.Half)
                        .gameObject.SetActive(false);
                    heartList[i].transform.GetChild((int)HeartType.Full)
                        .gameObject.SetActive(false);
                    heartList[i].transform.GetChild((int)HeartType.ShieldNone)
                        .gameObject.SetActive(false);
                    heartList[i].transform.GetChild((int)HeartType.ShieldHalf)
                        .gameObject.SetActive(false);
                    heartList[i].transform.GetChild((int)HeartType.ShieldFull)
                        .gameObject.SetActive(false);
                }
            }
        }
    }

    //---------------------------------------------------
    //  ��UI���_���[�W��ɕύX����
    //---------------------------------------------------
    private IEnumerator ChangeToDmageFace()
    {
        //  �_���[�W��ɕύX����
        faceImage.sprite = faceDamage;

        //  �w�莞�ԑ҂�
        yield return new WaitForSeconds(1);

        //  �ʏ��ɖ߂�
        faceImage.sprite = faceNormal;
    }

    //---------------------------------------------------
    //  ��UI���J�n�p���c��̗͂ɂ���Đ؂�ւ���
    //---------------------------------------------------
    private void ChangeDamageBand()
    {
        //  �̗͂��ő�̗͂̔����ȉ����J�n�p�P���L����
        if (currentHealth <= currentMaxHealth / 2)
            faceBand1.SetActive(true);
        else faceBand1.SetActive(false);

        //  �c��̗͂�2���J�n�p�Q���L����
        if (currentHealth <= 2)
            faceBand2.SetActive(true);
        else faceBand2.SetActive(false);

    }

    //------------------------------------------------------
    //  �m�b�N�o�b�N
    //------------------------------------------------------
    private void KnockBack(Collider2D collision)
    {
        float distance = 5.0f;  //  �m�b�N�o�b�N����
        float duration = 0.1f; //  �ړ��ɂ����鎞�ԁi�b�j

        //  �{�X����̃x�N�g�����v�Z
        Vector3 vec = -collision.transform.up;
        Vector3 ppos = collision.transform.position;
        Vector3 direction = default;    //  �m�b�N�o�b�N����

        //Debug.Log($"<color=yellow>�m�b�N�o�b�N�I</color>");

        //  �ⓚ���p�Ń{�X�̑O�Ƀm�b�N�o�b�N����
        direction = vec;

        //  �m�b�N�o�b�N��̍��W���v�Z
        Vector3 pos = ppos + direction * distance;

        //  �ړ��J�n
        this.transform.DOMove(pos, duration)
            .SetEase(Ease.Linear);
    }
}
