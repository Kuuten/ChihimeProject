using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Numerics;

//--------------------------------------------------------------
//
//  巻物のアニメーション
//
//--------------------------------------------------------------
public class ScrollAnimation : MonoBehaviour
{
    //  シングルトンなインスタンス
    public static ScrollAnimation Instance
    {
        get; private set;
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {

    }

    void Update()
    {
        
    }

    //  巻物を開く
    public IEnumerator OpenScroll()
    {
        this.GetComponent<RectTransform>().DOAnchorPosX(40,2)
        .SetEase(Ease.OutCubic)
        .OnStart(
        () =>
        {
            SoundManager.Instance.PlaySFX( (int)AudioChannel.SFX, (int)SFXList.SFX_HYOUSIGI);
        })
        .OnComplete(() =>{  });

        yield return null;
    }


    //  巻物を閉じる
    public IEnumerator CloseScroll()
    {
        this.GetComponent<RectTransform>().DOAnchorPosX(1240.5f,2)
        .SetEase(Ease.OutCubic)
        .OnStart(
        () =>
        {
            SoundManager.Instance.PlaySFX( (int)AudioChannel.SFX, (int)SFXList.SFX_HYOUSIGI);
        })
        .OnComplete(() =>{  });

        yield return null;
    }
}
