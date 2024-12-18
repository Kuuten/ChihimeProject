using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//--------------------------------------------------------------
//
//  タイトルヴィジュアルのフェードインクラス
//
//--------------------------------------------------------------
public class VisualFadeIn : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    //--------------------------------------------------------------
    //  タイトルビジュアルのフェードイン
    //--------------------------------------------------------------
    public IEnumerator FadeInTitleVisual()
    {
        bool complete = false;

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
}
