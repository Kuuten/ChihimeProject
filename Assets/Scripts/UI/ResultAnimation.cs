using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms.Impl;


//--------------------------------------------------------------
//
//  ステージクリア表示クラス
//
//--------------------------------------------------------------
public class ResultAnimation : MonoBehaviour
{
    //  「結魂回避成功！」の画像オブジェクト
    private GameObject resultImage;
    //  獲得ソウルの画像オブジェクト
    private GameObject soulImage;
    //  獲得ソウルのテキストオブジェクト
    private GameObject soulText;
    //  骨Gの画像オブジェクト
    private GameObject honeGImage;
    //  骨Gのテキストパネルオブジェクト
    private GameObject honeGTextPanel;
    //  骨Gのテキストオブジェクト
    private GameObject honeGText;
    //  ボタンオブジェクト
    private GameObject nextButton;
    //  紙吹雪オブジェクト
    private GameObject kamiFubuki;

    //  カウント用ソウルの数
    int soulNum;

    void Start()
    {
        //  ソウルの数初期化
        soulNum = 0;

        // GameManagerからオブジェクトを割り当てる
        resultImage = GameManager.Instance.GetGameObject((int)eResultObj.RESULT_IMAGE);
        soulImage = GameManager.Instance.GetGameObject((int)eResultObj.SOUL_IMAGE);
        soulText = GameManager.Instance.GetGameObject((int)eResultObj.SOUL_TEXT);
        honeGImage = GameManager.Instance.GetGameObject((int)eResultObj.HONEG_IMAGE);
        honeGTextPanel = GameManager.Instance.GetGameObject((int)eResultObj.HONEG_PANEL);
        honeGText = GameManager.Instance.GetGameObject((int)eResultObj.HONEG_TEXT);
        nextButton = GameManager.Instance.GetGameObject((int)eResultObj.BUTTON);
        kamiFubuki = GameManager.Instance.GetGameObject((int)eResultObj.KAMIFUBUKI);

        //  結果表示開始
        StartCoroutine( ResultStart() );
    }

    void Update()
    {
        // テキストを更新
        soulText.GetComponent<TextMeshProUGUI>().text = $"{soulNum}";
    }

    //-----------------------------------------------------------------
    //  ゲーム結果表示アニメーション
    //-----------------------------------------------------------------
    private IEnumerator ResultStart()
    {
        Debug.Log("結果表示を開始します");

        //  BGMを再生
        SoundManager.Instance.PlayBGM((int)MusicList.BGM_RESULT);

        //  キャンバスを有効化
        this.gameObject.SetActive(true);

        //  「結魂回避成功！を有効化
        resultImage.SetActive(true);

        //  「結魂回避成功！のアニメーション開始
        resultImage.transform.localScale = new Vector3(1,0,1);
        resultImage.GetComponent<RectTransform>().DOScaleY(1.0f,0.5f);

        //  SEを鳴らす
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX_SYSTEM,
            (int)SFXList.SFX_NORMAL_SHOT);
        
        //  アニメーションを待つ
        yield return new WaitForSeconds(0.5f);

        //  獲得ソウルを有効化
        soulImage.SetActive(true);
        soulImage.transform.localScale = new Vector3(1,0,1);
        soulImage.GetComponent<RectTransform>().DOScaleY(1.0f,0.5f);

        //  SEを鳴らす
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX_SYSTEM,
            (int)SFXList.SFX_NORMAL_SHOT);

        //  アニメーションを待つ
        yield return new WaitForSeconds(0.5f);

        //  獲得ソウルテキスト有効化
        soulText.SetActive(true);

        //  待つ
        yield return new WaitForSeconds(0.5f);

        //  獲得ソウルの数字をカウントアニメーション開始
        int targetNum = MoneyManager.Instance.GetKonNum();
        int count = 0;

        // 数値の変更
        DOTween.To(
            () => soulNum,                  // 何を対象にするのか
            num => soulNum = num,           // 値の更新
            targetNum,                      // 最終的な値
            2.0f                            // アニメーション時間
        )
        .OnUpdate( () => 
        { 
            count++;
            if(count%5 != 0)return;
            //  カウントSEを鳴らす
            SoundManager.Instance.PlaySFX(
                (int)AudioChannel.SFX_SYSTEM,
                (int)SFXList.SFX_RESULT_COUNT);
        })
        .OnComplete( () =>
        {
            //  完了SEを鳴らす
            SoundManager.Instance.PlaySFX(
                (int)AudioChannel.SFX_SYSTEM,
                (int)SFXList.SFX_RESULT_CASH);
        });

        //  アニメーションを待つ
        yield return new WaitForSeconds(3);

        //  ソウルテキストのスケーリング開始
        soulText.GetComponent<RepeatScaling>().enabled = true;

        //  紙吹雪を舞い散らせる
        kamiFubuki.SetActive(true);

        //  骨G画像を有効化
        honeGImage.SetActive(true);

        yield return null;

        //  骨Gアニメーション開始
        honeGImage.GetComponent<RectTransform>().anchoredPosition = new Vector2(-487,660);

        

        honeGImage.GetComponent<RectTransform>().DOAnchorPosY(37,2.0f)
            .SetEase(Ease.OutBounce);

        //  アニメーションを待つ
        yield return new WaitForSeconds(0.45f);

        //  SEを鳴らす※要タイミング調整
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX_SYSTEM,
            (int)SFXList.SFX_RESULT_BOUND);

        //  アニメーションを待つ
        yield return new WaitForSeconds(0.5f);

        //  SEを鳴らす※要タイミング調整
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX_SYSTEM,
            (int)SFXList.SFX_RESULT_BOUND);

        //  パネルを表示
        honeGTextPanel.SetActive(true);
        
        //  テキストを表示
        honeGText.SetActive(true);

        //  骨Gにのメッセージを設定
        string[] message =
        {
            "さすが若様！\n若様の勇姿に爺は感激ですぞ～！！",
            "さすが若様！\n若様の勇姿に爺は感激ですぞ～！！",
            "さすが若様！\n若様の勇姿に爺は感激ですぞ～！！",
            "さすが若様！\n若様の勇姿に爺は感激ですぞ～！！",
            "さすが若様！\n若様の勇姿に爺は感激ですぞ～！！",
            "さすが若様！\n若様の勇姿に爺は感激ですぞ～！！",
        };
        //  ステージ番号を取得
        int stageNum = (int)PlayerInfoManager.stageInfo;
        //  1文字当たりにかかる時間
        float duration = 0.1f;

        //  テキストアニメーション開始
        honeGText.GetComponent<TextMeshProUGUI>().text = "";

        //変化前のテキスト
        var beforeText = honeGText.GetComponent<TextMeshProUGUI>().text;

        DOTweenTMPAnimator animator = new DOTweenTMPAnimator(honeGText.GetComponent<TextMeshProUGUI>());
        honeGText.GetComponent<TextMeshProUGUI>().DOText(message[stageNum],duration*message[stageNum].Length)
        .OnUpdate( () => 
        {
              var currentText = honeGText.GetComponent<TextMeshProUGUI>().text;
              if (beforeText == currentText)
                return;

            //  SEを鳴らす
            SoundManager.Instance.PlaySFX(
                (int)AudioChannel.SFX_SYSTEM,
                (int)SFXList.SFX_RESULT_TEXTNEXT);

            //次のチェック用にテキスト更新
            beforeText = currentText;
 
        });

        //  アニメーションを待つ
        yield return new WaitForSeconds(duration*message[stageNum].Length);

        //  終わったらボタンを表示
        nextButton.SetActive(true);

        //  PlayerInputのマップをUIモードに変更
        PlayerInput playerInput = GameManager.Instance.GetPlayer()
            .GetComponent<PlayerInput>();
        playerInput.enabled = true;
        playerInput.SwitchCurrentActionMap("Title_UI");

        //  ボタンを選択状態にする
        EventSystem.current.SetSelectedGameObject(nextButton.gameObject);



        yield return null;
    }
}
