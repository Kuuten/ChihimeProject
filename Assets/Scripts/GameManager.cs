using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
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

    //  デバッグ用
    private InputAction test;
    private InputAction test2;
    private bool testSwitch = true;
    IEnumerator shotCorourine;


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

    void Start()
    {
        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        test = playerInput.actions["TestButton"];
        test2 = playerInput.actions["TestButton2"];

        gameState = (int)eGameState.Zako;   //  最初はザコ戦

        //  初期化開始
        StartCoroutine(StartInit());
    }

    IEnumerator StartInit()
    {
        //  フェードイン
        yield return StartCoroutine(WaitingForFadeIn());

        //  メインループ開始
        yield return StartCoroutine(GameLoop());
    }

    private IEnumerator GameLoop()
    {
        //yield return StartCoroutine(Tutorial());
        yield return StartCoroutine(GameStarting());
        yield return StartCoroutine(GamePlaying());

        yield return null;
    }

    void Update()
    {
        if (test.WasPressedThisFrame())
        {
            testSwitch = false;

            if(testSwitch == false)
            {
                //  Pauserが付いたオブジェクトをポーズ
                Pauser.Pause();

                //  止める
                Time.timeScale = 0;

                //  ポーズ後に入力がきかなくなるのでリセット
                player.GetComponent<PlayerInput>().enabled = true;
            }
        }

        if (test2.WasPressedThisFrame())
        {
            testSwitch = true;

            if(testSwitch == true)
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

        //  ステージクリアフラグが立ったら情報保存してショップへ遷移
        bool clearFlag = false; //  仮処理

        GameObject player = GameManager.Instance.GetPlayer();
        int maxHP = player.GetComponent<PlayerHealth>().GetCurrentMaxHealth();
        int hP = player.GetComponent<PlayerHealth>().GetCurrentHealth();
        int bombNum = player.GetComponent<PlayerBombManager>().GetBombNum();
        int kon = MoneyManager.Instance.GetKonNum();
        int shotLV = player.GetComponent<PlayerShotManager>().GetNormalShotLevel();;
        int speedLV = player.GetComponent<PlayerMovement>().GetSpeedLevel();

        if(clearFlag)
        {
            //  情報保存
            PlayerInfoManager.SetInfo(maxHP,hP,kon,bombNum,shotLV,speedLV);

            // 巻物アニメーション

            //  フェードアウトを待つ
            yield return StartCoroutine(WaitingForFadeOut());

            //  ショップシーンへ遷移
        }

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
}
