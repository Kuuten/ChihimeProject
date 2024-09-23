using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  オブジェクトを回転させるクラス
//
//--------------------------------------------------------------
public class Rotaion : MonoBehaviour
{
    // 左回転か右回転かどうかの設定
    public enum LeftOrRight {
        Left,                      //  左回転
        Right,                     //  右回転
    }
    public LeftOrRight leftOrRight = LeftOrRight.Left;

    //  回転時間
    [SerializeField] private float duraiton;

    // Start is called before the first frame update
    void Start()
    {
        // スプライトを回転させる
        if( leftOrRight == LeftOrRight.Left)
        {
            transform.DORotate(new Vector3(0, 0, 360f), duraiton, RotateMode.FastBeyond360)  
                .SetEase(Ease.Linear)  
                .SetLoops(-1, LoopType.Incremental);
        }
        else
        {
            transform.DORotate(new Vector3(0, 0, -360f), duraiton, RotateMode.FastBeyond360)  
                .SetEase(Ease.Linear)  
                .SetLoops(-1, LoopType.Incremental);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
