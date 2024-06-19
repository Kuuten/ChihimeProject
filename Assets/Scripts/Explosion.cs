using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  爆発エフェクトクラス
//
//--------------------------------------------------------------
public class Explosion : MonoBehaviour
{
    void Start()
    {
        Destroy(this.gameObject, 0.5f);
    }

    void Update()
    {
        
    }
}
