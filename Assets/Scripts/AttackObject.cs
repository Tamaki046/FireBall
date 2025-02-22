using System.Collections;
using UnityEngine;

public class AttackObject : MonoBehaviour
{
    protected bool is_active = true;
    public static event System.Action<Vector3,float> BreakEvent;

    [SerializeField]
    [Range(0.5f,3.0f)]
    [Tooltip("Žc‘¶ŽžŠÔ")]
    protected float object_lifetime_sec = 1.5f;

    [SerializeField]
    [Range(0.1f,10.0f)]
    [Tooltip("”j‰ó”¼Œa")]
    protected float BREAK_RADIUS;

    protected virtual void Start()
    {
        ConnectEventAction(true);
    }

    protected void ConnectEventAction(bool connect_event)
    {
        if (connect_event)
        {
            StageManager.TimeUp += SetActiveFalse;
            StageManager.LeaveScene += PrepareLeaveScene;
            Player.GameOver += SetActiveFalse;
        }
        else
        {
            StageManager.TimeUp -= SetActiveFalse;
            StageManager.LeaveScene -= PrepareLeaveScene;
            Player.GameOver -= SetActiveFalse;
        }
    }

    protected void PrepareLeaveScene()
    {
        ConnectEventAction(false);
        return;
    }

    protected void SetActiveFalse()
    {
        try
        {
            is_active = false;
            Rigidbody attack_rigidbody = GetComponent<Rigidbody>();
            attack_rigidbody.linearVelocity = Vector3.zero;
            attack_rigidbody.isKinematic = true;
        }
        catch (MissingReferenceException)
        {
            return;
        }
        return;
    }

    protected virtual void Update()
    {
        if (!is_active)
        {
            return;
        }
        if(this.transform.position.y <= -10.0f){
            DestroyThisGameObject();
        }
        object_lifetime_sec -= Time.deltaTime;
        if (object_lifetime_sec <= 0.0f)
        {
            DestroyThisGameObject();
        }
    }

    protected virtual void DestroyThisGameObject()
    {
        PrepareLeaveScene();
        Destroy(this.gameObject);
        return;
    }

    protected virtual void OnCollisionEnter(Collision collision_object)
    {
        if (is_active)
        {
            BreakField(this.transform.position);
        }
        DestroyThisGameObject();
        return;
    }

    protected void BreakField(Vector3 break_position)
    {
        BreakEvent.Invoke(break_position, BREAK_RADIUS);
    }
}
