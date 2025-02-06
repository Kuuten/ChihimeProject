using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


//
//  背景板2枚を切り替えて行く方式で行く
//
public class BackGroundMove3D : MonoBehaviour
{
    //  スクロールするスピード
    [SerializeField] private float scrollSpeed = 1.0f;

    void Start()
    {
        
    }

    void Update()
    {
        transform.position += new Vector3(0,-scrollSpeed,0) * Time.deltaTime;

        if (transform.position.y <= -99.5f)
        {
            transform.position = new Vector3(-6.4f,99.5f,70f);
        }
    }
}
