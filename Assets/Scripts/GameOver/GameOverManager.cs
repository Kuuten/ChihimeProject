using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

//--------------------------------------------------------------
//
//  ゲームオーバーシーンの管理クラス
//
//--------------------------------------------------------------
public class GameOverManager : MonoBehaviour
{
    //  PressAnyButton用
    [SerializeField] GameObject pressAnyButton;
    private bool bInputFlag = false;
    
    //  入力用
    PlayerInput input;
    InputAction anykey;

    //  その他
    [SerializeField] private FadeIO Fade;               //  FadeIO
    [SerializeField] private ScrollAnimation Scroll;    //  巻物
    [SerializeField] private GameObject soundManager;   //  SoundManager

    void Start()
    {
        // InputActionにAnyButtonを設定
        input = GetComponent<PlayerInput>();
        anykey = input.actions["AnyButton"];

        //  アクションマップを設定
        InputActionMap title_ui = input.actions.FindActionMap("TITLE_UI");

        //  StageIdManagerがない時は生成する
        if (!GameObject.Find("SoundManager"))
        {
            Debug.Log("SoundManagerがないので生成します");
            Instantiate(soundManager);
        }

        //  初期化開始
        StartCoroutine(StartInit());
    }

    IEnumerator StartInit()
    {
        /* ～～～～～～～～～～～演出の開始～～～～～～～～～～～ */

        //  フェードイン
        yield return StartCoroutine(WaitingForFadeIn());

        yield return new WaitForSeconds(1); //  1秒待つ

        //  巻物を開くアニメーション
        yield return StartCoroutine(WaitingForOpeningScroll());

        yield return new WaitForSeconds(2.0f); //  2秒待つ

        //  ゲームオーバーBGM再生
        SoundManager.Instance.PlayBGM((int)MusicList.BGM_GAMEOVER);



        yield return new WaitForSeconds(3); //  3秒待つ

        //  入力可能にする
        bInputFlag = true;

        //  PressAnyButtonを表示
        pressAnyButton.SetActive(true);
    }

    void Update()
    {
        if(bInputFlag)
        {
            //  どれかボタンを押したら
            if(anykey.triggered)
            {
                bInputFlag = false;
                
                //  巻物を閉じるアニメーション
                StartCoroutine(WaitingForClosingScroll());
            }
        }
    }

    //  ゲームオーバーのBGMを止める
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

        //  BGMを止めてタイトルへ
        StopBGM();
        LoadingScene.Instance.LoadNextScene("Title");
    }
}
