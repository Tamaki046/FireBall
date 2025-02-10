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
        SetEventAction();
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
        if(this.transform.position.y <= -10){
            Destroy(this.gameObject);
        }
        object_lifetime_sec -= Time.deltaTime;
        if (object_lifetime_sec <= 0.0f)
        {
            Destroy(this.gameObject);
        }
    }

    protected virtual void OnCollisionEnter(Collision collision_object)
    {
        BreakField();
        Destroy(this.gameObject);
    }

    protected void BreakField()
    {
        Vector3 collision_position = this.transform.position;
        BreakEvent.Invoke(collision_position, BREAK_RADIUS);
    }
}
