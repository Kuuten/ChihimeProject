using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Enemy;


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
    private bool bDeath;        //  ���S�t���O
    private bool bSuperMode;    //  ���G���[�h�t���O

    //  �_�ł����邽�߂�SpriteRenderer
    SpriteRenderer sp;
    //  �_�ł̊Ԋu
    private float flashInterval;
    //  �_�ł�����Ƃ��̃��[�v�J�E���g
    private int loopCount;

    //  ����G�t�F�N�g
    [SerializeField] private GameObject explosion;

    //  ����_
    enum Control
    {
        Left,
        Center,
        Right,

        Max
    };

    void Start()
    {
        //  ���S�t���OOFF
        bDeath = false;
        //  �ŏ��͖��G���[�hOFF
        bSuperMode = false;
        //  ���[�v�J�E���g��ݒ�
        loopCount = 1;
        //  �_�ł̊Ԋu��ݒ�
        flashInterval = 0.2f;
        //  SpriteRender���擾
        sp = GetComponent<SpriteRenderer>();

        //  �s���J�n
        StartCoroutine(Douji_Action());
    }

    private void OnDestroy()
    {
        Debug.Log("�{�X���j�I�X�e�[�W�N���A�I");


        //  �{�X����ꂽ��X�e�[�W�N���A
        GameManager.Instance.SetStageClearFlag(true);
    }

    void Update()
    {
        
    }

    //  �G�̃f�[�^��ݒ� 
    public void SetBossData(EnemySetting es)
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
        if(bSuperMode || bDeath)
        {
            return;
        }

        if (collision.CompareTag("NormalBullet"))
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

            // �c��HP�\��
            Debug.Log("�c��HP: " + hp);

            //  ���S�t���OON
            if(hp <= 0)
            {
                bDeath = true;
                Death();                       //  ���ꉉ�o
            }
        }
        else if (collision.CompareTag("DoujiConvert"))
        {
            //  �_���[�W����
            float d = GameManager.Instance.GetPlayer()
                .GetComponent<PlayerShotManager>().GetConvertShotPower();
            Damage(d);

            //  �_�ŉ��o
            StartCoroutine(Blink(true,loopCount,flashInterval));

            //  �_���[�WSE�Đ�
            StartCoroutine(PlayDamageSFXandSuperModeOff());

            // �c��HP�\��
            Debug.Log("�c��HP: " + hp);

            //  ���S�t���OON
            if(hp <= 0)
            {
                bDeath = true;
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
                Death();                        //  ���ꉉ�o
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
    //  ���ꉉ�o(�ʏ�e�E�{��)
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
    //  �h�E�W�̈ړ��p�^�[��
    //
    //******************************************************************

    //------------------------------------------------------------------
    //  �h�E�W�̍U���Ǘ��֐�
    //------------------------------------------------------------------
    private IEnumerator Douji_Attack()
    {



        //  �ړ����ԑ҂�
        //yield return new WaitForSeconds(duration);

        Debug.Log("***�h�E�W�e���t�F�[�Y�J�n�I***");

        //  Phase1
        //yield return StartCoroutine(Douji_Phase1());

        //Debug.Log("***�h�E�W�M�~�b�N�t�F�[�Y�J�n�I***");

        //  Phase2
        //yield return StartCoroutine(Douji_Phase2());

        //Debug.Log("***�h�E�W�����t�F�[�Y�J�n�I***");

        //  Phase2
        //yield return StartCoroutine(Douji_Phase3());

        yield return null;
    }

    //------------------------------------------------------------------
    //  �h�E�W�̍s���Ǘ��֐�
    //------------------------------------------------------------------
    private IEnumerator Douji_Action()
    {
        Debug.Log("�t�F�[�Y�P�J�n");
        //  �t�F�[�Y�P
        while (true)
        {
            yield return StartCoroutine(Douji_LoopMove(1.5f,2.0f));

            //  HP���O���̓��؂����甲����
            if(hp <= enemyData.Hp*2/3)break;
        }
        Debug.Log("�t�F�[�Y�Q�ֈڍs");
        //  �t�F�[�Y�Q
        while (true)
        {
            yield return StartCoroutine(Douji_LoopMove(1.0f,1.5f));

            //  HP���O���̈��؂����甲����
            if(hp <= enemyData.Hp*1/3)break;
        }
        Debug.Log("�t�F�[�Y�R�ֈڍs");
        //  �t�F�[�Y�R
        while (true)
        {
            yield return StartCoroutine(Douji_LoopMove(1.0f,1.0f));
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

        Debug.Log("���ݔԍ�: " + currentlNum);
        Debug.Log("�ڕW�ԍ�: " + targetNum);

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
}
