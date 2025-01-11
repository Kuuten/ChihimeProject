using UnityEngine.Audio;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

//--------------------------------------------------------------
//
//  �R���t�B�O��ʂ̃I�[�f�B�I�����̃N���X
//
//--------------------------------------------------------------
public class Audio : MonoBehaviour
{
    //  Audio�~�L�T�[������Ƃ�
    [SerializeField] AudioMixer audioMixer;

    //  ���ꂼ��̃X���C�_�[������Ƃ�
    [SerializeField] Slider BGMSlider;
    [SerializeField] Slider SFXSlider;

    //  �ύX�O�̃R���t�B�O�ɓ������u�Ԃ̒l��ۑ�����ϐ�
    float preBGMVolume;
    float preSFXVolume;

    //  ���ꂼ��̏����l
    static readonly float initialBGMVolume = 6.0f;
    static readonly float initialSFXVolume = 6.0f;

    //  �l�͈̔�
    static readonly float AudioMixerMinVolume = -80f;
    static readonly float AudioMixerVolumeRange = 90f;
    static readonly float SliderValueRange = 10f;

    // �㏑�����̕ۑ���
    private string BGM_savePath = "bgm_config.json";
    private string SFX_savePath = "sfx_config.json";

    private void Start()
    {
        //  ������
        Init();
    }

    //---------------------------------------------------------------------
    //  ������
    //---------------------------------------------------------------------
    public void Init()
    {
        //  AudioMixer��BGM�̃{�����[���l���擾���ăX���C�_�[�ɐݒ�
        LoadBGM();

        //  AudioMixer��CONVERT_SHOT�̃{�����[���l���擾���ăX���C�_�[�ɐݒ�
        LoadSFX();
    }

    //---------------------------------------------------------------------
    //  �X���C�_�[�̒l��ϊ�����AudioMixer�ɓK�p����
    //---------------------------------------------------------------------
    public void SetBGM()
    {
        //  0�`10(�X���C�_�[)�̒l��-80�`0�iAudioMixer�j�̒l�ɂ���
        float value = AudioMixerMinVolume + 
            (AudioMixerVolumeRange/SliderValueRange) * BGMSlider.value;

        audioMixer.SetFloat("BGM", value);
    }

    public void SetBGM(float base_value)
    {
        //  0�`10(�X���C�_�[)�̒l��-80�`0�iAudioMixer�j�̒l�ɂ���
        float value = AudioMixerMinVolume + 
            (AudioMixerVolumeRange/SliderValueRange) * base_value;

        audioMixer.SetFloat("BGM", value);
    }

    public void SetSFX()
    {
        //  0�`10(�X���C�_�[)�̒l��-80�`0�iAudioMixer�j�̒l�ɂ���
        float base_value = AudioMixerMinVolume + 
            (AudioMixerVolumeRange/SliderValueRange) * SFXSlider.value;

        //  CONVERT_SHOT����Ƃ���
        audioMixer.SetFloat("CONVERT_SHOT", base_value);
        audioMixer.SetFloat("SFX", base_value-10);
        audioMixer.SetFloat("SFX_ENEMY", base_value-20f);
        audioMixer.SetFloat("ENEMY_SHOT", base_value-10);
        audioMixer.SetFloat("SFX_DAMAGE", base_value-10);
        audioMixer.SetFloat("SFX_SYSTEM", base_value-10);
        audioMixer.SetFloat("SFX_SYSTEM2", base_value);
        audioMixer.SetFloat("NORMAL_SHOT", base_value-20);
        audioMixer.SetFloat("BOMB", base_value);
        audioMixer.SetFloat("SFX_LOOP", base_value-20);
    }

    public void SetSFX(float baseValue)
    {
        //  0�`10(�X���C�_�[)�̒l��-80�`0�iAudioMixer�j�̒l�ɂ���
        float base_value = AudioMixerMinVolume + 
            (AudioMixerVolumeRange/SliderValueRange) * baseValue;

        //  CONVERT_SHOT����Ƃ���
        audioMixer.SetFloat("CONVERT_SHOT", base_value);
        audioMixer.SetFloat("SFX", base_value-10);
        audioMixer.SetFloat("SFX_ENEMY", base_value-20f);
        audioMixer.SetFloat("ENEMY_SHOT", base_value-10);
        audioMixer.SetFloat("SFX_DAMAGE", base_value-10);
        audioMixer.SetFloat("SFX_SYSTEM", base_value-10);
        audioMixer.SetFloat("SFX_SYSTEM2", base_value);
        audioMixer.SetFloat("NORMAL_SHOT", base_value-20);
        audioMixer.SetFloat("BOMB", base_value);
        audioMixer.SetFloat("SFX_LOOP", base_value-20);
    }

    //---------------------------------------------------------------------
    //  �Z�[�u�p���\�b�h(Json�`���ŕۑ�����)
    //---------------------------------------------------------------------
    public void SaveBGM()
    {
        //  BGMSlider.value��Json�`���ŕۑ�

        //  �X���C�_�[�̒l��string�^�ɂ��Ċi�[
        string data = BGMSlider.value.ToString();

        // �t�@�C���ɕۑ�
        var path = Path.Combine(Application.persistentDataPath, BGM_savePath);
        File.WriteAllText(path, data);
    }

    public void SaveSFX()
    {
        //  SFXSlider.value��Json�`���ŕۑ�

        //  �X���C�_�[�̒l��string�^�ɂ��Ċi�[
        string data = SFXSlider.value.ToString();

        // �t�@�C���ɕۑ�
        var path = Path.Combine(Application.persistentDataPath, SFX_savePath);
        File.WriteAllText(path, data);
    }

    //---------------------------------------------------------------------
    //  ���[�h�p���\�b�h(�^�C�g���ŃR���t�B�O�������ꂽ���Ɏ��s����)
    //---------------------------------------------------------------------
    public void LoadBGM()
    {
        // �t�@�C������ǂݍ���
        var path = Path.Combine(Application.persistentDataPath, BGM_savePath);
        if (!File.Exists(path))
        {
            Debug.Log("bgm_config.json��������܂���ł����B\n" +
                "���̂܂ܑ��s���܂��B");
            
            //  �Ƃ肠�����K��l��ݒ�
            BGMSlider.value = initialBGMVolume;
        }
        else
        {
             Debug.Log("bgm_config.json��������܂����I\n" +
                    "���[�h���܂��B");

            var json = File.ReadAllText(path);

            // JSON�t�@�C���̏����X���C�_�[�ɐݒ�
            BGMSlider.value = int.Parse(json);
        }
        
        //  ���̎��_�̒l��ۑ�
        preBGMVolume = BGMSlider.value;

        //  AudioMixer�ɔ��f
        SetBGM();

        Debug.Log("���[�h����"); 
    }

    public void LoadSFX()
    {
        // �t�@�C������ǂݍ���
        var path = Path.Combine(Application.persistentDataPath, SFX_savePath);
        if (!File.Exists(path))
        {
            Debug.Log("sfx_config.json��������܂���ł����B\n" +
                "���̂܂ܑ��s���܂��B");

            //  �Ƃ肠�����K��l��ݒ�
            SFXSlider.value = initialSFXVolume;
        }
        else
        {
             Debug.Log("sfx_config.json��������܂����I\n" +
                    "���[�h���܂��B");

            var json = File.ReadAllText(path);

            // JSON�t�@�C���̏����X���C�_�[�ɐݒ�
            SFXSlider.value = int.Parse(json);
        }

        //  ���̎��_�̒l��ۑ�
        preSFXVolume = SFXSlider.value;

        //  AudioMixer�ɔ��f
        SetSFX();

        Debug.Log("���[�h����"); 
    }

    //---------------------------------------------------------------------
    //  �T�E���h�R���t�B�O�ŃL�����Z���������ꂽ���̏���
    //---------------------------------------------------------------------
    public void OnCancelButtonDown()
    {
        //  ������
        Init();
    }
}