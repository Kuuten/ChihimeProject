using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//--------------------------------------------------------------
//
//  プレイヤーの移動管理クラス
//
//--------------------------------------------------------------
public class PlayerMovement : MonoBehaviour
{
    //  移動スピード
    [SerializeField] private float moveSpeed = 3.0f;

    void Start()
    {

    }

    void Update()
    {
        Move();
    }

    //-------------------------------------------
    //  移動処理
    //-------------------------------------------
    private void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        Vector3 moveVector = new Vector3(x, y, 0);
        moveVector.Normalize();
        transform.position += moveVector * moveSpeed * Time.deltaTime;
    }


}
