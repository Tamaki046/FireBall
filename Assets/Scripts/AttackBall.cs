using System.Linq;
using UnityEngine;

public class AttackBall : AttackObject
{
    private Vector3 core_rotation_velocity;
    private float cnt_flare_spark_cycle;

    [SerializeField]
    [Tooltip("火の粉のプレハブ")]
    private GameObject attack_spark_prefab;

    [SerializeField]
    [Range(0.1f,1.0f)]
    [Tooltip("火の粉最大発射間隔")]
    private float MAX_FLARE_SPARK_CYCLE = 0.3f;

    [SerializeField]
    [Min(0.1f)]
    [Tooltip("火の粉の発射速度")]
    private float SPARK_SPEED = 3.0f;

    [SerializeField]
    [Min(0.1f)]
    [Tooltip("火の粉が出ない秒数")]
    private float no_spark_sec = 0.2f;

    protected override void Start()
    {
        base.Start();
        const float MAX_ROTATION_SPEED = 50.0f;
        core_rotation_velocity = new Vector3(
                                    Random.Range(-1.0f, 1.0f),
                                    Random.Range(-1.0f, 1.0f),
                                    Random.Range(-1.0f, 1.0f)
                                    ).normalized * MAX_ROTATION_SPEED;
        //PlaySE()
        return;
    }

    void PlaySE(AudioClip se_clip, Vector3 se_position, float se_volume, bool is_3d)
    {
        GameObject audio_object = new GameObject("TempAudioSource");
        audio_object.transform.position = se_position;

        AudioSource audio_source = audio_object.AddComponent<AudioSource>();
        audio_source.clip = se_clip;
        audio_source.volume = se_volume * PlayerPrefs.GetFloat("SEVolume");
        float blend_3d = 0.0f;
        if (is_3d)
        {
            blend_3d = 1.0f;
        }
        audio_source.spatialBlend = blend_3d;
        audio_source.Play();

        Destroy(audio_object, se_clip.length);
        return;
    }

    protected override void Update()
    {
        base.Update();
        this.transform.localEulerAngles += core_rotation_velocity * Time.deltaTime;
        if (!is_active)
        {
            return;
        }
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
                base.DestroyThisGameObject();
            }
        }
        return;
    }

    protected override void OnCollisionEnter(Collision collision_object)
    {
        int[] NOT_DESTROY_LAYERS = {
            LayerMask.NameToLayer("Wall")
            };
        if (!NOT_DESTROY_LAYERS.Contains(collision_object.gameObject.layer))
        {
            int TARGET_LAYER = LayerMask.NameToLayer("Target");
            if(collision_object.gameObject.layer != TARGET_LAYER)
            {
                base.BreakField(this.transform.position);
            }
            base.DestroyThisGameObject();
        }
        return;
    }

    void SpawnFireSpark(){
        Vector3 shot_position = this.transform.position;
        GameObject firespark = Instantiate(attack_spark_prefab, shot_position, Quaternion.identity);
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
