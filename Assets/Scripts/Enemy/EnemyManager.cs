using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using System;
using TMPro;
using DG.Tweening;

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

// 敵の弾の種類
public enum BULLET_TYPE {
    //  青弾
    Snipe_Normal,        //  自機狙い弾・通常
    Snipe_RotationL,     //  自機狙い弾・左回転
    Snipe_RotationR,     //  自機狙い弾・右回転
    Snipe_Long,          //  自機狙い弾・ロング
    Snipe_Big,           //  自機狙い弾・大玉
    //  赤弾
    Wildly_Normal,       //  バラマキ弾・通常
    Wildly_RotationL,    //  バラマキ弾・左回転
    Wildly_RotationR,    //  バラマキ弾・右回転
    Wildly_Long,         //  バラマキ弾・ロング
    Wildly_Big,          //  バラマキ弾・大玉
    //  ドウジギミック弾
    Douji_Gimmick_Top,
    Douji_Gimmick_Bottom,
    Douji_Gimmick_Left,
    Douji_Gimmick_Right,
    //  ギミック警告
    Douji_Warning,
    //  ドウジ発狂弾
    Douji_Berserk_Bullet,
    //  警告の予測ライン
    Douji_DangerLine_Top,
    Douji_DangerLine_Bottom,
    Douji_DangerLine_Left,
    Douji_DangerLine_Right,
    //  ツクモPhase1弾
    Tsukumo_Gimmick_Top,
    Tsukumo_Gimmick_Bottom,
    Tsukumo_Gimmick_Left,
    Tsukumo_Gimmick_Right,
    //  ツクモPhase2ギミック弾
    Tsukumo_Gimmick,
    //  ツクモ発狂弾8
    Tsukumo_Berserk_Bullet,
    //  ツクモPhase1ホーミング弾
    Tsukumo_Phase1_Homing,
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

    // 敵の出現セット(全ステージ共通規格)
    public enum EnemyPattern
    {
        E01,        //  ザコ１
        E01_B,      //  ザコ１（ドロップ有り）
        E02,        //  ザコ２
        E02_B,      //  ザコ２（ドロップ有り）
        MidBoss,    //  中ボス
        EX,         //  ちっちゃいおっさん

        Max
    }

    // 特殊な敵の出現セット(全ステージ共通規格)
    public enum SpecialEnemyPattern
    {
        DollSiege,  // 人形包囲陣

        Max
    }

    //  パワーアップアイテムを落とすかどうか
    public enum DROP_TYPE {
        None,                //  なし
        Drop,                //  あり
    }

    [SerializeField] private GameObject[] enemyPrefab;
    [SerializeField] private GameObject[] specialEnemyPrefab;
    private const float appearY = -6.5f;


    private int enemyDestroyNum = 0;        //  破壊された敵数
    private bool endZakoStage;              //  ザコステージの終了フラグ

    //  ドロップする小魂のプレハブ
    private GameObject smallKonItem;
    AsyncOperationHandle<GameObject> loadHandleSmallKon;

    //  ドロップする小魂のプレハブ
    private GameObject largeKonItem;
    AsyncOperationHandle<GameObject> loadHandleLargeKon;

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

    //  ドロップアイテムのロード完了フラグ
    private bool isCompleteLoading;

    //  ツクモのホーミング弾用
    [SerializeField] private SpriteRenderer homingBullet;
    [SerializeField] private Animator homingAnimator;

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

    //  ステージ開始時に表示するテキスト
    [SerializeField] private TextMeshProUGUI stageStartingText;
    private static readonly string[] stageName = 
    {
        "ステージ１\n鬼の棲む山",
        "ステージ２\n九十九長屋",
        "ステージ３\n☆十二支喫茶えとわーる☆",
        "ステージ４\nクラマハイパーアリーナ",
        "ステージ５\nおねぇちゃんのお部屋",
        "ステージ６\n婚魂大社",
    };

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
        isCompleteLoading = false;

        //  敵情報のロード
        //  ※EmemySettingの読み込みまではここでやる
        //  EnemyDataへの設定は生成時にEnemyにセットする
        await LoadEnemySetting();

        Debug.Log("敵情報ファイルのロード完了");

        //  プレハブのロード
        await LoadPrefab();

        Debug.Log("ドロップアイテムのロード完了");

        isCompleteLoading = true;
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
        //smallKonItem = new GameObject();
        //largeKonItem = new GameObject();
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
        List<string> keys1 = new List<string>() {"SmallKon"};
        List<string> keys2 = new List<string>() {"LargeKon"};
        List<string> keys3 = new List<string>() {"PowerupItems"};
        List<string> keys4 = new List<string>() {"SpeedupItems"};
        List<string> keys5 = new List<string>() {"BombItems"};
        List<string> keys6 = new List<string>() {"HealItems"};

        //  小魂をロード
        loadHandleSmallKon = Addressables.LoadAssetAsync<GameObject>( "item_smallKon" );

        //  ロードの完了を待つ
        await loadHandleSmallKon.Task;

        //  大魂をロード
        loadHandleLargeKon = Addressables.LoadAssetAsync<GameObject>( "item_largeKon" );

        //  ロードの完了を待つ
        await loadHandleLargeKon.Task;

        //  パワーアップアイテムをロード
        loadHandlePowerup = Addressables.LoadAssetsAsync<GameObject>
            (
                keys3,
                null,
                Addressables.MergeMode.Union,
                false
            );

        //  ロードの完了を待つ
        await loadHandlePowerup.Task;

        //  スピードアップアイテムをロード
        loadHandleSpeedup = Addressables.LoadAssetsAsync<GameObject>
            (
                keys4,
                null,
                Addressables.MergeMode.Union,
                false
            );

        //  ロードの完了を待つ
        await loadHandleSpeedup.Task;

        //  ボムアイテムをロード
        loadHandleBomb = Addressables.LoadAssetsAsync<GameObject>
            (
                keys5,
                null,
                Addressables.MergeMode.Union,
                false
            );

        //  ロードの完了を待つ
        await loadHandleBomb.Task;

        //  ヒールアイテムをロード
        loadHandleHeal = Addressables.LoadAssetsAsync<GameObject>
            (
                keys6,
                null,
                Addressables.MergeMode.Union,
                false
            );

        //  ロードの完了を待つ
        await loadHandleHeal.Task;

        //  ロードした小魂をコピー
        if (loadHandleSmallKon.Result != null)
        {
            smallKonItem = loadHandleSmallKon.Result;
        }

        //  ロードした大魂をコピー
        if (loadHandleLargeKon.Result != null)
        {
            largeKonItem = loadHandleLargeKon.Result;
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
            Addressables.Release(loadHandleSmallKon);
            Addressables.Release(loadHandleLargeKon);
            Addressables.Release(loadHandlePowerup);
            Addressables.Release(loadHandleSpeedup);
            Addressables.Release(loadHandleBomb);
            Addressables.Release(loadHandleHeal);
            Addressables.Release(loadHandleEnemySetting);
        } catch (Exception e) {
            // 例外発生時の処理
            Debug.Log("loadHandleSmallKonが未使用です");
            Debug.Log("loadHandleLargeKonが未使用です");
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
        
    }

    //------------------------------------------------
    //  敵が死んでいたらリストから削除
    //------------------------------------------------
    public void DeleteEnemyFromList(GameObject obj)
    {
        if(obj == null)
        {
            enemyObjList.Remove(obj);
        } 
    }

    //------------------------------------------------
    //  敵を全削除
    //------------------------------------------------
    public void DeleteAllEnemy()
    {
        foreach(GameObject obj in enemyObjList)
        {
            if(obj)Destroy(obj);
        }
    }

    //------------------------------------------------
    //  リストに敵オブジェクトを追加
    //------------------------------------------------
    public void AddEnemyFromList(GameObject obj)
    {
        if(obj != null)
        {
            enemyObjList.Add(obj);
        }
        else
        {
            Debug.LogError("空のオブジェクトが引数に指定されています！");
        }
    }

    //------------------------------------------------
    //  プロパティ
    //------------------------------------------------
    public int GetDestroyNum(){return enemyDestroyNum;}
    public void SetDestroyNum(int num){enemyDestroyNum = num;}
    public List<DROP_TYPE> GetDropType(){ return dropType; }
    public GameObject GetSmallKon(){ return smallKonItem; }
    public GameObject GetLargeKon(){ return largeKonItem; }
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
    public bool GetIsCompleteLoading(){ return isCompleteLoading; }
    public List<GameObject> GetEnemyObjectList(){ return enemyObjList; }
    public EnemySetting GetEnemySetting(){ return enemySetting; }
    public SpriteRenderer GetHomingBulletSprite(){ return homingBullet; }
    public Animator GetHomingBulletAnimator(){ return homingAnimator; }

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
    //  画面中央を中心として周りに人形を展開
    //------------------------------------------------
    public void SetDollGroup(ePowerupItems item = ePowerupItems.None)
    {
        float totalDegree = 360;        //  撃つ範囲の総角  
        int wayNum = 8;                 //  弾のway数
        float Degree = totalDegree / wayNum;     //  弾一発毎にずらす角度
        float distance = 5f;            //  プレイヤーから離す距離

        //  プレイヤーの前方ベクトルを取得
        Vector3 vector0 = GameManager.Instance.GetPlayer().transform.up;
        Vector3[] vector = new Vector3[wayNum];

        for (int i = 0; i < wayNum; i++)
        {
            //  ベクトルの角度を設定
            vector[i] = Quaternion.Euler
                (0, 0, -Degree * i) * vector0;
            vector[i].z = 0f;

            //  敵を生成する座標を計算
            Vector3 playerPos = new Vector3(0,2,0);
            Vector3 pos = distance * vector[i] + playerPos;

            //  人形オブジェクトを生成
            enemyObjList.Add(Instantiate(specialEnemyPrefab[(int)SpecialEnemyPattern.DollSiege],pos,transform.rotation));

            //  敵情報の設定
            enemyObjList.Last().GetComponent<Enemy>().SetEnemyData(enemySetting, item);
        }
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
        else if (bossType == BossType.Tsukumo)
            obj.GetComponent<BossTsukumo>().SetBossData(enemySetting,item);
        //else if(bossType == BossType.Kuchinawa)
        //    obj.GetComponent<BossKuchinawa>().SetBossData(enemySetting,item);
        //else if(bossType == BossType.Kurama)
        //    obj.GetComponent<BossKurama>().SetBossData(enemySetting,item);
        //else if(bossType == BossType.Wadatsumi)
        //    obj.GetComponent<BossWadatsumi>().SetBossData(enemySetting,item);
        //else if(bossType == BossType.Hakumen)
        //    obj.GetComponent<BossHakumen>().SetBossData(enemySetting,item);
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
            if(enemyObjList[i].gameObject == null)continue;

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

        //  エラー回避
        if(nearestNum == -1)return null;

        return enemyObjList[nearestNum];
    }

    //-----------------------------------------------------------
    //  各ステージ開始時のステージ名演出
    //-----------------------------------------------------------
    private IEnumerator StagingLevelBeginning()
    {
        float fade_time = 2f;   //  フェードに要する時間(秒)

        //  ステージに応じたテキストを設定する
        stageStartingText.text = stageName[(int)PlayerInfoManager.stageInfo];

        //  テキストをフェードインさせる
        stageStartingText.DOFade(1.0f, fade_time);

        //  待つ
        yield return new WaitForSeconds(fade_time);

        //  テキストをフェードアウトさせる
        stageStartingText.DOFade(0.0f, fade_time);

        yield return null;
    }

    //-----------------------------------------------------------
    //  ステージ１のザコ敵の出現パターン
    //-----------------------------------------------------------
    public IEnumerator AppearEnemy_Stage01()
    {
        //  ザコ戦BGM開始
        SoundManager.Instance.PlayBGM((int)MusicList.BGM_DOUJI_STAGE_ZAKO);

        ////  ステージ開始演出
        //yield return StartCoroutine(StagingLevelBeginning());

        //yield return new WaitForSeconds(3f);

        ////  Wave1
        //SetEnemy(
        //    enemyPrefab[(int)EnemyPattern.E01],
        //    new Vector3(GetRandomX(), appearY, 0)
        //);

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
        //enemyPrefab[(int)EnemyPattern.E01],
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
        //new Vector3(GetRandomX(), appearY, 0), ePowerupItems.SpeedUp);

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
        //new Vector3(GetRandomX(), appearY, 0), ePowerupItems.Bomb);

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
        //new Vector3(GetRandomX(), appearY, 0), ePowerupItems.Bomb);

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
                                    enemyPrefab[(int)EnemyPattern.MidBoss],
                                    new Vector3(0, -4, 0),
                                    ePowerupItems.PowerUp
                                );

        //  中ボスのHPスライダーを生成
        GameObject midSliderObject = Instantiate(midBossSlider);
        midSliderObject.transform.SetParent(bossCanavs.transform, false);

        //  HPスライダーを設定
        MidBoss.GetComponent<Enemy>().SetHpSlider(midSliderObject.GetComponent<Slider>());

        //  ザコ戦終了フラグがTRUEになるまで待つ
        yield return new WaitUntil(() => endZakoStage == true);

        //  ザコ敵全削除
        DeleteAllEnemy();

        //  中ボスのスライダーを削除
        Destroy(midSliderObject);

        //  PlayerのShotManagerを初期化する
        GameObject player = GameManager.Instance.GetPlayer();
        player.GetComponent<PlayerShotManager>().DisableShot();

        //  BombManagerを無効化する
        player.GetComponent<PlayerBombManager>().DisableBomb();

        //  プレイヤーが死んでいたらbreak
        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph.GetCurrentHealth() <= 0) yield break;

        //  ５秒待つ
        yield return new WaitForSeconds(5);

        //  ポーズを無効化
        GameManager.Instance.GetPauseAction().Disable();

        //  イベントモードへ移行
        GameManager.Instance.SetGameState((int)eGameState.Event);

        //  イベントシーンマネージャーを有効化
        eventSceneManager.SetActive(true);

        //  ボス戦開始フラグがTRUEになるまで待つ(ボス戦モード)
        yield return new WaitUntil(() => EventSceneManager.Instance.GetStartBoss());

        /***********************ここからボス戦***********************/

        //  ポーズを有効化
        GameManager.Instance.GetPauseAction().Enable();

        //  ボスBGM再生
        SoundManager.Instance.PlayBGM((int)MusicList.BGM_DOUJI_STAGE_BOSS);

        //  イベントシーンマネージャーを無効化
        eventSceneManager.SetActive(false);

        //  ショットを有効化
        player.GetComponent<PlayerShotManager>().EnableShot();

        //  BombManagerを有効化する
        player.GetComponent<PlayerBombManager>().EnableBomb();

        //  ボスのHPスライダーを生成
        GameObject sliderObject = Instantiate(bossSlider);
        sliderObject.transform.SetParent(bossCanavs.transform, false);


        //  ボスオブジェクトを取得
        GameObject BossObj = EventSceneManager.Instance.GetBossObject();
        if(!BossObj) Debug.LogError("ボスオブジェクトがnullになっています！");


        //  ボスコンポーネントを取得
        BossDouji boss_douji = BossObj.GetComponent<BossDouji>();
        if(!boss_douji)Debug.LogError("ボスはBossDoujiコンポーネントを持っていません！\n" +
            "PlayerInfoManager.stageInfoの値を確認してください");


        //  ボスごとにHPスライダーを設定
        boss_douji.SetHpSlider(sliderObject.GetComponent<Slider>());
        bossNameHPSlider[(int)BossType.Tsukumo].SetActive(true);

        //  ボスコンポーネントを有効化
        boss_douji.enabled = true;
        BossObj.GetComponent<BoxCollider2D>().enabled = true;

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

        //  BombManagerを無効化する
        player.GetComponent<PlayerBombManager>().DisableBomb();

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
        //  ザコ戦BGM開始
        SoundManager.Instance.PlayBGM((int)MusicList.BGM_TSUKUMO_STAGE_ZAKO);

        ////  ステージ開始演出
        //yield return StartCoroutine(StagingLevelBeginning());

        //yield return new WaitForSeconds(3f);

        ////  Wave1
        //SetEnemy(
        //    enemyPrefab[(int)EnemyPattern.E01],
        //    new Vector3(GetRandomX(), appearY, 0)
        //);

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
        //enemyPrefab[(int)EnemyPattern.E01],
        //new Vector3(GetRandomX(), appearY, 0), ePowerupItems.PowerUp);

        //yield return new WaitForSeconds(1.0f);

        ////  Wave4
        //SetEnemy(
        //enemyPrefab[(int)EnemyPattern.E02],
        //new Vector3(GetRandomX(), appearY, 0));

        ////  人形包囲陣
        //SetDollGroup();

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
        //new Vector3(GetRandomX(), appearY, 0), ePowerupItems.SpeedUp);

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
        //new Vector3(GetRandomX(), appearY, 0), ePowerupItems.Bomb);

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
        ////  人形包囲陣
        //SetDollGroup();

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
        //new Vector3(GetRandomX(), appearY, 0), ePowerupItems.Bomb);

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

        //yield return new WaitForSeconds(8.0f);

        //  WaveX
        GameObject MidBoss = SetEnemy(
                                    enemyPrefab[(int)EnemyPattern.MidBoss],
                                    new Vector3(0, -6, 0),
                                    ePowerupItems.PowerUp
                                );

        //  中ボスのHPスライダーを生成
        GameObject midSliderObject = Instantiate(midBossSlider);
        midSliderObject.transform.SetParent(bossCanavs.transform, false);

        //  HPスライダーを設定
        MidBoss.GetComponent<Enemy>().SetHpSlider(midSliderObject.GetComponent<Slider>());

        //  ザコ戦終了フラグがTRUEになるまで待つ
        yield return new WaitUntil(() => endZakoStage == true);

        //  ザコ敵全削除
        DeleteAllEnemy();

        //  中ボスのスライダーを削除
        Destroy(midSliderObject);

        //  PlayerのShotManagerを初期化する
        GameObject player = GameManager.Instance.GetPlayer();
        player.GetComponent<PlayerShotManager>().DisableShot();

        //  BombManagerを無効化する
        player.GetComponent<PlayerBombManager>().DisableBomb();

        //  プレイヤーが死んでいたらbreak
        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph.GetCurrentHealth() <= 0) yield break;

        //  ５秒待つ
        yield return new WaitForSeconds(5);

        //  ポーズを無効化
        GameManager.Instance.GetPauseAction().Disable();

        //  イベントモードへ移行
        GameManager.Instance.SetGameState((int)eGameState.Event);

        //  イベントシーンマネージャーを有効化
        eventSceneManager.SetActive(true);

        //  ボス戦開始フラグがTRUEになるまで待つ(ボス戦モード)
        yield return new WaitUntil(() => EventSceneManager.Instance.GetStartBoss());

        ///***********************ここからボス戦***********************/

        //  ポーズを有効化
        GameManager.Instance.GetPauseAction().Enable();

        //  ボスBGM再生
        SoundManager.Instance.PlayBGM((int)MusicList.BGM_TSUKUMO_STAGE_BOSS);

        //  イベントシーンマネージャーを無効化
        eventSceneManager.SetActive(false);

        //  ショットを有効化
        player.GetComponent<PlayerShotManager>().EnableShot();

        //  BombManagerを有効化する
        player.GetComponent<PlayerBombManager>().EnableBomb();

        //  ボスのHPスライダーを生成
        GameObject sliderObject = Instantiate(bossSlider);
        sliderObject.transform.SetParent(bossCanavs.transform, false);


        //  ボスオブジェクトを取得
        GameObject BossObj = EventSceneManager.Instance.GetBossObject();
        if(!BossObj) Debug.LogError("ボスオブジェクトがnullになっています！");


        //  ボスコンポーネントを取得
        BossTsukumo boss_tsukumo = BossObj.GetComponent<BossTsukumo>();
        if(!boss_tsukumo)Debug.LogError("ボスはBossTsukumoコンポーネントを持っていません！\n" +
            "PlayerInfoManager.stageInfoの値を確認してください");


        //  ボスごとにHPスライダーを設定
        boss_tsukumo.SetHpSlider(sliderObject.GetComponent<Slider>());
        bossNameHPSlider[(int)BossType.Tsukumo].SetActive(true);

        //  ボスコンポーネントを有効化
        boss_tsukumo.enabled = true;
        BossObj.GetComponent<BoxCollider2D>().enabled = true;

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

        //  BombManagerを無効化する
        player.GetComponent<PlayerBombManager>().DisableBomb();

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
