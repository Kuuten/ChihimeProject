using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

//--------------------------------------------------------------
//
//  �U�R�G�̊�{�N���X
//
//--------------------------------------------------------------
public class Enemy : MonoBehaviour
{
    // �����̎��
    [SerializeField] private enum MOVE_TYPE {
        None,                       // ���ݒ�
        ClampL,                     // �J�N�J�N�ړ����X�^�[�g
        ClampR,                     // �J�N�J�N�ړ��E�X�^�[�g
        ClampRandom,                // �J�N�J�N�ړ������_���X�^�[�g
        ChargeToPlayer,             // �v���C���[�ɓːi
        Straight,                   // �����ړ�
        AdjustLine,                 // �����킹
        Curve,                      // ������
        OssanMove,                  // �������񃀁[�u

        CurveAndShoot,              //  �������ړ�&�e�������ċA���Ă���
        SnipeShot3way,              //  ���@�_���e������
        WildlyShot3way,             //  �o���}�L�e������
        CounterShot3way,            //  �Rway�̌����Ԃ��e������

        MidBoss_Appearance,         //  ���{�X�o��
        MidBoss_MoveSide            //  ���{�X���E�J��Ԃ��ړ�
    }
    // �����̎��
    [SerializeField] private MOVE_TYPE moveType = MOVE_TYPE.None;

    // �U�R�G�̎��(���O��EnemySetting��Id�Ɠ����ɂ���)
    public enum ENEMY_TYPE {
        None,                       //  ���ݒ�
        Kooni,                      //  �q�S
        Onibi,                      //  �S��
        Ibarakidouji,               //  ��ؓ��q
        Douji,                      //  �h�E�W
        Ossan                       //  �������Ⴂ��������
    }
    // �U�R�G�̎��
    public ENEMY_TYPE enemyType = ENEMY_TYPE.None;

    // ���{�X���ǂ����̐ݒ�
    public enum IS_MID_BOSS {
        No,                      //  �U�R
        Yes,                     //  ���{�X
    }
    public IS_MID_BOSS isMidBoss = IS_MID_BOSS.No;

    // �ł��Ԃ��e�̐ݒ�
    public enum REVENGE_SHOT {
        None,                       //  �Ȃ�
        Enable,                     //  ����
    }
    public REVENGE_SHOT revengeShot = REVENGE_SHOT.None;

    //  EnemyData�N���X����̏��擾�p
    EnemyData enemyData;
    
    //  �p�����[�^
    private float hp;
    private bool bDeath;        //  ���S�t���O
    private bool bSuperMode;    //  ���G���[�h�t���O
    
    //  ����G�t�F�N�g
    [SerializeField] private GameObject explosion;

    //  �J�����Ɏʂ��Ă��邩�̔���p
    private bool visible = false;

    //  �_�ł����邽�߂�SpriteRenderer
    SpriteRenderer sp;
    //  �_�ł̊Ԋu
    private float flashInterval;
    //  �_�ł�����Ƃ��̃��[�v�J�E���g
    private int loopCount;

    //  HP�X���C�_�[
    private Slider hpSlider;

    //  �h���b�v�p���[�A�b�v�A�C�e���ꗗ
    private ePowerupItems powerupItems;

    private void Start()
    {
        //  �J�����Ɏʂ��Ă��Ȃ�
        visible = false;
        //  ���S�t���OOFF
        bDeath = false;
        //  �ŏ��͖��G���[�hON
        bSuperMode = true;
        //  ���[�v�J�E���g��ݒ�
        loopCount = 1;
        //  �_�ł̊Ԋu��ݒ�
        flashInterval = 0.2f;

        //  �G���̃A�T�[�V�����i�������Ȃ���΂����Ȃ������j
        Assert.IsTrue(enemyType.ToString() != ENEMY_TYPE.None.ToString(),
            "EnemyType���C���X�y�N�^�[�Őݒ肳��Ă��܂���I");

        //  SpriteRender���擾
        sp = GetComponent<SpriteRenderer>();
    }

    //  �G�̃f�[�^��ݒ� 
    public void SetEnemyData(EnemySetting es, ePowerupItems item)
    {
        //  �G�̃f�[�^��ݒ� 
        enemyData = es.DataList
            .FirstOrDefault(enemy => enemy.Id == enemyType.ToString() );

        //  �̗͂�ݒ�
        hp = enemyData.Hp;

        Debug.Log( "�^�C�v: " + enemyType.ToString() + "\nHP: " + hp );
        Debug.Log( enemyType.ToString() + "�̐ݒ芮��" );

        //Debug.Log($"ID�F{enemyData.Id}");
        //Debug.Log($"HP�F{enemyData.Hp}");
        //Debug.Log($"�U���́F{enemyData.Attack}");
        //Debug.Log($"�����F{enemyData.Money}");

        if(item == ePowerupItems.None)
        {
            powerupItems = ePowerupItems.None;
        }
        else
        {
            //  �h���b�v�A�C�e����ݒ�
            powerupItems = item;    
        }


    }

    private void OnDestroy()
    {
        //  �j�󂳂ꂽ�G����+1
        int destroyNum = EnemyManager.Instance.GetDestroyNum();
        destroyNum++;
        EnemyManager.Instance.SetDestroyNum(destroyNum);
        Debug.Log("�j�󂳂ꂽ�G�� : " + destroyNum);

        //  ���{�X�����ꂽ��U�R����I������
        if(isMidBoss == IS_MID_BOSS.Yes)
        {
            EnemyManager.Instance.SetEndZakoStage(true);
        }
    }

    void Update()
    {
        //  �X���C�_�[���X�V
        if(hpSlider != null)hpSlider.value = hp / enemyData.Hp;

        //  �s���p�^�[��
        switch(moveType)
        {
            case MOVE_TYPE.None:
                break;
            //  �ዉ�U�R
            case MOVE_TYPE.ClampL:
                SetCoroutine( Clamp(true) );
                break;
            case MOVE_TYPE.ClampR:
                SetCoroutine( Clamp(false) );
                break;
            case MOVE_TYPE.ClampRandom:
                SetCoroutine( ClampRandom() );
                break;
            case MOVE_TYPE.ChargeToPlayer:
                SetCoroutine( ChargeToPlayer() );
                break;
            case MOVE_TYPE.OssanMove:
                SetCoroutine( OssanMove() );
                break;
            case MOVE_TYPE.Straight:
                SetCoroutine( Straight() );
                break;
            case MOVE_TYPE.AdjustLine:
                SetCoroutine( AdjustLine() );
                break;
            case MOVE_TYPE.Curve:
                SetCoroutine( Curve() );
                break;
            //  �����U�R
            case MOVE_TYPE.CurveAndShoot:
                SetCoroutine( CurveAndShootRandom() );
                break;
            case MOVE_TYPE.SnipeShot3way:
                SetCoroutine( SnipeShot3way() );
                break;
            case MOVE_TYPE.WildlyShot3way:
                SetCoroutine( WildlyShot3way() );
                break;
            case MOVE_TYPE.CounterShot3way:
                SetCoroutine( CounterShot3way() );
                break;
            //  ���{�X
            case MOVE_TYPE.MidBoss_Appearance:
                SetCoroutine( MidBoss_Appearance() );
                break;
            case MOVE_TYPE.MidBoss_MoveSide:
                SetCoroutine( MidBoss_MoveSide() );
                break;
        }

        ////  ������
        //Vector3 pos = transform.position;

        ////  �O�i
        //pos += new Vector3(
        //    0,
        //    moveSpeed * Time.deltaTime,
        //    0
        //    );
        //transform.position = pos;
    }

    //  Renderer���J�����Ɏʂ������_�ŌĂ΂��
    private void OnBecameVisible()
    {
        visible = true;
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

    //------------------------------------------------------
    //  ���{�X�p��HP�X���C�_�[���Z�b�g����
    //------------------------------------------------------
    public void SetHpSlider(Slider slider)
    {
        hpSlider = slider;
    }

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
        if(!visible || bDeath)return;

        if (collision.CompareTag("DeadWall"))
        {
            if(bSuperMode)return;

            Destroy(this.gameObject);
        }
        else if (collision.CompareTag("NormalBullet"))
        {
            //  �e�̏���
            Destroy(collision.gameObject);

            //  ���G���[�h�Ȃ�e���������ĕԂ�
            if(bSuperMode)return;

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
                //  �����Ԃ��e���L���Ȃ猂���Ԃ�
                if(revengeShot == REVENGE_SHOT.Enable)
                {
                    if(isMidBoss == IS_MID_BOSS.Yes)
                    {
                        StartCoroutine(MidBoss_CounterShot8way());
                    }
                    else StartCoroutine(RevengeShot());
                }
                Death();                       //  ���ꉉ�o
            }
        }
        else if (collision.CompareTag("DoujiConvert"))
        {
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
                //  �����Ԃ��e���L���Ȃ猂���Ԃ�
                if(revengeShot == REVENGE_SHOT.Enable)
                {
                    if(isMidBoss == IS_MID_BOSS.Yes)
                    {
                        StartCoroutine(MidBoss_CounterShot8way());
                    }
                    else StartCoroutine(RevengeShot());
                }
                Death2();                      //  ���ꉉ�o2
            }
        }
        else if (collision.CompareTag("DoujiKonburst"))
        {
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
                //  �����Ԃ��e���L���Ȃ猂���Ԃ�
                if(revengeShot == REVENGE_SHOT.Enable)
                {
                    if(isMidBoss == IS_MID_BOSS.Yes)
                    {
                        StartCoroutine(MidBoss_CounterShot8way());
                    }
                    else StartCoroutine(RevengeShot());
                }
                Death2();                      //  ���ꉉ�o2
            }
        }
        else if (collision.CompareTag("TsukumoConvert"))
        {
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
                //  �����Ԃ��e���L���Ȃ猂���Ԃ�
                if(revengeShot == REVENGE_SHOT.Enable)
                {
                    if(isMidBoss == IS_MID_BOSS.Yes)
                    {
                        StartCoroutine(MidBoss_CounterShot8way());
                    }
                    else StartCoroutine(RevengeShot());
                }
                Death2();                      //  ���ꉉ�o2
            }
        }
        else if (collision.CompareTag("TsukumoKonburst"))
        {
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
                //  �����Ԃ��e���L���Ȃ猂���Ԃ�
                if(revengeShot == REVENGE_SHOT.Enable)
                {
                    if(isMidBoss == IS_MID_BOSS.Yes)
                    {
                        StartCoroutine(MidBoss_CounterShot8way());
                    }
                    else StartCoroutine(RevengeShot());
                }
                Death2();                      //  ���ꉉ�o2
            }
        }
        else if (collision.CompareTag("Bomb"))
        {
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
                //  �����Ԃ��e���L���Ȃ猂���Ԃ�
                if(revengeShot == REVENGE_SHOT.Enable)
                {
                    if(isMidBoss == IS_MID_BOSS.Yes)
                    {
                        StartCoroutine(MidBoss_CounterShot8way());
                    }
                    else StartCoroutine(RevengeShot());
                }
                Death();                        //  ���ꉉ�o
            }
        }
    }


    //-------------------------------------------
    //  �����蔻��𔲂������̏���
    //-------------------------------------------
    private void OnTriggerExit2D(Collider2D collision)
    {
        //bSuperMode = false;
    }

    //-------------------------------------------
    //  �����Ԃ�����
    //-------------------------------------------
    public IEnumerator RevengeShot()
    {
         SetCoroutine( CounterShot3way() );

        yield return null;
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
        //if(super)bSuperMode = false;
    }

    //-------------------------------------------
    //  ���ꉉ�o(�ʏ�e�E�{��)
    //-------------------------------------------
    private void Death()
    {
        //  ����G�t�F�N�g
        Instantiate(explosion, transform.position, transform.rotation);

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
        if (drop) drop.DropKon(enemyData.Money);

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
    //  �U�R�G�̈ړ��p�^�[��
    //
    //******************************************************************

    private void SetCoroutine(IEnumerator func)
    {
        StartCoroutine(func);
        moveType = MOVE_TYPE.None;
    }

    //------------------------------------------------------------------
    //  �w����W�܂Œ����ړ�����
    //------------------------------------------------------------------
    private IEnumerator LineMove(Vector3 pos, float duration)
    {
        transform.DOMove(pos, duration)
            .SetEase(Ease.Linear)
            .WaitForCompletion();

        yield return null;
    }

    //------------------------------------------------------------------
    //  ���݈ʒu����w�苗�����������ړ�����
    //------------------------------------------------------------------
    private IEnumerator LineMoveDistance(Vector3 distance, float duration)
    {
        transform.DOMove(distance, duration)
            .SetRelative(true)
            .SetEase(Ease.Linear)
            .WaitForCompletion();

        yield return null;
    }

    //------------------------------------------------------------------
    //  �N�����v�ړ��i�X�^�[�g�ʒu�����E�؂�ւ��\�j
    //------------------------------------------------------------------
    private IEnumerator Clamp(bool startFromLeft)
    {
        //  ���G���[�hOFF
        bSuperMode = false;

        //  �J�n�_
        Vector3 start = Vector3.zero;

        //  �o�H��10�_
        Vector3[] points = new Vector3[10];

        //  �ړ��ɂ����鎞��
        float duration = 2.0f;

        //  �����ɂ���ĊJ�n�ʒu������������
        if(startFromLeft)
        {
            start = EnemyManager.Instance.GetSpawnerPos(8);
            points[0] = EnemyManager.Instance.GetControlPointPos(6);
            points[1] = EnemyManager.Instance.GetControlPointPos(7);
            points[2] = EnemyManager.Instance.GetControlPointPos(8);
            points[3] = EnemyManager.Instance.GetControlPointPos(5);
            points[4] = EnemyManager.Instance.GetControlPointPos(4);
            points[5] = EnemyManager.Instance.GetControlPointPos(3);
            points[6] = EnemyManager.Instance.GetControlPointPos(0);
            points[7] = EnemyManager.Instance.GetControlPointPos(1);
            points[8] = EnemyManager.Instance.GetControlPointPos(2);
            points[9] = EnemyManager.Instance.GetSpawnerPos(2);
        }
        else
        {
            start = EnemyManager.Instance.GetSpawnerPos(6);
            points[0] = EnemyManager.Instance.GetControlPointPos(8);
            points[1] = EnemyManager.Instance.GetControlPointPos(7);
            points[2] = EnemyManager.Instance.GetControlPointPos(6);
            points[3] = EnemyManager.Instance.GetControlPointPos(3);
            points[4] = EnemyManager.Instance.GetControlPointPos(4);
            points[5] = EnemyManager.Instance.GetControlPointPos(5);
            points[6] = EnemyManager.Instance.GetControlPointPos(2);
            points[7] = EnemyManager.Instance.GetControlPointPos(1);
            points[8] = EnemyManager.Instance.GetControlPointPos(0);
            points[9] = EnemyManager.Instance.GetSpawnerPos(0);
        }

        //  �J�n���W��ݒ�
        transform.position = start;

        Debug.Log("�������W�ݒ芮���I");

        for(int i = 0; i < points.Length;i++)
        {
            yield return StartCoroutine(LineMove( points[i], duration ));

            yield return new WaitForSeconds(duration);
        }

        yield return null;
    }

    //  �����_��
    private IEnumerator ClampRandom()
    {
        //  ���G���[�hOFF
        bSuperMode = false;

        bool left = Convert.ToBoolean( UnityEngine.Random.Range(0,2) );

        SetCoroutine(Clamp(left));

        yield return null;
    }
    //------------------------------------------------------------------
    //  ��苗���܂Œ��i�̌�A���@�_���œ˂�����
    //------------------------------------------------------------------
    private IEnumerator ChargeToPlayer()
    {
        const int Right = 6;
        const int Middle = 7;
        const int Left = 8;
        int[] Position = new int[3];
        Position[0] = Right;
        Position[1] = Middle;
        Position[2] = Left;

        int rand = UnityEngine.Random.Range(0,3);

        float d = 5.0f;         //  �ړ�����
        float speed2 = 5.0f;    //  �ړ��X�s�[�h�Q
        Vector3 dintance = new Vector3(0,d,0);

        //  �X�|�i�[[6][7][8]���烉���_���ɏo��
        Vector3 start = EnemyManager.Instance.GetSpawnerPos(Position[rand]);
        transform.position = start;

        //  ��苗�����i
        yield return StartCoroutine(
            LineMoveDistance( dintance, 1.0f ));

        //  �������ԑ҂�
        yield return new WaitForSeconds(1.0f);

        //  ���G���[�hOFF
        bSuperMode = false;

        //  2�b�҂�
        yield return new WaitForSeconds(2.0f);

        //  �v���C���[�ւ̃x�N�g�����v�Z
        Vector3 playerPos = GameManager.Instance.GetPlayer().transform.position;
        Vector3 vec = playerPos - transform.position;
        vec.Normalize();

        //  ���̕����ɂł���߂ȋ����L�΂������W�Ɉړ�����
        float dist = 30.0f;
        vec = vec * dist;

        //  ��苗�����i
        yield return StartCoroutine(
            LineMoveDistance( vec, vec.magnitude/speed2 ));

        //  �������ԑ҂�
        yield return new WaitForSeconds(vec.magnitude/speed2);

        yield return null;
    }
    //------------------------------------------------------------------
    //  �������񃀁[�u
    //------------------------------------------------------------------
    private IEnumerator OssanMove()
    {
        float speed = 3.0f;    //  �ړ��X�s�[�h

        //  ���G���[�hOFF
        bSuperMode = false;

        //  2�b�҂�
        yield return new WaitForSeconds(2.0f);

        //  �v���C���[�ւ̃x�N�g�����v�Z
        Vector3 playerPos = GameManager.Instance.GetPlayer().transform.position;
        Vector3 vec = playerPos - transform.position;
        vec.Normalize();

        //  ���̕����ɂł���߂ȋ����L�΂������W�Ɉړ�����
        float dist = 30.0f;
        vec = vec * dist;

        //  ��苗�����i
        yield return StartCoroutine(
            LineMoveDistance( vec, vec.magnitude/speed ));

        yield return null;
    }

    //------------------------------------------------------------------
    //  �����ړ�(�X�|�i�[���甽�Α��̃X�|�i�[�ֈړ�)
    //------------------------------------------------------------------
    private IEnumerator Straight()
    {
        //  ���G���[�hOFF
        bSuperMode = false;

        //  �J�n���W��ݒ�(3�`11)
        int rand = UnityEngine.Random.Range(3,12);
        int goalNum = 0;
        Vector3 start = EnemyManager.Instance.GetSpawnerPos(rand);
        transform.position = start;

        if(rand == 0 || rand == 1 || rand ==2)
            Debug.Log("rand�̒l���s���ł�: " + rand);

        //  �ړ��X�s�[�h��ݒ�
        float speed = 3.0f;

        //  �J�n���W�ɂ���ĖڕW�ԍ���ݒ�
        if(rand == 3)goalNum = 11;
        else if(rand == 4)goalNum = 10;
        else if(rand == 5)goalNum = 9;
        else if(rand == 6)goalNum = 2;
        else if(rand == 7)goalNum = 1;
        else if(rand == 8)goalNum = 0;
        else if(rand == 9)goalNum = 5;
        else if(rand == 10)goalNum = 4;
        else if(rand == 11)goalNum = 3;

        //  �ڕW���W��ݒ�
        Vector3 goal = EnemyManager.Instance.GetSpawnerPos(goalNum);

        Vector3 distance = goal - start;

        //  �ڕW�ɒ����ړ�
        yield return StartCoroutine(
            LineMove(goal, distance.magnitude/speed));

        //  �������ԑ҂�
        yield return new WaitForSeconds(distance.magnitude/speed);

        yield return null;
    }
    //------------------------------------------------------------------
    //  �v���C���[��Y����������TRUE��Ԃ�
    //------------------------------------------------------------------
    private bool AdjustAxisY()
    {
        GameObject player = GameManager.Instance.GetPlayer();
        float distanceY = player.transform.position.y - transform.position.y;
        if(Math.Abs(distanceY) <= 0.05f)
        {
            return true;
        }

        return false;  
    }
    //------------------------------------------------------------------
    //  �v���C���[��Y����������TRUE��Ԃ�
    //------------------------------------------------------------------
    private bool AdjustAxisX()
    {
        GameObject player = GameManager.Instance.GetPlayer();
        float distanceX = player.transform.position.x - transform.position.x;
        if(Math.Abs(distanceX) <= 0.05f)
        {
            return true;
        }

        return false;  
    }
    //------------------------------------------------------------------
    //  �����ړ����Ƀv���C���[��Y���������Ύ��@�ɓˌ�
    //------------------------------------------------------------------
    private IEnumerator AdjustLineY()
    {
        // 0,1,2,6,7,8�̃��X�g��p�ӂ���
        List<int> spawmNo = new List<int>();
        spawmNo.Add(0);
        spawmNo.Add(1);
        spawmNo.Add(2);
        spawmNo.Add(6);
        spawmNo.Add(7);
        spawmNo.Add(8);

        int rand = spawmNo[UnityEngine.Random.Range(0,spawmNo.Count)];
        int goalNum = 0;
        Vector3 start = EnemyManager.Instance.GetSpawnerPos(rand);
        transform.position = start;

        //  �ړ��X�s�[�h��ݒ�
        float speed1 = 1.0f;
        float speed2 = 2.0f;

        //  �J�n���W�ɂ���ĖڕW�ԍ���ݒ�
        if(rand == 0)goalNum = 8;
        else if(rand == 1)goalNum = 7;
        else if(rand == 2)goalNum = 6;
        else if(rand == 6)goalNum = 2;
        else if(rand == 7)goalNum = 1;
        else if(rand == 8)goalNum = 0;

        //  �ڕW���W��ݒ�
        Vector3 goal = EnemyManager.Instance.GetSpawnerPos(goalNum);

        Vector3 distance = goal - start;

        //  �ڕW�ɒ����ړ�
        Tweener tweener = transform.DOMove(goal, distance.magnitude/speed1)
            .SetEase(Ease.Linear);

        //  Y���������܂ő҂�
        yield return new WaitUntil(() => AdjustAxisY() == true);

        // DoMove��r���I��
        tweener.Kill();

        //  1�b�҂�
        yield return new WaitForSeconds(1);

        GameObject player = GameManager.Instance.GetPlayer();
        float dX =  player.transform.position.x - transform.position.x;

        if(dX > 0)
        {
            transform.DOMoveX(11f, Math.Abs(dX)/speed2)
                .SetEase(Ease.Linear);
        }
        else
        {
            transform.DOMoveX(-14f, Math.Abs(dX)/speed2)
                .SetEase(Ease.Linear);
        }

        //  �҂�
        yield return new WaitForSeconds(dX/speed2);

        yield return null;
    }
    //------------------------------------------------------------------
    //  �����ړ����Ƀv���C���[��X���������Ύ��@�ɓˌ�
    //------------------------------------------------------------------
    private IEnumerator AdjustLineX()
    {
        // 0,1,2,6,7,8�̃��X�g��p�ӂ���
        List<int> spawmNo = new List<int>();
        spawmNo.Add(3);
        spawmNo.Add(4);
        spawmNo.Add(5);
        spawmNo.Add(9);
        spawmNo.Add(10);
        spawmNo.Add(11);

        int rand = spawmNo[UnityEngine.Random.Range(0,spawmNo.Count)];
        int goalNum = 0;
        Vector3 start = EnemyManager.Instance.GetSpawnerPos(rand);
        transform.position = start;

        //  �ړ��X�s�[�h��ݒ�
        float speed1 = 1.0f;
        float speed2 = 2.0f;

        //  �J�n���W�ɂ���ĖڕW�ԍ���ݒ�
        if(rand == 3)goalNum = 11;
        else if(rand == 4)goalNum = 10;
        else if(rand == 5)goalNum = 9;
        else if(rand == 9)goalNum = 5;
        else if(rand == 10)goalNum = 4;
        else if(rand == 11)goalNum = 3;

        //  �ڕW���W��ݒ�
        Vector3 goal = EnemyManager.Instance.GetSpawnerPos(goalNum);

        Vector3 distance = goal - start;

        //  �ڕW�ɒ����ړ�
        Tweener tweener = transform.DOMove(goal, distance.magnitude/speed1)
            .SetEase(Ease.Linear);

        //  X���������܂ő҂�
        yield return new WaitUntil(() => AdjustAxisX() == true);

        // DoMove��r���I��
        tweener.Kill();

        //  1�b�҂�
        yield return new WaitForSeconds(1);

        GameObject player = GameManager.Instance.GetPlayer();
        float dY =  player.transform.position.y - transform.position.y;

        if(dY > 0)
        {
            transform.DOMoveY(11f, Math.Abs(dY)/speed2)
                .SetEase(Ease.Linear);
        }
        else
        {
            transform.DOMoveY(-8f, Math.Abs(dY)/speed2)
                .SetEase(Ease.Linear);
        }

        //  �҂�
        yield return new WaitForSeconds(dY/speed2);

        yield return null;
    }
    //------------------------------------------------------------------
    //  �����ړ����Ƀv���C���[��X����Y���������Ύ��@�ɓˌ�
    //------------------------------------------------------------------
    private IEnumerator AdjustLine()
    {
        //  ���G���[�hOFF
        bSuperMode = false;

        //  X����Y�����̓����_��
        int rand = UnityEngine.Random.Range(0,2);

        if(rand == 0)StartCoroutine(AdjustLineY());
        else StartCoroutine(AdjustLineX());

        yield return null;
    }
    //------------------------------------------------------------------
    //  �������ړ��X�e�b�v�P(�X�^�[�g�ʒu��)
    //------------------------------------------------------------------
    private IEnumerator CurveFromL1()
    {
        float duration = 3.0f; //  �ړ��ɂ����鎞��

        Vector3 start = EnemyManager.Instance.GetSpawnerPos(8);
        Vector3 middle1 = new Vector3(-4.0f,-1.65f,0);
        Vector3 middle2 = new Vector3(0f,-0.65f,0);
        Vector3 middle3 = new Vector3(4.5f,-1.65f,0);
        Vector3 end = EnemyManager.Instance.GetSpawnerPos(6);

        //  �������W��ݒ�
        transform.position = start;

        transform.DOLocalPath(
            new[]
            {
                start,
                middle1,
                middle2,
            },
            duration, PathType.CatmullRom)
            .SetEase(Ease.Linear)
            .WaitForCompletion();
         
        //  5�b�҂�
        yield return new WaitForSeconds(duration);
    }
    //------------------------------------------------------------------
    //  �������ړ��X�e�b�v�Q(�X�^�[�g�ʒu��)
    //------------------------------------------------------------------
    private IEnumerator CurveFromL2()
    {
        float duration = 3.0f; //  �ړ��ɂ����鎞��

        Vector3 start = EnemyManager.Instance.GetSpawnerPos(8);
        Vector3 middle1 = new Vector3(-4.0f,-1.65f,0);
        Vector3 middle2 = new Vector3(0f,-0.65f,0);
        Vector3 middle3 = new Vector3(4.5f,-1.65f,0);
        Vector3 end = EnemyManager.Instance.GetSpawnerPos(6);

        //  �������W��ݒ�
        transform.position = middle2;

        transform.DOLocalPath(
            new[]
            {
                middle2,
                middle3,
                end,
            },
            duration, PathType.CatmullRom)
            .SetEase(Ease.Linear)
            .WaitForCompletion();
         
        //  5�b�҂�
        yield return new WaitForSeconds(duration);
    }
    //------------------------------------------------------------------
    //  �������ړ�(�X�^�[�g�ʒu��)
    //------------------------------------------------------------------
    private IEnumerator CurveFromL()
    {
        yield return StartCoroutine( CurveFromL1() );

        yield return StartCoroutine( CurveFromL2() );
        
        yield return null; 
    }
    //------------------------------------------------------------------
    //  �������ړ��X�e�b�v�P(�X�^�[�g�ʒu�E)
    //------------------------------------------------------------------
    private IEnumerator CurveFromR1()
    {
        float duration = 3.0f; //  �ړ��ɂ����鎞��

        Vector3 start = EnemyManager.Instance.GetSpawnerPos(6);
        Vector3 middle1 = new Vector3(4.5f,-1.65f,0);
        Vector3 middle2 = new Vector3(0f,-0.65f,0);
        Vector3 middle3 = new Vector3(-4.0f,-1.65f,0);
        Vector3 end = EnemyManager.Instance.GetSpawnerPos(8);

        //  �������W��ݒ�
        transform.position = start;

        transform.DOLocalPath(
            new[]
            {
                start,
                middle1,
                middle2,
            },
            duration, PathType.CatmullRom)
            .SetEase(Ease.Linear)
            .WaitForCompletion();
         
        //  5�b�҂�
        yield return new WaitForSeconds(duration);
    }
    //------------------------------------------------------------------
    //  �������ړ��X�e�b�v�Q(�X�^�[�g�ʒu�E)
    //------------------------------------------------------------------
    private IEnumerator CurveFromR2()
    {
        float duration = 3.0f; //  �ړ��ɂ����鎞��

        Vector3 start = EnemyManager.Instance.GetSpawnerPos(6);
        Vector3 middle1 = new Vector3(4.5f,-1.65f,0);
        Vector3 middle2 = new Vector3(0f,-0.65f,0);
        Vector3 middle3 = new Vector3(-4.0f,-1.65f,0);
        Vector3 end = EnemyManager.Instance.GetSpawnerPos(8);

        //  �������W��ݒ�
        transform.position = middle2;

        transform.DOLocalPath(
            new[]
            {
                middle2,
                middle3,
                end,
            },
            duration, PathType.CatmullRom)
            .SetEase(Ease.Linear)
            .WaitForCompletion();
         
        //  5�b�҂�
        yield return new WaitForSeconds(duration);
    }
    //------------------------------------------------------------------
    //  �������ړ�(�X�^�[�g�ʒu�E)
    //------------------------------------------------------------------
    private IEnumerator CurveFromR()
    {
        yield return StartCoroutine( CurveFromR1() );

        yield return StartCoroutine( CurveFromR2() );
        
        yield return null; 
    }
    //------------------------------------------------------------------
    //  �������ړ�(�X�^�[�g�ʒu�̓����_��)
    //------------------------------------------------------------------
    private IEnumerator Curve()
    {
        //  ���G���[�hOFF
        bSuperMode = false;

        //  �X�^�[�g�ʒu�̓����_��
        int rand = UnityEngine.Random.Range(0,2);

        if(rand == 0)StartCoroutine(CurveFromL());
        else StartCoroutine(CurveFromR());

        yield return null;
    }


    //------------------------------------------------------------------
    //  �������ړ��̌㎩�@�_���e�������ĕ������ړ��ŏ����Ă���(�����U�R)
    //------------------------------------------------------------------
    private IEnumerator CurveAndShootL()
    {
        yield return StartCoroutine( CurveFromL1() );

        //  �����Ŏ��@�_���e������
        yield return StartCoroutine( SnipeShot3way() );

        //  2�b�҂�
        yield return new WaitForSeconds(2);

        yield return StartCoroutine( CurveFromL2() );
        
        yield return null; 
    }
    //------------------------------------------------------------------
    //  �������ړ��̌㎩�@�_���e�������ĕ������ړ��ŏ����Ă���(�����U�R)
    //------------------------------------------------------------------
    private IEnumerator CurveAndShootR()
    {
        yield return StartCoroutine( CurveFromR1() );

        //  �����Ŏ��@�_���e������
        yield return StartCoroutine( SnipeShot3way() );

        //  2�b�҂�
        yield return new WaitForSeconds(2);

        yield return StartCoroutine( CurveFromR2() );

        yield return null;
    }
    //------------------------------------------------------------------
    //  �������ړ��̌㎩�@�_���e�������ĕ������ړ��ŏ����Ă���(�����U�R)
    //------------------------------------------------------------------
    private IEnumerator CurveAndShootRandom()
    {
        //  ���G���[�hOFF
        bSuperMode = false;

        //  �X�^�[�g�ʒu�̓����_��
        int rand = UnityEngine.Random.Range(0,2);

        if(rand == 0)StartCoroutine(CurveAndShootL());
        else StartCoroutine(CurveAndShootR());

        yield return null;
    }
    //------------------------------------------------------------------
    //  ���@�_���e������
    //------------------------------------------------------------------
    private IEnumerator SnipeShot3way()
    {
        //  �ʏ펩�@�_���e�̃v���n�u���擾
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Snipe_Normal);

        float Degree = 30;              //  ���炷�p�x
        int wayNum = 3;                 //  �e��way��
        float speed = 2.0f;             //  �e��
        int chain = 1;                  //  �A�e��
        float chainInterval = 0.3f;     //  �A�e�̊Ԋu�i�b�j

        //  �G����v���C���[�ւ̃x�N�g�����擾
        Vector3 playerPos = GameManager.Instance.GetPlayer().transform.position;
        Vector3 vector0 = (playerPos - transform.position).normalized;
        Vector3[] vector = new Vector3[wayNum];

        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < wayNum; i++)
            {
                vector[i] = Quaternion.Euler( 0, 0, -Degree + (i * Degree) ) * vector0;
                vector[i].z = 0f;

                //�e�C���X�^���X���擾���A�����Ɣ��ˊp�x��^����
                GameObject Bullet_obj = 
                    (GameObject)Instantiate(bullet, transform.position, transform.rotation);
                EnemyBullet enemyBullet = Bullet_obj.GetComponent<EnemyBullet>();
                enemyBullet.SetSpeed(speed);
                enemyBullet.SetVelocity(vector[i]);
                enemyBullet.SetPower(enemyData.Attack);

                //  ����SE�Đ�
                SoundManager.Instance.PlaySFX(
                (int)AudioChannel.ENEMY_SHOT,
                (int)SFXList.SFX_ENEMY_SHOT);
            }
            yield return new WaitForSeconds(chainInterval);
        }


        yield return null;
    }
    //------------------------------------------------------------------
    //  �o���}�L�e������
    //------------------------------------------------------------------
    private IEnumerator WildlyShot3way()
    {
        //  �ʏ펩�@�_���e�̃v���n�u���擾
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Wildly_Normal);

        float Degree = 45;              //  ���炷�p�x
        int wayNum = 3;                 //  �e��way��
        float speed = 2.0f;             //  �e��
        int chain = 1;                  //  �A�e��
        float chainInterval = 0.3f;     //  �A�e�̊Ԋu�i�b�j

        //  �G�̑O���x�N�g�����擾
        Vector3 vector0 = transform.up;
        Vector3[] vector = new Vector3[wayNum];
        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < wayNum; i++)
            {
                vector[i] = Quaternion.Euler( 0, 0, -Degree + (i * Degree) ) * vector0;
                vector[i].z = 0f;

                //�e�C���X�^���X���擾���A�����Ɣ��ˊp�x��^����
                GameObject Bullet_obj = 
                    (GameObject)Instantiate(bullet, transform.position, transform.rotation);
                EnemyBullet enemyBullet = Bullet_obj.GetComponent<EnemyBullet>();
                enemyBullet.SetSpeed(speed);
                enemyBullet.SetVelocity(vector[i]);
                enemyBullet.SetPower(enemyData.Attack);

                //  ����SE�Đ�
                SoundManager.Instance.PlaySFX(
                (int)AudioChannel.ENEMY_SHOT,
                (int)SFXList.SFX_ENEMY_SHOT);
            }
            yield return new WaitForSeconds(chainInterval);
        }

        yield return null;
    }
    //------------------------------------------------------------------
    //  ���ꂽ���͂Rway�̌����Ԃ��e������
    //------------------------------------------------------------------
    private IEnumerator CounterShot3way()
    {
        //  �ʏ펩�@�_���e�̃v���n�u���擾
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Snipe_Normal);

        float Degree = 30;              //  ���炷�p�x
        int wayNum = 3;                 //  �e��way��
        float speed = 2.0f;             //  �e��
        int chain = 1;                  //  �A�e��
        float chainInterval = 0.3f;     //  �A�e�̊Ԋu�i�b�j

        //  �G����v���C���[�ւ̃x�N�g�����擾
        Vector3 playerPos = GameManager.Instance.GetPlayer().transform.position;
        Vector3 vector0 = (playerPos - transform.position).normalized;
        Vector3[] vector = new Vector3[wayNum];

        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < wayNum; i++)
            {
                vector[i] = Quaternion.Euler( 0, 0, -Degree + (i * Degree) ) * vector0;
                vector[i].z = 0f;

                //�e�C���X�^���X���擾���A�����Ɣ��ˊp�x��^����
                GameObject Bullet_obj = 
                    (GameObject)Instantiate(bullet, transform.position, transform.rotation);
                EnemyBullet enemyBullet = Bullet_obj.GetComponent<EnemyBullet>();
                enemyBullet.SetSpeed(speed);
                enemyBullet.SetVelocity(vector[i]);
                enemyBullet.SetPower(enemyData.Attack);

                //  ����SE�Đ�
                SoundManager.Instance.PlaySFX(
                (int)AudioChannel.ENEMY_SHOT,
                (int)SFXList.SFX_ENEMY_SHOT);
            }
            yield return new WaitForSeconds(chainInterval);
        }

        yield return null;
    }



    //------------------------------------------------------------------
    //  ���{�X�E��������w��|�C���g�Ɉړ�����
    //------------------------------------------------------------------
    private IEnumerator MidBoss_Appearance()
    {
        Vector3 goal = new Vector3(0,-2.5f,0); //  �ڕW���W
        float duration = 3.0f;                  //  �ړ��ɂ����鎞��
        float wait_time = 2.0f;                 //  �ҋ@����

        //  �������W��ݒ�
        transform.position = new Vector3(0,-6,0);

        //  ���W�ɒ����ړ�����
        StartCoroutine(LineMove(goal, duration));

        //  �ړ����Ԓ��ҋ@
        yield return new WaitForSeconds(duration);

        //  ���G���[�hOFF
        bSuperMode = false;

        //  �ړ���ɑҋ@
        yield return new WaitForSeconds(wait_time);

        //  ���ړ����[�h�Ɉڍs
        moveType = MOVE_TYPE.MidBoss_MoveSide;

        yield return null;
    }

    //------------------------------------------------------------------
    //  ���{�X�E���E�ɌJ��Ԃ��ړ�����
    //------------------------------------------------------------------
    private IEnumerator MidBoss_MoveSide()
    {
        //  Phase1
        yield return StartCoroutine(MidBoss_Phase1());

        Debug.Log("***���{�X��Q�i�K�J�n�I***");

        //  Phase2
        yield return StartCoroutine(MidBoss_Phase2());

        yield return null;
    }
    //------------------------------------------------------------------
    //  ���{�X�EPhase1
    //------------------------------------------------------------------
    private IEnumerator MidBoss_Phase1()
    {
        float duration = 2.5f;  //  �ړ��ɂ����鎞��

        while(true)
        {
            transform.DOLocalMoveX(5f, duration)
                .SetEase(Ease.OutBack);

            //  �ړ����ԑ҂�
            yield return new WaitForSeconds(duration);

            ////  �o���}�L�e
            //yield return StartCoroutine(MidBoss_WildlyShot5way());

            //  360�x�o���}�L�e
            yield return StartCoroutine(MidBoss_WildlyShot360());

            //  HP��������؂����甲����
            if(hp <= enemyData.Hp*0.7)break;

            transform.DOLocalMoveX(-5f, duration)
            .SetEase(Ease.OutBack);

            //  �ړ����ԑ҂�
            yield return new WaitForSeconds(duration);

            ////  �o���}�L�e
            //yield return  StartCoroutine(MidBoss_WildlyShot5way());

            //  360�x�o���}�L�e
            yield return StartCoroutine(MidBoss_WildlyShot360());

            //  HP��������؂����甲����
            if(hp <= enemyData.Hp*0.7)break;
        }
    }

    //------------------------------------------------------------------
    //  ���{�X�EPhase2
    //------------------------------------------------------------------
    private IEnumerator MidBoss_Phase2()
    {
        int rand_move = 40;       //  �W�����v�ړ���臒l
        int rand_wideshot = 70;   //  �o���}�L�e��臒l
        int rand_snipeshot = 100; //  ���@�_���e��臒l

        //  �W�����v
        yield return StartCoroutine(MidBoss_Jump());

        //  �Q�b�҂�
        yield return new WaitForSeconds(2);

        while(true)
        {
            int rand = UnityEngine.Random.Range(0,100);
            if(rand < rand_move)
            {
                yield return StartCoroutine(MidBoss_JumpAndMoveSide());
            }
            else if(rand <= rand_wideshot)
            {
                yield return StartCoroutine(MidBoss_WildlyShot5way());
            }
            else if(rand <= rand_snipeshot)
            {
                yield return StartCoroutine(MidBoss_SnipeShot3way());
            }

            //  1�b�҂�
            yield return new WaitForSeconds(1);
        }
    }
    //------------------------------------------------------------------
    //  ���{�X�E�W�����v
    //------------------------------------------------------------------
    private IEnumerator MidBoss_Jump()
    {
        float duration = 0.1f;

        //  �D�JSE�Đ�
        SoundManager.Instance.PlaySFX(
        (int)AudioChannel.SFX_SYSTEM,
        (int)SFXList.SFX_MIDBOSS_PHASE2);

        //  �W�����v����
        transform.DOLocalMoveY(2f, duration)
                .SetEase(Ease.OutExpo)
                .SetRelative(true);

        //  �W�����vSE�Đ�
        SoundManager.Instance.PlaySFX(
        (int)AudioChannel.SFX_ENEMY,
        (int)SFXList.SFX_MIDBOSS_JUMP);

        yield return new WaitForSeconds(duration);

        transform.DOLocalMoveY(-2f, duration)
        .SetEase(Ease.InExpo)
        .SetRelative(true);

        yield return new WaitForSeconds(duration);

        //  �W�����v����
        transform.DOLocalMoveY(2f, duration)
                .SetEase(Ease.OutExpo)
                .SetRelative(true);

        //  �W�����vSE�Đ�
        SoundManager.Instance.PlaySFX(
        (int)AudioChannel.SFX_ENEMY,
        (int)SFXList.SFX_MIDBOSS_JUMP);

        yield return new WaitForSeconds(duration);

        transform.DOLocalMoveY(-2f, duration)
        .SetEase(Ease.InExpo)
        .SetRelative(true);

        yield return new WaitForSeconds(duration);


        yield return null;
    }
    //------------------------------------------------------------------
    //  ���{�X�E�W�����v�Ɖ��ړ�
    //------------------------------------------------------------------
    private IEnumerator MidBoss_JumpAndMoveSide()
    {
        float interval = 2.0f;  //  ���̍s���܂ł̎���(�b)
        float duration = 0.25f;
        float jumpY = 7.0f;
        float jump_minX = -7.0f;
        float jump_maxX = 5.0f;

        //  �\������łQ�񏬃W�����v����
        //---------------------------------------------------------------------
        //  �W�����v����
        transform.DOLocalMoveY(2f, duration)
                .SetEase(Ease.OutExpo)
                .SetRelative(true);

        //  �W�����vSE�Đ�
        SoundManager.Instance.PlaySFX(
        (int)AudioChannel.SFX_ENEMY,
        (int)SFXList.SFX_MIDBOSS_JUMP);

        yield return new WaitForSeconds(duration);

        transform.DOLocalMoveY(-2f, duration)
        .SetEase(Ease.InExpo)
        .SetRelative(true);

        yield return new WaitForSeconds(duration);

        //  �W�����v����
        transform.DOLocalMoveY(2f, duration)
                .SetEase(Ease.OutExpo)
                .SetRelative(true);

        //  �W�����vSE�Đ�
        SoundManager.Instance.PlaySFX(
        (int)AudioChannel.SFX_ENEMY,
        (int)SFXList.SFX_MIDBOSS_JUMP);

        yield return new WaitForSeconds(duration);

        transform.DOLocalMoveY(-2f, duration)
        .SetEase(Ease.InExpo)
        .SetRelative(true);

        yield return new WaitForSeconds(duration);
        //---------------------------------------------------------------------


        //  ���ړ�����
        float rndX = UnityEngine.Random.Range(jump_minX, jump_maxX);
        transform.DOMoveX(rndX, interval)
            .SetEase(Ease.OutBack);

        //  �W�����v����
        transform.DOLocalMoveY(jumpY, interval/2)
                .SetEase(Ease.OutExpo)
                .SetRelative(true);

        //  �W�����vSE�Đ�
        SoundManager.Instance.PlaySFX(
        (int)AudioChannel.SFX_ENEMY,
        (int)SFXList.SFX_MIDBOSS_JUMP);

        //  �ړ����ԑ҂�
        yield return new WaitForSeconds(interval/2);

        //  �W�����v����߂�
        transform.DOLocalMoveY(-jumpY, interval/2)
                .SetEase(Ease.OutExpo)
                .SetRelative(true);

        //  �߂莞�ԑ҂�
        yield return new WaitForSeconds(interval/2);

        //  ���ړ�����
        rndX = UnityEngine.Random.Range(jump_minX, jump_maxX);
        transform.DOMoveX(rndX, interval)
            .SetEase(Ease.OutBack);

        //  �W�����v����
        transform.DOLocalMoveY(jumpY, interval/2)
                .SetEase(Ease.OutExpo)
                .SetRelative(true);

        //  �W�����vSE�Đ�
        SoundManager.Instance.PlaySFX(
        (int)AudioChannel.SFX_ENEMY,
        (int)SFXList.SFX_MIDBOSS_JUMP);

        //  �ړ����ԑ҂�
        yield return new WaitForSeconds(interval/2);

        //  �W�����v����߂�
        transform.DOLocalMoveY(-jumpY, interval/2)
                .SetEase(Ease.OutExpo)
                .SetRelative(true);

        //  �߂莞�ԑ҂�
        yield return new WaitForSeconds(interval/2);
    }
    //------------------------------------------------------------------
    //  ���{�X�E�Tway�̃o���}�L�e������/�e���E�x��
    //------------------------------------------------------------------
    private IEnumerator MidBoss_WildlyShot5way()
    {
        //  �ʏ펩�@�_���e�̃v���n�u���擾
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Wildly_Normal);

        float totalDegree = 120;        //  ���͈͂̑��p  
        int wayNum = 5;                 //  �e��way��(�K��3way�ȏ�̊�ɂ��邱��)
        float Degree = totalDegree / (wayNum-1);     //  �e�ꔭ���ɂ��炷�p�x         
        float speed = 2.0f;             //  �e��
        int chain = 3;                  //  �A�e��
        float chainInterval = 0.5f;     //  �A�e�̊Ԋu�i�b�j

        //  �G�̑O���x�N�g�����擾
        Vector3 vector0 = transform.up;
        Vector3[] vector = new Vector3[wayNum];
        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < wayNum; i++)
            {
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
    //  ���{�X�E�Rway�̎��@�_���e������/�e���E����
    //------------------------------------------------------------------
    private IEnumerator MidBoss_SnipeShot3way()
    {
        //  �ʏ펩�@�_���e�̃v���n�u���擾
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Snipe_Normal);

        float Degree = 15;              //  ���炷�p�x
        int wayNum = 3;                 //  �e��way��
        float speed = 3.0f;             //  �e��
        int chain = 1;                  //  �A�e��
        float chainInterval = 0.3f;     //  �A�e�̊Ԋu�i�b�j

        //  �G����v���C���[�ւ̃x�N�g�����擾
        Vector3 playerPos = GameManager.Instance.GetPlayer().transform.position;
        Vector3 vector0 = (playerPos - transform.position).normalized;
        Vector3[] vector = new Vector3[wayNum];

        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < wayNum; i++)
            {
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
    //  ���{�X�E360�x�̃o���}�L�e������/�e���E�x��
    //------------------------------------------------------------------
    private IEnumerator MidBoss_WildlyShot360()
    {
        //  �ʏ펩�@�_���e�̃v���n�u���擾
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Wildly_Normal);

        float totalDegree = 360*5;        //  ���͈͂̑��p  
        int wayNum = 24*5;                //  �e��way��
        float Degree = totalDegree / wayNum;     //  �e�ꔭ���ɂ��炷�p�x         
        float speed = 2.0f;               //  �e��
        int chain = wayNum;               //  �A�e��
        float chainInterval = 0.03f;      //  �A�e�̊Ԋu�i�b�j

        //  �G�̑O���x�N�g�����擾
        Vector3 vector0 = transform.up;
        Vector3[] vector = new Vector3[wayNum];
        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < 1; i++)
            {
                vector[i] = Quaternion.Euler
                    (0, 0, -Degree*j) * vector0;
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
    //  ���{�X�E���ꂽ���͂W�����Ɍ����Ԃ��e������
    //------------------------------------------------------------------
    private IEnumerator MidBoss_CounterShot8way()
    {
        //  �ʏ펩�@�_���e�̃v���n�u���擾
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Snipe_Normal);


        float totalDegree = 360;        //  ���͈͂̑��p  
        int wayNum = 8;                 //  �e��way��
        float Degree = totalDegree / (wayNum-1);     //  �e�ꔭ���ɂ��炷�p�x  
        float speed = 2.0f;             //  �e��
        int chain = 1;                  //  �A�e��
        float chainInterval = 0.3f;     //  �A�e�̊Ԋu�i�b�j

        //  �G����v���C���[�ւ̃x�N�g�����擾
        Vector3 playerPos = GameManager.Instance.GetPlayer().transform.position;
        Vector3 vector0 = (playerPos - transform.position).normalized;
        Vector3[] vector = new Vector3[wayNum];

        for (int j = 0; j < chain; j++)
        {
            for (int i = 0; i < wayNum; i++)
            {
                vector[i] = Quaternion.Euler( 0, 0, i * Degree ) * vector0;
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


}
