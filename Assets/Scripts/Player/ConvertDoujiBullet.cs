using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  プレイヤーのドウジ魂バート弾クラス
//
//--------------------------------------------------------------
public class ConvertDoujiBullet : MonoBehaviour
{
    //  Animator.speedみたいなやつを取得して
    //  それをDotweenでだんだん加速させる
    //  DOTween.Toを使えばやれるはず

    //  スケールもDoTweenでだんだんと小さくする

    private Animator animator;

    void Start()
    {
        animator = this.GetComponent<Animator>();

        //  ShotManager側で設定するようにする
        this.GetComponent<SpriteRenderer>().flipY = true;
    }

    void Update()
    {
        
    }
}
