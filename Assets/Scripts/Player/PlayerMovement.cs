using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

//  スピードのレベル
enum eSpeedLevel
{
    Lv1,
    Lv2,
    Lv3,

    Max
};

//--------------------------------------------------------------
//
//  プレイヤーの移動管理クラス
//
//--------------------------------------------------------------
public class PlayerMovement : MonoBehaviour
{
    //  移動スピード
    private float[] moveSpeed = new float[(int)eSpeedLevel.Max];

    //  移動スピードの設定値
    private const float moveSpeedLv1 = 7.0f;
    private const float moveSpeedLv2 = 8.0f;
    private const float moveSpeedLv3 = 9.0f;

    //  スピードレベル
    private int speedLevel;
    //  スピードレベル表示用画像のプレハブ
    [SerializeField] private GameObject speedLevelIcon;
    //  スピードレベルアイコンのリスト
    private List<GameObject> speedLevelIconList = new List<GameObject>();
    //  スピードレベルアイコンの親オブジェクトの位置取得用
    [SerializeField] private GameObject speedLevelIconRootObj;

    //  Animatorの再生速度の設定値
    private const float AnimSpeedLv1 = 0.7f;
    private const float AnimSpeedLv2 = 0.8f;
    private const float AnimSpeedLv3 = 0.9f;

    //  移動制限用の壁
    [SerializeField] private GameObject wallLeft;
    [SerializeField] private GameObject wallRight;
    [SerializeField] private GameObject wallTop;
    [SerializeField] private GameObject wallBottom;

    //  入力
    private float horizontalInput, verticalInput;
    InputAction move;

    bool bCanMove;

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

        //  親オブジェクトの子オブジェクトとしてスピードレベルアイコンを生成
        for( int i=0; i<(int)eSpeedLevel.Max;i++ )
        {
            GameObject obj = Instantiate(speedLevelIcon);
            obj.GetComponent<RectTransform>().SetParent( speedLevelIconRootObj.transform);
            obj.GetComponent<RectTransform>().localScale = Vector3.one;
            obj.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0,0,0);

            speedLevelIconList.Add( obj );   //  リストに追加
        }

        bCanMove = true;
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

        //  スピードレベルアイコンを更新
        UpdateSpeedLevelIcon();

        //  移動
        //Move();
        NewInputMove();

    }

    //------------------------------------------------
    //  スピードレベルアイコンの数を更新する
    //------------------------------------------------
    private void UpdateSpeedLevelIcon()
    {
        if(speedLevel < 0)Debug.LogError("normalShotLevelにマイナスの値が入っています！");

        //  1回全部表示
        for(int i=0;i<speedLevelIconList.Count;i++)
        {
            speedLevelIconList[i].gameObject.SetActive(true);
        }

        //  非表示処理
        for(int i=speedLevelIconList.Count-1;i>speedLevel;i--)
        {
            speedLevelIconList[i].gameObject.SetActive(false);
        }
    }

    //-------------------------------------------
    //  プロパティ
    //-------------------------------------------
    public int GetSpeedLevel(){ return speedLevel; }
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

        // Raycast
        Debug.DrawRay(transform.position + moveVector * 0.2f, moveVector, Color.red);
        var hit = Physics2D.Raycast(transform.position + moveVector * 0.2f, moveVector, 1.0f);
        if (hit.collider.CompareTag("Wall"))
        {
            bCanMove = false;
            return;
        }
        else
        {
            bCanMove = true;
        }

        


        if(bCanMove)transform.position += moveVector * moveSpeed[speedLevel] * Time.deltaTime;

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
