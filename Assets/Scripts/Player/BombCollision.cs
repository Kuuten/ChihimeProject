using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  ƒvƒŒƒCƒ„[‚Ìƒ{ƒ€‚Ì“–‚½‚è”»’èƒNƒ‰ƒX
//
//--------------------------------------------------------------
public class BombCollision : MonoBehaviour
{
    

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    //-------------------------------------------------------
    //  “G‚Ì’e‚Æ‚Ì“–‚½‚è”»’è
    //-------------------------------------------------------
    private void OnTriggerEnter2D(Collider2D collision)
    {   
        if(collision.CompareTag("EnemyBullet"))
        {
            //  “G’e‚Æ“–‚½‚Á‚½‚ç“G’e‚ğÁ‚·
            Destroy(collision.gameObject);
        }
    }
}
