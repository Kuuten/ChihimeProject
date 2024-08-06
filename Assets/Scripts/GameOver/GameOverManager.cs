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
    //  画像プレハブ配列
    [SerializeField] GameObject[] gameOverImage;

    //  PressAnyButton用
    [SerializeField] GameObject pressAnyButton;
    private bool bInputFlag = false;
    
    //  入力用
    PlayerInput input;
    InputAction anykey;

    //  その他
    [SerializeField] private SoundManager Sound;        //  SoundManager
    [SerializeField] private FadeIO Fade;               //  FadeIO
    [SerializeField] private ScrollAnimation Scroll;    //  巻物

    IEnumerator Start()
    {
        //  配列チェック
        if(gameOverImage.Length > (int)PlayerInfoManager.StageInfo.Max)
            Debug.LogError("画像プレハブの配列の数がオーバーしています！");

        //  現在のステージ番号にあった画像を生成
        int num = (int)PlayerInfoManager.stageInfo;
        Instantiate(gameOverImage[num]);

        // InputActionにAnyButtonを設定
        input = GetComponent<PlayerInput>();
        anykey = input.actions["AnyButton"];

        //  アクションマップを設定
        InputActionMap title_ui = input.actions.FindActionMap("TITLE_UI");

        /* ～～～～～～～～～～～演出の開始～～～～～～～～～～～ */

        //  フェードイン
        yield return StartCoroutine(WaitingForFadeIn());

        yield return new WaitForSeconds(1); //  1秒待つ

        //  巻物アニメーション
        yield return StartCoroutine(WaitingForOpeningScroll());

        //  ゲームオーバーBGM再生
        if(Sound == null)
        {
            Sound = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        }
        Sound.Play((int)AudioChannel.MUSIC, (int)MusicList.BGM_GAMEOVER);

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
                //  BGMを止めてタイトルへ
                StopBGM();
                LoadingScene.Instance.LoadNextScene("Title");
            }
        }
    }

    //  ゲームオーバーのBGMを止める
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
}
