using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


//
//  背景板枚を切り替えて行く方式で行く
//
public class BackGroundMove : MonoBehaviour
{
    //  スクロールするスピード
    [SerializeField] private float scrollSpeed = 1.0f;

    void Start()
    {
        
    }

    void Update()
    {
        RectTransform rt = this.GetComponent<RectTransform>();
        rt.anchoredPosition += new Vector2(0,-scrollSpeed)*Time.deltaTime;

        if (rt.anchoredPosition.y <= -1080f)
        {
            rt.anchoredPosition = new Vector2(-58,1080.0f);
        }
    }
}
