using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

//--------------------------------------------------------------
//
//  タイトル管理クラス
//
//--------------------------------------------------------------
public class TitleManager : MonoBehaviour
{
    [SerializeField] private FadeIO Fade;
    [SerializeField] private ScrollAnimation Scroll;

    [SerializeField] private LogoEasing logoEasing;
    [SerializeField] private LogoScaling logoScaling;
    [SerializeField] private GameObject Buttons;
    [SerializeField] private GameObject Cursor;
    [SerializeField] private GameObject[] DisableObject;

    private int Pos = 1;                    //  一番上
    private  int menuNum = 3;               //  メニューの数
    private float lineHeight = 111.7f;      //  1回で上下に動く幅

    PlayerInput _input;
    InputAction nevigate;
    float verticalInput;
    float horizontalInput;

    bool canContorol;   //  操作可能フラグ

    enum TitleMode
    {
        Normal,
        Config,

        Max
    }
    TitleMode titleMode;

    //[SerializeField] private EventSystem eventSystem;


    //--------------------------------------------------------------
    //  Config
    //--------------------------------------------------------------
    [SerializeField] private GameObject ConfigCanvas;

    //  コンフィグパネル
    [SerializeField] private GameObject ConfigPanel;

    //  コンフィグパネルの子オブジェクト一覧
    enum eConfigPanel
    {
        WaitingPanel,
        KeyConfigButton,
        SoundConfigButton,
        SaveButton,
        CancelButton,
    }

    //  キーコンフィグパネル
    [SerializeField] private GameObject KeyConfigPanel;
    //  サウンドコンフィグパネル
    [SerializeField] private GameObject SoundConfigPanel;

    //  キーボードの上下左右文字表示
    [SerializeField] private GameObject keyboardNavigateTextObj;
    //  ゲームパッドのボタンセット
    [SerializeField] private GameObject gamepadButtonSetObj;
    //  キーボードのボタンセット
    [SerializeField] private GameObject keyboardButtonSetObj;


    void Start()
    {
        //  最初は通常モード
        titleMode = TitleMode.Normal;

        canContorol = false;

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

        //  タイトルロゴイーズイン
        yield return StartCoroutine(WaitingForEasingTitlelogo());

       yield return new WaitForSeconds(1); //  1秒待つ

        //  タイトルロゴスケーリングON
        logoScaling.enabled = true;

        //  ボタンを表示
        Buttons.SetActive(true);

        //  操作可能にする
        canContorol = true;

        //  タイトルBGM再生
        SoundManager.Instance.PlayBGM((int)MusicList.BGM_TITLE);

        yield return null;
    }

    void Update()
    {
        //  操作不能ならリターン
        if(!canContorol)return;

        switch(titleMode)
        {
            case TitleMode.Normal:  //  通常モード
                MoveTitleMenuCursor();  //  カーソル移動
                break;

            case TitleMode.Config:  //  コンフィグモード
                MoveConfigMenuCursor();  //  カーソル移動
                break;
        }
    }

    //----------------------------------------------------------------
    //  アクションマップ切り替え用（現在切り替える予定なし）
    //----------------------------------------------------------------
    private void OnEnable()
    {
        // InputActionにNavigateを設定
        _input = GetComponent<PlayerInput>();
        nevigate = _input.actions["Navigate"];

        //  アクションマップを設定
        InputActionMap player = _input.actions.FindActionMap("Player");
        InputActionMap title_ui = _input.actions.FindActionMap("TITLE_UI");

        //  各マップにモードチェンジを設定
        player["Pause"].started += ToTitleUIMode;
    }

    private void OnDisable()
    {
        //  アクションマップを設定
        InputActionMap player = _input.actions.FindActionMap("Player");
        InputActionMap title_ui = _input.actions.FindActionMap("TITLE_UI");

        //  各マップにモードチェンジを設定
        player["Pause"].started -= ToTitleUIMode;
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

    //  タイトルのBGMを止める
    public void StopBGM()
    {
        SoundManager.Instance.Stop((int)AudioChannel.MUSIC);
    }

    // フェードインの完了を待つ
    IEnumerator WaitingForFadeIn()
    {
        yield return StartCoroutine(Fade.StartFadeIn());
    }

    // フェードアウトの完了を待つ
    IEnumerator WaitingForFadeOut()
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

        //  BGMを止めてメインへ
        StopBGM();
        LoadingScene.Instance.LoadNextScene("SelectConvert");
    }

    //  タイトルロゴのイーズインを待つ
    IEnumerator WaitingForEasingTitlelogo()
    {
        yield return StartCoroutine(logoEasing.EasingTitlelogo());
    }

    //  コンフィグが押された時の処理
    public void OnPressedConfig()
    {
        //  背景以外を非アクティブにする
        for(int i=0;i<DisableObject.Length;i++)
        {
            DisableObject[i].SetActive(false);
        }

        //  コンフィグキャンバスをアクティブにする
        ConfigCanvas.SetActive(true);

        //  実行モードをコンフィグモードにする
        titleMode = TitleMode.Config;

        //----------------------------------------------------------------
        //  接続されているデバイスによって表示を変える
        //----------------------------------------------------------------
        if(_input.currentControlScheme == "Gamepad")
        {
            keyboardNavigateTextObj.SetActive(false);
            gamepadButtonSetObj.SetActive(true);
            keyboardButtonSetObj.SetActive(false);

            //  最初のボタンを選択状態にする
            EventSystem.current.SetSelectedGameObject(
                gamepadButtonSetObj.transform.GetChild(0).gameObject);
            
        }
        else if(_input.currentControlScheme == "Keyboard")
        {
            keyboardNavigateTextObj.SetActive(true);
            gamepadButtonSetObj.SetActive(false);
            keyboardButtonSetObj.SetActive(true);

            //  最初のボタンを選択状態にする
            EventSystem.current.SetSelectedGameObject(
                keyboardButtonSetObj.transform.GetChild(0).gameObject);
        }
    }

     //  ゲーム開始が押された時の処理
    public void OnPressedStart()
    {
        //  決定音再生
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_DECISION);

        StartCoroutine( WaitingForClosingScroll() );
    }

     //  コンフィグ画面でセーブして戻るが押された時の処理
    public void OnPressedSave()
    {
        //  決定音再生
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_DECISION);

        //  背景以外をアクティブにする
        for(int i=0;i<DisableObject.Length;i++)
        {
            DisableObject[i].SetActive(true);
        }

        //  コンフィグキャンバスを非アクティブにする
        ConfigCanvas.SetActive(false);

        //  コンフィグボタンを選択状態にする
        EventSystem.current.SetSelectedGameObject(
            Buttons.transform.GetChild(1).gameObject);

        //  実行モードを通常モードにする
        titleMode = TitleMode.Normal;
    }

     //  コンフィグ画面でもどるが押された時の処理
    public void OnPressedCancel()
    {
        //  決定音再生
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_DECISION);

        //  背景以外をアクティブにする
        for(int i=0;i<DisableObject.Length;i++)
        {
            DisableObject[i].SetActive(true);
        }

        //  コンフィグキャンバスを非アクティブにする
        ConfigCanvas.SetActive(false);

        //  コンフィグボタンを選択状態にする
        EventSystem.current.SetSelectedGameObject(
            Buttons.transform.GetChild(1).gameObject);

        //  実行モードを通常モードにする
        titleMode = TitleMode.Normal;
    }

    //  終了が押された時の処理
    public void OnPressedExit()
    {
#if UNITY_EDITOR
        //ゲームプレイ終了
        UnityEditor.EditorApplication.isPlaying = false;
#else
        //ゲームプレイ終了
        Application.Quit();
#endif
    }

    //---------------------------------------------------
    //  タイトル画面でのカーソル移動
    //---------------------------------------------------
    private void MoveTitleMenuCursor()
    {
        UnityEngine.Vector2 inputNavigateAxis = nevigate.ReadValue<UnityEngine.Vector2>();
        verticalInput = inputNavigateAxis.y;
        horizontalInput = inputNavigateAxis.x;

        //  入力がない場合は弾く
        if(!nevigate.WasPressedThisFrame())return;
        else if(Mathf.Abs(verticalInput) > 0.0f)
        {
            //  セレクトSE再生
            SoundManager.Instance.PlaySFX((int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_SELECT);
        }

        if (verticalInput < 0)
        {
            if (Pos != menuNum)
            {
                Cursor.GetComponent<RectTransform>().anchoredPosition += 
                new UnityEngine.Vector2(0,-lineHeight);
                Pos += 1;
            }
            else
            {
                Cursor.GetComponent<RectTransform>().anchoredPosition += 
                new UnityEngine.Vector2(0,lineHeight*(menuNum-1));
                Pos = 1;
            }
            
        }
        else if (verticalInput > 0)
        {
            if (Pos != 1)
            {
                Cursor.GetComponent<RectTransform>().anchoredPosition +=
                    new UnityEngine.Vector2(0, lineHeight);
                Pos -= 1;
            }
            else
            {
                Cursor.GetComponent<RectTransform>().anchoredPosition +=
                new UnityEngine.Vector2(0, -lineHeight * (menuNum - 1));
                Pos = menuNum;
            }
        } 
    }

    //---------------------------------------------------
    //  コンフィグ画面でのカーソル移動
    //---------------------------------------------------
    private void MoveConfigMenuCursor()
    {
        UnityEngine.Vector2 inputNavigateAxis = nevigate.ReadValue<UnityEngine.Vector2>();
        verticalInput = inputNavigateAxis.y;
        horizontalInput = inputNavigateAxis.x;

        //  入力がない場合は弾く
        if(!nevigate.WasPressedThisFrame())return;
        else
        {
            //  セレクトSE再生
            SoundManager.Instance.PlaySFX((int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_SELECT);
        } 
    }

    //---------------------------------------------------
    //  キーコンフィグボタンが押された時の処理
    //---------------------------------------------------
    public void OnPressedKeyConfig()
    {
        //  キーコンフィグボタンを無効にする
        ConfigPanel.transform.GetChild((int)eConfigPanel.KeyConfigButton)
            .GetComponent<Button>().interactable = false;

        //  キーボードボタンを選択状態にする
        EventSystem.current.SetSelectedGameObject(
            KeyConfigPanel.transform.GetChild(0).gameObject);

    }

}
