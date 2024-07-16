using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

//--------------------------------------------------------------
//
//  プレイヤーの移動管理クラス
//
//--------------------------------------------------------------
public class PlayerMovement : MonoBehaviour
{
    //  移動スピード
    [SerializeField] private float moveSpeed = 3.0f;

    //  移動制限用の壁
    [SerializeField] private GameObject wallLeft;
    [SerializeField] private GameObject wallRight;
    [SerializeField] private GameObject wallTop;
    [SerializeField] private GameObject wallBottom;

    //  入力
    private float horizontalInput, verticalInput;
    InputAction move;

    void Start()
    {
        // InputActionにMoveを設定
        PlayerInput playerInput = GetComponent<PlayerInput>();
        move = playerInput.actions["Move"];
    }

    void Update()
    {
        //Move();
        NewInputMove();

    }

    //-------------------------------------------
    //  移動処理
    //-------------------------------------------
    private void Move()
    {
        // float x = Input.GetAxisRaw("Horizontal");
        //float y = Input.GetAxisRaw("Vertical");
        //Vector3 moveVector = new Vector3(x, y, 0);
        //moveVector.Normalize();
        //transform.position += moveVector * moveSpeed * Time.deltaTime;

        //transform.position = new Vector3(
        //        Mathf.Clamp(
        //            transform.position.x,
        //            wallLeft.transform.position.x,
        //            wallRight.transform.position.x
        //        ),
        //        Mathf.Clamp(
        //            transform.position.y,
        //            wallBottom.transform.position.y,
        //            wallTop.transform.position.y
        //        ),
        //        transform.position.z
        //    );
    }

    private void NewInputMove()
    {
        Vector2 inputMoveAxis = move.ReadValue<Vector2>();
        horizontalInput = inputMoveAxis.x;
        verticalInput = inputMoveAxis.y;

        Vector3 moveVector = new Vector3(horizontalInput, verticalInput, 0);
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
