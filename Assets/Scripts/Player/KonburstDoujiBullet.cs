using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  プレイヤーのドウジ魂バースト弾クラス
//
//--------------------------------------------------------------
public class KonburstDoujiBullet : MonoBehaviour
{
    private Vector3 velocity;
    private int gamestatus;
    private bool fripY;
    private const float priod = 5.0f;  //  寿命（秒）

    [SerializeField] private Vector3 positionStrength;
    [SerializeField] private Vector3 rotationStrength;

    void Start()
    {
        velocity = Vector3.zero;

        //  GameManagerから状態を取得
        gamestatus = GameManager.Instance.GetGameState();

        //  弾の向き
        switch(gamestatus)
        {
            case (int)eGameState.Zako:
                velocity = new Vector3(0,-20f,0);   //  下方向へ撃つ
                break;
            case (int)eGameState.Boss:
                velocity = new Vector3(0,20f,0);   //  上方向へ撃つ
                break;
            case (int)eGameState.Event:
                break;
        }

        //  現在の位置からvelocity分移動
        this.transform.DOMove(velocity,priod).SetRelative(true).SetEase(Ease.Linear);

        //  寿命が来たら消す
        Destroy(gameObject, priod);

        //  カメラを揺らす
        CameraShaker(priod);
    }

    void Update()
    {
        
    }

    //-------------------------------------------
    //  プロパティ
    //-------------------------------------------
    public void SetVelocity(Vector3 v){ velocity = v; }
    public Vector3 GetVelocity(){ return velocity; }
    public void SetFripY(bool frip){ fripY = frip; }
    public bool GetFripY(){ return fripY; }

    //-------------------------------------------
    //  カメラを揺らす
    //-------------------------------------------
    private void CameraShaker(float duration)
    {
        Camera.main.transform.DOComplete();
        Camera.main.transform.DOShakePosition(duration, positionStrength);
        Camera.main.transform.DOShakeRotation(duration, rotationStrength);
    }
}
