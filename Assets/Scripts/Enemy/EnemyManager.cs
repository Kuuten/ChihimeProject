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
//  敵の管理クラス
//
//--------------------------------------------------------------

public class EnemyManager : MonoBehaviour
{
    //  シングルトンなインスタンス
    public static EnemyManager Instance
    {
        get; private set;
    }

    // 敵の出現セット 
    public enum EnemyPattern
    {
        //  １面
        E01_3,    //  縦３
        E01_3B,   //  縦３B

        Max
    }

    //  敵の種類
    public enum EnemyType
    {
        Kooni,          //  子鬼
        Onibi,          //  鬼火
        Ibarakidouji,   //  茨城童子
        Douji,          //  ボス・ドウジ
    
        Max
    }

    //  パワーアップアイテムを落とすかどうか
    public enum DROP_TYPE {
        None,                //  なし
        Drop,                //  あり
    }

    [SerializeField] private GameObject[] enemyPrefab;
    private const float appearY = -5.5f;

    private int appearStep = 0;
    private float timer = 0;

    //private int enemyDestroyMaxNum = 61;    //  １ステージの最大敵数
    private int enemyGenerateNum = 0;       //  生成された敵数
    private int enemyDestroyNum = 0;         //  破壊された敵数

    //  ドロップする魂のプレハブ達
    private List<GameObject> konItems;
    AsyncOperationHandle<IList<GameObject>> loadHandleKon;

    //  パワーアイテムのプレハブ達
    private List<GameObject> powerupItems;
    AsyncOperationHandle<IList<GameObject>> loadHandlePowerup;

    //  パワーアップアイテムを落とすかどうか
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

        //  プレハブのロード
        await LoadPrefab();
    }

    //-----------------------------------------------------------------
    //  プレハブのロード
    //-----------------------------------------------------------------
    private async UniTask LoadPrefab()
    {
        konItems = new List<GameObject>();
        powerupItems = new List<GameObject>();

        //  キーによるプレハブロード
        await LoadAssetsByAdrressablesKey();
    }

    //------------------------------------------------
    //  キーによるプレハブロード
    //------------------------------------------------
    private async UniTask LoadAssetsByAdrressablesKey()
    {
        // ロードする為のキー
        List<string> keys1 = new List<string>() {"KonItems"};
        List<string> keys2 = new List<string>() {"PowerupItems"};

        //  ドロップアイテムをロード
        loadHandleKon = Addressables.LoadAssetsAsync<GameObject>
            (
                keys1,
                null,
                Addressables.MergeMode.Union,
                false
            );

        //  ロードの完了を待つ
        await loadHandleKon.Task;

        //  パワーアップアイテムをロード
        loadHandlePowerup = Addressables.LoadAssetsAsync<GameObject>
            (
                keys2,
                null,
                Addressables.MergeMode.Union,
                false
            );

        //  ロードの完了を待つ
        await loadHandlePowerup.Task;

        //  要素の数だけリストに追加
        foreach (var addressable in loadHandleKon.Result)
        {
            if (addressable != null)
            {
                konItems.Add( addressable );
            }
        }

        //  ドロップ要素の数だけリストに追加
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
         // 解放
        Addressables.Release(loadHandleKon);
        Addressables.Release(loadHandlePowerup);
    }

    void Update()
    {
        AppearEnemy_Stage01();
        
    }

    //------------------------------------------------
    //  プロパティ
    //------------------------------------------------
    public int GetDestroyNum(){return enemyDestroyNum;}
    public void SetDestroyNum(int num){enemyDestroyNum = num;}
    public List<DROP_TYPE> GetDropType(){ return dropType; }
    public List<GameObject> GetKonItems(){ return konItems; }
    public List<GameObject> GetPowerupItems(){ return powerupItems; }

    //------------------------------------------------
    //  ランダムなX座標を返す
    //------------------------------------------------
    private float GetRandomX()
    {
        return Random.Range(-8.5f,-0.5f);
    }

    //------------------------------------------------
    //  指定の座標に指定の敵セットを生成する
    //------------------------------------------------
    private void SetEnemy(GameObject prefab,Vector3 pos)
    {
        Instantiate(prefab,pos,transform.rotation);

        enemyGenerateNum++; //  敵生成数+1
    }

    //------------------------------------------------
    //  タイマー
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
    //  ステージ１のザコ敵の出現パターン
    //------------------------------------------------
    public void AppearEnemy_Stage01()
    {
        //  アイテムドロップは１ステージ５回程度！

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
