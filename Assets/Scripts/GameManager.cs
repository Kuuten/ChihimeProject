using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

//  ゲーム中の状態
public enum eGameState
{
    Zako,       //  ザコ戦中
    Boss,       //  ボス戦中
    Event,      //  会話イベント中

    StateMax
}

//  結果表示用オブジェクト
public enum eResultObj
{
    RESULT_CANVAS,
    RESULT_IMAGE,
    SOUL_IMAGE,
    SOUL_TEXT,
    HONEG_IMAGE,
    HONEG_PANEL,
    HONEG_TEXT,
    BUTTON,
    KAMIFUBUKI,

    Max
}

// ショップアイテムID
public enum eShopItemID
{
    RedHeart,           //  赤いハート
    DoubleupHeart,      //  ダブルアップハート
    GoldHeart,          //  金色のハート
    HoneGBomb,          //  骨Gボム
    Powerup,            //  ショット強化
    Speedup,            //  スピード強化
    Shield,             //  シールド

    Max
}

//--------------------------------------------------------------
//
//  ゲーム管理クラス
//
//--------------------------------------------------------------
public class GameManager : MonoBehaviour
{
    [SerializeField]private GameObject player;              //  プレイヤーオブジェクト
    [SerializeField]private GameObject enemyGenerator;      //  敵マネージャーオブジェクト

    private int gameState;  //  ゲーム全体の状態

    [SerializeField] private GameObject soundManager;       //  サウンドマネージャー
    [SerializeField] private FadeIO Fade;                   //  FadeIOクラス
    [SerializeField] private ScrollAnimation Scroll;        //  巻物

    //  デバッグ用
    private InputAction pauseButton;                        //  ポーズボタンアクション
    private bool pauseSwitch;                               //  ポーズフラグ


    //  シングルトンなインスタンス
    public static GameManager Instance
    {
        get; private set;
    }

    //  お金の価値
    private const int smallKonValue = 50;
    public int GetSmallKonValue(){ return smallKonValue; }
    private const int bigKonValue = 100;
    public int GetBigKonValue(){ return bigKonValue; }

    //  出現ポイント
    private GameObject[] spawner;
    //  制御点
    private GameObject[] controlPoint;

    //  ステージクリアフラグ
    bool stageClearFlag;

    //  シーン切り替えフラグ
    bool sceneChangeFlag;

    //----------------------------------------------------
    //  リザルト表示用
    //----------------------------------------------------
    //  リザルト表示用オブジェクト
    [SerializeField] private GameObject[] resultObject;
    //  イベントキャンバスオブジェクト
    [SerializeField] private GameObject eventCanvas;


    //  ポーズキャンバスオブジェクト
    [SerializeField] private GameObject pauseCanvas;
    //  PlayerInput
    PlayerInput _input;
    // 「ゲームにもどる」ボタン
    [SerializeField] private Button returnGameButton;
    //  ゲーム開始フラグ
    private bool startFlag;

    //----------------------------------------------------
    //  ショップ表示用
    //----------------------------------------------------
    //  アイテムボタンプレハブ
    [SerializeField] private GameObject[] itemButtonPrefab;
    //  アイテムリストオブジェクト（生成位置）
    [SerializeField] private GameObject itemListObject;
    //  ショップキャンバスオブジェクト
    [SerializeField] private GameObject shopCanvas;
    //  再生成ボタンオブジェクト
    [SerializeField] private GameObject regenerateButton;
    //  ShopManagerクラス
    [SerializeField] private ShopManager shopManager;

    //------------------------------------------------------------------------------
    //  プロパティ
    //------------------------------------------------------------------------------
    public int GetGameState()
    {
        return gameState;
    }
    public void SetGameState(int state)
    {
        gameState = state;
    }

    public GameObject GetPlayer(){ return player; }
    public bool GetStageClearFlag(){ return stageClearFlag; }
    public void SetStageClearFlag(bool b){ stageClearFlag = b; }
    public GameObject GetGameObject(int num){ return resultObject[num]; }
    public InputAction GetPauseAction(){ return pauseButton; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

#if UNITY_EDITOR
        //  SoundManagerがなければ生成
        if( !GameObject.Find("SoundManager") )
        {
            Instantiate(soundManager);
        }
#endif
    }

    //---------------------------------------------------------------------
    //  初期化
    //---------------------------------------------------------------------
    void Start()
    {
        //  ポーズキャンバスを無効化
        pauseCanvas.SetActive(false);

        // InputActionを初期化
        _input = player.GetComponent<PlayerInput>();
        pauseButton = _input.actions["Pause"];
        pauseSwitch = true;
        
        gameState = (int)eGameState.Zako;   //  最初はザコ戦
        stageClearFlag = false;
        sceneChangeFlag = false;
        startFlag = false;

        //  初期化開始
        StartCoroutine(StartInit());
    }

    IEnumerator StartInit()
    {
        //  ショットを無効化
        GetPlayer().GetComponent<PlayerShotManager>().DisableShot();

        //  ボムを無効化する
        GetPlayer().GetComponent<PlayerBombManager>().DisableBomb();

        //  フェードイン
        yield return StartCoroutine(WaitingForFadeIn());

        yield return new WaitForSeconds(1); //  1秒待つ

        //  巻物アニメーション
        yield return StartCoroutine(WaitingForOpeningScroll());

        yield return new WaitForSeconds(2.0f); //  2秒待つ

        //  巻物アニメが終わったので操作可能
        startFlag = true;

        //  ショットを有効化
        GetPlayer().GetComponent<PlayerShotManager>().EnableShot();

        //  ボムを有効化する
        GetPlayer().GetComponent<PlayerBombManager>().EnableBomb();

        //  プレイヤーにPauserを追加
        GetPlayer().AddComponent<Pauser>();

        //  メインループ開始
        yield return StartCoroutine(GameLoop());
    }

    //---------------------------------------------------------------------
    //  更新
    //---------------------------------------------------------------------
    private IEnumerator GameLoop()
    {
        //yield return StartCoroutine(Tutorial());
        yield return StartCoroutine(GameStarting());
        yield return StartCoroutine(GamePlaying());
        yield return StartCoroutine(GameResult());
        yield return StartCoroutine(ShopMode());

        yield return null;
    }

    void Update()
    {
        //  巻物アニメが終わってないならreturn
        if(!startFlag)return;

        if (pauseButton.WasPressedThisFrame())
        {
            pauseSwitch = !pauseSwitch;

            if (pauseSwitch == false)
            {
                //  ポーズキャンバスを有効化
                pauseCanvas.SetActive(true);

                //  アクションマップを設定
                InputActionMap mapPlayer = _input.actions.FindActionMap("Player");

                //  各マップにモードチェンジを設定
                mapPlayer["Pause"].started -= ToTitleUIMode;

                //  最初はドウジを選択状態にする
                EventSystem.current.SetSelectedGameObject(returnGameButton.gameObject);

                //  Pauserが付いたオブジェクトをポーズ
                Pauser.Pause();

                //  止める
                Time.timeScale = 0;

                //  ポーズ後に入力がきかなくなるのでリセット
                player.GetComponent<PlayerInput>().enabled = true;
            }
            else
            {
                //  ポーズキャンバスを無効化
                pauseCanvas.SetActive(false);

                //  アクションマップを設定
                InputActionMap mapTitle_ui = _input.actions.FindActionMap("TITLE_UI");

                //  各マップにモードチェンジを設定
                mapTitle_ui["Pause"].started -= ToPlayerMode;

                //  再開する
                Time.timeScale = 1;

                //  Pauserが付いたオブジェクトをポーズ
                Pauser.Resume();
            }
        }


    }

    //-----------------------------------------------------------------
    //  「ゲームにもどる」を押した時の処理
    //-----------------------------------------------------------------
    public void OnReturnGameButtonDown()
    {
        //  ポーズキャンバスを無効化
        pauseCanvas.SetActive(false);

        //  アクションマップを設定
        InputActionMap mapTitle_ui = _input.actions.FindActionMap("TITLE_UI");

        //  各マップにモードチェンジを設定
        mapTitle_ui["Pause"].started -= ToPlayerMode;

        //  再開する
        Time.timeScale = 1;

        //  Pauserが付いたオブジェクトをレジューム
        Pauser.Resume();
    }

    //-----------------------------------------------------------------
    //  「タイトルにもどる」を押した時の処理
    //-----------------------------------------------------------------
    public void OnReturnTitleButtonDown()
    {
        //  ポーズキャンバスを無効化
        pauseCanvas.SetActive(false);

        //  アクションマップを設定
        InputActionMap mapTitle_ui = _input.actions.FindActionMap("TITLE_UI");

        //  各マップにモードチェンジを設定
        mapTitle_ui["Pause"].started -= ToPlayerMode;

        //  再開する
        Time.timeScale = 1;

        //  Pauserが付いたオブジェクトをポーズ
        Pauser.Pause();

        //  タイトルシーンへ遷移
        StartCoroutine(WaitingForClosingScrollByButtonDown());

    }

    //-----------------------------------------------------------------
    //  ゲーム開始前
    //-----------------------------------------------------------------
    private IEnumerator GameStarting()
    {
        Debug.Log("GameStarting");

        //  このステージのIDを取得
        //int stageID = StageIdManager.Instance.GetStageID();

        //  ステージBGMを設定

        //  キャラの配置・パラメータをリセット
        //ResetAllCharacters();

        //  ゲーム開始前メッセージ（ステージ名がアルファのアニメーション）
        //float msg_alpha_speed = 1.0f; //  ステージ名のアルファが最大になるのにかかる時間
        //float msg_wait  = 3.0f;       //  メッセージの表示時間
        //yield return StartCoroutine(PreGameMessage
        //                            (
        //                                msg1speed, msg1wait
        //                            ));


        //  EnemyManagerのロード完了まで待つ
        yield return new WaitUntil(()=> EnemyManager.Instance.GetIsCompleteLoading() == true);
        
        //  敵の出現開始
        StartCoroutine( EnemyManager.Instance.AppearEnemy() );
        

        yield return null;
    }

    //-----------------------------------------------------------------
    //  ゲーム中
    //-----------------------------------------------------------------
    private IEnumerator GamePlaying()
    {
        Debug.Log("GamePlaying");

        //  Inputsystemをプレイヤーモードに
        //EnableCharacterControl();

        //  ステージクリアしてない間は待つ
        yield return new WaitUntil(()=> stageClearFlag == true);

        yield return null;
    }

    // フェードインの完了を待つ
    public IEnumerator WaitingForFadeIn()
    {
        yield return StartCoroutine(Fade.StartFadeIn());
    }

    // フェードアウトの完了を待つ
    public IEnumerator WaitingForFadeOut()
    {
        yield return StartCoroutine(Fade.StartFadeOut());
    }

    //  巻物を開くアニメーションの完了を待つ
    IEnumerator WaitingForOpeningScroll()
    {
        yield return StartCoroutine(Scroll.OpenScroll());
    }

    //  巻物を閉じるアニメーションの完了を待つ
    IEnumerator WaitingForClosingScroll()
    {
        yield return StartCoroutine(Scroll.CloseScroll());

        yield return new WaitForSeconds(1f); //  1秒待つ

        ////  体験版ではこっちを使う
        //yield return StartCoroutine(ResetAndChangeScene("TrialEnding"));

        //  製品版ではこっちを使う
        yield return StartCoroutine(DataCopyAndChangeScene());
    }

    //  巻物を閉じるアニメーションの完了を待つ
    IEnumerator WaitingForClosingScrollByButtonDown()
    {
        yield return StartCoroutine(Scroll.CloseScroll());

        yield return new WaitForSeconds(1f); //  1秒待つ

        yield return StartCoroutine(ResetAndChangeScene("Title"));
    }

    //  タイトルのBGMを止める
    public void StopBGM()
    {
        SoundManager.Instance.Stop((int)AudioChannel.MUSIC);
    }

    // 指定シーンへ情報をリセットして遷移する
    public IEnumerator ResetAndChangeScene(string scene_name)
    {
        //  BGMを止める
        StopBGM();

        //  情報をリセット
        PlayerInfoManager.ResetInfo();

        //   指定シーンへ遷移
        LoadingScene.Instance.LoadNextScene(scene_name);

        yield return null;
    }

    // 情報保存＆シーン遷移
    public IEnumerator DataCopyAndChangeScene()
    {
        //  各種情報を取得
        GameObject player = GameManager.Instance.GetPlayer();
        int maxHP = player.GetComponent<PlayerHealth>().GetCurrentMaxHealth();
        int hP = player.GetComponent<PlayerHealth>().GetCurrentHealth();
        int bombNum = player.GetComponent<PlayerBombManager>().GetBombNum();
        int kon = MoneyManager.Instance.GetKonNum();
        int shotLV = player.GetComponent<PlayerShotManager>().GetNormalShotLevel();;
        int speedLV = player.GetComponent<PlayerMovement>().GetSpeedLevel();
        bool isShield = player.GetComponent<PlayerHealth>().GetIsShielded();

        //  情報保存
        PlayerInfoManager.SetInfo(maxHP,hP,kon,bombNum,shotLV,speedLV,isShield);

        //  BGMを止める
        StopBGM();

        //  現在ステージを確認する
        CheckStageNo();

        yield return null;
    }

    //  現在ステージを更新/
    private void CheckStageNo()
    {
        //****************************************************************
        //  一旦ステージ３までの予定で後でステージ６まで作る
        //****************************************************************
        int currentStageNum = (int)PlayerInfoManager.stageInfo;
        if(currentStageNum >= (int)PlayerInfoManager.StageInfo.Stage03)
        {
            //  全ステージ終わっていたらエンディングへ
            ResetAndChangeScene("Ending");
        }
        else
        {
            //  インクリメントしてstageInfoにセット
            currentStageNum++;
            PlayerInfoManager.stageInfo = (PlayerInfoManager.StageInfo)currentStageNum;
            
            //  魂バートセレクト画面へ
            LoadingScene.Instance.LoadNextScene("SelectConvert");
        }
        //****************************************************************
        //
        //****************************************************************
    }

    //-----------------------------------------------------------------
    //  ゲーム結果表示
    //-----------------------------------------------------------------
    private IEnumerator GameResult()
    {
        Debug.Log("***結果表示待ち。***");

        //  結果表示開始フラグがTRUEにまるまで待つ
        yield return new WaitUntil(()=> EventSceneManager.Instance.GetStartResult() == true);

        Debug.Log("***結果表示モードになりました。***");

        //  結果表示Cnavasを表示
        resultObject[(int)eResultObj.RESULT_CANVAS].SetActive(true);

        //  シーン切り替えフラグがTRUEにまるまで待つ
        yield return new WaitUntil(()=> sceneChangeFlag == true);

        //  シーン切り替えフラグをリセット
        sceneChangeFlag = false;
    }


    //--------------------------------------------------------------------------
    //  情報保存＆ショップへ遷移(リザルトで「次へ」ボタンが押されたら呼ばれる)
    //--------------------------------------------------------------------------
    public void OnNextButtonDownAtResult()
    {
        if(!sceneChangeFlag)    //  連打対策
        {
            //  結果表示キャンバスを非表示にする
            resultObject[(int)eResultObj.RESULT_CANVAS].SetActive(false);

            //  BGMを止める
            SoundManager.Instance.Stop((int)AudioChannel.MUSIC);

            //  紙吹雪を非表示にする
            resultObject[(int)eResultObj.KAMIFUBUKI].SetActive(false);

            //  決定SEを鳴らす
            SoundManager.Instance.PlaySFX(
                (int)AudioChannel.SFX,
                (int)SFXList.SFX_TITLE_SELECT);

            ////------------------------------------------------------------------
            ////  体験版用処理
            ////------------------------------------------------------------------
            ////  シーン切り替えフラグをTRUE
            //sceneChangeFlag = true;
            ////  巻物アニメーション＆情報保存
            //StartCoroutine(WaitingForClosingScroll());
            //------------------------------------------------------------------

            //------------------------------------------------------------------
            //  製品版用処理
            //------------------------------------------------------------------
            if( PlayerInfoManager.stageInfo != PlayerInfoManager.StageInfo.Stage06 )
            {
                shopCanvas.SetActive(true); //  ショップキャンバスを表示
                InstantiateRandomItems();   //  ItemListにランダムにプレハブを生成
            }
            else
            {
                //  シーン切り替えフラグをTRUE
                sceneChangeFlag = true;
                //  巻物アニメーション＆情報保存
                StartCoroutine(WaitingForClosingScroll());
            }

        }
    }

    //-----------------------------------------------------------------
    //  ショップ画面を有効化
    //-----------------------------------------------------------------
    private IEnumerator ShopMode()
    {
        Debug.Log("***ショップ画面表示モードになりました。***");

        yield return null;
    }

    //--------------------------------------------------------------------------
    //  商品になるアイテムをランダムに３個生成
    //--------------------------------------------------------------------------
    private void InstantiateRandomItems()
    {
        //  始点と終点を設定する
        int start = (int)eShopItemID.RedHeart;
        int end = (int)eShopItemID.Max-1;

        //  ボタンを何個生成するか
        int chooseNum = 3;

        // 0～eShopItemID.Maxまでのリストを作る
        List<int> idList = new List<int>();
        for(int i=start;i<=end;i++)
        {
            idList.Add(i);
        }      

        //  その中からランダムに3個抽出する
        while(chooseNum-- > 0)
        {
            //  ランダムな数値を抽出(0~3)
            int index = UnityEngine.Random.Range(0, (int)idList.Count);
            int rand = idList[index];

            //  ItemListオブジェクトの子としてオブジェクトをを生成する
            GameObject obj = Instantiate(itemButtonPrefab[rand],itemListObject.transform);

            //  ボタンの種類によってイベントを登録する
            Button button = obj.GetComponent<Button>();
            if(rand == (int)eShopItemID.RedHeart)
            {
                //  オブジェクトのID2の子オブジェクトから値段を取得
                string s = obj.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text;
                shopManager.SetRedHeartValueText(s);

                shopManager.SetRedHeartButton(obj);
                button.onClick.AddListener( ()=>Debug.Log("赤いハートを買った！"));
                button.onClick.AddListener( ()=>shopManager.OnRedHeartButtonDown());
            }
            else if(rand == (int)eShopItemID.DoubleupHeart)
            {
                //  オブジェクトのID2の子オブジェクトから値段を取得
                string s = obj.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text;
                shopManager.SetDoubleupValueText(s);

                shopManager.SetDoubleupHeartButton(obj);
                button.onClick.AddListener( ()=>Debug.Log("ダブルアップハートを買った！"));
                button.onClick.AddListener( ()=>shopManager.OnDoubleupHeartButtonDown());
            }
            else if(rand == (int)eShopItemID.GoldHeart)
            {
                //  オブジェクトのID2の子オブジェクトから値段を取得
                string s = obj.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text;
                shopManager.SetGoldHeartValueText(s);

                shopManager.SetGoldHeartButton(obj);
                button.onClick.AddListener( ()=>Debug.Log("金色のハートを買った！"));
                button.onClick.AddListener( ()=>shopManager.OnGoldHeartButtonDown());
            }
            else if(rand == (int)eShopItemID.HoneGBomb)
            {
                //  オブジェクトのID2の子オブジェクトから値段を取得
                string s = obj.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text;
                shopManager.SetBombValueText(s);

                shopManager.SetBombButton(obj);
                button.onClick.AddListener( ()=>Debug.Log("骨Gボムを買った！"));
                button.onClick.AddListener( ()=>shopManager.OnHoneGBombButtonDown());
            }
            else if(rand == (int)eShopItemID.Powerup)
            {
                //  オブジェクトのID2の子オブジェクトから値段を取得
                string s = obj.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text;
                shopManager.SetPowerupValueText(s);

                shopManager.SetPowerupButton(obj);
                button.onClick.AddListener( ()=>Debug.Log("ショット強化を買った！"));
                button.onClick.AddListener( ()=>shopManager.OnPowerupButtonDown());
            }
            else if(rand == (int)eShopItemID.Speedup)
            {
                //  オブジェクトのID2の子オブジェクトから値段を取得
                string s = obj.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text;
                shopManager.SetSpeedupValueText(s);

                shopManager.SetSpeedupButton(obj);
                button.onClick.AddListener( ()=>Debug.Log("スピード強化を買った！"));
                button.onClick.AddListener( ()=>shopManager.OnSpeedupButtonDown());
            }
            else if(rand == (int)eShopItemID.Shield)
            {
                //  オブジェクトのID2の子オブジェクトから値段を取得
                string s = obj.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text;
                shopManager.SetShieldValueText(s);

                shopManager.SetShieldButton(obj);
                button.onClick.AddListener( ()=>Debug.Log("シールド追加を買った！"));
                button.onClick.AddListener( ()=>shopManager.OnShieldButtonDown());
            }



            //  その番号をidListから除外
            idList.RemoveAt(index);
        }

        //  PlayerInputのマップをUIモードに変更
        PlayerInput playerInput = GameManager.Instance.GetPlayer()
            .GetComponent<PlayerInput>();
        playerInput.enabled = true;
        playerInput.SwitchCurrentActionMap("Title_UI");

        //  itemListObjectの最初の子オブジェクトを選択状態にする
        EventSystem.current.SetSelectedGameObject(
            itemListObject.transform.GetChild(0).gameObject);

        //  生成完了したらリストを全削除
        idList.Clear();
    }

    //--------------------------------------------------------------------------
    //  アイテムボタンを全部削除して再生成(再入荷:3000魂)
    //--------------------------------------------------------------------------
    public void ReGenerate()
    {
        int value = 3000;   //  再入荷代金

        //  代金分魂を減らす
        if(MoneyManager.Instance.CanBuyItem(value))
        { 
            //  親オブジェクトのTransformを取得する
            Transform parentTransform = itemListObject.transform;

            //  全ての子オブジェクトを削除する
            foreach (Transform child in parentTransform)
            {
                Destroy(child.gameObject);
            }

            //  再生成
            InstantiateRandomItems();
        }
        //  購入不可能ならメッセージを出す
        else
        {
            //  購入失敗メッセージを出す
            StartCoroutine(shopManager.DisplayFailedMessage());
        }

        //  再生成ボタンオブジェクトを選択状態にする
        EventSystem.current.SetSelectedGameObject(
            regenerateButton.gameObject);
    }

    //--------------------------------------------------------------------------
    //  ショップへ遷移(ショップで「次へ」ボタンが押されたら呼ばれる)
    //--------------------------------------------------------------------------
    public void OnNextButtonDownAtShop()
    {
        if(!sceneChangeFlag)    //  連打対策
        {
            //  ショップキャンバスを非表示にする
            shopCanvas.SetActive(false);

            //  決定SEを鳴らす
            SoundManager.Instance.PlaySFX(
                (int)AudioChannel.SFX,
                (int)SFXList.SFX_TITLE_SELECT);

            //  イベントキャンバスを非表示にする
            eventCanvas.SetActive(false);

            //  シーン切り替えフラグをTRUE
            sceneChangeFlag = true;

            //  巻物アニメーション＆情報保存
            StartCoroutine(WaitingForClosingScroll());
        }
    }

    //  アクションマップをTitle_UIに切り替える
    private void ToTitleUIMode(InputAction.CallbackContext context)
    {
        _input.SwitchCurrentActionMap("Title_UI");
    }

    //  アクションマップをPlayerに切り替える
    private void ToPlayerMode(InputAction.CallbackContext context)
    {
        _input.SwitchCurrentActionMap("Player");
    }
}
