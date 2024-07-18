using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using System.Linq;
using Cysharp.Threading.Tasks;

//--------------------------------------------------------------
//
//  �U�R�G�̊�{�N���X
//
//--------------------------------------------------------------
public class Enemy : MonoBehaviour
{
    // �����̎��
    public enum MOVE_TYPE {
        None,                       // ���ݒ�
        Clamp,                      // �J�N�J�N�ړ�
        ChargeToPlayer,             // �v���C���[�ɓːi
        Straight,                   // �����ړ�
        AdjustLine,                 // �����킹
        Curve                       // ������
    }
    // �����̎��
    public MOVE_TYPE moveType = MOVE_TYPE.None;

    // �U�R�G�̎��(���O��EnemySetting��Id�Ɠ����ɂ���)
    public enum ENEMY_TYPE {
        None,                       //  ���ݒ�
        Kooni,                      //  �q�S
        Onibi,                      //  �S��
        Ibarakidouji,               //  ��ؓ��q
        Douji                       //  �h�E�W
    }
    // �����̎��
    public ENEMY_TYPE enemyType = ENEMY_TYPE.None;

    //  �G���
    EnemySetting enemySetting;

    //  �o���|�C���g(GameManager����Getter�Ŏ󂯎��)
    private GameObject[] spawner;
    //  ����_(GameManager����Getter�Ŏ󂯎��)
    private GameObject[] controlPoint;
    
    [SerializeField] private float moveSpeed = 1.0f;
    [SerializeField] private GameObject explosion;

    private float period = 20;  //  �G�̎����i�b�j

    private float cycleCount = 0.1f; // �P�b�Ԃɉ��������
    private float curveLength = 1;   // �J�[�u�̍ő勗��
    private float cycleRadian = 0;   // �T�C���ɓn���l
    private float centerX;           // X���W�̒��S

    private bool visible = false;

    //  EnemyData�N���X����̏��擾�p
    EnemyData enemyData;


    private async UniTask Start()
    {
        visible = false;

        // ����Y���W���uX���W�̒��S�v�Ƃ��ĕۑ�
        centerX = transform.position.x;

        //  ������ݒ�
        Destroy(this.gameObject, period);

        //  �G���̃A�T�[�V�����i�������Ȃ���΂����Ȃ������j
        Assert.IsTrue(enemyType.ToString() != ENEMY_TYPE.None.ToString(),
            "EnemyType���C���X�y�N�^�[�Őݒ肳��Ă��܂���I");

        //  �G�f�[�^���擾
        enemySetting = await Addressables.LoadAssetAsync<EnemySetting>("EnemySetting");
        enemyData = enemySetting.DataList
            .FirstOrDefault(enemy => enemy.Id == enemyType.ToString() );
        //Debug.Log($"ID�F{enemyData.Id}");
        //Debug.Log($"HP�F{enemyData.Hp}");
        //Debug.Log($"�U���́F{enemyData.Attack}");
        //Debug.Log($"�����F{enemyData.Money}");
    }

    private void OnDestroy()
    {
                // ���
        Addressables.Release(enemySetting);
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

    //----------------------------------------------------------------------
    //  �v���p�e�B
    //----------------------------------------------------------------------


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
            //  �_�ł�����

            //  �_���[�W����

            
            //  ����G�t�F�N�g
            Instantiate(explosion, transform.position, transform.rotation);

            //  �I�u�W�F�N�g���폜
            Destroy(this.gameObject);

            //  DropItems������ꍇ�̓A�C�e���h���b�v
            DropItems drop = this.GetComponent<DropItems>();
            if(drop)drop.DropPowerupItem();

            //  �����𐶐�
            DropMoneyItems(enemyData.Money);
        }
        else if(collision.CompareTag("ConverterBullet"))
        {
            //  �_�ł�����

            //  �_���[�W����

            //  ����G�t�F�N�g
            Instantiate(explosion, transform.position, transform.rotation);

            //  �I�u�W�F�N�g���폜
            Destroy(this.gameObject);

            //  DropItems������ꍇ�̓A�C�e���h���b�v
            DropItems drop = this.GetComponent<DropItems>();
            if(drop)drop.DropPowerupItem();

            //  �����𐶐�(���o�[�g�̎���2�{)
            int dropMoney = enemyData.Money;
            DropMoneyItems(2 * dropMoney);
        }
        else return;
        
        

        //  �v���C���[�̉����ꉉ�o
        Destroy(collision.gameObject);
    }

    //  �����𐶐�
    private void DropMoneyItems(int money)
    {
        DropItems drop = this.GetComponent<DropItems>();
        if(!drop)return;

        //  ���A�C�e�����h���b�v������
        drop.DropKon(money);
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
