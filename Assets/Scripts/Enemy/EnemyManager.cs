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

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

    }

    private async void Start()
    {
        appearStep = 0;
        timer = 0;
        enemyGenerateNum = 0;
        enemyDestroyNum = 0;

        //  �v���n�u�̃��[�h
        await LoadPrefab();
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
        Addressables.Release(loadHandleKon);
        Addressables.Release(loadHandlePowerup);
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
        return Random.Range(-8.5f,-0.5f);
    }

    //------------------------------------------------
    //  �w��̍��W�Ɏw��̓G�Z�b�g�𐶐�����
    //------------------------------------------------
    private void SetEnemy(GameObject prefab,Vector3 pos)
    {
        Instantiate(prefab,pos,transform.rotation);

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
            case 0:
                SetEnemy(
                        enemyPrefab[(int)EnemyPattern.E01_3B],
                        new Vector3(GetRandomX(),appearY,0));
                appearStep++;
                break;
            case 1:
                if(Timer(3))appearStep++;
                break;
            case 2:
                SetEnemy(
                        enemyPrefab[(int)EnemyPattern.E01_3B],
                        new Vector3(GetRandomX(),appearY,0));
                appearStep++;
                break;
            case 3:
                if (Timer(3)) appearStep++;
                break;
            case 4:
                SetEnemy(
                        enemyPrefab[(int)EnemyPattern.E01_3B],
                        new Vector3(GetRandomX(), appearY, 0));
                appearStep++;
                break;
            case 5:
                if (Timer(3)) appearStep++;
                break;
                //case 6:
                //    SetEnemy(
                //            enemyPrefab[(int)EnemyPattern.E01_3x1B],
                //            new Vector3(GetRandomX(),appearY,0));
                //    break;
                //case 7:
                //    if(Timer(3))appearStep++;
                //    break;
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
