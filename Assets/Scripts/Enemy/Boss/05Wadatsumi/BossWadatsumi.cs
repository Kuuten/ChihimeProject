using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnemyManager;

//--------------------------------------------------------------
//
//  �{�X�E���_�c�~�̃N���X
//
//--------------------------------------------------------------
public class BossWadatsumi : BossBase
{
    //------------------------------------------------------------
    //  Phase2�p
    //------------------------------------------------------------


    //------------------------------------------------------------
    //  Phase3�p
    //------------------------------------------------------------


    /// <summary>
    /// ������
    /// </summary>
     protected override void Awake()
    {
        base.Awake();

        //  ���O�������Őݒ肵�Ȃ���EnemyData�̓ǂݍ��݂Ɏ��s����
        boss_id = "Wadatsumi";
    }

    protected override void Start()
    {
        base.Start();



        //  �s���J�n
        StartCoroutine(StartAction());
    }

    //*********************************************************************************
    //
    //  �X�V
    //
    //*********************************************************************************
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
            //  Warning!(����̂�)
            yield return StartCoroutine(Warning());
            StartCoroutine(WildlyShot(7.0f));
            //yield return StartCoroutine(ShoujiKekkai());
            yield return StartCoroutine(LoopMove(1.0f, 1.0f));
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
            //StartCoroutine(BerserkFireworks());
            yield return StartCoroutine(GenerateBerserkBullet(2.5f));
            yield return StartCoroutine(LoopMoveBerserk(0.6f, 3.0f));
            StartCoroutine(GenerateBerserkBullet(2.5f));
            yield return StartCoroutine(WildlyShot(9.0f));
            yield return StartCoroutine(MoveToCenter());
        }
    }

    //------------------------------------------------------------------
    //  �ʏ�U���p�^�[��(Phase1)
    //------------------------------------------------------------------
    protected override IEnumerator Shot()
    {
        yield return StartCoroutine(WildlyShot(7.0f));

        //yield return StartCoroutine(SnipeShot());

        //yield return StartCoroutine(OriginalShot());

      
    }

    //------------------------------------------------------------------
    //  �o���}�L�e
    //------------------------------------------------------------------
    protected override IEnumerator WildlyShot(float speed)
    {
        //  �ʏ�o���}�L�e�̃v���n�u���擾
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Wildly_Big);

        float totalDegree = 180;        //  ���͈͂̑��p  
        int wayNum = 15;                //  �e��way��(�K��3way�ȏ�̊�ɂ��邱��)
        float Degree = totalDegree / (wayNum-1);     //  �e�ꔭ���ɂ��炷�p�x         
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
            //  ���X�g�ɒǉ�
            AddBulletFromList(Bullet_obj);

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
            //  ���X�g�ɒǉ�
            AddBulletFromList(Bullet_obj);

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
            //  ���X�g�ɒǉ�
            AddBulletFromList(Bullet_objR);

            EnemyBullet enemyBulletR = Bullet_objR.GetComponent<EnemyBullet>();
            enemyBulletR.SetSpeed(speed);
            enemyBulletR.SetVelocity(vector[i]);
            enemyBulletR.SetPower(enemyData.Attack);

            vector[i] = Quaternion.Euler(0, 0, (i * Degree)) * vectorL;
            vector[i].z = 0f;

            GameObject Bullet_objL = 
                (GameObject)Instantiate(bullet, transform.position, transform.rotation);
            //  ���X�g�ɒǉ�
            AddBulletFromList(Bullet_objL);

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
                //  ���X�g�ɒǉ�
                AddBulletFromList(Bullet_obj);

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
    protected override IEnumerator OriginalShot()
    {
        //  �ʏ펩�@�_���e�̃v���n�u���擾
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Wildly_Big);

        //  �����_���ȃR���g���[���|�C���g�Ɍ������Ēe�����ł����A
        //  ��苗���܂ŋ߂Â��Ƃ��̎������]���n�߂�B
        //  ��莞�ԉ�]�����e�̓v���C���[�Ɍ������Ĕ��ł����B

        int wayNum = 5;                 //  ��x�Ɍ��e��
        float speed = 7.0f;             //  �e��
        float chainInterval = 2f;       //  �A�e�̊Ԋu�i�b�j
        float Interval = 3f;            //  ���̍s���܂ł̊Ԋu�i�b�j

        //  �ڕW�̃R���g���[���|�C���g�ԍ��i�[�p
        List<int> targetNum = new List<int>();

        //  3�`8�܂ŃZ�b�g
        for(int i=3;i<3+(wayNum+1);i++)
        {
            targetNum.Add(i);
        }

        /* �e�������� */
        for (int i = 0; i < wayNum; i++)
        {
            //  �e�C���X�^���X���擾���A�����Ɣ��ˊp�x��^����
            GameObject Bullet_obj =
                (GameObject)Instantiate(bullet, transform.position, transform.rotation);
            //  ���X�g�ɒǉ�
            AddBulletFromList(Bullet_obj);

            //  �e�Ƀf�t�H���g��EnemyBullet�R���|�[�l���g������̂ł�����폜����
            Destroy(Bullet_obj.GetComponent<EnemyBullet>());

            //  �����TsukumoHomingBullet�R���|�[�l���g��ǉ�����
            Bullet_obj.AddComponent<TsukumoHomingBullet>();

            //  3�`8�Ԃ܂ł��d���Ȃ��Ń����_���ɒ��o
            if(targetNum.Count > 6/*�ڕW���W�̐�*/-wayNum)
            {
                int index = Random.Range(0, targetNum.Count);
 
                int ransu = targetNum[index];

                //  �e�ɖڕW�ԍ����Z�b�g
                Bullet_obj.GetComponent<TsukumoHomingBullet>().SetTargetNum(ransu);
 
                targetNum.RemoveAt(index);
            }

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

        yield return new WaitForSeconds(Interval);
    }

    //-------------------------------------------------------------------
    //  �����e��������
    //------------------------------------------------------------------
    protected override IEnumerator GenerateBerserkBullet(float duration)
    {
        float speed = 7.0f;                     //  �e��
        float totalDegree = 360;                //  ���͈͂̑��p  
        int wayNum = 5;                         //  �e��way��
        float Degree = totalDegree / wayNum;    //  �e�ꔭ���ɂ��炷�p�x
        float bulletDistance = 2.0f;            //  �v���C���[�ƓW�J����e�̍ő勗��

        //  �����e�̃v���n�u���擾
        GameObject bullet = EnemyManager.Instance
            .GetBulletPrefab((int)BULLET_TYPE.Tsukumo_Berserk_Bullet);

        //  �c�N���̑O���x�N�g�����擾
        Vector3 vector0 = -transform.up;

        Vector3[] vector = new Vector3[wayNum];

        ////  �e�𐶐�
        //for (int i = 0; i < wayNum; i++)
        //{
        //    GameObject Bullet_obj = Instantiate(bullet,transform.position,Quaternion.identity);
        //    //  ���X�g�ɒǉ�
        //    AddBulletFromList(Bullet_obj);

        //    enemyPhase3Bullet = Bullet_obj.GetComponent<TsukumoPhase3Bullet>();
        //    enemyPhase3Bullet.SetParentTransform(transform);

        //    //  �x�N�g�����p�x�ŉ�
        //    vector[i] = Quaternion.Euler
        //        (0, 0, Degree * i) * vector0 * bulletDistance;
        //    vector[i].z = 0f;

        //    //  �e���ƍU���͂�ݒ�
        //    enemyPhase3Bullet.SetVec(vector[i]);
        //    enemyPhase3Bullet.SetSpeed(speed);
        //    enemyPhase3Bullet.SetPower(enemyData.Attack);

        //    //  SE���Đ�
        //    SoundManager.Instance.PlaySFX(
        //        (int)AudioChannel.ENEMY_SHOT,
        //        (int)SFXList.SFX_TSUKUMO_SHOT1);
        //}

        yield return new WaitForSeconds(duration);
    }

    //------------------------------------------------------------------
    //  �����ړ�
    //------------------------------------------------------------------
    protected override IEnumerator LoopMoveBerserk(float duration,float interval)
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

        //  ���݂̔ԍ����X�V
        currentlNum = targetNum;

        //  ���̈ړ��܂ő҂�
        yield return new WaitForSeconds(interval);
    }

    //------------------------------------------------------------------
    //  �����K�g�����O�V���b�g
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
                //  ���X�g�ɒǉ�
                AddBulletFromList(Bullet_obj);

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
