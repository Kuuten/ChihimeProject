using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

//--------------------------------------------------------------
//
//  3Dボタンをビルボード化する
//
//--------------------------------------------------------------
public class BillboardButton : MonoBehaviour
{
    [SerializeField] private Camera subCamera;

    // メニューのボタンの種類
    public enum ButtonsType {
        Douji,
        Tsukumo,
        Kuchinawa,
        Kurama,
        Wadatsumi,
        Hakumen,

        Max
    }
    // ザコ敵の種類
    public ButtonsType buttonType = ButtonsType.Douji;

    void Start()
    {
    }
    void Update()
    {
        //  ビルボード化
        this.transform.rotation = Quaternion.Euler(new Vector3(0, -30, 0))
                                * subCamera.transform.rotation;
    }
}
