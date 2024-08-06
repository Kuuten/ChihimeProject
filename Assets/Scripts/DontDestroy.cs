using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    //  シングルトンなインスタンス
    public static DontDestroy Instance
    {
        get; private set;
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instantiate(this.gameObject);
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    void Update()
    {
        
    }
}
