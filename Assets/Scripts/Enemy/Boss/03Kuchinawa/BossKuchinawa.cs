using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Unity.VisualScripting;
using UnityEngine.Timeline;
using UnityEngine.VFX;
using System.Linq;

//--------------------------------------------------------------
//
//  �{�X�E�N�`�i���̃N���X
//
//--------------------------------------------------------------
public class BossKuchinawa : BossBase
{
    //------------------------------------------------------------
    //  Phase2�p
    //------------------------------------------------------------


    //------------------------------------------------------------
    //  Phase3�p
    //------------------------------------------------------------

    //  �I�u�W�F�N�g���X�g
    public enum eObjectList
    {
        Wave01,     //  �g��P
        Wave02,     //  �g��Q
        Circle,     //  ���@�w
        Beam,       //  �r�[��
        Signal,     //  �V�O�i��

        Max
    }

    //  Wave01
    [SerializeField] GameObject wave01Prefab;
    private GameObject wave01Object;
    //  Wave02
    [SerializeField] GameObject wave02Prefab;
    private GameObject wave02Object;
    //  Circle
    [SerializeField] GameObject magicCirclePrefab;
    private GameObject magicCircleObject;
    //  Beam
    [SerializeField] GameObject beamPrefab;
    private GameObject beamObject;
    //  PlayableDirector
    private PlayableDirector director;
    private bool isTimelineFinished; // Director�I���t���O
    Coroutine coroutine;

    /// <summary>
    /// ������
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        //  ���O�������Őݒ肵�Ȃ���EnemyData�̓ǂݍ��݂Ɏ��s����
        boss_id = "Kuchinawa";
    }

    protected override void Start()
    {
        base.Start();

        if (PlayableDirectorManager.Instance == null)
        {
            Debug.LogError("PlayableDirectorManager �̃C���X�^���X�� null �ł��I");
            return;
        }

        director = PlayableDirectorManager.Instance.GetDirector();

        if (director == null)
        {
            Debug.LogError("PlayableDirector ��������܂���I");
            return;
        }


        //  �^�C�����C���I�����̃C�x���g��o�^
        director.stopped += OnTimelineStopped;

        //  �s���J�n
        StartCoroutine(StartAction());
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        director.Stop();           //  Director���~

        director.stopped -= OnTimelineStopped;
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
            //StartCoroutine(WildlyShot(7.0f));
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
        float Degree = totalDegree / (wayNum - 1);     //  �e�ꔭ���ɂ��炷�p�x
        float chainInterval = 0.03f;    //  �A�e�̊Ԋu�i�b�j
        float AttackInterval = 0.5f;    //  �e�����̊Ԋu�i�b�j
        Vector3[] vector = new Vector3[wayNum];

        //-----------------------------------------------
        //  �E���獶�փo���}�L
        //-----------------------------------------------
        for (int i = 0; i < wayNum; i++)
        {
            Vector3 vector0 = Quaternion.Euler(0, 0, 90) * -transform.up;

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

            if (i == 0)
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
            Vector3 vector0 = Quaternion.Euler(0, 0, -90) * -transform.up;

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

            if (i == 0)
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
            Vector3 vectorR = Quaternion.Euler(0, 0, 90) * -transform.up;
            Vector3 vectorL = Quaternion.Euler(0, 0, -90) * -transform.up;

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

            if (i == 0)
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

                vector[i] = Quaternion.Euler(0, 0, -Degree + (i * Degree)) * vector0;
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
        for (int i = 3; i < 3 + (wayNum + 1); i++)
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
            if (targetNum.Count > 6/*�ڕW���W�̐�*/- wayNum)
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
        //  �r�[�����ˁI
        Play();

        yield return new WaitForSeconds(duration);
    }

    //------------------------------------------------------------------
    //  �����ړ�
    //------------------------------------------------------------------
    protected override IEnumerator LoopMoveBerserk(float duration, float interval)
    {
        int currentlNum = (int)Control.Left;       //  ���݈ʒu
        List<int> targetList = new List<int>();    //  �ڕW�ʒu��⃊�X�g
        int targetNum = (int)Control.Right;        //  �ڕW�ʒu

        Vector3 vec = Vector3.down;     //  �e�̃x�N�g��

        //  ���݈ʒu�����߂�i��ԋ߂��ʒu�Ƃ���j
        Vector3 p1 = EnemyManager.Instance.GetControlPointPos((int)Control.Left);
        Vector3 p2 = EnemyManager.Instance.GetControlPointPos((int)Control.Right);
        float d1 = Vector3.Distance(p1, this.transform.position);
        float d2 = Vector3.Distance(p2, this.transform.position);
        List<float> dList = new List<float>();
        dList.Clear();
        dList.Add(d1);
        dList.Add(d2);

        //  ���ёւ�
        dList.Sort();

        if (dList[0] == d1) currentlNum = (int)Control.Left;
        if (dList[0] == d2) currentlNum = (int)Control.Right;

        //  ���X�g���N���A
        targetList.Clear();

        //  �ڕW�̔ԍ���ݒ�
        if (currentlNum == (int)Control.Left)
        {
            targetList.Add((int)Control.Right);
        }
        else if (currentlNum == (int)Control.Right)
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

                vector[i] = Quaternion.Euler(0, 0, -Degree + i * Degree) * vector0;
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

        //  3�b�҂�
        yield return new WaitForSeconds(1.0f);
    }

    /// <summary>
    ///     ���f���𐶐����ăA�j���[�V�������Đ�
    /// </summary>
    /// <returns></returns>
	public IEnumerator PlayAnimation()
    {
        //  �v���C�O�Ƀt���O�����Z�b�g
        isTimelineFinished = false;

        //  ���W��ݒ肵�Ă���
        Vector3 centerPos = this.transform.position;
        Vector3 target1 = centerPos + new Vector3(0, -2, 0);

        //  �g��P�𐶐�
        wave01Object = Instantiate(wave01Prefab);
        wave01Object.transform.position = target1;
        Animator animator01 = wave01Object.GetComponent<Animator>();

        //  �g��2�𐶐�
        wave02Object = Instantiate(wave02Prefab);
        wave02Object.transform.position = target1;
        Animator animator02 = wave02Object.GetComponent<Animator>();

        //  ���@�w�𐶐�
        magicCircleObject = Instantiate(magicCirclePrefab);
        magicCircleObject.transform.position = target1;
        Animator animator03 = magicCircleObject.GetComponent<Animator>();

        //  �r�[���𐶐�
        beamObject = Instantiate(beamPrefab);
        beamObject.transform.position = target1;
        Vector3 target2 = centerPos + new Vector3(0, -9, 0);
        beamObject.transform.position = target2;

        //  �V�����Q�[���I�u�W�F�N�g�𐶐����đS�Ă̐e�I�u�W�F�N�g�ɂ���
        GameObject parent = new GameObject("���y�o�X�^�[");
        wave01Object.transform.SetParent(parent.transform);
        wave02Object.transform.SetParent(parent.transform);
        magicCircleObject.transform.SetParent(parent.transform);
        beamObject.transform.SetParent(parent.transform);

        //  �e�I�u�W�F�N�g��e���X�g�ɒǉ�
        AddBulletFromList(parent);

        // // ����Ă���Animator��PlayableDirector�Ƀo�C���h
        Bind(animator01, (int)eObjectList.Wave01);
        Bind(animator02, (int)eObjectList.Wave02);
        Bind(animator03, (int)eObjectList.Circle);
        Bind(null, (int)eObjectList.Beam);
        Bind(null, (int)eObjectList.Signal);

        // �Đ����āA�Đ��I����ɐ��������I�u�W�F�N�g����������
        director.Play();

        //  Director�̍Đ��I���҂�
        yield return new WaitUntil(() => isTimelineFinished == true);

        coroutine = null;

        Destroy(parent);
    }

    /// <summary>
    /// /   Director��Animator���o�C���h
    /// </summary>
    /// <param name="animator"></param>
	void Bind(Animator animator, int streamId)
    {
        // Timeline����CharacterAnimationTrack�̃g���b�N�ւ̎Q�Ƃ��擾����
        // ��������Animator���𗬂�����
        string[] streamName =
        {
            "AnimationTrackWave01",
            "AnimationTrackWave02",
            "AnimationTrackCircle",
            "Visual Effect Control Track",
            "Signal Track",
        };


        //  �G���[����
        if (streamName.Length != (int)eObjectList.Max)
        {
            Debug.LogError("streamName��ShaderEffect�̗v�f������v���܂���I");
        }


        //  �X�g���[�������������Ċi�[
        PlayableBinding binding = director.playableAsset.outputs.First(c => c.streamName == streamName[streamId]);


        //  �o�C���h
        if (!binding.IsUnityNull())
        {
            if(streamId == (int)eObjectList.Signal)
            {
                director.SetGenericBinding(binding.sourceObject, this.GetComponent<SignalReceiver>());
            }
            else if(streamId == (int)eObjectList.Beam)
            {
                director.SetGenericBinding(binding.sourceObject, beamObject.GetComponent<VisualEffect>());
            }
            else 
            {
                director.SetGenericBinding(binding.sourceObject, animator);
            }
        }

    }

    /// <summary>
    /// �Đ�
    /// </summary>
	public void Play()
    {
        if (coroutine == null)
            coroutine = StartCoroutine(PlayAnimation());
    }

    /// <summary>
    ///     PlayableDirector �̏I�������m
    /// </summary>
    void OnTimelineStopped(PlayableDirector pd)
    {
        if (pd == director)
        {
            isTimelineFinished = true; // `isTimelineFinished` �� true �ɂ���
            Debug.Log("Timeline����~���܂���");
        }
    }

    /// <summary>
    /// TimeLine���狅�̃q�b�g�{�b�N�X�L�������󂯎�������̏���
    /// </summary>
    public void OnCircleColliderEnabled()
    {
        if (beamObject) beamObject.GetComponent<CircleCollider2D>().enabled = true;
    }

    /// <summary>
    /// TimeLine����r�[�������蔻��L�������󂯎�������̏���
    /// </summary>
    public void OnPolygonColliderEnabled()
    {
        if (beamObject) beamObject.GetComponent<PolygonCollider2D>().enabled = true;
    }

    /// <summary>
    /// �S�����蔻�薳�������󂯎�������̏���
    /// </summary>
    public void OnAllColliderDisabled()
    {
        if (beamObject)
        {
            beamObject.GetComponent<CircleCollider2D>().enabled = false;
            beamObject.GetComponent<PolygonCollider2D>().enabled = false;
        }
    }
}
