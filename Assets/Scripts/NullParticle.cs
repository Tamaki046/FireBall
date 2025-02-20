using UnityEngine;

public class Null_Particle : MonoBehaviour
{
    [SerializeField]
    [Range(0.1f,1.0f)]
    [Tooltip("パーティクルの残存時間最小値（秒）")]
    private float LIFETIME_SEC_MIN;

    [SerializeField]
    [Range(0.1f,1.0f)]
    [Tooltip("パーティクルの残存時間最大値（秒）")]
    private float LIFETIME_SEC_MAX;

    private bool is_active = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        float lifetime_sec = Random.Range(LIFETIME_SEC_MIN, LIFETIME_SEC_MAX);
        Invoke(nameof(ClearParticle), lifetime_sec);
        ConnectEventAction(true);
    }
    void ConnectEventAction(bool connect_event)
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
        return;
    }

    void PrepareLeaveScene()
    {
        ConnectEventAction(false);
        return;
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
        catch (MissingReferenceException)
        {
            return;
        }
        return;
    }

    void ClearParticle()
    {
        if (is_active)
        {
            DestroyThisGameObject();
        }
    }

    void DestroyThisGameObject()
    {
        PrepareLeaveScene();
        Destroy(this.gameObject);
        return;
    }
}
