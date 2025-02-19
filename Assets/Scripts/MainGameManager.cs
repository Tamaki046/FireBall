using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class StageManager : MonoBehaviour
{
    private float start_countdown_sec = 3.0f;
    private float left_time_sec = 0.0f;
    private float next_target_add_sec = 0.0f;
    private bool is_game_started = false;
    private bool is_game_finished = false;
    private int beat_cnt = 0;
    private GameObject FINISH_UI;
    public static event System.Action GameStart;
    public static event System.Action TimeUp;
    public static event System.Action LeaveScene;

    [Header("フィールド情報")]
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

    [Header("床破壊情報")]
    [SerializeField]
    [Tooltip("床破壊時のパーティクルオブジェクト")]
    private GameObject BREAK_PARTICLE;

    [SerializeField]
    [Range(1, 10)]
    [Tooltip("床破壊パーティクルのベース数")]
    private int PARTICLE_BASE_NUM;

    [SerializeField]
    [Tooltip("床破壊時の音")]
    private AudioClip BREAK_SE;

    [SerializeField]
    [Range(0.0f, 1.0f)]
    [Tooltip("床破壊効果音のベースボリューム")]
    private float BREAK_BASE_VOLUME;

    [SerializeField]
    [Range(0.0f, 100.0f)]
    [Tooltip("床破壊パーティクルのベース速度")]
    private float PARTICLE_BASE_SPEED;

    [Header("敵キャラクター情報")]
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
    [Tooltip("敵死亡時の光の色")]
    private GameObject DEAD_FLASH_PREFAB;

    [SerializeField]
    [Tooltip("敵死亡時の光の色")]
    private Color DEAD_FLASH_COLOR;

    [SerializeField]
    [Range(0.0f,10.0f)]
    [Tooltip("敵死亡時の光の強さ")]
    private float DEAD_FLASH_INTENSITY;

    [SerializeField]
    [Range(0.0f, 5.0f)]
    [Tooltip("敵死亡時の光の残存秒数")]
    private float DEAD_FLASH_LIFETIME_SEC;

    [SerializeField]
    [Tooltip("敵撃破時の音")]
    private AudioClip BEAT_SE;

    [SerializeField]
    [Range(0.0f, 1.0f)]
    [Tooltip("敵撃破時の音のベースボリューム")]
    private float BEAT_SOUND_VOLUME;

    [SerializeField]
    [Tooltip("敵追加時の音")]
    private AudioClip ADD_SPAWN_SE;

    [SerializeField]
    [Range(0.0f, 1.0f)]
    [Tooltip("敵追加時の音のベースボリューム")]
    private float ADD_SPAWN_BASE_VOLUME;

    [Header("UI情報")]
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
    [Range(0.1f, 99.0f)]
    [Tooltip("敵追加時間周期")]
    private float ADD_TARGET_CYCLE_SEC;

    [SerializeField]
    [Tooltip("BGM")]
    private AudioClip BGM_CLIP;

    [SerializeField]
    [Range(0.0f,1.0f)]
    [Tooltip("BGMボリューム")]
    private float BGM_VOLUME;

    [SerializeField]
    [Tooltip("終了時効果音")]
    private AudioClip FINISH_SE;

    [SerializeField]
    [Range(0.0f,1.0f)]
    [Tooltip("終了時効果音のボリューム")]
    private float FINISH_SE_VOLUME;

    void Start()
    {
        SetStartUI(true);
        SetFinishUIsEnable(false);
        SetFieldTiles();
        ConnectEventAction(true);
        PlayBGM(BGM_CLIP, BGM_VOLUME);
        left_time_sec = TIME_SEC;
        next_target_add_sec = TIME_SEC - ADD_TARGET_CYCLE_SEC;
    }

    void SetStartUI(bool is_visible)
    {
        GameObject start_ui = GameObject.FindGameObjectWithTag("StartUI");
        start_ui.GetComponent<Canvas>().enabled = is_visible;
        UpdateCountdownStartTimer(Mathf.Ceil(start_countdown_sec));
        return;
    }

    void UpdateCountdownStartTimer(float print_sec)
    {
        if(print_sec <= 0.0f)
        {
            SetStartUI(false);
        }
        else
        {
            GameObject start_countdown = GameObject.FindGameObjectWithTag("StartCountdown");
            start_countdown.GetComponent<TextMeshProUGUI>().text = $"{Mathf.Ceil(print_sec):0}";
        }
            
        return;
    }

    void StartGame()
    {
        SpawnTarget();
        is_game_started = true;
        GameStart.Invoke();
        return;
    }

    void SetFinishUIsEnable(bool is_visible)
    {
        GameObject finish_uis = GameObject.FindGameObjectWithTag("FinishUIs");
        finish_uis.GetComponent<Canvas>().enabled = is_visible;
        return;
    }

    void Update()
    {
        if (!is_game_started)
        {
            float next_sec = start_countdown_sec - Time.deltaTime;
            if (Mathf.Ceil(start_countdown_sec) != Mathf.Ceil(next_sec))
            {
                UpdateCountdownStartTimer(Mathf.Ceil(next_sec));
            }
            start_countdown_sec = next_sec;
            if(start_countdown_sec <= 0.0f)
            {
                StartGame();
            }
        }
        else if (is_game_started && !is_game_finished)
        {
            left_time_sec -= Time.deltaTime;
            
            if (left_time_sec <= 0.0f)
            {
                left_time_sec = 0.0f;
                TimeUpGame();
            }else if(left_time_sec <= next_target_add_sec)
            {
                PlaySE(ADD_SPAWN_SE, Vector3.zero, ADD_SPAWN_BASE_VOLUME, false);
                SpawnTarget();
                next_target_add_sec -= ADD_TARGET_CYCLE_SEC;
            }
            TIMER_TEXT.text = $"{Mathf.Ceil(left_time_sec):00}";
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
        audio_source.volume = bgm_volume * PlayerPrefs.GetFloat("BGMVolume");
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

    void ConnectEventAction(bool connect_event)
    {
        if (connect_event)
        {
            AttackObject.BreakEvent += GenerateBreakEffect;
            Target.DeadEvent += DeadTarget;
            Player.GameOver += FinishGame;
        }
        else
        {
            AttackObject.BreakEvent -= GenerateBreakEffect;
            Target.DeadEvent -= DeadTarget;
            Player.GameOver -= FinishGame;
        }
        return;
    }

    void GenerateBreakEffect(Vector3 break_position, float break_radius)
    {
        PlaySE(BREAK_SE,break_position, break_radius* BREAK_BASE_VOLUME, true);
        GenerateBreakParticle(break_position, break_radius);
        return;
    }

    void PlaySE(AudioClip se_clip,Vector3 se_position, float se_volume, bool is_3d)
    {
        GameObject audio_object = new GameObject("TempAudioSource");
        audio_object.transform.position = se_position;

        AudioSource audio_source = audio_object.AddComponent<AudioSource>();
        audio_source.clip = se_clip;
        audio_source.volume = se_volume * PlayerPrefs.GetFloat("SEVolume");
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
        is_game_finished = true;
        SetFinishUIsEnable(true);
        return;
    }

    private void TimeUpGame()
    {
        TimeUp.Invoke();
        FinishGame();
        return;
    }

    void RestartGame()
    {
        PrepareLeaveScene();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        return;
    }

    void PrepareLeaveScene()
    {
        LeaveScene.Invoke();
        ConnectEventAction(false);
        return;
    }

    void BackToTitle()
    {
        PrepareLeaveScene();
        SceneManager.LoadScene("TitleScene");
        return;
    }
}
