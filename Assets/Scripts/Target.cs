using Unity.VisualScripting;
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

    private const string SE_PREFS_KEY = "SEVolume";

    [Header("移動情報")]
    [Tooltip("プレイヤーとの距離感"), Range(0.1f, 10.0f)]
    [SerializeField]
    private float DISTANCE_FROM_PLAYER;

    [Tooltip("距離の許容率"), Range(0.0f, 1.0f)]
    [SerializeField]
    private float DISTANCE_ALLOW_RATE;

    [Tooltip("移動速度"), Min(0.1f)]
    [SerializeField]
    private float MOVE_SPEED;


    [Header("攻撃情報")]
    [Tooltip("放つ火の玉のプレハブ")]
    [SerializeField]
    private GameObject ATTACK_BALL_PREFAB;

    [Tooltip("火の玉の速度"), Min(0.1f)]
    [SerializeField]
    private float SHOT_SPEED = 10.0f;

    [Tooltip("火の玉の発射間隔"), Range(0.1f, 99.9f)]
    [SerializeField]
    private float SHOT_CYCLE_SEC = 2.0f;

    [Tooltip("火の玉の発射間隔の乱数率"), Range(0.0f, 1.0f)]
    [SerializeField]
    private float SHOT_CYCLE_RANDOM_RATE;

    [Tooltip("火の玉発射効果音")]
    [SerializeField]
    private AudioClip SHOT_SE_CLIP;

    [Tooltip("火の玉発射効果音ボリューム"), Range(0.0f,1.0f)]
    [SerializeField]
    private float SHOT_SE_BASE_VOLUME;


    [Header("死亡時情報")]
    [Tooltip("死亡時に生成するオブジェクトのプレハブ")]
    [SerializeField]
    private GameObject DEAD_OBJECT_PREFAB;

    [Tooltip("死亡時にオブジェクトを生成する数"), Range(0, 10)]
    [SerializeField]
    private int DEAD_OBJECT_NUM = 10;

    [Tooltip("死亡時に放出するオブジェクトの初速度"), Range(0.1f, 10.0f)]
    [SerializeField]
    private float DEAD_OBJECT_EMIT_SPEED = 10.0f;

    [Tooltip("死亡時に放出するパーティクルプレハブ")]
    [SerializeField]
    private GameObject DEAD_PARTICLE;


    private void Start()
    {
        const string PLAYER_TAG = "Player";
        PLAYER_GAMEOBJECT = GameObject.FindWithTag(PLAYER_TAG);
        ConnectEventAction(true);
        cnt_shot_cycle = CalcShotCycle();
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
        return;
    }

    private float CalcShotCycle()
    {
        return SHOT_CYCLE_SEC * (1.0f + Random.Range(0.0f, SHOT_CYCLE_RANDOM_RATE));
    }


    private void Update()
    {
        const float STOP_TIME_BORDER = 0.5f;
        if (Time.timeScale < STOP_TIME_BORDER)
        {
            return;
        }
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

    private void LookAtPlayer()
    {
        Vector3 vector_to_player = PLAYER_GAMEOBJECT.transform.position - this.transform.position;
        Quaternion quaternion_to_player = Quaternion.LookRotation(vector_to_player);
        this.transform.rotation = quaternion_to_player;
        return;
    }

    private void ChasePlayer()
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

    private void ShotFireBall()
    {
        Vector3 player_foot_position = new Vector3(
            PLAYER_GAMEOBJECT.transform.position.x,
            0.5f,
            PLAYER_GAMEOBJECT.transform.position.z
            );
        Vector3 SHOT_DIRECTION = (player_foot_position - this.transform.position).normalized;
        Vector3 shot_position = this.transform.position + SHOT_DIRECTION * 2.0f;
        GameObject fireball = Instantiate(ATTACK_BALL_PREFAB, shot_position, Quaternion.identity);
        Rigidbody fireball_rigidbody = fireball.GetComponent<Rigidbody>();
        Vector3 shot_velocity = SHOT_DIRECTION * SHOT_SPEED;
        fireball_rigidbody.linearVelocity = shot_velocity;
        PlaySE(SHOT_SE_CLIP, SHOT_SE_BASE_VOLUME);
        return;
    }


    private void PrepareLeaveScene()
    {
        ConnectEventAction(false);
        return;
    }

    private void SetActiveFalse()
    {
        is_active = false;
        return;
    }


    private void OnCollisionEnter(Collision collision_object)
    {
        int FIREBALL_LAYER_INT = LayerMask.NameToLayer("Attack Ball");
        if ((collision_object.gameObject.layer == FIREBALL_LAYER_INT) && (!isDead))
        {
            isDead = true;
            Dead();
        }
        return;
    }

    private void Dead()
    {
        EmitDeadObjects(this.transform.position);
        DeadEvent.Invoke(this.transform.position);
        Destroy(this.gameObject);
    }

    private void EmitDeadObjects(Vector3 dead_position)
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
        Instantiate(DEAD_PARTICLE, dead_position, Quaternion.identity);
        return;
    }


    private void PlaySE(AudioClip se_clip, float base_volume)
    {
        AudioSource se_source = this.AddComponent<AudioSource>();
        se_source.clip = se_clip;
        se_source.volume = base_volume * PlayerPrefs.GetFloat(SE_PREFS_KEY);
        const float BLEND_3D = 1.0f;
        se_source.spatialBlend = BLEND_3D;
        se_source.Play();
        Destroy(se_source, se_clip.length);
        return;
    }
}