using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Numerics;

//--------------------------------------------------------------
//
//  �����̃A�j���[�V����
//
//--------------------------------------------------------------
public class ScrollAnimation : MonoBehaviour
{
    //  �V���O���g���ȃC���X�^���X
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

    //  �������J��
    public IEnumerator OpenScroll()
    {
        transform.DOMoveX(40,2)
        .SetEase(Ease.OutCubic)
        .OnStart(
        () =>
        {
            SoundManager.Instance.Play( (int)AudioChannel.SFX, (int)SFXList.SFX_HYOUSIGI);
            SoundManager.Instance.Play( (int)AudioChannel.SFX_ENEMY, (int)SFXList.SFX_SCROLL_MOVE);
            SoundManager.Instance.Play( (int)AudioChannel.SFX_SYSTEM, (int)SFXList.SFX_SCROLL_MOVE2);
        })
        .OnComplete(() =>{  });

        yield return null;
    }
}
