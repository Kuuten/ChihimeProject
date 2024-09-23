using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

//  �X�s�[�h�̃��x��
enum eSpeedLevel
{
    Lv1,
    Lv2,
    Lv3,

    Max
};

//--------------------------------------------------------------
//
//  �v���C���[�̈ړ��Ǘ��N���X
//
//--------------------------------------------------------------
public class PlayerMovement : MonoBehaviour
{
    //  �ړ��X�s�[�h
    private float[] moveSpeed = new float[(int)eSpeedLevel.Max];

    //  �ړ��X�s�[�h�̐ݒ�l
    private const float moveSpeedLv1 = 7.0f;
    private const float moveSpeedLv2 = 8.0f;
    private const float moveSpeedLv3 = 9.0f;

    //  �X�s�[�h���x��
    private int speedLevel;
    //  �X�s�[�h���x���\���p�摜�̃v���n�u
    [SerializeField] private GameObject speedLevelIcon;
    //  �X�s�[�h���x���A�C�R���̃��X�g
    private List<GameObject> speedLevelIconList = new List<GameObject>();
    //  �X�s�[�h���x���A�C�R���̐e�I�u�W�F�N�g�̈ʒu�擾�p
    [SerializeField] private GameObject speedLevelIconRootObj;

    //  Animator�̍Đ����x�̐ݒ�l
    private const float AnimSpeedLv1 = 0.7f;
    private const float AnimSpeedLv2 = 0.8f;
    private const float AnimSpeedLv3 = 0.9f;

    //  �ړ������p�̕�
    [SerializeField] private GameObject wallLeft;
    [SerializeField] private GameObject wallRight;
    [SerializeField] private GameObject wallTop;
    [SerializeField] private GameObject wallBottom;

    //  ����
    private float horizontalInput, verticalInput;
    InputAction move;

    bool bCanMove;

    void Start()
    {
        // InputAction��Move��ݒ�
        PlayerInput playerInput = GetComponent<PlayerInput>();
        move = playerInput.actions["Move"];

        //  ���x�����̃X�s�[�h��ݒ�
        moveSpeed[(int)eSpeedLevel.Lv1] = moveSpeedLv1;
        moveSpeed[(int)eSpeedLevel.Lv2] = moveSpeedLv2;
        moveSpeed[(int)eSpeedLevel.Lv3] = moveSpeedLv3;

        //  �ŏ���Lv.1
        speedLevel = (int)eSpeedLevel.Lv1;

        //  �e�I�u�W�F�N�g�̎q�I�u�W�F�N�g�Ƃ��ăX�s�[�h���x���A�C�R���𐶐�
        for( int i=0; i<(int)eSpeedLevel.Max;i++ )
        {
            GameObject obj = Instantiate(speedLevelIcon);
            obj.GetComponent<RectTransform>().SetParent( speedLevelIconRootObj.transform);
            obj.GetComponent<RectTransform>().localScale = Vector3.one;
            obj.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0,0,0);

            speedLevelIconList.Add( obj );   //  ���X�g�ɒǉ�
        }

        bCanMove = true;
    }

    void Update()
    {
        //  �X�s�[�h���x�����̏���
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

        //  �X�s�[�h���x���A�C�R�����X�V
        UpdateSpeedLevelIcon();

        //  �ړ�
        //Move();
        NewInputMove();

    }

    //------------------------------------------------
    //  �X�s�[�h���x���A�C�R���̐����X�V����
    //------------------------------------------------
    private void UpdateSpeedLevelIcon()
    {
        if(speedLevel < 0)Debug.LogError("normalShotLevel�Ƀ}�C�i�X�̒l�������Ă��܂��I");

        //  1��S���\��
        for(int i=0;i<speedLevelIconList.Count;i++)
        {
            speedLevelIconList[i].gameObject.SetActive(true);
        }

        //  ��\������
        for(int i=speedLevelIconList.Count-1;i>speedLevel;i--)
        {
            speedLevelIconList[i].gameObject.SetActive(false);
        }
    }

    //-------------------------------------------
    //  �v���p�e�B
    //-------------------------------------------
    public int GetSpeedLevel(){ return speedLevel; }
    public void SetSpeedLevel(int level)
    {
        //  �͈͊O�Ȃ�
        if(level < (int)eSpeedLevel.Lv1 ||
            level > (int)eSpeedLevel.Lv3)
        {
            Debug.LogError("�X�s�[�h���x���ɔ͈͊O�̐��l�������Ă��܂��I");
            return;
        }
        if(level != speedLevel)speedLevel = level;
    }

    //-------------------------------------------
    //  �ړ�����
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
    //  �X�s�[�h���x�����̏���
    //-------------------------------------------
    void Level1()
    {
        //  �ړ��X�s�[�h��ݒ�
        moveSpeed[(int)eSpeedLevel.Lv1] = moveSpeedLv1;

        //  Animator�̍Đ��X�s�[�h��ݒ�
        this.GetComponent<Animator>().speed = AnimSpeedLv1;
    }

    void Level2()
    {
        //  �ړ��X�s�[�h��ݒ�
        moveSpeed[(int)eSpeedLevel.Lv2] = moveSpeedLv2;

        //  Animator�̍Đ��X�s�[�h��ݒ�
        this.GetComponent<Animator>().speed = AnimSpeedLv2;
    }

    void Level3()
    {
        //  �ړ��X�s�[�h��ݒ�
        moveSpeed[(int)eSpeedLevel.Lv3] = moveSpeedLv3;

        //  Animator�̍Đ��X�s�[�h��ݒ�
        this.GetComponent<Animator>().speed = AnimSpeedLv3;
    }

    //---------------------------------------------------
    //  �ړ����x�̃��x���A�b�v
    //---------------------------------------------------
    public void LevelupMoveSpeed()
    {
        if(speedLevel < (int)eSpeedLevel.Lv3)speedLevel++;
    }

    //---------------------------------------------------
    //  �ړ����x�̃��x���_�E��
    //---------------------------------------------------
    public void LeveldownMoveSpeed()
    {
        if(speedLevel > (int)eSpeedLevel.Lv1)speedLevel--;
    }

}
