using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

//--------------------------------------------------------------
//
//  3D�{�^�����r���{�[�h������
//
//--------------------------------------------------------------
public class BillboardButton : MonoBehaviour
{
    [SerializeField] private Camera subCamera;

    // ���j���[�̃{�^���̎��
    public enum ButtonsType {
        Douji,
        Tsukumo,
        Kuchinawa,
        Kurama,
        Wadatsumi,
        Hakumen,

        Max
    }
    // �U�R�G�̎��
    public ButtonsType buttonType = ButtonsType.Douji;

    void Start()
    {
    }
    void Update()
    {
        //  �r���{�[�h��
        this.transform.rotation = Quaternion.Euler(new Vector3(0, -30, 0))
                                * subCamera.transform.rotation;
    }
}
