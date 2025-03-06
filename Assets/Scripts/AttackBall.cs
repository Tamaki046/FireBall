using System.Linq;
using UnityEngine;

//TODO：多重継承発生中
public class AttackBall : AttackObject
{
    private float cnt_flare_spark_cycle;

    [SerializeField, Tooltip("火の粉のプレハブ")]
    private GameObject attack_spark_prefab;

    [SerializeField, Tooltip("火の粉最大発射間隔"), Range(0.1f,1.0f)]
    private float MAX_FLARE_SPARK_CYCLE = 0.3f;

    [SerializeField, Tooltip("火の粉の発射速度"), Min(0.1f)]
    private float SPARK_SPEED = 3.0f;

    [SerializeField, Tooltip("火の粉が出ない秒数"), Min(0.1f)]
    private float no_spark_sec = 0.2f;


    protected override void UpdateOnReady()
    {
        base.UpdateOnReady();

        if (no_spark_sec > 0.0f)
        {
            no_spark_sec -= Time.deltaTime;
        }
        else
        {
            cnt_flare_spark_cycle += Time.deltaTime;
            float r = Random.value;
            // 3乗オーダーぐらいが難易度的に良かった印象？
            if (r <= Mathf.Pow(cnt_flare_spark_cycle / MAX_FLARE_SPARK_CYCLE, 3))
            {
                SpawnFireSpark();
                cnt_flare_spark_cycle = 0.0f;
            }
            if (this.transform.position.y <= -10.0f)
            {
                base.DestroyThisGameObject();
            }
        }
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

    private void SpawnFireSpark(){
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
