using UnityEngine;

public class DeadParticle : MonoBehaviour
{
    [Tooltip("�Q�[���I�u�W�F�N�g���Ŏ���")]
    [SerializeField]
    private float DESTROY_SEC;

    private void Start()
    {
        this.gameObject.GetComponent<ParticleSystem>().Play();
        Destroy(this.gameObject, DESTROY_SEC);
    }
}
