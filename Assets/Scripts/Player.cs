using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody player_rigidbody;
    private CharacterController character_controller;
    private Transform camera_transform;
    private float shot_lest_time = 0.0f;

    [SerializeField]
    [Tooltip("発射する火の玉のプレハブ")]
    private GameObject fireball_prefab;
    
    [SerializeField]
    [Tooltip("移動速度")]
    private float MOVE_SPEED = 1.0f;
    
    [SerializeField]
    [Tooltip("玉の発射速度")]
    private float SHOT_SPEED = 10.0f;

    [SerializeField]
    [Tooltip("玉の連射間隔")]
    private float SHOT_CYCLE = 0.1f;

    [SerializeField]
    [Tooltip("カメラ感度")]
    private float CAMERA_SENSITIVITY = 10.0f;

    private bool is_knockbacking = false;
    private Vector3 knockback_velocity = Vector3.zero;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player_rigidbody = this.GetComponent<Rigidbody>();
        player_rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        character_controller = this.GetComponent<CharacterController>();
        camera_transform = this.transform.Find("Main Camera").transform;
    }

    // Update is called once per frame
    void Update()
    {
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
            MoveCamera();

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
    }

    void MoveCamera(){
        Vector2 mouse_input = new Vector2(
            Input.GetAxisRaw("Mouse X"),
            Input.GetAxisRaw("Mouse Y")
        ) * CAMERA_SENSITIVITY;
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

    //RigidBodyだけで実装（若干カクつく）
    /*
    void MoveCharacter()
    {
        Vector3 front_back_input = this.transform.forward * Input.GetAxis("Vertical");
        Vector3 right_left_input = this.transform.right * Input.GetAxis("Horizontal");
        Vector3 velocity = (front_back_input + right_left_input).normalized * MOVE_SPEED;
        velocity.y = player_rigidbody.linearVelocity.y;
        player_rigidbody.linearVelocity = velocity;
        return;
    }
    */

    void ShotFireball(){
        Vector3 SHOT_DIRECTION = new Vector3(
            this.transform.forward.x * Mathf.Sqrt(Mathf.Cos(camera_transform.rotation.eulerAngles.x * Mathf.Deg2Rad)),
            - Mathf.Sin(camera_transform.rotation.eulerAngles.x * Mathf.Deg2Rad),
            this.transform.forward.z * Mathf.Sqrt(Mathf.Cos(camera_transform.rotation.eulerAngles.x * Mathf.Deg2Rad))
        ).normalized;
        Vector3 shot_position = this.transform.position + SHOT_DIRECTION;
        GameObject fireball = Instantiate(fireball_prefab, shot_position, this.transform.rotation);
        Rigidbody fireball_rigidbody = fireball.GetComponent<Rigidbody>();
        Vector3 shot_velocity = SHOT_DIRECTION * SHOT_SPEED;
        fireball_rigidbody.linearVelocity = shot_velocity;
        return;
    }

    void OnCollisionEnter(Collision collision_object)
    {
        int[] COLLISION_LAYERS = {
            LayerMask.NameToLayer("Attack Ball")
        };
        if (COLLISION_LAYERS.Contains(collision_object.gameObject.layer))
        {
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
        Debug.Log("KnockBack!");
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
        return;
    }

    void FinishKnockBack()
    {
        is_knockbacking = false;
        camera_transform.localPosition = Vector3.zero;
        return;
    }
}
