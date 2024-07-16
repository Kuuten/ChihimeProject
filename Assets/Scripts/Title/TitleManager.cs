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
using UnityEngine.UIElements;

//--------------------------------------------------------------
//
//  タイトル管理クラス
//
//--------------------------------------------------------------
public class TitleManager : MonoBehaviour
{
    [SerializeField] private SoundManager Sound;
    [SerializeField] private FadeIO Fade;
    [SerializeField] private ScrollAnimation Scroll;
    [SerializeField] private LogoEasing logoEasing;
    [SerializeField] private LogoScaling logoScaling;
    [SerializeField] private GameObject Buttons;
    [SerializeField] private GameObject Cursor;
    [SerializeField] private GameObject ConfigPanel;
    [SerializeField] private GameObject[] DisableObject;

    private int Pos = 1;                    //  一番上
    private  int menuNum = 3;               //  メニューの数
    private float lineHeight = 111.7f;      //  1回で上下に動く幅

    PlayerInput _input;
    InputAction nevigate;
    float verticalInput;

    IEnumerator Start()
    {
        //  フェードイン
        yield return StartCoroutine(WaitingForFadeIn());

        yield return new WaitForSeconds(1); //  1秒待つ

        //  巻物アニメーション
        yield return StartCoroutine(WaitingForOpeningScroll());

        yield return new WaitForSeconds(1); //  1秒待つ

        //  タイトルロゴイーズイン
        yield return StartCoroutine(WaitingForEasingTitlelogo());

       yield return new WaitForSeconds(1); //  1秒待つ

        //  タイトルロゴスケーリングON
        logoScaling.enabled = true;

        //  ボタンを表示
        Buttons.SetActive(true);

        //  タイトルBGM再生
        Sound.Play((int)AudioChannel.MUSIC, (int)MusicList.BGM_TITLE);

        yield return null;
    }

    void Update()
    {
        UnityEngine.Vector2 inputNavigateAxis = nevigate.ReadValue<UnityEngine.Vector2>();
        verticalInput = inputNavigateAxis.y;

        //  入力がない場合は弾く
        if(!nevigate.WasPressedThisFrame())return;
        else
        {
            //  セレクトBGM再生
            Sound.Play((int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_SELECT);
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

    //----------------------------------------------------------------
    //  アクションマップ切り替え用（現在切り替える予定なし）
    //----------------------------------------------------------------
    private void OnEnable()
    {
        // InputActionにMoveを設定
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
        Sound.Stop((int)AudioChannel.MUSIC);
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

    //  巻物アニメーションの完了を待つ
    IEnumerator WaitingForOpeningScroll()
    {
        yield return StartCoroutine(Scroll.OpenScroll());
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
    }

     //  コンフィグ画面でもどるが押された時の処理
    public void OnPressedBack()
    {
        //  背景以外をアクティブにする
        for(int i=0;i<DisableObject.Length;i++)
        {
            DisableObject[i].SetActive(true);
        }

        //  コンフィグパネルを非アクティブにする
        ConfigPanel.SetActive(false);
    }
}
