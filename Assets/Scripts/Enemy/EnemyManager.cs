using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System;
using UnityEngine.Experimental.GlobalIllumination;

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

    // �G�̏o���Z�b�g 
    public enum EnemyPattern
    {
        //  �P��
        E01,     //  �q�S
        E01_B,   //  �q�SB
        E02,     //  �q�g�_�}
        E02_B,   //  �q�g�_�}B
        E03,     //  ��ؓ��q
        EX,      //  �������Ⴂ��������

        Max
    }

    //  �p���[�A�b�v�A�C�e���𗎂Ƃ����ǂ���
    public enum DROP_TYPE {
        None,                //  �Ȃ�
        Drop,                //  ����
    }

    [SerializeField] private GameObject[] enemyPrefab;
    private const float appearY = -6.5f;


    private int enemyDestroyNum = 0;        //  �j�󂳂ꂽ�G��
    private bool endZakoStage;              //  �U�R�X�e�[�W�̏I���t���O

    //  �h���b�v���鍰�̃v���n�u�B
    private List<GameObject> konItems;
    AsyncOperationHandle<IList<GameObject>> loadHandleKon;

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

        //  �G���̃��[�h
        //  ��EmemySetting�̓ǂݍ��݂܂ł͂����ł��
        //  EnemyData�ւ̐ݒ�͐�������Enemy�ɃZ�b�g����
        await LoadEnemySetting();

        Debug.Log("�G���t�@�C���̃��[�h����");

        //  �v���n�u�̃��[�h
        await LoadPrefab();

        Debug.Log("�h���b�v�A�C�e���̃��[�h����");
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
        konItems = new List<GameObject>();
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
        List<string> keys1 = new List<string>() {"KonItems"};
        List<string> keys2 = new List<string>() {"PowerupItems"};
        List<string> keys3 = new List<string>() {"SpeedupItems"};
        List<string> keys4 = new List<string>() {"BombItems"};
        List<string> keys5 = new List<string>() {"HealItems"};

        //  �h���b�v�A�C�e�������[�h
        loadHandleKon = Addressables.LoadAssetsAsync<GameObject>
            (
                keys1,
                null,
                Addressables.MergeMode.Union,
                false
            );

        //  ���[�h�̊�����҂�
        await loadHandleKon.Task;

        //  �p���[�A�b�v�A�C�e�������[�h
        loadHandlePowerup = Addressables.LoadAssetsAsync<GameObject>
            (
                keys2,
                null,
                Addressables.MergeMode.Union,
                false
            );

        //  ���[�h�̊�����҂�
        await loadHandlePowerup.Task;

        //  �X�s�[�h�A�b�v�A�C�e�������[�h
        loadHandleSpeedup = Addressables.LoadAssetsAsync<GameObject>
            (
                keys3,
                null,
                Addressables.MergeMode.Union,
                false
            );

        //  ���[�h�̊�����҂�
        await loadHandleSpeedup.Task;

        //  �{���A�C�e�������[�h
        loadHandleBomb = Addressables.LoadAssetsAsync<GameObject>
            (
                keys4,
                null,
                Addressables.MergeMode.Union,
                false
            );

        //  ���[�h�̊�����҂�
        await loadHandleBomb.Task;

        //  �q�[���A�C�e�������[�h
        loadHandleHeal = Addressables.LoadAssetsAsync<GameObject>
            (
                keys5,
                null,
                Addressables.MergeMode.Union,
                false
            );

        //  ���[�h�̊�����҂�
        await loadHandleHeal.Task;

        //  �v�f�̐��������X�g�ɒǉ�
        foreach (var addressable in loadHandleKon.Result)
        {
            if (addressable != null)
            {
                konItems.Add( addressable );
            }
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
            Addressables.Release(loadHandleKon);
            Addressables.Release(loadHandlePowerup);
            Addressables.Release(loadHandleSpeedup);
            Addressables.Release(loadHandleBomb);
            Addressables.Release(loadHandleHeal);
            Addressables.Release(loadHandleEnemySetting);
        } catch (Exception e) {
            // ��O�������̏���
            Debug.Log("loadHandleKon�����g�p�ł�");
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
        //  �G���X�g���Ď����Ď���ł����烊�X�g����폜
        for(int i = 0;i<enemyObjList.Count;i++)
        {
            if(enemyObjList[i].gameObject == null)
            {
                enemyObjList.Remove(enemyObjList[i]);
            }
        }
    }

    //------------------------------------------------
    //  �v���p�e�B
    //------------------------------------------------
    public int GetDestroyNum(){return enemyDestroyNum;}
    public void SetDestroyNum(int num){enemyDestroyNum = num;}
    public List<DROP_TYPE> GetDropType(){ return dropType; }
    public List<GameObject> GetKonItems(){ return konItems; }
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
    //  �{�X�̓G�������Z�b�g����
    //------------------------------------------------
    public void SetBoss(BossType bossType,ePowerupItems item)
    {
        //  �G���̐ݒ�
        GameObject obj = EventSceneManager.Instance.GetBossObject();

        //  �X�e�[�^�X��ݒ�
        if(bossType == BossType.Douji)
            obj.GetComponent<BossDouji>().SetBossData(enemySetting,item);
        //else if(bossType == BossType.Tsukumo)
        //    obj.GetComponent<BossTsukumo>().SetBossData(enemySetting);
        //else if(bossType == BossType.Kuchinawa)
        //    obj.GetComponent<BossKuchinawa>().SetBossData(enemySetting);
        //else if(bossType == BossType.Kurama)
        //    obj.GetComponent<BossKurama>().SetBossData(enemySetting);
        //else if(bossType == BossType.Wadatsumi)
        //    obj.GetComponent<BossWadatsumi>().SetBossData(enemySetting);
        //else if(bossType == BossType.Hakumen)
        //    obj.GetComponent<BossHakumen>().SetBossData(enemySetting);
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
        return enemyObjList[nearestNum];
    }

    //-----------------------------------------------------------
    //  �X�e�[�W�P�̃U�R�G�̏o���p�^�[��
    //-----------------------------------------------------------
    public IEnumerator AppearEnemy_Stage01()
    {
        //  �U�R��BGM�J�n
        //SoundManager.Instance.PlayBGM((int)MusicList.BGM_DOUJI_STAGE_ZAKO);

        //yield return new WaitForSeconds(3f);

        ////  Wave1
        //SetEnemy(
        //    enemyPrefab[(int)EnemyPattern.E01],
        //    new Vector3(GetRandomX(), appearY, 0)
        //);

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01_B],
        new Vector3(GetRandomX(), appearY, 0), ePowerupItems.PowerUp);

        yield return new WaitForSeconds(5.0f);

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
        //enemyPrefab[(int)EnemyPattern.E01_B],
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
        //new Vector3(GetRandomX(), appearY, 0), ePowerupItems.Random);

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
        //new Vector3(GetRandomX(), appearY, 0), ePowerupItems.Random);

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
                                    enemyPrefab[(int)EnemyPattern.E03],
                                    new Vector3(0, -4, 0),
                                    ePowerupItems.PowerUp
                                );

        //  ���{�X��HP�X���C�_�[�𐶐�
        GameObject midSliderObject = Instantiate(midBossSlider);
        midSliderObject.transform.SetParent(bossCanavs.transform, false);

        //  HP�X���C�_�[��ݒ�
        MidBoss.GetComponent<Enemy>().SetHpSlider(midSliderObject.GetComponent<Slider>());

        //  �U�R��I���t���O��TRUE�ɂȂ�܂ő҂�
        yield return new WaitUntil(()=> endZakoStage == true);

        //  ���{�X�̃X���C�_�[���폜
        Destroy(midSliderObject);

        //  Player��ShotManager������������
        GameObject player = GameManager.Instance.GetPlayer();
        player.GetComponent<PlayerShotManager>().DisableShot();

        //  �v���C���[������ł�����break
        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if(ph.GetCurrentHealth() <= 0)yield break;

        //  �T�b�҂�
        yield return new WaitForSeconds(5);

        //  �f�B���N�V���i�����C�g�𖳌���
        directionalLight.enabled = false;

        //  �|�[�Y�𖳌���
        GameManager.Instance.GetPauseAction().Disable();

        //  �C�x���g���[�h�ֈڍs
        GameManager.Instance.SetGameState((int)eGameState.Event);

        //  �C�x���g�V�[���}�l�[�W���[��L����
        eventSceneManager.SetActive(true);

        //  �{�X��J�n�t���O��TRUE�ɂȂ�܂ő҂�(�{�X�탂�[�h)
        yield return new WaitUntil(()=> EventSceneManager.Instance.GetStartBoss());

        /***********************��������{�X��***********************/

        //  �|�[�Y��L����
        GameManager.Instance.GetPauseAction().Enable();

        //  �{�XBGM�Đ�
        //SoundManager.Instance.PlayBGM((int)MusicList.BGM_DOUJI_STAGE_BOSS);

        //  �C�x���g�V�[���}�l�[�W���[�𖳌���
        eventSceneManager.SetActive(false);

        //  �V���b�g��L����
        player.GetComponent<PlayerShotManager>().EnableShot();

        //  �{�X��HP�X���C�_�[�𐶐�
        GameObject sliderObject = Instantiate(bossSlider);
        sliderObject.transform.SetParent(bossCanavs.transform, false);

        //  �{�X���Ƃ�HP�X���C�_�[��ݒ�
        GameObject BossObj = EventSceneManager.Instance.GetBossObject();
        if(BossObj.GetComponent<BossDouji>())
        {
            BossObj.GetComponent<BossDouji>()
                .SetHpSlider(sliderObject.GetComponent<Slider>());
            bossNameHPSlider[(int)BossType.Douji].SetActive(true);
        }
        //else if (BossObj.GetComponent<BossTsukumo>())
        //{
        //    BossObj.GetComponent<BossTsukumo>()
        //        .SetHpSlider(sliderObject.GetComponent<Slider>());
        //    bossNameHPSlider[(int)BossType.Tsukumo].SetActive(true);
        //}
        //else if (BossObj.GetComponent<BossKuchinawa>())
        //{
        //    BossObj.GetComponent<BossKuchinawa>()
        //        .SetHpSlider(sliderObject.GetComponent<Slider>());
        //    bossNameHPSlider[(int)BossType.Kuchinawa].SetActive(true);
        //}
        //else if (BossObj.GetComponent<BossKurama>())
        //{
        //    BossObj.GetComponent<BossKurama>()
        //        .SetHpSlider(sliderObject.GetComponent<Slider>());
        //    bossNameHPSlider[(int)BossType.Kurama].SetActive(true);
        //}
        //else if (BossObj.GetComponent<BossWadatsumi>())
        //{
        //    BossObj.GetComponent<BossWadatsumi>()
        //        .SetHpSlider(sliderObject.GetComponent<Slider>());
        //    bossNameHPSlider[(int)BossType.Wadatsumi].SetActive(true);
        //}
        //else if (BossObj.GetComponent<BossHakumen>())
        //{
        //    BossObj.GetComponent<BossHakumen>()
        //        .SetHpSlider(sliderObject.GetComponent<Slider>());
        //    bossNameHPSlider[(int)BossType.Hakumen].SetActive(true);
        //}

        //  �{�X�R���|�[�l���g��L����
        GameObject boss = EventSceneManager.Instance.GetBossObject();
        boss.GetComponent<BossDouji>().enabled = true;
        boss.GetComponent<BoxCollider2D>().enabled = true;

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
        yield return null;
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
