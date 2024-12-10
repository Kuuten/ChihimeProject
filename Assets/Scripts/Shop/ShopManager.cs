using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

//--------------------------------------------------------------
//
//  �V���b�v�V�[���Ǘ��N���X(����������)
//
//--------------------------------------------------------------
public class ShopManager : MonoBehaviour
{
    enum eMenuList
    {
        RedHeart,           //  �Ԃ��n�[�g
        DoubleupHeart,      //  �_�u���A�b�v�n�[�g
        GoldHeart,          //  ���F�̃n�[�g
        Bomb,               //  ��G�{��

    }
    private readonly string failedText = "��������ւ�Ł`�B";  //  ��������Ȃ����̕�����

    bool canContorol;   //  ����\�t���O

    void Start()
    {
        StartCoroutine(StartInit());
    }

    //--------------------------------------------------------------
    //  �������R���[�`��
    //--------------------------------------------------------------
    IEnumerator StartInit()
    {
        //**************************************************************
        //  �����ŃV���b�v�̉�ʂ�\�����Ă���
        //**************************************************************

        //  ���t�F�[�h�C��
        //yield return StartCoroutine(WaitingForWhiteFadeIn());

        //  �V���b�vBGM�Đ�
        SoundManager.Instance.PlayBGM((int)MusicList.BGM_SHOP);

        //yield return new WaitForSeconds(1); //  1�b�҂�

        //  ����\�ɂ���
        canContorol = true;

        //  �ԃn�[�g��I����Ԃɐݒ�
        //EventSystem.current.SetSelectedGameObject(menuButtonObj[(int)eMenuList.RedHeart]);


        yield return null;
    }

    void Update()
    {
        //  ����s�\�Ȃ烊�^�[��
        if(!canContorol)return;
    }

    //--------------------------------------------------------------
    //  ���b�Z�[�W�E�B���h�E���o������
    //--------------------------------------------------------------
    IEnumerator DisplayMessage(string msg)
    {
        ////  ���b�Z�[�W�I�u�W�F�N�g��\��
        //messageObj.SetActive(true);

        yield return null;
    }

    //--------------------------------------------------------------
    //  �Ԃ��n�[�g�{�^���������ꂽ���̏���
    //--------------------------------------------------------------
    public void OnRedHeartButtonDown()
    {
        
    }

    //--------------------------------------------------------------
    //  �_�u���A�b�v�n�[�g�{�^���������ꂽ���̏���
    //--------------------------------------------------------------
    public void OnDoubleupHeartButtonDown()
    {
        
    }

    //--------------------------------------------------------------
    //  ���F�̃n�[�g�{�^���������ꂽ���̏���
    //--------------------------------------------------------------
    public void OnGoldHeartButtonDown()
    {
        
    }

    //--------------------------------------------------------------
    //  ��G�̃{���{�^���������ꂽ���̏���
    //--------------------------------------------------------------
    public void OnHoneGBombButtonDown()
    {
        
    }
}
