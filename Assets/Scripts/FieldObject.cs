using System.Collections;
using UnityEngine;

public class FieldObject : MonoBehaviour
{
    private BoxCollider object_collider;
    private MeshRenderer object_renderer;

    [SerializeField]
    [Min(0.1f)]
    [Tooltip("���̕�������")]
    private float REPAIR_SEC = 3.0f;

    [SerializeField]
    [Min(0.001f)]
    [Tooltip("���S����̋����ɉ������������Ԃ̉��Z���[�g")]
    private float DISTANCE_TIME_RATE = 0.01f;

    [SerializeField]
    [Tooltip("���j�󎞂̃p�[�e�B�N���I�u�W�F�N�g")]
    private GameObject BROKEN_PARTICLE;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetEventAction();
        object_collider = GetComponent<BoxCollider>();
        object_renderer = GetComponent<MeshRenderer>();
    }

    void SetEventAction()
    {
        AttackObject.BreakEvent += BreakTile;
    }

    void BreakTile(Vector3 break_position, float break_radius)
    {
        Vector3 break_position_formed = new Vector3(
                break_position.x,
                this.transform.position.y,
                break_position.z
            );
        float distance = Vector3.Distance(break_position_formed, this.transform.position);
        if(distance <= break_radius)
        {
            StartCoroutine(BreakAndRepair(break_radius-distance));
        }
    }

    IEnumerator BreakAndRepair(float distance_from_outside)
    {
        object_collider.enabled = false;
        object_renderer.enabled = false;
        yield return new WaitForSeconds(REPAIR_SEC + distance_from_outside * DISTANCE_TIME_RATE);
        object_collider.enabled = true;
        object_renderer.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
