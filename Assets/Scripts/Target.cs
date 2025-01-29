using UnityEngine;
public class Target : MonoBehaviour
{

    private bool isDead = false;

    [SerializeField]
    [Tooltip("リスポーン座標の最小値")]
    private Vector3 MIN_POSITION = new Vector3(
        -10.0f,
        2.0f,
        -10.0f
    );
    [SerializeField]
    [Tooltip("リスポーン座標の最大値")]
    private Vector3 MAX_POSITION = new Vector3(
        10.0f,
        5.0f,
        10.0f
    );

    private GameObject PLAYER_GAMEOBJECT;

    [SerializeField]
    [Tooltip("プレイヤーとの距離感")]
    private float DISTANCE_FROM_PLAYER;

    [SerializeField]
    [Tooltip("距離の許容率")]
    private float DISTANCE_ALLOW_RATE;

    [SerializeField]
    [Tooltip("移動速度")]
    private float MOVE_SPEED;

    [SerializeField]
    [Tooltip("放つ火の玉")]
    private GameObject ATTACK_BALL_PREFAB;

    [SerializeField]
    [Tooltip("火の玉の速度")]
    private float SHOT_SPEED = 10.0f;

    private float cnt_shot_cycle = 0.0f;

    [SerializeField]
    [Tooltip("火の玉の発射間隔")]
    private float SHOT_CYCLE_SEC = 2.0f;

    [SerializeField]
    [Tooltip("死亡時に生成するオブジェクト")]
    private GameObject DEAD_OBJECT_PREFAB;

    [SerializeField]
    [Tooltip("死亡時にオブジェクトを生成する数")]
    private int DEAD_OBJECT_NUM = 10;

    [SerializeField]
    [Tooltip("死亡時に放出するオブジェクトの初速度")]
    private float DEAD_OBJECT_EMIT_SPEED = 10.0f;

    public static event System.Action DeadEvent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        const string PLAYER_TAG = "Player";
        PLAYER_GAMEOBJECT = GameObject.FindWithTag(PLAYER_TAG);
    }

    // Update is called once per frame
    void Update()
    {
        LookAtPlayer();
        ChasePlayer();
        cnt_shot_cycle += Time.deltaTime;
        if (cnt_shot_cycle >= SHOT_CYCLE_SEC)
        {
            cnt_shot_cycle -= SHOT_CYCLE_SEC;
            ShotFireBall();
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
        }else if (distance_from_player > MAX_DISTANCE)
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
        Vector3 shot_position = this.transform.position + SHOT_DIRECTION*2.0f;
        GameObject fireball = Instantiate(ATTACK_BALL_PREFAB, shot_position, this.transform.rotation);
        Rigidbody fireball_rigidbody = fireball.GetComponent<Rigidbody>();
        Vector3 shot_velocity = SHOT_DIRECTION * SHOT_SPEED;
        fireball_rigidbody.linearVelocity = shot_velocity;
        return;
    }

    Vector3 GenerateRandomPosition()
    {
        return new Vector3(
            Random.Range(MIN_POSITION.x, MAX_POSITION.x),
            Random.Range(MIN_POSITION.y, MAX_POSITION.y),
            Random.Range(MIN_POSITION.z, MAX_POSITION.z)
        );
    }

    void OnCollisionEnter(Collision collision_object)
    {
        int FIREBALL_LAYER_INT = LayerMask.NameToLayer("Attack Ball");
        if((collision_object.gameObject.layer == FIREBALL_LAYER_INT) && (!isDead))
        {
            isDead = true;
            Dead();
        }
        return;
    }

    void Dead()
    {
        EmitDeadObjects();
        DeadEvent.Invoke();
        Destroy(this.gameObject);
    }

    void EmitDeadObjects()
    {
        for(int i = 0; i < DEAD_OBJECT_NUM; i++)
        {
            Vector3 direction = new Vector3(
                Random.Range(-1.0f, 1.0f),
                Random.Range(-1.0f, 1.0f),
                Random.Range(-1.0f, 1.0f)
                ).normalized;
            GameObject deadobject = Instantiate(DEAD_OBJECT_PREFAB, this.transform.position, this.transform.rotation);
            Rigidbody deadobject_rigidbody = deadobject.GetComponent<Rigidbody>();
            Vector3 emit_velocity = direction * DEAD_OBJECT_EMIT_SPEED;
            deadobject_rigidbody.linearVelocity = emit_velocity;
        }
        return;
    }
}
