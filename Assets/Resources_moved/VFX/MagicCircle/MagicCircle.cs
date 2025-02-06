using DG.Tweening;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  魔法円クラス
//
//--------------------------------------------------------------
public class MagicCircle : MonoBehaviour
{
    [SerializeField, ShowAssetPreview]
    public Material mat;
    [Watch]public float radius;

    void Start()
    {
        mat = this.GetComponent<Renderer>().material;

        radius = 0f;

        // 数値の変更
        DOTween.To(
            () => radius,                  // 何を対象にするのか
            num => radius = num,           // 値の更新
            1.0f,                          // 最終的な値
            2.0f                           // アニメーション時間
        ).SetLoops(-1, LoopType.Restart);
    }

    private void OnDestroy()
    {
        if (mat != null) {
            Destroy(mat);
        }
    }

    void Update()
    {
       mat.SetFloat("_Radius", radius ); 
    }
}
