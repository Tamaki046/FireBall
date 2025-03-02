using UnityEngine;

public class DeadParticle : MonoBehaviour
{
    [Tooltip("ゲームオブジェクト消滅時間")]
    [SerializeField]
    private float DESTROY_SEC;

    private void Start()
    {
        this.gameObject.GetComponent<ParticleSystem>().Play();
        Destroy(this.gameObject, DESTROY_SEC);
    }
}
