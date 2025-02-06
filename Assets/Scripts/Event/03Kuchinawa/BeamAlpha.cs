using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  ビームのアルファクラス
//
//--------------------------------------------------------------
public class BeamAlpha : MonoBehaviour
{
    private void OnEnable()
    {
        transform.GetComponent<SpriteRenderer>().color = Color.white;
    }

    void Update()
    {
        
    }

    public IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(2.0f);

        
        transform.DOScaleX( 
                            GetComponent<BeamScaling>().GetBeforeScale().x,
                            0.5f
                          )
                          .SetEase(Ease.InExpo);

        yield return new WaitForSeconds(0.5f);

        transform.GetComponent<SpriteRenderer>().DOFade(0f,1f)
            .OnComplete(()=>{ transform.gameObject.SetActive(false); });

        yield return null;
    }
}
