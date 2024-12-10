using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

//--------------------------------------------------------------
//
//  ポップアップウィンドウクラス
//
//--------------------------------------------------------------
public class PopupWindow : MonoBehaviour
{
    //  アニメーションの完了フラグ
    private bool isComplete;

    void Start()
    {

    }

    private void OnEnable()
    {
        Init();
    }

    private void Init()
    {
        //  初期化
        isComplete = false;

        //  ポップアップ開始
        StartCoroutine(Popup());
    }

    void Update()
    {
        
    }

    //--------------------------------------------------------------
    //  ポップアップアニメーション
    //--------------------------------------------------------------
    private IEnumerator Popup()
    {
        float openDuration = 0.4f;   //  アニメーション時間(秒)
        float closeDuration = 0.2f;  //  アニメーション時間(秒)

        //  一旦スケールを小さくする
        transform.localScale = new Vector3(0f,0f,0f);

        //  アニメーション開始
        yield return StartCoroutine(PopupOpen(openDuration));

        //  待機時間
        yield return new WaitForSeconds(1f);

        //  アニメーション開始
        yield return StartCoroutine(PopupClose(closeDuration));

        yield return null;
    }

    //--------------------------------------------------------------
    //  ポップアップオープン
    //--------------------------------------------------------------
    private IEnumerator PopupOpen(float duration)
    {
        transform.DOScale(1f,duration)
            .SetEase(Ease.InOutElastic);

        yield return null;
    }

    //--------------------------------------------------------------
    //  ポップアップ・クローズ
    //--------------------------------------------------------------
    private IEnumerator PopupClose(float duration)
    {
        transform.DOScale(0f,duration)
            .OnComplete(()=>
            {
                isComplete = true;  //  アニメーション完了
                this.gameObject.SetActive(false);
            });

        yield return null;
    }
}
