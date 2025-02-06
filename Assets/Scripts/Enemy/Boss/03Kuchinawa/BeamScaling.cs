using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UIElements;

//--------------------------------------------------------------
//
//  �r�[���̃X�P�[�����O�N���X
//
//--------------------------------------------------------------
public class BeamScaling : MonoBehaviour
{
    [SerializeField]
    private Vector3 before_scale;  //  ���̃X�P�[���ɂ�����l

    [SerializeField]
    private Vector3 after_scale;  //  ���̃X�P�[���ɂ�����l

    private void OnEnable()
    {
        transform.localScale = new Vector3( 
                                            before_scale.x,
                                            before_scale.y,
                                            before_scale.z
                                          );

        YScalling();

    }

    void Update()
    {
        
    }

    public void YScalling()
    {
        transform.DOScaleY( after_scale.y, 0.5f)
            .SetEase(Ease.InExpo)
            .OnComplete(()=>{ XScalling(); });
    }

    public void XScalling()
    {
        transform.DOScaleX(after_scale.x, 0.5f)
            .SetEase(Ease.InExpo)
            .OnComplete(()=>{ StartCoroutine(GetComponent<BeamAlpha>().FadeOut()); });
    }

    public Vector3 GetBeforeScale(){ return before_scale; }
    public Vector3 GetAfterScale(){ return after_scale; }
}
