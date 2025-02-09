using DG.Tweening;
using System;
using System.Collections;
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
    // EnentSystemオブジェクト
    [SerializeField] private GameObject eventSystemObj;

    [SerializeField] private FadeIO Fade;
    [SerializeField] private ScrollAnimation Scroll;

    [SerializeField] private LogoEasing logoEasing;
    [SerializeField] private LogoScaling logoScaling;

    [SerializeField] private VisualEasing visualEasing;
    [SerializeField] private VisualFadeIn visualFadeIn;

    [SerializeField] private GameObject Buttons;
    //[SerializeField] private GameObject Cursor;
    [SerializeField] private GameObject[] DisableObject;

    //  走るスプライトオブジェクト
    [SerializeField] private GameObject[] runObject;

    //  キーコンフィグの上書き分のセーブ・ロード用
    [SerializeField] private RebindSaveManager rebindSaveManager;

    enum RunObject
    {
        Chihime,
        Douji,
        Dodome,
        Tsukumo,
        Momo,
        Gorozaemon,
        Ossan
    }

    PlayerInput _input;
    InputAction nevigate;
    InputAction submit;
    float verticalInput;
    float horizontalInput;

    enum TitleMode
    {
        Normal,
        Config,

        Max
    }
    TitleMode titleMode;

    // デフォルト解像度を設定
    private readonly int defaultWidth = 1280;
    private readonly int defaultHeight = 720;

    // 現在のフルスクリーン状態を保持
    private bool isFullScreen = false;


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

    //  キーボードの上下左右文字表示
    [SerializeField] private GameObject keyboardNavigateTextObj;
    //  ゲームパッドのボタンセット
    [SerializeField] private GameObject gamepadButtonSetObj;
    //  キーボードのボタンセット
    [SerializeField] private GameObject keyboardButtonSetObj;
    //  最初に選択されるボタン
    [SerializeField] private GameObject firstSelectedButton;
    [SerializeField] private GameObject firstSelectedButtonGamePad;

    //  ウィンドウ関連
    [SerializeField] private Button windowSwitchButton;
    [SerializeField] private TextMeshProUGUI screenStateText;

    // 起動時に設定を適用
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeScreenSettings()
    {
        Screen.SetResolution(1280, 720, false);
    }

    void Start()
    {
        //  最初は操作無効
        nevigate.Disable();
        submit.Disable();

        //  最初は通常モード
        titleMode = TitleMode.Normal;

        //  キーコンフィグ設定のロード
        rebindSaveManager.Load();

        StartCoroutine(StartInit());
    }

    IEnumerator StartInit()
    {
        //  フェードイン
        yield return StartCoroutine(WaitingForFadeIn());

        yield return new WaitForSeconds(1); //  1秒待つ

        //  巻物アニメーション
        yield return StartCoroutine(WaitingForOpeningScroll());

        yield return new WaitForSeconds(1); //  1秒待つ

        //  タイトルロゴイーズイン
        yield return StartCoroutine(WaitingForEasingTitlelogo());

        yield return new WaitForSeconds(0.5f); //  0.5秒待つ

        // ヴィジュアルフェードイン
        yield return StartCoroutine(WaitingForFadeInTitlelVusual());

        yield return new WaitForSeconds(0.5f); //  0.5秒待つ

        //  操作を有効にする
        nevigate.Enable();
        submit.Enable();

        //  ボタンを表示
        Buttons.SetActive(true);

        //  タイトルBGM再生
        SoundManager.Instance.PlayBGM((int)MusicList.BGM_TITLE);

        yield return new WaitForSeconds(3); //  3秒待つ

        //  走る千姫くんとそれを追いかける六魔天
        float repeat_time = 10f; //  繰り返す間隔（秒）
        InvokeRepeating("RunToLeftAll",0f, repeat_time);

        yield return new WaitForSeconds(3.2f); //  3秒待つ

        //  タイトルロゴスケーリング開始
        logoScaling.enabled = true;
    }

    void Update()
    {
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

        // InputActionにSubmitを設定
        submit = _input.actions["Submit"];

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

        //  BGMを止めて魂バートセレクトへ
        StopBGM();
        LoadingScene.Instance.LoadNextScene("SelectConvert");
    }

    //  タイトルロゴのイーズインを待つ
    IEnumerator WaitingForEasingTitlelogo()
    {
        yield return StartCoroutine(logoEasing.EasingTitlelogo());
    }

    //  タイトルロゴのイーズインを待つ
    IEnumerator WaitingForEasingTitlelVusual()
    {
        yield return StartCoroutine(visualEasing.EasingTitleVisual());
    }

    //  タイトルロゴのフェードインを待つ
    IEnumerator WaitingForFadeInTitlelVusual()
    {
        yield return StartCoroutine(visualFadeIn.FadeInTitleVisual());
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
            EventSystem.current.SetSelectedGameObject(firstSelectedButtonGamePad.gameObject);
            
        }
        else if(_input.currentControlScheme == "Keyboard")
        {
            keyboardNavigateTextObj.SetActive(true);
            gamepadButtonSetObj.SetActive(false);
            keyboardButtonSetObj.SetActive(true);

            //  最初のボタンを選択状態にする
            EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);
        }
    }

     //  ゲーム開始が押された時の処理
    public void OnPressedStart()
    {
        //  連打を防ぐ
        Buttons.transform.GetChild(0).GetComponent<Button>().enabled = false;

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

     //  コンフィグ画面でキャンセルが押された時の処理
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
        Vector2 inputNavigateAxis = nevigate.ReadValue<Vector2>();
        verticalInput = inputNavigateAxis.y;
        horizontalInput = inputNavigateAxis.x;

        

        //  入力がない場合かEventSystemが非表示の場合は弾く
        if (!nevigate.WasPressedThisFrame() || !eventSystemObj.activeSelf) return;

        //  垂直入力のみがある場合
        if (Mathf.Abs(verticalInput) >= 1.0f && Mathf.Abs(horizontalInput) <= 0.0f)
        {
            //  セレクトSE再生
            SoundManager.Instance.PlaySFX((int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_SELECT);
        }
        else // それ以外は弾く
        {
            return;
        }

        //if (verticalInput < 0)
        //{
        //    if (Pos != menuNum)
        //    {
        //        Cursor.GetComponent<RectTransform>().anchoredPosition +=
        //        new UnityEngine.Vector2(0, -lineHeight);
        //        Pos += 1;
        //    }
        //    else
        //    {
        //        Cursor.GetComponent<RectTransform>().anchoredPosition +=
        //        new UnityEngine.Vector2(0, lineHeight * (menuNum - 1));
        //        Pos = 1;
        //    }

        //}
        //else if (verticalInput > 0)
        //{
        //    if (Pos != 1)
        //    {
        //        Cursor.GetComponent<RectTransform>().anchoredPosition +=
        //            new UnityEngine.Vector2(0, lineHeight);
        //        Pos -= 1;
        //    }
        //    else
        //    {
        //        Cursor.GetComponent<RectTransform>().anchoredPosition +=
        //        new UnityEngine.Vector2(0, -lineHeight * (menuNum - 1));
        //        Pos = menuNum;
        //    }
        //}
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
        //  どちらも入力がある場合も弾く
        else if(Mathf.Abs(verticalInput) >= 1.0f && Mathf.Abs(horizontalInput) >= 1.0f)
        {
            return;
        }
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

        //  ウィンドウサイズボタンを選択状態にする
        EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);

    }

    //---------------------------------------------------
    //  千姫くん達が画面左へ走る処理
    //---------------------------------------------------
    private void RunToLeft(GameObject prefab,float duration, float delay)
    {
        float targetX = -11f;   //  目標X座標

        //  オブジェクト生成
        GameObject obj = Instantiate(prefab);

        //  左へ移動
        obj.transform.DOMoveX(targetX, duration)
            .SetEase(Ease.Linear)
            .SetDelay(delay)
            .OnComplete( ()=>Destroy(obj) );
    }

    //---------------------------------------------------
    //  百々目が画面左へ走る処理
    //---------------------------------------------------
    private void RunDodomeToLeft(GameObject prefab,float duration, float delay)
    {
        float targetX = -11f;   //  目標X座標
        float targetY = -4.0f;  //  目標Y座標

        //  オブジェクト生成
        GameObject obj = Instantiate(prefab);

        //  左へ移動
        obj.transform.DOMoveX(targetX, duration)
            .SetEase(Ease.Linear)
            .SetDelay(delay)
            .OnComplete( ()=>Destroy(obj) );

        //  上下に揺れる
        obj.transform.DOMoveY(targetY, duration/4)
            .SetEase(Ease.Linear)
            .SetLoops(-1,LoopType.Yoyo)
            .SetDelay(delay);
    }

    private void RunToLeftAll()
    {
        //  目的地に着くまでにかかる時間（秒）
        float chihime_AnimeTime = 5.0f;
        float other_AnimeTime = 4.0f;
        float gorozaemon_AnimeTime = 6.0f;
        float ossan_AnimeTime = 2.0f;

        //  千姫くんに対しての遅延の基準時間(秒)
        float delay_BaseTime = 2.0f;
        float gorozaemon_delay_BaseTime = 2.5f; //  ごろざえもん体験版仕様
        //float gorozaemon_delay_BaseTime = 3.0f; //  ごろざえもん製品版仕様
        float ossan_delay_BaseTime_ = 4.0f;

        //  遅延のバイアス
        float delayBias = 0.1f;

        for(int i=0;i<runObject.Length;i++)
        {
            if(i == 0)
            {
                //  千姫くんを走らせる
                RunToLeft(runObject[i],chihime_AnimeTime, 0f);
            }
            else if( i == (int)RunObject.Dodome)
            {
                //  百々目を走らせる
                RunDodomeToLeft(
                    runObject[i],
                    other_AnimeTime,
                    delay_BaseTime + (delayBias * i));
            }
            else if( i == (int)RunObject.Gorozaemon)
            {
                //  ごろざえもんを走らせる
                RunToLeft(runObject[i],gorozaemon_AnimeTime, gorozaemon_delay_BaseTime);
            }
            else if( i == (int)RunObject.Ossan)
            {
                //  ちっちゃいおっさんを走らせる
                RunToLeft(runObject[i],ossan_AnimeTime, ossan_delay_BaseTime_);
            }
            //  ツクモと百は体験版では実装なし
            else if( i == (int)RunObject.Tsukumo || i == (int)RunObject.Momo )
            {
                continue;
            }
            else
            {
                //  それ以外を走らせる
                RunToLeft(
                    runObject[i],
                    other_AnimeTime,
                    delay_BaseTime + (delayBias * i));
            }
        }

        //RunToLeft(runObject[(int)RunObject.Douji],other_AnimeTime, delay_BaseTime);
        //RunToLeft(runObject[(int)RunObject.Tsukumo],other_AnimeTime, delay_BaseTime+0.1f);
        //RunToLeft(runObject[(int)RunObject.Kuchinawa],other_AnimeTime, delay_BaseTime+0.2f);
        //RunToLeft(runObject[(int)RunObject.Kurama],other_AnimeTime, delay_BaseTime+0.3f);
        //RunToLeft(runObject[(int)RunObject.Wadatsumi],other_AnimeTime, delay_BaseTime+0.4f);
        //RunToLeft(runObject[(int)RunObject.Hakumen],other_AnimeTime, delay_BaseTime+0.5f);
    }

    //---------------------------------------------------
    //  画面の状態の表示を切り替える
    //---------------------------------------------------
    private void SetScreenStateText(bool isFullScreen)
    {
        //  ローカル変数を用意
        string[] screenModeText = { "ウィンドウ", "フルスクリーン" };

        // フルスクリーンモードを切り替える
        ToggleFullScreen();

        int index = Convert.ToInt32(!Screen.fullScreen);

        //  スクリーン状態のテキストを更新
        screenStateText.text = $"現在の画面モード:{screenModeText[index]}";
    }

    public void SetScreenStateText()
    {
        //  状態を反転
        Screen.fullScreen = !Screen.fullScreen;

        //  ローカル変数を用意
        string[] screenModeText = { "ウィンドウ", "フルスクリーン" };
        int index = Convert.ToInt32(!Screen.fullScreen);

        //  スクリーン状態のテキストを更新
        screenStateText.text = $"現在の画面モード:{screenModeText[index]}";
    }

    // フルスクリーンモードを切り替えるメソッド
    private void ToggleFullScreen()
    {
        isFullScreen = !isFullScreen;

        if (isFullScreen)
        {
            // フルスクリーンモードに切り替え
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
        }
        else
        {
            // ウィンドウモードに切り替え
            Screen.SetResolution(defaultWidth, defaultHeight, false);
        }
    }

}
