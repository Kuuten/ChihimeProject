using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


//
//  �w�i����؂�ւ��čs�������ōs��
//
public class OverRayMove : MonoBehaviour
{
    //  �X�N���[������X�s�[�h
    [SerializeField] private float scrollSpeed = 1.0f;

    void Start()
    {
        
    }

    void Update()
    {
        RectTransform rt = this.GetComponent<RectTransform>();
        rt.anchoredPosition += new Vector2(0,-scrollSpeed)*Time.deltaTime;

        if (rt.anchoredPosition.y <= -1400f)
        {
            rt.anchoredPosition = new Vector2(-58,1400.0f);
        }
    }
}
