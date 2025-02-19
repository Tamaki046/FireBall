using UnityEngine;

public class DeadParticle : MonoBehaviour
{
    [SerializeField]
    [Tooltip("ゲームオブジェクト消滅時間")]
    private float DESTROY_SEC;

    void Start()
    {
        this.gameObject.GetComponent<ParticleSystem>().Play();
        Destroy(this.gameObject, DESTROY_SEC);
    }
}
