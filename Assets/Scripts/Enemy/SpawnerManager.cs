using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  �G�̃X�|�i�[�̊Ǘ��N���X
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

    //  n�Ԗڂ̃X�|�i�[�̍��W�𑼂̃N���X�ɓn��
    public Vector3 GetSpawnerPos(int n)
    {
        return spawner[n].transform.position;
    }
}
