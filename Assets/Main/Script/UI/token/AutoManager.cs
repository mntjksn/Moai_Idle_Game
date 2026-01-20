using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AutoTimeManager : MonoBehaviour
{
    public static AutoTimeManager Instance;

    // 현재 세이브 데이터(틱 루프에서 1회 로드해서 캐싱)
    private GameData data;

    // 작업 5분 / 쿨타임 10분
    private const float WORK_TIME = 300f;
    private const float COOLDOWN_TIME = 600f;

    [Header("UI")]
    public GameObject autoSpawnPanel;
    public GameObject autoMergePanel;

    public Slider autoSpawnSlider;
    public Slider autoMergeSlider;

    public TextMeshProUGUI autoSpawnText;
    public TextMeshProUGUI autoMergeText;

    [Header("Toggle Buttons")]
    public Button spawnToggleButton;
    public TextMeshProUGUI spawnToggleText;
    public Image spawnToggleImage;

    public Button mergeToggleButton;
    public TextMeshProUGUI mergeToggleText;
    public Image mergeToggleImage;

    // 토글 버튼 색상(ON / OFF)
    private readonly Color onColor = new Color(1f, 1f, 1f, 1f);
    private readonly Color offColor = new Color(0.6f, 0.6f, 0.6f, 1f);

    // Update 대신 코루틴 루프로 틱을 돌림
    private Coroutine tickRoutine;

    // 저장 난사 방지용
    private bool dirty = false;         // 변경이 있었는지 플래그
    private float saveCooldown = 1.0f;  // 최소 1초마다만 저장
    private float saveTimer = 0f;

    // 로직 틱 주기(Update 대체)
    private const float TICK_INTERVAL = 0.2f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 버튼 리스너는 Start에서 1회만(중복 등록 방지)
        if (spawnToggleButton != null)
        {
            spawnToggleButton.onClick.RemoveListener(ToggleAutoSpawn);
            spawnToggleButton.onClick.AddListener(ToggleAutoSpawn);
        }

        if (mergeToggleButton != null)
        {
            mergeToggleButton.onClick.RemoveListener(ToggleAutoMerge);
            mergeToggleButton.onClick.AddListener(ToggleAutoMerge);
        }

        // 최초 UI 세팅
        ForceUpdateUI();

        // Update 대신 코루틴 루프 시작
        if (tickRoutine != null)
            StopCoroutine(tickRoutine);

        tickRoutine = StartCoroutine(TickLoop());
    }

    private void OnDisable()
    {
        // 패널이 꺼질 때 루프 중단
        if (tickRoutine != null)
        {
            StopCoroutine(tickRoutine);
            tickRoutine = null;
        }

        // 남은 변경 사항이 있으면 강제 저장
        FlushSave();
    }

    // =====================================================
    // Update 대체 루프
    // - 일정 주기로 데이터 로드 1회
    // - 타이머 진행
    // - UI 갱신(추가 Load 금지)
    // - 저장은 dirty + 쿨다운 조건일 때만
    // =====================================================
    private IEnumerator TickLoop()
    {
        var wait = new WaitForSeconds(TICK_INTERVAL);

        while (true)
        {
            // 1) 데이터 1회 로드(틱당 1번만)
            data = SaveManager.Load();

            // 2) 타이머 진행(변경 여부 반환)
            bool changed = StepAutoTimers(TICK_INTERVAL);

            // 3) UI 갱신(현재 data만 사용, Load 금지)
            UpdateAutoUI_FromCachedData();

            // 4) 변경이 있었으면 저장 대상 표시
            if (changed)
                MarkDirty();

            // 5) 저장은 쿨타임/dirty 조건 만족할 때만 수행
            TrySave(TICK_INTERVAL);

            yield return wait;
        }
    }

    // =====================================================
    // 타이머 1스텝 진행
    // - 자동 소환 / 자동 합치기 각각 처리
    // - 상태가 바뀌면 changed=true로 반환
    // =====================================================
    private bool StepAutoTimers(float dt)
    {
        bool changed = false;

        // -----------------------
        // 자동 소환 타이머
        // -----------------------
        if (data.settings.autoSpawnPurchased && data.settings.autoSpawnEnabled)
        {
            // 1) 작업 시간 감소
            if (data.settings.autoSpawnActive)
            {
                float before = data.settings.autoSpawnRemain;
                data.settings.autoSpawnRemain = Mathf.Max(0f, before - dt);

                if (!Mathf.Approximately(before, data.settings.autoSpawnRemain))
                    changed = true;

                // 작업 시간 종료 → 자동 소환 실행 → 쿨타임 시작
                if (data.settings.autoSpawnRemain <= 0f)
                {
                    if (AutoSpawner.Instance != null)
                        AutoSpawner.Instance.RunAutoSpawn();

                    data.settings.autoSpawnActive = false;
                    data.settings.autoSpawnCooldown = COOLDOWN_TIME;
                    changed = true;
                }
            }
            // 2) 쿨타임 감소
            else
            {
                if (data.settings.autoSpawnCooldown > 0f)
                {
                    float before = data.settings.autoSpawnCooldown;
                    data.settings.autoSpawnCooldown = Mathf.Max(0f, before - dt);

                    if (!Mathf.Approximately(before, data.settings.autoSpawnCooldown))
                        changed = true;

                    // 쿨 종료 → 다시 작업 시작
                    if (data.settings.autoSpawnCooldown <= 0f)
                    {
                        data.settings.autoSpawnActive = true;
                        data.settings.autoSpawnRemain = WORK_TIME;
                        changed = true;
                    }
                }
            }
        }

        // -----------------------
        // 자동 합치기 타이머
        // -----------------------
        if (data.settings.autoMergePurchased && data.settings.autoMergeEnabled)
        {
            // 1) 작업 시간 감소
            if (data.settings.autoMergeActive)
            {
                float before = data.settings.autoMergeRemain;
                data.settings.autoMergeRemain = Mathf.Max(0f, before - dt);

                if (!Mathf.Approximately(before, data.settings.autoMergeRemain))
                    changed = true;

                // 작업 시간 종료 → 자동 합치기 실행 → 쿨타임 시작
                if (data.settings.autoMergeRemain <= 0f)
                {
                    if (AutoMerger.Instance != null)
                        AutoMerger.Instance.RunAutoMerge();

                    data.settings.autoMergeActive = false;
                    data.settings.autoMergeCooldown = COOLDOWN_TIME;
                    changed = true;
                }
            }
            // 2) 쿨타임 감소
            else
            {
                if (data.settings.autoMergeCooldown > 0f)
                {
                    float before = data.settings.autoMergeCooldown;
                    data.settings.autoMergeCooldown = Mathf.Max(0f, before - dt);

                    if (!Mathf.Approximately(before, data.settings.autoMergeCooldown))
                        changed = true;

                    // 쿨 종료 → 다시 작업 시작
                    if (data.settings.autoMergeCooldown <= 0f)
                    {
                        data.settings.autoMergeActive = true;
                        data.settings.autoMergeRemain = WORK_TIME;
                        changed = true;
                    }
                }
            }
        }

        return changed;
    }

    // =====================================================
    // UI 강제 갱신(패널 표시 포함)
    // - 외부에서 "지금 상태로 UI 다시 그려줘" 할 때 사용
    // =====================================================
    public void ForceUpdateUI()
    {
        data = SaveManager.Load();

        if (autoSpawnPanel != null)
            autoSpawnPanel.SetActive(data.settings.autoSpawnPurchased);

        if (autoMergePanel != null)
            autoMergePanel.SetActive(data.settings.autoMergePurchased);

        UpdateAutoUI_FromCachedData();
    }

    // =====================================================
    // UI 갱신(Load 금지)
    // - TickLoop에서 로드한 data를 그대로 사용
    // =====================================================
    private void UpdateAutoUI_FromCachedData()
    {
        // 패널 표시 여부
        if (autoSpawnPanel != null)
            autoSpawnPanel.SetActive(data.settings.autoSpawnPurchased);

        if (autoMergePanel != null)
            autoMergePanel.SetActive(data.settings.autoMergePurchased);

        // -----------------------
        // 자동 소환 UI
        // -----------------------
        if (data.settings.autoSpawnPurchased)
        {
            if (data.settings.autoSpawnActive)
            {
                float elapsed = WORK_TIME - data.settings.autoSpawnRemain;

                if (autoSpawnSlider != null)
                {
                    autoSpawnSlider.maxValue = WORK_TIME;
                    autoSpawnSlider.value = elapsed;
                }

                if (autoSpawnText != null)
                    autoSpawnText.text = $"{Mathf.FloorToInt(elapsed / 60f)}분 / 5분";
            }
            else
            {
                float cd = data.settings.autoSpawnCooldown;

                if (autoSpawnSlider != null)
                {
                    autoSpawnSlider.maxValue = COOLDOWN_TIME;
                    autoSpawnSlider.value = COOLDOWN_TIME - cd;
                }

                int min = Mathf.FloorToInt((COOLDOWN_TIME - cd) / 60f);

                if (autoSpawnText != null)
                    autoSpawnText.text = $"쿨타임 {min}분 / 10분";
            }
        }

        // -----------------------
        // 자동 합치기 UI
        // -----------------------
        if (data.settings.autoMergePurchased)
        {
            if (data.settings.autoMergeActive)
            {
                float elapsed = WORK_TIME - data.settings.autoMergeRemain;

                if (autoMergeSlider != null)
                {
                    autoMergeSlider.maxValue = WORK_TIME;
                    autoMergeSlider.value = elapsed;
                }

                if (autoMergeText != null)
                    autoMergeText.text = $"{Mathf.FloorToInt(elapsed / 60f)}분 / 5분";
            }
            else
            {
                float cd = data.settings.autoMergeCooldown;

                if (autoMergeSlider != null)
                {
                    autoMergeSlider.maxValue = COOLDOWN_TIME;
                    autoMergeSlider.value = COOLDOWN_TIME - cd;
                }

                int min = Mathf.FloorToInt((COOLDOWN_TIME - cd) / 60f);

                if (autoMergeText != null)
                    autoMergeText.text = $"쿨타임 {min}분 / 10분";
            }
        }

        // 토글 버튼 UI 갱신
        UpdateToggleUI_FromCachedData();
    }

    // 토글 버튼 텍스트/색상 갱신(Load 금지)
    private void UpdateToggleUI_FromCachedData()
    {
        if (spawnToggleText != null)
            spawnToggleText.text = data.settings.autoSpawnEnabled ? "ON" : "OFF";

        if (spawnToggleImage != null)
            spawnToggleImage.color = data.settings.autoSpawnEnabled ? onColor : offColor;

        if (mergeToggleText != null)
            mergeToggleText.text = data.settings.autoMergeEnabled ? "ON" : "OFF";

        if (mergeToggleImage != null)
            mergeToggleImage.color = data.settings.autoMergeEnabled ? onColor : offColor;
    }

    // =====================================================
    // 토글 버튼 클릭
    // - 클릭은 즉시 저장(유저 입력은 반영이 바로 되어야 해서)
    // =====================================================
    private void ToggleAutoSpawn()
    {
        data = SaveManager.Load();

        if (!data.settings.autoSpawnPurchased)
            return;

        data.settings.autoSpawnEnabled = !data.settings.autoSpawnEnabled;

        // ON으로 켰는데 타이머가 둘 다 0이면 초기 세팅(선택)
        if (data.settings.autoSpawnEnabled)
        {
            if (data.settings.autoSpawnRemain <= 0f && data.settings.autoSpawnCooldown <= 0f)
            {
                data.settings.autoSpawnActive = true;
                data.settings.autoSpawnRemain = WORK_TIME;
            }
        }

        SaveManager.Save(data);

        // UI 즉시 반영
        MarkDirty();
        UpdateAutoUI_FromCachedData();
    }

    private void ToggleAutoMerge()
    {
        data = SaveManager.Load();

        if (!data.settings.autoMergePurchased)
            return;

        data.settings.autoMergeEnabled = !data.settings.autoMergeEnabled;

        if (data.settings.autoMergeEnabled)
        {
            if (data.settings.autoMergeRemain <= 0f && data.settings.autoMergeCooldown <= 0f)
            {
                data.settings.autoMergeActive = true;
                data.settings.autoMergeRemain = WORK_TIME;
            }
        }

        SaveManager.Save(data);

        MarkDirty();
        UpdateAutoUI_FromCachedData();
    }

    // =====================================================
    // 저장 제어
    // - TickLoop에서 자주 변경되는 값은 dirty로 모았다가
    //   saveCooldown마다 한 번씩만 저장
    // =====================================================
    private void MarkDirty()
    {
        dirty = true;
    }

    private void TrySave(float dt)
    {
        saveTimer += dt;

        // 변경이 없으면 저장할 필요 없음
        if (!dirty) return;

        // 쿨다운이 끝나야 저장
        if (saveTimer < saveCooldown) return;

        SaveManager.Save(data);
        dirty = false;
        saveTimer = 0f;
    }

    // 남은 변경이 있으면 즉시 저장(패널 꺼질 때 등)
    private void FlushSave()
    {
        if (!dirty) return;

        SaveManager.Save(data);
        dirty = false;
        saveTimer = 0f;
    }
}