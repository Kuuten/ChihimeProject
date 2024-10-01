using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//--------------------------------------------------------------
//
//  魂バート弾選択シーンクラス
//
//--------------------------------------------------------------
public class SelectConvertManager : MonoBehaviour
{
    //  魂バート弾ボタン
    [SerializeField] private Button doujiButton;
    [SerializeField] private Button tsukumoButton;
    [SerializeField] private Button kuchinawaButton;
    [SerializeField] private Button kuramaButton;
    [SerializeField] private Button wadatsumiButton;
    [SerializeField] private Button hakumenButton;

    //  魂バート説明文
    [SerializeField] private GameObject doujiText;
    [SerializeField] private GameObject tsukumoText;
    [SerializeField] private GameObject kuchinawaText;
    [SerializeField] private GameObject kuramaText;
    [SerializeField] private GameObject wadatsumiText;
    [SerializeField] private GameObject hakumenText;

    [SerializeField] private FadeIO Fade;               //  FadeIO
    [SerializeField] private ScrollAnimation Scroll;    //  巻物
    [SerializeField] private GameObject soundManager;   //  SoundManager

    //  前回選択されていたオブジェクト
    private GameObject preSelectedObject;

    IEnumerator Start()
    {
        //  StageIdManagerがない時は生成する
        if (!GameObject.Find("SoundManager"))
        {
            Debug.Log("SoundManagerがないので生成します");
            Instantiate(soundManager);
        }

        /* ～～～～～～～～～～～演出の開始～～～～～～～～～～～ */

        //  フェードイン
        yield return StartCoroutine(WaitingForFadeIn());

        yield return new WaitForSeconds(1); //  1秒待つ

        //  巻物を開くアニメーション
        yield return StartCoroutine(WaitingForOpeningScroll());

        yield return new WaitForSeconds(2.0f); //  2秒待つ

        /* ～～～～～～～～～～～演出の終了～～～～～～～～～～～ */

        //  BGM再生
        //SoundManager.Instance.PlayBGM((int)MusicList.BGM_GAMEOVER);

        //  最初はドウジを選択状態にする
        EventSystem.current.SetSelectedGameObject(doujiButton.gameObject);
        preSelectedObject = doujiButton.gameObject;
    }

    void Update()
    {
        //  今回選択されているオブジェクトが前回と違ったらSE再生
        if(preSelectedObject != EventSystem.current.currentSelectedGameObject)
        {
            //  セレクトSE再生
            SoundManager.Instance.PlaySFX((int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_SELECT);
        }

        //  ボタンに合わせて説明文を表示
        if(EventSystem.current.currentSelectedGameObject == doujiButton.gameObject)
        {
            doujiText.SetActive(true);
            tsukumoText.SetActive(false);
            kuchinawaText.SetActive(false);
            kuramaText.SetActive(false);
            wadatsumiText.SetActive(false);
            hakumenText.SetActive(false);
        }
        else if(EventSystem.current.currentSelectedGameObject == tsukumoButton.gameObject)
        {
            doujiText.SetActive(false);
            tsukumoText.SetActive(true);
            kuchinawaText.SetActive(false);
            kuramaText.SetActive(false);
            wadatsumiText.SetActive(false);
            hakumenText.SetActive(false);
        }
        else if(EventSystem.current.currentSelectedGameObject == kuchinawaButton.gameObject)
        {
            doujiText.SetActive(false);
            tsukumoText.SetActive(false);
            kuchinawaText.SetActive(true);
            kuramaText.SetActive(false);
            wadatsumiText.SetActive(false);
            hakumenText.SetActive(false);
        }
        else if(EventSystem.current.currentSelectedGameObject == kuramaButton.gameObject)
        {
            doujiText.SetActive(false);
            tsukumoText.SetActive(false);
            kuchinawaText.SetActive(false);
            kuramaText.SetActive(true);
            wadatsumiText.SetActive(false);
            hakumenText.SetActive(false);
        }
        else if(EventSystem.current.currentSelectedGameObject == wadatsumiButton.gameObject)
        {
            doujiText.SetActive(false);
            tsukumoText.SetActive(false);
            kuchinawaText.SetActive(false);
            kuramaText.SetActive(false);
            wadatsumiText.SetActive(true);
            hakumenText.SetActive(false);
        }
        else if(EventSystem.current.currentSelectedGameObject == hakumenButton.gameObject)
        {
            doujiText.SetActive(false);
            tsukumoText.SetActive(false);
            kuchinawaText.SetActive(false);
            kuramaText.SetActive(false);
            wadatsumiText.SetActive(false);
            hakumenText.SetActive(true);
        }

        //  今回選択されたオブジェクトを保存
        preSelectedObject = EventSystem.current.currentSelectedGameObject;
    }

    //  ドウジボタンを押下した時
    public void OnDoujiButtonDown()
    {
        //  決定音再生
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_DECISION);

        //  巻物を閉じるアニメーションの完了を待つ
        //StartCoroutine(WaitingForClosingScroll());
    
        //  魂バート弾をセット
        PlayerInfoManager.g_CONVERTSHOT = SHOT_TYPE.DOUJI;

        //  BGMを止めてタイトルへ
        SoundManager.Instance.Stop((int)AudioChannel.MUSIC);
        LoadingScene.Instance.LoadNextScene("Main");
    }

    //  ツクモボタンを押下した時
    public void OnTsukumoButtonDown()
    {
        //  決定音再生
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_DECISION);

        //  巻物を閉じるアニメーションの完了を待つ
        StartCoroutine(WaitingForClosingScroll());
    
        //  魂バート弾をセット
        PlayerInfoManager.g_CONVERTSHOT = SHOT_TYPE.TSUKUMO;
    }


    //  クチナワボタンを押下した時
    public void OnKuchinawaButtonDown()
    {
        //  決定音再生
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_DECISION);

        //  巻物を閉じるアニメーションの完了を待つ
        StartCoroutine(WaitingForClosingScroll());
    
        //  魂バート弾をセット
        PlayerInfoManager.g_CONVERTSHOT = SHOT_TYPE.KUCHINAWA;
    }


    //  クラマボタンを押下した時
    public void OnKuramaButtonDown()
    {
        //  決定音再生
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_DECISION);

        //  巻物を閉じるアニメーションの完了を待つ
        StartCoroutine(WaitingForClosingScroll());
    
        //  魂バート弾をセット
        PlayerInfoManager.g_CONVERTSHOT = SHOT_TYPE.KURAMA;
    }


    //  ワダツミボタンを押下した時
    public void OnWadatsumiButtonDown()
    {
        //  決定音再生
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_DECISION);

        //  巻物を閉じるアニメーションの完了を待つ
        StartCoroutine(WaitingForClosingScroll());
    
        //  魂バート弾をセット
        PlayerInfoManager.g_CONVERTSHOT = SHOT_TYPE.WADATSUMI;
    }

    //  ハクメンボタンを押下した時
    public void OnHakumenButtonDown()
    {
        //  決定音再生
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_DECISION);

        //  巻物を閉じるアニメーションの完了を待つ
        StartCoroutine(WaitingForClosingScroll());
    
        //  魂バート弾をセット
        PlayerInfoManager.g_CONVERTSHOT = SHOT_TYPE.WADATSUMI;
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
        SoundManager.Instance.Stop((int)AudioChannel.MUSIC);
        LoadingScene.Instance.LoadNextScene("Main");
    }
}
