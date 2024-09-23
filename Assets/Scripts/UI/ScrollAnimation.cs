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


    //  ���������
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
