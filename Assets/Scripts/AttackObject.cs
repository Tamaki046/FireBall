using System.Collections;
using UnityEngine;

public class AttackObject : MonoBehaviour
{
    protected bool is_active = true;
    public static event System.Action<Vector3,float> BreakEvent;

    [SerializeField]
    [Range(0.5f,3.0f)]
    [Tooltip("�c������")]
    protected float object_lifetime_sec = 1.5f;

    [SerializeField]
    [Range(0.1f,10.0f)]
    [Tooltip("�j�󔼌a")]
    protected float BREAK_RADIUS;


    protected virtual void Start()
    {
        SetEventAction();
        Debug.Log(this.transform.position);
    }

    protected void SetEventAction()
    {
        StageManager.TimeUp += SetActiveFalse;
        Player.GameOver += SetActiveFalse;
    }

    void SetActiveFalse()
    {
        try
        {
            is_active = false;
            Rigidbody attack_rigidbody = GetComponent<Rigidbody>();
            attack_rigidbody.linearVelocity = Vector3.zero;
            attack_rigidbody.isKinematic = true;
        }
        catch (MissingReferenceException e)
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
            Destroy(this.gameObject);
        }
        object_lifetime_sec -= Time.deltaTime;
        if (object_lifetime_sec <= 0.0f)
        {
            Destroy(this.gameObject);
        }
        Debug.Log(this.transform.position);
    }

    protected virtual void OnCollisionEnter(Collision collision_object)
    {
        BreakField(this.transform.position);
        Destroy(this.gameObject);
        return;
    }

    protected void BreakField(Vector3 break_position)
    {
        BreakEvent.Invoke(break_position, BREAK_RADIUS);
    }
}
