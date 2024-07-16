using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


//
//  背景板２枚を切り替えて行く方式で行く
//
public class BackGroundMove : MonoBehaviour
{
    //  スクロールするスピード
    [SerializeField] private float scrollSpeed = 1.0f;
    //  折り返し地点用オブジェクト
    [SerializeField] private GameObject returnPoint;
    //  再スタート地点用オブジェクト
    [SerializeField] private GameObject restartPoint;

    void Start()
    {
        
    }

    void Update()
    {
        this.transform.position += new Vector3(0,-scrollSpeed,0)*Time.deltaTime;

        if (this.transform.position.y <= returnPoint.transform.position.y)
        {
            this.transform.position = restartPoint.transform.position;
        }
    }
}
