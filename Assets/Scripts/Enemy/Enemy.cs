using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;

//--------------------------------------------------------------
//
//  �U�R�G�̊�{�N���X
//
//--------------------------------------------------------------
public class Enemy : MonoBehaviour
{
    // �����̎��
    public enum MOVE_TYPE {
        Clamp,                      // �J�N�J�N�ړ�
        ChargeToPlayer,             // �v���C���[�ɓːi
        Straight,                   // �����ړ�
        AdjustLine,                 // �����킹
        Curve                       // ������
    }
    // �����̎��
    public MOVE_TYPE type = MOVE_TYPE.Clamp;

    //  �o���|�C���g
    [SerializeField] private GameObject[] spawner;
    //  ����_
    [SerializeField] private GameObject[] controlPoint;
    
    [SerializeField] private float moveSpeed = 1.0f;
    [SerializeField] private GameObject explosion;

    //  ���炦�邨��
    [SerializeField] private int money = 50;

    private float period = 20;  //  �G�̎����i�b�j

    private float cycleCount = 0.1f; // �P�b�Ԃɉ��������
    private float curveLength = 1;   // �J�[�u�̍ő勗��
    private float cycleRadian = 0;   // �T�C���ɓn���l
    private float centerX;           // X���W�̒��S

    private bool visible = false;

    //  �X�e�[�^�X
    private int Id;     //  ID
    private int Hp;     //  HP
    private int Attack; //  �U����
    private int Money;  //  ���Ƃ����z�i���j


    void Start()
    {
        visible = false;

        // ����Y���W���uX���W�̒��S�v�Ƃ��ĕۑ�
        centerX = transform.position.x;

        //  ������ݒ�
        Destroy(this.gameObject, period);

        //  �X�e�[�^�X��ݒ�(EnemySetting����擾)
        Id = -1;
        Hp = -1;
        Attack = -1;
        Money = -1;

        ////  �G�f�[�^���擾
        //enemySetting = await Addressables.LoadAssetAsync<EnemySetting>("EnemySetting");

        //var EnemyData = enemySetting.DataList
        //    .FirstOrDefault(enemy => enemy.Id == "Kooni");
        //Debug.Log($"ID�F{EnemyData.Id}");
        //Debug.Log($"HP�F{EnemyData.Hp}");
        //Debug.Log($"�U���́F{EnemyData.Attack}");
        //Debug.Log($"�����F{EnemyData.Money}");
    }

    void Update()
    {
        Vector3 pos = transform.position;

        //// �㉺�ɃJ�[�u
        //if (type == MOVE_TYPE.CURVE)
        //{
        //    if (cycleCount > 0)
        //    {
        //        cycleRadian += (cycleCount * 2 * Mathf.PI) / 50;
        //        pos.x = Mathf.Sin(cycleRadian) * curveLength + centerX;
        //    }
        //}

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

    //  �G�ɓ��������甚������
    //  �����蔻��̊�b�m���F
    //  �����蔻����s���ɂ́A
    //  �E���҂�Collider�����Ă���
    //  �E�ǂ��炩��RigidBody�����Ă���
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!visible)return;

        if(collision.CompareTag("Player"))
        {
            Instantiate(
                    explosion,
                    collision.transform.position,
                    collision.transform.rotation
                );
        }
        else if(collision.CompareTag("NormalBullet"))
        {
            //  DropItems������ꍇ�̓A�C�e���h���b�v
            DropItems drop = this.GetComponent<DropItems>();
            if(drop)drop.DropPowerupItem();

            //  ���͏�Ƀh���b�v

            //  �����𐶐�
            DropMoneyItems();

            //  �v���C���[�̂��������Z
            //scoreManager.AddMoney(100);

            


        }
        else if(collision.CompareTag("ConverterBullet"))
        {
            //  DropItems������ꍇ�̓A�C�e���h���b�v
            DropItems drop = this.GetComponent<DropItems>();
            if(drop)drop.DropPowerupItem();

            //  �����𐶐�

            //  �v���C���[�̂��������Z
            //scoreManager.AddMoney(100);

        }
        else return;

        //  ����
        Instantiate(explosion, transform.position, transform.rotation);
        
        Destroy(this.gameObject);
        Destroy(collision.gameObject);
    }

    //  �����𐶐�
    private void DropMoneyItems()
    {
        //  �G�̗��Ƃ����z���擾

        //  ���A�C�e�����h���b�v������
    }

    //  Wave�ړ�
    //private void WaveMove()
    //{
    //    // ���E�ɃE�F�[�u
    //    if (type == MOVE_TYPE.Wave)
    //    {
    //        if (cycleCount > 0)
    //        {
    //            cycleRadian += (cycleCount * 2 * Mathf.PI) / 50;
    //            pos.x = Mathf.Sin(cycleRadian) * curveLength + centerX;
    //        }
    //    }
    //}
}
