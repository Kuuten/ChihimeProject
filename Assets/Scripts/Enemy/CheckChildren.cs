using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  �q�I�u�W�F�N�g���P�ł����邩�`�F�b�N����
//
//--------------------------------------------------------------
public class CheckChildren : MonoBehaviour
{
    
    void Start()
    {
        
    }

    void Update()
    {
        //  �q�I�u�W�F�N�g��0�ɂȂ�Ώ���
        int children = this.transform.childCount;
        if(children == 0)Destroy(this.gameObject);
    }
}
