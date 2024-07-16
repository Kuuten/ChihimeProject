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

    private InputAction fullScreen;

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
        fullScreen = systemInput.actions["FullScreen"];;
    }

    void Update()
    {
        if (fullScreen.WasPressedThisFrame())
        {
            if( Screen.fullScreen )
            {
                //  �t���X�N���[���̎��̓E�B���h�E���[�h�ɕύX
                Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
            }
            else
            {
                //  �E�B���h�E���[�h�̎��̓t���X�N���[���ɕύX
                Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
            }
        }
    }
}
