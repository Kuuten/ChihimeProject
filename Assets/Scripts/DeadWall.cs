using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//--------------------------------------------------------------
//
//  ���̕ǃN���X
//
//--------------------------------------------------------------
public class DeadWall : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Enemy�ɂ��������I");

            //  ���̕ǂɓ���������I�u�W�F�N�g����
            Destroy(other.gameObject);
        }

        if(other.gameObject.CompareTag("Kon"))
        {
            Debug.Log("Kon�ɂ��������I");
        }

        if(other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player�ɂ��������I");
        }
    }
}
