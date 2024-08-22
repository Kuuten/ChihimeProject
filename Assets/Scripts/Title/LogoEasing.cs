using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

//--------------------------------------------------------------
//
//  タイトルロゴのイージングクラス
//
//--------------------------------------------------------------
public class LogoEasing : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {
        
    }

    public IEnumerator EasingTitlelogo()
    {
         //-------------------------------------------
        //  イーズインした後上に45度傾ける
        //--------------------------------------------
        Sequence sequence = DOTween.Sequence();
        bool complete = false;

        //  指定位置へ移動
        sequence.Append
            (
                    GetComponent<RectTransform>().DOAnchorPos(
                        new UnityEngine.Vector2(-100f,0f),0.5f)
                    .SetEase(Ease.InOutQuint)
                    .OnStart(() => {
                        SoundManager.Instance.PlaySFX( (int)AudioChannel.SFX, (int)SFXList.SFX_DASH);
                    })
                    .OnComplete(() =>{

                    })
            )
            //  45度回転
            .Append
            (
                transform.DOLocalRotate(new UnityEngine.Vector3(0, 0, 45f), 1f)  
                .SetEase(Ease.OutExpo)  
                .OnStart(() => {
                    SoundManager.Instance.PlaySFX( (int)AudioChannel.SFX, (int)SFXList.SFX_BRAKE);
                })
                .OnComplete(() =>{

                })
            )
            //  45度回転して元に戻る
            .Append
            (
                transform.DOLocalRotate(new UnityEngine.Vector3(0, 0, 0), 0.5f)  
                .SetEase(Ease.InExpo)  
                .OnStart(() => {

                })
                .OnComplete(() =>{
                    complete = true;
                })
            );


        //  完了まで待つ
        yield return new WaitUntil(() => complete == true);
    }
}
