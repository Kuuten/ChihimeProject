using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  �V���b�v�V�[���̃v���C���[�X�e�[�^�X�N���X
//
//--------------------------------------------------------------
//  ���ȊO�̗̑́E�V���b�g�З́E�X�s�[�h�E�{���̐���\������
//  �̗͂̂ݍX�V����

public class PlayerStatus : MonoBehaviour
{
    //  �V���b�g�З̓A�C�R���̐����ʒu
    [SerializeField] private GameObject power_root;
    //  �V���b�g�З̓A�C�R��
    [SerializeField] private GameObject power_icon;

    //  �X�s�[�h�A�C�R���̐����ʒu
    [SerializeField] private GameObject speed_root;
    //   �X�s�[�h�A�C�R��
    [SerializeField] private GameObject speed_icon;

    //  �{���A�C�R���̐����ʒu
    [SerializeField] private GameObject bomb_root;
    //   �{���A�C�R��
    [SerializeField] private GameObject bomb_icon;

    //  �n�[�g�A�C�R���̐����ʒu
    [SerializeField] private GameObject heart_root;
    //   �n�[�g�A�C�R��
    [SerializeField] private GameObject heart_icon;
    //  �n�[�g�t���[���I�u�W�F�N�g�̃��X�g
    private List<GameObject> heartList = new List<GameObject>();
    //  �n�[�g�̃^�C�v
    enum HeartType
    {
        Half,   //  ����
        Full,   //  �n�[�g�P��

        Max
    }

    void Start()
    {
        //  �V���b�g�З�
        for(int i=0; i<PlayerInfoManager.g_SHOT_LV;i++)
        {
            GameObject power = Instantiate(power_icon);
            power.transform.parent = power_root.transform;
        }

        //  �X�s�[�h
        for(int i=0; i<PlayerInfoManager.g_SPEED_LV;i++)
        {
            GameObject speed = Instantiate(speed_icon);
            speed.transform.parent = speed_root.transform;
        }

        //  �{��
        for(int i=0; i<PlayerInfoManager.g_BOMBNUM;i++)
        {
            GameObject bomb = Instantiate(bomb_icon);
            bomb.transform.parent = bomb_root.transform;
        }

        //  �n�[�g
        for(int i=0; i<PlayerInfoManager.g_CURRENTHP/2;i++)
        {
            GameObject heart = Instantiate(heart_icon);
            heart.transform.parent = heart_root.transform;

            heart.transform.GetChild((int)HeartType.Half).gameObject.SetActive(true);
            heart.transform.GetChild((int)HeartType.Full).gameObject.SetActive(true);


            heartList.Add( heart );   //  ���X�g�ɒǉ�
        }

    }

    void Update()
    {
        //  �n�[�g�摜���X�V
        CalculateHealthUI(PlayerInfoManager.g_CURRENTHP);
    }

    //---------------------------------------------------
    //  ���ݑ̗͂��󂯎���đ̗�UI���v�Z����
    //---------------------------------------------------
    private void CalculateHealthUI(int health)
    {
        if(health < 0)
        {
            health = 0;
        }

            //  �̗�0�Ȃ�n�[�g��S����\���ɂ���
            if (health == 0)
            {
                for(int i=0;i<heartList.Count;i++)
                {
                    heartList[i].transform.GetChild((int)HeartType.Half)
                        .gameObject.SetActive(false);
                    heartList[i].transform.GetChild((int)HeartType.Full)
                        .gameObject.SetActive(false);
                }
            }
            else if(health == 1)
            {
                for(int i=0;i<heartList.Count;i++)
                {
                    if(i==0)
                    {
                        heartList[i].transform.GetChild((int)HeartType.Half)
                            .gameObject.SetActive(true);
                        heartList[i].transform.GetChild((int)HeartType.Full)
                            .gameObject.SetActive(false);
                    }

                    //  �c����\���ɂ���
                    for(int j=1;j<heartList.Count;j++)
                    {
                        heartList[j].transform.GetChild((int)HeartType.Half)
                            .gameObject.SetActive(false);
                        heartList[j].transform.GetChild((int)HeartType.Full)
                            .gameObject.SetActive(false);
                    }

                } 
            }
            else // �̗͂��Q�ȏ�̎�
            {
                //  ��U���ݑ̗͂̂Ƃ��܂őS���t���Ŗ��߂�
                int fullNum = health / 2;
                for(int i=0;i<fullNum;i++)
                {
                    heartList[i].transform.GetChild((int)HeartType.Half)
                        .gameObject.SetActive(true);
                    heartList[i].transform.GetChild((int)HeartType.Full)
                        .gameObject.SetActive(true);
                }

                //  ��������ꍇ�͍Ō�̔ԍ������n�[�t�ɂ���
                int taegetNum = health - fullNum;
                if(health % 2 != 0)
                {
                    heartList[taegetNum-1].transform.GetChild((int)HeartType.Half)
                        .gameObject.SetActive(true);
                    heartList[taegetNum-1].transform.GetChild((int)HeartType.Full)
                        .gameObject.SetActive(false);
                }

                //  �c����\���ɂ���
                for(int i=taegetNum;i<heartList.Count;i++)
                {
                    heartList[i].transform.GetChild((int)HeartType.Half)
                        .gameObject.SetActive(false);
                    heartList[i].transform.GetChild((int)HeartType.Full)
                        .gameObject.SetActive(false);
                }
            }
    }
}
