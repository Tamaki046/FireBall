using System.Linq;
using UnityEngine;

//TODO�F���d�p��������
public class AttackBall : AttackObject
{
    private float cnt_flare_spark_cycle;

    [SerializeField, Tooltip("�΂̕��̃v���n�u")]
    private GameObject attack_spark_prefab;

    [SerializeField, Tooltip("�΂̕��ő唭�ˊԊu"), Range(0.1f,1.0f)]
    private float MAX_FLARE_SPARK_CYCLE = 0.3f;

    [SerializeField, Tooltip("�΂̕��̔��ˑ��x"), Min(0.1f)]
    private float SPARK_SPEED = 3.0f;

    [SerializeField, Tooltip("�΂̕����o�Ȃ��b��"), Min(0.1f)]
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
            // 3��I�[�_�[���炢����Փx�I�ɗǂ�������ہH
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
