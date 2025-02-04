using System.Collections;
using UnityEngine;

public class AttackObject : MonoBehaviour
{
    public static event System.Action<Vector3,float> BreakEvent;

    [SerializeField]
    [Tooltip("Žc‘¶ŽžŠÔ")]
    protected float object_lifetime_sec = 1.5f;

    [SerializeField]
    [Tooltip("”j‰ó”¼Œa")]
    protected float BREAK_RADIUS;

    protected virtual void Update()
    {
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
