using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


//--------------------------------------------------------------
//
//  システム管理クラス
//
//--------------------------------------------------------------
public class SystemManager : MonoBehaviour
{
    //  シングルトンなインスタンス
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
                //  フルスクリーンの時はウィンドウモードに変更
                Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
            }
            else
            {
                //  ウィンドウモードの時はフルスクリーンに変更
                Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
            }
        }
    }
}
