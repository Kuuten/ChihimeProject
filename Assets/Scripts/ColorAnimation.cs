using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//--------------------------------------------------------------
//
//  �F����F�ɕω�������N���X
//
//--------------------------------------------------------------
public class ColorAnimation : MonoBehaviour
{
    [SerializeField] private Gradient target;
    [SerializeField] private float duration = 0.5f;

    void Start()
    {
        ColorChange();
    }

    void Update()
    {
        
    }
    private void ColorChange()
    {
        this.GetComponent<Image>()
            .DOGradientColor(target, duration)
            .OnComplete(ColorChange);
    }
}
