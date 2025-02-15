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
    private float left_time_sec = 0.0f;
    private float next_target_add_sec = 0.0f;
    private bool is_game_end = false;
    private int beat_cnt = 0;
    public static event System.Action TimeUp;

    [Header("�t�B�[���h���")]
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

    [Header("���j����")]
    [SerializeField]
    [Tooltip("���j�󎞂̃p�[�e�B�N���I�u�W�F�N�g")]
    private GameObject BREAK_PARTICLE;

    [SerializeField]
    [Range(1, 10)]
    [Tooltip("���j��p�[�e�B�N���̃x�[�X��")]
    private int PARTICLE_BASE_NUM;

    [SerializeField]
    [Tooltip("���j�󎞂̉�")]
    private AudioClip BREAK_SE;

    [SerializeField]
    [Range(0.0f, 1.0f)]
    [Tooltip("���j����ʉ��̃x�[�X�{�����[��")]
    private float BREAK_BASE_VOLUME;

    [SerializeField]
    [Range(0.0f, 100.0f)]
    [Tooltip("���j��p�[�e�B�N���̃x�[�X���x")]
    private float PARTICLE_BASE_SPEED;

    [Header("�G�L�����N�^�[���")]
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
    [Tooltip("�G���S���̌��̐F")]
    private GameObject DEAD_FLASH_PREFAB;

    [SerializeField]
    [Tooltip("�G���S���̌��̐F")]
    private Color DEAD_FLASH_COLOR;

    [SerializeField]
    [Range(0.0f,10.0f)]
    [Tooltip("�G���S���̌��̋���")]
    private float DEAD_FLASH_INTENSITY;

    [SerializeField]
    [Range(0.0f, 5.0f)]
    [Tooltip("�G���S���̌��̎c���b��")]
    private float DEAD_FLASH_LIFETIME_SEC;

    [SerializeField]
    [Tooltip("�G���j���̉�")]
    private AudioClip BEAT_SE;

    [SerializeField]
    [Range(0.0f, 1.0f)]
    [Tooltip("�G���j���̉��̃x�[�X�{�����[��")]
    private float BEAT_SOUND_VOLUME;

    [Header("UI���")]
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
    [Range(0.1f, 99.0f)]
    [Tooltip("�G�ǉ����Ԏ���")]
    private float ADD_TARGET_CYCLE_SEC;

    [SerializeField]
    [Tooltip("�I�����x��")]
    private GameObject FINISH_LABEL;

    [SerializeField]
    [Tooltip("BGM")]
    private AudioClip BGM_CLIP;

    [SerializeField]
    [Range(0.0f,1.0f)]
    [Tooltip("BGM�{�����[��")]
    private float BGM_VOLUME;

    [SerializeField]
    [Tooltip("�I�������ʉ�")]
    private AudioClip FINISH_SE;

    [SerializeField]
    [Range(0.0f,1.0f)]
    [Tooltip("�I�������ʉ��̃{�����[��")]
    private float FINISH_SE_VOLUME;

    void Start()
    {
        SetFieldTiles();
        SpawnTarget();
        SetEventAction();
        PlayBGM(BGM_CLIP, BGM_VOLUME);
        left_time_sec = TIME_SEC;
        next_target_add_sec = TIME_SEC - ADD_TARGET_CYCLE_SEC;
    }

    private void Update()
    {
        if (!is_game_end)
        {
            left_time_sec -= Time.deltaTime;
            if(left_time_sec <= next_target_add_sec)
            {
                SpawnTarget();
                next_target_add_sec -= ADD_TARGET_CYCLE_SEC;
            }
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
                    Instantiate(fieldblock_prefab_white, tile_position, Quaternion.identity);
                }
                else
                {
                    Instantiate(fieldblock_prefab_black, tile_position, Quaternion.identity);
                }
            }
        }
    }

    void PlayBGM(AudioClip bgm_clip, float bgm_volume)
    {
        GameObject audio_object = new GameObject("TempAudioSource");

        AudioSource audio_source = audio_object.AddComponent<AudioSource>();
        audio_source.clip = bgm_clip;
        audio_source.volume = bgm_volume;
        audio_source.loop = true;
        audio_source.Play();
        return;
    }

    void SpawnTarget()
    {
        Vector3 spawn_position = new Vector3(
                Random.Range(SPAWN_RANGE_MIN.x, SPAWN_RANGE_MAX.x),
                Random.Range(SPAWN_RANGE_MIN.y, SPAWN_RANGE_MAX.y),
                Random.Range(SPAWN_RANGE_MIN.z, SPAWN_RANGE_MAX.z)
            );
        GameObject spawn_target_object = Instantiate(TARGET_PREFAB, this.transform.position+spawn_position, Quaternion.identity);
        return;
    }

    void SetEventAction()
    {
        AttackObject.BreakEvent += GenerateBreakEffect;
        Target.DeadEvent += DeadTarget;
        Player.GameOver += FinishGame;
        return;
    }

    void GenerateBreakEffect(Vector3 break_position, float break_radius)
    {
        PlaySE(BREAK_SE,break_position, break_radius*BREAK_BASE_VOLUME,true);
        GenerateBreakParticle(break_position, break_radius);
        return;
    }

    void PlaySE(AudioClip se_clip,Vector3 se_position, float se_volume, bool is_3d)
    {
        GameObject audio_object = new GameObject("TempAudioSource");
        audio_object.transform.position = se_position;

        AudioSource audio_source = audio_object.AddComponent<AudioSource>();
        audio_source.clip = se_clip;
        audio_source.volume = se_volume;
        float blend_3d = 0.0f;
        if (is_3d)
        {
            blend_3d = 1.0f;
        }
        audio_source.spatialBlend = blend_3d;
        audio_source.Play();

        Destroy(audio_object, se_clip.length);
        return;
    }

    void GenerateBreakParticle(Vector3 break_position, float break_radius)
    {
        int generate_num = PARTICLE_BASE_NUM * (int)Mathf.Round(break_radius);
        for(int i = 0; i < generate_num; i++)
        {
            Vector3 direction = new Vector3(
                Random.Range(-1.0f, 1.0f),
                Random.Range(0.0f, 1.0f),
                Random.Range(-1.0f, 1.0f)
                ).normalized;
            GameObject particle = Instantiate(BREAK_PARTICLE, break_position, Quaternion.identity);
            Rigidbody particle_rigidbody = particle.GetComponent<Rigidbody>();
            Vector3 particle_velocity = direction * Random.Range(0.0f,PARTICLE_BASE_SPEED*break_radius);
            particle_rigidbody.linearVelocity = particle_velocity;
        }
        return;
    }

    public void DeadTarget(Vector3 dead_position)
    {
        beat_cnt++;
        BEAT_TEXT.text = String.Format("{0:000}",beat_cnt);
        PlaySE(BEAT_SE,dead_position,BEAT_SOUND_VOLUME,true);
        FlashDeadLight(dead_position);
        SpawnTarget();
        return;
    }

    void FlashDeadLight(Vector3 dead_position)
    {
        Vector3 flash_position = dead_position;
        GameObject dead_flash_object = Instantiate(DEAD_FLASH_PREFAB, dead_position, Quaternion.identity);
        dead_flash_object.transform.position = flash_position;
        Destroy(dead_flash_object, DEAD_FLASH_LIFETIME_SEC);
        return;
    }

    void FinishGame()
    {
        PlaySE(FINISH_SE, Vector3.zero, FINISH_SE_VOLUME, false);
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
