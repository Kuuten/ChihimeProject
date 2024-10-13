using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static UnityEngine.RuleTile.TilingRuleOutput;

//--------------------------------------------------------------
//
// 3Dメニューの回転クラス
//
//--------------------------------------------------------------
public class Rotate3DMenu : MonoBehaviour
{
    bool bComplete;            //  回転完了フラグ
    Quaternion preRotation;    //  前回の姿勢
    float time;
    const float default_time = 0.5f;
    bool bClockwise;

    //  シングルトンなインスタンス
    public static Rotate3DMenu Instance
    {
        get; private set;
    }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        bComplete = true;
        bClockwise = true;
        preRotation = Quaternion.identity;
    }

    void Update()
    {

        //  仮処理-------------------------------
        var current = Keyboard.current;
        var enter = current.enterKey;

        if(enter.wasPressedThisFrame)
        {
            
        }
        //---------------------------------------

        //  回転が未完了の時
        if(!bComplete)
        {
            if(Timer()) //  タイマーが完了したなら
            {
                bComplete = true;   //  完了

                //  preRotationからX軸で60度回転させた回転に補正
                if(bClockwise)
                {
                    Quaternion rot = Quaternion.AngleAxis(-60, Vector3.right);
                    Quaternion q = preRotation;
                    this.transform.rotation = q * rot;
                }
                else
                {
                    Quaternion rot = Quaternion.AngleAxis(60, Vector3.right);
                    Quaternion q = preRotation;
                    this.transform.rotation = q * rot;
                }

                //  今回の回転を保存
                preRotation = transform.rotation;
            }
            else
            {
                CompleteRotation(); //  回転させる
            }
        }


        
    }

    //-----------------------------------------------------------------
    //  プロパティ
    //-----------------------------------------------------------------
    public bool GetComplete(){ return bComplete; }
    public void SetComplete(bool flag){ bComplete = flag; }
    public bool GeClockwise(){ return bClockwise; }
    public void SetClockwise(bool flag){ bClockwise = flag; }


    //  指定した魂バートが正面に来るように回転してその完了を知らせる
    public void CompleteRotation()
    {
        bComplete = false;
        
        float degree = 60;
        float duration = default_time;

        Quaternion result = Quaternion.identity;

        if(bClockwise)
        {
            Quaternion rot = Quaternion.AngleAxis(-degree, Vector3.right);
            Quaternion q = this.transform.rotation;
            result = q * rot;
        }
        else
        {
            Quaternion rot = Quaternion.AngleAxis(degree, Vector3.right);
            Quaternion q = this.transform.rotation;
            result = q * rot;
        }

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            result,
            (degree/duration)*Time.deltaTime);
    }

    //  タイマー
    private bool Timer()
    {
        if(time <= 0.0f)
        {
            time = 0.0f;
            return true;
        }
        else time -= Time.deltaTime;

        return false;
    }

    //  メニューを回転させるトリガー
    public void TurnMenu(bool crockwise)
    {
        time = default_time;
        bComplete = false;
        bClockwise = crockwise;
    }
}
