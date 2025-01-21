using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Enemy;
using UniRx;


//--------------------------------------------------------------
//
//  ツクモのPhase2の弾クラス
//
//--------------------------------------------------------------
public class TsukumoPhase2Bullet_B_Knockback : MonoBehaviour
{
    GameObject parent;

    void Awake()
    {
        //  現れるまでは当たり判定無効
        this.GetComponent<BoxCollider2D>().enabled = false;

        //  親オブエジェクトをキャッシュ
        parent = transform.parent.gameObject;

        // UniRx を使用して親のアルファ値を監視
        Observable.EveryUpdate()
            .Select(_ => parent.GetComponent<SpriteRenderer>().color.a) // 親のアルファ値を取得
            .DistinctUntilChanged()             // 値が変化したときのみ通知
            .Where(alpha => alpha == 1.0f)      // アルファ値が1.0のときにフィルタ
            .Subscribe(_ =>
            {
                // BoxCollider2D を有効化
                this.GetComponent<BoxCollider2D>().enabled = true;
            })
            .AddTo(this); // コンポーネントのライフサイクルで購読を破棄
    }

    void Update()
    {
        ///***********デバッグ表示**************************************/
        //Vector3 vec = transform.up;
        //Debug.DrawRay(transform.position, vec*1.0f, Color.red);
        ////-------------------------------------------------------------
    }

    //-------------------------------------------------
    //  当たり判定
    //-------------------------------------------------
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //  死んでいるか画面外なら処理しない
        if(parent.GetComponent<TsukumoPhase2Bullet_B>().GetHP() <= 0)return;

        //  プレイヤーにぶつかったらノックバック
        else if (collision.CompareTag("Player"))
        {
            //  ノックバック
            KnockBack(collision);
        }
    }

    //------------------------------------------------------
    //  ノックバック
    //------------------------------------------------------
    private void KnockBack(Collider2D collision)
    {
        float distance = 1.0f;  //  ノックバック距離
        float duration = 0.1f; //  移動にかかる時間（秒）

        //  障子からのベクトルを計算
        Vector3 vec = transform.up;
        Vector3 ppos = collision.transform.position;
        Vector3 direction = default;    //  ノックバック方向

        //Debug.Log($"<color=yellow>ノックバック！</color>");

        //  衝突した時プレイヤーが前か後ろか判定する
        float dot = (vec.x * ppos.x) + (vec.y * ppos.y);

        if(dot > 0) //  前にいる
        {
            direction = vec;
        }
        else if( dot < 0 )  //  後ろにいる
        {
             direction = -vec;

            //Debug.Log($"<color=yellow>後ろにいる！！{direction}</color>");
        }
        else // 真横にいる
        {
            //  プレイヤーへのベクトルを計算して設定
            Vector3 v = ppos - transform.position;
            v.Normalize();
            direction = v;
        }

        //  ノックバック後の座標を計算
        Vector3 pos = ppos + direction * distance;

        //  移動開始
        collision.transform.DOMove(pos, duration)
            .SetEase(Ease.Linear);
    }
}
