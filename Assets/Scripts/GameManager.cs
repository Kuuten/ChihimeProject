using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

//  �Q�[�����̏��
public enum eGameState
{
    Zako,       //  �U�R�풆
    Boss,       //  �{�X�풆
    Event,      //  ��b�C�x���g��

    StateMax
}

//  ���ʕ\���p�I�u�W�F�N�g
public enum eResultObj
{
    RESULT_CANVAS,
    RESULT_IMAGE,
    SOUL_IMAGE,
    SOUL_TEXT,
    HONEG_IMAGE,
    HONEG_PANEL,
    HONEG_TEXT,
    BUTTON,
    KAMIFUBUKI,

    Max
}

// �V���b�v�A�C�e��ID
public enum eShopItemID
{
    RedHeart,           //  �Ԃ��n�[�g
    DoubleupHeart,      //  �_�u���A�b�v�n�[�g
    GoldHeart,          //  ���F�̃n�[�g
    HoneGBomb,          //  ��G�{��
    Powerup,            //  �V���b�g����
    Speedup,            //  �X�s�[�h����
    Shield,             //  �V�[���h

    Max
}

//--------------------------------------------------------------
//
//  �Q�[���Ǘ��N���X
//
//--------------------------------------------------------------
public class GameManager : MonoBehaviour
{
    [SerializeField]private GameObject player;              //  �v���C���[�I�u�W�F�N�g
    [SerializeField]private GameObject enemyGenerator;      //  �G�}�l�[�W���[�I�u�W�F�N�g

    private int gameState;  //  �Q�[���S�̂̏��

    [SerializeField] private GameObject soundManager;       //  �T�E���h�}�l�[�W���[
    [SerializeField] private FadeIO Fade;                   //  FadeIO�N���X
    [SerializeField] private ScrollAnimation Scroll;        //  ����

    //  �f�o�b�O�p
    private InputAction pauseButton;                        //  �|�[�Y�{�^���A�N�V����
    private bool pauseSwitch;                               //  �|�[�Y�t���O


    //  �V���O���g���ȃC���X�^���X
    public static GameManager Instance
    {
        get; private set;
    }

    //  �����̉��l
    private const int smallKonValue = 50;
    public int GetSmallKonValue(){ return smallKonValue; }
    private const int bigKonValue = 100;
    public int GetBigKonValue(){ return bigKonValue; }

    //  �o���|�C���g
    private GameObject[] spawner;
    //  ����_
    private GameObject[] controlPoint;

    //  �X�e�[�W�N���A�t���O
    bool stageClearFlag;

    //  �V�[���؂�ւ��t���O
    bool sceneChangeFlag;

    //----------------------------------------------------
    //  ���U���g�\���p
    //----------------------------------------------------
    //  ���U���g�\���p�I�u�W�F�N�g
    [SerializeField] private GameObject[] resultObject;
    //  �C�x���g�L�����o�X�I�u�W�F�N�g
    [SerializeField] private GameObject eventCanvas;


    //  �|�[�Y�L�����o�X�I�u�W�F�N�g
    [SerializeField] private GameObject pauseCanvas;
    //  PlayerInput
    PlayerInput _input;
    // �u�Q�[���ɂ��ǂ�v�{�^��
    [SerializeField] private Button returnGameButton;
    //  �Q�[���J�n�t���O
    private bool startFlag;

    //----------------------------------------------------
    //  �V���b�v�\���p
    //----------------------------------------------------
    //  �A�C�e���{�^���v���n�u
    [SerializeField] private GameObject[] itemButtonPrefab;
    //  �A�C�e�����X�g�I�u�W�F�N�g�i�����ʒu�j
    [SerializeField] private GameObject itemListObject;
    //  �V���b�v�L�����o�X�I�u�W�F�N�g
    [SerializeField] private GameObject shopCanvas;
    //  �Đ����{�^���I�u�W�F�N�g
    [SerializeField] private GameObject regenerateButton;
    //  ShopManager�N���X
    [SerializeField] private ShopManager shopManager;

    //------------------------------------------------------------------------------
    //  �v���p�e�B
    //------------------------------------------------------------------------------
    public int GetGameState()
    {
        return gameState;
    }
    public void SetGameState(int state)
    {
        gameState = state;
    }

    public GameObject GetPlayer(){ return player; }
    public bool GetStageClearFlag(){ return stageClearFlag; }
    public void SetStageClearFlag(bool b){ stageClearFlag = b; }
    public GameObject GetGameObject(int num){ return resultObject[num]; }
    public InputAction GetPauseAction(){ return pauseButton; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

#if UNITY_EDITOR
        //  SoundManager���Ȃ���ΐ���
        if( !GameObject.Find("SoundManager") )
        {
            Instantiate(soundManager);
        }
#endif
    }

    //---------------------------------------------------------------------
    //  ������
    //---------------------------------------------------------------------
    void Start()
    {
        //  �|�[�Y�L�����o�X�𖳌���
        pauseCanvas.SetActive(false);

        // InputAction��������
        _input = player.GetComponent<PlayerInput>();
        pauseButton = _input.actions["Pause"];
        pauseSwitch = true;
        
        gameState = (int)eGameState.Zako;   //  �ŏ��̓U�R��
        stageClearFlag = false;
        sceneChangeFlag = false;
        startFlag = false;

        //  �������J�n
        StartCoroutine(StartInit());
    }

    IEnumerator StartInit()
    {
        //  �V���b�g�𖳌���
        GetPlayer().GetComponent<PlayerShotManager>().DisableShot();

        //  �{���𖳌�������
        GetPlayer().GetComponent<PlayerBombManager>().DisableBomb();

        //  �t�F�[�h�C��
        yield return StartCoroutine(WaitingForFadeIn());

        yield return new WaitForSeconds(1); //  1�b�҂�

        //  �����A�j���[�V����
        yield return StartCoroutine(WaitingForOpeningScroll());

        yield return new WaitForSeconds(2.0f); //  2�b�҂�

        //  �����A�j�����I������̂ő���\
        startFlag = true;

        //  �V���b�g��L����
        GetPlayer().GetComponent<PlayerShotManager>().EnableShot();

        //  �{����L��������
        GetPlayer().GetComponent<PlayerBombManager>().EnableBomb();

        //  �v���C���[��Pauser��ǉ�
        GetPlayer().AddComponent<Pauser>();

        //  ���C�����[�v�J�n
        yield return StartCoroutine(GameLoop());
    }

    //---------------------------------------------------------------------
    //  �X�V
    //---------------------------------------------------------------------
    private IEnumerator GameLoop()
    {
        //yield return StartCoroutine(Tutorial());
        yield return StartCoroutine(GameStarting());
        yield return StartCoroutine(GamePlaying());
        yield return StartCoroutine(GameResult());
        yield return StartCoroutine(ShopMode());

        yield return null;
    }

    void Update()
    {
        //  �����A�j�����I����ĂȂ��Ȃ�return
        if(!startFlag)return;

        if (pauseButton.WasPressedThisFrame())
        {
            pauseSwitch = !pauseSwitch;

            if (pauseSwitch == false)
            {
                //  �|�[�Y�L�����o�X��L����
                pauseCanvas.SetActive(true);

                //  �A�N�V�����}�b�v��ݒ�
                InputActionMap mapPlayer = _input.actions.FindActionMap("Player");

                //  �e�}�b�v�Ƀ��[�h�`�F���W��ݒ�
                mapPlayer["Pause"].started -= ToTitleUIMode;

                //  �ŏ��̓h�E�W��I����Ԃɂ���
                EventSystem.current.SetSelectedGameObject(returnGameButton.gameObject);

                //  Pauser���t�����I�u�W�F�N�g���|�[�Y
                Pauser.Pause();

                //  �~�߂�
                Time.timeScale = 0;

                //  �|�[�Y��ɓ��͂������Ȃ��Ȃ�̂Ń��Z�b�g
                player.GetComponent<PlayerInput>().enabled = true;
            }
            else
            {
                //  �|�[�Y�L�����o�X�𖳌���
                pauseCanvas.SetActive(false);

                //  �A�N�V�����}�b�v��ݒ�
                InputActionMap mapTitle_ui = _input.actions.FindActionMap("TITLE_UI");

                //  �e�}�b�v�Ƀ��[�h�`�F���W��ݒ�
                mapTitle_ui["Pause"].started -= ToPlayerMode;

                //  �ĊJ����
                Time.timeScale = 1;

                //  Pauser���t�����I�u�W�F�N�g���|�[�Y
                Pauser.Resume();
            }
        }


    }

    //-----------------------------------------------------------------
    //  �u�Q�[���ɂ��ǂ�v�����������̏���
    //-----------------------------------------------------------------
    public void OnReturnGameButtonDown()
    {
        //  �|�[�Y�L�����o�X�𖳌���
        pauseCanvas.SetActive(false);

        //  �A�N�V�����}�b�v��ݒ�
        InputActionMap mapTitle_ui = _input.actions.FindActionMap("TITLE_UI");

        //  �e�}�b�v�Ƀ��[�h�`�F���W��ݒ�
        mapTitle_ui["Pause"].started -= ToPlayerMode;

        //  �ĊJ����
        Time.timeScale = 1;

        //  Pauser���t�����I�u�W�F�N�g�����W���[��
        Pauser.Resume();
    }

    //-----------------------------------------------------------------
    //  �u�^�C�g���ɂ��ǂ�v�����������̏���
    //-----------------------------------------------------------------
    public void OnReturnTitleButtonDown()
    {
        //  �|�[�Y�L�����o�X�𖳌���
        pauseCanvas.SetActive(false);

        //  �A�N�V�����}�b�v��ݒ�
        InputActionMap mapTitle_ui = _input.actions.FindActionMap("TITLE_UI");

        //  �e�}�b�v�Ƀ��[�h�`�F���W��ݒ�
        mapTitle_ui["Pause"].started -= ToPlayerMode;

        //  �ĊJ����
        Time.timeScale = 1;

        //  Pauser���t�����I�u�W�F�N�g���|�[�Y
        Pauser.Pause();

        //  �^�C�g���V�[���֑J��
        StartCoroutine(WaitingForClosingScrollByButtonDown());

    }

    //-----------------------------------------------------------------
    //  �Q�[���J�n�O
    //-----------------------------------------------------------------
    private IEnumerator GameStarting()
    {
        Debug.Log("GameStarting");

        //  ���̃X�e�[�W��ID���擾
        //int stageID = StageIdManager.Instance.GetStageID();

        //  �X�e�[�WBGM��ݒ�

        //  �L�����̔z�u�E�p�����[�^�����Z�b�g
        //ResetAllCharacters();

        //  �Q�[���J�n�O���b�Z�[�W�i�X�e�[�W�����A���t�@�̃A�j���[�V�����j
        //float msg_alpha_speed = 1.0f; //  �X�e�[�W���̃A���t�@���ő�ɂȂ�̂ɂ����鎞��
        //float msg_wait  = 3.0f;       //  ���b�Z�[�W�̕\������
        //yield return StartCoroutine(PreGameMessage
        //                            (
        //                                msg1speed, msg1wait
        //                            ));


        //  EnemyManager�̃��[�h�����܂ő҂�
        yield return new WaitUntil(()=> EnemyManager.Instance.GetIsCompleteLoading() == true);
        
        //  �G�̏o���J�n
        StartCoroutine( EnemyManager.Instance.AppearEnemy() );
        

        yield return null;
    }

    //-----------------------------------------------------------------
    //  �Q�[����
    //-----------------------------------------------------------------
    private IEnumerator GamePlaying()
    {
        Debug.Log("GamePlaying");

        //  Inputsystem���v���C���[���[�h��
        //EnableCharacterControl();

        //  �X�e�[�W�N���A���ĂȂ��Ԃ͑҂�
        yield return new WaitUntil(()=> stageClearFlag == true);

        yield return null;
    }

    // �t�F�[�h�C���̊�����҂�
    public IEnumerator WaitingForFadeIn()
    {
        yield return StartCoroutine(Fade.StartFadeIn());
    }

    // �t�F�[�h�A�E�g�̊�����҂�
    public IEnumerator WaitingForFadeOut()
    {
        yield return StartCoroutine(Fade.StartFadeOut());
    }

    //  �������J���A�j���[�V�����̊�����҂�
    IEnumerator WaitingForOpeningScroll()
    {
        yield return StartCoroutine(Scroll.OpenScroll());
    }

    //  ���������A�j���[�V�����̊�����҂�
    IEnumerator WaitingForClosingScroll()
    {
        yield return StartCoroutine(Scroll.CloseScroll());

        yield return new WaitForSeconds(1f); //  1�b�҂�

        ////  �̌��łł͂��������g��
        //yield return StartCoroutine(ResetAndChangeScene("TrialEnding"));

        //  ���i�łł͂��������g��
        yield return StartCoroutine(DataCopyAndChangeScene());
    }

    //  ���������A�j���[�V�����̊�����҂�
    IEnumerator WaitingForClosingScrollByButtonDown()
    {
        yield return StartCoroutine(Scroll.CloseScroll());

        yield return new WaitForSeconds(1f); //  1�b�҂�

        yield return StartCoroutine(ResetAndChangeScene("Title"));
    }

    //  �^�C�g����BGM���~�߂�
    public void StopBGM()
    {
        SoundManager.Instance.Stop((int)AudioChannel.MUSIC);
    }

    // �w��V�[���֏������Z�b�g���đJ�ڂ���
    public IEnumerator ResetAndChangeScene(string scene_name)
    {
        //  BGM���~�߂�
        StopBGM();

        //  �������Z�b�g
        PlayerInfoManager.ResetInfo();

        //   �w��V�[���֑J��
        LoadingScene.Instance.LoadNextScene(scene_name);

        yield return null;
    }

    // ���ۑ����V�[���J��
    public IEnumerator DataCopyAndChangeScene()
    {
        //  �e������擾
        GameObject player = GameManager.Instance.GetPlayer();
        int maxHP = player.GetComponent<PlayerHealth>().GetCurrentMaxHealth();
        int hP = player.GetComponent<PlayerHealth>().GetCurrentHealth();
        int bombNum = player.GetComponent<PlayerBombManager>().GetBombNum();
        int kon = MoneyManager.Instance.GetKonNum();
        int shotLV = player.GetComponent<PlayerShotManager>().GetNormalShotLevel();;
        int speedLV = player.GetComponent<PlayerMovement>().GetSpeedLevel();
        bool isShield = player.GetComponent<PlayerHealth>().GetIsShielded();

        //  ���ۑ�
        PlayerInfoManager.SetInfo(maxHP,hP,kon,bombNum,shotLV,speedLV,isShield);

        //  BGM���~�߂�
        StopBGM();

        //  ���݃X�e�[�W���m�F����
        CheckStageNo();

        yield return null;
    }

    //  ���݃X�e�[�W���X�V/
    private void CheckStageNo()
    {
        //****************************************************************
        //  ��U�X�e�[�W�R�܂ł̗\��Ō�ŃX�e�[�W�U�܂ō��
        //****************************************************************
        int currentStageNum = (int)PlayerInfoManager.stageInfo;
        if(currentStageNum >= (int)PlayerInfoManager.StageInfo.Stage03)
        {
            //  �S�X�e�[�W�I����Ă�����G���f�B���O��
            ResetAndChangeScene("Ending");
        }
        else
        {
            //  �C���N�������g����stageInfo�ɃZ�b�g
            currentStageNum++;
            PlayerInfoManager.stageInfo = (PlayerInfoManager.StageInfo)currentStageNum;
            
            //  ���o�[�g�Z���N�g��ʂ�
            LoadingScene.Instance.LoadNextScene("SelectConvert");
        }
        //****************************************************************
        //
        //****************************************************************
    }

    //-----------------------------------------------------------------
    //  �Q�[�����ʕ\��
    //-----------------------------------------------------------------
    private IEnumerator GameResult()
    {
        Debug.Log("***���ʕ\���҂��B***");

        //  ���ʕ\���J�n�t���O��TRUE�ɂ܂�܂ő҂�
        yield return new WaitUntil(()=> EventSceneManager.Instance.GetStartResult() == true);

        Debug.Log("***���ʕ\�����[�h�ɂȂ�܂����B***");

        //  ���ʕ\��Cnavas��\��
        resultObject[(int)eResultObj.RESULT_CANVAS].SetActive(true);

        //  �V�[���؂�ւ��t���O��TRUE�ɂ܂�܂ő҂�
        yield return new WaitUntil(()=> sceneChangeFlag == true);

        //  �V�[���؂�ւ��t���O�����Z�b�g
        sceneChangeFlag = false;
    }


    //--------------------------------------------------------------------------
    //  ���ۑ����V���b�v�֑J��(���U���g�Łu���ցv�{�^���������ꂽ��Ă΂��)
    //--------------------------------------------------------------------------
    public void OnNextButtonDownAtResult()
    {
        if(!sceneChangeFlag)    //  �A�ő΍�
        {
            //  ���ʕ\���L�����o�X���\���ɂ���
            resultObject[(int)eResultObj.RESULT_CANVAS].SetActive(false);

            //  BGM���~�߂�
            SoundManager.Instance.Stop((int)AudioChannel.MUSIC);

            //  ��������\���ɂ���
            resultObject[(int)eResultObj.KAMIFUBUKI].SetActive(false);

            //  ����SE��炷
            SoundManager.Instance.PlaySFX(
                (int)AudioChannel.SFX,
                (int)SFXList.SFX_TITLE_SELECT);

            ////------------------------------------------------------------------
            ////  �̌��ŗp����
            ////------------------------------------------------------------------
            ////  �V�[���؂�ւ��t���O��TRUE
            //sceneChangeFlag = true;
            ////  �����A�j���[�V���������ۑ�
            //StartCoroutine(WaitingForClosingScroll());
            //------------------------------------------------------------------

            //------------------------------------------------------------------
            //  ���i�ŗp����
            //------------------------------------------------------------------
            if( PlayerInfoManager.stageInfo != PlayerInfoManager.StageInfo.Stage06 )
            {
                shopCanvas.SetActive(true); //  �V���b�v�L�����o�X��\��
                InstantiateRandomItems();   //  ItemList�Ƀ����_���Ƀv���n�u�𐶐�
            }
            else
            {
                //  �V�[���؂�ւ��t���O��TRUE
                sceneChangeFlag = true;
                //  �����A�j���[�V���������ۑ�
                StartCoroutine(WaitingForClosingScroll());
            }

        }
    }

    //-----------------------------------------------------------------
    //  �V���b�v��ʂ�L����
    //-----------------------------------------------------------------
    private IEnumerator ShopMode()
    {
        Debug.Log("***�V���b�v��ʕ\�����[�h�ɂȂ�܂����B***");

        yield return null;
    }

    //--------------------------------------------------------------------------
    //  ���i�ɂȂ�A�C�e���������_���ɂR����
    //--------------------------------------------------------------------------
    private void InstantiateRandomItems()
    {
        //  �n�_�ƏI�_��ݒ肷��
        int start = (int)eShopItemID.RedHeart;
        int end = (int)eShopItemID.Max-1;

        //  �{�^�������������邩
        int chooseNum = 3;

        // 0�`eShopItemID.Max�܂ł̃��X�g�����
        List<int> idList = new List<int>();
        for(int i=start;i<=end;i++)
        {
            idList.Add(i);
        }      

        //  ���̒����烉���_����3���o����
        while(chooseNum-- > 0)
        {
            //  �����_���Ȑ��l�𒊏o(0~3)
            int index = UnityEngine.Random.Range(0, (int)idList.Count);
            int rand = idList[index];

            //  ItemList�I�u�W�F�N�g�̎q�Ƃ��ăI�u�W�F�N�g���𐶐�����
            GameObject obj = Instantiate(itemButtonPrefab[rand],itemListObject.transform);

            //  �{�^���̎�ނɂ���ăC�x���g��o�^����
            Button button = obj.GetComponent<Button>();
            if(rand == (int)eShopItemID.RedHeart)
            {
                //  �I�u�W�F�N�g��ID2�̎q�I�u�W�F�N�g����l�i���擾
                string s = obj.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text;
                shopManager.SetRedHeartValueText(s);

                shopManager.SetRedHeartButton(obj);
                button.onClick.AddListener( ()=>Debug.Log("�Ԃ��n�[�g�𔃂����I"));
                button.onClick.AddListener( ()=>shopManager.OnRedHeartButtonDown());
            }
            else if(rand == (int)eShopItemID.DoubleupHeart)
            {
                //  �I�u�W�F�N�g��ID2�̎q�I�u�W�F�N�g����l�i���擾
                string s = obj.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text;
                shopManager.SetDoubleupValueText(s);

                shopManager.SetDoubleupHeartButton(obj);
                button.onClick.AddListener( ()=>Debug.Log("�_�u���A�b�v�n�[�g�𔃂����I"));
                button.onClick.AddListener( ()=>shopManager.OnDoubleupHeartButtonDown());
            }
            else if(rand == (int)eShopItemID.GoldHeart)
            {
                //  �I�u�W�F�N�g��ID2�̎q�I�u�W�F�N�g����l�i���擾
                string s = obj.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text;
                shopManager.SetGoldHeartValueText(s);

                shopManager.SetGoldHeartButton(obj);
                button.onClick.AddListener( ()=>Debug.Log("���F�̃n�[�g�𔃂����I"));
                button.onClick.AddListener( ()=>shopManager.OnGoldHeartButtonDown());
            }
            else if(rand == (int)eShopItemID.HoneGBomb)
            {
                //  �I�u�W�F�N�g��ID2�̎q�I�u�W�F�N�g����l�i���擾
                string s = obj.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text;
                shopManager.SetBombValueText(s);

                shopManager.SetBombButton(obj);
                button.onClick.AddListener( ()=>Debug.Log("��G�{���𔃂����I"));
                button.onClick.AddListener( ()=>shopManager.OnHoneGBombButtonDown());
            }
            else if(rand == (int)eShopItemID.Powerup)
            {
                //  �I�u�W�F�N�g��ID2�̎q�I�u�W�F�N�g����l�i���擾
                string s = obj.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text;
                shopManager.SetPowerupValueText(s);

                shopManager.SetPowerupButton(obj);
                button.onClick.AddListener( ()=>Debug.Log("�V���b�g�����𔃂����I"));
                button.onClick.AddListener( ()=>shopManager.OnPowerupButtonDown());
            }
            else if(rand == (int)eShopItemID.Speedup)
            {
                //  �I�u�W�F�N�g��ID2�̎q�I�u�W�F�N�g����l�i���擾
                string s = obj.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text;
                shopManager.SetSpeedupValueText(s);

                shopManager.SetSpeedupButton(obj);
                button.onClick.AddListener( ()=>Debug.Log("�X�s�[�h�����𔃂����I"));
                button.onClick.AddListener( ()=>shopManager.OnSpeedupButtonDown());
            }
            else if(rand == (int)eShopItemID.Shield)
            {
                //  �I�u�W�F�N�g��ID2�̎q�I�u�W�F�N�g����l�i���擾
                string s = obj.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text;
                shopManager.SetShieldValueText(s);

                shopManager.SetShieldButton(obj);
                button.onClick.AddListener( ()=>Debug.Log("�V�[���h�ǉ��𔃂����I"));
                button.onClick.AddListener( ()=>shopManager.OnShieldButtonDown());
            }



            //  ���̔ԍ���idList���珜�O
            idList.RemoveAt(index);
        }

        //  PlayerInput�̃}�b�v��UI���[�h�ɕύX
        PlayerInput playerInput = GameManager.Instance.GetPlayer()
            .GetComponent<PlayerInput>();
        playerInput.enabled = true;
        playerInput.SwitchCurrentActionMap("Title_UI");

        //  itemListObject�̍ŏ��̎q�I�u�W�F�N�g��I����Ԃɂ���
        EventSystem.current.SetSelectedGameObject(
            itemListObject.transform.GetChild(0).gameObject);

        //  �������������烊�X�g��S�폜
        idList.Clear();
    }

    //--------------------------------------------------------------------------
    //  �A�C�e���{�^����S���폜���čĐ���(�ē���:3000��)
    //--------------------------------------------------------------------------
    public void ReGenerate()
    {
        int value = 3000;   //  �ē��ב��

        //  ������������炷
        if(MoneyManager.Instance.CanBuyItem(value))
        { 
            //  �e�I�u�W�F�N�g��Transform���擾����
            Transform parentTransform = itemListObject.transform;

            //  �S�Ă̎q�I�u�W�F�N�g���폜����
            foreach (Transform child in parentTransform)
            {
                Destroy(child.gameObject);
            }

            //  �Đ���
            InstantiateRandomItems();
        }
        //  �w���s�\�Ȃ烁�b�Z�[�W���o��
        else
        {
            //  �w�����s���b�Z�[�W���o��
            StartCoroutine(shopManager.DisplayFailedMessage());
        }

        //  �Đ����{�^���I�u�W�F�N�g��I����Ԃɂ���
        EventSystem.current.SetSelectedGameObject(
            regenerateButton.gameObject);
    }

    //--------------------------------------------------------------------------
    //  �V���b�v�֑J��(�V���b�v�Łu���ցv�{�^���������ꂽ��Ă΂��)
    //--------------------------------------------------------------------------
    public void OnNextButtonDownAtShop()
    {
        if(!sceneChangeFlag)    //  �A�ő΍�
        {
            //  �V���b�v�L�����o�X���\���ɂ���
            shopCanvas.SetActive(false);

            //  ����SE��炷
            SoundManager.Instance.PlaySFX(
                (int)AudioChannel.SFX,
                (int)SFXList.SFX_TITLE_SELECT);

            //  �C�x���g�L�����o�X���\���ɂ���
            eventCanvas.SetActive(false);

            //  �V�[���؂�ւ��t���O��TRUE
            sceneChangeFlag = true;

            //  �����A�j���[�V���������ۑ�
            StartCoroutine(WaitingForClosingScroll());
        }
    }

    //  �A�N�V�����}�b�v��Title_UI�ɐ؂�ւ���
    private void ToTitleUIMode(InputAction.CallbackContext context)
    {
        _input.SwitchCurrentActionMap("Title_UI");
    }

    //  �A�N�V�����}�b�v��Player�ɐ؂�ւ���
    private void ToPlayerMode(InputAction.CallbackContext context)
    {
        _input.SwitchCurrentActionMap("Player");
    }
}
