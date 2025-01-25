using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using System;
using TMPro;
using DG.Tweening;

//  �{�X�̎��
public enum BossType
{
    Douji,
    Tsukumo,
    Kuchinawa,
    Kurama,
    Wadatsumi,
    Hakumen,

    Max
}

// �G�̒e�̎��
public enum BULLET_TYPE {
    //  �e
    Snipe_Normal,        //  ���@�_���e�E�ʏ�
    Snipe_RotationL,     //  ���@�_���e�E����]
    Snipe_RotationR,     //  ���@�_���e�E�E��]
    Snipe_Long,          //  ���@�_���e�E�����O
    Snipe_Big,           //  ���@�_���e�E���
    //  �Ԓe
    Wildly_Normal,       //  �o���}�L�e�E�ʏ�
    Wildly_RotationL,    //  �o���}�L�e�E����]
    Wildly_RotationR,    //  �o���}�L�e�E�E��]
    Wildly_Long,         //  �o���}�L�e�E�����O
    Wildly_Big,          //  �o���}�L�e�E���
    //  �h�E�W�M�~�b�N�e
    Douji_Gimmick_Top,
    Douji_Gimmick_Bottom,
    Douji_Gimmick_Left,
    Douji_Gimmick_Right,
    //  �M�~�b�N�x��
    Douji_Warning,
    //  �h�E�W�����e
    Douji_Berserk_Bullet,
    //  �x���̗\�����C��
    Douji_DangerLine_Top,
    Douji_DangerLine_Bottom,
    Douji_DangerLine_Left,
    Douji_DangerLine_Right,
    //  �c�N��Phase1�e
    Tsukumo_Gimmick_Top,
    Tsukumo_Gimmick_Bottom,
    Tsukumo_Gimmick_Left,
    Tsukumo_Gimmick_Right,
    //  �c�N��Phase2�M�~�b�N�e
    Tsukumo_Gimmick,
    //  �c�N�������e8
    Tsukumo_Berserk_Bullet,
    //  �c�N��Phase1�z�[�~���O�e
    Tsukumo_Phase1_Homing,
}

//--------------------------------------------------------------
//
//  �G�̊Ǘ��N���X
//
//--------------------------------------------------------------

public class EnemyManager : MonoBehaviour
{
    //  �V���O���g���ȃC���X�^���X
    public static EnemyManager Instance
    {
        get; private set;
    }

    // �G�̏o���Z�b�g(�S�X�e�[�W���ʋK�i)
    public enum EnemyPattern
    {
        E01,        //  �U�R�P
        E01_B,      //  �U�R�P�i�h���b�v�L��j
        E02,        //  �U�R�Q
        E02_B,      //  �U�R�Q�i�h���b�v�L��j
        MidBoss,    //  ���{�X
        EX,         //  �������Ⴂ��������

        Max
    }

    // ����ȓG�̏o���Z�b�g(�S�X�e�[�W���ʋK�i)
    public enum SpecialEnemyPattern
    {
        DollSiege,  // �l�`��͐w

        Max
    }

    //  �p���[�A�b�v�A�C�e���𗎂Ƃ����ǂ���
    public enum DROP_TYPE {
        None,                //  �Ȃ�
        Drop,                //  ����
    }

    [SerializeField] private GameObject[] enemyPrefab;
    [SerializeField] private GameObject[] specialEnemyPrefab;
    private const float appearY = -6.5f;


    private int enemyDestroyNum = 0;        //  �j�󂳂ꂽ�G��
    private bool endZakoStage;              //  �U�R�X�e�[�W�̏I���t���O

    //  �h���b�v���鏬���̃v���n�u
    private GameObject smallKonItem;
    AsyncOperationHandle<GameObject> loadHandleSmallKon;

    //  �h���b�v���鏬���̃v���n�u
    private GameObject largeKonItem;
    AsyncOperationHandle<GameObject> loadHandleLargeKon;

    //  �p���[�A�C�e���̃v���n�u�B
    private List<GameObject> powerupItems;
    AsyncOperationHandle<IList<GameObject>> loadHandlePowerup;

    //  �X�s�[�h�A�C�e���̃v���n�u�B
    private List<GameObject> speedupItems;
    AsyncOperationHandle<IList<GameObject>> loadHandleSpeedup;

    //  �{���A�C�e���̃v���n�u�B
    private List<GameObject> bombItems;
    AsyncOperationHandle<IList<GameObject>> loadHandleBomb;

    //  �q�[���A�C�e���̃v���n�u�B
    private List<GameObject> healItems;
    AsyncOperationHandle<IList<GameObject>> loadHandleHeal;

    //  �p���[�A�b�v�A�C�e���𗎂Ƃ����ǂ���
    private List<DROP_TYPE> dropType = new List<DROP_TYPE>();

    //  �G���
    private EnemySetting enemySetting;
    AsyncOperationHandle<IList<EnemySetting>> loadHandleEnemySetting;

    //  EnemyData�N���X����̏��擾�p
    private EnemyData enemyData;

    //  �G�̒e�v���n�u
    [SerializeField] private GameObject[] enemyBullet;

    //  �h���b�v�A�C�e���̃��[�h�����t���O
    private bool isCompleteLoading;

    //  �c�N���̃z�[�~���O�e�p
    [SerializeField] private SpriteRenderer homingBullet;
    [SerializeField] private Animator homingAnimator;

    //----------------------------------------------------
    //  �ړ��o�H�p�X�|�i�[������_�̃g�����X�t�H�[��
    //----------------------------------------------------
    [SerializeField] private Transform[] spawners;       //  �X�|�i�[
    [SerializeField] private Transform[] controlPoints;  //  ����_

    //  �C�x���g�V�[���}�l�[�W���[�I�u�W�F�N�g
    [SerializeField] private GameObject eventSceneManager;

    //  �{�X�̃L�����o�X
    [SerializeField] private GameObject bossCanavs;

    //  ���{�X��HP�X���C�_�[
    [SerializeField] private GameObject midBossSlider;

    //  �{�X��HP�X���C�_�[
    [SerializeField] private GameObject bossSlider;

    //  �f�B���N�V���i�����C�g
    [SerializeField] private Light directionalLight;

    //  WARNING���̃��C����`�悷��L�����o�X(�ʒu�擾�p)
    [SerializeField] private GameObject dangerLineCanvas;

    //  �G�I�u�W�F�N�g�̃��X�g
    List<GameObject> enemyObjList;

    //  HP�X���C�_�[�̃{�X��
    [SerializeField] private GameObject[] bossNameHPSlider;

    //  �X�e�[�W�J�n���ɕ\������e�L�X�g
    [SerializeField] private TextMeshProUGUI stageStartingText;
    private static readonly string[] stageName = 
    {
        "�X�e�[�W�P\n�S�̐��ގR",
        "�X�e�[�W�Q\n��\�㒷��",
        "�X�e�[�W�R\n���\��x�i�����Ƃ�[�遙",
        "�X�e�[�W�S\n�N���}�n�C�p�[�A���[�i",
        "�X�e�[�W�T\n���˂������̂�����",
        "�X�e�[�W�U\n�������",
    };

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private async void Start()
    {
        //appearStep = 0;
        //timer = 0;
        endZakoStage = false;
        enemyObjList = new List<GameObject>();
        isCompleteLoading = false;

        //  �G���̃��[�h
        //  ��EmemySetting�̓ǂݍ��݂܂ł͂����ł��
        //  EnemyData�ւ̐ݒ�͐�������Enemy�ɃZ�b�g����
        await LoadEnemySetting();

        Debug.Log("�G���t�@�C���̃��[�h����");

        //  �v���n�u�̃��[�h
        await LoadPrefab();

        Debug.Log("�h���b�v�A�C�e���̃��[�h����");

        isCompleteLoading = true;
    }

    //-----------------------------------------------------------------
    //  �G���̃��[�h
    //-----------------------------------------------------------------
    private async UniTask LoadEnemySetting()
    {
        enemySetting = new EnemySetting();

        //  �L�[�ɂ��G���̃��[�h
        await LoadEnemySettingByKey();
    }

    //-----------------------------------------------------------------
    //  �L�[�ɂ��G���̃��[�h
    //-----------------------------------------------------------------
    private async UniTask LoadEnemySettingByKey()
    {
        // ���[�h����ׂ̃L�[
        List<string> keys = new List<string>() {"EnemySetting"};

        //  �G�������[�h
        loadHandleEnemySetting = Addressables.LoadAssetsAsync<EnemySetting>
            (
                keys,
                null,
                Addressables.MergeMode.Union,
                false
            );

        //  ���[�h�̊�����҂�
        await loadHandleEnemySetting.Task;

        //  ���[�h���ʂ��i�[
        enemySetting = loadHandleEnemySetting.Result.First();

    }

    //-----------------------------------------------------------------
    //  �v���n�u�̃��[�h
    //-----------------------------------------------------------------
    private async UniTask LoadPrefab()
    {
        //smallKonItem = new GameObject();
        //largeKonItem = new GameObject();
        powerupItems = new List<GameObject>();
        speedupItems = new List<GameObject>();
        bombItems = new List<GameObject>();
        healItems = new List<GameObject>();

        //  �L�[�ɂ��v���n�u���[�h
        await LoadAssetsByAdrressablesKey();
    }

    //------------------------------------------------
    //  �L�[�ɂ��v���n�u���[�h
    //------------------------------------------------
    private async UniTask LoadAssetsByAdrressablesKey()
    {
        // ���[�h����ׂ̃L�[
        List<string> keys1 = new List<string>() {"SmallKon"};
        List<string> keys2 = new List<string>() {"LargeKon"};
        List<string> keys3 = new List<string>() {"PowerupItems"};
        List<string> keys4 = new List<string>() {"SpeedupItems"};
        List<string> keys5 = new List<string>() {"BombItems"};
        List<string> keys6 = new List<string>() {"HealItems"};

        //  ���������[�h
        loadHandleSmallKon = Addressables.LoadAssetAsync<GameObject>( "item_smallKon" );

        //  ���[�h�̊�����҂�
        await loadHandleSmallKon.Task;

        //  �印�����[�h
        loadHandleLargeKon = Addressables.LoadAssetAsync<GameObject>( "item_largeKon" );

        //  ���[�h�̊�����҂�
        await loadHandleLargeKon.Task;

        //  �p���[�A�b�v�A�C�e�������[�h
        loadHandlePowerup = Addressables.LoadAssetsAsync<GameObject>
            (
                keys3,
                null,
                Addressables.MergeMode.Union,
                false
            );

        //  ���[�h�̊�����҂�
        await loadHandlePowerup.Task;

        //  �X�s�[�h�A�b�v�A�C�e�������[�h
        loadHandleSpeedup = Addressables.LoadAssetsAsync<GameObject>
            (
                keys4,
                null,
                Addressables.MergeMode.Union,
                false
            );

        //  ���[�h�̊�����҂�
        await loadHandleSpeedup.Task;

        //  �{���A�C�e�������[�h
        loadHandleBomb = Addressables.LoadAssetsAsync<GameObject>
            (
                keys5,
                null,
                Addressables.MergeMode.Union,
                false
            );

        //  ���[�h�̊�����҂�
        await loadHandleBomb.Task;

        //  �q�[���A�C�e�������[�h
        loadHandleHeal = Addressables.LoadAssetsAsync<GameObject>
            (
                keys6,
                null,
                Addressables.MergeMode.Union,
                false
            );

        //  ���[�h�̊�����҂�
        await loadHandleHeal.Task;

        //  ���[�h�����������R�s�[
        if (loadHandleSmallKon.Result != null)
        {
            smallKonItem = loadHandleSmallKon.Result;
        }

        //  ���[�h�����印���R�s�[
        if (loadHandleLargeKon.Result != null)
        {
            largeKonItem = loadHandleLargeKon.Result;
        }

        //  �h���b�v�v�f�̐��������X�g�ɒǉ�
        foreach (var addressable in loadHandlePowerup.Result)
        {
            if (addressable != null)
            {
                powerupItems.Add( addressable );
            }
        }

        //  �h���b�v�v�f�̐��������X�g�ɒǉ�
        foreach (var addressable in loadHandleSpeedup.Result)
        {
            if (addressable != null)
            {
                speedupItems.Add( addressable );
            }
        }

        //  �h���b�v�v�f�̐��������X�g�ɒǉ�
        foreach (var addressable in loadHandleBomb.Result)
        {
            if (addressable != null)
            {
                bombItems.Add( addressable );
            }
        }

        //  �h���b�v�v�f�̐��������X�g�ɒǉ�
        foreach (var addressable in loadHandleHeal.Result)
        {
            if (addressable != null)
            {
                healItems.Add( addressable );
            }
        }

        Debug.Log("powerupItems: " + powerupItems.ToString());
        Debug.Log("speedupItems: " + speedupItems.ToString());
        Debug.Log("bombItems: " + bombItems.ToString());
        Debug.Log("healItems: " + healItems.ToString());

    }

    private void OnDestroy()
    {
        // ���
        try {
            Addressables.Release(loadHandleSmallKon);
            Addressables.Release(loadHandleLargeKon);
            Addressables.Release(loadHandlePowerup);
            Addressables.Release(loadHandleSpeedup);
            Addressables.Release(loadHandleBomb);
            Addressables.Release(loadHandleHeal);
            Addressables.Release(loadHandleEnemySetting);
        } catch (Exception e) {
            // ��O�������̏���
            Debug.Log("loadHandleSmallKon�����g�p�ł�");
            Debug.Log("loadHandleLargeKon�����g�p�ł�");
            Debug.Log("loadHandlePowerup�����g�p�ł�");
            Debug.Log("loadHandleSpeedup�����g�p�ł�");
            Debug.Log("loadHandleBomb�����g�p�ł�");
            Debug.Log("loadHandleHeal�����g�p�ł�");
            Debug.Log("loadHandleEnemySetting�����g�p�ł�");
            Debug.Log(e.Message);
        }

    }

    void Update()
    {
        
    }

    //------------------------------------------------
    //  �G������ł����烊�X�g����폜
    //------------------------------------------------
    public void DeleteEnemyFromList(GameObject obj)
    {
        if(obj == null)
        {
            enemyObjList.Remove(obj);
        } 
    }

    //------------------------------------------------
    //  �G��S�폜
    //------------------------------------------------
    public void DeleteAllEnemy()
    {
        foreach(GameObject obj in enemyObjList)
        {
            if(obj)Destroy(obj);
        }
    }

    //------------------------------------------------
    //  ���X�g�ɓG�I�u�W�F�N�g��ǉ�
    //------------------------------------------------
    public void AddEnemyFromList(GameObject obj)
    {
        if(obj != null)
        {
            enemyObjList.Add(obj);
        }
        else
        {
            Debug.LogError("��̃I�u�W�F�N�g�������Ɏw�肳��Ă��܂��I");
        }
    }

    //------------------------------------------------
    //  �v���p�e�B
    //------------------------------------------------
    public int GetDestroyNum(){return enemyDestroyNum;}
    public void SetDestroyNum(int num){enemyDestroyNum = num;}
    public List<DROP_TYPE> GetDropType(){ return dropType; }
    public GameObject GetSmallKon(){ return smallKonItem; }
    public GameObject GetLargeKon(){ return largeKonItem; }
    public List<GameObject> GetPowerupItems(){ return powerupItems; }
    public List<GameObject> GetSpeedupItems(){ return speedupItems; }
    public List<GameObject> GetBombItems(){ return bombItems; }
    public List<GameObject> GetHealItems(){ return healItems; }
    public Vector3 GetSpawnerPos(int i){ return spawners[i].position; }
    public Vector3 GetControlPointPos(int i){ return controlPoints[i].position; }
    public GameObject GetBulletPrefab(int type){ return enemyBullet[type]; }
    public bool GetEndZakoStage(){ return endZakoStage; }
    public void SetEndZakoStage(bool b){ endZakoStage = b; }
    public GameObject GetEnemyPrefab(int n){ return enemyPrefab[n]; }
    public GameObject GetDangerLineCanvas(){ return dangerLineCanvas;}
    public bool GetIsCompleteLoading(){ return isCompleteLoading; }
    public List<GameObject> GetEnemyObjectList(){ return enemyObjList; }
    public EnemySetting GetEnemySetting(){ return enemySetting; }
    public SpriteRenderer GetHomingBulletSprite(){ return homingBullet; }
    public Animator GetHomingBulletAnimator(){ return homingAnimator; }

    //------------------------------------------------
    //  �����_����X���W��Ԃ�
    //------------------------------------------------
    private float GetRandomX()
    {
        return UnityEngine.Random.Range(-7.7f,6.07f);
    }

    //------------------------------------------------
    //  �w��̍��W�Ɏw��̓G�Z�b�g�𐶐�����
    //------------------------------------------------
    public GameObject SetEnemy(GameObject prefab,Vector3 pos,ePowerupItems item = ePowerupItems.None)
    {
        //  �G�I�u�W�F�N�g�̐���
        enemyObjList.Add(Instantiate(prefab,pos,transform.rotation));

        //  �G���̐ݒ�
        enemyObjList.Last().GetComponent<Enemy>().SetEnemyData(enemySetting, item);

        return enemyObjList.Last();
    }

    //------------------------------------------------
    //  ��ʒ����𒆐S�Ƃ��Ď���ɐl�`��W�J
    //------------------------------------------------
    public void SetDollGroup(ePowerupItems item = ePowerupItems.None)
    {
        float totalDegree = 360;        //  ���͈͂̑��p  
        int wayNum = 8;                 //  �e��way��
        float Degree = totalDegree / wayNum;     //  �e�ꔭ���ɂ��炷�p�x
        float distance = 5f;            //  �v���C���[���痣������

        //  �v���C���[�̑O���x�N�g�����擾
        Vector3 vector0 = GameManager.Instance.GetPlayer().transform.up;
        Vector3[] vector = new Vector3[wayNum];

        for (int i = 0; i < wayNum; i++)
        {
            //  �x�N�g���̊p�x��ݒ�
            vector[i] = Quaternion.Euler
                (0, 0, -Degree * i) * vector0;
            vector[i].z = 0f;

            //  �G�𐶐�������W���v�Z
            Vector3 playerPos = new Vector3(0,2,0);
            Vector3 pos = distance * vector[i] + playerPos;

            //  �l�`�I�u�W�F�N�g�𐶐�
            enemyObjList.Add(Instantiate(specialEnemyPrefab[(int)SpecialEnemyPattern.DollSiege],pos,transform.rotation));

            //  �G���̐ݒ�
            enemyObjList.Last().GetComponent<Enemy>().SetEnemyData(enemySetting, item);
        }
    }

    //------------------------------------------------
    //  �{�X�̓G�������Z�b�g����
    //------------------------------------------------
    public void SetBoss(BossType bossType,ePowerupItems item)
    {
        //  �G���̐ݒ�
        GameObject obj = EventSceneManager.Instance.GetBossObject();

        //  �X�e�[�^�X��ݒ�
        if(bossType == BossType.Douji)
            obj.GetComponent<BossDouji>().SetBossData(enemySetting,item);
        else if (bossType == BossType.Tsukumo)
            obj.GetComponent<BossTsukumo>().SetBossData(enemySetting,item);
        //else if(bossType == BossType.Kuchinawa)
        //    obj.GetComponent<BossKuchinawa>().SetBossData(enemySetting,item);
        //else if(bossType == BossType.Kurama)
        //    obj.GetComponent<BossKurama>().SetBossData(enemySetting,item);
        //else if(bossType == BossType.Wadatsumi)
        //    obj.GetComponent<BossWadatsumi>().SetBossData(enemySetting,item);
        //else if(bossType == BossType.Hakumen)
        //    obj.GetComponent<BossHakumen>().SetBossData(enemySetting,item);
    }

    //-----------------------------------------------------------
    //  ������X�e�[�W�ɂ���ēG�o���p�^�[�����Ăѕ�����
    //-----------------------------------------------------------
    public IEnumerator AppearEnemy()
    {
        if(PlayerInfoManager.stageInfo == PlayerInfoManager.StageInfo.Stage01)
        {
            StartCoroutine(AppearEnemy_Stage01());
        }
        else if(PlayerInfoManager.stageInfo == PlayerInfoManager.StageInfo.Stage02)
        {
            StartCoroutine(AppearEnemy_Stage02());
        }
        else if(PlayerInfoManager.stageInfo == PlayerInfoManager.StageInfo.Stage03)
        {
            StartCoroutine(AppearEnemy_Stage03());
        }
        else if(PlayerInfoManager.stageInfo == PlayerInfoManager.StageInfo.Stage04)
        {
            StartCoroutine(AppearEnemy_Stage04());
        }
        else if(PlayerInfoManager.stageInfo == PlayerInfoManager.StageInfo.Stage05)
        {
            StartCoroutine(AppearEnemy_Stage05());
        }
        else if(PlayerInfoManager.stageInfo == PlayerInfoManager.StageInfo.Stage06)
        {
            StartCoroutine(AppearEnemy_Stage06());
        }

        yield return null;
    }

    //-----------------------------------------------------------
    //  �v���C���[�Ɉ�ԋ߂��G�I�u�W�F�N�g���������ĕԂ�
    //-----------------------------------------------------------
    public GameObject GetNearestEnemyFromPlayer()
    {
        //  �v���C���[�̍��W���擾
        Vector3 playerPos = GameManager.Instance.GetPlayer().transform.position;

        //  �S�Ă̓G���X�g�̓G�Ƌ������r
        float d1 = 0f;          //  ����̓G�Ƃ̋���
        float d2 = 100f;        //  �O��̓G�Ƃ̋���(�K���ȑ傫���l)
        int nearestNum = -1;    //  ��ԋ߂��G�̃��X�g�ԍ�

        //  �G�����Ȃ��ꍇ��null
        if(enemyObjList.Count == 0)return null;

        for(int i = 0;i<enemyObjList.Count;i++)
        {
            if(enemyObjList[i].gameObject == null)continue;

            //  ���X�g��i�Ԗڂ̓G�ƃv���C���[�̋������Z�o
            d1 = Vector3.Distance(enemyObjList[i].transform.position, playerPos);

            //  ����̕����߂������炻���d2�ɃR�s�[
            if(d1 < d2)
            {
                d2 = d1;

                //  ��ԋ߂��G�̃��X�g�ԍ����R�s�[
                nearestNum = i;
            }
        }

        //  �G���[���
        if(nearestNum == -1)return null;

        return enemyObjList[nearestNum];
    }

    //-----------------------------------------------------------
    //  �e�X�e�[�W�J�n���̃X�e�[�W�����o
    //-----------------------------------------------------------
    private IEnumerator StagingLevelBeginning()
    {
        float fade_time = 2f;   //  �t�F�[�h�ɗv���鎞��(�b)

        //  �X�e�[�W�ɉ������e�L�X�g��ݒ肷��
        stageStartingText.text = stageName[(int)PlayerInfoManager.stageInfo];

        //  �e�L�X�g���t�F�[�h�C��������
        stageStartingText.DOFade(1.0f, fade_time);

        //  �҂�
        yield return new WaitForSeconds(fade_time);

        //  �e�L�X�g���t�F�[�h�A�E�g������
        stageStartingText.DOFade(0.0f, fade_time);

        yield return null;
    }

    //-----------------------------------------------------------
    //  �X�e�[�W�P�̃U�R�G�̏o���p�^�[��
    //-----------------------------------------------------------
    public IEnumerator AppearEnemy_Stage01()
    {
        //  �U�R��BGM�J�n
        SoundManager.Instance.PlayBGM((int)MusicList.BGM_DOUJI_STAGE_ZAKO);

        ////  �X�e�[�W�J�n���o
        //yield return StartCoroutine(StagingLevelBeginning());

        //yield return new WaitForSeconds(3f);

        ////  Wave1
        //SetEnemy(
        //    enemyPrefab[(int)EnemyPattern.E01],
        //    new Vector3(GetRandomX(), appearY, 0)
        //);

        ////  Wave2
        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.32f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(1.0f);

        ////  Wave3
        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.32f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.3f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0), ePowerupItems.PowerUp);

        //yield return new WaitForSeconds(1.0f);

        ////  Wave4
        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E02],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(1.0f);

        ////  Wave5
        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E02],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(1.0f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E02],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(1.0f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E02_B],
        //new Vector3(GetRandomX(), appearY, 0), ePowerupItems.SpeedUp);

        //yield return new WaitForSeconds(5.0f);

        ////  Wave6
        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.32f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.3f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01_B],
        //new Vector3(GetRandomX(), appearY, 0), ePowerupItems.Bomb);

        //yield return new WaitForSeconds(0.3f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.3f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.3f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01_B],
        //new Vector3(GetRandomX(), appearY, 0), ePowerupItems.PowerUp);

        //yield return new WaitForSeconds(1.0f);

        ////  Wave7
        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.32f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.3f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01_B],
        //new Vector3(GetRandomX(), appearY, 0), ePowerupItems.Heal);

        //yield return new WaitForSeconds(0.3f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.32f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.3f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01_B],
        //new Vector3(GetRandomX(), appearY, 0), ePowerupItems.Random);

        //yield return new WaitForSeconds(0.3f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.3f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.3f);

        //yield return new WaitForSeconds(1.0f);

        ////  Wave8
        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.32f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.3f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.3f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.32f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.3f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.3f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.32f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.3f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.3f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01_B],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.3f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E02],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(1.0f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E02],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(1.0f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E02_B],
        //new Vector3(GetRandomX(), appearY, 0), ePowerupItems.PowerUp);

        //yield return new WaitForSeconds(1.0f);

        ////  Wave9
        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01_B],
        //new Vector3(GetRandomX(), appearY, 0), ePowerupItems.Random);

        //yield return new WaitForSeconds(1.0f);

        ////  wave10
        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01_B],
        //new Vector3(GetRandomX(), appearY, 0), ePowerupItems.Bomb);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E02],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(1.0f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E02],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(1.0f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E02_B],
        //new Vector3(GetRandomX(), appearY, 0), ePowerupItems.PowerUp);

        //yield return new WaitForSeconds(5.0f);

        //  Wave11
        GameObject MidBoss = SetEnemy(
                                    enemyPrefab[(int)EnemyPattern.MidBoss],
                                    new Vector3(0, -4, 0),
                                    ePowerupItems.PowerUp
                                );

        //  ���{�X��HP�X���C�_�[�𐶐�
        GameObject midSliderObject = Instantiate(midBossSlider);
        midSliderObject.transform.SetParent(bossCanavs.transform, false);

        //  HP�X���C�_�[��ݒ�
        MidBoss.GetComponent<Enemy>().SetHpSlider(midSliderObject.GetComponent<Slider>());

        //  �U�R��I���t���O��TRUE�ɂȂ�܂ő҂�
        yield return new WaitUntil(() => endZakoStage == true);

        //  �U�R�G�S�폜
        DeleteAllEnemy();

        //  ���{�X�̃X���C�_�[���폜
        Destroy(midSliderObject);

        //  Player��ShotManager������������
        GameObject player = GameManager.Instance.GetPlayer();
        player.GetComponent<PlayerShotManager>().DisableShot();

        //  BombManager�𖳌�������
        player.GetComponent<PlayerBombManager>().DisableBomb();

        //  �v���C���[������ł�����break
        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph.GetCurrentHealth() <= 0) yield break;

        //  �T�b�҂�
        yield return new WaitForSeconds(5);

        //  �|�[�Y�𖳌���
        GameManager.Instance.GetPauseAction().Disable();

        //  �C�x���g���[�h�ֈڍs
        GameManager.Instance.SetGameState((int)eGameState.Event);

        //  �C�x���g�V�[���}�l�[�W���[��L����
        eventSceneManager.SetActive(true);

        //  �{�X��J�n�t���O��TRUE�ɂȂ�܂ő҂�(�{�X�탂�[�h)
        yield return new WaitUntil(() => EventSceneManager.Instance.GetStartBoss());

        /***********************��������{�X��***********************/

        //  �|�[�Y��L����
        GameManager.Instance.GetPauseAction().Enable();

        //  �{�XBGM�Đ�
        SoundManager.Instance.PlayBGM((int)MusicList.BGM_DOUJI_STAGE_BOSS);

        //  �C�x���g�V�[���}�l�[�W���[�𖳌���
        eventSceneManager.SetActive(false);

        //  �V���b�g��L����
        player.GetComponent<PlayerShotManager>().EnableShot();

        //  BombManager��L��������
        player.GetComponent<PlayerBombManager>().EnableBomb();

        //  �{�X��HP�X���C�_�[�𐶐�
        GameObject sliderObject = Instantiate(bossSlider);
        sliderObject.transform.SetParent(bossCanavs.transform, false);


        //  �{�X�I�u�W�F�N�g���擾
        GameObject BossObj = EventSceneManager.Instance.GetBossObject();
        if(!BossObj) Debug.LogError("�{�X�I�u�W�F�N�g��null�ɂȂ��Ă��܂��I");


        //  �{�X�R���|�[�l���g���擾
        BossDouji boss_douji = BossObj.GetComponent<BossDouji>();
        if(!boss_douji)Debug.LogError("�{�X��BossDouji�R���|�[�l���g�������Ă��܂���I\n" +
            "PlayerInfoManager.stageInfo�̒l���m�F���Ă�������");


        //  �{�X���Ƃ�HP�X���C�_�[��ݒ�
        boss_douji.SetHpSlider(sliderObject.GetComponent<Slider>());
        bossNameHPSlider[(int)BossType.Tsukumo].SetActive(true);

        //  �{�X�R���|�[�l���g��L����
        boss_douji.enabled = true;
        BossObj.GetComponent<BoxCollider2D>().enabled = true;

        //  �{�X��I���t���O��TRUE�ɂȂ�܂ő҂�
        yield return new WaitUntil(() => GameManager.Instance.GetStageClearFlag());

        //  �|�[�Y�𖳌���
        GameManager.Instance.GetPauseAction().Disable();

        //  ���E�̏�C�I�u�W�F�N�g�𖳌���
        EventSceneManager.Instance.GetFogObjectL().SetActive(false);
        EventSceneManager.Instance.GetFogObjectR().SetActive(false);

        //  �{�X��HP�L�����o�X���\��
        bossCanavs.SetActive(false);

        // �V���b�g�𖳌���
        player.GetComponent<PlayerShotManager>().DisableShot();

        //  BombManager�𖳌�������
        player.GetComponent<PlayerBombManager>().DisableBomb();

        //  �T�b�҂�
        yield return new WaitForSeconds(5);

        //  Player�̃V���b�g��������
        player.GetComponent<PlayerShotManager>().InitShot();

        //  �C�x���g���[�h�ֈڍs
        GameManager.Instance.SetGameState((int)eGameState.Event);

        //  �C�x���g�V�[���}�l�[�W���[��L����
        eventSceneManager.SetActive(true);
    }

    //-----------------------------------------------------------
    //  �X�e�[�W�Q�̃U�R�G�̏o���p�^�[��
    //-----------------------------------------------------------
    public IEnumerator AppearEnemy_Stage02()
    {
        //  �U�R��BGM�J�n
        SoundManager.Instance.PlayBGM((int)MusicList.BGM_TSUKUMO_STAGE_ZAKO);

        ////  �X�e�[�W�J�n���o
        //yield return StartCoroutine(StagingLevelBeginning());

        //yield return new WaitForSeconds(3f);

        ////  Wave1
        //SetEnemy(
        //    enemyPrefab[(int)EnemyPattern.E01],
        //    new Vector3(GetRandomX(), appearY, 0)
        //);

        ////  Wave2
        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.32f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(1.0f);

        ////  Wave3
        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.32f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.3f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0), ePowerupItems.PowerUp);

        //yield return new WaitForSeconds(1.0f);

        ////  Wave4
        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E02],
        //new Vector3(GetRandomX(), appearY, 0));

        ////  �l�`��͐w
        //SetDollGroup();

        //yield return new WaitForSeconds(1.0f);

        ////  Wave5
        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E02],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(1.0f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E02],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(1.0f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E02_B],
        //new Vector3(GetRandomX(), appearY, 0), ePowerupItems.SpeedUp);

        //yield return new WaitForSeconds(5.0f);

        ////  Wave6
        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.32f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.3f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01_B],
        //new Vector3(GetRandomX(), appearY, 0), ePowerupItems.Bomb);

        //yield return new WaitForSeconds(0.3f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.3f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.3f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01_B],
        //new Vector3(GetRandomX(), appearY, 0), ePowerupItems.PowerUp);

        //yield return new WaitForSeconds(1.0f);

        ////  Wave7
        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.32f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.3f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01_B],
        //new Vector3(GetRandomX(), appearY, 0), ePowerupItems.Heal);

        //yield return new WaitForSeconds(0.3f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.32f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.3f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01_B],
        //new Vector3(GetRandomX(), appearY, 0), ePowerupItems.Random);

        //yield return new WaitForSeconds(0.3f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.3f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.3f);

        //yield return new WaitForSeconds(1.0f);

        ////  Wave8
        ////  �l�`��͐w
        //SetDollGroup();

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.32f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.3f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.3f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.32f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.3f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.3f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.32f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.3f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.3f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01_B],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(0.3f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E02],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(1.0f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E02],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(1.0f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E02_B],
        //new Vector3(GetRandomX(), appearY, 0), ePowerupItems.PowerUp);

        //yield return new WaitForSeconds(1.0f);

        ////  Wave9
        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01_B],
        //new Vector3(GetRandomX(), appearY, 0), ePowerupItems.Random);

        //yield return new WaitForSeconds(1.0f);

        ////  wave10
        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0));

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E01_B],
        //new Vector3(GetRandomX(), appearY, 0), ePowerupItems.Bomb);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E02],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(1.0f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E02],
        //new Vector3(GetRandomX(), appearY, 0));

        //yield return new WaitForSeconds(1.0f);

        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E02_B],
        //new Vector3(GetRandomX(), appearY, 0), ePowerupItems.PowerUp);

        //yield return new WaitForSeconds(8.0f);

        //  WaveX
        GameObject MidBoss = SetEnemy(
                                    enemyPrefab[(int)EnemyPattern.MidBoss],
                                    new Vector3(0, -6, 0),
                                    ePowerupItems.PowerUp
                                );

        //  ���{�X��HP�X���C�_�[�𐶐�
        GameObject midSliderObject = Instantiate(midBossSlider);
        midSliderObject.transform.SetParent(bossCanavs.transform, false);

        //  HP�X���C�_�[��ݒ�
        MidBoss.GetComponent<Enemy>().SetHpSlider(midSliderObject.GetComponent<Slider>());

        //  �U�R��I���t���O��TRUE�ɂȂ�܂ő҂�
        yield return new WaitUntil(() => endZakoStage == true);

        //  �U�R�G�S�폜
        DeleteAllEnemy();

        //  ���{�X�̃X���C�_�[���폜
        Destroy(midSliderObject);

        //  Player��ShotManager������������
        GameObject player = GameManager.Instance.GetPlayer();
        player.GetComponent<PlayerShotManager>().DisableShot();

        //  BombManager�𖳌�������
        player.GetComponent<PlayerBombManager>().DisableBomb();

        //  �v���C���[������ł�����break
        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph.GetCurrentHealth() <= 0) yield break;

        //  �T�b�҂�
        yield return new WaitForSeconds(5);

        //  �|�[�Y�𖳌���
        GameManager.Instance.GetPauseAction().Disable();

        //  �C�x���g���[�h�ֈڍs
        GameManager.Instance.SetGameState((int)eGameState.Event);

        //  �C�x���g�V�[���}�l�[�W���[��L����
        eventSceneManager.SetActive(true);

        //  �{�X��J�n�t���O��TRUE�ɂȂ�܂ő҂�(�{�X�탂�[�h)
        yield return new WaitUntil(() => EventSceneManager.Instance.GetStartBoss());

        ///***********************��������{�X��***********************/

        //  �|�[�Y��L����
        GameManager.Instance.GetPauseAction().Enable();

        //  �{�XBGM�Đ�
        SoundManager.Instance.PlayBGM((int)MusicList.BGM_TSUKUMO_STAGE_BOSS);

        //  �C�x���g�V�[���}�l�[�W���[�𖳌���
        eventSceneManager.SetActive(false);

        //  �V���b�g��L����
        player.GetComponent<PlayerShotManager>().EnableShot();

        //  BombManager��L��������
        player.GetComponent<PlayerBombManager>().EnableBomb();

        //  �{�X��HP�X���C�_�[�𐶐�
        GameObject sliderObject = Instantiate(bossSlider);
        sliderObject.transform.SetParent(bossCanavs.transform, false);


        //  �{�X�I�u�W�F�N�g���擾
        GameObject BossObj = EventSceneManager.Instance.GetBossObject();
        if(!BossObj) Debug.LogError("�{�X�I�u�W�F�N�g��null�ɂȂ��Ă��܂��I");


        //  �{�X�R���|�[�l���g���擾
        BossTsukumo boss_tsukumo = BossObj.GetComponent<BossTsukumo>();
        if(!boss_tsukumo)Debug.LogError("�{�X��BossTsukumo�R���|�[�l���g�������Ă��܂���I\n" +
            "PlayerInfoManager.stageInfo�̒l���m�F���Ă�������");


        //  �{�X���Ƃ�HP�X���C�_�[��ݒ�
        boss_tsukumo.SetHpSlider(sliderObject.GetComponent<Slider>());
        bossNameHPSlider[(int)BossType.Tsukumo].SetActive(true);

        //  �{�X�R���|�[�l���g��L����
        boss_tsukumo.enabled = true;
        BossObj.GetComponent<BoxCollider2D>().enabled = true;

        //  �{�X��I���t���O��TRUE�ɂȂ�܂ő҂�
        yield return new WaitUntil(() => GameManager.Instance.GetStageClearFlag());

        //  �|�[�Y�𖳌���
        GameManager.Instance.GetPauseAction().Disable();

        //  ���E�̏�C�I�u�W�F�N�g�𖳌���
        EventSceneManager.Instance.GetFogObjectL().SetActive(false);
        EventSceneManager.Instance.GetFogObjectR().SetActive(false);

        //  �{�X��HP�L�����o�X���\��
        bossCanavs.SetActive(false);

        // �V���b�g�𖳌���
        player.GetComponent<PlayerShotManager>().DisableShot();

        //  BombManager�𖳌�������
        player.GetComponent<PlayerBombManager>().DisableBomb();

        //  �T�b�҂�
        yield return new WaitForSeconds(5);

        //  Player�̃V���b�g��������
        player.GetComponent<PlayerShotManager>().InitShot();

        //  �C�x���g���[�h�ֈڍs
        GameManager.Instance.SetGameState((int)eGameState.Event);

        //  �C�x���g�V�[���}�l�[�W���[��L����
        eventSceneManager.SetActive(true);
    }

    //-----------------------------------------------------------
    //  �X�e�[�W�R�̃U�R�G�̏o���p�^�[��
    //-----------------------------------------------------------
    public IEnumerator AppearEnemy_Stage03()
    {
        yield return null;
    }

    //-----------------------------------------------------------
    //  �X�e�[�W�S�̃U�R�G�̏o���p�^�[��
    //-----------------------------------------------------------
    public IEnumerator AppearEnemy_Stage04()
    {
        yield return null;
    }

    //-----------------------------------------------------------
    //  �X�e�[�W�T�̃U�R�G�̏o���p�^�[��
    //-----------------------------------------------------------
    public IEnumerator AppearEnemy_Stage05()
    {
        yield return null;
    }

    //-----------------------------------------------------------
    //  �X�e�[�W�U�̃U�R�G�̏o���p�^�[��
    //-----------------------------------------------------------
    public IEnumerator AppearEnemy_Stage06()
    {
        yield return null;
    }
}
