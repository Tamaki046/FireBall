using UnityEngine;

public class Field_Tile : MonoBehaviour
{
    [SerializeField]
    [Tooltip("床の復旧時間")]
    private float RESPAWN_SEC = 10.0f;

    [SerializeField]
    [Tooltip("床破壊時のパーティクルオブジェクト")]
    private GameObject BROKEN_PARTICLE;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision_object)
    {
        int FIREBALL_LAYER_INT = LayerMask.NameToLayer("Attack Ball");
        int FIRESPARK_LAYER_INT = LayerMask.NameToLayer("Attack Spark");
        if(collision_object.gameObject.layer == FIREBALL_LAYER_INT || collision_object.gameObject.layer == FIRESPARK_LAYER_INT){
            EmitParticles();
            this.gameObject.SetActive(false);
            Invoke(nameof(RespawnTile),RESPAWN_SEC);
        }
        return;
    }

    void EmitParticles()
    {
        for(int i = 0; i < 10; i++)
        {
            EmitSingleParticle();
        }
        return;
    }

    void EmitSingleParticle()
    {
        GameObject particle = Instantiate(BROKEN_PARTICLE, this.transform.position, this.transform.rotation);
        float PARTICLE_SPEED = 10.0f;
        Rigidbody particle_rigidbody = particle.GetComponent<Rigidbody>();
        Vector3 shot_velocity = new Vector3(
            Random.Range(-1.0f, 1.0f),
            Random.Range(0.0f, 0.5f),
            Random.Range(-1.0f, 1.0f)
        ).normalized * PARTICLE_SPEED;
        particle_rigidbody.linearVelocity = shot_velocity;
        return;
    }

    void RespawnTile()
    {
        this.gameObject.SetActive(true);
        return;
    }
}
