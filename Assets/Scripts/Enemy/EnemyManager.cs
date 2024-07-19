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

    [SerializeField] private GameObject[] enemyPrefab;
    private const float appearY = -5.5f;

    private int appearStep = 0;
    private float timer = 0;

    //private int enemyDestroyMaxNum = 61;    //  �P�X�e�[�W�̍ő�G��
    private int enemyGenerateNum = 0;       //  �������ꂽ�G��
    private int enemyDestroyNum = 0;         //  �j�󂳂ꂽ�G��

    //  �h���b�v����A�C�e���̃v���n�u�B
    private List<GameObject> dropItems;

    AsyncOperationHandle<IList<GameObject>> loadHandle;

    //  �p���[�A�b�v�A�C�e���𗎂Ƃ����ǂ���
    private DROP_TYPE dropType = DROP_TYPE.None;


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
        dropItems = new List<GameObject>();

        //  �ꉞ���X�g�����Z�b�g���Ă���
        dropItems.Clear();

        // ���[�h����ׂ̃L�[
        List<string> keys = new List<string>();
        List<string> keys1 = new List<string>() {"KonItems"};
        List<string> keys2 = new List<string>() {"KonItems", "PowerupItems"};

        //  �h���b�v�Ȃ��Ȃ�p���[�A�b�v�A�C�e���̃��[�h�͂Ȃ�
        if(dropType == DROP_TYPE.None)
        {
            keys = keys1;
        }
        else // �h���b�v����
        {
            keys = keys2;
        }

        //  �L�[�ɂ��v���n�u���[�h
        await LoadAssetsByAdrressablesKey(keys);
    }

    //------------------------------------------------
    //  �L�[�ɂ��v���n�u���[�h
    //------------------------------------------------
    private async UniTask LoadAssetsByAdrressablesKey(List<string> keys)
    {
        //  �A�C�e�������[�h
        loadHandle = Addressables.LoadAssetsAsync<GameObject>
            (
                keys,
                null,
                Addressables.MergeMode.Union,
                false
            );

        //  ���[�h�̊�����҂�
        await loadHandle.Task;

        //  �v�f�̐��������X�g�ɒǉ�
        foreach (var addressable in loadHandle.Result)
        {
            if (addressable != null)
            {
                dropItems.Add( addressable );
            }
        }
    }

    private void OnDestroy()
    {
         // ���
        Addressables.Release(loadHandle);
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
    public DROP_TYPE GetDropType(){ return dropType; }
    public List<GameObject> GetDropItems(){ return dropItems; }

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
                        enemyPrefab[(int)EnemyPattern.E01_3],
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
            //case 3:
            //    if(Timer(3))appearStep++;
            //    break;
            //case 4:
            //    SetEnemy(
            //            enemyPrefab[(int)EnemyPattern.E01_3x1],
            //            new Vector3(GetRandomX(),appearY,0));
            //    break;
            //case 5:
            //    if(Timer(3))appearStep++;
            //    break;
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
