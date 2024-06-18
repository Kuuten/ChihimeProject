using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  “G‚ÌŠî–{ƒNƒ‰ƒX
//
//--------------------------------------------------------------
public class Enemy : MonoBehaviour
{
    //  “G‚ÌˆÚ“®F^‰º‚ÉˆÚ“®‚·‚é
    //  “G‚ğ¶¬F¶¬Hê‚ğì‚é
    //  “G‚É’e‚ª“–‚½‚Á‚½‚ç”š”­‚·‚é
    //  “G‚ÆPlayer‚ª‚Ô‚Â‚©‚Á‚½‚ç”š”­‚·‚é

    [SerializeField] float moveSpeed = 1.0f;

    void Start()
    {
        
    }

    void Update()
    {
        transform.position += new Vector3(0,moveSpeed * Time.deltaTime,0);
    }
}
