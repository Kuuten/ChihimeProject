using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  クチナワのビームクラス
//
//--------------------------------------------------------------
public class KuchinawaPhase3Bullet : MonoBehaviour
{
    //  VFXなので威力のみ設定
    private int power;
    void Awake()
    {
        power = 1;  // 暫定  ※後でBossKuchinawaで設定すること！
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    //------------------------------------------------------
    //  プロパティ
    //------------------------------------------------------
    public int GetPower(){ return power; }
}
