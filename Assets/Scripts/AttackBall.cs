using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

// TODO：AttackParticleクラスと近い処理
//     　上手くオーバーライドとかを使えばまとめられる可能性がある
public class AttackBall : GameObjectBase
{
    public static event System.Action<Vector3, float> BreakEvent;
    private float cnt_flare_spark_cycle;

    [Tooltip("残存時間"), Range(0.5f, 3.0f)]
    [SerializeField]
    private float object_lifetime_sec = 1.5f;

    [Tooltip("破壊半径"), Range(0.1f, 10.0f)]
    [SerializeField]
    private float BREAK_RADIUS;

    [SerializeField, Tooltip("火の粉のプレハブ")]
    private GameObject attack_spark_prefab;

    [SerializeField, Tooltip("火の粉最大発射間隔"), Range(0.1f,1.0f)]
    private float MAX_FLARE_SPARK_CYCLE = 0.3f;

    [SerializeField, Tooltip("火の粉の発射速度"), Min(0.1f)]
    private float SPARK_SPEED = 3.0f;

    [SerializeField, Tooltip("火の粉が出ない秒数"), Min(0.1f)]
    private float no_spark_sec = 0.2f;

    private void Start()
    {
        ConnectEventAction(true);
        base.TransitionToActing();
        return;
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
    }


    protected override void UpdateOnActing()
    {
        if (this.transform.position.y <= -10.0f)
        {
            DestroyThisGameObject();
        }

        object_lifetime_sec -= Time.deltaTime;
        if (object_lifetime_sec <= 0.0f)
        {
            DestroyThisGameObject();
        }

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
                DestroyThisGameObject();
            }
        }
    }

    private void SpawnFireSpark()
    {
        Vector3 shot_position = this.transform.position;
        GameObject firespark = Instantiate(attack_spark_prefab, shot_position, Quaternion.identity);
        Rigidbody firespark_rigidbody = firespark.GetComponent<Rigidbody>();
        Vector3 shot_velocity = new Vector3(
            Random.Range(-1.0f, 1.0f),
            Random.Range(-1.0f, 1.0f),
            Random.Range(-1.0f, 1.0f)
        ).normalized * SPARK_SPEED;
        firespark_rigidbody.linearVelocity = shot_velocity;
        return;
    }

    private void OnCollisionEnter(Collision collision_object)
    {
        int[] NOT_DESTROY_LAYERS = {
            LayerMask.NameToLayer("Wall")
            };
        if (!NOT_DESTROY_LAYERS.Contains(collision_object.gameObject.layer))
        {
            int TARGET_LAYER = LayerMask.NameToLayer("Target");
            if(collision_object.gameObject.layer != TARGET_LAYER && base.state == States.ACTING)
            {
                BreakField(this.transform.position);
            }
            DestroyThisGameObject();
        }
        return;
    }

    private void BreakField(Vector3 break_position)
    {
        BreakEvent.Invoke(break_position, BREAK_RADIUS);
        return;
    }

    private void DestroyThisGameObject()
    {
        PrepareLeaveScene();
        Destroy(this.gameObject);
        return;
    }


    private void PrepareLeaveScene()
    {
        ConnectEventAction(false);
        return;
    }

    private void SetActiveFalse()
    {
        // オブジェクトの破壊とイベントの実行の順序が前後する場合があるので、
        // 問題なくゲームが進むようtry-catchで対処
        try
        {
            Rigidbody attack_rigidbody = GetComponent<Rigidbody>();
            attack_rigidbody.linearVelocity = Vector3.zero;
            attack_rigidbody.isKinematic = true;
            base.TransitionToFinished();
        }
        catch (MissingReferenceException)
        {
            return;
        }
        return;
    }
}
