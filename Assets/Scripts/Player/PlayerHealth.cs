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
using DG.Tweening.Core.Easing;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

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
    private const int limitHealth = 10;
    private bool bSuperMode;
    private bool bDeath;
    private bool bDamage;

    //  �_���[�W���o�p��Animator
    [SerializeField] private RuntimeAnimatorController animPlayerFront;
    [SerializeField] private RuntimeAnimatorController animPlayerFrontDamage;
    [SerializeField] private RuntimeAnimatorController animPlayerBack;
    [SerializeField] private RuntimeAnimatorController animPlayerBackDamage;

    //  ���S���o�p��Animator
    [SerializeField] private RuntimeAnimatorController animPlayerDeath1;
    [SerializeField] private RuntimeAnimatorController animPlayerDeath2;

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

        //  �e�I�u�W�F�N�g�̎q�I�u�W�F�N�g�Ƃ��ăn�[�g�t���[���𐶐�
        for( int i=0; i<currentMaxHealth/2;i++ )
        {
            GameObject obj = Instantiate(heartFrameObj);
            obj.GetComponent<RectTransform>().SetParent( heartRootObj.transform);
            obj.GetComponent<RectTransform>().localScale = Vector3.one;
            obj.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0,0,0);
            obj.transform.GetChild((int)HeartType.Half).gameObject.SetActive(true);
            obj.transform.GetChild((int)HeartType.Full).gameObject.SetActive(true);


            heartList.Add( obj );   //  ���X�g�ɒǉ�
        }
    }

    void Update()
    {
        //  �n�[�g�摜���X�V
        CalculateHealthUI(currentHealth);

        //  �v���C���[������ł����珈�����Ȃ�
        if(bDeath)return;

        //  ��UI���J�n�p���X�V
        ChangeDamageBand();

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
                ChangePlayerSpriteToFront(false);    //  ������
                break;
        }
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
            //  ���G���[�hON
            bSuperMode = true;

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
                
                //  �V���b�g�̖�����
                PlayerShotManager psm = this.GetComponent<PlayerShotManager>();
                psm.DisableShot();

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
        else if(collision.CompareTag("Boss") )    //  �G�{�̂�HIT�I
        {
            //  ���G���[�hON
            bSuperMode = true;

            //  �v���C���[�̃_���[�W����
            EnemyData ed;
            if(collision.GetComponent<BossDouji>()) 
            {
                ed = collision.GetComponent<BossDouji>().GetEnemyData();
            }
            //if(collision.GetComponent<BossTsukumo>()) 
            //{
            //    ed = collision.GetComponent<BossTsukumo>().GetEnemyData();
            //}
            //if(collision.GetComponent<BossKuchinawa>()) 
            //{
            //    ed = collision.GetComponent<BossKuchinawa>().GetEnemyData();
            //}
            //if(collision.GetComponent<BossKurama>()) 
            //{
            //    ed = collision.GetComponent<BossKurama>().GetEnemyData();
            //}
            //if(collision.GetComponent<BossWadatsumi>()) 
            //{
            //    ed = collision.GetComponent<BossWadatsumi>().GetEnemyData();
            //}
            //if(collision.GetComponent<BossHakumen>()) 
            //{
            //    ed = collision.GetComponent<BossHakumen>().GetEnemyData();
            //}
            else ed = null;

            Damage( ed.Attack );

            //  ���S�t���OON
            if(currentHealth <= 0)
            {
                bDamage = false;
                bDeath = true;
                //  HitCircle���\���ɂ���
                this.transform.GetChild(6).gameObject.SetActive(false);

                //  �V���b�g�̖�����
                PlayerShotManager psm = this.GetComponent<PlayerShotManager>();
                psm.DisableShot();

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
            Debug.Log("�V���b�g�����_�E���I");
            Debug.Log("�V���b�g���� :" + ps.GetNormalShotLevel());
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
            //  ���G���[�hON
            bSuperMode = true;

            //  �v���C���[�̃_���[�W����
            int power = 0;
            if(collision.GetComponent<EnemyBullet>())
            {
                power = collision.GetComponent<EnemyBullet>().GetPower();
            }
            else if(collision.GetComponent<DoujiPhase2Bullet>())
            {
                power = collision.GetComponent<DoujiPhase2Bullet>().GetPower();
            }
            else if(collision.GetComponent<DoujiPhase3Bullet>())
            {
                power = collision.GetComponent<DoujiPhase3Bullet>().GetPower();
                Debug.Log("�_���[�W�I" + power);
            }

            Damage( power );

            //  ���S�t���OON
            if(currentHealth <= 0)
            {
                bDamage = false;
                bDeath = true;
                //  HitCircle���\���ɂ���
                this.transform.GetChild(6).gameObject.SetActive(false);

                //  �V���b�g�̖�����
                PlayerShotManager psm = this.GetComponent<PlayerShotManager>();
                psm.DisableShot();

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
    private IEnumerator GameOverAnimation()
    {
        //  �ŏI�ҋ@����
        float procedural_time = 2.0f;
        //  �A�j���[�V�����ɂ����鎞��
        float duration = 1.0f;

        //  �������W�̐ݒ�
        gameOver.GetComponent<RectTransform>().anchoredPosition
            = new Vector2 (-17, 563);

        //  GameOver�\����L����
        gameOver.SetActive(true);

        //  �ړ��J�n
        gameOver.GetComponent<RectTransform>()
            .DOAnchorPos(new Vector2(-17f,0f),duration)
            .SetEase(Ease.OutElastic);

        //  GameOver�W���O����炷
        SoundManager.Instance.PlaySFX(
        (int)AudioChannel.SFX_SYSTEM,
        (int)SFXList.SFX_GAMEOVER);

        //  �P�b�҂�
        yield return new WaitForSeconds(duration);

        //  ��]�J�n
        gameOver.GetComponent<RectTransform>()
            .DOLocalRotate(new Vector3(0, 0, -35f),duration)
            .SetEase(Ease.InCubic);

        //  �P�b�҂�
        yield return new WaitForSeconds(duration);

        //  �ړ��J�n
        gameOver.GetComponent<RectTransform>()
            .DOAnchorPos(new Vector2(-17f,-700f),duration)
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

        //  �v���C���[�̂���G�t�F�N�g
        GameObject obj = Instantiate(
            playerDeathEffect, 
            this.transform.position,
            Quaternion.identity);

        //  ����G�t�F�N�g�̏I����҂�
        yield return new WaitForSeconds(0.583f);

        //  GameOver�A�j���[�V�����̏I����҂�
        yield return StartCoroutine( GameOverAnimation() );

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
        if(health < 0)
        {
            health = 0;
        }

            //  �̗�0�Ȃ�n�[�g��S����\���ɂ���
            if (health == 0)
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
        if(currentHealth <= currentMaxHealth/2)
            faceBand1.SetActive(true);
        else faceBand1.SetActive(false);

        //  �c��̗͂�2���J�n�p�Q���L����
        if(currentHealth <= 2)
            faceBand2.SetActive(true);
        else faceBand2.SetActive(false);

    }
}
