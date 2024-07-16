using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  敵のスポナーの管理クラス
//
//--------------------------------------------------------------
public class SpawnerManager : MonoBehaviour
{
    [SerializeField] private GameObject[] spawner;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    //  n番目のスポナーの座標を他のクラスに渡す
    public Vector3 GetSpawnerPos(int n)
    {
        return spawner[n].transform.position;
    }
}
