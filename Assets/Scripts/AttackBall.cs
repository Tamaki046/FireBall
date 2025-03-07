using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

// TODO�FAttackParticle�N���X�Ƌ߂�����
//     �@��肭�I�[�o�[���C�h�Ƃ����g���΂܂Ƃ߂���\��������
public class AttackBall : GameObjectBase
{
    public static event System.Action<Vector3, float> BreakEvent;
    private float cnt_flare_spark_cycle;

    [Tooltip("�c������"), Range(0.5f, 3.0f)]
    [SerializeField]
    private float object_lifetime_sec = 1.5f;

    [Tooltip("�j�󔼌a"), Range(0.1f, 10.0f)]
    [SerializeField]
    private float BREAK_RADIUS;

    [SerializeField, Tooltip("�΂̕��̃v���n�u")]
    private GameObject attack_spark_prefab;

    [SerializeField, Tooltip("�΂̕��ő唭�ˊԊu"), Range(0.1f,1.0f)]
    private float MAX_FLARE_SPARK_CYCLE = 0.3f;

    [SerializeField, Tooltip("�΂̕��̔��ˑ��x"), Min(0.1f)]
    private float SPARK_SPEED = 3.0f;

    [SerializeField, Tooltip("�΂̕����o�Ȃ��b��"), Min(0.1f)]
    private float no_spark_sec = 0.2f;

    private void Start()
    {
        ConnectEventAction(true);
        base.TransitionToActing();
        return;
    }

    private void ConnectEventAction(bool is_connect_event)
    {
        if (is_connect_event)
        {
            StageManager.GameStop += SetActiveFalse;
            StageManager.LeaveScene += PrepareLeaveScene;
            Player.GameOver += SetActiveFalse;
        }
        else
        {
            StageManager.GameStop -= SetActiveFalse;
            StageManager.LeaveScene -= PrepareLeaveScene;
            Player.GameOver -= SetActiveFalse;
        }
    }


    protected override void UpdateOnActing()
    {
        if (this.transform.position.y <= -10.0f)
        {
            DestroyThisGameObject();
        }

        object_lifetime_sec -= Time.deltaTime;
        if (object_lifetime_sec <= 0.0f)
        {
            DestroyThisGameObject();
        }

        if (no_spark_sec > 0.0f)
        {
            no_spark_sec -= Time.deltaTime;
        }
        else
        {
            cnt_flare_spark_cycle += Time.deltaTime;
            float r = Random.value;
            // 3��I�[�_�[���炢����Փx�I�ɗǂ�������ہH
            if (r <= Mathf.Pow(cnt_flare_spark_cycle / MAX_FLARE_SPARK_CYCLE, 3))
            {
                SpawnFireSpark();
                cnt_flare_spark_cycle = 0.0f;
            }
            if (this.transform.position.y <= -10.0f)
            {
                DestroyThisGameObject();
            }
        }
    }

    private void SpawnFireSpark()
    {
        Vector3 shot_position = this.transform.position;
        GameObject firespark = Instantiate(attack_spark_prefab, shot_position, Quaternion.identity);
        Rigidbody firespark_rigidbody = firespark.GetComponent<Rigidbody>();
        Vector3 shot_velocity = new Vector3(
            Random.Range(-1.0f, 1.0f),
            Random.Range(-1.0f, 1.0f),
            Random.Range(-1.0f, 1.0f)
        ).normalized * SPARK_SPEED;
        firespark_rigidbody.linearVelocity = shot_velocity;
        return;
    }

    private void OnCollisionEnter(Collision collision_object)
    {
        int[] NOT_DESTROY_LAYERS = {
            LayerMask.NameToLayer("Wall")
            };
        if (!NOT_DESTROY_LAYERS.Contains(collision_object.gameObject.layer))
        {
            int TARGET_LAYER = LayerMask.NameToLayer("Target");
            if(collision_object.gameObject.layer != TARGET_LAYER && base.state == States.ACTING)
            {
                BreakField(this.transform.position);
            }
            DestroyThisGameObject();
        }
        return;
    }

    private void BreakField(Vector3 break_position)
    {
        BreakEvent.Invoke(break_position, BREAK_RADIUS);
        return;
    }

    private void DestroyThisGameObject()
    {
        PrepareLeaveScene();
        Destroy(this.gameObject);
        return;
    }


    private void PrepareLeaveScene()
    {
        ConnectEventAction(false);
        return;
    }

    private void SetActiveFalse()
    {
        // �I�u�W�F�N�g�̔j��ƃC�x���g�̎��s�̏������O�シ��ꍇ������̂ŁA
        // ���Ȃ��Q�[�����i�ނ悤try-catch�őΏ�
        try
        {
            Rigidbody attack_rigidbody = GetComponent<Rigidbody>();
            attack_rigidbody.linearVelocity = Vector3.zero;
            attack_rigidbody.isKinematic = true;
            base.TransitionToFinished();
        }
        catch (MissingReferenceException)
        {
            return;
        }
        return;
    }
}
