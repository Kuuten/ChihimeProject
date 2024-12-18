using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;


//--------------------------------------------------------------
//
//  �^�C�g�����B�W���A���̃C�[�W���O�N���X
//
//--------------------------------------------------------------
public class VisualEasing : MonoBehaviour
{

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    //--------------------------------------------------------------
    //  �^�C�g���r�W���A���̃C�[�W���O
    //--------------------------------------------------------------
    public IEnumerator EasingTitleVisual()
    {
        bool complete = false;

        GetComponent<RectTransform>().DOAnchorPos(
            new UnityEngine.Vector2(40f,0f),0.5f)
        .SetEase(Ease.InOutQuint)
        .OnStart(() => {
            SoundManager.Instance.PlaySFX( (int)AudioChannel.SFX, (int)SFXList.SFX_VISUAL);
        })
        .OnComplete(() =>{
            complete = true;
        });

        //  �����܂ő҂�
        yield return new WaitUntil(() => complete == true);
    }
}
