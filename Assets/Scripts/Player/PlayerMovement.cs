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

    //  �ړ������p�̕�
    [SerializeField] private GameObject wallLeft;
    [SerializeField] private GameObject wallRight;
    [SerializeField] private GameObject wallTop;
    [SerializeField] private GameObject wallBottom;

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
         float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector3 moveVector = new Vector3(x, y, 0);
        moveVector.Normalize();
        transform.position += moveVector * moveSpeed * Time.deltaTime;

        transform.position = new Vector3(
                Mathf.Clamp(
                    transform.position.x,
                    wallLeft.transform.position.x,
                    wallRight.transform.position.x
                ),
                Mathf.Clamp(
                    transform.position.y,
                    wallBottom.transform.position.y,
                    wallTop.transform.position.y
                ),
                transform.position.z
            );
    }
}
