using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  �G�̒e�N���X
//
//--------------------------------------------------------------
public class EnemyBullet : MonoBehaviour
{
    
    void Start()
    {
        
    }

    void Update()
    {
        //  ������ɔ��˂����
        transform.position += new Vector3(2,3,0) * Time.deltaTime;
    }
}
