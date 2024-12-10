using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

//--------------------------------------------------------------
//
//  紙芝居シーン管理クラス
//
//--------------------------------------------------------------
public class IntroManager : MonoBehaviour
{
    [SerializeField] private GameObject UICanvas;       //  UIキャンバス(Fade/巻物)
    [SerializeField] private FadeIO Fade;               //  FadeIO
    [SerializeField] private ScrollAnimation Scroll;    //  巻物

    bool canContorol;   //  操作可能フラグ

    [SerializeField] private TextMeshProUGUI sceneText; //  Canvasのテキストオブジェクト
    [SerializeField] private Image sceneImage;          //  Canvasの画像オブジェクト
    [SerializeField] private GameObject skipPanel;      //  SkipPanelオブジェクト
    [SerializeField] private GameObject noButton;       //  「いいえ」ボタンオブジェクト

    //  紙芝居のID
    private enum SceneType
    {
        Scene01,
        Scene02,
        Scene03,    //  このimageはダミーで表示されない
        Scene04,
        Scene05,

        Max
    };

    //  イラスト
    [SerializeField] private Sprite[] image;

    //  テキスト
    private string[] text =
    {
        "今は昔の物語。\r\n具体的には20年ほど前。\r\n人と妖怪の間に大きな戦があったそうな･･････。",
        "妖怪王率いる妖怪の軍勢に立ち向かったのは\r\n\r\nたった一人の巫女だった\r\n\r\n地は裂け山は割れ\r\n\r\n",
        "ることもなく\r\n\r\n彼らはひっそりと戦ったそうな",
        "そんな人妖の大戦（おおいくさ）も\r\n\r\n巫女と妖怪の王が結ばれることで\r\n\r\nあっけなく幕を下ろした",
        "そして時は流れ\r\n\r\n巫女と妖怪王の間に産まれた半人半妖の子が\r\n\r\n１４歳になったある日の物語である"
    };

    //  フェード時間
    private static readonly float fade_duraiton = 3f;

    //  画面の更新間隔
    private static readonly float update_interval = 7f;

    //  入力関係
    PlayerInput _input;
    InputAction pause;

    void Start()
    {
        StartCoroutine(StartInit());
    }

    // フェードインの完了を待つ
    IEnumerator WaitingForFadeIn()
    {
        yield return StartCoroutine(Fade.StartFadeIn());

        yield return new WaitForSeconds(1f); //  1秒待つ
    }

    //  白フェードイン版
    IEnumerator WaitingForWhiteFadeIn()
    {
        yield return StartCoroutine(Fade.StartFadeIn(new Color(1f,1f,1f)));

        yield return new WaitForSeconds(1f); //  1秒待つ
    }

    // フェードアウトの完了を待つ
    IEnumerator WaitingForFadeOut()
    {
        yield return StartCoroutine(Fade.StartFadeOut());

        yield return new WaitForSeconds(1f); //  1秒待つ
    }

    //  白フェードアウト版
    IEnumerator WaitingForWhiteFadeOut()
    {
        yield return StartCoroutine(Fade.StartFadeOut(new Color(1f,1f,1f)));
    }

    //  巻物を開くアニメーションの完了を待つ
    IEnumerator WaitingForOpeningScroll()
    {
        yield return StartCoroutine(Scroll.OpenScroll());
    }

    //  タイトルのBGMを止める
    public void StopBGM()
    {
        SoundManager.Instance.Stop((int)AudioChannel.MUSIC);
    }

    //  巻物を閉じるアニメーションの完了を待つ
    IEnumerator WaitingForClosingScroll()
    {
        yield return StartCoroutine(Scroll.CloseScroll());

        yield return new WaitForSeconds(1f); //  1秒待つ
    }

    //--------------------------------------------------------------
    //  初期化コルーチン
    //--------------------------------------------------------------
    IEnumerator StartInit()
    {
        // InputActionにNavigateを設定
        _input = GetComponent<PlayerInput>();
        pause = _input.actions["Pause"];

        //  最初のページにする
        sceneImage.sprite = image[(int)SceneType.Scene01];
        sceneText.text = text[(int)SceneType.Scene01];

        //  スキップパネルを非表示にする
        skipPanel.SetActive(false);

        //  必ず表示にしておく
        UICanvas.SetActive(true);

        //  画像のアルファを0にする
        sceneImage.DOFade(0f, 0f);
        
        //  テキストのアルファを0にする
        sceneText.DOFade(0f, 0f);

        //  フェードイン
        yield return StartCoroutine(WaitingForFadeIn());

        //  巻物アニメーション
        yield return StartCoroutine(WaitingForOpeningScroll());

        yield return new WaitForSeconds(2f); //  2秒待つ

        //  IntroBGM再生
        //SoundManager.Instance.PlayBGM((int)MusicList.BGM_SHOP);

        //  操作可能にする
        canContorol = true;

        /**************************初期化終了****************************/

        //-----------------------------
        //  紙芝居開始
        //-----------------------------
        StartCoroutine(StartScene());
    }

    void Update()
    {
        //  操作不能ならリターン
        if(!canContorol)return;

        //  決定ボタンでパネルを表示
        if(pause.WasPressedThisFrame())
        {
            //  スキップパネルを表示にする
            skipPanel.SetActive(true);

            //  「いいえ」ボタンを選択状態にする
            EventSystem.current.SetSelectedGameObject(noButton.gameObject);

            //  停止
            Time.timeScale = 0f;
        }
    }

    //--------------------------------------------------------------
    //  シーンの再生
    //--------------------------------------------------------------
    IEnumerator StartScene()
    {
        StartCoroutine(StartFadeImage(SceneType.Scene01 ,fade_duraiton));
        StartCoroutine(StartFadeText(SceneType.Scene01 ,fade_duraiton));

        yield return new WaitForSeconds(update_interval);

        StartCoroutine(StartFadeImage(SceneType.Scene02 ,fade_duraiton));
        StartCoroutine(StartFadeText(SceneType.Scene02 ,fade_duraiton));

        yield return new WaitForSeconds(update_interval);

        //  テキストだけ変更
        StartCoroutine(StartFadeText(SceneType.Scene03 ,fade_duraiton));

        yield return new WaitForSeconds(update_interval);

        StartCoroutine(StartFadeImage(SceneType.Scene03 ,fade_duraiton));
        StartCoroutine(StartFadeText(SceneType.Scene04 ,fade_duraiton));

        yield return new WaitForSeconds(update_interval);

        StartCoroutine(StartFadeImage(SceneType.Scene04 ,fade_duraiton));
        StartCoroutine(StartFadeText(SceneType.Scene05 ,fade_duraiton));

        yield return new WaitForSeconds(update_interval);

        //-------------------------再生終了-------------------------

        //  シーン終了処理
        yield return StartCoroutine(EndIntroScene());
    }

    //--------------------------------------------------------------
    //  指定シーンの絵をフェードインさせる
    //--------------------------------------------------------------
    IEnumerator StartFadeImage(SceneType sceneType, float duration)
    {
        //  まず指定シーンの画像に差し替える
        sceneImage.sprite = image[(int)sceneType];

        //  画像のアルファを0にする
        sceneImage.DOFade(0f, 0f);

        yield return null;

        //  画像をフェードインさせる
        sceneImage.DOFade(1f,duration);

        yield return new WaitForSeconds(duration);
    }

    //--------------------------------------------------------------
    //  指定シーンのテキストをフェードインさせる
    //--------------------------------------------------------------
    IEnumerator StartFadeText(SceneType sceneType, float duration)
    {
        //  まず指定シーンのテキストに差し替える
        sceneText.text = text[(int)sceneType];

        //  テキストのアルファを0にする
        sceneText.DOFade(0f, 0f);

        yield return null;

        //  画像をフェードインさせる
        sceneText.DOFade(1f,duration);

        yield return new WaitForSeconds(duration);
    }

    //--------------------------------------------------------------
    //  シーンの終了処理
    //--------------------------------------------------------------
    IEnumerator EndIntroScene()
    {
        //  巻物アニメーション
        yield return StartCoroutine(WaitingForClosingScroll());

        //  フェードアウト
        //yield return StartCoroutine(WaitingForFadeOut());

        //  BGMを止めてメインへ
        StopBGM();
        LoadingScene.Instance.LoadNextScene("Stage01");
    }

    //--------------------------------------------------------------
    //  スキップメニューで「はい」を押した時の処理
    //--------------------------------------------------------------
    public void OnSkipButtonDown()
    {
        //  元に戻す
        Time.timeScale = 1f;

        //  シーン終了処理
        StartCoroutine(EndIntroScene());
    }

    //--------------------------------------------------------------
    //  スキップメニューで「いいえ」を押した時の処理
    //--------------------------------------------------------------
    public void OnContinueButtonDown()
    {
        //  元に戻す
        Time.timeScale = 1f;

        //  スキップパネルを非表示にする
        skipPanel.SetActive(false);
    }
}
