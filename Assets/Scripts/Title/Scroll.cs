using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  タイトルの背景の柄をスクロールさせる
//
//--------------------------------------------------------------
public class Scroll : MonoBehaviour
{
    private RectTransform rect;
    private Vector2 velocity;
    [SerializeField] private float period = 10.0f;
    private const float resolutionX = 1280f;

    void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    void Update()
    {
        velocity.x = Screen.width / period * Time.deltaTime;
        velocity.y = Screen.height / period * Time.deltaTime;

        if(rect.anchoredPosition.x <= -resolutionX)
        {
            //Debug.Log("ScreenWidth " + Screen.width);
            rect.anchoredPosition = new Vector2(0,0);
        }
        else
        {
            rect.anchoredPosition += new Vector2(-velocity.x,-velocity.y); 
        }
    }
}
