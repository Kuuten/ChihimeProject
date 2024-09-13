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
        E01,     //  子鬼
        E01_B,   //  子鬼B
        E02,     //  ヒトダマ
        E02_B,   //  ヒトダマB
        E03,     //  茨木童子

        Max
    }

    //  パワーアップアイテムを落とすかどうか
    public enum DROP_TYPE {
        None,                //  なし
        Drop,                //  あり
    }

    [SerializeField] private GameObject[] enemyPrefab;
    private const float appearY = -5.5f;


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

    //  敵情報
    private EnemySetting enemySetting;
    AsyncOperationHandle<IList<EnemySetting>> loadHandleEnemySetting;

    //  EnemyDataクラスからの情報取得用
    private EnemyData enemyData;

   //  敵の弾プレハブ
    [SerializeField] private GameObject[] enemyBullet;

    //  デバッグ用
    private InputAction test;

    //----------------------------------------------------
    //  移動経路用スポナー＆制御点のトランスフォーム
    //----------------------------------------------------
    [SerializeField] private Transform[] spawners;       //  スポナー
    [SerializeField] private Transform[] controlPoints;  //  制御点

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

        /* デバッグ用 */
        PlayerInput playerInput = GameManager.Instance.GetPlayer().GetComponent<PlayerInput>();
        test = playerInput.actions["TestButton2"];

        //  敵情報のロード
        //  ※EmemySettingの読み込みまではここでやる
        //  EnemyDataへの設定は生成時にEnemyにセットする
        await LoadEnemySetting();

        Debug.Log("敵情報ファイルのロード完了");

        //  プレハブのロード
        await LoadPrefab();

        Debug.Log("ドロップアイテムのロード完了");

        //  敵の出現開始
        StartCoroutine( AppearEnemy_Stage01() );
    }

    //-----------------------------------------------------------------
    //  敵情報のロード
    //-----------------------------------------------------------------
    private async UniTask LoadEnemySetting()
    {
        enemySetting = new EnemySetting();

        //  キーによる敵情報のロード
        await LoadEnemySettingByKey();
    }

    //-----------------------------------------------------------------
    //  キーによる敵情報のロード
    //-----------------------------------------------------------------
    private async UniTask LoadEnemySettingByKey()
    {
        // ロードする為のキー
        List<string> keys = new List<string>() {"EnemySetting"};

        //  敵情報をロード
        loadHandleEnemySetting = Addressables.LoadAssetsAsync<EnemySetting>
            (
                keys,
                null,
                Addressables.MergeMode.Union,
                false
            );

        //  ロードの完了を待つ
        await loadHandleEnemySetting.Task;

        //  ロード結果を格納
        enemySetting = loadHandleEnemySetting.Result.First();

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
        try {
            Addressables.Release(loadHandleKon);
            Addressables.Release(loadHandlePowerup);
            Addressables.Release(loadHandleEnemySetting);
        } catch (Exception e) {
            // 例外発生時の処理
            Debug.Log("loadHandleKonが未使用です");
            Debug.Log("loadHandlePowerupが未使用です");
            Debug.Log("loadHandleEnemySettingが未使用です");
            Debug.Log(e.Message);
        }

    }

    void Update()
    {
        //  Eneterで敵出現
        if (test.WasPressedThisFrame())
        {

            //  敵の出現開始
            StartCoroutine( AppearEnemy_Stage01() );
        }
    }

    //------------------------------------------------
    //  プロパティ
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
    //  ランダムなX座標を返す
    //------------------------------------------------
    private float GetRandomX()
    {
        return UnityEngine.Random.Range(-7.7f,6.07f);
    }

    //------------------------------------------------
    //  指定の座標に指定の敵セットを生成する
    //------------------------------------------------
    private void SetEnemy(GameObject prefab,Vector3 pos)
    {
        //  敵オブジェクトの生成
        GameObject obj = Instantiate(prefab,pos,transform.rotation);

        //  敵情報の設定
        obj.GetComponent<Enemy>().SetEnemyData(enemySetting);

        enemyGenerateNum++; //  敵生成数+1
    }

    //------------------------------------------------
    //  ステージ１のザコ敵の出現パターン
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
