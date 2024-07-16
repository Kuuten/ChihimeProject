using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using System;
using DG.Tweening;
using Unity.VisualScripting;

//--------------------------------------------------------------
//
//  プレイヤーの体力管理クラス
//
//--------------------------------------------------------------

//  基本体力はハート３個分（半ハート６個分）
//  最大体力はハート６個分（半ハート１２個分）
//  最小単位の半ハート換算でプログラムする
public class PlayerHealth : MonoBehaviour
{
    private int currentMaxHealth;
    private int currentHealth;
    private const int limitHealth = 12;

    [SerializeField]private GameObject enemyGenerator;



    void Start()
    {
        currentMaxHealth = 6;
        currentHealth = 6;
    }

    void Update()
    {
        //if (test.WasPressedThisFrame())
        //{
        //    testSwitch = !testSwitch;

        //    //  敵のジェネレーターを無効化
        //    SetGeneratorActive(testSwitch);

        //    if(testSwitch == false)
        //    {
        //        //  Pauserが付いたオブジェクトをポーズ
        //        Pauser.Pause();
        //        //  ポーズ後に入力がきかなくなるのでリセット
        //        this.GetComponent<PlayerInput>().enabled = true;
        //    }
        //    else
        //    {
        //        //  Pauserが付いたオブジェクトをポーズ
        //        Pauser.Resume(); 
        //    }
        //}
    }

    //-------------------------------------------
    //  ダメージ処理
    //-------------------------------------------
    public void Damage(int value)
    {
        if(currentHealth > 0)
        {
            currentHealth -= value;
        }
        else
        {
            currentHealth = 0;

            //  プレイヤーを止める
            this.GetComponent<PlayerMovement>().enabled = false;
            this.GetComponent<PlayerShotManager>().enabled = false;
            this.GetComponent<BoxCollider2D>().enabled = false;

            //  やられ演出
            StartCoroutine(Death());
        }
        
    }

    //-------------------------------------------
    //  回復処理
    //-------------------------------------------
    public void Heal(int value)
    {
        int target = currentHealth + value;

        // 数値の変更
        DOTween.To(
            () => currentHealth,          // 何を対象にするのか
            num => currentHealth = num,   // 値の更新
            target,                       // 最終的な値
            value/2                       // アニメーション時間
        );
    }

    //-------------------------------------------
    //  体力のプロパティ
    //-------------------------------------------
    public void SetCurrentHealth(int value)
    {
        Assert.IsFalse((value < 0 || value > currentMaxHealth),
            "体力に範囲外の数が設定されています！");

        if(currentMaxHealth != value)currentMaxHealth = value;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public void SetCurrentMaxHealth(int value)
    {
        Assert.IsFalse((value < 0 || value > limitHealth),
            "最大体力に範囲外の数が設定されています！");

        if(currentMaxHealth != value)currentMaxHealth = value;
    }

    public int GetCurrentMaxHealth()
    {
        return currentMaxHealth;
    }

    //-------------------------------------------
    //  やられ演出
    //-------------------------------------------
    private IEnumerator Death()
    {
        //  Pauserが付いたオブジェクトをポーズ
        Pauser.Pause();


        yield return null;
    }


    private IEnumerator Death2()
    {
        //-------------------------------------------
        //  上へ飛び上がった後、下へ落ちていく
        //-------------------------------------------
        DG.Tweening.Sequence sequence = DOTween.Sequence();
        bool complete = false;

        //  指定位置へ移動
        sequence.Append
            (
                    transform.DOLocalMoveY(2,0.5f)
                    .SetEase(Ease.OutCubic)
                    .SetRelative(true)  //  相対値移動
                    .OnStart(() => {
                        //Sound.Play( (int)AudioChannel.SFX, (int)SFXList.SFX_DASH);
                    })
                    .OnComplete(() =>{

                    })

            )
            .Append
            (
                transform.DOMoveY(-13,1.5f)
                .SetEase(Ease.InSine)  
                .OnStart(() => {
                    
                })
                .OnComplete(() =>{

                })
            );

        //  完了まで待つ
        yield return new WaitUntil(() => complete == true);
    }
}
