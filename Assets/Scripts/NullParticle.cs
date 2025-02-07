using UnityEngine;

public class Null_Particle : MonoBehaviour
{
    [SerializeField]
    [Range(0.1f,1.0f)]
    [Tooltip("�p�[�e�B�N���̎c�����ԍŏ��l�i�b�j")]
    private float LIFETIME_SEC_MIN;

    [SerializeField]
    [Range(0.1f,1.0f)]
    [Tooltip("�p�[�e�B�N���̎c�����ԍő�l�i�b�j")]
    private float LIFETIME_SEC_MAX;

    private bool is_active = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        float lifetime_sec = Random.Range(LIFETIME_SEC_MIN, LIFETIME_SEC_MAX);
        Invoke(nameof(ClearParticle), lifetime_sec);
        SetEventAction();
    }
    void SetEventAction()
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

    void ClearParticle()
    {
        if (is_active)
        {
            Destroy(this.gameObject);
        }
    }
}
