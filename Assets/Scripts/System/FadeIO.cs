using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Numerics;

//--------------------------------------------------------------
//
//  フェードイン・アウトクラス
//
//--------------------------------------------------------------
public class FadeIO : MonoBehaviour
{

    void Start()
    {

    }

    public IEnumerator StartAnimation()
    {
        bool complete = false;

        //  アニメーション準備
        Image fadeImage = GetComponent<Image>();
        fadeImage.enabled = true;

        Color c = fadeImage.color;
        c.a = 1.0f; // 初期値
        fadeImage.color = c;

        Sequence sequence = DOTween.Sequence();

        sequence.Append
            (
                    DOTween.ToAlpha
                    (
                        () => fadeImage.color,
                        color => fadeImage.color = color,
                        1f, // 目標値
                        0.5f // 所要時間
                    )
                    .OnStart(() => {

                    })
            );
        sequence.Append
            (
                DOTween.ToAlpha
                (
                    () => fadeImage.color,
                    color => fadeImage.color = color,
                    0f, // 目標値
                    0.5f // 所要時間
                )
            )
            .OnStart(
            () =>
            {

            })
            .OnComplete(
            () =>
            {
                complete = true;
            });

        //  完了まで待つ
        yield return new WaitUntil(() => complete == true);
    }

    // フェードイン 
    public IEnumerator StartFadeIn()
    {
        bool complete = false;

        //  アニメーション準備
        Image fadeImage = GetComponent<Image>();
        fadeImage.enabled = true;
        Color c = fadeImage.color;
        c.a = 1.0f; // 初期値
        fadeImage.color = c;

        Sequence sequence = DOTween.Sequence();
        sequence.Append
    (
        DOTween.ToAlpha
        (
            () => fadeImage.color,
            color => fadeImage.color = color,
            0f, // 目標値
            1.0f // 所要時間
        )
    )
    .OnStart(
    () =>
    {

    })
    .OnComplete(
    () =>
    {
        complete = true;
    });

      //  完了まで待つ
      yield return new WaitUntil(() => complete == true);
    }

    // フェードアウト 
    public IEnumerator StartFadeOut()
    {
        bool complete = false;

        //  アニメーション準備
        Image fadeImage = GetComponent<Image>();
        fadeImage.enabled = true;
        Color c = fadeImage.color;
        c.a = 1.0f; // 初期値
        fadeImage.color = c;

        Sequence sequence = DOTween.Sequence();

        sequence.Append
            (
                    DOTween.ToAlpha
                    (
                        () => fadeImage.color,
                        color => fadeImage.color = color,
                        1f, // 目標値
                        1.0f // 所要時間
                    )
                    .OnStart(() => {

                    })
            );

        //  完了まで待つ
        yield return new WaitUntil(() => complete == true);
    }
}