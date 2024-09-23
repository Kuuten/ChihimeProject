using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

    //  �I�[�f�B�I�\�[�X���X�g
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

        CHANNEL_MAX
    };

    //-----------------------------------------------
    //  BGM
    //-----------------------------------------------
    public enum MusicList
    {

        BGM_TITLE,
        BGM_TUTORIAL,
        BGM_DOUJI_STAGE_ZAKO,
        BGM_DOUJI_STAGE_BOSS,
        BGM_KURAMA_STAGE_ZAKO,
        BGM_KURAMA_STAGE_BOSS,
        BGM_KUCHINAWA_STAGE_ZAKO,
        BGM_KUCHINAWA_STAGE_BOSS,
        BGM_TSUKUMO_STAGE_ZAKO,
        BGM_TSUKUMO_STAGE_BOSS,
        BGM_WADATSUMI_STAGE_ZAKO,
        BGM_WADATSUMI_STAGE_BOSS,
        BGM_HAKUMEN_STAGE_ZAKO,
        BGM_HAKUMEN_STAGE_BOSS,


        BGM_GAMEOVER,
        BGM_GAMECLEAR,

        CLIP_MAX
    };

    //-----------------------------------------------
    //  SFX�N���b�v���X�g
    //-----------------------------------------------
    public enum SFXList
    {
        //  �^�C�g��
        SFX_HYOUSIGI,         //  ���q�؂̉�
        SFX_DASH,             //  �_�b�V��
        SFX_BRAKE,            //  �u���[�L
        SFX_TITLE_SELECT,     //  �J�[�\����
        SFX_TITLE_DECISION,   //  �J�[�\�����艹

        //  �C�x���g�E�`���[�g���A��

        //  �Q�[�����E�V���b�v
        SFX_GAMESTART,
        SFX_NORMAL_SHOT,
        SFX_CONVERT_SHOT_GAUGE1,
        SFX_CONVERT_SHOT_GAUGE2,
        SFX_CONVERT_SHOT_FAIL,
        SFX_DOUJI_CONVERT_SHOT_MIDDLE,
        SFX_DOUJI_CONVERT_SHOT_FULL,
        SFX_KONBURST_CUTIN,
        SFX_KONBURST_DOUJI,
        SFX_PLAYER_DAMAGE,
        SFX_PLAYER_DEATH,
        SFX_ENEMY_SHOT,
        SFX_ENEMY_DAMAGE,
        SFX_MIDBOSS_PHASE2,
        SFX_MIDBOSS_JUMP,
        SFX_DOUJI_WARNING,
        SFX_DOUJI_KOONIRUSH,
        SFX_RESULT_COUNT,
        SFX_RESULT_CASH,
        SFX_RESULT_TEXTNEXT,
        SFX_RESULT_BOUND,

        SFX_DOUJI_SHOT1,
        SFX_DOUJI_SHOT2,
        SFX_TSUKUMO_SHOT,
        SFX_KUCHINAWA_SHOT,
        SFX_KURAMA_SHOT,
        SFX_WADATSUMI_SHOT,
        SFX_HAKUMEN_SHOT,

        //  �Q�[���N���A
        SFX_STAGECLEAR,
        SFX_GAMEOVER,

        CLIP_MAX
    };

//--------------------------------------------------------------
//
//  �T�E���h�Ǘ��N���X
//
//--------------------------------------------------------------
public class SoundManager : MonoBehaviour
{
    // �I�[�f�B�I�N���b�v���X�g
    [SerializeField] private List<AudioClip> _MusicClipList;
    [SerializeField] private List<AudioClip> _SFXClipList;

    //  AudioSource
    private AudioSource[] _AudioSource;

    //  �V���O���g���ȃC���X�^���X
    public static SoundManager Instance
    {
        get; private set;
    }

    private void Awake()
    {
        //  AudioSource�z��̏�����
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
        //  �R���|�[�l���g�̎擾
        _AudioSource = this.GetComponents<AudioSource>();
    }

    //--------------------------------------------------------
    //  BGM�̍Đ�
    //--------------------------------------------------------
    public void PlayBGM(int clipId)
    {
#if UNITY_EDITOR
        Debug.Assert(
            clipId < (int)MusicList.CLIP_MAX && clipId >= 0,
            "MusicClipID���͈͊O�̒l�ł��I"
        );

#endif
        //  �o�͐�ƃN���b�v��ݒ肵�čĐ�
        _AudioSource[(int)AudioChannel.MUSIC].clip = _MusicClipList[clipId];
        
        _AudioSource[(int)AudioChannel.MUSIC].Play();
    }

    //--------------------------------------------------------
    //  SE�̍Đ�
    //--------------------------------------------------------
    public void PlaySFX(int channelType, int clipId)
    {
#if UNITY_EDITOR
        Debug.Assert(
            channelType > (int)AudioChannel.MUSIC || channelType < (int)AudioChannel.CHANNEL_MAX,
            "�͈͊O��channelType���w�肵�Ă��邩BGM���w�肵�Ă��܂��I"
        );
        Debug.Assert(
            clipId < (int)SFXList.CLIP_MAX && clipId >= 0,
            "SFXClipID���͈͊O�̒l�ł��I"
        );
#endif
        //  �o�͐�ƃN���b�v��ݒ肵�čĐ�       
        _AudioSource[channelType].PlayOneShot( _SFXClipList[clipId] );
    }

    //--------------------------------------------------------
    //  �I�[�f�B�I�̒�~
    //--------------------------------------------------------
    public void Stop(int channelType)
    {
        _AudioSource[channelType].Stop();
    }
}
