using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    }

    void Start()
    {
        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        test = playerInput.actions["TestButton"];
        test2 = playerInput.actions["TestButton2"];

        gameState = (int)eGameState.Zako;   //  最初はザコ戦

        //  メインループ開始
        StartCoroutine(GameLoop());
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

        //************以降はプレイヤー死亡************

        //  プレイヤーのやられ演出を実行＆終了を待つ
        //  ↓これ参考に

        //loseSide.DeadStaging();    //  死亡演出再生
        ////  演出終了まで待つ
        //while (!loseSide.GetIsDeadStagingEnd())
        //{
        //    yield return null;
        //}

        //  ゲームオーバーシーンへ

        yield return null;
    }
}
