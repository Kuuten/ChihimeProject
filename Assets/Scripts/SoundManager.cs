using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

    //  オーディオソースリスト
    public enum AudioChannel
    {
        MUSIC,
        SFX,
        SFX_ENEMY,
        ENEMY_SHOT,
        SFX_DAMAGE,
        SFX_SYSTEM,
        SFX_NORMAL_SHOT,
        SFX_CONVERT_SHOT,
        SFX_BOMB,
        SFX_LOOP,
        SFX_SYSTEM2,

        CHANNEL_MAX
    };

    //-----------------------------------------------
    //  BGM
    //-----------------------------------------------
    public enum MusicList
    {
        BGM_TITLE,
        BGM_INTRO,
        BGM_TUTORIAL,
        BGM_SELECTCONVERT,

        BGM_DOUJI_STAGE_ZAKO,
        BGM_DOUJI_STAGE_BOSS,
        BGM_TSUKUMO_STAGE_ZAKO,
        BGM_TSUKUMO_STAGE_BOSS,
        BGM_KURAMA_STAGE_ZAKO,
        BGM_KURAMA_STAGE_BOSS,
        BGM_KUCHINAWA_STAGE_ZAKO,
        BGM_KUCHINAWA_STAGE_BOSS,
        BGM_WADATSUMI_STAGE_ZAKO,
        BGM_WADATSUMI_STAGE_BOSS,
        BGM_HAKUMEN_STAGE_ZAKO,
        BGM_HAKUMEN_STAGE_BOSS,

        BGM_RESULT,
        BGM_SHOP,
        


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
        SFX_DASH,             //  ダッシュ
        SFX_VISUAL,           //  ヴィジュアル
        SFX_BRAKE,            //  ブレーキ
        SFX_TITLE_SELECT,     //  カーソル音
        SFX_TITLE_DECISION,   //  カーソル決定音
        SFX_TITLE_INCORRECT,  //  決定不可音

        //  イベント・チュートリアル

        //  ゲーム中・ショップ
        SFX_GAMESTART,

        SFX_NORMAL_SHOT,
        SFX_CONVERT_SHOT_GAUGE1,
        SFX_CONVERT_SHOT_GAUGE2,
        SFX_CONVERT_SHOT_GAUGE3,
        SFX_CONVERT_SHOT_FAIL,
        SFX_DOUJI_CONVERT_SHOT_MIDDLE,
        SFX_DOUJI_CONVERT_SHOT_FULL,
        SFX_TSUKUMO_CONVERT_SHOT_01,
        SFX_TSUKUMO_CONVERT_SHOT_02,
        SFX_TSUKUMO_CONVERT_SHOT_03,
        SFX_BOMB,
        SFX_KONBURST_CUTIN,
        SFX_KONBURST_DOUJI,
        SFX_KONBURST_TSUKUMO_TIMELIMIT,
        SFX_PLAYER_DAMAGE,
        SFX_PLAYER_DEATH,
        SFX_GET_KON,
        SFX_GET_HEART,
        SFX_GET_BOMB,
        SFX_GET_POWERUP,
        SFX_SHIELDBREAK,

        SFX_ENEMY_SHOT,
        SFX_ENEMY_DAMAGE,
        SFX_ENEMY_DEATH,
        SFX_MIDBOSS_PHASE2,
        SFX_MIDBOSS_JUMP,
        SFX_DOUJI_WARNING,
        SFX_DOUJI_KOONIRUSH,

        SFX_RESULT_COUNT,
        SFX_RESULT_CASH,
        SFX_RESULT_TEXTNEXT,
        SFX_RESULT_BOUND,
        SFX_BOSS_APPEAR,

        SFX_DOUJI_SHOT1,
        SFX_DOUJI_SHOT2,

        SFX_TSUKUMO_SHOT1,
        SFX_TSUKUMO_SHOT2,
        SFX_TSUKUMO_SHOT3,

        SFX_KUCHINAWA_SHOT,
        SFX_KURAMA_SHOT,
        SFX_WADATSUMI_SHOT,
        SFX_HAKUMEN_SHOT,

        SFX_SHOP_ENTER01,
        SFX_SHOP_ENTER02,
        SFX_SHOP_ENTER03,

        //  ゲームクリア
        SFX_STAGECLEAR,
        SFX_GAMEOVER,

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
    //  BGMの再生
    //--------------------------------------------------------
    public void PlayBGM(int clipId)
    {
#if UNITY_EDITOR
        Debug.Assert(
            clipId < (int)MusicList.CLIP_MAX && clipId >= 0,
            "MusicClipIDが範囲外の値です！"
        );

#endif
        //  BGMはループをON
        _AudioSource[(int)AudioChannel.MUSIC].loop = true;

        //  出力先とクリップを設定して再生
        _AudioSource[(int)AudioChannel.MUSIC].clip = _MusicClipList[clipId];
        
        _AudioSource[(int)AudioChannel.MUSIC].Play();
    }

    //--------------------------------------------------------
    //  SEの再生
    //--------------------------------------------------------
    public void PlaySFX(int channelType, int clipId)
    {
#if UNITY_EDITOR
        Debug.Assert(
            channelType > (int)AudioChannel.MUSIC || channelType < (int)AudioChannel.CHANNEL_MAX,
            "範囲外のchannelTypeを指定しているかBGMを指定しています！"
        );
        Debug.Assert(
            clipId < (int)SFXList.CLIP_MAX && clipId >= 0,
            "SFXClipIDが範囲外の値です！"
        );
#endif
        //  出力先とクリップを設定して再生       
        _AudioSource[channelType].PlayOneShot( _SFXClipList[clipId] );
    }

    //--------------------------------------------------------
    //  ループSEの再生
    //--------------------------------------------------------
    public void PlayLoopSFX(int channelType, int clipId)
    {
#if UNITY_EDITOR
        Debug.Assert(
            channelType > (int)AudioChannel.MUSIC || channelType < (int)AudioChannel.CHANNEL_MAX,
            "範囲外のchannelTypeを指定しているかBGMを指定しています！"
        );
        Debug.Assert(
            clipId < (int)SFXList.CLIP_MAX && clipId >= 0,
            "SFXClipIDが範囲外の値です！"
        );
#endif

        //  出力先とクリップを設定して再生       
        _AudioSource[channelType].clip = _SFXClipList[clipId];
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
