using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using NUnit.Framework;
using UnityEngine;

public class TileBreaker : MonoBehaviour
{
    [SerializeField]
    [Tooltip("床破壊時のパーティクルオブジェクト")]
    private GameObject BROKEN_PARTICLE;

    [SerializeField]
    [Tooltip("床の復旧時間")]
    private float RESPAWN_SEC = 10.0f;

    private List<GameObject> break_tiles_array = new List<GameObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //EmitParticles();
        //Invoke(nameof(RepairTiles), RESPAWN_SEC);
        //const float OFFSET = 1.0f;
        //Invoke(nameof(DestroyBreaker), RESPAWN_SEC+OFFSET);
    }

    void EmitParticles()
    {
        for (int i = 0; i < 10; i++)
        {
            EmitSingleParticle();
        }
        return;
    }

    void OnTriggerEnter(Collider collider_object)
    {
        //break_tiles_array.Add(collider_object.gameObject);
        //collider_object.gameObject.SetActive(false);
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

    void RepairTiles()
    {
        foreach(GameObject t in break_tiles_array)
        {
            t.SetActive(true);
        }
        return;
    }

    void DestroyBreaker()
    {
        Destroy(this.gameObject);
        return;
    }
}
