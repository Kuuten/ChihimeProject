using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

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

    //  メニューボタンのインデックス
    private int menuIndex;

    //  説明動画の背景オブジェクト
    [SerializeField] private GameObject explanationMovieBack;

    //  説明テキスト
    [SerializeField] private GameObject[] explanationTextObj;

    [SerializeField] private FadeIO Fade;               //  FadeIO
    [SerializeField] private ScrollAnimation Scroll;    //  巻物
    [SerializeField] private GameObject soundManager;   //  SoundManager

    //  魂バート用の動画クリップ
    [SerializeField] private VideoClip[] videoClip;
    //  魂バート用の動画プレイヤー
    [SerializeField] private VideoPlayer videoPlayer;

    bool bCanDecision;  //  ボタンの決定可能フラグ

    PlayerInput _input;
    InputAction navigate;
    float verticalInput;

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

        _input = GetComponent<PlayerInput>();
        navigate =  _input.actions["Navigate"];
        verticalInput = default;
        menuIndex = (int)BossType.Douji;

        bCanDecision = true;

        /* ～～～～～～～～～～～演出の開始～～～～～～～～～～～ */

        //  フェードイン
        yield return StartCoroutine(WaitingForFadeIn());

        yield return new WaitForSeconds(1); //  1秒待つ

        //  巻物を開くアニメーション
        yield return StartCoroutine(WaitingForOpeningScroll());

        yield return new WaitForSeconds(2.0f); //  2秒待つ

        /* ～～～～～～～～～～～演出の終了～～～～～～～～フラグ～～～ */

        //  BGM再生
        //SoundManager.Instance.PlayBGM((int)MusicList.BGM_GAMEOVER);

        //  最初はドウジを選択状態にする
        EventSystem.current.SetSelectedGameObject(doujiButton.gameObject);
        preSelectedObject = doujiButton.gameObject;

        //  動画と動画の枠を表示する
        explanationMovieBack.SetActive(true);

    }

    void Update()
    {
        //  3Dメニューを回転させる
        RotateMenu();

        //  選択ボタンによって説明動画を再生する
        PlayExplanationMovie();

        //  今回選択されているオブジェクトが前回と違ったらSE再生
        if(preSelectedObject != EventSystem.current.currentSelectedGameObject)
        {
            //  セレクトSE再生
            SoundManager.Instance.PlaySFX((int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_SELECT);
        }

        //  今回選択されたオブジェクトを保存
        preSelectedObject = EventSystem.current.currentSelectedGameObject;
    }

    //  ドウジボタンを押下した時
    public void OnDoujiButtonDown()
    {
        //  決定可能フラグがfalseならリターン
        if(!bCanDecision)return;

        //  ボタンを無効化
        doujiButton.GetComponent<Button>().enabled = false;
        tsukumoButton.GetComponent<Button>().enabled = false;
        kuchinawaButton.GetComponent<Button>().enabled = false;
        kuramaButton.GetComponent<Button>().enabled = false;
        wadatsumiButton.GetComponent<Button>().enabled = false;
        hakumenButton.GetComponent<Button>().enabled = false;

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
        //  決定可能フラグがfalseならリターン
        if(!bCanDecision)return;

        //  ボタンを無効化
        doujiButton.GetComponent<Button>().enabled = false;
        tsukumoButton.GetComponent<Button>().enabled = false;
        kuchinawaButton.GetComponent<Button>().enabled = false;
        kuramaButton.GetComponent<Button>().enabled = false;
        wadatsumiButton.GetComponent<Button>().enabled = false;
        hakumenButton.GetComponent<Button>().enabled = false;

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
        //  決定可能フラグがfalseならリターン
        if(!bCanDecision)return;

        ////  ボタンを無効化
        //doujiButton.GetComponent<Button>().enabled = false;
        //tsukumoButton.GetComponent<Button>().enabled = false;
        //kuchinawaButton.GetComponent<Button>().enabled = false;
        //kuramaButton.GetComponent<Button>().enabled = false;
        //wadatsumiButton.GetComponent<Button>().enabled = false;
        //hakumenButton.GetComponent<Button>().enabled = false;

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
        //  決定可能フラグがfalseならリターン
        if(!bCanDecision)return;

        ////  ボタンを無効化
        //doujiButton.GetComponent<Button>().enabled = false;
        //tsukumoButton.GetComponent<Button>().enabled = false;
        //kuchinawaButton.GetComponent<Button>().enabled = false;
        //kuramaButton.GetComponent<Button>().enabled = false;
        //wadatsumiButton.GetComponent<Button>().enabled = false;
        //hakumenButton.GetComponent<Button>().enabled = false;

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
        //  決定可能フラグがfalseならリターン
        if(!bCanDecision)return;

        ////  ボタンを無効化
        //doujiButton.GetComponent<Button>().enabled = false;
        //tsukumoButton.GetComponent<Button>().enabled = false;
        //kuchinawaButton.GetComponent<Button>().enabled = false;
        //kuramaButton.GetComponent<Button>().enabled = false;
        //wadatsumiButton.GetComponent<Button>().enabled = false;
        //hakumenButton.GetComponent<Button>().enabled = false;

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
        //  決定可能フラグがfalseならリターン
        if(!bCanDecision)return;

        ////  ボタンを無効化
        //doujiButton.GetComponent<Button>().enabled = false;
        //tsukumoButton.GetComponent<Button>().enabled = false;
        //kuchinawaButton.GetComponent<Button>().enabled = false;
        //kuramaButton.GetComponent<Button>().enabled = false;
        //wadatsumiButton.GetComponent<Button>().enabled = false;
        //hakumenButton.GetComponent<Button>().enabled = false;

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

    //----------------------------------------------------
    //  3Dメニューを回転させる
    //----------------------------------------------------
    private void RotateMenu()
    {
        //  上下の入力を検出する
        Vector2 inputNavigateAxis = navigate.ReadValue<Vector2>();
        verticalInput = inputNavigateAxis.y;

        //  3Dメニューを回転させる
        if (verticalInput > 0 && Rotate3DMenu.Instance.GetComplete())
        {
            Debug.Log("下回転");
            Rotate3DMenu.Instance.TurnMenu(true);

            bCanDecision = false;

            //  インデックスプラス
            if(menuIndex >= (int)BossType.Hakumen)
            {
                menuIndex = (int)BossType.Douji;
            }
            else menuIndex++;

            //  ボタンの選択状態を更新
            SelectButtonByMenuIndex();

            //  ボタンのスケールを更新
            UpdateButtonScale(doujiButton.gameObject);
            UpdateButtonScale(tsukumoButton.gameObject);
            UpdateButtonScale(kuchinawaButton.gameObject);
            UpdateButtonScale(kuramaButton.gameObject);
            UpdateButtonScale(wadatsumiButton.gameObject);
            UpdateButtonScale(hakumenButton.gameObject);
        }
        else if (verticalInput < 0 && Rotate3DMenu.Instance.GetComplete())
        {
            Debug.Log("上回転");
            Rotate3DMenu.Instance.TurnMenu(false);

            bCanDecision = false;

            //  インデックスマイナス
            if(menuIndex <= (int)BossType.Douji)
            {
                menuIndex = (int)BossType.Hakumen;
            }
            else menuIndex--;

            //  ボタンの選択状態を更新
            SelectButtonByMenuIndex();

            //  ボタンのスケールを更新
            UpdateButtonScale(doujiButton.gameObject);
            UpdateButtonScale(tsukumoButton.gameObject);
            UpdateButtonScale(kuchinawaButton.gameObject);
            UpdateButtonScale(kuramaButton.gameObject);
            UpdateButtonScale(wadatsumiButton.gameObject);
            UpdateButtonScale(hakumenButton.gameObject);
        }
    }

    //----------------------------------------------------
    //  選択されたボタンを大きくする
    //----------------------------------------------------
    private void UpdateButtonScale(GameObject obj)
    {
        float duration = 0.7f;

        if(obj == EventSystem.current.currentSelectedGameObject)
        {
            obj.transform.DOScale(1.2f,duration)
                .SetEase(Ease.InOutElastic)
                .OnComplete(()=>{ bCanDecision = true; });
        }
        else
        {
            obj.transform.DOScale(1.0f,duration)
                .SetEase(Ease.InOutElastic);
        }
    }

    //----------------------------------------------------
    //  未実装ボタンが押された時の処理
    //----------------------------------------------------
    private void OnNotImplementedButtonDown()
    {
        //  不正解SEを再生してリターン
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX, (int)SFXList.SFX_TITLE_INCORRECT);

        return;
    }

    //----------------------------------------------------
    //  魂バート弾の説明動画を再生する
    //----------------------------------------------------
    private void PlayExplanationMovie()
    {
        if(!Rotate3DMenu.Instance.GetComplete() || !bCanDecision)return;

        //  選択ボタンによって説明動画を再生する
        if(doujiButton.gameObject == EventSystem.current.currentSelectedGameObject)
        {
            videoPlayer.clip = videoClip[(int)BossType.Douji];
            videoPlayer.Play();
        }
        else if(tsukumoButton.gameObject == EventSystem.current.currentSelectedGameObject)
        {
            videoPlayer.clip = videoClip[(int)BossType.Tsukumo];
            videoPlayer.Play();
        }
        else if(kuchinawaButton.gameObject == EventSystem.current.currentSelectedGameObject)
        {
            videoPlayer.clip = videoClip[(int)BossType.Kuchinawa];
            videoPlayer.Play();
        }
        else if(kuramaButton.gameObject == EventSystem.current.currentSelectedGameObject)
        {
            videoPlayer.clip = videoClip[(int)BossType.Kurama];
            videoPlayer.Play();
        }
        else if(wadatsumiButton.gameObject == EventSystem.current.currentSelectedGameObject)
        {
            videoPlayer.clip = videoClip[(int)BossType.Wadatsumi];
            videoPlayer.Play();
        }
        else if(hakumenButton.gameObject == EventSystem.current.currentSelectedGameObject)
        {
            videoPlayer.clip = videoClip[(int)BossType.Hakumen];
            videoPlayer.Play();
        }
    }

    //----------------------------------------------------
    //  インデックスでボタンを選択する
    //----------------------------------------------------
    private void SelectButtonByMenuIndex()
    {
        //  インデックスでボタンを選択状態にする
        if(menuIndex == (int)BossType.Douji)
        {
            EventSystem.current.SetSelectedGameObject(doujiButton.gameObject);
            explanationTextObj[(int)BossType.Douji].SetActive(true);
            explanationTextObj[(int)BossType.Tsukumo].SetActive(false);
            explanationTextObj[(int)BossType.Kuchinawa].SetActive(false);
            explanationTextObj[(int)BossType.Kurama].SetActive(false);
            explanationTextObj[(int)BossType.Wadatsumi].SetActive(false);
            explanationTextObj[(int)BossType.Hakumen].SetActive(false);
        }
        else if(menuIndex == (int)BossType.Tsukumo)
        {
            EventSystem.current.SetSelectedGameObject(tsukumoButton.gameObject);
            explanationTextObj[(int)BossType.Douji].SetActive(false);
            explanationTextObj[(int)BossType.Tsukumo].SetActive(true);
            explanationTextObj[(int)BossType.Kuchinawa].SetActive(false);
            explanationTextObj[(int)BossType.Kurama].SetActive(false);
            explanationTextObj[(int)BossType.Wadatsumi].SetActive(false);
            explanationTextObj[(int)BossType.Hakumen].SetActive(false);
        }
        else if(menuIndex == (int)BossType.Kuchinawa)
        {
            EventSystem.current.SetSelectedGameObject(kuchinawaButton.gameObject);
            explanationTextObj[(int)BossType.Douji].SetActive(false);
            explanationTextObj[(int)BossType.Tsukumo].SetActive(false);
            explanationTextObj[(int)BossType.Kuchinawa].SetActive(true);
            explanationTextObj[(int)BossType.Kurama].SetActive(false);
            explanationTextObj[(int)BossType.Wadatsumi].SetActive(false);
            explanationTextObj[(int)BossType.Hakumen].SetActive(false);
        }
        else if(menuIndex == (int)BossType.Kurama)
        {
            EventSystem.current.SetSelectedGameObject(kuramaButton.gameObject);
            explanationTextObj[(int)BossType.Douji].SetActive(false);
            explanationTextObj[(int)BossType.Tsukumo].SetActive(false);
            explanationTextObj[(int)BossType.Kuchinawa].SetActive(false);
            explanationTextObj[(int)BossType.Kurama].SetActive(true);
            explanationTextObj[(int)BossType.Wadatsumi].SetActive(false);
            explanationTextObj[(int)BossType.Hakumen].SetActive(false);
        }
        else if(menuIndex == (int)BossType.Wadatsumi)
        {
            EventSystem.current.SetSelectedGameObject(wadatsumiButton.gameObject);
            explanationTextObj[(int)BossType.Douji].SetActive(false);
            explanationTextObj[(int)BossType.Tsukumo].SetActive(false);
            explanationTextObj[(int)BossType.Kuchinawa].SetActive(false);
            explanationTextObj[(int)BossType.Kurama].SetActive(false);
            explanationTextObj[(int)BossType.Wadatsumi].SetActive(true);
            explanationTextObj[(int)BossType.Hakumen].SetActive(false);
        }
        else if(menuIndex == (int)BossType.Hakumen)
        {
            EventSystem.current.SetSelectedGameObject(hakumenButton.gameObject);
            explanationTextObj[(int)BossType.Douji].SetActive(false);
            explanationTextObj[(int)BossType.Tsukumo].SetActive(false);
            explanationTextObj[(int)BossType.Kuchinawa].SetActive(false);
            explanationTextObj[(int)BossType.Kurama].SetActive(false);
            explanationTextObj[(int)BossType.Wadatsumi].SetActive(false);
            explanationTextObj[(int)BossType.Hakumen].SetActive(true);
        }
    }
}
