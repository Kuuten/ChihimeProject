using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using System.Linq;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine.Experimental.GlobalIllumination;

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

    //  �G���
    EnemySetting enemySetting;

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

    //  EnemyData�N���X����̏��擾�p
    EnemyData enemyData;

    //  �_�ł����邽�߂�SpriteRenderer
    SpriteRenderer sp;
    //  �_�ł̊Ԋu
    private float flashInterval;
    //  �_�ł�����Ƃ��̃��[�v�J�E���g
    private int loopCount;


    private async UniTask Start()
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
        flashInterval = 0.01f;

        //  �G���̃A�T�[�V�����i�������Ȃ���΂����Ȃ������j
        Assert.IsTrue(enemyType.ToString() != ENEMY_TYPE.None.ToString(),
            "EnemyType���C���X�y�N�^�[�Őݒ肳��Ă��܂���I");

        //GameObject���j�����ꂽ���ɃL�����Z�����΂��g�[�N�����쐬
        var token = this.GetCancellationTokenOnDestroy();

        //  SpriteRender���擾
        sp = GetComponent<SpriteRenderer>();

        //  �G�f�[�^���擾
        enemySetting = await Addressables.LoadAssetAsync<EnemySetting>("EnemySetting")
            .WithCancellation(token);
        enemyData = enemySetting.DataList
            .FirstOrDefault(enemy => enemy.Id == enemyType.ToString() );

        //  �̗͂�ݒ�
        hp = enemyData.Hp;

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

         // ���
         if(enemySetting != null)
        {
            Addressables.Release(enemySetting);
            enemySetting = null;
        }
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
    public void SetHp(float health){ hp = health; }
    public float GetHp(){ return hp; }

    //  �G�ɓ��������甚������
    //  �����蔻��̊�b�m���F
    //  �����蔻����s���ɂ́A
    //  �E���҂�Collider�����Ă���
    //  �E�ǂ��炩��RigidBody�����Ă���
    private async void OnTriggerEnter2D(Collider2D collision)
    {
        if(!visible || bSuperMode || bDeath)return;

        if (collision.CompareTag("DeadWall"))
        {
            Destroy(this.gameObject);
        }
        else if (collision.CompareTag("NormalBullet"))
        {
            //  �e�̏���
            Destroy(collision.gameObject);

            //  �_���[�W����
            float d = GameManager.Instance.GetPlayer()
                .GetComponent<PlayerShotManager>().GetNormalShotPower();
            Damage(d);

            //  �_�ŉ��o
            var task = Blink();
            await task;

            //  ���S�t���OON
            if(hp <= 0)
            {
                bDeath = true;
                Death();       //  ���ꉉ�o
            }
        }
        else if (collision.CompareTag("ConverterBullet"))
        {
            //  �_���[�W����

            //  �_�ŉ��o
            var task = Blink();
            await task;

            //  ���S�t���OON
            if(hp <= 0)
            {
                bDeath = true;
                Death2();       //  ���ꉉ�o2
            }
        }

    }

    //  �����𐶐�
    private void DropMoneyItems(int money)
    {
        DropItems drop = this.GetComponent<DropItems>();
        if(!drop)return;

        //  ���A�C�e�����h���b�v������
        drop.DropKon(money);
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
            await UniTask.Delay(TimeSpan.FromSeconds(flashInterval))
                .AttachExternalCancellation(token);

            //spriteRenderer���I��
            sp.enabled = true;
        }
        //  ���G���[�hOFF
        bSuperMode = false;
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
        DropMoneyItems(enemyData.Money);

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
        DropMoneyItems(2 * dropMoney);

        //  �I�u�W�F�N�g���폜
        Destroy(this.gameObject);
    }


}
