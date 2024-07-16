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
    [SerializeField] private SoundManager Sound;

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
        transform.DOMoveX(40,2)
        .SetEase(Ease.OutCubic)
        .OnStart(
        () =>
        {
            Sound.Play( (int)AudioChannel.SFX, (int)SFXList.SFX_HYOUSIGI);
            Sound.Play( (int)AudioChannel.SFX_ENEMY, (int)SFXList.SFX_SCROLL_MOVE);
            Sound.Play( (int)AudioChannel.SFX_SYSTEM, (int)SFXList.SFX_SCROLL_MOVE2);
        })
        .OnComplete(() =>{  });

        yield return null;
    }
}
