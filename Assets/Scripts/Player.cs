using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody rigidbody;
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



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidbody = this.GetComponent<Rigidbody>();
        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        character_controller = this.GetComponent<CharacterController>();
        camera_transform = this.transform.Find("Main Camera").transform;
    }

    // Update is called once per frame
    void Update()
    {
        MoveCamera();

        MoveCharacter();

        if (shot_lest_time > 0.0f)
        {
            shot_lest_time -= Time.deltaTime;
        }
        else
        {
            //const int MOUSE_LEFT_BUTTON_ID = 0;
            //if(Input.GetMouseButtonDown(MOUSE_LEFT_BUTTON_ID)){
            if (Input.GetButton("Fire1"))
            {
                ShotFireball();
                shot_lest_time = SHOT_CYCLE;
            }
        }
    }

    void MoveCamera(){
        Vector2 mouse_input = new Vector2(
            Input.GetAxisRaw("Mouse X"),
            Input.GetAxisRaw("Mouse Y")
        ) * CAMERA_SENSITIVITY;
        this.transform.rotation = Quaternion.Euler(
            transform.rotation.eulerAngles.x,
            transform.rotation.eulerAngles.y + mouse_input.x,
            transform.rotation.eulerAngles.z
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
}
