using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using System.Linq;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine.Experimental.GlobalIllumination;
using System.Threading;

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
        Clamp,                      // �J�N�J�N�ړ�
        ChargeToPlayer,             // �v���C���[�ɓːi
        Straight,                   // �����ړ�
        AdjustLine,                 // �����킹
        Curve                       // ������
    }
    // �����̎��
    [SerializeField] private MOVE_TYPE moveType = MOVE_TYPE.None;

    // �U�R�G�̎��(���O��EnemySetting��Id�Ɠ����ɂ���)
    public enum ENEMY_TYPE {
        None,                       //  ���ݒ�
        Kooni,                      //  �q�S
        Onibi,                      //  �S��
        Ibarakidouji,               //  ��ؓ��q
        Douji                       //  �h�E�W
    }
    // �U�R�G�̎��
    public ENEMY_TYPE enemyType = ENEMY_TYPE.None;

    //  EnemyData�N���X����̏��擾�p
    EnemyData enemyData;

    //  �o���|�C���g(GameManager����Getter�Ŏ󂯎��)
    private GameObject[] spawner;
    //  ����_(GameManager����Getter�Ŏ󂯎��)
    private GameObject[] controlPoint;
    
    //  �p�����[�^
    [SerializeField] private float moveSpeed = 1.0f;
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

    //----------------------------------------------------
    //  �ړ��o�H�p�X�|�i�[������_�̃g�����X�t�H�[��
    //----------------------------------------------------
    private Transform[] spawners;       //  �X�|�i�[
    private Transform[] controlPoints;  //  ����_


    private void Start()
    {
        //  �J�����Ɏʂ��Ă��Ȃ�
        visible = false;
        //  ���S�t���OOFF
        bDeath = false;
        //  �ŏ��͖��G���[�hOFF
        bSuperMode = false;
        //  ���[�v�J�E���g��ݒ�
        loopCount = 1;
        //  �_�ł̊Ԋu��ݒ�
        flashInterval = 0.1f;

        //  �G���̃A�T�[�V�����i�������Ȃ���΂����Ȃ������j
        Assert.IsTrue(enemyType.ToString() != ENEMY_TYPE.None.ToString(),
            "EnemyType���C���X�y�N�^�[�Őݒ肳��Ă��܂���I");

        //  SpriteRender���擾
        sp = GetComponent<SpriteRenderer>();
    }

    //  �G�̃f�[�^��ݒ� 
    public void SetEnemyData(EnemySetting es)
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
    }

    private void OnDestroy()
    {
        //  �j�󂳂ꂽ�G����+1
        int destroyNum = EnemyManager.Instance.GetDestroyNum();
        destroyNum++;
        EnemyManager.Instance.SetDestroyNum(destroyNum);
        Debug.Log("�j�󂳂ꂽ�G�� : " + destroyNum);
    }

    void Update()
    {
        Vector3 pos = transform.position;

        //  �O�i
        pos += new Vector3(
            0,
            moveSpeed * Time.deltaTime,
            0
            );
        transform.position = pos;
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
            StartCoroutine(Blink(false,loopCount,flashInterval));

            //  ���S�t���OON
            if(hp <= 0)
            {
                bDeath = true;
                Death();       //  ���ꉉ�o
            }
        }
        else if (collision.CompareTag("DoujiConvert"))
        {
            //  �_���[�W����
            float d = GameManager.Instance.GetPlayer()
                .GetComponent<PlayerShotManager>().GetConvertShotPower();
            Damage(d);

            //  �_�ŉ��o
            StartCoroutine(Blink(false,loopCount,flashInterval));

            //  ���S�t���OON
            if(hp <= 0)
            {
                bDeath = true;
                Death2();       //  ���ꉉ�o2
            }
        }
        else if (collision.CompareTag("DoujiKonburst"))
        {
            //  �_���[�W����
            float d = GameManager.Instance.GetPlayer()
                .GetComponent<PlayerBombManager>().GetKonburstShotPower();
            Damage(d);

            //  �_�ŉ��o
            StartCoroutine(Blink(false,loopCount,flashInterval));

            //  ���S�t���OON
            if(hp <= 0)
            {
                bDeath = true;
                Death2();       //  ���ꉉ�o2
            }
        }
        else if (collision.CompareTag("Bomb"))
        {
            //  �_���[�W����
            float d = GameManager.Instance.GetPlayer()
                .GetComponent<PlayerBombManager>().GetBombPower();
            Damage(d);

            //  �_�ŉ��o
            StartCoroutine(Blink(false,loopCount,flashInterval));

            //  ���S�t���OON
            if(hp <= 0)
            {
                bDeath = true;
                Death();       //  ���ꉉ�o
            }
        }

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
    //  ���ꉉ�o(�ʏ�e)
    //-------------------------------------------
    private void Death()
    {
        //  ����G�t�F�N�g
        Instantiate(explosion, transform.position, transform.rotation);

        //  �A�C�e���h���b�v����
        DropItems drop = this.GetComponent<DropItems>();
        if (drop) drop.DropPowerupItem();

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

        //  �A�C�e���h���b�v����
        DropItems drop = this.GetComponent<DropItems>();
        if (drop) drop.DropPowerupItem();

        //  �����𐶐�(���o�[�g�̎���2�{)
        int dropMoney = enemyData.Money;
        drop.DropKon(2 * dropMoney);

        //  �I�u�W�F�N�g���폜
        Destroy(this.gameObject);
    }

    //******************************************************************
    //
    //  �G�̈ړ��p�^�[��
    //
    //******************************************************************

    //------------------------------------------------------------------
    //  �N�����v�ړ��i�X�^�[�g�ʒu�����E�؂�ւ��\�j
    //------------------------------------------------------------------

    //------------------------------------------------------------------
    //  ��苗���܂Œ��i�̌�A���@�_���œ˂�����
    //------------------------------------------------------------------

    //------------------------------------------------------------------
    //  �����ړ�(�X�|�i�[���甽�Α��̃X�|�i�[�ֈړ�)
    //------------------------------------------------------------------

    //------------------------------------------------------------------
    //  �����ړ����Ƀv���C���[��X�Ŏ���Y���������Ύ��@�ɓˌ�
    //------------------------------------------------------------------

    //------------------------------------------------------------------
    //  �������ړ�
    //------------------------------------------------------------------

    //------------------------------------------------------------------
    //  �������ړ��̌�e�������ĕ������ړ��ŏ����Ă���(�����U�R)
    //------------------------------------------------------------------
}
