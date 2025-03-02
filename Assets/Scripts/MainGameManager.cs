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
using Cursor = UnityEngine.Cursor;
using Random = UnityEngine.Random;

public class StageManager : MonoBehaviour
{
    private float start_countdown_sec = 3.0f;
    private float left_time_sec = 0.0f;
    private bool is_game_started = false;
    private bool is_game_finished = false;
    private bool is_game_leaving = false;
    private int beat_cnt = 0;
    private int appear_cnt = 0;
    private int next_target_add_cnt = 0;
    private TextMeshProUGUI beat_count_textmesh;
    private TextMeshProUGUI timer_textmesh;
    public static event System.Action GameStart;
    public static event System.Action GameStop;
    public static event System.Action LeaveScene;

    private const string START_UI_TAG = "StartUI";
    private const string FINISH_UI_TAG = "FinishUI";
    private const string PAUSE_UI_TAG = "PauseUI";
    private const string TIMER_TEXT_TAG = "TimerText";
    private const string BEAT_COUNT_TEXT_TAG = "BeatCountText";
    private const string START_COUNTDOWN_TAG = "StartCountdownText";

    private const string BGM_PREFS_KEY = "BGMVolume";
    private const string SE_PREFS_KEY = "SEVolume";

    [Header("フィールド情報")]
    [Tooltip("フィールドブロックのプレハブ（白）")]
    [SerializeField]
    private GameObject fieldblock_prefab_white;

    [Tooltip("フィールドブロックのプレハブ（黒）")]
    [SerializeField]
    private GameObject fieldblock_prefab_black;

    [Tooltip("ステージの1辺のタイル数"), Range(1, 100)]
    [SerializeField]
    private int STAGE_SIZE = 10;

    [Tooltip("ステージのタイル色の周期"), Range(1, 100)]
    [SerializeField]
    private int STAGE_COLOR_CYCLE = 5;


    [Header("床破壊情報")]
    [Tooltip("床破壊時のパーティクルオブジェクト")]
    [SerializeField]
    private GameObject BREAK_PARTICLE;

    [Tooltip("床破壊パーティクルのベース数"), Range(1, 10)]
    [SerializeField]
    private int PARTICLE_BASE_NUM;

    [SerializeField]
    [Tooltip("床破壊時の音")]
    private AudioClip BREAK_SE_CLIP;

    [Tooltip("床破壊効果音のベースボリューム"), Range(0.0f, 1.0f)]
    [SerializeField]
    private float BREAK_BASE_VOLUME;

    [Tooltip("床破壊パーティクルのベース速度"), Range(0.0f, 100.0f)]
    [SerializeField]
    private float PARTICLE_BASE_SPEED;


    [Header("敵キャラクター情報")]
    [Tooltip("生成する敵キャラクターのプレハブ")]
    [SerializeField]
    private GameObject TARGET_PREFAB;

    [Tooltip("敵キャラクターの生成範囲の最小値")]
    [SerializeField]
    private Vector3 SPAWN_RANGE_MIN;

    [Tooltip("敵キャラクターの生成範囲の最大値")]
    [SerializeField]
    private Vector3 SPAWN_RANGE_MAX;

    [Tooltip("敵死亡時の光")]
    [SerializeField]
    private GameObject DEAD_FLASH_PREFAB;

    [Tooltip("敵死亡時の光の色")]
    [SerializeField]
    private Color DEAD_FLASH_COLOR;

    [Tooltip("敵死亡時の光の強さ"), Range(0.0f,10.0f)]
    [SerializeField]
    private float DEAD_FLASH_INTENSITY;

    [Tooltip("敵死亡時の光の残存秒数"), Range(0.0f, 5.0f)]
    [SerializeField]
    private float DEAD_FLASH_LIFETIME_SEC;

    [Tooltip("敵撃破時の音")]
    [SerializeField]
    private AudioClip BEAT_SE_CLIP;

    [Tooltip("敵撃破時の音のベースボリューム"), Range(0.0f, 1.0f)]
    [SerializeField]
    private float BEAT_SE_BASE_VOLUME;

    [Tooltip("敵追加時の音")]
    [SerializeField]
    private AudioClip ADD_SPAWN_SE_CLIP;

    [Tooltip("敵追加時の音のベースボリューム"), Range(0.0f, 1.0f)]
    [SerializeField]
    private float ADD_SPAWN_SE_BASE_VOLUME;


    [Header("BGMや効果音")]
    [Tooltip("BGM音声ファイル")]
    [SerializeField]
    private AudioClip BGM_CLIP;

    [Tooltip("BGMベースボリューム"), Range(0.0f,1.0f)]
    [SerializeField]
    private float BGM_BASE_VOLUME;

    [Tooltip("ポーズSE音声ファイル")]
    [SerializeField]
    private AudioClip PAUSE_SE_CLIP;

    [Tooltip("ポーズSEベースボリューム"), Range(0.0f, 1.0f)]
    [SerializeField]
    private float PAUSE_BASE_VOLUME;

    [Tooltip("終了時効果音ファイル")]
    [SerializeField]
    private AudioClip FINISH_SE_CLIP;

    [Tooltip("終了時効果音ベースボリューム"), Range(0.0f,1.0f)]
    [SerializeField]
    private float FINISH_SE_BASE_VOLUME;

    [SerializeField]
    [Tooltip("ゲーム開始SEファイル")]
    private AudioClip START_SE_CLIP;

    [Tooltip("ゲーム開始SEベースボリューム"), Range(0.0f, 1.0f)]
    [SerializeField]
    private float START_SE_BASE_VOLUME;

    [Tooltip("タイトル帰還SE")]
    [SerializeField]
    private AudioClip TITLE_BACK_SE_CLIP;

    [Tooltip("タイトル帰還SEベースボリューム"), Range(0.0f, 1.0f)]
    [SerializeField]
    private float TITLE_BACK_SE_BASE_VOLUME;


    [Header("バランス調整情報")]
    [Tooltip("制限時間"), Range(0.1f, 99.0f)]
    [SerializeField]
    private float TIMELIMIT_SEC;

    [Tooltip("敵追加時間周期"), Range(0.1f, 99.0f)]
    [SerializeField]
    private float ADD_TARGET_CYCLE_SEC;

    [Tooltip("敵追加スコア周期"), Range(1,10)]
    [SerializeField]
    private int SPAWN_SCORE_RATE;

    [Tooltip("プレイヤー近辺へのリスポーンを可能にするか")]
    [SerializeField]
    private bool ENABLE_NEAR_SPAWN = false;

    [Tooltip("プレイヤー近辺のリスポーン禁止半径"), Range(0.0f, 10.0f)]
    [SerializeField]
    private float DONT_SPAWN_RADIUS;


    private void Start()
    {
        SetupUIs();
        SetupFieldTiles();
        DisplayCursor(false);
        ConnectEventAction(true);
        PlayBGM(BGM_CLIP, BGM_BASE_VOLUME);
        left_time_sec = TIMELIMIT_SEC;
        appear_cnt = 1;
        next_target_add_cnt = appear_cnt * SPAWN_SCORE_RATE;
    }

    private void SetupUIs()
    {
        GetAllTextUIs();
        DisplayUI(START_UI_TAG, true);
        UpdateCountdownStartTimer(Mathf.Ceil(start_countdown_sec));
        DisplayUI(FINISH_UI_TAG, false);
        DisplayUI(PAUSE_UI_TAG, false);
        return;
    }

    private void GetAllTextUIs()
    {
        GetTextUI(TIMER_TEXT_TAG, ref timer_textmesh);
        GetTextUI(BEAT_COUNT_TEXT_TAG, ref beat_count_textmesh);
        return;
    }

    private void GetTextUI(string tag, ref TextMeshProUGUI textmesh)
    {
        GameObject text_object = GameObject.FindGameObjectWithTag(tag);
        textmesh = text_object.GetComponent<TextMeshProUGUI>();
        return;
    }

    private void DisplayUI(string tag, bool is_visible)
    {
        GameObject ui_object = GameObject.FindGameObjectWithTag(tag);
        ui_object.GetComponent<Canvas>().enabled = is_visible;
        return;
    }

    private void UpdateCountdownStartTimer(float print_sec)
    {
        if(print_sec <= 0.0f)
        {
            DisplayUI(START_UI_TAG, false);
        }
        else
        {
            GameObject start_countdown = GameObject.FindGameObjectWithTag(START_COUNTDOWN_TAG);
            start_countdown.GetComponent<TextMeshProUGUI>().text = $"{Mathf.Ceil(print_sec):0}";
        }
        return;
    }

    private void SetupFieldTiles()
    {
        float TILE_SIZE = fieldblock_prefab_white.transform.localScale.x;
        Vector3 tile_position;
        Transform PARENT_TRANSFORM = this.transform;
        float ORIGIN_COORDINATE;
        if (STAGE_SIZE % 2 == 0)
        {
            ORIGIN_COORDINATE = (STAGE_SIZE / 2) * TILE_SIZE * (-1.0f) + (TILE_SIZE / 2.0f);
        }
        else
        {
            ORIGIN_COORDINATE = (STAGE_SIZE / 2) * TILE_SIZE * (-1.0f);
        }
        for (int i = 0; i < STAGE_SIZE; i++)
        {
            for (int j = 0; j < STAGE_SIZE; j++)
            {
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

    private void DisplayCursor(bool is_enable)
    {
        if (is_enable)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        return;
    }

    private void ConnectEventAction(bool is_connect_event)
    {
        if (is_connect_event)
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


    private void Update()
    {
        TransitionPauseState();
        if (IsPausing())
        {
            return;
        }
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
            UpdateTimelimitTimer();
        }
    }

    private void TransitionPauseState()
    {
        if (is_game_finished)
        {
            if (IsPausing())
            {
                Time.timeScale = 1.0f;
                DisplayCursor(true);
                DisplayUI(PAUSE_UI_TAG,false);
            }
        }
        if (is_game_started && !is_game_finished && Input.GetKeyDown(KeyCode.P))
        {
            PlaySE(PAUSE_SE_CLIP, Vector3.zero, PAUSE_BASE_VOLUME, false);
            if (IsPausing())
            {
                Time.timeScale = 1.0f;
                DisplayCursor(false);
                DisplayUI(PAUSE_UI_TAG, false);
            }
            else
            {
                Time.timeScale = 0.0f;
                DisplayCursor(true);
                DisplayUI(PAUSE_UI_TAG, true);
            }
        }
        return;
    }

    private bool IsPausing()
    {
        const float STOP_TIME_SCALE = 0.5f;
        if (Time.timeScale < STOP_TIME_SCALE)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void StartGame()
    {
        SpawnTarget();
        is_game_started = true;
        GameStart.Invoke();
        return;
    }

    private void SpawnTarget()
    {
        Vector3 player_position = GameObject.FindGameObjectWithTag("Player").transform.position;
        Vector3 spawn_position = new Vector3(
                Random.Range(SPAWN_RANGE_MIN.x, SPAWN_RANGE_MAX.x),
                Random.Range(SPAWN_RANGE_MIN.y, SPAWN_RANGE_MAX.y),
                Random.Range(SPAWN_RANGE_MIN.z, SPAWN_RANGE_MAX.z)
            );
        while(!ENABLE_NEAR_SPAWN && (Vector3.Distance(player_position,spawn_position) <= DONT_SPAWN_RADIUS)){
            spawn_position = new Vector3(
                Random.Range(SPAWN_RANGE_MIN.x, SPAWN_RANGE_MAX.x),
                Random.Range(SPAWN_RANGE_MIN.y, SPAWN_RANGE_MAX.y),
                Random.Range(SPAWN_RANGE_MIN.z, SPAWN_RANGE_MAX.z)
            );
        }
        GameObject spawn_target_object = Instantiate(TARGET_PREFAB, this.transform.position+spawn_position, Quaternion.identity);
        return;
    }

    private void TimeUpGame()
    {
        GameStop.Invoke();
        FinishGame();
        return;
    }

    private void FinishGame()
    {
        PlaySE(FINISH_SE_CLIP, Vector3.zero, FINISH_SE_BASE_VOLUME, false);
        is_game_finished = true;
        DisplayCursor(true);
        DisplayUI(FINISH_UI_TAG, true);
        return;
    }

    private void UpdateTimelimitTimer()
    {
        left_time_sec -= Time.deltaTime;
        if (left_time_sec <= 0.0f)
        {
            left_time_sec = 0.0f;
            TimeUpGame();
        }
        timer_textmesh.text = $"{Mathf.Ceil(left_time_sec):00}";
        return;
    }


    private void PlayBGM(AudioClip bgm_clip, float bgm_volume)
    {
        GameObject audio_object = new GameObject("TempAudioSource");

        AudioSource audio_source = audio_object.AddComponent<AudioSource>();
        audio_source.clip = bgm_clip;
        audio_source.volume = bgm_volume * PlayerPrefs.GetFloat(BGM_PREFS_KEY);
        audio_source.loop = true;
        audio_source.Play();
        return;
    }

    private void PlaySE(AudioClip se_clip,Vector3 se_position, float se_volume, bool is_3d)
    {
        GameObject audio_object = new GameObject("TempAudioSource");
        audio_object.transform.position = se_position;

        AudioSource audio_source = audio_object.AddComponent<AudioSource>();
        audio_source.clip = se_clip;
        audio_source.volume = se_volume * PlayerPrefs.GetFloat(SE_PREFS_KEY);
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

    private async ValueTask AsyncPlaySE2D(AudioClip se_clip, float se_volume)
    {
        GameObject audio_object = new GameObject("TempAudioSource");
        audio_object.transform.position = Vector3.zero;

        AudioSource audio_source = audio_object.AddComponent<AudioSource>();
        audio_source.clip = se_clip;
        audio_source.volume = se_volume * PlayerPrefs.GetFloat(SE_PREFS_KEY);
        const float SOUND_2D = 0.0f;
        audio_source.spatialBlend = SOUND_2D;
        audio_source.Play();

        Destroy(audio_object, se_clip.length);
        await Awaitable.WaitForSecondsAsync(se_clip.length);
        return;
    }


    private void GenerateBreakEffect(Vector3 break_position, float break_radius)
    {
        PlaySE(BREAK_SE_CLIP,break_position, break_radius* BREAK_BASE_VOLUME, true);
        GenerateBreakParticle(break_position, break_radius);
        return;
    }

    private void GenerateBreakParticle(Vector3 break_position, float break_radius)
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


    private void DeadTarget(Vector3 dead_position)
    {
        beat_cnt++;
        beat_count_textmesh.text = String.Format("{0:000}",beat_cnt);
        PlaySE(BEAT_SE_CLIP,dead_position,BEAT_SE_BASE_VOLUME,true);
        FlashDeadLight(dead_position);
        SpawnTarget();
        if(beat_cnt >= next_target_add_cnt)
        {
            appear_cnt += 1;
            next_target_add_cnt += appear_cnt * SPAWN_SCORE_RATE;
            PlaySE(ADD_SPAWN_SE_CLIP, Vector3.zero, ADD_SPAWN_SE_BASE_VOLUME, false);
            SpawnTarget();
        }
        return;
    }

    private void FlashDeadLight(Vector3 dead_position)
    {
        Vector3 flash_position = dead_position;
        GameObject dead_flash_object = Instantiate(DEAD_FLASH_PREFAB, dead_position, Quaternion.identity);
        dead_flash_object.transform.position = flash_position;
        Destroy(dead_flash_object, DEAD_FLASH_LIFETIME_SEC);
        return;
    }


    public void ResumeGame()
    {
        Time.timeScale = 1.0f;
        DisplayCursor(false);
        DisplayUI(PAUSE_UI_TAG, false);
        PlaySE(PAUSE_SE_CLIP, Vector3.zero, PAUSE_BASE_VOLUME, false);
        return;
    }

    private async void RestartGame()
    {
        if (!is_game_leaving)
        {
            is_game_leaving = true;
            PrepareLeaveScene();
            if (is_game_finished)
            {
                UpdateRanking();
            }
            await AsyncPlaySE2D(START_SE_CLIP, START_SE_BASE_VOLUME);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        return;
    }

    private void PrepareLeaveScene()
    {
        LeaveScene.Invoke();
        ConnectEventAction(false);
        return;
    }

    public async void BackToTitle()
    {
        if (!is_game_leaving)
        {
            is_game_leaving = true;
            GameStop.Invoke();
            PrepareLeaveScene();
            Time.timeScale = 1.0f;
            if (is_game_finished)
            {
                UpdateRanking();
            }
            await AsyncPlaySE2D(TITLE_BACK_SE_CLIP, TITLE_BACK_SE_BASE_VOLUME);
            SceneManager.LoadScene("TitleScene");
        }
        return;
    }

    private void UpdateRanking()
    {
        int[] scores =
        {
            PlayerPrefs.GetInt("Score1st",0),
            PlayerPrefs.GetInt("Score2nd",0),
            PlayerPrefs.GetInt("Score3rd",0),
            PlayerPrefs.GetInt("Score4th",0),
            PlayerPrefs.GetInt("Score5th",0),
            beat_cnt
        };
        Array.Sort(scores);
        Array.Reverse(scores);

        PlayerPrefs.SetInt("Score1st", scores[0]);
        PlayerPrefs.SetInt("Score2nd", scores[1]);
        PlayerPrefs.SetInt("Score3rd", scores[2]);
        PlayerPrefs.SetInt("Score4th", scores[3]);
        PlayerPrefs.SetInt("Score5th", scores[4]);
        PlayerPrefs.Save();
        return;
    }
}
