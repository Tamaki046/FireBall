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

    [Header("移動情報")]
    [SerializeField]
    [Range(0.1f, 10.0f)]
    [Tooltip("プレイヤーとの距離感")]
    private float DISTANCE_FROM_PLAYER;

    [SerializeField]
    [Range(0.0f, 1.0f)]
    [Tooltip("距離の許容率")]
    private float DISTANCE_ALLOW_RATE;

    [SerializeField]
    [Min(0.1f)]
    [Tooltip("移動速度")]
    private float MOVE_SPEED;

    [Header("攻撃情報")]
    [SerializeField]
    [Tooltip("放つ火の玉")]
    private GameObject ATTACK_BALL_PREFAB;

    [SerializeField]
    [Min(0.1f)]
    [Tooltip("火の玉の速度")]
    private float SHOT_SPEED = 10.0f;

    [SerializeField]
    [Range(0.1f, 99.9f)]
    [Tooltip("火の玉の発射間隔")]
    private float SHOT_CYCLE_SEC = 2.0f;

    [SerializeField]
    [Range(0.0f, 1.0f)]
    [Tooltip("火の玉の発射間隔の乱数加数")]
    private float SHOT_CYCLE_RANDOM_RATE;

    [SerializeField]
    [Tooltip("火の玉発射効果音")]
    private AudioClip SHOT_SE;

    [SerializeField]
    [Range(0.0f,1.0f)]
    [Tooltip("火の玉発射効果音ボリューム")]
    private float SHOT_VOLUME;

    [Header("死亡時情報")]
    [SerializeField]
    [Tooltip("死亡時に生成するオブジェクト")]
    private GameObject DEAD_OBJECT_PREFAB;

    [SerializeField]
    [Range(0, 10)]
    [Tooltip("死亡時にオブジェクトを生成する数")]
    private int DEAD_OBJECT_NUM = 10;

    [SerializeField]
    [Range(0.1f, 10.0f)]
    [Tooltip("死亡時に放出するオブジェクトの初速度")]
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