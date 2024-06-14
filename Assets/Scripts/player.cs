using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class player : MonoBehaviour
{
    // Player������L�[�i�\���L�[�j�œ�����
    //  �E�����L�[�̓��͂��󂯎��
    //  �EPlayer�̈ʒu��ύX����

    //  �e������
    //  �E�e�����
    //  �E�e�̓��������
    //  �E���˃|�C���g�����

    //  �G�̈ړ�
    //  �G�𐶐�
    //  �G�ɒe�����������甚������
    //  �G�ƃv���C���[���Ԃ������甚������

    //  �ړ��X�s�[�h
    [SerializeField] private float moveSpeed = 3.0f;

    //  �e�̔��˓_
    [SerializeField]private Transform firePoint;
    //  �e�̃v���n�u
    [SerializeField]private GameObject bulletPrefab;
    //  �V���b�g�Ԋu�̃J�E���g�p
    private int shotCount = 0;
    //  �e�����t���[�����Ɍ��Ă邩
    private const int shotInterval = 10;

    void Start()
    {
        shotCount = 0;
    }

    void Update()
    {
        Move();

        Shot();
    }

    //-------------------------------------------
    //  �ړ�����
    //-------------------------------------------
    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        Vector3 moveVector = new Vector3(x, y, 0);
        moveVector.Normalize();
        transform.position += moveVector * moveSpeed * Time.deltaTime;
    }

    //-------------------------------------------
    //  �ړ�����
    //-------------------------------------------
    void Shot()
    {
        if( Input.GetKey(KeyCode.Z))    //  Z�L�[����
        {
            shotCount++;
            if(shotCount % shotInterval == 0)
            {
                Instantiate( bulletPrefab, firePoint.position, bulletPrefab.transform.rotation);
            }
        }
    }
}
