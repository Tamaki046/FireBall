using System.Collections;
using UnityEngine;

public class FieldObject : MonoBehaviour
{
    private BoxCollider object_collider;
    private MeshRenderer object_renderer;

    [SerializeField]
    [Min(0.1f)]
    [Tooltip("°‚Ì•œ‹ŒŠÔ")]
    private float REPAIR_SEC = 3.0f;

    [SerializeField]
    [Min(0.001f)]
    [Tooltip("’†S‚©‚ç‚Ì‹——£‚É‰‚¶‚½•œ‹ŒŠÔ‚Ì‰ÁZƒŒ[ƒg")]
    private float DISTANCE_TIME_RATE = 0.01f;

    private bool is_staying_player = false;
    private bool is_active = true;
    private bool is_game_finished = false;

    private void Start()
    {
        ConnectEventAction(true);
        object_collider = GetComponent<BoxCollider>();
        object_renderer = GetComponent<MeshRenderer>();
    }

    private void ConnectEventAction(bool connect_event)
    {
        if (connect_event)
        {
            AttackObject.BreakEvent += BreakTile;
            StageManager.TimeUp += SetActiveFalse;
            StageManager.LeaveScene += PrepareLeaveScene;
            Player.GameOver += SetActiveFalse;
        }
        else
        {
            AttackObject.BreakEvent -= BreakTile;
            StageManager.TimeUp -= SetActiveFalse;
            StageManager.LeaveScene -= PrepareLeaveScene;
            Player.GameOver -= SetActiveFalse;
        }
        return;
    }

    private void PrepareLeaveScene()
    {
        ConnectEventAction(false);
        return;
    }

    private void SetActiveFalse()
    {
        is_game_finished = true;
        return;
    }

    private void BreakTile(Vector3 break_position, float break_radius)
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
        SetFieldActive(!is_game_finished);
    }

    private void SetFieldActive(bool is_active)
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

    private void OnTriggerExit(Collider collider_object)
    {
        int PLAYER_LAYER = LayerMask.NameToLayer("Player");
        if (collider_object.gameObject.layer == PLAYER_LAYER)
        {
            is_staying_player = false;
        }
    }

    private void Update()
    {
        const float STOP_TIME_SCALE = 0.5f;
        if (Time.timeScale < STOP_TIME_SCALE)
        {
            return;
        }
        if (is_active && !object_renderer.enabled)
        {
            SetFieldActive(true);
        }
    }
}
