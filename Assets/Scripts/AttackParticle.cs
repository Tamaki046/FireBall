using System.Collections;
using UnityEngine;

public class AttackParticle : GameObjectBase
{
    public static event System.Action<Vector3,float> BreakEvent;

    [Tooltip("残存時間"), Range(0.5f,3.0f)]
    [SerializeField]
    private float object_lifetime_sec = 1.5f;

    [Tooltip("破壊半径"), Range(0.1f,10.0f)]
    [SerializeField]
    private float BREAK_RADIUS;

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
        return;
    }


    private void OnCollisionEnter(Collision collision_object)
    {
        if (base.state == States.ACTING)
        {
            BreakField(this.transform.position);
        }
        DestroyThisGameObject();
        return;
    }

    private void BreakField(Vector3 break_position)
    {
        BreakEvent.Invoke(break_position, BREAK_RADIUS);
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
