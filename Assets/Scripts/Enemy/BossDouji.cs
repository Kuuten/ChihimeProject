using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

// �G�̒e�̎��
public enum BULLET_TYPE {
    //  �e
    Snipe_Normal,        //  ���@�_���e�E�ʏ�
    Snipe_RotationL,     //  ���@�_���e�E����]
    Snipe_RotationR,     //  ���@�_���e�E�E��]
    Snipe_Long,          //  ���@�_���e�E�����O
    Snipe_Big,           //  ���@�_���e�E���
    //  �Ԓe
    Wildly_Normal,       //  �o���}�L�e�E�ʏ�
    Wildly_RotationL,    //  �o���}�L�e�E����]
    Wildly_RotationR,    //  �o���}�L�e�E�E��]
    Wildly_Long,         //  �o���}�L�e�E�����O
    Wildly_Big,          //  �o���}�L�e�E���
    //  �h�E�W�M�~�b�N�e
    Douji_Gimmick_Top,
    Douji_Gimmick_Bottom,
    Douji_Gimmick_Left,
    Douji_Gimmick_Right,
    //  �M�~�b�N�x��
    Douji_Warning,
    //  �h�E�W�����e
    Douji_Berserk_Bullet,
    //  �x���̗\�����C��
    Douji_DangerLine_Top,
    Douji_DangerLine_Bottom,
    Douji_DangerLine_Left,
    Douji_DangerLine_Right,
}

//--------------------------------------------------------------
//
//  �{�X�E�h�E�W�̃N���X
//
//--------------------------------------------------------------
public class BossDouji : MonoBehaviour
{
    //  EnemyData�N���X����̏��擾�p
    EnemyData enemyData;
    
    //  �p�����[�^
    private float hp;
    private bool bDeath;            //  ���S�t���O
    private bool bSuperMode;        //  ���G���[�h�t���O
    private bool bSuperModeInterval;//  �t�F�[�Y�؂�ւ����̖��G���[�h�t���O

    //  �_�ł����邽�߂�SpriteRenderer
    SpriteRenderer sp;
    //  �_�ł̊Ԋu
    private float flashInterval;
    //  �_�ł�����Ƃ��̃��[�v�J�E���g
    private int loopCount;

    //  ����G�t�F�N�g
    [SerializeField] private GameObject explosion;

    //  HP�X���C�_�[
    private Slider hpSlider;

    //  ����_
    enum Control
    {
        Left,
        Center,
        Right,

        Max
    };

    //  �h���b�v�p���[�A�b�v�A�C�e���ꗗ
    private ePowerupItems powerupItems;

    //  �M�~�b�N�e�̎g�p�ςݔԍ��i�[�p
    private int[] kooniNum = new int[(int)DoujiPhase2Bullet.KooniDirection.MAX];

    //------------------------------------------------------------
    //  Phase2�p
    //------------------------------------------------------------
    private GameObject warningObject;

    private bool bWarningFirst;

    //  WARNING���̗\�����C��
    private GameObject[] dangerLineObject;

    //  �R���[�`����~�p�t���O
    Coroutine phase1_Coroutine;
    Coroutine phase2_Coroutine;
    private bool bStopPhase1;
    private bool bStopPhase2;

    private const float phase1_end = 0.66f;   //  �t�F�[�Y�P�I��������HP������臒l
    private const float phase2_end = 0.33f;   //  �t�F�[�Y�Q�I��������HP������臒l



    void Start()
    {
        //  �x���I�u�W�F�N�g���擾
        warningObject = new GameObject();
        warningObject =
            EnemyManager.Instance.GetBulletPrefab((int)BULLET_TYPE.Douji_Warning);

        //  �t���O������
        bStopPhase1 = false;
        bStopPhase2 = false;

        //  WARNING���̗\�����C���I�u�W�F�N�g���擾
        dangerLineObject = new GameObject[(int)DoujiPhase2Bullet.KooniDirection.MAX];
        dangerLineObject[(int)DoujiPhase2Bullet.KooniDirection.TOP] =
            EnemyManager.Instance.GetBulletPrefab((int)BULLET_TYPE.Douji_DangerLine_Top);
        dangerLineObject[(int)DoujiPhase2Bullet.KooniDirection.BOTTOM] =
            EnemyManager.Instance.GetBulletPrefab((int)BULLET_TYPE.Douji_DangerLine_Bottom);
        dangerLineObject[(int)DoujiPhase2Bullet.KooniDirection.LEFT] =
            EnemyManager.Instance.GetBulletPrefab((int)BULLET_TYPE.Douji_DangerLine_Left);
        dangerLineObject[(int)DoujiPhase2Bullet.KooniDirection.RIGHT] =
            EnemyManager.Instance.GetBulletPrefab((int)BULLET_TYPE.Douji_DangerLine_Right);

        //  �M�~�b�N�e�̎g�p�ςݔԍ���������
        for(int i=0;i<(int)DoujiPhase2Bullet.KooniDirection.MAX;i++)
        {
            kooniNum[i] = -1;
        }

        //  ���S�t���OOFF
        bDeath = false;
        //  �ŏ��͖��G���[�hOFF
        bSuperMode = false;
        bSuperModeInterval = false;
        //  ���[�v�J�E���g��ݒ�
        loopCount = 1;
        //  �_�ł̊Ԋu��ݒ�
        flashInterval = 0.2f;
        //  SpriteRender���擾
        sp = GetComponent<SpriteRenderer>();
        //  Warning�̏���t���O
        bWarningFirst = false;

        //  �s���J�n
        StartCoroutine(WaitDoujiAction(1));
    }

    private void OnDestroy()
    {
        Debug.Log("�{�X���j�I�X�e�[�W�N���A�I");

        //  �{�X����ꂽ��X�e�[�W�N���A
        GameManager.Instance.SetStageClearFlag(true);
    }

    //*********************************************************************************
    //
    //  �X�V
    //
    //*********************************************************************************
    void Update()
    {
        //  HP��臒l��؂����甲����
        if (hp <= enemyData.Hp*phase1_end)
        {
            if(!bStopPhase1)
            {
                bStopPhase1 = true;
            }
        }
        if(hp <= enemyData.Hp*phase2_end)
        {
            if(!bStopPhase2)
            {
                bStopPhase2 = true;
            }
        }
        
        //  �X���C�_�[���X�V
        hpSlider.value = hp / enemyData.Hp;
    }

    //  �G�̃f�[�^��ݒ� 
    public void SetBossData(EnemySetting es, ePowerupItems item)
    {
        string boss_id = "Douji";

        //  �G�̃f�[�^��ݒ� 
        enemyData = es.DataList
            .FirstOrDefault(enemy => enemy.Id == boss_id );

        //  �̗͂�ݒ�
        hp = enemyData.Hp;

        Debug.Log( "�^�C�v: " + boss_id + "\nHP: " + hp );
        Debug.Log( boss_id + "�̐ݒ芮��" );

        //Debug.Log($"ID�F{enemyData.Id}");
        //Debug.Log($"HP�F{enemyData.Hp}");
        //Debug.Log($"�U���́F{enemyData.Attack}");
        //Debug.Log($"�����F{enemyData.Money}");

        if(item == ePowerupItems.None)
        {
            powerupItems = default;
        }
        else
        {
            //  �h���b�v�A�C�e����ݒ�
            powerupItems = item;    
        }
    }

    //----------------------------------------------------------------------
    //  �v���p�e�B
    //----------------------------------------------------------------------
    public EnemyData GetEnemyData(){ return enemyData; }
    public void SetEnemyData(EnemyData ed){ enemyData = ed; }
    public void SetHp(float health){ hp = health; }
    public float GetHp(){ return hp; }
    public void SetSuperMode(bool flag){ bSuperMode = flag; }
    public bool GetSuperMode(){ return bSuperMode; }
    public void SetHpSlider(Slider s){ hpSlider = s; }

    //------------------------------------------------------
    //  �_���[�WSE���Đ������㖳�G���[�h���I�t�ɂ���
    //------------------------------------------------------
    IEnumerator PlayDamageSFXandSuperModeOff()
    {
        float interval = 0.1f;  //  ���G�����������܂ł̎��ԁi�b�j

        //  �_���[�WSE�Đ�
        SoundManager.Instance.PlaySFX(
        (int)AudioChannel.SFX_ENEMY,
        (int)SFXList.SFX_ENEMY_DAMAGE);

        //  ���b���҂�
        yield return new WaitForSeconds(interval);

        //  ���G���[�hOFF
        bSuperMode = false;
    }

    //  �G�ɓ��������甚������
    //  �����蔻��̊�b�m���F
    //  �����蔻����s���ɂ́A
    //  �E���҂�Collider�����Ă���
    //  �E�ǂ��炩��RigidBody�����Ă���
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(bDeath)return;


        if (collision.CompareTag("NormalBullet"))
        {
            //  �e�̏���
            Destroy(collision.gameObject);

            //  ���G���[�h�Ȃ�e���������ĕԂ�
            if(bSuperMode || bSuperModeInterval)return;

            //  �_���[�W����
            float d = GameManager.Instance.GetPlayer()
                .GetComponent<PlayerShotManager>().GetNormalShotPower();
            Damage(d);

            //  �_�ŉ��o
            StartCoroutine(Blink(true,loopCount,flashInterval));

            //  �_���[�WSE�Đ�
            StartCoroutine(PlayDamageSFXandSuperModeOff());

            //  ���S�t���OON
            if(hp <= 0)
            {
                bDeath = true;
                Death();                       //  ���ꉉ�o
            }
        }
        else if (collision.CompareTag("DoujiConvert"))
        {
            //  �t�F�[�Y�ؑ֎��̖��G���[�h�Ȃ�e���������ĕԂ�
            if(bSuperModeInterval)
            {
                //  �e�̏���
                Destroy(collision.gameObject);
                return;
            }

            //  �_���[�W����
            float d = collision.GetComponent<ConvertDoujiBullet>().GetInitialPower();
            Damage(d);

            //  ���U�������U��������
            bool isFullPower = GameManager.Instance.GetPlayer()
                .GetComponent<PlayerShotManager>().IsConvertFullPower();

            //  ���o�[�X�g�Q�[�W�𑝂₷
            if(isFullPower)
            {
                PlayerBombManager.Instance.PlusKonburstGauge(true);
            }
            //  ���o�[�X�g�Q�[�W���������₷
            else
            {
                PlayerBombManager.Instance.PlusKonburstGauge(false);  
            }

            //  �_�ŉ��o
            StartCoroutine(Blink(true,loopCount,flashInterval));

            //  �_���[�WSE�Đ�
            StartCoroutine(PlayDamageSFXandSuperModeOff());

            //  ���S�t���OON
            if(hp <= 0)
            {
                bDeath = true;
                Death2();                      //  ���ꉉ�o2
            }
        }
        else if (collision.CompareTag("DoujiKonburst"))
        {
            //  �t�F�[�Y�ؑ֎��̖��G���[�h�Ȃ�e���������ĕԂ�
            if(bSuperModeInterval)return;

            //  �_���[�W����
            float d = GameManager.Instance.GetPlayer()
                .GetComponent<PlayerBombManager>().GetKonburstShotPower();
            Damage(d);

            //  �_�ŉ��o
            StartCoroutine(Blink(true,loopCount,flashInterval));

            //  �_���[�WSE�Đ�
            StartCoroutine(PlayDamageSFXandSuperModeOff());

            //  ���S�t���OON
            if(hp <= 0)
            {
                bDeath = true;
                Death2();                      //  ���ꉉ�o2
            }
        }
        else if (collision.CompareTag("TsukumoConvert"))
        {
            //  �t�F�[�Y�ؑ֎��̖��G���[�h�Ȃ�e���������ĕԂ�
            if(bSuperModeInterval)
            {
                //  �e�̏���
                Destroy(collision.gameObject);
                return;
            }

            //  �e������
            Destroy(collision.gameObject);

            //  �_���[�W����
            float d = GameManager.Instance.GetPlayer()
                .GetComponent<PlayerShotManager>().GetConvertShotPower();
            Damage(d);

            //  ���U�������U��������
            bool isFullPower = GameManager.Instance.GetPlayer()
                .GetComponent<PlayerShotManager>().IsConvertFullPower();

            //  ���o�[�X�g�Q�[�W�𑝂₷
            if(isFullPower)
            {
                PlayerBombManager.Instance.PlusKonburstGauge(true);
            }
            //  ���o�[�X�g�Q�[�W���������₷
            else
            {
                PlayerBombManager.Instance.PlusKonburstGauge(false);  
            }

            //  �_�ŉ��o
            StartCoroutine(Blink(true,loopCount,flashInterval));

            //  �_���[�WSE�Đ�
            StartCoroutine(PlayDamageSFXandSuperModeOff());

            //  ���S�t���OON
            if(hp <= 0)
            {
                bDeath = true;
                Death2();                      //  ���ꉉ�o2
            }
        }
        else if (collision.CompareTag("TsukumoKonburst"))
        {
            //  �t�F�[�Y�ؑ֎��̖��G���[�h�Ȃ�e���������ĕԂ�
            if(bSuperModeInterval)
            {
                //  �e�̏���
                Destroy(collision.gameObject);
                return;
            }

            //  �e������
            Destroy(collision.gameObject);

            //  �_���[�W����
            float d = GameManager.Instance.GetPlayer()
                .GetComponent<PlayerBombManager>().GetKonburstShotPower();
            Damage(d);

            //  �_�ŉ��o
            StartCoroutine(Blink(true,loopCount,flashInterval));

            //  �_���[�WSE�Đ�
            StartCoroutine(PlayDamageSFXandSuperModeOff());

            //  ���S�t���OON
            if(hp <= 0)
            {
                bDeath = true;
                Death2();                      //  ���ꉉ�o2
            }
        }
        else if (collision.CompareTag("Bomb"))
        {
            //  �t�F�[�Y�ؑ֎��̖��G���[�h�Ȃ�Ԃ�
            if(bSuperModeInterval)return;

            //  �_���[�W����
            float d = GameManager.Instance.GetPlayer()
                .GetComponent<PlayerBombManager>().GetBombPower();
            Damage(d);

            //  �_�ŉ��o
            StartCoroutine(Blink(true,loopCount,flashInterval));

            //  �_���[�WSE�Đ�
            StartCoroutine(PlayDamageSFXandSuperModeOff());

            //  ���S�t���OON
            if(hp <= 0)
            {
                bDeath = true;
                Death();                        //  ���ꉉ�o
            }
        }

        // �c��HP�\��
        //Debug.Log("�c��HP: " + hp);
    }

    //-------------------------------------------
    //  �_���[�W����
    //-------------------------------------------
    public void Damage(float value)
    {
        if(hp > 0.0f)
        {
            hp -= value;
        }
        else
        {
            hp = 0.0f;
        }
    }

   //-------------------------------------------
    //  �_���[�W���̓_�ŉ��o
    //-------------------------------------------
    public IEnumerator Blink(bool super, int loop_count, float flash_interval)
    {
        //  ���G���[�hON
        if(super)bSuperMode = true;

        //�_�Ń��[�v�J�n
        for (int i = 0; i < loop_count; i++)
        {
            //flashInterval�҂��Ă���
            yield return new WaitForSeconds(flash_interval);

            //spriteRenderer���I�t
            if(sp)sp.enabled = false;

            //flashInterval�҂��Ă���
            yield return new WaitForSeconds(flash_interval);

            //spriteRenderer���I��
            if(sp)sp.enabled = true;
        }
        //  ���G���[�hOFF
        if(super)bSuperMode = false;
    }

    //-------------------------------------------
    //  ���ꉉ�o(�ʏ�e�E�{��)
    //-------------------------------------------
    private void Death()
    {
        //  ����G�t�F�N�g
        Instantiate(explosion, transform.position, transform.rotation);

        // ����SE
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX_ENEMY,
            (int)SFXList.SFX_ENEMY_DEATH);

        //  �A�C�e���h���b�v����
        DropItems drop = this.GetComponent<DropItems>();
        if (powerupItems == ePowerupItems.Random)
        {
            if (drop) drop.DropRandomPowerupItem();
        }
        else
        {
            if (drop) drop.DropPowerupItem(powerupItems);
        }

        //  �����𐶐�
        drop.DropKon(enemyData.Money);

        //  �I�u�W�F�N�g���폜
        Destroy(this.gameObject);
    }

    //-------------------------------------------
    //  ���ꉉ�o(���o�[�g)
    //-------------------------------------------
    private void Death2()
    {
        //  ����G�t�F�N�g
        Instantiate(explosion, transform.position, transform.rotation);

        // ����SE
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX_ENEMY,
            (int)SFXList.SFX_ENEMY_DEATH);

        //  �A�C�e���h���b�v����
        DropItems drop = this.GetComponent<DropItems>();
        if (powerupItems == ePowerupItems.Random)
        {
            if (drop) drop.DropRandomPowerupItem();
        }
        else
        {
            if (drop) drop.DropPowerupItem(powerupItems);
        }

        //  �����𐶐�(���o�[�g�̎���2�{)
        int dropMoney = enemyData.Money;
        drop.DropKon(2 * dropMoney);

        //  �I�u�W�F�N�g���폜
        Destroy(this.gameObject);
    }

    //******************************************************************
    //
    //  �h�E�W�̈ړ��p�^�[��
    //
    //******************************************************************

    //------------------------------------------------------------------
    //  ���b�҂��Ă���h�E�W�̍s���J�n
    //------------------------------------------------------------------
    private IEnumerator WaitDoujiAction(float duration)
    {
        yield return new WaitForSeconds(duration);

        //  �s���J�n
        StartCoroutine(StartDoujiAction());
    }

    //------------------------------------------------------------------
    //  �h�E�W�̍s���Ǘ��֐�
    //------------------------------------------------------------------
    private IEnumerator StartDoujiAction()
    {
        Debug.Log("***�h�E�W�e���t�F�[�Y�J�n�I***");

        //  �t�F�[�Y�P�J�n
        phase1_Coroutine = StartCoroutine(Douji_Phase1());

        //  �t���O��TRUE�ɂȂ�܂ő҂�
        yield return new WaitUntil(()=>bStopPhase1 == true);

        //  �t���O�ŃR���[�`����~
        StopCoroutine(phase1_Coroutine);

        //  �t�F�[�Y�ύX
        yield return StartCoroutine(Douji_PhaseChange());

        //  �t�F�[�Y�Q�J�n
        phase2_Coroutine = StartCoroutine(Douji_Phase2());

        //  �t���O��TRUE�ɂȂ�܂ő҂�
        yield return new WaitUntil(()=>bStopPhase2 == true);

        //  �t���O�ŃR���[�`����~
        StopCoroutine(phase2_Coroutine);

        //  �t�F�[�Y�ύX
        yield return StartCoroutine(Douji_PhaseChange());

        //  �t�F�[�Y�R�J�n
        StartCoroutine(Douji_Phase3());
    }

    //------------------------------------------------------------------
    //  �h�E�W�̃t�F�[�Y�`�F���W���̍s��
    //------------------------------------------------------------------
    private IEnumerator Douji_PhaseChange()
    {
        //  �ړ��ɂ����鎞��(�b)
        float duration = 1.5f;
        //  �ړ���ɑҋ@���鎞��(�b)
        float interval = 5.0f;

        //  �^�񒆂Ɉړ�����
        yield return StartCoroutine(Douji_MoveToCenter(duration));

        //  ���̃t�F�[�Y�܂ő҂�
        yield return new WaitForSeconds(interval);

        //  ���G���[�hOFF
        bSuperModeInterval = false;
    }

    //------------------------------------------------------------------
    //  �h�E�W��Phase1
    //------------------------------------------------------------------
    private IEnumerator Douji_Phase1()
    {
        Debug.Log("�t�F�[�Y�P�J�n");

        //  �t�F�[�Y�P
        while (!bStopPhase1)
        {
            yield return StartCoroutine(Douji_LoopMove(1.5f, 0.5f));

            yield return StartCoroutine(Shot());


            //yield return StartCoroutine(Douji_LoopMove(1.0f, 1.0f));
            //yield return StartCoroutine(Warning());
            //StartCoroutine(KooniParty());
            //StartCoroutine(KooniParty());
            //StartCoroutine(KooniParty());
            //yield return StartCoroutine(KooniParty());


            //yield return StartCoroutine(Douji_BerserkBarrage());

            //yield return StartCoroutine(Douji_LoopMoveBerserk(3, 0.6f, 1.0f));

            //yield return StartCoroutine(Douji_BerserkGatling());

            //yield return StartCoroutine(Douji_LoopMoveBerserk(3, 0.6f, 1.0f));

            //yield return StartCoroutine(Douji_BerserkGatling());
        }
    }

    //------------------------------------------------------------------
    //  �h�E�W��Phase2
    //------------------------------------------------------------------
    private IEnumerator Douji_Phase2()
    {
        Debug.Log("�t�F�[�Y�Q�ֈڍs");

        //  �t�F�[�Y�Q
        while (!bStopPhase2)
        {
            yield return StartCoroutine(Douji_LoopMove(1.0f,1.0f));

            StartCoroutine(WildlyShotSmall());

            //  Warning!(����̂�)
            yield return StartCoroutine(Warning());

            StartCoroutine(KooniParty());
            StartCoroutine(KooniParty());
            StartCoroutine(KooniParty());

            yield return StartCoroutine(KooniParty());
        }
    }

    //------------------------------------------------------------------
    //  �h�E�W��Phase3
    //------------------------------------------------------------------
    private IEnumerator Douji_Phase3()
    {
        Debug.Log("�t�F�[�Y�R�ֈڍs");

        //  �t�F�[�Y�R
        while (true)
        {
            yield return StartCoroutine(Douji_LoopMoveBerserk(3, 0.6f, 1.0f));

            yield return StartCoroutine(Douji_BerserkBarrage());
            yield return StartCoroutine(Douji_BerserkGatling());

            yield return StartCoroutine(Douji_LoopMoveBerserk(3, 0.6f, 1.0f));

            yield return StartCoroutine(Douji_BerserkBarrage());
            yield return StartCoroutine(Douji_BerserkGatling());
        }
    }

    //------------------------------------------------------------------
    //  �h�E�W�̈ړ�
    //------------------------------------------------------------------
    private IEnumerator Douji_LoopMove(float duration,float interval)
    {
        int currentlNum = (int)Control.Center;      //  ���݈ʒu
        List<int> targetList = new List<int>();     //  �ڕW�ʒu��⃊�X�g
        int targetNum = (int)Control.Center;        //  �ڕW�ʒu

        //  ���݈ʒu�����߂�i��ԋ߂��ʒu�Ƃ���j
        Vector3 p1 = EnemyManager.Instance.GetControlPointPos((int)Control.Left);
        Vector3 p2 = EnemyManager.Instance.GetControlPointPos((int)Control.Center);
        Vector3 p3 = EnemyManager.Instance.GetControlPointPos((int)Control.Right);
        float d1 = Vector3.Distance(p1,this.transform.position);
        float d2 = Vector3.Distance(p2,this.transform.position);
        float d3 = Vector3.Distance(p3,this.transform.position);
        List<float> dList = new List<float>();
        dList.Clear();
        dList.Add(d1);
        dList.Add(d2);
        dList.Add(d3);
        
        //  ���ёւ�
        dList.Sort();

        if(dList[0] == d1)currentlNum = (int)Control.Left;
        if(dList[0] == d2)currentlNum = (int)Control.Center;
        if(dList[0] == d3)currentlNum = (int)Control.Right;

        //  ���X�g���N���A
        targetList.Clear();

        //  �ڕW�̔ԍ��𒊑I
        if(currentlNum ==(int)Control.Left)
        {
            targetList.Add((int)Control.Center);
            targetList.Add((int)Control.Right);
        }
        else if(currentlNum ==(int)Control.Center)
        {
            targetList.Add((int)Control.Left);
            targetList.Add((int)Control.Right);
        }
        else if(currentlNum ==(int)Control.Right)
        {
            targetList.Add((int)Control.Left);
            targetList.Add((int)Control.Center); 
        }

        //  �ڕW�ԍ��𒊑I
        targetNum = targetList[Random.Range(0, targetList.Count)];

        //  �ڕW���W���擾
        Vector3 targetPos = EnemyManager.Instance.GetControlPointPos(targetNum);

        //  ���ړ��J�n
        transform.DOLocalMoveX(targetPos.x, duration)
            .SetEase(Ease.Linear);

        //  �c�ړ��J�n
        transform.DOLocalMoveY(-2f, duration/2)
            .SetEase(Ease.Linear)
            .SetRelative(true);

        //  �ړ����ԑ҂�
        yield return new WaitForSeconds(duration/2);

        //  �c�ړ��J�n
        transform.DOLocalMoveY(2f, duration/2)
            .SetEase(Ease.Linear)
            .SetRelative(true);

        //  �ړ����ԑ҂�
        yield return new WaitForSeconds(duration/2);

        //  ���݂̔ԍ����X�V
        currentlNum = targetNum;

        //  ���̈ړ��܂ő҂�
        yield return new WaitForSeconds(interval);
    }

    //------------------------------------------------------------------
    //  �t�F�[�Y�̐؂�ւ����Ƀh�E�W���^�񒆂Ɉړ�����
    //------------------------------------------------------------------
    private IEnumerator Douji_MoveToCenter(float duration)
    {
        int targetNum = (int)Control.Center;        //  �ڕW�ʒu

        //  ���G���[�hON
        bSuperModeInterval = true;

        //  �ڕW���W���擾
        Vector3 targetPos = EnemyManager.Instance.GetControlPointPos(targetNum);

        //  ���ړ��J�n
        transform.DOLocalMoveX(targetPos.x, duration)
            .SetEase(Ease.Linear);

        //  �c�ړ��J�n
        transform.DOLocalMoveY(targetPos.y, duration)
            .SetEase(Ease.Linear);

        //  �ړ����ԑ҂�
        yield return new WaitForSeconds(duration);

        //  �A�C�e���h���b�v����
        DropItems drop = this.GetComponent<DropItems>();
        //  �m��h���b�v�ŃV���b�g�����𗎂Ƃ�
        if (drop) drop.DropPowerupItem(ePowerupItems.PowerUp);
    }

    //------------------------------------------------------------------
    //  �ʏ�e�������_���ɑI�����Č���
    //------------------------------------------------------------------
    private IEnumerator Shot()
    {
        int rand = Random.Range(0,100);

        //  �����m����臒l�ŌĂѕ�����
        if (rand <= 49f)
        {
            yield return StartCoroutine(WildlyShot());

            yield return StartCoroutine(SnipeShot());
        }
        else if (rand <= 99f)
        {
            yield return StartCoroutine(OriginalShot());

            yield return StartCoroutine(StraightShot());

            yield return StartCoroutine(StraightShot());
        }
    }

    //------------------------------------------------------------------
    //  �o���}�L�e(��)
    //------------------------------------------------------------------
    private IEnumerator WildlyShotSmall()
    {
        //  �ʏ�o���}�L�e�̃v���n�u���擾
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Wildly_Normal);

        float totalDegree = 180;        //  ���͈͂̑��p  
        int wayNum = 5;                 //  �e��way��(�K��3way�ȏ�̊�ɂ��邱��)
        float Degree = totalDegree / (wayNum-1);     //  �e�ꔭ���ɂ��炷�p�x         
        float speed = 3.0f;             //  �e��
        int chain = 5;                  //  �A�e��
        float chainInterval = 0.8f;     //  �A�e�̊Ԋu�i�b�j

        //  �G�̑O���x�N�g�����擾
        Vector3[] vector = new Vector3[wayNum];
        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < wayNum; i++)
            {
                Vector3 vector0 = Quaternion.Euler(0,0,Random.Range(-10,11)) * -transform.up;

                vector[i] = Quaternion.Euler(
                        0, 0, -Degree * ((wayNum-1)/2) + (i * Degree)
                    ) * vector0;
                vector[i].z = 0f;

                //�e�C���X�^���X���擾���A�����Ɣ��ˊp�x��^����
                GameObject Bullet_obj = 
                    (GameObject)Instantiate(bullet, transform.position, transform.rotation);
                EnemyBullet enemyBullet = Bullet_obj.GetComponent<EnemyBullet>();
                enemyBullet.SetSpeed(speed);
                enemyBullet.SetVelocity(vector[i]);
                enemyBullet.SetPower(enemyData.Attack);

                if(i == 0)
                {
                    //  ����SE�Đ�
                    SoundManager.Instance.PlaySFX(
                    (int)AudioChannel.ENEMY_SHOT,
                    (int)SFXList.SFX_ENEMY_SHOT);
                }
            }
            yield return new WaitForSeconds(chainInterval);
        }

        yield return null;
    }

    //------------------------------------------------------------------
    //  �o���}�L�e
    //------------------------------------------------------------------
    private IEnumerator WildlyShot()
    {
        //  �ʏ�o���}�L�e�̃v���n�u���擾
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Wildly_Big);

        float totalDegree = 360;        //  ���͈͂̑��p  
        int wayNum = 9;                 //  �e��way��(�K��3way�ȏ�̊�ɂ��邱��)
        float Degree = totalDegree / (wayNum-1);     //  �e�ꔭ���ɂ��炷�p�x         
        float speed = 7.0f;             //  �e��
        int chain = 10;                 //  �A�e��
        float chainInterval = 0.4f;     //  �A�e�̊Ԋu�i�b�j

        //  �G�̑O���x�N�g�����擾
        Vector3[] vector = new Vector3[wayNum];
        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < wayNum; i++)
            {
                /*�@�J�n���������_���ɂ��炷�@*/
                Vector3 vector0 = Quaternion.Euler(0,0,Random.Range(-90,91)) * -transform.up;
                //Vector3 vector0 = -transform.up;

                vector[i] = Quaternion.Euler(
                        0, 0, -Degree * ((wayNum-1)/2) + (i * Degree)
                    ) * vector0;
                vector[i].z = 0f;

                //�e�C���X�^���X���擾���A�����Ɣ��ˊp�x��^����
                GameObject Bullet_obj = 
                    (GameObject)Instantiate(bullet, transform.position, transform.rotation);
                EnemyBullet enemyBullet = Bullet_obj.GetComponent<EnemyBullet>();
                enemyBullet.SetSpeed(speed);
                enemyBullet.SetVelocity(vector[i]);
                enemyBullet.SetPower(enemyData.Attack);

                if(i == 0)
                {
                    //  ����SE�Đ�
                    SoundManager.Instance.PlaySFX(
                    (int)AudioChannel.ENEMY_SHOT,
                    (int)SFXList.SFX_ENEMY_SHOT);
                }
            }
            yield return new WaitForSeconds(chainInterval);
        }

        yield return null;
    }

    //------------------------------------------------------------------
    //  ���@�_���e
    //------------------------------------------------------------------
    private IEnumerator SnipeShot()
    {
        //  �ʏ펩�@�_���e�̃v���n�u���擾
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Snipe_Big);

        float Degree = 15;              //  ���炷�p�x
        int wayNum = 3;                 //  �e��way��
        float speed = 9.0f;            //  �e��
        int chain = 3;                  //  �A�e��
        float chainInterval = 1f;       //  �A�e�̊Ԋu�i�b�j



        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < wayNum; i++)
            {
                //  �G����v���C���[�ւ̃x�N�g�����擾
                Vector3 playerPos = GameManager.Instance.GetPlayer().transform.position;
                Vector3 vector0 = (playerPos - transform.position).normalized;
                Vector3[] vector = new Vector3[wayNum];

                vector[i] = Quaternion.Euler( 0, 0, -Degree + (i * Degree) ) * vector0;
                vector[i].z = 0f;

                //�e�C���X�^���X���擾���A�����Ɣ��ˊp�x��^����
                GameObject Bullet_obj = 
                    (GameObject)Instantiate(bullet, transform.position, transform.rotation);
                EnemyBullet enemyBullet = Bullet_obj.GetComponent<EnemyBullet>();
                enemyBullet.SetSpeed(speed);
                enemyBullet.SetVelocity(vector[i]);
                enemyBullet.SetPower(enemyData.Attack);

                if(i == 0)
                {
                    //  ����SE�Đ�
                    SoundManager.Instance.PlaySFX(
                    (int)AudioChannel.ENEMY_SHOT,
                    (int)SFXList.SFX_ENEMY_SHOT);
                }
            }
            yield return new WaitForSeconds(chainInterval);
        }

        yield return null;
    }

    //------------------------------------------------------------------
    //  �I���W�i���e�E�K�g�����O�V���b�g
    //------------------------------------------------------------------
    private IEnumerator OriginalShot()
    {
        //  �e�̃v���n�u���擾
        //GameObject bulletL = EnemyManager.Instance
        //    .GetBulletPrefab((int)BULLET_TYPE.Wildly_RotationL);

        //GameObject bulletR = EnemyManager.Instance
        //    .GetBulletPrefab((int)BULLET_TYPE.Wildly_RotationR);
        GameObject bulletL = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Wildly_Big);

        GameObject bulletR = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Wildly_Big);

        float totalDegree = 180;        //  ���͈͂̑��p  
        int wayNum = 1;                 //  �e��way��(�K��3way�ȏ�̊�ɂ��邱��)
        int chain = 5;                  //  �A�e��
        float Degree = totalDegree/chain;     //  �e�ꔭ���ɂ��炷�p�x         
        float speed = 7.0f;             //  �e��
        float chainInterval = 0.05f;    //  �A�e�̊Ԋu�i�b�j

        //  �G�̑O���x�N�g�����擾
        Vector3[] vector = new Vector3[wayNum];
        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < wayNum; i++)
            {
                Vector3 vector0 = Quaternion.Euler(0,0,90) * -transform.up;

                vector[i] = Quaternion.Euler(0,0,j * -Degree) * vector0;
                vector[i].z = 0f;

                //�e�C���X�^���X���擾���A�����Ɣ��ˊp�x��^����
                GameObject Bullet_obj = 
                    (GameObject)Instantiate(bulletL, transform.position, transform.rotation);
                EnemyBullet enemyBullet = Bullet_obj.GetComponent<EnemyBullet>();
                enemyBullet.SetSpeed(speed);
                enemyBullet.SetVelocity(vector[i]);
                enemyBullet.SetPower(enemyData.Attack);

                if(i == 0)
                {
                    //  ����SE�Đ�
                    SoundManager.Instance.PlaySFX(
                    (int)AudioChannel.ENEMY_SHOT,
                    (int)SFXList.SFX_ENEMY_SHOT);
                }
            }
            yield return new WaitForSeconds(chainInterval);
        }

        //  �P�b�Ԋu���󂯂�
        yield return new WaitForSeconds(1.0f);

        //  �G�̑O���x�N�g�����擾
        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < wayNum; i++)
            {
                Vector3 vector0 = Quaternion.Euler(0,0,-90) * -transform.up;

                vector[i] = Quaternion.Euler(0,0,j * Degree) * vector0;
                vector[i].z = 0f;

                //�e�C���X�^���X���擾���A�����Ɣ��ˊp�x��^����
                GameObject Bullet_obj = 
                    (GameObject)Instantiate(bulletL, transform.position, transform.rotation);
                EnemyBullet enemyBullet = Bullet_obj.GetComponent<EnemyBullet>();
                enemyBullet.SetSpeed(speed);
                enemyBullet.SetVelocity(vector[i]);
                enemyBullet.SetPower(enemyData.Attack);

                if(i == 0)
                {
                    //  ����SE�Đ�
                    SoundManager.Instance.PlaySFX(
                    (int)AudioChannel.ENEMY_SHOT,
                    (int)SFXList.SFX_ENEMY_SHOT);
                }
            }
            yield return new WaitForSeconds(chainInterval);
        }

        //  �G�̑O���x�N�g�����擾
        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < wayNum; i++)
            {
                Vector3 vector0 = Quaternion.Euler(0,0,-90) * -transform.up;

                vector[i] = Quaternion.Euler(0,0,j * Degree) * vector0;
                vector[i].z = 0f;

                //�e�C���X�^���X���擾���A�����Ɣ��ˊp�x��^����
                GameObject Bullet_obj = 
                    (GameObject)Instantiate(bulletL, transform.position, transform.rotation);
                EnemyBullet enemyBullet = Bullet_obj.GetComponent<EnemyBullet>();
                enemyBullet.SetSpeed(speed*2);
                enemyBullet.SetVelocity(vector[i]);
                enemyBullet.SetPower(enemyData.Attack);

                if(i == 0)
                {
                    //  ����SE�Đ�
                    SoundManager.Instance.PlaySFX(
                    (int)AudioChannel.ENEMY_SHOT,
                    (int)SFXList.SFX_ENEMY_SHOT);
                }
            }
            yield return new WaitForSeconds(chainInterval);

            for (int i = 0; i < wayNum; i++)
            {
                Vector3 vector0 = Quaternion.Euler(0,0,90) * -transform.up;

                vector[i] = Quaternion.Euler(0,0,j * -Degree) * vector0;
                vector[i].z = 0f;

                //�e�C���X�^���X���擾���A�����Ɣ��ˊp�x��^����
                GameObject Bullet_obj = 
                    (GameObject)Instantiate(bulletL, transform.position, transform.rotation);
                EnemyBullet enemyBullet = Bullet_obj.GetComponent<EnemyBullet>();
                enemyBullet.SetSpeed(speed*2);
                enemyBullet.SetVelocity(vector[i]);
                enemyBullet.SetPower(enemyData.Attack);

                if(i == 0)
                {
                    //  ����SE�Đ�
                    SoundManager.Instance.PlaySFX(
                    (int)AudioChannel.ENEMY_SHOT,
                    (int)SFXList.SFX_ENEMY_SHOT);
                }
            }
            yield return new WaitForSeconds(chainInterval);
        }

        yield return null;
    }

    //------------------------------------------------------------------
    //  �������낵�V���b�g
    //------------------------------------------------------------------
    private IEnumerator StraightShot()
    {
        int currentlNum = (int)Control.Left;       //  ���݈ʒu
        List<int> targetList = new List<int>();    //  �ڕW�ʒu��⃊�X�g
        int targetNum = (int)Control.Right;        //  �ڕW�ʒu

        Vector3 vec = Vector3.down;     //  �e�̃x�N�g��
        float duration = 2.0f;
        int bulletNum = 3;
        float interval = 2.0f;

        //  �ʏ�o���}�L�e�̃v���n�u���擾
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Wildly_Big);

        //  ���݈ʒu�����߂�i��ԋ߂��ʒu�Ƃ���j
        Vector3 p1 = EnemyManager.Instance.GetControlPointPos((int)Control.Left);
        Vector3 p2 = EnemyManager.Instance.GetControlPointPos((int)Control.Right);
        float d1 = Vector3.Distance(p1,this.transform.position);
        float d2 = Vector3.Distance(p2,this.transform.position);
        List<float> dList = new List<float>();
        dList.Clear();
        dList.Add(d1);
        dList.Add(d2);
        
        //  ���ёւ�
        dList.Sort();

        if(dList[0] == d1)currentlNum = (int)Control.Left;
        if(dList[0] == d2)currentlNum = (int)Control.Right;

        //  ���X�g���N���A
        targetList.Clear();

        //  �ڕW�̔ԍ���ݒ�
        if(currentlNum ==(int)Control.Left)
        {
            targetList.Add((int)Control.Right);
        }
        else if(currentlNum ==(int)Control.Right)
        {
            targetList.Add((int)Control.Left);
        }

        //  �ڕW�ԍ���ݒ�
        targetNum = targetList[Random.Range(0, targetList.Count)];

        //  �ڕW���W���擾
        Vector3 targetPos = EnemyManager.Instance.GetControlPointPos(targetNum);

        //  ���ړ��J�n
        transform.DOLocalMoveX(targetPos.x, duration)
            .SetEase(Ease.Linear);

        //  �e�𐶐�
        GameObject bullet1 = Instantiate(bullet,transform.position,Quaternion.identity);
        bullet1.transform.DOMoveY(-15,duration)
            .SetRelative(true)
            .SetEase(Ease.InOutQuint)
            .OnComplete(()=>Destroy(bullet1));

        yield return new WaitForSeconds(duration/bulletNum);

        GameObject bullet2 = Instantiate(bullet,transform.position,Quaternion.identity);
        bullet2.transform.DOMoveY(-15,duration)
            .SetRelative(true)
            .SetEase(Ease.InOutQuint)
            .OnComplete(()=>Destroy(bullet2));

        yield return new WaitForSeconds(duration/bulletNum);

        GameObject bullet3 = Instantiate(bullet,transform.position,Quaternion.identity);
        bullet3.transform.DOMoveY(-15,duration)
            .SetRelative(true)
            .SetEase(Ease.InOutQuint)
            .OnComplete(()=>Destroy(bullet3));

        yield return new WaitForSeconds(duration/bulletNum);

        //  ���݂̔ԍ����X�V
        currentlNum = targetNum;

        //  ���̈ړ��܂ő҂�
        yield return new WaitForSeconds(interval);
    }

    //------------------------------------------------------------------
    //  Phase2:�q�S�̌Q��̐i�H��\������
    //------------------------------------------------------------------
    private IEnumerator DisplayDirection(DoujiPhase2Bullet.KooniDirection direction, Vector2 pos)
    {
        GameObject line = null;

        //  SE���Đ�
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX_SYSTEM,
            (int)SFXList.SFX_DOUJI_WARNING);

        //  �\���i�H�̃X���C�_�[�𐶐�
        GameObject canvas = EnemyManager.Instance.GetDangerLineCanvas();
        line = Instantiate(dangerLineObject[(int)direction]);
        line.transform.SetParent(canvas.transform);
        line.GetComponent<RectTransform>().anchoredPosition = pos;

        yield return new WaitForSeconds(1);

        if(line.gameObject)Destroy(line.gameObject);
    }

    //------------------------------------------------------------------
    //  Phase2:�A���t�@�A�j���[�V����
    //------------------------------------------------------------------
    private IEnumerator AlphaAnimation(float start, float end)
    {
        float duration = 0.4f;

        //  �A���t�@��0.0�ɐݒ�
        var  fadeImage = warningObject.GetComponent<Image>();
        fadeImage.enabled = true;
        var c = fadeImage.color;
        c.a = start;    // �����l
        fadeImage.color = c;

        //  �A�j���[�V�����J�n
        DOTween.ToAlpha(
	        ()=> fadeImage.color,
	        color => fadeImage.color = color,
	        end,         // �ڕW�l
	        duration    // ���v����
        );

        yield return new WaitForSeconds(duration);
    }

    //------------------------------------------------------------------
    //  Phase2:�x�����o��
    //------------------------------------------------------------------
    private IEnumerator Warning()
    {
        float duration = 3.0f;

        if(bWarningFirst)yield break;

        //  SE���Đ�
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX_SYSTEM,
            (int)SFXList.SFX_DOUJI_WARNING);

        //  WARNING��L����
        warningObject.SetActive(true);

        ////  �A���t�@�A�j���[�V����
        //yield return StartCoroutine(AlphaAnimation(0f,0.8f));
        //yield return StartCoroutine(AlphaAnimation(0.8f,0f));
        //yield return StartCoroutine(AlphaAnimation(0f,0.8f));
        //yield return StartCoroutine(AlphaAnimation(0.8f,0f));

        //  ����ł͂Ȃ��Ȃ����̂�TRUE
        bWarningFirst = true;

        //  ���o���R�b�Ȃ̂ł��̕��҂�
        yield return new WaitForSeconds(duration);

        //  WARNING�𖳌���
        warningObject.SetActive(false);
    }

    //------------------------------------------------------------------
    //  Phase2:�����_���ȃX�|�i�[����q�S���ˌ����Ă���
    //------------------------------------------------------------------
    private IEnumerator KooniParty()
    {
        int fourDirection = -1;

        while(true)
        {
            //  �܂��͂S�����Œ��I
            fourDirection  = Random.Range(0,4);

            //  �ԍ����g�p�ς݂ł͂Ȃ�������
            if(kooniNum[fourDirection] == -1)
            {
                //  ����̔ԍ����L�^
                kooniNum[fourDirection] = fourDirection;
                break;
            }
        }

        //  �����m����臒l�ŌĂѕ�����
        if (fourDirection == 0)         //  ������Ƃ���
        {
            //  �M�~�b�N�e�̃v���n�u���擾
            /*�v���n�u�̎�ނ�������\��*/
            GameObject bullet = EnemyManager.Instance
                .GetBulletPrefab((int)BULLET_TYPE.Douji_Gimmick_Top);

            //  �R�ӏ��Œ��I
            int rand = Random.Range(0,3);
            Vector3 pos = default;
            if(rand == 0)       //  ��
            {
                pos = EnemyManager.Instance.GetSpawnerPos(0);

                //  �q�S�̌Q��̐i�H��\������
                yield return StartCoroutine(
                    DisplayDirection(
                        DoujiPhase2Bullet.KooniDirection.TOP,
                        new Vector2(-375,300)
                        ));
            }
            else if(rand == 1)  //  ��
            {
                pos = EnemyManager.Instance.GetSpawnerPos(1);

                //  �q�S�̌Q��̐i�H��\������
                yield return StartCoroutine(
                    DisplayDirection(
                        DoujiPhase2Bullet.KooniDirection.TOP,
                        new Vector2(-60,300)
                        ));
            }
            else if(rand == 2)  //  �E
            {
                pos = EnemyManager.Instance.GetSpawnerPos(2);

                //  �q�S�̌Q��̐i�H��\������
                yield return StartCoroutine(
                    DisplayDirection(
                        DoujiPhase2Bullet.KooniDirection.TOP,
                        new Vector2(250,300)
                        ));
            }

            //  0.5�b�̃f�B���C��������
            yield return new WaitForSeconds(0.5f);

            //  �q�S�𐶐�
            GameObject obj = Instantiate(bullet,pos,Quaternion.identity);
            DoujiPhase2Bullet bulletComp =  obj.GetComponent<DoujiPhase2Bullet>();
            bulletComp.SetPower(enemyData.Attack);
            bulletComp.SetDirection(DoujiPhase2Bullet.KooniDirection.TOP);

            //  �q�S���ˌ�����
            yield return StartCoroutine(bulletComp.KooniRush());

            //  ���Z�b�g
            kooniNum[fourDirection] = -1;
        }
        else if (fourDirection == 1)    //  �������Ƃ���
        {
            //  �M�~�b�N�e�̃v���n�u���擾
            GameObject bullet = EnemyManager.Instance
                .GetBulletPrefab((int)BULLET_TYPE.Douji_Gimmick_Bottom);
            //  �R�ӏ��Œ��I
            int rand = Random.Range(0,3);
            Vector3 pos = default;
            if(rand == 0)       //  ��
            {
                pos = EnemyManager.Instance.GetSpawnerPos(8);

                //  �q�S�̌Q��̐i�H��\������
                yield return StartCoroutine(
                    DisplayDirection(
                        DoujiPhase2Bullet.KooniDirection.BOTTOM,
                        new Vector2(-375,-300)
                        ));
            }
            else if(rand == 1)  //  ��
            {
                pos = EnemyManager.Instance.GetSpawnerPos(7);


                //  �q�S�̌Q��̐i�H��\������
                yield return StartCoroutine(
                    DisplayDirection(
                        DoujiPhase2Bullet.KooniDirection.BOTTOM,
                        new Vector2(-60,-300)
                        ));
            }
            else if(rand == 2)  //  �E
            {
                pos = EnemyManager.Instance.GetSpawnerPos(6);

                //  �q�S�̌Q��̐i�H��\������
                yield return StartCoroutine(
                    DisplayDirection(
                        DoujiPhase2Bullet.KooniDirection.BOTTOM,
                        new Vector2(250,-300)
                        ));
            }

            //  0.5�b�̃f�B���C��������
            yield return new WaitForSeconds(0.5f);

            //  �q�S�𐶐�
            GameObject obj = Instantiate(bullet,pos,Quaternion.identity);
            DoujiPhase2Bullet bulletComp =  obj.GetComponent<DoujiPhase2Bullet>();
            bulletComp.SetPower(enemyData.Attack);
            bulletComp.SetDirection(DoujiPhase2Bullet.KooniDirection.BOTTOM);

            //  �q�S���ˌ�����
            yield return StartCoroutine(bulletComp.KooniRush());

            //  ���Z�b�g
            kooniNum[fourDirection] = -1;
        }
        else if (fourDirection == 2)    //  �������Ƃ���
        {
            //  �M�~�b�N�e�̃v���n�u���擾
            GameObject bullet = EnemyManager.Instance
                .GetBulletPrefab((int)BULLET_TYPE.Douji_Gimmick_Left);
            //  �R�ӏ��Œ��I
            int rand = Random.Range(0,3);
            Vector3 pos = default;
            if(rand == 0)       //  ��
            {
                pos = EnemyManager.Instance.GetSpawnerPos(11);

                //  ���W���Z�b�g
                //pos2 = new Vector2(-560,190);

                //  �q�S�̌Q��̐i�H��\������
                yield return StartCoroutine(
                    DisplayDirection(
                        DoujiPhase2Bullet.KooniDirection.LEFT,
                        new Vector2(-430,180)
                        ));
            }
            else if(rand == 1)  //  ��
            {
                pos = EnemyManager.Instance.GetSpawnerPos(10);

                //  ���W���Z�b�g
                //pos2 = new Vector2(-560,-18);

                //  �q�S�̌Q��̐i�H��\������
                yield return StartCoroutine(
                    DisplayDirection(
                        DoujiPhase2Bullet.KooniDirection.LEFT,
                        new Vector2(-430,-15)
                        ));
            }
            else if(rand == 2)  //  ��
            {
                pos = EnemyManager.Instance.GetSpawnerPos(9);

                //  ���W���Z�b�g
                //pos2 = new Vector2(-560,-215);

                //  �q�S�̌Q��̐i�H��\������
                yield return StartCoroutine(
                    DisplayDirection(
                        DoujiPhase2Bullet.KooniDirection.LEFT,
                        new Vector2(-430,-220)
                        ));
            }

            //  0.5�b�̃f�B���C��������
            yield return new WaitForSeconds(0.5f);

            //  �q�S�𐶐�
            GameObject obj = Instantiate(bullet,pos,Quaternion.identity);
            DoujiPhase2Bullet bulletComp =  obj.GetComponent<DoujiPhase2Bullet>();
            bulletComp.SetPower(enemyData.Attack);
            bulletComp.SetDirection(DoujiPhase2Bullet.KooniDirection.LEFT);

            //  �q�S���ˌ�����
            yield return StartCoroutine(bulletComp.KooniRush());

            //  ���Z�b�g
            kooniNum[fourDirection] = -1;
        }
        else if (fourDirection == 3)    //  �E�����Ƃ���
        {
            //  �M�~�b�N�e�̃v���n�u���擾
            GameObject bullet = EnemyManager.Instance
                .GetBulletPrefab((int)BULLET_TYPE.Douji_Gimmick_Right);
            //  �R�ӏ��Œ��I
            int rand = Random.Range(0,3);
            Vector3 pos = default;
            if(rand == 0)       //  ��
            {
                pos = EnemyManager.Instance.GetSpawnerPos(3);

                //  ���W���Z�b�g
                //pos2 = new Vector2(440,190);

                //  �q�S�̌Q��̐i�H��\������
                yield return StartCoroutine(
                    DisplayDirection(
                        DoujiPhase2Bullet.KooniDirection.RIGHT,
                        new Vector2(300,185)
                        ));
            }
            else if(rand == 1)  //  ��
            {
                pos = EnemyManager.Instance.GetSpawnerPos(4);

                //  ���W���Z�b�g
                //pos2 = new Vector2(440,-18);

                //  �q�S�̌Q��̐i�H��\������
                yield return StartCoroutine(
                    DisplayDirection(
                        DoujiPhase2Bullet.KooniDirection.RIGHT,
                        new Vector2(300,-15)
                        ));
            }
            else if(rand == 2)  //  ��
            {
                pos = EnemyManager.Instance.GetSpawnerPos(5);

                //  ���W���Z�b�g
                //pos2 = new Vector2(440,-215);

                //  �q�S�̌Q��̐i�H��\������
                yield return StartCoroutine(
                    DisplayDirection(
                        DoujiPhase2Bullet.KooniDirection.RIGHT,
                        new Vector2(300,-215)
                        ));
            }

            //  0.5�b�̃f�B���C��������
            yield return new WaitForSeconds(0.5f);

            //  �q�S�𐶐�
            GameObject obj = Instantiate(bullet,pos,Quaternion.identity);
            DoujiPhase2Bullet bulletComp =  obj.GetComponent<DoujiPhase2Bullet>();
            bulletComp.SetPower(enemyData.Attack);
            bulletComp.SetDirection(DoujiPhase2Bullet.KooniDirection.RIGHT);

            //  �q�S���ˌ�����
            yield return StartCoroutine(bulletComp.KooniRush());

            //  ���Z�b�g
            kooniNum[fourDirection] = -1;
        }

        yield return null;
    }

   //-------------------------------------------------------------------
    //  �h�E�W�̔����e��������
    //------------------------------------------------------------------
    private IEnumerator GenerateBerserkBullet(float duration)
    {
        //  �����e�̃v���n�u���擾
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Douji_Berserk_Bullet);

        //  �e�𐶐�
        GameObject obj = Instantiate(bullet,transform.position,Quaternion.identity);
        DoujiPhase3Bullet enemyBullet = obj.GetComponent<DoujiPhase3Bullet>();

        enemyBullet.SetPower(enemyData.Attack);

        //  SE���Đ�
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.ENEMY_SHOT,
            (int)SFXList.SFX_DOUJI_SHOT1);

        yield return new WaitForSeconds(duration);
    }

    //------------------------------------------------------------------
    //  �h�E�W�̔����e
    //------------------------------------------------------------------
    private IEnumerator Douji_LoopMoveBerserk(int bulletNum,float duration,float interval)
    {
        int currentlNum = (int)Control.Left;       //  ���݈ʒu
        List<int> targetList = new List<int>();    //  �ڕW�ʒu��⃊�X�g
        int targetNum = (int)Control.Right;        //  �ڕW�ʒu

        Vector3 vec = Vector3.down;     //  �e�̃x�N�g��

        //  ���݈ʒu�����߂�i��ԋ߂��ʒu�Ƃ���j
        Vector3 p1 = EnemyManager.Instance.GetControlPointPos((int)Control.Left);
        Vector3 p2 = EnemyManager.Instance.GetControlPointPos((int)Control.Right);
        float d1 = Vector3.Distance(p1,this.transform.position);
        float d2 = Vector3.Distance(p2,this.transform.position);
        List<float> dList = new List<float>();
        dList.Clear();
        dList.Add(d1);
        dList.Add(d2);
        
        //  ���ёւ�
        dList.Sort();

        if(dList[0] == d1)currentlNum = (int)Control.Left;
        if(dList[0] == d2)currentlNum = (int)Control.Right;

        //  ���X�g���N���A
        targetList.Clear();

        //  �ڕW�̔ԍ���ݒ�
        if(currentlNum ==(int)Control.Left)
        {
            targetList.Add((int)Control.Right);
        }
        else if(currentlNum ==(int)Control.Right)
        {
            targetList.Add((int)Control.Left);
        }

        //  �ڕW�ԍ���ݒ�
        targetNum = targetList[Random.Range(0, targetList.Count)];

        //  �ڕW���W���擾
        Vector3 targetPos = EnemyManager.Instance.GetControlPointPos(targetNum);

        //  ���ړ��J�n
        transform.DOLocalMoveX(targetPos.x, duration)
            .SetEase(Ease.Linear);

        //  �����e�̃v���n�u���擾
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Douji_Berserk_Bullet);

        //  �e�𐶐�
        yield return StartCoroutine(GenerateBerserkBullet(duration/bulletNum));

        yield return StartCoroutine(GenerateBerserkBullet(duration/bulletNum));

        yield return StartCoroutine(GenerateBerserkBullet(duration/bulletNum));

        //  ���݂̔ԍ����X�V
        currentlNum = targetNum;

        //  ���̈ړ��܂ő҂�
        yield return new WaitForSeconds(interval);
    }

    //------------------------------------------------------------------
    //  �h�E�W�̔����e��
    //------------------------------------------------------------------
    private IEnumerator Douji_BerserkBarrage()
    {
        //  �ʏ�o���}�L�e�̃v���n�u���擾
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Wildly_Big);

        float totalDegree = 180;        //  ���͈͂̑��p  
        int wayNum = 9;                 //  �e��way��(�K��3way�ȏ�̊�ɂ��邱��)
        float Degree = totalDegree / (wayNum-1);     //  �e�ꔭ���ɂ��炷�p�x         
        float speed = 8.0f;             //  �e��
        int chain = 5;                  //  �A�e��
        float chainInterval = 0.3f;     //  �A�e�̊Ԋu�i�b�j

        //  �G�̑O���x�N�g�����擾
        Vector3[] vector = new Vector3[wayNum];
        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < wayNum; i++)
            {
                Vector3 vector0 = Quaternion.Euler(0,0,Random.Range(-10,11)) * -transform.up;

                vector[i] = Quaternion.Euler(
                        0, 0, -Degree * ((wayNum-1)/2) + (i * Degree)
                    ) * vector0;
                vector[i].z = 0f;

                //�e�C���X�^���X���擾���A�����Ɣ��ˊp�x��^����
                GameObject Bullet_obj = 
                    (GameObject)Instantiate(bullet, transform.position, transform.rotation);
                EnemyBullet enemyBullet = Bullet_obj.GetComponent<EnemyBullet>();
                enemyBullet.SetSpeed(speed);
                enemyBullet.SetVelocity(vector[i]);
                enemyBullet.SetPower(enemyData.Attack);

                if(i == 0)
                {
                    //  ����SE�Đ�
                    SoundManager.Instance.PlaySFX(
                    (int)AudioChannel.ENEMY_SHOT,
                    (int)SFXList.SFX_ENEMY_SHOT);
                }
            }
            yield return new WaitForSeconds(chainInterval);
        }

        yield return null;

        yield return null;
    }

    //------------------------------------------------------------------
    //  ���@�_�������K�g�����O�V���b�g
    //------------------------------------------------------------------
    private IEnumerator Douji_BerserkGatling()
    {
        //  �e�̃v���n�u���擾
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Snipe_Big);

        int wayNum = 3;                 //  �e��way��
        float Degree = 20;              //  ���炷�p�x
        int chain = 2;                  //  �A�e��         
        float speed = 8.0f;             //  �e��
        float chainInterval = 0.5f;     //  �A�e�̊Ԋu�i�b�j

        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < wayNum; i++)
            {
                //  �G����v���C���[�ւ̃x�N�g�����擾
                Vector3 playerPos = GameManager.Instance.GetPlayer().transform.position;
                Vector3 vector0 = (playerPos - transform.position).normalized;
                Vector3[] vector = new Vector3[wayNum];

                vector[i] = Quaternion.Euler( 0, 0, -Degree + i * Degree ) * vector0;
                vector[i].z = 0f;

                //�e�C���X�^���X���擾���A�����Ɣ��ˊp�x��^����
                GameObject Bullet_obj = 
                    (GameObject)Instantiate(bullet, transform.position, transform.rotation);
                EnemyBullet enemyBullet = Bullet_obj.GetComponent<EnemyBullet>();
                enemyBullet.SetSpeed(speed);
                enemyBullet.SetVelocity(vector[i]);
                enemyBullet.SetPower(enemyData.Attack);

                if(i == 0)
                {
                    //  ����SE�Đ�
                    SoundManager.Instance.PlaySFX(
                    (int)AudioChannel.ENEMY_SHOT,
                    (int)SFXList.SFX_ENEMY_SHOT);
                }
            }
            yield return new WaitForSeconds(chainInterval);
        }

        //  3�b�҂�
        yield return new WaitForSeconds(1.0f);

        yield return null;
    }
}
