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
        E01,     //  �q�S
        E01_B,   //  �q�SB
        E02,     //  �q�g�_�}
        E02_B,   //  �q�g�_�}B
        E03,     //  ��ؓ��q

        Max
    }

    //  �p���[�A�b�v�A�C�e���𗎂Ƃ����ǂ���
    public enum DROP_TYPE {
        None,                //  �Ȃ�
        Drop,                //  ����
    }

    [SerializeField] private GameObject[] enemyPrefab;
    private const float appearY = -5.5f;


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

   //  �G�̒e�v���n�u
    [SerializeField] private GameObject[] enemyBullet;

    //  �f�o�b�O�p
    private InputAction test;

    //----------------------------------------------------
    //  �ړ��o�H�p�X�|�i�[������_�̃g�����X�t�H�[��
    //----------------------------------------------------
    [SerializeField] private Transform[] spawners;       //  �X�|�i�[
    [SerializeField] private Transform[] controlPoints;  //  ����_

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
        enemyGenerateNum = 0;
        enemyDestroyNum = 0;

        /* �f�o�b�O�p */
        PlayerInput playerInput = GameManager.Instance.GetPlayer().GetComponent<PlayerInput>();
        test = playerInput.actions["TestButton2"];

        //  �G���̃��[�h
        //  ��EmemySetting�̓ǂݍ��݂܂ł͂����ł��
        //  EnemyData�ւ̐ݒ�͐�������Enemy�ɃZ�b�g����
        await LoadEnemySetting();

        Debug.Log("�G���t�@�C���̃��[�h����");

        //  �v���n�u�̃��[�h
        await LoadPrefab();

        Debug.Log("�h���b�v�A�C�e���̃��[�h����");

        //  �G�̏o���J�n
        StartCoroutine( AppearEnemy_Stage01() );
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
            Debug.Log(e.Message);
        }

    }

    void Update()
    {
        //  Eneter�œG�o��
        if (test.WasPressedThisFrame())
        {

            //  �G�̏o���J�n
            StartCoroutine( AppearEnemy_Stage01() );
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
    public Vector3 GetSpawnerPos(int i){ return spawners[i].position; }
    public Vector3 GetControlPointPos(int i){ return controlPoints[i].position; }
    public GameObject GetBulletPrefab(int type){ return enemyBullet[type]; }

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
    //  �X�e�[�W�P�̃U�R�G�̏o���p�^�[��
    //------------------------------------------------
    private IEnumerator AppearEnemy_Stage01()
    {
        yield return new WaitForSeconds(3f);

        //  Wave1
        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(5.0f);

        //  Wave2
        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(0.32f);

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(1.0f);

        //  Wave3
        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(0.32f);

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(0.3f);

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01_B],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(1.0f);

        //  Wave4
        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E02],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(1.0f);

        //  Wave5
        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E02],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(1.0f);

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E02],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(1.0f);

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E02_B],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(5.0f);

        //  Wave6
        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(0.32f);

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(0.3f);

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01_B],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(0.3f);

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(0.3f);

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(0.3f);

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01_B],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(1.0f);

        //  Wave7
        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(0.32f);

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(0.3f);

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01_B],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(0.3f);

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(0.32f);

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(0.3f);

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01_B],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(0.3f);

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(0.3f);

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(0.3f);

        yield return new WaitForSeconds(1.0f);

        //  Wave8
        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(0.32f);

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(0.3f);

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(0.3f);

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(0.32f);

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(0.3f);

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(0.3f);

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(0.32f);

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(0.3f);

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(0.3f);

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01_B],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(0.3f);

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E02],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(1.0f);

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E02],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(1.0f);

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E02_B],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(1.0f);

        //  Wave9
        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01_B],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(1.0f);

        //  wave10
        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01],
        new Vector3(GetRandomX(), appearY, 0));

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E01_B],
        new Vector3(GetRandomX(), appearY, 0));

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E02],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(1.0f);

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E02],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(1.0f);

        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E02_B],
        new Vector3(GetRandomX(), appearY, 0));

        yield return new WaitForSeconds(5.0f);

        //  Wave11
        SetEnemy(
        enemyPrefab[(int)EnemyPattern.E03],
        new Vector3(-1, -6, 0));

        yield return null;
    }
}
