using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

//--------------------------------------------------------------
//
//  �{�X�E�c�N���̃N���X
//
//--------------------------------------------------------------
public class BossTsukumo : MonoBehaviour
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
    private int[] buelletNum = new int[(int)TsukumoPhase2Bullet.Direction.MAX];

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
        dangerLineObject = new GameObject[(int)TsukumoPhase2Bullet.Direction.MAX];
        dangerLineObject[(int)TsukumoPhase2Bullet.Direction.TOP] =
            EnemyManager.Instance.GetBulletPrefab((int)BULLET_TYPE.Douji_DangerLine_Top);
        dangerLineObject[(int)TsukumoPhase2Bullet.Direction.BOTTOM] =
            EnemyManager.Instance.GetBulletPrefab((int)BULLET_TYPE.Douji_DangerLine_Bottom);
        dangerLineObject[(int)TsukumoPhase2Bullet.Direction.LEFT] =
            EnemyManager.Instance.GetBulletPrefab((int)BULLET_TYPE.Douji_DangerLine_Left);
        dangerLineObject[(int)TsukumoPhase2Bullet.Direction.RIGHT] =
            EnemyManager.Instance.GetBulletPrefab((int)BULLET_TYPE.Douji_DangerLine_Right);

        //  �M�~�b�N�e�̎g�p�ςݔԍ���������
        for(int i=0;i<(int)TsukumoPhase2Bullet.Direction.MAX;i++)
        {
            buelletNum[i] = -1;
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
                hp = enemyData.Hp*phase1_end;
                bStopPhase1 = true;
            }
        }
        if(hp <= enemyData.Hp*phase2_end)
        {
            if(!bStopPhase2)
            {
                hp = enemyData.Hp*phase2_end;
                bStopPhase2 = true;
            }
        }
        
        //  �X���C�_�[���X�V
        hpSlider.value = hp / enemyData.Hp;
    }

    //  �G�̃f�[�^��ݒ� 
    public void SetBossData(EnemySetting es, ePowerupItems item)
    {
        string boss_id = "Tsukumo";

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
    //  �c�N���̈ړ��p�^�[��
    //
    //******************************************************************

    //------------------------------------------------------------------
    //  ���b�҂��Ă���c�N���̍s���J�n
    //------------------------------------------------------------------
    private IEnumerator WaitDoujiAction(float duration)
    {
        yield return new WaitForSeconds(duration);

        //  �s���J�n
        StartCoroutine(StartDoujiAction());
    }

    //------------------------------------------------------------------
    //  �c�N���̍s���Ǘ��֐�
    //------------------------------------------------------------------
    private IEnumerator StartDoujiAction()
    {
        Debug.Log("***�c�N���e���t�F�[�Y�J�n�I***");

        //  �t�F�[�Y�P�J�n
        phase1_Coroutine = StartCoroutine(Tsukumo_Phase1());

        //  �t���O��TRUE�ɂȂ�܂ő҂�
        yield return new WaitUntil(()=>bStopPhase1 == true);

        //  �t���O�ŃR���[�`����~
        StopCoroutine(phase1_Coroutine);

        //  �t�F�[�Y�ύX
        yield return StartCoroutine(Tsukumo_PhaseChange());

        //  �t�F�[�Y�Q�J�n
        phase2_Coroutine = StartCoroutine(Tsukumo_Phase2());

        //  �t���O��TRUE�ɂȂ�܂ő҂�
        yield return new WaitUntil(()=>bStopPhase2 == true);

        //  �t���O�ŃR���[�`����~
        StopCoroutine(phase2_Coroutine);

        //  �t�F�[�Y�ύX
        yield return StartCoroutine(Tsukumo_PhaseChange());

        //  �t�F�[�Y�R�J�n
        StartCoroutine(Douji_Phase3());
    }

    //------------------------------------------------------------------
    //  �c�N���̃t�F�[�Y�`�F���W���̍s��
    //------------------------------------------------------------------
    private IEnumerator Tsukumo_PhaseChange()
    {
        //  �ړ��ɂ����鎞��(�b)
        float duration = 1.5f;
        //  �ړ���ɑҋ@���鎞��(�b)
        float interval = 5.0f;

        //  �^�񒆂Ɉړ�����
        yield return StartCoroutine(Tsukumo_MoveToCenter(duration));

        //  ���̃t�F�[�Y�܂ő҂�
        yield return new WaitForSeconds(interval);

        //  ���G���[�hOFF
        bSuperModeInterval = false;
    }

    //------------------------------------------------------------------
    //  �c�N����Phase1
    //------------------------------------------------------------------
    private IEnumerator Tsukumo_Phase1()
    {
        Debug.Log("�t�F�[�Y�P�J�n");

        //  �t�F�[�Y�P
        while (!bStopPhase1)
        {
            yield return StartCoroutine(Tsukumo_LoopMove(1.5f, 0.5f));

            yield return StartCoroutine(Shot());


            //yield return StartCoroutine(Tsukumo_LoopMove(1.0f, 1.0f));
            //yield return StartCoroutine(Warning());
            //StartCoroutine(TatamiSand());
            //yield return StartCoroutine(TatamiSand());


            //yield return StartCoroutine(Douji_BerserkBarrage());

            //yield return StartCoroutine(Douji_LoopMoveBerserk(3, 0.6f, 1.0f));

            //yield return StartCoroutine(Douji_BerserkGatling());

            //yield return StartCoroutine(Douji_LoopMoveBerserk(3, 0.6f, 1.0f));

            //yield return StartCoroutine(Douji_BerserkGatling());
        }
    }

    //------------------------------------------------------------------
    //  �c�N����Phase2
    //------------------------------------------------------------------
    private IEnumerator Tsukumo_Phase2()
    {
        Debug.Log("�t�F�[�Y�Q�ֈڍs");

        //  �t�F�[�Y�Q
        while (!bStopPhase2)
        {
            StartCoroutine(WildlyShotSmall());

            //  Warning!(����̂�)
            yield return StartCoroutine(Warning());

            StartCoroutine(TatamiSand());
            //StartCoroutine(TatamiSand());
            //StartCoroutine(TatamiSand());

            yield return StartCoroutine(TatamiSand());

            yield return StartCoroutine(Tsukumo_LoopMove(1.0f,1.0f));
        }
    }

    //------------------------------------------------------------------
    //  �c�N����Phase3
    //------------------------------------------------------------------
    private IEnumerator Douji_Phase3()
    {
        Debug.Log("�t�F�[�Y�R�ֈڍs");

        //  �t�F�[�Y�R
        while (true)
        {
            yield return StartCoroutine(Tsukumo_LoopMove(1.5f, 0.5f));

            yield return StartCoroutine(Shot());

            //yield return StartCoroutine(Douji_LoopMoveBerserk(3, 0.6f, 1.0f));

            //yield return StartCoroutine(Douji_BerserkBarrage());
            //yield return StartCoroutine(Douji_BerserkGatling());

            //yield return StartCoroutine(Douji_LoopMoveBerserk(3, 0.6f, 1.0f));

            //yield return StartCoroutine(Douji_BerserkBarrage());
            //yield return StartCoroutine(Douji_BerserkGatling());
        }
    }

    //------------------------------------------------------------------
    //  �c�N���̈ړ�
    //------------------------------------------------------------------
    private IEnumerator Tsukumo_LoopMove(float duration,float interval)
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
    //  �t�F�[�Y�̐؂�ւ����Ƀc�N�����^�񒆂Ɉړ�����
    //------------------------------------------------------------------
    private IEnumerator Tsukumo_MoveToCenter(float duration)
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
        yield return StartCoroutine(WildlyShot());

        yield return StartCoroutine(SnipeShot());

        yield return StartCoroutine(OriginalShot());
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

        float totalDegree = 180;        //  ���͈͂̑��p  
        int wayNum = 15;                //  �e��way��(�K��3way�ȏ�̊�ɂ��邱��)
        float Degree = totalDegree / (wayNum-1);     //  �e�ꔭ���ɂ��炷�p�x         
        float speed = 7.0f;             //  �e��
        float chainInterval = 0.03f;    //  �A�e�̊Ԋu�i�b�j
        float AttackInterval = 0.5f;    //  �e�����̊Ԋu�i�b�j
        Vector3[] vector = new Vector3[wayNum];

        //-----------------------------------------------
        //  �E���獶�փo���}�L
        //-----------------------------------------------
        for (int i = 0; i < wayNum; i++)
        {
            Vector3 vector0 = Quaternion.Euler(0,0,90) * -transform.up;

            vector[i] = Quaternion.Euler(0, 0, (i * -Degree)) * vector0;
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

            yield return new WaitForSeconds(chainInterval);
        }

        //  �U���Ԋu���J����
        yield return new WaitForSeconds(AttackInterval);

        //-----------------------------------------------
        //  ������E�փo���}�L
        //-----------------------------------------------
        for (int i = 0; i < wayNum; i++)
        {
            Vector3 vector0 = Quaternion.Euler(0,0,-90) * -transform.up;

            vector[i] = Quaternion.Euler(0, 0, (i * Degree)) * vector0;
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

            yield return new WaitForSeconds(chainInterval);
        }

        //  �U���Ԋu���J����
        yield return new WaitForSeconds(AttackInterval);

        //-----------------------------------------------
        //  ���E����o���}�L
        //-----------------------------------------------
        for (int i = 0; i < wayNum; i++)
        {
            Vector3 vectorR = Quaternion.Euler(0,0,90) * -transform.up;
            Vector3 vectorL = Quaternion.Euler(0,0,-90) * -transform.up;

            //�e�C���X�^���X���擾���A�����Ɣ��ˊp�x��^����
            vector[i] = Quaternion.Euler(0, 0, (i * -Degree)) * vectorR;
            vector[i].z = 0f;

            GameObject Bullet_objR = 
                (GameObject)Instantiate(bullet, transform.position, transform.rotation);
            EnemyBullet enemyBulletR = Bullet_objR.GetComponent<EnemyBullet>();
            enemyBulletR.SetSpeed(speed);
            enemyBulletR.SetVelocity(vector[i]);
            enemyBulletR.SetPower(enemyData.Attack);

            vector[i] = Quaternion.Euler(0, 0, (i * Degree)) * vectorL;
            vector[i].z = 0f;

            GameObject Bullet_objL = 
                (GameObject)Instantiate(bullet, transform.position, transform.rotation);
            EnemyBullet enemyBulletL = Bullet_objL.GetComponent<EnemyBullet>();
            enemyBulletL.SetSpeed(speed);
            enemyBulletL.SetVelocity(vector[i]);
            enemyBulletL.SetPower(enemyData.Attack);

            if(i == 0)
            {
                //  ����SE�Đ�
                SoundManager.Instance.PlaySFX(
                (int)AudioChannel.ENEMY_SHOT,
                (int)SFXList.SFX_ENEMY_SHOT);
            }

            yield return new WaitForSeconds(chainInterval);
        }

        //  �U���Ԋu���J����
        yield return new WaitForSeconds(AttackInterval);
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
    //  �I���W�i���e�E�z�[�~���O�V���b�g
    //------------------------------------------------------------------
    private IEnumerator OriginalShot()
    {
        //  �ʏ펩�@�_���e�̃v���n�u���擾
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Wildly_Big);

        //  �����_���ȃR���g���[���|�C���g�Ɍ������Ēe�����ł����A
        //  ��苗���܂ŋ߂Â��Ƃ��̎������]���n�߂�B
        //  ��莞�ԉ�]�����e�̓v���C���[�Ɍ������Ĕ��ł����B

        int wayNum = 5;                 //  ��x�Ɍ��e��
        float speed = 7.0f;             //  �e��
        int chain = 1;                  //  �A�e��
        float chainInterval = 2f;       //  �A�e�̊Ԋu�i�b�j
        float Interval = 3f;            //  ���̍s���܂ł̊Ԋu�i�b�j

        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < wayNum; i++)
            {
                //  �e�C���X�^���X���擾���A�����Ɣ��ˊp�x��^����
                GameObject Bullet_obj =
                    (GameObject)Instantiate(bullet, transform.position, transform.rotation);

                //  �e�Ƀf�t�H���g��EnemyBullet�R���|�[�l���g������̂ł�����폜����
                Destroy(Bullet_obj.GetComponent<EnemyBullet>());

                //  �����TsukumoHomingBullet�R���|�[�l���g��ǉ�����
                Bullet_obj.AddComponent<TsukumoHomingBullet>();

                //  �K�v�ȏ����Z�b�g����
                TsukumoHomingBullet enemyBullet = Bullet_obj.GetComponent<TsukumoHomingBullet>();
                enemyBullet.SetSpeed(speed);
                enemyBullet.SetPower(enemyData.Attack);

                if (i == 0)
                {
                    //  ����SE�Đ�
                    SoundManager.Instance.PlaySFX(
                    (int)AudioChannel.ENEMY_SHOT,
                    (int)SFXList.SFX_ENEMY_SHOT);
                }
            }
            yield return new WaitForSeconds(chainInterval);
        }

        yield return new WaitForSeconds(Interval);;
    }


    //------------------------------------------------------------------
    //  Phase2:�q�S�̌Q��̐i�H��\������
    //------------------------------------------------------------------
    private IEnumerator DisplayDirection(TsukumoPhase2Bullet.Direction direction, Vector2 pos)
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

        //  �e�X�g
        GameObject canvas = GameObject.Find("BossCanvas");

        if(!canvas.activeSelf)
        {
            Debug.LogError("BossCanvas���L��������Ă��܂���I");
        }


        //  WARNING��L����
        warningObject.SetActive(true);

        //  ����ł͂Ȃ��Ȃ����̂�TRUE
        bWarningFirst = true;

        //  ���o���R�b�Ȃ̂ł��̕��҂�
        yield return new WaitForSeconds(duration);

        //  WARNING�𖳌���
        warningObject.SetActive(false);
    }

    //------------------------------------------------------------------
    //  Phase2:�����_���ȃX�|�i�[����􂪋���ł���
    //------------------------------------------------------------------
    private IEnumerator TatamiSand()
    {
        int fourDirection = -1;             //  ���I�������
        int fourDirection_mirror = -1;      //  ���̔��Α��̕���

        //  ���[���狲�ݍ��ނ̂Œ��I��������͏�ƍ���������΂悢

        while(true)
        {
            //  �܂��͂Q�����Œ��I
            int rand  = Random.Range(0,2);

            if (rand == 0)
            {
                fourDirection = 0;  //  �����
            }
            else
            {
               fourDirection = 2;   //  ������
            }

            //  ���I���������ɉ����Ĕ��Α��̕�����ݒ肷��
            if (fourDirection == 0)fourDirection_mirror = 1;        //  �㉺
            else if(fourDirection == 2)fourDirection_mirror = 3;    //  ���E

            //  �ԍ����g�p�ς݂ł͂Ȃ�������
            if(buelletNum[fourDirection] == -1)
            {
                //  ����̔ԍ����L�^
                buelletNum[fourDirection] = fourDirection;

                //  ���Α��̔ԍ����o�^
                buelletNum[fourDirection_mirror] = fourDirection_mirror;

                break;
            }
        }

        //  �����m����臒l�ŌĂѕ�����
        if (fourDirection == 0)         //  ������Ƃ���
        {
            //  �M�~�b�N�e�̃v���n�u���擾
            /*�v���n�u�̎�ނ�������\��*/
            GameObject bullet = EnemyManager.Instance
                .GetBulletPrefab((int)BULLET_TYPE.Tsukumo_Gimmick_Top);

            GameObject bullet_mirror = EnemyManager.Instance
                .GetBulletPrefab((int)BULLET_TYPE.Tsukumo_Gimmick_Bottom);

            //  �R�ӏ��Œ��I
            int rand = Random.Range(0,3);
            Vector3 pos = default;
            Vector3 pos_mirror = default;
            
            if(rand == 0)       //  ��
            {
                pos = EnemyManager.Instance.GetSpawnerPos(0);

                //  �q�S�̌Q��̐i�H��\������
                StartCoroutine(
                    DisplayDirection(
                        TsukumoPhase2Bullet.Direction.TOP,
                        new Vector2(-375,300)
                        ));

                pos_mirror = EnemyManager.Instance.GetSpawnerPos(8);

                //  �q�S�̌Q��̐i�H��\������
                yield return StartCoroutine(
                    DisplayDirection(
                        TsukumoPhase2Bullet.Direction.BOTTOM,
                        new Vector2(-375,-300)
                        ));
            }
            else if(rand == 1)  //  ��
            {
                pos = EnemyManager.Instance.GetSpawnerPos(1);

                //  �q�S�̌Q��̐i�H��\������
                StartCoroutine(
                    DisplayDirection(
                        TsukumoPhase2Bullet.Direction.TOP,
                        new Vector2(-60,300)
                        ));

                pos_mirror = EnemyManager.Instance.GetSpawnerPos(7);

                //  �q�S�̌Q��̐i�H��\������
                yield return StartCoroutine(
                    DisplayDirection(
                        TsukumoPhase2Bullet.Direction.BOTTOM,
                        new Vector2(-60,-300)
                        ));
            }
            else if(rand == 2)  //  �E
            {
                pos = EnemyManager.Instance.GetSpawnerPos(2);

                //  �q�S�̌Q��̐i�H��\������
                StartCoroutine(
                    DisplayDirection(
                        TsukumoPhase2Bullet.Direction.TOP,
                        new Vector2(250,300)
                        ));

                pos_mirror = EnemyManager.Instance.GetSpawnerPos(6);

                //  �q�S�̌Q��̐i�H��\������
                yield return StartCoroutine(
                    DisplayDirection(
                        TsukumoPhase2Bullet.Direction.BOTTOM,
                        new Vector2(250,-300)
                        ));
            }

            //  0.5�b�̃f�B���C��������
            yield return new WaitForSeconds(0.5f);

            /************************/
            /* ������̃Z�b�g�A�b�v */
            /************************/

            //  �q�S�𐶐�
            GameObject obj_up = Instantiate(bullet,pos,Quaternion.identity);
            TsukumoPhase2Bullet bulletComp_up =  obj_up.GetComponent<TsukumoPhase2Bullet>();
            bulletComp_up.SetPower(enemyData.Attack);
            bulletComp_up.SetDirection(TsukumoPhase2Bullet.Direction.TOP);

            //  �q�S���ˌ�����
            StartCoroutine(bulletComp_up.BulletMove());

            /************************/
            /* �������̃Z�b�g�A�b�v */
            /************************/

            //  �q�S�𐶐�
            GameObject obj_bottom = Instantiate(bullet,pos_mirror,Quaternion.identity);
            TsukumoPhase2Bullet bulletComp_bottom =  obj_bottom.GetComponent<TsukumoPhase2Bullet>();
            bulletComp_bottom.SetPower(enemyData.Attack);
            bulletComp_bottom.SetDirection(TsukumoPhase2Bullet.Direction.BOTTOM);

            //  �q�S���ˌ�����
            yield return StartCoroutine(bulletComp_bottom.BulletMove());

            //  ���Z�b�g
            buelletNum[fourDirection] = -1;
            buelletNum[fourDirection_mirror] = -1;
        }
        else if (fourDirection == 2)    //  �������Ƃ���
        {
            //  �M�~�b�N�e�̃v���n�u���擾
            GameObject bullet = EnemyManager.Instance
                .GetBulletPrefab((int)BULLET_TYPE.Tsukumo_Gimmick_Left);

            GameObject bullet_mirror = EnemyManager.Instance
                .GetBulletPrefab((int)BULLET_TYPE.Tsukumo_Gimmick_Right);

            //  �R�ӏ��Œ��I
            int rand = Random.Range(0,3);
            Vector3 pos = default;
            Vector3 pos_mirror = default;

            if(rand == 0)       //  ��
            {
                pos = EnemyManager.Instance.GetSpawnerPos(11);

                //  �q�S�̌Q��̐i�H��\������
                StartCoroutine(
                    DisplayDirection(
                        TsukumoPhase2Bullet.Direction.LEFT,
                        new Vector2(-430,180)
                        ));

                pos_mirror = EnemyManager.Instance.GetSpawnerPos(3);

                //  �q�S�̌Q��̐i�H��\������
                yield return StartCoroutine(
                    DisplayDirection(
                        TsukumoPhase2Bullet.Direction.RIGHT,
                        new Vector2(300,180)
                        ));
            }
            else if(rand == 1)  //  ��
            {
                pos = EnemyManager.Instance.GetSpawnerPos(10);

                //  �q�S�̌Q��̐i�H��\������
                StartCoroutine(
                    DisplayDirection(
                        TsukumoPhase2Bullet.Direction.LEFT,
                        new Vector2(-430,-15)
                        ));

                pos_mirror = EnemyManager.Instance.GetSpawnerPos(4);

                //  �q�S�̌Q��̐i�H��\������
                yield return StartCoroutine(
                    DisplayDirection(
                        TsukumoPhase2Bullet.Direction.RIGHT,
                        new Vector2(300,-15)
                        ));
            }
            else if(rand == 2)  //  ��
            {
                pos = EnemyManager.Instance.GetSpawnerPos(9);

                //  �q�S�̌Q��̐i�H��\������
                StartCoroutine(
                    DisplayDirection(
                        TsukumoPhase2Bullet.Direction.LEFT,
                        new Vector2(-430,-220)
                        ));

                pos_mirror = EnemyManager.Instance.GetSpawnerPos(5);

                //  �q�S�̌Q��̐i�H��\������
                yield return StartCoroutine(
                    DisplayDirection(
                        TsukumoPhase2Bullet.Direction.RIGHT,
                        new Vector2(300,-215)
                        ));
            }

            //  0.5�b�̃f�B���C��������
            yield return new WaitForSeconds(0.5f);

            /************************/
            /* �������̃Z�b�g�A�b�v */
            /************************/

            //  �q�S�𐶐�
            GameObject obj_left = Instantiate(bullet,pos,Quaternion.identity);
            TsukumoPhase2Bullet bulletComp_left =  obj_left.GetComponent<TsukumoPhase2Bullet>();
            bulletComp_left.SetPower(enemyData.Attack);
            bulletComp_left.SetDirection(TsukumoPhase2Bullet.Direction.LEFT);

            //  �q�S���ˌ�����
            StartCoroutine(bulletComp_left.BulletMove());

            /************************/
            /* �E�����̃Z�b�g�A�b�v */
            /************************/

            //  �q�S�𐶐�
            GameObject obj_right = Instantiate(bullet,pos_mirror,Quaternion.identity);
            TsukumoPhase2Bullet bulletComp_right =  obj_right.GetComponent<TsukumoPhase2Bullet>();
            bulletComp_right.SetPower(enemyData.Attack);
            bulletComp_right.SetDirection(TsukumoPhase2Bullet.Direction.RIGHT);

            //  �q�S���ˌ�����
            yield return StartCoroutine(bulletComp_right.BulletMove());

            //  ���Z�b�g
            buelletNum[fourDirection] = -1;
            buelletNum[fourDirection_mirror] = -1;


        }
        yield return null;
    }
}
