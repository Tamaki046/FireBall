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


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        float lifetime_sec = Random.Range(LIFETIME_SEC_MIN, LIFETIME_SEC_MAX);
        Invoke(nameof(ClearParticle), lifetime_sec);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ClearParticle()
    {
        Destroy(this.gameObject);
    }
}
