using DG.Tweening;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

//--------------------------------------------------------------
//
//  �{�X�Ɋ�{�̃N���X
//
//--------------------------------------------------------------
public class BossBase : MonoBehaviour
{
    //  EnemyData�N���X����̏��擾�p
    protected EnemyData enemyData;
    
    //  �p�����[�^
    protected string boss_id;         //  EnemyData����ID
    protected float hp;
    protected bool bDeath;            //  ���S�t���O
    protected bool bSuperMode;        //  ���G���[�h�t���O
    protected bool bSuperModeInterval;//  �t�F�[�Y�؂�ւ����̖��G���[�h�t���O

    //  �_�ł����邽�߂�SpriteRenderer
    protected SpriteRenderer sp;
    //  �_�ł̊Ԋu
    protected float flashInterval;
    //  �_�ł�����Ƃ��̃��[�v�J�E���g
    protected int loopCount;

    //  ����G�t�F�N�g
    [SerializeField,ShowAssetPreview]
    public GameObject explosion;

    //  HP�X���C�_�[
    protected Slider hpSlider;

    //  ����_
    protected enum Control
    {
        Left,
        Center,
        Right,

        Max
    };

    //  �h���b�v�p���[�A�b�v�A�C�e���ꗗ
    protected ePowerupItems powerupItems;

    //  �e�I�u�W�F�N�g�̃��X�g
    protected List<GameObject> bulletList;
    //  �e�I�u�W�F�N�g�̃R�s�[���X�g
    protected List<GameObject> copyBulletList;

    //  �R���[�`����~�p�t���O
    protected Coroutine phase1_Coroutine;
    protected Coroutine phase2_Coroutine;
    protected bool bStopPhase1;
    protected bool bStopPhase2;

    protected const float phase1_end = 0.66f;   //  �t�F�[�Y�P�I��������HP������臒l
    protected const float phase2_end = 0.33f;   //  �t�F�[�Y�Q�I��������HP������臒l
    
    /// <summary>
    /// ������
    /// </summary>
    protected virtual void Awake()
    {
        boss_id = "";

        //  �t���O������
        bStopPhase1 = false;
        bStopPhase2 = false;

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

        //  �e�̃��X�g
        bulletList = new List<GameObject>();

        //���P���ɑ������ƎQ�Ɠn���i�ύX�����݂ɍ�p����j�ɂȂ�R�s�[�̈Ӗ����Ȃ��Ȃ邪�A
        //�@��������ăR���X�g���N�^�̈����Ń��X�g��n���Βl�n���i�����f�[�^�����ʕ��j�ɂȂ�
        copyBulletList = new List<GameObject>(bulletList);
    }

    protected virtual void Start()
    {

    }

    /// <summary>
    /// �X�V
    /// </summary>
    protected virtual void Update()
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

        //  �e���X�g���Ď����ċ�Ȃ�폜        {
        DeleteBulletFromList();
        
        //  �X���C�_�[���X�V
        hpSlider.value = hp / enemyData.Hp;
    }

    /// <summary>
    /// �I�u�W�F�N�g���j�����ꂽ���̏���
    /// </summary>
    protected void OnDestroy()
    {
        Debug.Log("�{�X���j�I�X�e�[�W�N���A�I");

        //  �e��S�폜
        DeleteAllBullet();

        //  �{�X����ꂽ��X�e�[�W�N���A
        GameManager.Instance.SetStageClearFlag(true);
    }

    /// <summary>
    ///  �G�̃f�[�^��ݒ� 
    /// </summary>
    /// <param name="es">�G�ݒ�t�@�C��</param>
    /// <param name="item">�h���b�v�A�C�e��</param>
    public void SetBossData(EnemySetting es, ePowerupItems item)
    {
        //  �G�̃f�[�^��ݒ� 
        if(boss_id == "")Debug.LogError("boss_id����ɂȂ��Ă��܂��I");
        else Debug.Log($"boss_id:{boss_id}�Őݒ肪�������܂����I");


        Debug.Log($"boss_id: {boss_id}");


        //  ID�Ńf�[�^��enemyData�ɐݒ�
        enemyData = es.DataList
            .FirstOrDefault(enemy => enemy.Id == boss_id );
        if(enemyData == null)Debug.LogError("enemyData�̎擾�Ɏ��s���܂���" +
            "�I\n�{�X�h���N���X��boss_id���m�F���Ă�������");
        else Debug.Log("enemyData�̐ݒ肪�������܂����I");


        //  �̗͂�ݒ�
        if(enemyData.Hp <= 0)Debug.LogError("�{�XHP���ŏ�����0�ȉ��ɐݒ肳��Ă��܂��I");
        else Debug.Log("�{�X��HP�̐ݒ肪�������܂����I");
        hp = enemyData.Hp;

        Debug.Log( "�^�C�v: " + boss_id + "\nHP: " + hp );

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
    protected IEnumerator PlayDamageSFXandSuperModeOff()
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
    protected void OnTriggerEnter2D(Collider2D collision)
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
    protected void Death()
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
    protected void Death2()
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

    //------------------------------------------------
    //  �I�u�W�F�N�g����ɂȂ��Ă����烊�X�g����폜
    //------------------------------------------------
    public void DeleteBulletFromList()
    {
        //  �R�s�[�Ń��[�v����
        foreach(GameObject bullet in copyBulletList)
        {
            if(bullet == null)
            {
                //  �{�̂̃��X�g����폜
                bulletList.Remove(bullet);
            } 
        }
    }

    //------------------------------------------------
    //  �e��S�폜
    //------------------------------------------------
    public void DeleteAllBullet()
    {
        foreach(GameObject obj in bulletList)
        {
            if(obj)Destroy(obj);
        }
    }

    //------------------------------------------------
    //  ���X�g�ɓG�I�u�W�F�N�g��ǉ�
    //------------------------------------------------
    public void AddBulletFromList(GameObject obj)
    {
        if(obj != null)
        {
            bulletList.Add(obj);
        }
        else
        {
            Debug.LogError("��̃I�u�W�F�N�g�������Ɏw�肳��Ă��܂��I");
        }
    }

    //******************************************************************
    //
    //  �ړ��p�^�[��
    //
    //******************************************************************

    //------------------------------------------------------------------
    //  �s���Ǘ��֐�
    //------------------------------------------------------------------
    protected IEnumerator StartAction()
    {
        Debug.Log("***�e���t�F�[�Y�J�n�I***");

        //  �t�F�[�Y�P�J�n
        phase1_Coroutine = StartCoroutine(Phase1());

        //  �t���O��TRUE�ɂȂ�܂ő҂�
        yield return new WaitUntil(()=>bStopPhase1 == true);

        //  �t���O�ŃR���[�`����~
        if(phase1_Coroutine != null)StopCoroutine(phase1_Coroutine);

        //  �t�F�[�Y�ύX
        yield return StartCoroutine(PhaseChange());

        //  �t�F�[�Y�Q�J�n
        phase2_Coroutine = StartCoroutine(Phase2());

        //  �t���O��TRUE�ɂȂ�܂ő҂�
        yield return new WaitUntil(()=>bStopPhase2 == true);

        //  �t���O�ŃR���[�`����~
        if(phase2_Coroutine != null)StopCoroutine(phase2_Coroutine);

        //  �t�F�[�Y�ύX
        yield return StartCoroutine(PhaseChange());

        //  �t�F�[�Y�R�J�n
        StartCoroutine(Phase3());
    }

    //------------------------------------------------------------------
    //  �t�F�[�Y�`�F���W���̍s��
    //------------------------------------------------------------------
    protected IEnumerator PhaseChange()
    {
        //  �ړ��ɂ����鎞��(�b)
        float duration = 1.5f;
        //  �ړ���ɑҋ@���鎞��(�b)
        float interval = 5.0f;

        //  �^�񒆂Ɉړ�����
        yield return StartCoroutine(MoveToCenterOnPhaseChange(duration));

        //  ���̃t�F�[�Y�܂ő҂�
        yield return new WaitForSeconds(interval);

        //  ���G���[�hOFF
        bSuperModeInterval = false;
    }

    /// <summary>
    ///  �c�N����Phase1
    /// </summary>
    protected virtual IEnumerator Phase1()
    {
        yield return null;
    }

    /// <summary>
    ///  �c�N����Phase2
    /// </summary>
    protected virtual IEnumerator Phase2()
    {
        yield return null;
    }

    /// <summary>
    ///  �c�N����Phase3
    /// </summary>
    protected virtual IEnumerator Phase3()
    {
        yield return null;
    }

    //------------------------------------------------------------------
    //  �ړ�
    //------------------------------------------------------------------
    protected virtual IEnumerator LoopMove(float duration,float interval)
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
    //  �^�񒆂ւ̈ړ�
    //------------------------------------------------------------------
    protected virtual IEnumerator MoveToCenter()
    {
        float duration = 1.0f;   // �ړ��ɂ����鎞��
        int controlPointId = 1;  // �����̃R���g���[���|�C���g

        //  �h�^�񒆂̃R���g���[���|�C���g��ڕW�Ƃ���
        Vector3 targetPos = EnemyManager.Instance.GetControlPointPos(controlPointId);

        //  �ړ��J�n
        transform.DOMove(targetPos, duration);

        //  �ړ����ԑ҂�
        yield return new WaitForSeconds(duration);
    }

    //------------------------------------------------------------------
    //  �t�F�[�Y�̐؂�ւ����Ƀ{�X���^�񒆂Ɉړ�����
    //------------------------------------------------------------------
    protected virtual IEnumerator MoveToCenterOnPhaseChange(float duration)
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
    //  �ʏ�U���p�^�[��(Phase1)
    //------------------------------------------------------------------
    protected virtual IEnumerator Shot()
    {
        yield return null;        
    }

    //------------------------------------------------------------------
    //  �o���}�L�e(��)
    //------------------------------------------------------------------
    protected virtual IEnumerator WildlyShotSmall()
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
    //  �o���}�L�e
    //------------------------------------------------------------------
    protected virtual IEnumerator WildlyShot(float speed)
    {
        yield return null;
    }

    //------------------------------------------------------------------
    //  ���@�_���e
    //------------------------------------------------------------------
    protected virtual IEnumerator SnipeShot()
    {
        yield return null;
    }

    //------------------------------------------------------------------
    //  �I���W�i���e�E�z�[�~���O�V���b�g
    //------------------------------------------------------------------
    protected virtual IEnumerator OriginalShot()
    {
        yield return null;
    }

    //-------------------------------------------------------------------
    //  �����e��������
    //------------------------------------------------------------------
    protected virtual IEnumerator GenerateBerserkBullet(float duration)
    {
        yield return null;
    }

    //------------------------------------------------------------------
    //  �����ړ�
    //------------------------------------------------------------------
    protected virtual IEnumerator LoopMoveBerserk(float duration,float interval)
    {
        yield return null;
    }

    //------------------------------------------------------------------
    //  �����K�g�����O�V���b�g
    //------------------------------------------------------------------
    protected virtual IEnumerator BerserkGatling()
    {
        yield return null;
    }

}
