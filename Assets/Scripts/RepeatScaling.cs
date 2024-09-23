using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  オブジェクトを拡大・縮小を繰り返させるクラス
//
//--------------------------------------------------------------
public class RepeatScaling : MonoBehaviour
{
    //  最大スケール
    [SerializeField] private float maxScale = 1.2f;
    //  最小スケール
    [SerializeField] private float minScale = 0.8f;
    //  一つのアニメーションにかかる時間
    [SerializeField] private float duration = 0.5f;

    void Start()
    {
        GetComponent<RectTransform>().localScale = 
            new Vector3(minScale,minScale,minScale);

        //  拡大する
        GetComponent<RectTransform>().DOScale(maxScale,duration)
                .SetEase(Ease.Linear)  
                .SetLoops(-1, LoopType.Yoyo); 
    }

    void Update()
    {
        
    }
}
