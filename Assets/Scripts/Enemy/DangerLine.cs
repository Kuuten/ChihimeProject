using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//--------------------------------------------------------------
//
//  ドウジのPhase2の弾の予測進路を表示するクラス
//
//--------------------------------------------------------------
public class DangerLine : MonoBehaviour
{
    // どっちの方向のラインか
    public enum LINE_TYPE {
        Horizontal,    //  縦
       Vertical        //  横
    }
    public LINE_TYPE lineType = LINE_TYPE.Horizontal;

    void Start()
    {
        float duration = 0.5f; //  かかる時間
        int loopNum = 2;       //  ループ回数

        if(lineType == LINE_TYPE.Horizontal)
        {
            //  一旦小さくしとく
            this.GetComponent<RectTransform>().localScale = new Vector3(0,1,1);
            //  幅アニメーション
            this.GetComponent<RectTransform>().DOScaleX(1.0f,duration)
                .SetEase(Ease.InOutCubic)
                .SetLoops(loopNum);
            //  ゲージのアニメーション
            Slider slider = this.GetComponent<Slider>();
            slider.value = 0f;
            DOTween.To(
                () => slider.value,             // 何を対象にするのか
                num => slider.value = num,      // 値の更新
                1.0f,                           // 最終的な値
                duration                        // アニメーション時間
                )
                .SetEase(Ease.InOutCubic)
                .SetLoops(loopNum);
        }
        else
        {
            //  一旦小さくしとく
            this.GetComponent<RectTransform>().localScale = new Vector3(0,1,1);
            this.GetComponent<RectTransform>().DOScaleX(1.0f,duration)
                .SetEase(Ease.InOutCubic)
                .SetLoops(loopNum);
            //  ゲージのアニメーション
            Slider slider = this.GetComponent<Slider>();
            slider.value = 0f;
            DOTween.To(
                () => slider.value,             // 何を対象にするのか
                num => slider.value = num,      // 値の更新
                1.0f,                           // 最終的な値
                duration                        // アニメーション時間
                )
                .SetEase(Ease.InOutCubic)
                .SetLoops(loopNum);
        }
    }

    void Update()
    {
        
    }
}
