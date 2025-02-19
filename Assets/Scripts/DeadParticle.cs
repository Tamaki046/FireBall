using UnityEngine;

public class DeadParticle : MonoBehaviour
{
    [SerializeField]
    [Tooltip("�Q�[���I�u�W�F�N�g���Ŏ���")]
    private float DESTROY_SEC;

    void Start()
    {
        this.gameObject.GetComponent<ParticleSystem>().Play();
        Destroy(this.gameObject, DESTROY_SEC);
    }
}
