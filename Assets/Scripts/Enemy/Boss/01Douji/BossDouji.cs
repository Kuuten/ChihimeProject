using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


//--------------------------------------------------------------
//
//  �{�X�E�h�E�W�̃N���X
//
//--------------------------------------------------------------
public class BossDouji : BossBase
{
    //  �M�~�b�N�e�̎g�p�ςݔԍ��i�[�p
    private int[] kooniNum = new int[(int)DoujiPhase2Bullet.KooniDirection.MAX];

    //------------------------------------------------------------
    //  Phase2�p
    //------------------------------------------------------------
    //  WARNING���̗\�����C��
    private GameObject[] dangerLineObject;

    /// <summary>
    /// ������
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        //  ���O�������Őݒ肵�Ȃ���EnemyData�̓ǂݍ��݂Ɏ��s����
        boss_id = "Douji";
    }

    protected override void Start()
    {
        base.Start();

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

        //  �s���J�n
        StartCoroutine(StartAction());
    }

    /// <summary>
    /// �X�V
    /// </summary>
    protected override void Update()
    {
        base.Update();
    }

    //******************************************************************
    //
    //  �ړ��p�^�[��
    //
    //******************************************************************

    /// <summary>
    ///  Phase1
    /// </summary>
    protected override IEnumerator Phase1()
    {
        Debug.Log("�t�F�[�Y�P�J�n");

        //  �t�F�[�Y�P
        while (!bStopPhase1)
        {
            yield return StartCoroutine(LoopMove(1.5f, 0.5f));

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

    /// <summary>
    ///  Phase2
    /// </summary>
    protected override IEnumerator Phase2()
    {
        Debug.Log("�t�F�[�Y�Q�ֈڍs");

        //  �t�F�[�Y�Q
        while (!bStopPhase2)
        {
            StartCoroutine(WildlyShotSmall());

            //  Warning!(����̂�)
            yield return StartCoroutine(Warning());

            StartCoroutine(KooniParty());
            StartCoroutine(KooniParty());
            StartCoroutine(KooniParty());

            yield return StartCoroutine(KooniParty());

            yield return StartCoroutine(LoopMove(1.0f,1.0f));
        }
    }

    /// <summary>
    ///  Phase3
    /// </summary>
    protected override IEnumerator Phase3()
    {
        Debug.Log("�t�F�[�Y�R�ֈڍs");

        //  �t�F�[�Y�R
        while (true)
        {
            yield return StartCoroutine(LoopMoveBerserk(0.6f, 1.0f));

            yield return StartCoroutine(BerserkBarrage());
            yield return StartCoroutine(BerserkGatling());

            yield return StartCoroutine(LoopMoveBerserk(0.6f, 1.0f));

            yield return StartCoroutine(BerserkBarrage());
            yield return StartCoroutine(BerserkGatling());
        }
    }

    //------------------------------------------------------------------
    //  �ʏ�U���p�^�[��(Phase1)
    //------------------------------------------------------------------
     protected override IEnumerator Shot()
    {
        int rand = Random.Range(0,100);

        //  �����m����臒l�ŌĂѕ�����
        if (rand <= 49f)
        {
            yield return StartCoroutine(WildlyShot(7.0f));

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
    protected override IEnumerator WildlyShotSmall()
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
    protected override IEnumerator WildlyShot(float speed)
    {
        //  �ʏ�o���}�L�e�̃v���n�u���擾
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Wildly_Big);

        float totalDegree = 360;        //  ���͈͂̑��p  
        int wayNum = 9;                 //  �e��way��(�K��3way�ȏ�̊�ɂ��邱��)
        float Degree = totalDegree / (wayNum-1);     //  �e�ꔭ���ɂ��炷�p�x         
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
    protected override IEnumerator SnipeShot()
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
    protected override IEnumerator OriginalShot()
    {
        //  �e�̃v���n�u���擾
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
    //  �����e��������
    //------------------------------------------------------------------
    protected override IEnumerator GenerateBerserkBullet(float duration)
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
    //  �����e
    //------------------------------------------------------------------
    protected override IEnumerator LoopMoveBerserk(float duration,float interval)
    {
        int currentlNum = (int)Control.Left;       //  ���݈ʒu
        List<int> targetList = new List<int>();    //  �ڕW�ʒu��⃊�X�g
        int targetNum = (int)Control.Right;        //  �ڕW�ʒu
        int bulletNum = 3;

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
    //  �����e��
    //------------------------------------------------------------------
    private IEnumerator BerserkBarrage()
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
    protected override IEnumerator BerserkGatling()
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
    }
}
