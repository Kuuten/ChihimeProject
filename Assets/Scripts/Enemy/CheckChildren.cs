using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  子オブジェクトが１個でもあるかチェックする
//
//--------------------------------------------------------------
public class CheckChildren : MonoBehaviour
{
    
    void Start()
    {
        
    }

    void Update()
    {
        //  子オブジェクトが0になれば消去
        int children = this.transform.childCount;
        if(children == 0)Destroy(this.gameObject);
    }
}
