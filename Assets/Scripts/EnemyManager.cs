using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  敵の管理クラス
//
//--------------------------------------------------------------
public class EnemyManager : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;

    void Start()
    {
        //  Spawn関数を、2秒後に0.5秒刻みで実行する
        InvokeRepeating("Spawn", 2f, 0.5f); 
    }

    //  生成する
    void Spawn()
    {
        Vector3 spawnPosition = new Vector3(
            Random.Range(-8.5f,6.5f),
            transform.position.y,
            transform.position.z
            
            );

        Instantiate(
        enemyPrefab,        //  生成するオブジェクト
        spawnPosition,      //  生成位置
        transform.rotation  //  生成時の向き
        );
    }

    void Update()
    {
        
    }
}
