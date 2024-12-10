using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//--------------------------------------------------------------
//
//  背景画像のフェードクラス
//
//--------------------------------------------------------------
public class BackGroundFade : MonoBehaviour
{
    void Start()
    {
        
    }

    private void OnEnable()
    {
        StartCoroutine(StartFadeOut());
    }

    // フェードアウト 
    public IEnumerator StartFadeOut()
    {
        bool complete = false;
        float duration = 3.0f;  //  アニメーション時間

        //  アニメーション準備
        Image fadeImage = GetComponent<Image>();
        fadeImage.enabled = true;
        Color c = fadeImage.color;
        c.a = 0.0f; // 初期値
        fadeImage.color = c;

        Sequence sequence = DOTween.Sequence();

        sequence.Append
            (
                    DOTween.ToAlpha
                    (
                        () => fadeImage.color,
                        color => fadeImage.color = color,
                        1f, // 目標値
                        duration // 所要時間
                    )
                    .OnStart(() => {

                    })
                    .OnComplete(
                    () =>
                    {
                        complete = true;
                    })
            );

        //  完了まで待つ
        yield return new WaitUntil(() => complete == true);
    }
}
