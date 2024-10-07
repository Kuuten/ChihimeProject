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

//  ボスの種類
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
        EX,      //  ちっちゃいおっさん

        Max
    }

    //  パワーアップアイテムを落とすかどうか
    public enum DROP_TYPE {
        None,                //  なし
        Drop,                //  あり
    }

    [SerializeField] private GameObject[] enemyPrefab;
    private const float appearY = -6.5f;


    private int enemyDestroyNum = 0;        //  破壊された敵数
    private bool endZakoStage;              //  ザコステージの終了フラグ

    //  ドロップする魂のプレハブ達
    private List<GameObject> konItems;
    AsyncOperationHandle<IList<GameObject>> loadHandleKon;

    //  パワーアイテムのプレハブ達
    private List<GameObject> powerupItems;
    AsyncOperationHandle<IList<GameObject>> loadHandlePowerup;

    //  スピードアイテムのプレハブ達
    private List<GameObject> speedupItems;
    AsyncOperationHandle<IList<GameObject>> loadHandleSpeedup;

    //  ボムアイテムのプレハブ達
    private List<GameObject> bombItems;
    AsyncOperationHandle<IList<GameObject>> loadHandleBomb;

    //  ヒールアイテムのプレハブ達
    private List<GameObject> healItems;
    AsyncOperationHandle<IList<GameObject>> loadHandleHeal;

    //  パワーアップアイテムを落とすかどうか
    private List<DROP_TYPE> dropType = new List<DROP_TYPE>();

    //  敵情報
    private EnemySetting enemySetting;
    AsyncOperationHandle<IList<EnemySetting>> loadHandleEnemySetting;

    //  EnemyDataクラスからの情報取得用
    private EnemyData enemyData;

   //  敵の弾プレハブ
    [SerializeField] private GameObject[] enemyBullet;

    //----------------------------------------------------
    //  移動経路用スポナー＆制御点のトランスフォーム
    //----------------------------------------------------
    [SerializeField] private Transform[] spawners;       //  スポナー
    [SerializeField] private Transform[] controlPoints;  //  制御点

    //  イベントシーンマネージャーオブジェクト
    [SerializeField] private GameObject eventSceneManager;

    //  ボスのキャンバス
    [SerializeField] private GameObject bossCanavs;

    //  中ボスのHPスライダー
    [SerializeField] private GameObject midBossSlider;

    //  ボスのHPスライダー
    [SerializeField] private GameObject bossSlider;

    //  ディレクショナルライト
    [SerializeField] private Light directionalLight;

    //  WARNING時のラインを描画するキャンバス(位置取得用)
    [SerializeField] private GameObject dangerLineCanvas;

    //  敵オブジェクトのリスト
    List<GameObject> enemyObjList;

    //  HPスライダーのボス名
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

        //  敵情報のロード
        //  ※EmemySettingの読み込みまではここでやる
        //  EnemyDataへの設定は生成時にEnemyにセットする
        await LoadEnemySetting();

        Debug.Log("敵情報ファイルのロード完了");

        //  プレハブのロード
        await LoadPrefab();

        Debug.Log("ドロップアイテムのロード完了");
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
        speedupItems = new List<GameObject>();
        bombItems = new List<GameObject>();
        healItems = new List<GameObject>();

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
        List<string> keys3 = new List<string>() {"SpeedupItems"};
        List<string> keys4 = new List<string>() {"BombItems"};
        List<string> keys5 = new List<string>() {"HealItems"};

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

        //  スピードアップアイテムをロード
        loadHandleSpeedup = Addressables.LoadAssetsAsync<GameObject>
            (
                keys3,
                null,
                Addressables.MergeMode.Union,
                false
            );

        //  ロードの完了を待つ
        await loadHandleSpeedup.Task;

        //  ボムアイテムをロード
        loadHandleBomb = Addressables.LoadAssetsAsync<GameObject>
            (
                keys4,
                null,
                Addressables.MergeMode.Union,
                false
            );

        //  ロードの完了を待つ
        await loadHandleBomb.Task;

        //  ヒールアイテムをロード
        loadHandleHeal = Addressables.LoadAssetsAsync<GameObject>
            (
                keys5,
                null,
                Addressables.MergeMode.Union,
                false
            );

        //  ロードの完了を待つ
        await loadHandleHeal.Task;

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

        //  ドロップ要素の数だけリストに追加
        foreach (var addressable in loadHandleSpeedup.Result)
        {
            if (addressable != null)
            {
                speedupItems.Add( addressable );
            }
        }

        //  ドロップ要素の数だけリストに追加
        foreach (var addressable in loadHandleBomb.Result)
        {
            if (addressable != null)
            {
                bombItems.Add( addressable );
            }
        }

        //  ドロップ要素の数だけリストに追加
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
        // 解放
        try {
            Addressables.Release(loadHandleKon);
            Addressables.Release(loadHandlePowerup);
            Addressables.Release(loadHandleSpeedup);
            Addressables.Release(loadHandleBomb);
            Addressables.Release(loadHandleHeal);
            Addressables.Release(loadHandleEnemySetting);
        } catch (Exception e) {
            // 例外発生時の処理
            Debug.Log("loadHandleKonが未使用です");
            Debug.Log("loadHandlePowerupが未使用です");
            Debug.Log("loadHandleSpeedupが未使用です");
            Debug.Log("loadHandleBombが未使用です");
            Debug.Log("loadHandleHealが未使用です");
            Debug.Log("loadHandleEnemySettingが未使用です");
            Debug.Log(e.Message);
        }

    }

    void Update()
    {
        //  敵リストを監視して死んでいたらリストから削除
        for(int i = 0;i<enemyObjList.Count;i++)
        {
            if(enemyObjList[i].gameObject == null)
            {
                enemyObjList.Remove(enemyObjList[i]);
            }
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
    //  ランダムなX座標を返す
    //------------------------------------------------
    private float GetRandomX()
    {
        return UnityEngine.Random.Range(-7.7f,6.07f);
    }

    //------------------------------------------------
    //  指定の座標に指定の敵セットを生成する
    //------------------------------------------------
    public GameObject SetEnemy(GameObject prefab,Vector3 pos,ePowerupItems item = ePowerupItems.None)
    {
        //  敵オブジェクトの生成
        enemyObjList.Add(Instantiate(prefab,pos,transform.rotation));

        //  敵情報の設定
        enemyObjList.Last().GetComponent<Enemy>().SetEnemyData(enemySetting, item);

        return enemyObjList.Last();
    }

    //------------------------------------------------
    //  ボスの敵を情報をセットする
    //------------------------------------------------
    public void SetBoss(BossType bossType,ePowerupItems item)
    {
        //  敵情報の設定
        GameObject obj = EventSceneManager.Instance.GetBossObject();

        //  ステータスを設定
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
    //  今いるステージによって敵出現パターンを呼び分ける
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
    //  プレイヤーに一番近い敵オブジェクトを検索して返す
    //-----------------------------------------------------------
    public GameObject GetNearestEnemyFromPlayer()
    {
        //  プレイヤーの座標を取得
        Vector3 playerPos = GameManager.Instance.GetPlayer().transform.position;

        //  全ての敵リストの敵と距離を比較
        float d1 = 0f;          //  今回の敵との距離
        float d2 = 100f;        //  前回の敵との距離(適当な大きい値)
        int nearestNum = -1;    //  一番近い敵のリスト番号

        //  敵がいない場合はnull
        if(enemyObjList.Count == 0)return null;

        for(int i = 0;i<enemyObjList.Count;i++)
        {
            //  リストのi番目の敵とプレイヤーの距離を算出
            d1 = Vector3.Distance(enemyObjList[i].transform.position, playerPos);

            //  今回の方が近かったらそれをd2にコピー
            if(d1 < d2)
            {
                d2 = d1;

                //  一番近い敵のリスト番号をコピー
                nearestNum = i;
            }
        }
        return enemyObjList[nearestNum];
    }

    //-----------------------------------------------------------
    //  ステージ１のザコ敵の出現パターン
    //-----------------------------------------------------------
    public IEnumerator AppearEnemy_Stage01()
    {
        //  ザコ戦BGM開始
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

        //  中ボスのHPスライダーを生成
        GameObject midSliderObject = Instantiate(midBossSlider);
        midSliderObject.transform.SetParent(bossCanavs.transform, false);

        //  HPスライダーを設定
        MidBoss.GetComponent<Enemy>().SetHpSlider(midSliderObject.GetComponent<Slider>());

        //  ザコ戦終了フラグがTRUEになるまで待つ
        yield return new WaitUntil(()=> endZakoStage == true);

        //  中ボスのスライダーを削除
        Destroy(midSliderObject);

        //  PlayerのShotManagerを初期化する
        GameObject player = GameManager.Instance.GetPlayer();
        player.GetComponent<PlayerShotManager>().DisableShot();

        //  プレイヤーが死んでいたらbreak
        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if(ph.GetCurrentHealth() <= 0)yield break;

        //  ５秒待つ
        yield return new WaitForSeconds(5);

        //  ディレクショナルライトを無効化
        directionalLight.enabled = false;

        //  ポーズを無効化
        GameManager.Instance.GetPauseAction().Disable();

        //  イベントモードへ移行
        GameManager.Instance.SetGameState((int)eGameState.Event);

        //  イベントシーンマネージャーを有効化
        eventSceneManager.SetActive(true);

        //  ボス戦開始フラグがTRUEになるまで待つ(ボス戦モード)
        yield return new WaitUntil(()=> EventSceneManager.Instance.GetStartBoss());

        /***********************ここからボス戦***********************/

        //  ポーズを有効化
        GameManager.Instance.GetPauseAction().Enable();

        //  ボスBGM再生
        //SoundManager.Instance.PlayBGM((int)MusicList.BGM_DOUJI_STAGE_BOSS);

        //  イベントシーンマネージャーを無効化
        eventSceneManager.SetActive(false);

        //  ショットを有効化
        player.GetComponent<PlayerShotManager>().EnableShot();

        //  ボスのHPスライダーを生成
        GameObject sliderObject = Instantiate(bossSlider);
        sliderObject.transform.SetParent(bossCanavs.transform, false);

        //  ボスごとにHPスライダーを設定
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

        //  ボスコンポーネントを有効化
        GameObject boss = EventSceneManager.Instance.GetBossObject();
        boss.GetComponent<BossDouji>().enabled = true;
        boss.GetComponent<BoxCollider2D>().enabled = true;

        //  ボス戦終了フラグがTRUEになるまで待つ
        yield return new WaitUntil(() => GameManager.Instance.GetStageClearFlag());

        //  ポーズを無効化
        GameManager.Instance.GetPauseAction().Disable();

        //  左右の障気オブジェクトを無効化
        EventSceneManager.Instance.GetFogObjectL().SetActive(false);
        EventSceneManager.Instance.GetFogObjectR().SetActive(false);

        //  ボスのHPキャンバスを非表示
        bossCanavs.SetActive(false);

        // ショットを無効化
        player.GetComponent<PlayerShotManager>().DisableShot();

        //  ５秒待つ
        yield return new WaitForSeconds(5);

        //  Playerのショットを初期化
        player.GetComponent<PlayerShotManager>().InitShot();

        //  イベントモードへ移行
        GameManager.Instance.SetGameState((int)eGameState.Event);

        //  イベントシーンマネージャーを有効化
        eventSceneManager.SetActive(true);
    }

    //-----------------------------------------------------------
    //  ステージ２のザコ敵の出現パターン
    //-----------------------------------------------------------
    public IEnumerator AppearEnemy_Stage02()
    {
        yield return null;
    }

    //-----------------------------------------------------------
    //  ステージ３のザコ敵の出現パターン
    //-----------------------------------------------------------
    public IEnumerator AppearEnemy_Stage03()
    {
        yield return null;
    }

    //-----------------------------------------------------------
    //  ステージ４のザコ敵の出現パターン
    //-----------------------------------------------------------
    public IEnumerator AppearEnemy_Stage04()
    {
        yield return null;
    }

    //-----------------------------------------------------------
    //  ステージ５のザコ敵の出現パターン
    //-----------------------------------------------------------
    public IEnumerator AppearEnemy_Stage05()
    {
        yield return null;
    }

    //-----------------------------------------------------------
    //  ステージ６のザコ敵の出現パターン
    //-----------------------------------------------------------
    public IEnumerator AppearEnemy_Stage06()
    {
        yield return null;
    }
}
