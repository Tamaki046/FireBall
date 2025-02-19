using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    private Rigidbody player_rigidbody;
    private CharacterController character_controller;
    private Transform camera_transform;
    private Light player_light;
    private float shot_lest_time = 0.0f;
    private bool is_knockbacking = false;
    private bool is_hittable = true;
    private Vector3 knockback_velocity = Vector3.zero;
    private bool is_game_started = false;
    private bool is_game_finished = false;
    private float camera_slow_rate = 0.0f;
    public static event System.Action GameOver;

    [Header("�U�����")]
    [SerializeField]
    [Tooltip("���˂���΂̋ʂ̃v���n�u")]
    private GameObject fireball_prefab;
    
    [SerializeField]
    [Min(0.1f)]
    [Tooltip("�ʂ̔��ˑ��x")]
    private float SHOT_SPEED = 10.0f;

    [SerializeField]
    [Range(0.3f,2.0f)]
    [Tooltip("�ʂ̘A�ˊԊu")]
    private float SHOT_CYCLE = 0.1f;

    [SerializeField]
    [Tooltip("���ˎ��̌��ʉ�")]
    private AudioClip SHOT_SE;

    [SerializeField]
    [Range(0.0f, 1.0f)]
    [Tooltip("���ˎ��̌��ʉ��{�����[��")]
    private float SHOT_VOLUME;

    [Header("�C����")]
    [SerializeField]
    [Range(0.0f, 1.0f)]
    [Tooltip("�C�⎞�̃J�������x�ቺ�x")]
    private float FAINTING_CAMERA_SLOW_RATE = 0.1f;

    [SerializeField]
    [Tooltip("�C�⎞�̌��̐F")]
    private Color FAINTING_LIGHT_COLOR;

    [SerializeField]
    [Tooltip("��e���̌��ʉ�")]
    private AudioClip HIT_SE;

    [SerializeField]
    [Range(0.0f,99.0f)]
    [Tooltip("��e��̖��G����")]
    private float INVINCIBLE_SEC;

    [SerializeField]
    [Range(0.0f, 1.0f)]
    [Tooltip("��e���̌��ʉ��{�����[��")]
    private float HIT_VOLUME;

    [Header("���̑�")]
    [SerializeField]
    [Min(0.1f)]
    [Tooltip("�ړ����x")]
    private float MOVE_SPEED = 1.0f;

    [SerializeField]
    [Range(0.1f, 100.0f)]
    [Tooltip("�J�������x")]
    private float CAMERA_SENSITIVITY = 10.0f;

    [SerializeField]
    [Range(-20.0f,0.0f)]
    [Tooltip("��������ƂȂ�Y���W")]
    private float DROP_POSITION_Y;

    [SerializeField]
    [Tooltip("�������̌��ʉ�")]
    private AudioClip FALL_SE;

    [SerializeField]
    [Range(0.0f, 1.0f)]
    [Tooltip("�������̌��ʉ��{�����[��")]
    private float FALL_VOLUME;


    void Start()
    {
        player_rigidbody = this.GetComponent<Rigidbody>();
        player_rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        character_controller = this.GetComponent<CharacterController>();
        camera_transform = this.transform.Find("Main Camera").transform;
        player_light = this.transform.Find("Point Light").GetComponent<Light>();
        this.gameObject.transform.position = new Vector3(0.0f, 2.0f, 0.0f);
        ConnectEventAction(true);
    }

    void ConnectEventAction(bool connect_event)
    {
        if (connect_event)
        {
            StageManager.GameStart += StartGame;
            StageManager.TimeUp += SetActiveFalse;
            StageManager.LeaveScene += PrepareLeaveScene;
        }
        else
        {
            StageManager.GameStart -= StartGame;
            StageManager.TimeUp -= SetActiveFalse;
            StageManager.LeaveScene -= PrepareLeaveScene;
        }
        return;
    }

    void StartGame()
    {
        is_game_started = true;
        return;
    }

    void PrepareLeaveScene()
    {
        ConnectEventAction(false);
        return;
    }

    void SetActiveFalse()
    {
        is_game_finished = true; 
        player_rigidbody.linearVelocity = Vector3.zero;
        player_rigidbody.isKinematic = true;
        return;
    }

    void Update()
    {
        if (is_game_finished)
        {
            return;
        }
        else if (!is_game_started)
        {
            MoveOnlyGravity();
            return;
        }
        MoveCamera();
        if (is_knockbacking)
        {
            ShakeCamera();
            if (!character_controller.isGrounded)
            {
                knockback_velocity.y += Physics.gravity.y * Time.deltaTime;
            }
            else
            {
                knockback_velocity = Vector3.zero;
            }
            character_controller.Move(knockback_velocity * Time.deltaTime);
        }
        else
        {
            MoveCharacter();
            if (shot_lest_time > 0.0f)
            {
                shot_lest_time -= Time.deltaTime;
            }
            else
            {
                if (Input.GetButton("Fire1"))
                {
                    ShotFireball();
                    shot_lest_time = SHOT_CYCLE;
                }
            }
        }
        if (this.transform.position.y <= DROP_POSITION_Y)
        {
            PlaySE(FALL_SE, FALL_VOLUME);
            GameOver.Invoke();
            SetActiveFalse();
        }
    }

    void MoveCamera(){
        Vector2 mouse_input = new Vector2(
            Input.GetAxisRaw("Mouse X"),
            Input.GetAxisRaw("Mouse Y")
        ) * CAMERA_SENSITIVITY * (1.0f - camera_slow_rate);
        this.transform.rotation = Quaternion.Euler(
            this.transform.rotation.eulerAngles.x,
            this.transform.rotation.eulerAngles.y + mouse_input.x,
            this.transform.rotation.eulerAngles.z
            );
        camera_transform.rotation = Quaternion.Euler(
            camera_transform.rotation.eulerAngles.x - mouse_input.y,
            camera_transform.rotation.eulerAngles.y,
            camera_transform.rotation.eulerAngles.z
            );
        return;
    }

    void MoveCharacter(){
        Vector3 front_back_input = this.transform.forward * Input.GetAxis("Vertical");
        Vector3 right_left_input = this.transform.right * Input.GetAxis("Horizontal");
        Vector3 velocity = (front_back_input + right_left_input).normalized * MOVE_SPEED;
        if(!character_controller.isGrounded){
            velocity.y += Physics.gravity.y;
        }
        character_controller.Move(velocity * Time.deltaTime);
        return;
    }

    void MoveOnlyGravity()
    {
        Vector3 velocity = Vector3.zero;
        if (!character_controller.isGrounded)
        {
            velocity.y += Physics.gravity.y;
        }
        character_controller.Move(velocity * Time.deltaTime);
        return;
    }

    void ShotFireball(){
        Vector3 SHOT_DIRECTION = new Vector3(
            this.transform.forward.x * Mathf.Sqrt(Mathf.Cos(camera_transform.rotation.eulerAngles.x * Mathf.Deg2Rad)),
            - Mathf.Sin(camera_transform.rotation.eulerAngles.x * Mathf.Deg2Rad),
            this.transform.forward.z * Mathf.Sqrt(Mathf.Cos(camera_transform.rotation.eulerAngles.x * Mathf.Deg2Rad))
        ).normalized;
        Vector3 shot_position = this.transform.position + SHOT_DIRECTION;
        GameObject fireball = Instantiate(fireball_prefab, shot_position, Quaternion.identity);
        Rigidbody fireball_rigidbody = fireball.GetComponent<Rigidbody>();
        Vector3 shot_velocity = SHOT_DIRECTION * SHOT_SPEED;
        fireball_rigidbody.linearVelocity = shot_velocity;
        PlaySE(SHOT_SE,SHOT_VOLUME);
        return;
    }

    void PlaySE(AudioClip se_clip, float base_volume)
    {
        AudioSource se_source = this.AddComponent<AudioSource>();
        se_source.clip = se_clip;
        se_source.volume = base_volume;
        se_source.Play();
        Destroy(se_source, se_clip.length);
        return;
    }

    void OnCollisionEnter(Collision collision_object)
    {
        int ENEMY_BALL_LAYERS = LayerMask.NameToLayer("Enemy Ball");
        if (collision_object.gameObject.layer == ENEMY_BALL_LAYERS && is_hittable)
        {
            is_hittable = false;
            KnockBack(collision_object.transform.position - this.transform.position);
        }
    }

    void ShakeCamera()
    {
        float SHAKE_POWER = 0.1f;
        float shake_degree = Random.Range(0.0f,2.0f*Mathf.PI);
        Vector3 shake_direction = new Vector3(
            Mathf.Cos(shake_degree),
            Mathf.Sin(shake_degree),
            0.0f
        );
        camera_transform.localPosition = shake_direction * SHAKE_POWER;
        return;
    }

    void KnockBack(Vector3 hit_direction)
    {
        camera_slow_rate = FAINTING_CAMERA_SLOW_RATE;
        player_light.color = FAINTING_LIGHT_COLOR;
        is_knockbacking = true;
        float FORCE_POWER = 5.0f;
        Vector3 knockback_direction = new Vector3(
            - hit_direction.x,
            0.0f,
            - hit_direction.z
        ).normalized;
        knockback_direction.y = 1.0f;
        knockback_velocity = knockback_direction.normalized * FORCE_POWER;
        character_controller.Move(knockback_velocity * Time.deltaTime);
        const float KNOCKBACK_SEC = 2.0f;
        Invoke(nameof(FinishKnockBack), KNOCKBACK_SEC);
        PlaySE(HIT_SE,HIT_VOLUME);
        return;
    }

    void FinishKnockBack()
    {
        is_knockbacking = false;
        player_light.color = new Color(1, 1, 1);
        camera_transform.localPosition = Vector3.zero;
        camera_slow_rate = 0.0f;
        Invoke(nameof(FinishInvincible), INVINCIBLE_SEC);
        return;
    }

    void FinishInvincible()
    {
        is_hittable = true;
        return;
    }
}
