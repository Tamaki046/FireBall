using System;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class StageManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("フィールドタイルのプレハブ")]
    private GameObject fieldtile_prefab;
    [SerializeField]
    [Tooltip("ステージの1辺のタイル数")]
    private int STAGE_SIZE = 10;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField]
    [Tooltip("生成する敵キャラクターのプレハブ")]
    private GameObject TARGET_PREFAB;

    [SerializeField]
    [Tooltip("敵キャラクターの生成範囲の最小値")]
    private Vector3 SPAWN_RANGE_MIN;

    [SerializeField]
    [Tooltip("敵キャラクターの生成範囲の最大値")]
    private Vector3 SPAWN_RANGE_MAX;

    [SerializeField]
    [Tooltip("撃破数ラベル")]
    private TextMeshProUGUI BEAT_LABEL;

    private int beat_cnt = 0;

    void Start()
    {
        SetFieldTiles();
        SpawnTarget();
        SetEventAction();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SetFieldTiles(){
        const float TILE_SIZE = 2.0f;
        Vector3 tile_position;
        Transform PARENT_TRANSFORM = this.transform;
        float ORIGIN_COORDINATE;
        if(STAGE_SIZE%2==0){
            ORIGIN_COORDINATE = (STAGE_SIZE/2) * TILE_SIZE * (-1.0f) + (TILE_SIZE/2.0f);
        }else{
            ORIGIN_COORDINATE = (STAGE_SIZE/2) * TILE_SIZE * (-1.0f);
        }
        for(int i=0;i<STAGE_SIZE;i++){
            for(int j=0;j<STAGE_SIZE;j++){
                tile_position = new Vector3(
                    ORIGIN_COORDINATE + (float)i * TILE_SIZE,
                    0.0f,
                    ORIGIN_COORDINATE + (float)j * TILE_SIZE
                );
                Instantiate(fieldtile_prefab, tile_position, PARENT_TRANSFORM.rotation, PARENT_TRANSFORM);
            }
        }
    }

    void SpawnTarget()
    {
        Vector3 spawn_position = new Vector3(
                Random.Range(SPAWN_RANGE_MIN.x, SPAWN_RANGE_MAX.x),
                Random.Range(SPAWN_RANGE_MIN.y, SPAWN_RANGE_MAX.y),
                Random.Range(SPAWN_RANGE_MIN.z, SPAWN_RANGE_MAX.z)
            );
        GameObject spawn_target_object = Instantiate(TARGET_PREFAB, this.transform.position+spawn_position, this.transform.rotation);
        return;
    }

    void SetEventAction()
    {
        Target.DeadEvent += DeadTarget;
    }

    public void DeadTarget()
    {
        beat_cnt++;
        BEAT_LABEL.text = $"Beat : {beat_cnt}";
        SpawnTarget();
        if (beat_cnt % 10 == 0)
        {
            SpawnTarget();
        }
        return;
    }
}
