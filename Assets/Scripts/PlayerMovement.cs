using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//--------------------------------------------------------------
//
//  �v���C���[�̈ړ��Ǘ��N���X
//
//--------------------------------------------------------------
public class PlayerMovement : MonoBehaviour
{
    //  �ړ��X�s�[�h
    [SerializeField] private float moveSpeed = 3.0f;

    void Start()
    {

    }

    void Update()
    {
        Move();
    }

    //-------------------------------------------
    //  �ړ�����
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
