using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AutoTimeManager : MonoBehaviour
{
    public static AutoTimeManager Instance;

    private GameData data;

    private const float WORK_TIME = 300f;     // 5분
    private const float COOLDOWN_TIME = 600f; // 10분

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

    // 색상
    private Color onColor = new Color(1f, 1f, 1f, 1f);
    private Color offColor = new Color(0.6f, 0.6f, 0.6f, 1f);

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ForceUpdateUI();

        spawnToggleButton.onClick.AddListener(ToggleAutoSpawn);
        mergeToggleButton.onClick.AddListener(ToggleAutoMerge);
    }

    private void Update()
    {
        data = SaveManager.Load();
        float dt = Time.deltaTime;

        // ================================
        // 자동 소환
        // ================================
        if (data.settings.autoSpawnPurchased && data.settings.autoSpawnEnabled)
        {
            // 1) 작업 시간 감소 중
            if (data.settings.autoSpawnActive)
            {
                data.settings.autoSpawnRemain -= dt;

                if (data.settings.autoSpawnRemain <= 0)
                {
                    AutoSpawner.Instance.RunAutoSpawn();
                    data.settings.autoSpawnRemain = 0;

                    // 작업 종료 → 쿨타임 시작
                    data.settings.autoSpawnActive = false;
                    data.settings.autoSpawnCooldown = COOLDOWN_TIME;
                }
            }
            else
            {
                // 2) 쿨타임 감소 중
                if (data.settings.autoSpawnCooldown > 0)
                {
                    data.settings.autoSpawnCooldown -= dt;

                    if (data.settings.autoSpawnCooldown <= 0)
                    {
                        // 쿨타임 종료 → 다시 활성
                        data.settings.autoSpawnActive = true;
                        data.settings.autoSpawnRemain = WORK_TIME;
                    }
                }
            }
        }

        // ================================
        // 자동 합치기
        // ================================
        if (data.settings.autoMergePurchased && data.settings.autoMergeEnabled)
        {
            if (data.settings.autoMergeActive)
            {
                data.settings.autoMergeRemain -= dt;

                if (data.settings.autoMergeRemain <= 0)
                {
                    AutoMerger.Instance.RunAutoMerge();
                    data.settings.autoMergeRemain = 0;

                    data.settings.autoMergeActive = false;
                    data.settings.autoMergeCooldown = COOLDOWN_TIME;
                }
            }
            else
            {
                if (data.settings.autoMergeCooldown > 0)
                {
                    data.settings.autoMergeCooldown -= dt;

                    if (data.settings.autoMergeCooldown <= 0)
                    {
                        data.settings.autoMergeActive = true;
                        data.settings.autoMergeRemain = WORK_TIME;
                    }
                }
            }
        }

        SaveManager.Save(data);
        UpdateAutoUI();
    }

    // ================================
    // UI 업데이트
    // ================================
    public void ForceUpdateUI()
    {
        data = SaveManager.Load();

        autoSpawnPanel.SetActive(data.settings.autoSpawnPurchased);
        autoMergePanel.SetActive(data.settings.autoMergePurchased);

        UpdateAutoUI();
    }

    private void UpdateAutoUI()
    {
        data = SaveManager.Load();

        // 자동 소환 UI
        if (data.settings.autoSpawnPurchased)
        {
            if (data.settings.autoSpawnActive)
            {
                float elapsed = WORK_TIME - data.settings.autoSpawnRemain;

                autoSpawnSlider.maxValue = WORK_TIME;
                autoSpawnSlider.value = elapsed;

                autoSpawnText.text =
                    $"{Mathf.FloorToInt(elapsed / 60f)}분 / 5분";
            }
            else
            {
                float cd = data.settings.autoSpawnCooldown;

                autoSpawnSlider.maxValue = COOLDOWN_TIME;
                autoSpawnSlider.value = COOLDOWN_TIME - cd;

                int min = Mathf.FloorToInt((COOLDOWN_TIME - cd) / 60f);
                autoSpawnText.text =
                    $"쿨타임 {min}분 / 10분";
            }
        }

        // 자동 합치기 UI
        if (data.settings.autoMergePurchased)
        {
            if (data.settings.autoMergeActive)
            {
                float elapsed = WORK_TIME - data.settings.autoMergeRemain;

                autoMergeSlider.maxValue = WORK_TIME;
                autoMergeSlider.value = elapsed;

                autoMergeText.text =
                    $"{Mathf.FloorToInt(elapsed / 60f)}분 / 5분";
            }
            else
            {
                float cd = data.settings.autoMergeCooldown;

                autoMergeSlider.maxValue = COOLDOWN_TIME;
                autoMergeSlider.value = COOLDOWN_TIME - cd;

                int min = Mathf.FloorToInt((COOLDOWN_TIME - cd) / 60f);
                autoMergeText.text =
                    $"쿨타임 {min}분 / 10분";
            }
        }
        UpdateToggleUI();
    }

    private void ToggleAutoSpawn()
    {
        data = SaveManager.Load();

        // OFF → ON 전환 가능 (기능 구입했을 때만)
        if (data.settings.autoSpawnPurchased)
        {
            data.settings.autoSpawnEnabled = !data.settings.autoSpawnEnabled;
            SaveManager.Save(data);
            UpdateToggleUI();
        }
    }

    private void ToggleAutoMerge()
    {
        data = SaveManager.Load();

        if (data.settings.autoMergePurchased)
        {
            data.settings.autoMergeEnabled = !data.settings.autoMergeEnabled;
            SaveManager.Save(data);
            UpdateToggleUI();
        }
    }

    private void UpdateToggleUI()
    {
        data = SaveManager.Load();

        // 자동 소환 버튼 UI
        if (data.settings.autoSpawnEnabled)
        {
            spawnToggleText.text = "ON";
            spawnToggleImage.color = onColor;
        }
        else
        {
            spawnToggleText.text = "OFF";
            spawnToggleImage.color = offColor;
        }

        // 자동 합치기 버튼 UI
        if (data.settings.autoMergeEnabled)
        {
            mergeToggleText.text = "ON";
            mergeToggleImage.color = onColor;
        }
        else
        {
            mergeToggleText.text = "OFF";
            mergeToggleImage.color = offColor;
        }
    }
}