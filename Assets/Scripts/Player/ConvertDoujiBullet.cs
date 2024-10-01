using DG.Tweening;
using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerShotManager;

//--------------------------------------------------------------
//
//  プレイヤーのドウジ魂バート弾クラス
//
//--------------------------------------------------------------
public class ConvertDoujiBullet : MonoBehaviour
{
    private Animator animator;
    private Vector3 velocity; 
    private int gamestatus;
    private bool fullPower;

    void Start()
    {
        animator = this.GetComponent<Animator>();
        velocity = Vector3.zero;

        //  GameManagerから状態を取得
        gamestatus = GameManager.Instance.GetGameState();

        //  弾の向き
        switch(gamestatus)
        {
            case (int)eGameState.Zako:
                velocity = new Vector3(0,-5f,0);   //  下方向へ撃つ
                break;
            case (int)eGameState.Boss:
                velocity = new Vector3(0,5f,0);   //  上方向へ撃つ
                break;
            case (int)eGameState.Event:
                break;
        }

        if(fullPower)
        {
            //  強攻撃SE再生
            SoundManager.Instance.PlaySFX(
                (int)AudioChannel.SFX_CONVERT_SHOT,
                (int)SFXList.SFX_DOUJI_CONVERT_SHOT_FULL);
        }
        else
        {
            //  中攻撃SE再生
            SoundManager.Instance.PlaySFX(
                (int)AudioChannel.SFX_CONVERT_SHOT,
                (int)SFXList.SFX_DOUJI_CONVERT_SHOT_MIDDLE);
        }

        //  現在の位置からvelocity分移動
        this.transform.DOMove(velocity,0.66f).SetRelative(true).SetEase(Ease.InOutQuint);

        //  スケールをだんだん小さくする
        this.transform.DOScale(new Vector3(0.1f,0.1f,0.1f),0.66f).SetEase(Ease.InExpo);
    }

    void Update()
    {
        //  ↓が１ならアニメーションが終了している(0はレイヤー番号:BaseLayer)
        if(animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.66f)
        {
            Destroy(this.gameObject);
        }
    }

    //-----------------------------------------------------------
    //  プロパティ
    //-----------------------------------------------------------
    public void SetVelocity(Vector3 v){ velocity = v; }
    public Vector3 GetVelocity(){ return velocity; }
    public void SetFullPower(bool b){ fullPower = b; }
    public bool GetFullPower(bool b){ return fullPower; }


}
