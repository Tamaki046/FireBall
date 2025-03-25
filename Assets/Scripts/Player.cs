using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Player : GameObjectBase
{
    private Rigidbody player_rigidbody;
    private CharacterController character_controller;
    private Transform camera_transform;
    private Light player_light;
    private float shot_lest_time = 0.0f;
    private bool is_knockbacking = false;
    private bool is_hittable = true;
    private Vector3 knockback_velocity = Vector3.zero;
    public static event System.Action GameOver;

    private const string PLAYER_CAMERA_TAG = "PlayerCamera";
    private const string PLAYER_LIGHT_TAG = "PlayerLight";

    private const string SE_PREFS_KEY = "SEVolume";
    private const string CAMERA_PREFS_KEY = "CameraSensitivity";

    [Header("攻撃情報")]
    [Tooltip("発射する火の玉のプレハブ")]
    [SerializeField]
    private GameObject FIREBALL_PREFAB;
    
    [Tooltip("玉の発射速度"), Min(0.1f)]
    [SerializeField]
    private float SHOT_SPEED = 10.0f;

    [Tooltip("玉の連射間隔"), Range(0.3f,2.0f)]
    [SerializeField]
    private float SHOT_CYCLE = 0.1f;

    [Tooltip("発射時の効果音ファイル")]
    [SerializeField]
    private AudioClip SHOT_SE_CLIP;

    [Tooltip("発射時の効果音ボリューム"), Range(0.0f, 1.0f)]
    [SerializeField]
    private float SHOT_SE_BASE_VOLUME;

    [Header("気絶情報")]
    [Tooltip("気絶時のカメラ感度"), Range(0.0f, 1.0f)]
    [SerializeField]
    private float FAINTING_CAMERA_SLOW_SENSITIVITY = 0.1f;

    [Tooltip("気絶時の光の色")]
    [SerializeField]
    private Color FAINTING_LIGHT_COLOR;

    [Tooltip("被弾時の効果音")]
    [SerializeField]
    private AudioClip HIT_SE_CLIP;

    [Tooltip("被弾時の効果音ボリューム"), Range(0.0f, 1.0f)]
    [SerializeField]
    private float HIT_SE_BASE_VOLUME;

    [Tooltip("被弾後の無敵時間"), Range(0.0f,99.0f)]
    [SerializeField]
    private float INVINCIBLE_SEC;


    [Header("その他")]
    [Tooltip("移動速度"), Min(0.1f)]
    [SerializeField]
    private float MOVE_SPEED = 1.0f;

    [Tooltip("カメラ感度"), Range(0.1f, 100.0f)]
    [SerializeField]
    private float CAMERA_SENSITIVITY = 10.0f;

    [Tooltip("落下判定となるY座標"), Range(-20.0f,0.0f)]
    [SerializeField]
    private float DROP_POSITION_Y;

    [Tooltip("落下時の効果音")]
    [SerializeField]
    private AudioClip FALL_SE_CLIP;

    [Tooltip("落下時の効果音ボリューム"), Range(0.0f, 1.0f)]
    [SerializeField]
    private float FALL_SE_BASE_VOLUME;


    private void Start()
    {
        SetupGameObjectsAndComponents();
        ConnectEventAction(true);
        this.gameObject.transform.position = new Vector3(0.0f, 2.0f, 0.0f);
        base.TransitionToReady();
    }

    private void SetupGameObjectsAndComponents()
    {
        player_rigidbody = this.GetComponent<Rigidbody>();
        player_rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        character_controller = this.GetComponent<CharacterController>();
        camera_transform = GameObject.FindWithTag(PLAYER_CAMERA_TAG).transform;
        player_light = GameObject.FindWithTag(PLAYER_LIGHT_TAG).GetComponent<Light>();
        return;
    }

    private void ConnectEventAction(bool is_connect_event)
    {
        if (is_connect_event)
        {
            StageManager.GameStart += StartGame;
            StageManager.GameStop += SetActiveFalse;
            StageManager.LeaveScene += PrepareLeaveScene;
        }
        else
        {
            StageManager.GameStart -= StartGame;
            StageManager.GameStop -= SetActiveFalse;
            StageManager.LeaveScene -= PrepareLeaveScene;
        }
        return;
    }


    protected override void UpdateOnReady()
    {
        MoveOnlyGravity();
        return;
    }

    private void MoveOnlyGravity()
    {
        Vector3 velocity = Vector3.zero;
        if (!character_controller.isGrounded)
        {
            velocity.y += Physics.gravity.y;
        }
        character_controller.Move(velocity * Time.deltaTime);
        return;
    }


    protected override void UpdateOnActing()
    {
        MoveCamera();

        if (is_knockbacking)
        {
            ShakeCamera();
            MoveByKnockBack();
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
            PlaySE(FALL_SE_CLIP, FALL_SE_BASE_VOLUME);
            GameOver.Invoke();
            SetActiveFalse();
        }
    }

    private void MoveByKnockBack()
    {
        if (!character_controller.isGrounded)
        {
            knockback_velocity.y += Physics.gravity.y * Time.deltaTime;
        }
        else
        {
            knockback_velocity = Vector3.zero;
        }
        character_controller.Move(knockback_velocity * Time.deltaTime);
        return;
    }

    private void MoveCharacter(){
        Vector3 front_back_input = this.transform.forward * Input.GetAxis("Vertical");
        Vector3 right_left_input = this.transform.right * Input.GetAxis("Horizontal");
        Vector3 velocity = (front_back_input + right_left_input).normalized * MOVE_SPEED;
        if(!character_controller.isGrounded){
            velocity.y += Physics.gravity.y;
        }
        character_controller.Move(velocity * Time.deltaTime);
        return;
    }

    private void CauseKnockBack(Vector3 hit_direction)
    {
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
        PlaySE(HIT_SE_CLIP,HIT_SE_BASE_VOLUME);
        return;
    }

    private void FinishKnockBack()
    {
        is_knockbacking = false;
        player_light.color = new Color(1, 1, 1);
        camera_transform.localPosition = Vector3.zero;
        Invoke(nameof(FinishInvincible), INVINCIBLE_SEC);
        return;
    }

    private void FinishInvincible()
    {
        is_hittable = true;
        return;
    }


    private void MoveCamera(){
        Vector2 mouse_input = new Vector2(
            Input.GetAxisRaw("Mouse X"),
            Input.GetAxisRaw("Mouse Y")
        );
        if (is_knockbacking)
        {
            mouse_input *= FAINTING_CAMERA_SLOW_SENSITIVITY;
        }
        else
        {
            mouse_input *= CAMERA_SENSITIVITY * PlayerPrefs.GetFloat(CAMERA_PREFS_KEY, 1.0f);
        }
        this.transform.rotation = Quaternion.Euler(
            this.transform.rotation.eulerAngles.x,
            this.transform.rotation.eulerAngles.y + mouse_input.x,
            this.transform.rotation.eulerAngles.z
            );
        const float MAX_ANGLE = 80.0f;
        camera_transform.rotation = Quaternion.Euler(
            ClampEulerAngles(camera_transform.rotation.eulerAngles.x - mouse_input.y, MAX_ANGLE, MAX_ANGLE),
            camera_transform.rotation.eulerAngles.y,
            camera_transform.rotation.eulerAngles.z
            );
        return;
    }

    private float ClampEulerAngles(float positive_angle, float up_max_angle, float down_max_angle)
    {
        float clamped_angles = positive_angle;
        if(clamped_angles <= 180.0f)
        {
            if(clamped_angles > down_max_angle)
            {
                clamped_angles = down_max_angle;
            }
        }
        else
        {
            if(clamped_angles < 360.0f - up_max_angle)
            {
                clamped_angles = 360.0f - up_max_angle;
            }
        }
        return clamped_angles;
    }

    private void ShakeCamera()
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


    private void ShotFireball(){
        Vector3 SHOT_DIRECTION = new Vector3(
            this.transform.forward.x * Mathf.Sqrt(Mathf.Cos(camera_transform.rotation.eulerAngles.x * Mathf.Deg2Rad)),
            - Mathf.Sin(camera_transform.rotation.eulerAngles.x * Mathf.Deg2Rad),
            this.transform.forward.z * Mathf.Sqrt(Mathf.Cos(camera_transform.rotation.eulerAngles.x * Mathf.Deg2Rad))
        ).normalized;
        Vector3 shot_position = this.transform.position + SHOT_DIRECTION;
        GameObject fireball = Instantiate(FIREBALL_PREFAB, shot_position, Quaternion.identity);
        Rigidbody fireball_rigidbody = fireball.GetComponent<Rigidbody>();
        Vector3 shot_velocity = SHOT_DIRECTION * SHOT_SPEED;
        fireball_rigidbody.linearVelocity = shot_velocity;
        PlaySE(SHOT_SE_CLIP,SHOT_SE_BASE_VOLUME);
        return;
    }


    private void StartGame()
    {
        base.TransitionToActing();
        return;
    }

    private void PrepareLeaveScene()
    {
        ConnectEventAction(false);
        return;
    }

    private void SetActiveFalse()
    {
        base.TransitionToFinished();
        player_rigidbody.linearVelocity = Vector3.zero;
        player_rigidbody.isKinematic = true;
        return;
    }


    private void OnCollisionEnter(Collision collision_object)
    {
        int ENEMY_BALL_LAYERS = LayerMask.NameToLayer("Enemy Ball");
        if (collision_object.gameObject.layer == ENEMY_BALL_LAYERS && is_hittable)
        {
            is_hittable = false;
            CauseKnockBack(collision_object.transform.position - this.transform.position);
        }
    }


    private void PlaySE(AudioClip se_clip, float base_volume)
    {
        AudioSource se_source = this.AddComponent<AudioSource>();
        se_source.clip = se_clip;
        se_source.volume = base_volume * PlayerPrefs.GetFloat(SE_PREFS_KEY, 1.0f);
        se_source.Play();
        Destroy(se_source, se_clip.length);
        return;
    }
}
