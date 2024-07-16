using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    //  オーディオソースリスト
    public enum AudioChannel
    {
        MUSIC,
        SFX,
        SFX_ENEMY,
        SFX_DAMAGE,
        SFX_SYSTEM,

        CHANNEL_MAX
    };

    //-----------------------------------------------
    //  BGM
    //-----------------------------------------------
    public enum MusicList
    {

        BGM_TITLE,
        BGM_TUTORIAL,
        BGM_DOUJI_STAGE,
        BGM_KURAMA_STAGE,
        BGM_KUCHINAWA_STAGE,
        BGM_TSUKUMO_STAGE,
        BGM_WADATSUMI_STAGE,
        BGM_HAKUMEN_STAGE,
        BGM_GAMEOVER,
        BGM_GAMECLEAR,

        CLIP_MAX
    };

    //-----------------------------------------------
    //  SFXクリップリスト
    //-----------------------------------------------
    public enum SFXList
    {
        //  タイトル
        SFX_HYOUSIGI,         //  拍子木の音
        SFX_SCROLL_MOVE,      //  巻物が動く時の音
        SFX_SCROLL_MOVE2,     //  巻物が動く時の音２（琴）
        SFX_DASH,             //  ダッシュ
        SFX_BRAKE,            //  ブレーキ
        SFX_TITLE_SELECT,     //  カーソル音
        SFX_TITLE_DECISION,   //  カーソル決定音

        //  イベント・チュートリアル

        //  ゲーム中・ショップ
        SFX_GAMESTART,
        SFX_NORMAL_SHOT,
        SFX_POWER_SHOT,
        SFX_WIDE_SHOT,
        SFX_PENETRATION_SHOT,
        SFX_HORMINGSHOT,

        //  ゲームクリア
        SFX_VICTORY,
        SFX_LOSE,
        SFX_GAMECLEAR01,
        SFX_GAMECLEAR02,

        CLIP_MAX
    };

//--------------------------------------------------------------
//
//  サウンド管理クラス
//
//--------------------------------------------------------------
public class SoundManager : MonoBehaviour
{

    // オーディオクリップリスト
    [SerializeField] private List<AudioClip> _MusicClipList;
    [SerializeField] private List<AudioClip> _SFXClipList;

    //  AudioSource
    private AudioSource[] _AudioSource;

    //  シングルトンなインスタンス
    public static SoundManager Instance
    {
        get; private set;
    }

    private void Awake()
    {
        //  AudioSource配列の初期化
        _AudioSource = new AudioSource[(int)AudioChannel.CHANNEL_MAX];

        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        //  コンポーネントの取得
        _AudioSource = this.GetComponents<AudioSource>();
    }

    //--------------------------------------------------------
    //  オーディオの再生
    //--------------------------------------------------------
    public void Play(int channelType, int clipId)
    {
#if UNITY_EDITOR
        Debug.Assert(
            channelType < (int)AudioChannel.CHANNEL_MAX && channelType >= 0,
            "AudioSourceIDが範囲外の値です！"
        );
        Debug.Assert(
            clipId < (int)MusicList.CLIP_MAX && clipId >= 0,
            "MusicClipIDが範囲外の値です！"
        );
        Debug.Assert(
            clipId < (int)SFXList.CLIP_MAX && clipId >= 0,
            "SFXClipIDが範囲外の値です！"
        );
#endif
        //  出力先とクリップを設定して再生
        if(channelType == (int)AudioChannel.MUSIC)
        {
            _AudioSource[channelType].clip = _MusicClipList[clipId];
        }
        else _AudioSource[channelType].clip = _SFXClipList[clipId];
        
        _AudioSource[channelType].Play();
    }

    //--------------------------------------------------------
    //  オーディオの停止
    //--------------------------------------------------------
    public void Stop(int channelType)
    {
        _AudioSource[channelType].Stop();
    }
}
