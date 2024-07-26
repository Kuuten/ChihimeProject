using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  プレイヤーの死亡エフェクト
//
//--------------------------------------------------------------
public class EffectPlayerDeath : MonoBehaviour
{
    private const float effectPeriod = 0.583f;
    private const float playSpeed = 2.0f;

    void Start()
    {
        Destroy(this.gameObject, effectPeriod/playSpeed); 
    }

    void Update()
    {
        
    }
}
