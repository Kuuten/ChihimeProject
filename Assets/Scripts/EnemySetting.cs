using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  �G�̃f�[�^�N���X
//
//--------------------------------------------------------------
[Serializable]
public class EnemyData
{
    public string Id;
    public int Hp;
    public int Attack;
    public int Money;
}


[CreateAssetMenu(menuName = "ScriptableObject/Enemy Setting", fileName = "EnemySetting")]
public class EnemySetting : ScriptableObject
{
    public List<EnemyData> DataList;

}
