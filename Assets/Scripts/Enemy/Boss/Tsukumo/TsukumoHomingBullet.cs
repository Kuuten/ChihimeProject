using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------
//
//  �c�N���̃z�[�~���O�e�N���X
//
//--------------------------------------------------------------
public class TsukumoHomingBullet : MonoBehaviour
{
    //  �O������e�̃x�N�g���ƈړ��X�s�[�h���擾���A�ړ����邾��

    private Vector3 velocity;
    private float speed;
    private int power;

    private int step;

    private static readonly float radius = 1.0f;
    [SerializeField] Vector3 controlPos;

    //  �J��Ԃ�����(�b)
    private float timer = 3;
    // �񂷊p�x(�x)
    float degree;

    // ��]��
    [SerializeField] private Vector3 axis = Vector3.forward;

    // �~�^������
    [SerializeField] private float period = 2;
    
    void Awake()
    {
        step = 0;
        speed = 0f;
        power = 0;
        velocity = Vector3.zero;
        controlPos = Vector3.zero;
        degree = 0;
    }

    private void Start()
    {
        //  �����_���ȃR���g���[���|�C���g�ɔ��
        FlyToRandomControlPoint();
    }

    void Update()
    {
        switch(step)
        {
            case 0:
                CaculateDistance();
                //  ���W���X�V
                transform.position += speed * velocity * Time.deltaTime;
                break;
            case 1:
                RorateAroundControlPoint();
                break;
            case 2:
                //  ���W���X�V
                transform.position += speed * velocity * Time.deltaTime;
                break;
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
    public void SetPower(int p){ power = p; }
    public int GetPower(){ return power; }

    //------------------------------------------------------
    //  �ڕW�n�_�Ƃ̋����𑪂�
    //------------------------------------------------------
    private void CaculateDistance()
    {
        //  �������v�Z����
        float distance = Vector3.Distance( controlPos, transform.position ); 

        //  ��苗���ɂȂ�Ɠ��B�t���O��True
        if(distance <= radius)
        {
            //  �F�ς�
            StartCoroutine(ColorAnimation());
            step++;
        }
    }

    //------------------------------------------------------
    //  �����_���ȃR���g���[���|�C���g�ɔ��ł���
    //------------------------------------------------------
    private void FlyToRandomControlPoint()
    {
        //  0�`8�Ԃ܂ł������_���ɒ��o
        int rand = UnityEngine.Random.Range( 3, 9 );
        
        //  �����_���ȃR���g���[���|�C���g�̍��W���擾
        controlPos = EnemyManager.Instance.GetControlPointPos(rand);

        //  ���̍��W�ւ̃x�N�g�����v�Z
        Vector3 vec = controlPos - this.transform.position;
        vec.Normalize();

        //  velocity�ɂ��̃x�N�g����ݒ�
        velocity = vec;
    }

    //------------------------------------------------------
    //  �R���g���[���|�C���g�̎������]����
    //------------------------------------------------------
    private void RorateAroundControlPoint()
    {
        var tr = transform;
        // ��]�̃N�H�[�^�j�I���쐬
        var angleAxis = Quaternion.AngleAxis(360 / period * Time.deltaTime, axis);

        // �~�^���̈ʒu�v�Z
        var pos = tr.position;

        pos -= controlPos;
        pos = angleAxis * pos;
        pos += controlPos;

        tr.position = pos;

        //  �J�E���g�_�E��
        if( timer <= 0 )
        {
            timer = 0;
            FlyToPlayer();  //  �v���C���[�Ɍ������Ĕ�΂�
            step++;
        }
        else
        {
            timer -= Time.deltaTime;
        }
    }

    //------------------------------------------------------
    //  �e�̐F�ւ��A�j���[�V����
    //------------------------------------------------------
    private IEnumerator ColorAnimation()
    {
        SpriteRenderer fadeSprite = this.GetComponent<SpriteRenderer>();
        fadeSprite.enabled = true;
        var c = fadeSprite.color;
        c.a = 1.0f; // �����l
        fadeSprite.color = c;

        DOTween.ToAlpha(
        ()=> fadeSprite.color,
        color => fadeSprite.color = color,
        0.0f, // �ڕW�l
        1.5f // ���v����
        );

        yield return new WaitForSeconds(1.5f);

        //  �X�v���C�g�������ւ�
        SpriteRenderer s = EnemyManager.Instance.GetHomingBulletSprite();
        this.GetComponent<SpriteRenderer>().sprite = s.sprite;
        this.GetComponent<SpriteRenderer>().color = new Color(1,1,1,0);

        DOTween.ToAlpha(
        ()=> fadeSprite.color,
        color => fadeSprite.color = color,
        1.0f, // �ڕW�l
        1.5f // ���v����
        );

        yield return new WaitForSeconds(1.5f);
    }

    //------------------------------------------------------
    //  �v���C���[�Ɍ������Ĕ��ł���
    //------------------------------------------------------
    private void FlyToPlayer()
    {
        //  �v���C���[�̍��W
        Vector3 playerPos = GameManager.Instance.GetPlayer().transform.position;

        //  �v���C���[�ւ̃x�N�g��
        Vector3 vector = playerPos - this.transform.position;
        vector.Normalize();

        //  ������ݒ�
        velocity = vector;
    }
}
