using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//--------------------------------------------------------------
//
//  死の壁クラス
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
            Debug.Log("Enemyにあたった！");

            //  死の壁に当たったらオブジェクト消去
            Destroy(other.gameObject);
        }

        if(other.gameObject.CompareTag("Kon"))
        {
            Debug.Log("Konにあたった！");
        }

        if(other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Playerにあたった！");
        }
    }
}
