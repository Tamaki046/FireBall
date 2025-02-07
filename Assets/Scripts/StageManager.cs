using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class StageManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("フィールドブロックのプレハブ（白）")]
    private GameObject fieldblock_prefab_white;

    [SerializeField]
    [Tooltip("フィールドブロックのプレハブ（黒）")]
    private GameObject fieldblock_prefab_black;

    [SerializeField]
    [Range(1,100)]
    [Tooltip("ステージの1辺のタイル数")]
    private int STAGE_SIZE = 10;

    [SerializeField]
    [Range(1, 100)]
    [Tooltip("ステージの色の周期")]
    private int STAGE_COLOR_CYCLE = 5;

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
    [Tooltip("撃破数テキスト")]
    private TextMeshProUGUI BEAT_TEXT;

    [SerializeField]
    [Tooltip("タイマーテキスト")]
    private TextMeshProUGUI TIMER_TEXT;

    [SerializeField]
    [Range(0.1f,99.0f)]
    [Tooltip("制限時間")]
    private float TIME_SEC;

    [SerializeField]
    [Tooltip("終了ラベル")]
    private GameObject FINISH_LABEL;

    public static event System.Action TimeUp;
    private float left_time_sec = 0.0f;
    private bool is_game_end = false;

    private int beat_cnt = 0;

    void Start()
    {
        SetFieldTiles();
        SpawnTarget();
        SetEventAction();
        left_time_sec = TIME_SEC;
    }

    private void Update()
    {
        if (!is_game_end)
        {
            left_time_sec -= Time.deltaTime;
            if (left_time_sec <= 0.0f)
            {
                left_time_sec = 0.0f;
                TimeUpGame();
            }
            TIMER_TEXT.text = String.Format("{0:00}", Mathf.Ceil(left_time_sec));
        }
    }

    void SetFieldTiles(){
        float TILE_SIZE = fieldblock_prefab_white.transform.localScale.x;
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
                if (((i / STAGE_COLOR_CYCLE) + (j / STAGE_COLOR_CYCLE)) % 2 == 0)
                {
                    Instantiate(fieldblock_prefab_white, tile_position, PARENT_TRANSFORM.rotation, PARENT_TRANSFORM);
                }
                else
                {
                    Instantiate(fieldblock_prefab_black, tile_position, PARENT_TRANSFORM.rotation, PARENT_TRANSFORM);
                }
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
        Player.GameOver += FinishGame;
    }

    public void DeadTarget()
    {
        beat_cnt++;
        BEAT_TEXT.text = String.Format("{0:000}",beat_cnt);
        SpawnTarget();
        if (beat_cnt % 10 == 0)
        {
            SpawnTarget();
        }
        return;
    }

    void FinishGame()
    {
        is_game_end = true;
        FINISH_LABEL.SetActive(true);
        return;
    }

    private void TimeUpGame()
    {
        TimeUp.Invoke();
        FinishGame();
        return;
    }
}
