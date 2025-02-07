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
    [Tooltip("�t�B�[���h�u���b�N�̃v���n�u�i���j")]
    private GameObject fieldblock_prefab_white;

    [SerializeField]
    [Tooltip("�t�B�[���h�u���b�N�̃v���n�u�i���j")]
    private GameObject fieldblock_prefab_black;

    [SerializeField]
    [Range(1,100)]
    [Tooltip("�X�e�[�W��1�ӂ̃^�C����")]
    private int STAGE_SIZE = 10;

    [SerializeField]
    [Range(1, 100)]
    [Tooltip("�X�e�[�W�̐F�̎���")]
    private int STAGE_COLOR_CYCLE = 5;

    [SerializeField]
    [Tooltip("��������G�L�����N�^�[�̃v���n�u")]
    private GameObject TARGET_PREFAB;

    [SerializeField]
    [Tooltip("�G�L�����N�^�[�̐����͈͂̍ŏ��l")]
    private Vector3 SPAWN_RANGE_MIN;

    [SerializeField]
    [Tooltip("�G�L�����N�^�[�̐����͈͂̍ő�l")]
    private Vector3 SPAWN_RANGE_MAX;

    [SerializeField]
    [Tooltip("���j���e�L�X�g")]
    private TextMeshProUGUI BEAT_TEXT;

    [SerializeField]
    [Tooltip("�^�C�}�[�e�L�X�g")]
    private TextMeshProUGUI TIMER_TEXT;

    [SerializeField]
    [Range(0.1f,99.0f)]
    [Tooltip("��������")]
    private float TIME_SEC;

    [SerializeField]
    [Tooltip("�I�����x��")]
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
