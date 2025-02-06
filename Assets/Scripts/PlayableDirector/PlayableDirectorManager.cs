using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

//--------------------------------------------------------------
//
//  PlayableDirectorの管理クラス
//
//--------------------------------------------------------------
public class PlayableDirectorManager : MonoBehaviour
{
    //  シングルトンなインスタンス
    public static PlayableDirectorManager Instance
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
        Instance = this;
    }

    void Start()
    {

    }

    void Update()
    {

    }

    public PlayableDirector GetDirector() { return this.gameObject.GetComponent<PlayableDirector>(); }
}
