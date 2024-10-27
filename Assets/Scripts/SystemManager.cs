using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


//--------------------------------------------------------------
//
//  �V�X�e���Ǘ��N���X
//
//--------------------------------------------------------------
public class SystemManager : MonoBehaviour
{
    //  �V���O���g���ȃC���X�^���X
    public static SystemManager Instance
    {
        get; private set;
    }

    //  InputAction
    private InputAction fullScreenAction;

    //  �؂�ւ��p�t���O
    private bool bSwitch; 

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        PlayerInput systemInput = this.GetComponent<PlayerInput>();
        fullScreenAction = systemInput.actions["FullScreen"];
        
        bSwitch = false;
    }

    void Update()
    {
        if (Screen.fullScreen)
        {
            //  �t���X�N���[�����ێ�
            Screen.SetResolution( Screen.width, Screen.height, FullScreenMode.FullScreenWindow);

        }
        else
        {
            //  �E�B���h�E���[�h���ێ�
            Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
        }

        //  Alt + Enter������
        if (fullScreenAction.WasPressedThisFrame())
        {
            bSwitch = !bSwitch;
            Screen.fullScreen = bSwitch;
            Debug.Log("�t���X�N���[�����:" + bSwitch);
        }
    }
}
