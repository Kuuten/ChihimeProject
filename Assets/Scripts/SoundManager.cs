using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    //  �I�[�f�B�I�\�[�X���X�g
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
    //  SFX�N���b�v���X�g
    //-----------------------------------------------
    public enum SFXList
    {
        //  �^�C�g��
        SFX_HYOUSIGI,         //  ���q�؂̉�
        SFX_SCROLL_MOVE,      //  �������������̉�
        SFX_SCROLL_MOVE2,     //  �������������̉��Q�i�Ձj
        SFX_DASH,             //  �_�b�V��
        SFX_BRAKE,            //  �u���[�L
        SFX_TITLE_SELECT,     //  �J�[�\����
        SFX_TITLE_DECISION,   //  �J�[�\�����艹

        //  �C�x���g�E�`���[�g���A��

        //  �Q�[�����E�V���b�v
        SFX_GAMESTART,
        SFX_NORMAL_SHOT,
        SFX_POWER_SHOT,
        SFX_WIDE_SHOT,
        SFX_PENETRATION_SHOT,
        SFX_HORMINGSHOT,

        //  �Q�[���N���A
        SFX_VICTORY,
        SFX_LOSE,
        SFX_GAMECLEAR01,
        SFX_GAMECLEAR02,

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
    //  �I�[�f�B�I�̍Đ�
    //--------------------------------------------------------
    public void Play(int channelType, int clipId)
    {
#if UNITY_EDITOR
        Debug.Assert(
            channelType < (int)AudioChannel.CHANNEL_MAX && channelType >= 0,
            "AudioSourceID���͈͊O�̒l�ł��I"
        );
        Debug.Assert(
            clipId < (int)MusicList.CLIP_MAX && clipId >= 0,
            "MusicClipID���͈͊O�̒l�ł��I"
        );
        Debug.Assert(
            clipId < (int)SFXList.CLIP_MAX && clipId >= 0,
            "SFXClipID���͈͊O�̒l�ł��I"
        );
#endif
        //  �o�͐�ƃN���b�v��ݒ肵�čĐ�
        if(channelType == (int)AudioChannel.MUSIC)
        {
            _AudioSource[channelType].clip = _MusicClipList[clipId];
        }
        else _AudioSource[channelType].clip = _SFXClipList[clipId];
        
        _AudioSource[channelType].Play();
    }

    //--------------------------------------------------------
    //  �I�[�f�B�I�̒�~
    //--------------------------------------------------------
    public void Stop(int channelType)
    {
        _AudioSource[channelType].Stop();
    }
}
