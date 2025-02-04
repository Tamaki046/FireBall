using System.Collections;
using UnityEngine;

public class Field_Tile : MonoBehaviour
{
    [SerializeField]
    [Tooltip("床の復旧時間")]
    private float REPAIR_SEC = 10.0f;

    [SerializeField]
    [Tooltip("床破壊時のパーティクルオブジェクト")]
    private GameObject BROKEN_PARTICLE;

    private MeshCollider mesh_collider;
    private MeshRenderer mesh_renderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetEventAction();
        mesh_collider = GetComponent<MeshCollider>();
        mesh_renderer = GetComponent<MeshRenderer>();
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
        if(Vector3.Distance(break_position_formed,this.transform.position) <= break_radius)
        {
            StartCoroutine(BreakAndRepair());
        }
    }

    IEnumerator BreakAndRepair()
    {
        mesh_collider.enabled = false;
        mesh_renderer.enabled = false;
        yield return new WaitForSeconds(REPAIR_SEC);
        mesh_collider.enabled = true;
        mesh_renderer.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
