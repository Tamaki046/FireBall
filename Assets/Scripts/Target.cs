using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.PlayerLoop;
public class Target : MonoBehaviour
{

    private bool isDead = false;
    private GameObject PLAYER_GAMEOBJECT;
    private float cnt_shot_cycle = 0.0f;
    private bool is_active = true;
    public static event System.Action<Vector3> DeadEvent;

    [Header("�ړ����")]
    [SerializeField]
    [Range(0.1f, 10.0f)]
    [Tooltip("�v���C���[�Ƃ̋�����")]
    private float DISTANCE_FROM_PLAYER;

    [SerializeField]
    [Range(0.0f, 1.0f)]
    [Tooltip("�����̋��e��")]
    private float DISTANCE_ALLOW_RATE;

    [SerializeField]
    [Min(0.1f)]
    [Tooltip("�ړ����x")]
    private float MOVE_SPEED;

    [Header("�U�����")]
    [SerializeField]
    [Tooltip("���΂̋�")]
    private GameObject ATTACK_BALL_PREFAB;

    [SerializeField]
    [Min(0.1f)]
    [Tooltip("�΂̋ʂ̑��x")]
    private float SHOT_SPEED = 10.0f;

    [SerializeField]
    [Range(0.1f, 99.9f)]
    [Tooltip("�΂̋ʂ̔��ˊԊu")]
    private float SHOT_CYCLE_SEC = 2.0f;

    [SerializeField]
    [Range(0.0f, 1.0f)]
    [Tooltip("�΂̋ʂ̔��ˊԊu�̗�������")]
    private float SHOT_CYCLE_RANDOM_RATE;

    [SerializeField]
    [Tooltip("�΂̋ʔ��ˌ��ʉ�")]
    private AudioClip SHOT_SE;

    [SerializeField]
    [Range(0.0f,1.0f)]
    [Tooltip("�΂̋ʔ��ˌ��ʉ��{�����[��")]
    private float SHOT_VOLUME;

    [Header("���S�����")]
    [SerializeField]
    [Tooltip("���S���ɐ�������I�u�W�F�N�g")]
    private GameObject DEAD_OBJECT_PREFAB;

    [SerializeField]
    [Range(0, 10)]
    [Tooltip("���S���ɃI�u�W�F�N�g�𐶐����鐔")]
    private int DEAD_OBJECT_NUM = 10;

    [SerializeField]
    [Range(0.1f, 10.0f)]
    [Tooltip("���S���ɕ��o����I�u�W�F�N�g�̏����x")]
    private float DEAD_OBJECT_EMIT_SPEED = 10.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        const string PLAYER_TAG = "Player";
        PLAYER_GAMEOBJECT = GameObject.FindWithTag(PLAYER_TAG);
        SetEventAction();
        cnt_shot_cycle = CalcShotCycle();
    }

    float CalcShotCycle()
    {
        return SHOT_CYCLE_SEC * (1.0f + Random.Range(0.0f, SHOT_CYCLE_RANDOM_RATE));
    }

    void SetEventAction()
    {
        StageManager.TimeUp += SetActiveFalse;
        Player.GameOver += SetActiveFalse;
    }

    void SetActiveFalse()
    {
        is_active = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (is_active)
        {
            LookAtPlayer();
            ChasePlayer();
            cnt_shot_cycle -= Time.deltaTime;
            if (cnt_shot_cycle <= 0.0f)
            {
                ShotFireBall();
                cnt_shot_cycle += CalcShotCycle();
            }
        }
    }

    void LookAtPlayer()
    {
        Vector3 vector_to_player = PLAYER_GAMEOBJECT.transform.position - this.transform.position;
        Quaternion quaternion_to_player = Quaternion.LookRotation(vector_to_player);
        this.transform.rotation = quaternion_to_player;
        return;
    }

    void ChasePlayer()
    {
        float MIN_DISTANCE = DISTANCE_FROM_PLAYER * (1.0f - DISTANCE_ALLOW_RATE);
        float MAX_DISTANCE = DISTANCE_FROM_PLAYER * (1.0f + DISTANCE_ALLOW_RATE);
        Vector3 direction_to_player = new Vector3(
            PLAYER_GAMEOBJECT.transform.position.x - this.transform.position.x,
            0.0f,
            PLAYER_GAMEOBJECT.transform.position.z - this.transform.position.z
        ).normalized;
        float distance_from_player = Vector3.Distance(PLAYER_GAMEOBJECT.transform.position, this.transform.position);
        if (distance_from_player < MIN_DISTANCE)
        {
            direction_to_player *= -1.0f;
        }
        else if (distance_from_player > MAX_DISTANCE)
        {
            direction_to_player *= 1.0f;
        }
        else
        {
            direction_to_player *= 0.0f;
        }
        this.transform.position += direction_to_player * MOVE_SPEED * Time.deltaTime;
        return;
    }

    void ShotFireBall()
    {
        Vector3 SHOT_DIRECTION = (PLAYER_GAMEOBJECT.transform.position - this.transform.position).normalized;
        Vector3 shot_position = this.transform.position + SHOT_DIRECTION * 2.0f;
        GameObject fireball = Instantiate(ATTACK_BALL_PREFAB, shot_position, Quaternion.identity);
        Rigidbody fireball_rigidbody = fireball.GetComponent<Rigidbody>();
        Vector3 shot_velocity = SHOT_DIRECTION * SHOT_SPEED;
        fireball_rigidbody.linearVelocity = shot_velocity;
        PlaySE(SHOT_SE, SHOT_VOLUME);
        return;
    }
    void PlaySE(AudioClip se_clip, float base_volume)
    {
        AudioSource se_source = this.AddComponent<AudioSource>();
        se_source.clip = se_clip;
        se_source.volume = base_volume;
        const float BLEND_3D = 1.0f;
        se_source.spatialBlend = BLEND_3D;
        se_source.Play();
        Destroy(se_source, se_clip.length);
        return;
    }

    void OnCollisionEnter(Collision collision_object)
    {
        int FIREBALL_LAYER_INT = LayerMask.NameToLayer("Attack Ball");
        if ((collision_object.gameObject.layer == FIREBALL_LAYER_INT) && (!isDead))
        {
            isDead = true;
            Dead();
        }
        return;
    }

    void Dead()
    {
        EmitDeadObjects(this.transform.position);
        DeadEvent.Invoke(this.transform.position);
        Destroy(this.gameObject);
    }

    void EmitDeadObjects(Vector3 dead_position)
    {
        for (int i = 0; i < DEAD_OBJECT_NUM; i++)
        {
            Vector3 direction = new Vector3(
                Random.Range(-1.0f, 1.0f),
                Random.Range(-1.0f, 1.0f),
                Random.Range(-1.0f, 1.0f)
                ).normalized;
            GameObject deadobject = Instantiate(DEAD_OBJECT_PREFAB, dead_position+direction*0.1f, Quaternion.identity);
            Rigidbody deadobject_rigidbody = deadobject.GetComponent<Rigidbody>();
            Vector3 emit_velocity = direction * DEAD_OBJECT_EMIT_SPEED;
            deadobject_rigidbody.linearVelocity = emit_velocity;
        }
        return;
    }
}