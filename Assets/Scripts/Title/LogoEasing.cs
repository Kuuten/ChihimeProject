using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

//--------------------------------------------------------------
//
//  �^�C�g�����S�̃C�[�W���O�N���X
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
        //  �C�[�Y�C����������45�x�X����
        //--------------------------------------------
        Sequence sequence = DOTween.Sequence();
        bool complete = false;

        //  �w��ʒu�ֈړ�
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
            //  45�x��]
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
            //  45�x��]���Č��ɖ߂�
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


        //  �����܂ő҂�
        yield return new WaitUntil(() => complete == true);
    }
}
