using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//--------------------------------------------------------------
//
//  ドウジのPhase3の弾クラス
//
//--------------------------------------------------------------
public class DoujiPhase3Bullet : MonoBehaviour
{
    private int power;

    void Awake()
    {
        power = 0;
    }

    void Start()
    {
        //  発狂弾の移動
        StartCoroutine(BerserkBullet());
    }

    void Update()
    {

    }

    //-------------------------------------------------
    //  当たり判定
    //-------------------------------------------------
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("DeadWall") ||
            collision.CompareTag("DeadWallBottom"))
        {
            Destroy(this.gameObject);
        }
    }


    //------------------------------------------------------
    //  プロパティ
    //------------------------------------------------------
    public void SetPower(int p){ power = p; }
    public int GetPower(){ return power; }

    //------------------------------------------------------
    //  発狂弾の移動
    //------------------------------------------------------
    private IEnumerator BerserkBullet()
    {
        //  アルファアニメーション
        float duration = 1.0f;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.enabled = true;
        Color c = sr.color;
        c.a = 0.0f; // 初期値
        sr.color = c;

        DOTween.ToAlpha(
	        ()=> sr.color,
	        color => sr.color = color,
	        1.0f,       // 目標値
	        duration    // 所要時間
        );

        yield return new WaitForSeconds(duration);

        //  移動開始
        this.transform.DOMoveY(-20f,0.5f)
            .SetRelative(true)
            .SetEase(Ease.InOutQuint);

        //  SEを再生
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX_ENEMY,
            (int)SFXList.SFX_DOUJI_SHOT2);

        yield return null;
    }
}
