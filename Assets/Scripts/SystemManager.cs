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

    //  InputAction
    private InputAction fullScreenAction;

    //  切り替え用フラグ
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
            //  フルスクリーンを維持
            Screen.SetResolution( Screen.width, Screen.height, FullScreenMode.FullScreenWindow);

        }
        else
        {
            //  ウィンドウモードを維持
            Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
        }

        //  Alt + Enterを押下
        if (fullScreenAction.WasPressedThisFrame())
        {
            bSwitch = !bSwitch;
            Screen.fullScreen = bSwitch;
            Debug.Log("フルスクリーン状態:" + bSwitch);
        }
    }
}
