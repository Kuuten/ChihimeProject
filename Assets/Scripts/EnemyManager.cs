using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  �G�̊Ǘ��N���X
//
//--------------------------------------------------------------
public class EnemyManager : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;

    void Start()
    {
        //  Spawn�֐����A2�b���0.5�b���݂Ŏ��s����
        InvokeRepeating("Spawn", 2f, 0.5f); 
    }

    //  ��������
    void Spawn()
    {
        Vector3 spawnPosition = new Vector3(
            Random.Range(-8.5f,6.5f),
            transform.position.y,
            transform.position.z
            
            );

        Instantiate(
        enemyPrefab,        //  ��������I�u�W�F�N�g
        spawnPosition,      //  �����ʒu
        transform.rotation  //  �������̌���
        );
    }

    void Update()
    {
        
    }
}
