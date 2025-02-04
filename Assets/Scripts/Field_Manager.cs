using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class Field_Manager : MonoBehaviour
{
    private MeshCollider field_mesh_collider;

    private List<Vector3> hole_position_list = new List<Vector3>();

    [SerializeField]
    [Tooltip("ŒŠ‚Ì”¼Œa")]
    private float HOLE_RADIUS;

    [SerializeField]
    [Tooltip("ŒŠ‚ª‹ó‚¢‚Ä‚¢‚éŠÔ")]
    private float HOLE_LIFETIME_SEC;

    [SerializeField]
    [Tooltip("1•Ó‚ÌƒƒbƒVƒ…’¸“_”")]
    private int MESH_VERTEX_NUM;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        field_mesh_collider = GetComponent<MeshCollider>();
        CreateVertices();
    }

    void CreateVertices()
    {
        List<Vector3> vertices = new List<Vector3>();
        for(int r = 0; r < MESH_VERTEX_NUM; r++)
        {
            for(int c=0;c < MESH_VERTEX_NUM; c++)
            {
                vertices.Add(new Vector3((float)r, 0.0f, (float)c));
            }
        }
        field_mesh_collider.sharedMesh.SetVertices(vertices);
        return;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GenerateHole(Vector3 generate_position)
    {
        hole_position_list.Add(generate_position);
        UpdateHoleMesh();
        return;
    }

    void FillHole(Vector3 fill_position)
    {
        hole_position_list.Remove(fill_position);
        UpdateHoleMesh();
        return;
    }

    IEnumerator GenerateAndFillHole(Vector3 hole_position)
    {
        GenerateHole(hole_position);
        yield return new WaitForSeconds(HOLE_LIFETIME_SEC);
        FillHole(hole_position);
    }

    void UpdateHoleMesh()
    {
        Mesh new_mesh = field_mesh_collider.sharedMesh;
        Vector3[] mesh_vertices = new_mesh.vertices;
        List<Vector3> active_vertices = new List<Vector3>();

        foreach(Vector3 vertex in mesh_vertices)
        {
            Vector3 vertex_position = transform.TransformPoint(vertex);
            bool is_out_of_hole = true;
            foreach(Vector3 hole_position in hole_position_list)
            {
                float distance = Vector3.Distance(vertex_position, hole_position);
                if(distance < HOLE_RADIUS)
                {
                    is_out_of_hole = false;
                    break;
                }
                if (is_out_of_hole)
                {
                    active_vertices.Add(vertex);
                }
            }
        }
        new_mesh.vertices = active_vertices.ToArray();
        field_mesh_collider.sharedMesh = new_mesh;
        return;
    }

    void OnCollisionEnter(Collision collision_object)
    {
        int[] BREAKER_LAYER_INTS = {
            LayerMask.NameToLayer("Attack Ball"),
            LayerMask.NameToLayer("Attack Spark"),
        };
        if (BREAKER_LAYER_INTS.Contains(collision_object.gameObject.layer))
        {
            GenerateAndFillHole(collision_object.gameObject.transform.position);
        }
        return;
    }
}
