using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

//--------------------------------------------------------------
//
//  体験版終了画面管理クラス
//
//--------------------------------------------------------------
public class TrialEndingManager : MonoBehaviour
{
    PlayerInput _input;
    private InputAction submit;

    [SerializeField] private FadeIO Fade;
    [SerializeField] private ScrollAnimation Scroll;

    bool canCommand;  //  操作可能フラグ

    void Start()
    {
        _input = GetComponent<PlayerInput>();
        _input.SwitchCurrentActionMap("Title_UI");

        submit = _input.actions["Submit"];

        canCommand = false;

        //  演出開始
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

        //  操作可能！
        canCommand = true;
    }

    void Update()
    {
        //  決定ボタンを押したら
        if (submit.WasPressedThisFrame() && canCommand)
        {
            //  操作不能にリセット
            canCommand = false;

            //  現在ステージをリセット
            PlayerInfoManager.stageInfo = PlayerInfoManager.StageInfo.Stage01;

           //   タイトルシーンへ
           StartCoroutine(WaitingForClosingScroll());
        }
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

        //   タイトルシーンへ
        LoadingScene.Instance.LoadNextScene("Title"); 
    }
}
