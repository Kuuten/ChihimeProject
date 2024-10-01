using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
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

//--------------------------------------------------------------
//
//  ゲーム管理クラス
//
//--------------------------------------------------------------
public class GameManager : MonoBehaviour
{
    [SerializeField]private GameObject player;
    [SerializeField]private GameObject enemyGenerator;

    private int gameState;

    [SerializeField] private GameObject soundManager;
    [SerializeField] private FadeIO Fade;
    [SerializeField] private ScrollAnimation Scroll;

    //  デバッグ用
    private InputAction test;
    private bool testSwitch;


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
        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        test = playerInput.actions["TestButton"];
        testSwitch = true;

        gameState = (int)eGameState.Zako;   //  最初はザコ戦
        stageClearFlag = false;
        sceneChangeFlag = false;

        //  初期化開始
        StartCoroutine(StartInit());
    }

    IEnumerator StartInit()
    {
        //  フェードイン
        yield return StartCoroutine(WaitingForFadeIn());

        yield return new WaitForSeconds(1); //  1秒待つ

        //  巻物アニメーション
        yield return StartCoroutine(WaitingForOpeningScroll());

        yield return new WaitForSeconds(2.0f); //  2秒待つ

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

        yield return null;
    }

    void Update()
    {
        if (test.WasPressedThisFrame())
        {
            testSwitch = !testSwitch;

            if (testSwitch == false)
            {
                //  Pauserが付いたオブジェクトをポーズ
                Pauser.Pause();

                //  止める
                Time.timeScale = 0;

                //  ポーズ後に入力がきかなくなるのでリセット
                player.GetComponent<PlayerInput>().enabled = true;
            }
            else
            {
                //  再開する
                Time.timeScale = 1;

                //  Pauserが付いたオブジェクトをポーズ
                Pauser.Resume();
            }
        }


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

        //  敵を一定間隔でポップさせる
        //StartCoroutine(PopEnemyInIntervals(_EnemyPopInterval));

        
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

        yield return new WaitForSeconds(2.0f); //  2.0秒待つ

        yield return StartCoroutine(DataCopyAndChangeScene());
    }

    //  タイトルのBGMを止める
    public void StopBGM()
    {
        SoundManager.Instance.Stop((int)AudioChannel.MUSIC);
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

        //  情報保存
        PlayerInfoManager.SetInfo(maxHP,hP,kon,bombNum,shotLV,speedLV);

        //  BGMを止める
        StopBGM();

        //  現在ステージを更新
        PlayerInfoManager.stageInfo = PlayerInfoManager.StageInfo.Stage02;

        //   ゲームクリアシーンへ
        LoadingScene.Instance.LoadNextScene("TrialEnding");

        ////   ショップシーンへ
        //LoadingScene.Instance.LoadNextScene("Shop"); 

        ////  体験版では特別クリア画面に遷移
        //if(Application.version == "0.5")
        //{
        //   //   ゲームクリアシーンへ
        //   LoadingScene.Instance.LoadNextScene("TrialEnding");
        //}
        ////  製品版ではショップシーンへ遷移
        //else
        //{
   
        //}

        yield return null;
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

        yield return null;
    }


    //-----------------------------------------------------------------
    //  情報保存＆ショップへ遷移(ボタンが押されたら呼ばれる)
    //-----------------------------------------------------------------
    public void AfterResult()
    {
        if(!sceneChangeFlag)
        {
            //  結果表示キャンバスを非表示にする
            resultObject[(int)eResultObj.RESULT_CANVAS].SetActive(false);

            //  紙吹雪を非表示にする
            resultObject[(int)eResultObj.KAMIFUBUKI].SetActive(false);

            //  決定SEを鳴らす
            SoundManager.Instance.PlaySFX(
                (int)AudioChannel.SFX,
                (int)SFXList.SFX_TITLE_SELECT);

            //  イベントキャンバスを非表示にする
            eventCanvas.SetActive(false);

            sceneChangeFlag = true;
            StartCoroutine(WaitingForClosingScroll());
        }
    }
}
