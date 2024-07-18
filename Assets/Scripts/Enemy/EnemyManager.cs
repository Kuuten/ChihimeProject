using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

//--------------------------------------------------------------
//
//  �G�̊Ǘ��N���X
//
//--------------------------------------------------------------

// �G�̏o���Z�b�g 
public enum EnemyPattern
{
    //  �P��
    E01_3x1,    //  �c�R�����P
    E01_3x1B,   //  �c�R�����PB

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

public class EnemyManager : MonoBehaviour
{
    //  �V���O���g���ȃC���X�^���X
    public static EnemyManager Instance
    {
        get; private set;
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
    EnemySetting enemySetting;

    private int appearStep = 0;
    private float timer = 0;
    private int waveNum = 0;    //  �P�X�e�[�W������̓G�E�F�[�u��


    void Start()
    {
        appearStep = 0;
        timer = 0;

    }

    private void OnDestroy()
    {
        // ���
        Addressables.Release(enemySetting);
    }

    void Update()
    {
        AppearEnemy_Stage01();
        
    }

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
                        enemyPrefab[(int)EnemyPattern.E01_3x1B],
                        new Vector3(GetRandomX(),appearY,0));
                appearStep++;
                break;
            case 1:
                if(Timer(3))appearStep++;
                break;
            //case 2:
            //    SetEnemy(
            //        enemyPrefab[(int)EnemyPattern.E01_3x1],
            //        new Vector3(GetRandomX(),appearY,0));
            //    appearStep++;
            //    break;
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
