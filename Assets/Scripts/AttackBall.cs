using System.Linq;
using UnityEngine;

public class FireBall : MonoBehaviour
{
    private Vector3 core_rotation_velocity;
    private Vector3 flare_rotation_velocity;
    private float cnt_flare_spark_cycle;

    [SerializeField]
    [Tooltip("�ʂ̎c������")]
    private float attack_ball_lifetime_sec = 1.5f;

    [SerializeField]
    [Tooltip("�΂̕��̃v���n�u")]
    private GameObject attack_spark_prefab;

    [SerializeField]
    [Tooltip("�΂̕��ő唭�ˊԊu")]
    private float MAX_FLARE_SPARK_CYCLE = 0.3f;

    [SerializeField]
    [Tooltip("�΂̕��̔��ˑ��x")]
    private float SPARK_SPEED = 3.0f;

    [SerializeField]
    [Tooltip("�΂̕����o�Ȃ��b��")]
    private float no_spark_sec = 0.2f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        const float MAX_ROTATION_SPEED = 50.0f;
        core_rotation_velocity = new Vector3(
                                    Random.Range(-1.0f, 1.0f),
                                    Random.Range(-1.0f, 1.0f),
                                    Random.Range(-1.0f, 1.0f)
                                    ).normalized * MAX_ROTATION_SPEED;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.localEulerAngles += core_rotation_velocity * Time.deltaTime;
        if (no_spark_sec > 0.0f)
        {
            no_spark_sec -= Time.deltaTime;
        }
        else
        {
            cnt_flare_spark_cycle += Time.deltaTime;
            float r = Random.value;
            if(r <= Mathf.Pow(cnt_flare_spark_cycle/MAX_FLARE_SPARK_CYCLE,3)){
                SpawnFireSpark();
                cnt_flare_spark_cycle = 0.0f;
            }
            if(this.transform.position.y <= -10){
                Destroy(this.gameObject);
            }
        }
        attack_ball_lifetime_sec -= Time.deltaTime;
        if(attack_ball_lifetime_sec <= 0.0f)
        {
            Destroy(this.gameObject);
        }
    }

    void OnCollisionEnter(Collision collision_object)
    {
        int[] COLLISION_LAYERS = {
            LayerMask.NameToLayer("Field"),
            LayerMask.NameToLayer("Target"),
            LayerMask.NameToLayer("Player")
        };
        if(COLLISION_LAYERS.Contains(collision_object.gameObject.layer)){
            Destroy(this.gameObject);
        }
    }

    void SpawnFireSpark(){
        Vector3 shot_position = this.transform.position;
        GameObject firespark = Instantiate(attack_spark_prefab, shot_position, this.transform.rotation);
        Rigidbody firespark_rigidbody = firespark.GetComponent<Rigidbody>();
        Vector3 shot_velocity = new Vector3(
            Random.Range(-1.0f,1.0f),
            Random.Range(-1.0f,1.0f),
            Random.Range(-1.0f,1.0f)
        ).normalized * SPARK_SPEED;
        firespark_rigidbody.linearVelocity = shot_velocity;
        return;
    }
}
