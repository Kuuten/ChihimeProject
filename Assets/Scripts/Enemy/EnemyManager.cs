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

    //private int enemyDestroyMaxNum = 61;    //  １ステージの最大敵数
    private int enemyGenerateNum = 0;       //  生成された敵数
    private int enemyDestroyNum = 0;         //  破壊された敵数

    //  ドロップするアイテムのプレハブ達
    private List<GameObject> dropItems;

    AsyncOperationHandle<IList<GameObject>> loadHandle;

    //  パワーアップアイテムを落とすかどうか
    private DROP_TYPE dropType = DROP_TYPE.None;


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
        dropItems = new List<GameObject>();

        //  一応リストをリセットしておく
        dropItems.Clear();

        // ロードする為のキー
        List<string> keys = new List<string>();
        List<string> keys1 = new List<string>() {"KonItems"};
        List<string> keys2 = new List<string>() {"KonItems", "PowerupItems"};

        //  ドロップなしならパワーアップアイテムのロードはなし
        if(dropType == DROP_TYPE.None)
        {
            keys = keys1;
        }
        else // ドロップあり
        {
            keys = keys2;
        }

        //  キーによるプレハブロード
        await LoadAssetsByAdrressablesKey(keys);
    }

    //------------------------------------------------
    //  キーによるプレハブロード
    //------------------------------------------------
    private async UniTask LoadAssetsByAdrressablesKey(List<string> keys)
    {
        //  アイテムをロード
        loadHandle = Addressables.LoadAssetsAsync<GameObject>
            (
                keys,
                null,
                Addressables.MergeMode.Union,
                false
            );

        //  ロードの完了を待つ
        await loadHandle.Task;

        //  要素の数だけリストに追加
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
         // 解放
        Addressables.Release(loadHandle);
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
    public DROP_TYPE GetDropType(){ return dropType; }
    public List<GameObject> GetDropItems(){ return dropItems; }

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
