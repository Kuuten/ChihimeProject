using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  “G‚Ì’eƒNƒ‰ƒX
//
//--------------------------------------------------------------
public class EnemyBullet : MonoBehaviour
{
    
    void Start()
    {
        
    }

    void Update()
    {
        //  ã•ûŒü‚É”­Ë‚³‚ê‚é
        transform.position += new Vector3(2,3,0) * Time.deltaTime;
    }
}
