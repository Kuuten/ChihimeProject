using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using static DropItems;
using UnityEngine.ResourceManagement.AsyncOperations;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine.InputSystem;
using System;
using static EnemyManager;
using static Enemy;

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
        E01_3,    //  �c�R
        E01_3B,   //  �c�RB

        Max
    }

    //  �G�̎��
    public enum EnemyType
    {
        Kooni,          //  �q�S
        Onibi,          //  �S��
        Ibarakidouji,   //  ��银�q
        Douji,          //  �{�X�E�h�E�W
    
        Max
    }

    //  �p���[�A�b�v�A�C�e���𗎂Ƃ����ǂ���
    public enum DROP_TYPE {
        None,                //  �Ȃ�
        Drop,                //  ����
    }

    [SerializeField] private GameObject[] enemyPrefab;
    private const float appearY = -5.5f;

    private int appearStep = 0;
    private float timer = 0;

    //private int enemyDestroyMaxNum = 61;    //  �P�X�e�[�W�̍ő�G��
    private int enemyGenerateNum = 0;       //  �������ꂽ�G��
    private int enemyDestroyNum = 0;         //  �j�󂳂ꂽ�G��

    //  �h���b�v���鍰�̃v���n�u�B
    private List<GameObject> konItems;
    AsyncOperationHandle<IList<GameObject>> loadHandleKon;

    //  �p���[�A�C�e���̃v���n�u�B
    private List<GameObject> powerupItems;
    AsyncOperationHandle<IList<GameObject>> loadHandlePowerup;

    //  �p���[�A�b�v�A�C�e���𗎂Ƃ����ǂ���
    private List<DROP_TYPE> dropType = new List<DROP_TYPE>();

    //  �G���
    private EnemySetting enemySetting;
    AsyncOperationHandle<IList<EnemySetting>> loadHandleEnemySetting;

    //  EnemyData�N���X����̏��擾�p
    private EnemyData enemyData;

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
        appearStep = 0;
        timer = 0;
        enemyGenerateNum = 0;
        enemyDestroyNum = 0;

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

        appearStep++;

    }

    //-----------------------------------------------------------------
    //  �v���n�u�̃��[�h
    //-----------------------------------------------------------------
    private async UniTask LoadPrefab()
    {
        konItems = new List<GameObject>();
        powerupItems = new List<GameObject>();

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
    }

    private void OnDestroy()
    {
        // ���
        try {
            Addressables.Release(loadHandleKon);
            Addressables.Release(loadHandlePowerup);
            Addressables.Release(loadHandleEnemySetting);
        } catch (Exception e) {
            // ��O�������̏���
            Debug.Log("loadHandleKon�����g�p�ł�");
            Debug.Log("loadHandlePowerup�����g�p�ł�");
            Debug.Log("loadHandleEnemySetting�����g�p�ł�");
        }

    }

    void Update()
    {
        AppearEnemy_Stage01();
        
    }

    //------------------------------------------------
    //  �v���p�e�B
    //------------------------------------------------
    public int GetDestroyNum(){return enemyDestroyNum;}
    public void SetDestroyNum(int num){enemyDestroyNum = num;}
    public List<DROP_TYPE> GetDropType(){ return dropType; }
    public List<GameObject> GetKonItems(){ return konItems; }
    public List<GameObject> GetPowerupItems(){ return powerupItems; }

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
    private void SetEnemy(GameObject prefab,Vector3 pos)
    {
        //  �G�I�u�W�F�N�g�̐���
        GameObject obj = Instantiate(prefab,pos,transform.rotation);

        //  �G���̐ݒ�
        obj.GetComponent<Enemy>().SetEnemyData(enemySetting);

        enemyGenerateNum++; //  �G������+1
    }

    //------------------------------------------------
    //  �^�C�}�[
    //------------------------------------------------
    public bool Timer(int time)
    {
        timer += Time.deltaTime;
        if(timer >= time)
        {
            timer = 0;
            return true;
        }

        return false;
    }

    //------------------------------------------------
    //  �X�e�[�W�P�̃U�R�G�̏o���p�^�[��
    //------------------------------------------------
    public void AppearEnemy_Stage01()
    {
        //  �A�C�e���h���b�v�͂P�X�e�[�W�T����x�I

        switch(appearStep)
        {
            case 0: //  �ҋ@

                break;
            case 1:
                SetEnemy(
                        enemyPrefab[(int)EnemyPattern.E01_3B],
                        new Vector3(GetRandomX(),appearY,0));
                appearStep++;
                break;
            case 2:
                if(Timer(3))appearStep++;
                break;
            case 3:
                SetEnemy(
                        enemyPrefab[(int)EnemyPattern.E01_3B],
                        new Vector3(GetRandomX(),appearY,0));
                appearStep++;
                break;
            case 4:
                if (Timer(3)) appearStep++;
                break;
            case 5:
                SetEnemy(
                        enemyPrefab[(int)EnemyPattern.E01_3B],
                        new Vector3(GetRandomX(), appearY, 0));
                appearStep++;
                break;
            case 6:
                if (Timer(3)) appearStep++;
                break;
            case 7:
                break;
                //case 8:
                //    SetEnemy(
                //            enemyPrefab[(int)EnemyPattern.E01_3x1],
                //            new Vector3(GetRandomX(),appearY,0));
                //    break;
                //case 9:
                //    if(Timer(3))appearStep++;
                //    break;
                //case 10:
                //    SetEnemy(
                //            enemyPrefab[(int)EnemyPattern.E01_3x1],
                //            new Vector3(GetRandomX(),appearY,0));
                //    break;
                //case 11:
                //    if(Timer(3))appearStep++;
                //    break;
        }
    }
}
