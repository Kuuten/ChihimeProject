using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  ツクモのPhase2の弾クラス
//
//--------------------------------------------------------------
public class TsukumoPhase2Bullet : MonoBehaviour
{
    private int power;

    public enum Direction
    {
        TOP,
        BOTTOM,
        LEFT,
        RIGHT,

        MAX
    }
    private Direction direction;

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
    public void SetDirection(Direction d) { this.direction = d; }

    //---------------------------------------------------------------
    //  弾が移動していく
    //---------------------------------------------------------------
    public IEnumerator BulletMove()
    {
        float duration = 3.0f;  //  アニメーション時間

        //  寿命を設定
        Destroy(gameObject, duration);

        //  SEを再生
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.ENEMY_SHOT,
            (int)SFXList.SFX_ENEMY_SHOT);

        if(direction == Direction.TOP)
        {
            //  Y軸移動
            transform.DOMoveY(-20f,duration);
        }
        else if(direction == Direction.BOTTOM)
        {
            //  Y軸移動
            transform.DOMoveY(20f,duration);
        }
        else if(direction == Direction.LEFT)
        {
            //  X軸移動
            transform.DOMoveX(20f,duration);
        }
        else if(direction == Direction.RIGHT)
        {
            //  X軸移動
            transform.DOMoveX(-20f,duration);
        }

        yield return null;
    }
}
