using System.Collections;
using UnityEngine;

public class FieldObject : MonoBehaviour
{
    private BoxCollider object_collider;
    private MeshRenderer object_renderer;

    [SerializeField]
    [Min(0.1f)]
    [Tooltip("床の復旧時間")]
    private float REPAIR_SEC = 3.0f;

    [SerializeField]
    [Min(0.001f)]
    [Tooltip("中心からの距離に応じた復旧時間の加算レート")]
    private float DISTANCE_TIME_RATE = 0.01f;

    [SerializeField]
    [Tooltip("床破壊時のパーティクルオブジェクト")]
    private GameObject BROKEN_PARTICLE;

    private bool is_staying_player = false;
    private bool is_active = true;
    private bool is_timeup = false;

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
        StageManager.TimeUp += SetActiveFalse;
        Player.GameOver += SetActiveFalse;
    }
    void SetActiveFalse()
    {
        is_timeup = true;
        return;
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
        SetFieldActive(false);
        yield return new WaitForSeconds(REPAIR_SEC + distance_from_outside * DISTANCE_TIME_RATE);
        SetFieldActive(!is_timeup);
    }

    void SetFieldActive(bool is_active)
    {
        this.is_active = is_active;
        if (!is_staying_player)
        {
            object_collider.isTrigger = !is_active;
            object_renderer.enabled = is_active;
        }
        return;
    }

    private void OnTriggerEnter(Collider collider_object)
    {
        int PLAYER_LAYER = LayerMask.NameToLayer("Player");
        if(collider_object.gameObject.layer == PLAYER_LAYER)
        {
            is_staying_player = true;
        }
    }
    void Update()
    {
        if (is_active && !object_renderer.enabled)
        {
            SetFieldActive(true);
        }
    }

    private void OnTriggerExit(Collider collider_object)
    {
        int PLAYER_LAYER = LayerMask.NameToLayer("Player");
        if (collider_object.gameObject.layer == PLAYER_LAYER)
        {
            is_staying_player = false;
        }
    }

}
