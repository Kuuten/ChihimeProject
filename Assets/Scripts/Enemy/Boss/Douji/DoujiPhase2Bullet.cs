using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  ドウジのPhase2の弾クラス
//
//--------------------------------------------------------------
public class DoujiPhase2Bullet : MonoBehaviour
{
    private int power;

    public enum KooniDirection
    {
        TOP,
        BOTTOM,
        LEFT,
        RIGHT,

        MAX
    }
    private KooniDirection direction;

    void Start()
    {

    }

    private void OnDestroy()
    {
        
    }

    void Update()
    {
        
    }

    //---------------------------------------------------------------
    //  プロパティ
    //---------------------------------------------------------------
    public void SetPower(int p){ power = p; }
    public int GetPower() { return power; }
    public void SetDirection(KooniDirection d) { this.direction = d; }

    //---------------------------------------------------------------
    //  子鬼が突撃する
    //---------------------------------------------------------------
    public IEnumerator KooniRush()
    {
        float duration = 3.0f;  //  アニメーション時間

        //  揺らす
        transform.DOPunchScale(Vector3.one * 0.98f, duration, 5, 0.01f)
            .SetLoops(-1, LoopType.Incremental);

        //  寿命を設定
        Destroy(gameObject, duration);

        //  SEを再生
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.ENEMY_SHOT,
            (int)SFXList.SFX_DOUJI_KOONIRUSH);

        if(direction == KooniDirection.TOP)
        {
            //  Y軸移動
            transform.DOMoveY(-20f,duration);
        }
        else if(direction == KooniDirection.BOTTOM)
        {
            //  Y軸移動
            transform.DOMoveY(20f,duration);
        }
        else if(direction == KooniDirection.LEFT)
        {
            //  X軸移動
            transform.DOMoveX(20f,duration);
        }
        else if(direction == KooniDirection.RIGHT)
        {
            //  X軸移動
            transform.DOMoveX(-20f,duration);
        }

        yield return null;
    }
}
