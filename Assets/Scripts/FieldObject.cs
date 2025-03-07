using System.Collections;
using UnityEngine;

public class FieldObject : GameObjectBase
{
    private BoxCollider object_collider;
    private MeshRenderer object_renderer;
    private bool is_staying_player = false;


    [Tooltip("���̕�������"), Min(0.1f)]
    [SerializeField]
    private float REPAIR_SEC = 3.0f;

    [Tooltip("���S����̋����ɉ������������Ԃ̉��Z���[�g"), Min(0.001f)]
    [SerializeField]
    private float DISTANCE_TIME_RATE = 0.01f;


    private void Start()
    {
        ConnectEventAction(true);
        object_collider = GetComponent<BoxCollider>();
        object_renderer = GetComponent<MeshRenderer>();
        base.TransitionToActing();
    }

    private void ConnectEventAction(bool is_connect_event)
    {
        if (is_connect_event)
        {
            AttackParticle.BreakEvent += BreakTile;
            AttackBall.BreakEvent += BreakTile;
            StageManager.GameStop += SetActiveFalse;
            StageManager.LeaveScene += PrepareLeaveScene;
            Player.GameOver += SetActiveFalse;
        }
        else
        {
            AttackParticle.BreakEvent -= BreakTile;
            AttackBall.BreakEvent -= BreakTile;
            StageManager.GameStop -= SetActiveFalse;
            StageManager.LeaveScene -= PrepareLeaveScene;
            Player.GameOver -= SetActiveFalse;
        }
        return;
    }


    protected override void UpdateOnReady()
    {
        if (!object_renderer.enabled)
        {
            SetFieldActive(true);
        }
    }

    private void SetActiveFalse()
    {
        base.TransitionToFinished();
        return;
    }


    private void PrepareLeaveScene()
    {
        ConnectEventAction(false);
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
        if(base.state == States.ACTING)
        {
            SetFieldActive(true);
        }
    }

    private void SetFieldActive(bool is_active)
    {
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
}
