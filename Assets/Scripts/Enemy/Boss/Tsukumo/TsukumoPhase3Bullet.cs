using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//--------------------------------------------------------------
//
//  �c�N����Phase3�̒e�N���X
//
//--------------------------------------------------------------
public class TsukumoPhase3Bullet : MonoBehaviour
{
    private Vector3 velocity;
    private float speed;
    private int power;
    private Vector3 vec;    //  �e�z�u���̃x�N�g��

    private int step;
    private int hp;
    private static readonly int hp_max = 3;

    private float timer;                //  ��]���J��Ԃ�����(�b)
    private Transform parentTransform;  //  �c�N����Transform

    private int homingFrame;            //  �z�[�~���O�̌v�Z�p�x�p

    void Awake()
    {
        speed = 0f;
        power = 0;
        velocity = Vector3.zero;
        step = 0;
        hp = hp_max;
        vec = Vector3.zero;
        timer = 1;
        parentTransform = default;
    }

    void Start()
    {
        //  �W�J�J�n
        StartCoroutine( Expand() );

        //  �T�b�����������
        Destroy(this.gameObject, 5f);
    }

    void Update()
    {
        //  �����e�̈ړ�
        BerserkBullet();

        //  �X�e�[�W�N���A������c��Ȃ��悤�ɏ���
        if(GameManager.Instance.GetStageClearFlag())
        {
            Destroy(this.gameObject);
        }
    }

    //-------------------------------------------------
    //  �����蔻��
    //-------------------------------------------------
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("DeadWall") ||
            collision.CompareTag("DeadWallBottom"))
        {
            Destroy(this.gameObject);
        }
    }


    //------------------------------------------------------
    //  �v���p�e�B
    //------------------------------------------------------
    public void SetVelocity(Vector3 v){ velocity = v; }
    public void SetSpeed(float s){ speed = s; }
    public void SetVec(Vector3 v){ vec = v; }
    public void SetPower(int p){ power = p; }
    public int GetPower(){ return power; }
    public void SetParentTransform(Transform t){ parentTransform = t; }

    //------------------------------------------------------
    //  �����e�̈ړ�
    //------------------------------------------------------
    private void BerserkBullet()
    {
        switch(step)
        {
            case 0: //  �W�J
                break;
            case 1: //  ��]
                RotationAxis();
                break;
            case 2: //  �z�[�~���O
                Homing();
                break;
            case 3: //  ���W�����X�V����
                //  ���W���X�V
                transform.position += speed * velocity * Time.deltaTime;
                break;
        }
    }

    //------------------------------------------------------
    //  �W�J
    //------------------------------------------------------
    private IEnumerator Expand()
    {
        float duration = 0.5f;   //  �ړ��ɂ����鎞�ԁi�b�j

        //  ��]����
        float rotationTime = 0.5f;
        Tweener tweener = transform.DOLocalRotate(new Vector3(0, 0, -360f), rotationTime, RotateMode.FastBeyond360)  
            .SetEase(Ease.Linear)  
            .SetLoops(-1, LoopType.Restart);

        //  �ڕW���W��ݒ肷��
        Vector3 targetPos = transform.position+ vec;

        //  �ړ��J�n
        transform.DOMove(targetPos,duration)
            .OnComplete(()=>{
                tweener.Kill();     //  ��]��~
                step++;
            });

        //  �ړ����ԑ҂�
        yield return new WaitForSeconds(duration);
    }
    //------------------------------------------------------
    //  �c�N���𒆐S�ɉ�]
    //------------------------------------------------------
    private void RotationAxis()
    {
        // ��]��
        Vector3 axis = parentTransform.forward;
        // �~�^������
        float period = 1;
        //  ���S�_
        Vector3 center = parentTransform.position;

        var tr = transform;
        // ��]�̃N�H�[�^�j�I���쐬
        var angleAxis = Quaternion.AngleAxis(360 / period * Time.deltaTime, axis);

        // �~�^���̈ʒu�v�Z
        var pos = tr.position;

        pos -= center;
        pos = angleAxis * pos;
        pos += center;

        tr.position = pos;

        //  �J�E���g�_�E��
        if( timer <= 0 )
        {
            timer = 0;
            step++;
        }
        else
        {
            timer -= Time.deltaTime;
        }
    }
    //------------------------------------------------------
    //  �z�[�~���O(�����p�x�␳����)
    //------------------------------------------------------
    private void Homing()
    {
        Debug.Log("�z�[�~���O���n�߂܂��I�I");

        //  �v���C���[�̍��W���擾����
        Vector3 pPos = GameManager.Instance.GetPlayer().transform.position;

        //  �e����v���C���[�ւ̃x�N�g�����v�Z����
        Vector3 v = pPos - transform.position;
        v.Normalize();

        //  velocity�ɂ��̃x�N�g����ݒ肷��
        velocity = v;

        //  ���W���X�V
        transform.position += speed * velocity * Time.deltaTime;

        //  �t���[�����X�V
        if(homingFrame < 60 * 1)homingFrame++;
        else
        {
            //  homingFrame��1�b�o������z�[�~���O����߂�
            step++;
        };
        
    }
}
