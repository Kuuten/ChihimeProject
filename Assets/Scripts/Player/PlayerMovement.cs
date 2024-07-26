using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

//  スピードのレベル
enum eSpeedLevel
{
    Lv1 = 1,
    Lv2,
    Lv3,

    LvMax
};

//--------------------------------------------------------------
//
//  プレイヤーの移動管理クラス
//
//--------------------------------------------------------------
public class PlayerMovement : MonoBehaviour
{
    //  移動スピード
    private float[] moveSpeed = new float[(int)eSpeedLevel.LvMax];

    //  移動スピードの設定値
    private const float moveSpeedLv1 = 5.0f;
    private const float moveSpeedLv2 = 6.0f;
    private const float moveSpeedLv3 = 7.0f;

    //  スピードレベル
    private int speedLevel;

    //  Animatorの再生速度の設定値
    private const float AnimSpeedLv1 = 0.5f;
    private const float AnimSpeedLv2 = 0.6f;
    private const float AnimSpeedLv3 = 0.7f;

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

        //  レベル毎のスピードを設定
        moveSpeed[(int)eSpeedLevel.Lv1] = moveSpeedLv1;
        moveSpeed[(int)eSpeedLevel.Lv2] = moveSpeedLv2;
        moveSpeed[(int)eSpeedLevel.Lv3] = moveSpeedLv3;

        //  最初はLv.1
        speedLevel = (int)eSpeedLevel.Lv1;
    }

    void Update()
    {
        //  スピードレベル毎の処理
        switch(speedLevel)
        {
            case (int)eSpeedLevel.Lv1:
                Level1();
                break;
            case (int)eSpeedLevel.Lv2:
                Level2();
                break;
            case (int)eSpeedLevel.Lv3:
                Level3();
                break;
        }

        //  移動
        //Move();
        NewInputMove();

    }

    //-------------------------------------------
    //  プロパティ
    //-------------------------------------------
    public float GetSpeedLevel(){ return speedLevel; }
    public void SetSpeedLevel(int level)
    {
        //  範囲外なら
        if(level < (int)eSpeedLevel.Lv1 ||
            level > (int)eSpeedLevel.Lv3)
        {
            Debug.LogError("スピードレベルに範囲外の数値が入っています！");
            return;
        }
        if(level != speedLevel)speedLevel = level;
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
        transform.position += moveVector * moveSpeed[speedLevel] * Time.deltaTime;

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

    //-------------------------------------------
    //  スピードレベル毎の処理
    //-------------------------------------------
    void Level1()
    {
        //  移動スピードを設定
        moveSpeed[(int)eSpeedLevel.Lv1] = moveSpeedLv1;

        //  Animatorの再生スピードを設定
        this.GetComponent<Animator>().speed = AnimSpeedLv1;
    }

    void Level2()
    {
        //  移動スピードを設定
        moveSpeed[(int)eSpeedLevel.Lv2] = moveSpeedLv2;

        //  Animatorの再生スピードを設定
        this.GetComponent<Animator>().speed = AnimSpeedLv2;
    }

    void Level3()
    {
        //  移動スピードを設定
        moveSpeed[(int)eSpeedLevel.Lv3] = moveSpeedLv3;

        //  Animatorの再生スピードを設定
        this.GetComponent<Animator>().speed = AnimSpeedLv3;
    }

    //---------------------------------------------------
    //  移動速度のレベルアップ
    //---------------------------------------------------
    public void LevelupMoveSpeed()
    {
        if(speedLevel < (int)eSpeedLevel.Lv3)speedLevel++;
    }

    //---------------------------------------------------
    //  移動速度のレベルダウン
    //---------------------------------------------------
    public void LeveldownMoveSpeed()
    {
        if(speedLevel > (int)eSpeedLevel.Lv1)speedLevel--;
    }

}
