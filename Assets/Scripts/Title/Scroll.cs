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

    void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    void Update()
    {
        velocity.x = Screen.width / period * Time.deltaTime;
        velocity.y = Screen.height / period * Time.deltaTime;

        if(rect.localPosition.x <= -Screen.width)
        {
            rect.localPosition = new Vector3(0,0,0);
        }
        else rect.localPosition += new Vector3(-velocity.x,-velocity.y,0); 
    }
}
