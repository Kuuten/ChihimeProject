using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class ConvertTsukumoBullet : MonoBehaviour
{
    GameObject target;
    private int gamestatus;
    private bool fullPower;
    private bool isL;
    private bool startHoming;
    private float speed = 5.0f;


   void Start()
    {
        target = null;
        startHoming = false;

        //  SE�Đ�
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX_CONVERT_SHOT,
            (int)SFXList.SFX_TSUKUMO_CONVERT_SHOT_01);

        //  �R���[�`���J�n
        StartCoroutine(Move());
    }
    
    void Update()
    {
        //  GameManager�����Ԃ��擾
        gamestatus = GameManager.Instance.GetGameState();

        if(startHoming)Homing();
    }

    //-----------------------------------------------------------
    //  �v���p�e�B
    //-----------------------------------------------------------
    public void SetFullPower(bool b){ fullPower = b; }
    public bool GetFullPower(bool b){ return fullPower; }
    public void SetIsL(bool b){ isL = b; }
    public bool GetIsL(){ return isL; }

    //-----------------------------------------------------------
    //  �ړ�
    //-----------------------------------------------------------
    private IEnumerator Move()
    {
        //  ��]����
        float rotationTime = 0.5f;
        Tweener tweener = transform.DOLocalRotate(new Vector3(0, 0, -360f), rotationTime, RotateMode.FastBeyond360)  
            .SetEase(Ease.Linear)  
            .SetLoops(-1, LoopType.Restart); 

        //  ��]SE�Đ�
        SoundManager.Instance.PlaySFX(
            (int)AudioChannel.SFX_LOOP,
            (int)SFXList.SFX_TSUKUMO_CONVERT_SHOT_02);

        //  �v���C���[�̍��W
        Vector3 playerPos = GameManager.Instance.GetPlayer().transform.position;

        //  �v���C���[�̍��E�x�N�g�������߂�
        Vector3 left = -this.transform.right;
        Vector3 right = this.transform.right;

        //  ���E�ɂP���ꂽ���W�����߂�
        float duration = 1.0f;
        float bias = 2.0f;
        Vector3 posL = this.transform.position + left * bias;
        Vector3 posR = this.transform.position + right * bias;

        //  �����ֈړ�����
        if(isL)
        {
            transform.DOMove(posL,duration);
        }
        else
        {
            transform.DOMove(posR,duration);
        }

        //  �ړ����ԑ҂�
        yield return new WaitForSeconds(duration);

        //  �҂�
        yield return new WaitForSeconds(3);

        //  ��]�A�j�����I��
        tweener.Kill();

        //  �z�[�~���O�J�n
        startHoming = true;

        //  �z�[�~���OSE�Đ�
        SoundManager.Instance.PlayLoopSFX(
            (int)AudioChannel.SFX_CONVERT_SHOT,
            (int)SFXList.SFX_TSUKUMO_CONVERT_SHOT_03);
    }

    //-----------------------------------------------------------
    //  �K���z�[�~���O(������܂Œǂ�����������)
    //-----------------------------------------------------------
    private void Homing()
    {
        //  ��ԋ߂��G�I�u�W�F�N�g���擾����
        if(gamestatus == (int)eGameState.Zako)
        {
            target = EnemyManager.Instance.GetNearestEnemyFromPlayer();
        }
        else if(gamestatus == (int)eGameState.Boss)
        {
            target = EventSceneManager.Instance.GetBossObject();
        }else if(gamestatus == (int)eGameState.Event)
        {
            startHoming = false;
            Destroy(this.gameObject);
            return;
        }

        //  �G�����Ȃ���Βe���������Ė߂�
        if(target == null)
        {
            Debug.Log("�G�����Ȃ��I");
            Destroy(this.gameObject);
            return;
        }

        //  �e����G�ւ̃x�N�g�������߂�
        Vector3 vec = target.transform.position - this.transform.position;

        //  �x�N�g���̃I�C���[�p�����߂�
        Quaternion q = Quaternion.Euler(vec);
        float degree = q.eulerAngles.z;

        //  �e�̌�����G�Ɍ�����
        this.transform.Rotate(0,0,degree);

        //  ��ԋ߂��G�֓ˌ�
        this.transform.position += vec * speed * Time.deltaTime;
    }
}
