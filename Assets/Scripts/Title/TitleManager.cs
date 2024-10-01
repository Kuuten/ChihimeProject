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
    [SerializeField] private GameObject ConfigPanel;
    [SerializeField] private GameObject KeyConfigButton;

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
        player["ModeChange"].started += ToTitleUIMode;
    }

    private void OnDisable()
    {
        //  アクションマップを設定
        InputActionMap player = _input.actions.FindActionMap("Player");
        InputActionMap title_ui = _input.actions.FindActionMap("TITLE_UI");

        //  各マップにモードチェンジを設定
        player["ModeChange"].started -= ToTitleUIMode;
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

        //  コンフィグパネルをアクティブにする
        ConfigPanel.SetActive(true);

        //  キーコンフィグボタンを選択状態にする
        EventSystem.current.SetSelectedGameObject(KeyConfigButton.gameObject);


        //  実行モードをコンフィグモードにする
        titleMode = TitleMode.Config;
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
        //  背景以外をアクティブにする
        for(int i=0;i<DisableObject.Length;i++)
        {
            DisableObject[i].SetActive(true);
        }

        //  コンフィグパネルを非アクティブにする
        ConfigPanel.SetActive(false);

        //  スタートボタンを選択状態にする
        EventSystem.current.SetSelectedGameObject(Buttons.transform.GetChild(0).gameObject);

        //  実行モードを通常モードにする
        titleMode = TitleMode.Normal;
    }

     //  コンフィグ画面でもどるが押された時の処理
    public void OnPressedCancel()
    {
        //  背景以外をアクティブにする
        for(int i=0;i<DisableObject.Length;i++)
        {
            DisableObject[i].SetActive(true);
        }

        //  コンフィグパネルを非アクティブにする
        ConfigPanel.SetActive(false);

        //  スタートボタンを選択状態にする
        EventSystem.current.SetSelectedGameObject(Buttons.transform.GetChild(0).gameObject);

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

        //  入力がない場合は弾く
        if(!nevigate.WasPressedThisFrame())return;
        else
        {
            //  セレクトSE再生
            SoundManager.Instance.PlaySFX((int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_SELECT);
        }

        if (verticalInput < 0 && nevigate.WasPressedThisFrame())
        {
            if (Pos != menuNum)
            {
                Cursor.GetComponent<RectTransform>().position += 
                new UnityEngine.Vector3(0,-lineHeight,0);
                Pos += 1;
            }
            else
            {
                Cursor.GetComponent<RectTransform>().position += 
                new UnityEngine.Vector3(0,lineHeight*(menuNum-1),0);
                Pos = 1;
            }
            
        }
        else if (verticalInput > 0 && nevigate.WasPressedThisFrame())
        {
            if (Pos != 1)
            {
                Cursor.GetComponent<RectTransform>().position +=
                    new UnityEngine.Vector3(0, lineHeight, 0);
                Pos -= 1;
            }
            else
            {
                Cursor.GetComponent<RectTransform>().position +=
                new UnityEngine.Vector3(0, -lineHeight * (menuNum - 1), 0);
                Pos = menuNum;
            }
        } 
    }

    //  キーコンフィグボタンが押された時
    public void OnPressedKeyConfig()
    {
    
    }

    //  サウンドコンフィグボタンが押された時
    public void OnPressedSoundConfig()
    {
    
    }

}
