using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//--------------------------------------------------------------
//
//  プレイヤーのボムのフェード処理板クラス
//
//--------------------------------------------------------------
public class BombFade : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {
        
    }

    // フェードイン 
    public IEnumerator StartFadeIn(GameObject obj, float time)
    {
        //  ゲームオブジェクトがない場合は抜ける
        if(obj == null)yield break;

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
            time // 所要時間
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
    public IEnumerator StartFadeOut(GameObject obj, float time)
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
            time // 所要時間
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

    // 強制削除 
    public IEnumerator DestroyObject()
    {
        //  ゲームオブジェクトがない場合は抜ける
        if(this.gameObject == null)yield break;

        DestroyImmediate (this.gameObject, true);

        yield return null;
    }
}
