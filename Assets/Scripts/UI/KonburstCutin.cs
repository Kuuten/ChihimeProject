using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.UI;


//--------------------------------------------------------------
//
//  プレイヤーの魂バーストのカットイン演出クラス
//
//--------------------------------------------------------------
public class KonburstCutin : MonoBehaviour
{
    void Start()
    {
        //  初期座標・スケール設定
        this.GetComponent<RectTransform>().anchoredPosition =
            new Vector3 (1140, 0, 0);
        this.transform.localScale =
            new Vector3(1,0.2f,1);

        //  カットイン開始
        StartCoroutine(CutinAnimation());
    }

    void Update()
    {
        
    }

    //--------------------------------------------------
    //  アニメーションさせる
    //--------------------------------------------------
    private IEnumerator CutinAnimation()
    {
        //  合計2.3秒

        //  イーズイン
        this.GetComponent<RectTransform>().DOAnchorPos(new Vector3(-59.25f,0),0.4f)
            .SetEase(Ease.InExpo);

        yield return new WaitForSeconds(0.4f);

        //  スケーリング
        this.GetComponent<RectTransform>().DOScale(1f, 0.4f)
            .SetEase(Ease.InExpo);

        yield return new WaitForSeconds(0.4f);

        //  0.5秒待機
        yield return new WaitForSeconds(0.5f);

        //  アルファアニメーション
        var  fadeImage = GetComponent<Image>();
        fadeImage.enabled = true;
        var c = fadeImage.color;
        c.a = 1.0f; // 初期値
        fadeImage.color = c;

        DOTween.ToAlpha(
	        ()=> fadeImage.color,
	        color => fadeImage.color = color,
	        0f, // 目標値
	        1f // 所要時間
        );

        //  0.5秒待機
        yield return new WaitForSeconds(1.0f);
    }
}
