using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 개별 미션 UI 하나를 담당하는 클래스
// - 진행도 표시
// - 보상 수령 처리
// - MissionManager에서 index를 받아 초기화됨
public class Mission : MonoBehaviour
{
    // 미션 인덱스 (MissionManager에서 주입)
    public int index_misson;

    [Header("UI")]
    public GameObject gift;
    public Button button;
    public Slider slider;
    public TextMeshProUGUI main_text, button_text, slider_text;
    public Image image;

    [SerializeField] private AudioSource audioSource;

    // 데이터 캐싱
    private MissionData mission;
    private GameData data;

    private void Awake()
    {
        Input.multiTouchEnabled = false;

        // 기본 상태: 보상 버튼 비활성
        button.interactable = false;

        // 보상 버튼 클릭 이벤트
        button.onClick.AddListener(OnMissionClick);
    }

    /// <summary>
    /// MissionManager에서 미션 인덱스를 전달받아 초기화
    /// </summary>
    public void Setup(int index)
    {
        index_misson = index;
        Refresh();  // 최초 UI 갱신
    }

    private void OnEnable()
    {
        Refresh(); // 패널 열릴 때 최신 상태 반영
    }

    /// <summary>
    /// 미션 데이터 로드 + UI 갱신
    /// </summary>
    private void Refresh()
    {
        data = SaveManager.Load();
        mission = GetMissionData(data);

        if (mission == null) return;

        // 텍스트 갱신
        main_text.text = mission.desc;
        button_text.text = $"{mission.reward:#,0}개";
        slider_text.text = $"{mission.current:#,0} / {mission.max:#,0}";

        // 슬라이더 진행도
        slider.maxValue = mission.max;
        slider.value = mission.current;

        // 보상 아이콘
        if (mission.icon != null)
            image.sprite = mission.icon;

        // 보상 수령 가능 여부
        button.interactable = mission.current >= mission.max;
    }

    /// <summary>
    /// index에 따른 미션 데이터를 구조화해서 반환
    /// (읽기 전용)
    /// </summary>
    private MissionData GetMissionData(GameData d)
    {
        switch (index_misson)
        {
            case 0:
                return new MissionData(
                    d.missions.mission_2_value,
                    d.missions.mission_2_max,
                    d.missions.mission_2_reward,
                    "신규 모아이를 획득하세요.",
                    Resources.Load<Sprite>("dia"));

            case 1:
                return new MissionData(
                    d.missions.mission_3_value,
                    d.missions.mission_3_max,
                    d.missions.mission_3_reward,
                    "모아이를 소환하세요.",
                    Resources.Load<Sprite>("gold"));

            case 2:
                return new MissionData(
                    d.missions.mission_4_value,
                    d.missions.mission_4_max,
                    d.missions.mission_4_reward,
                    "모아이를 합치세요.",
                    Resources.Load<Sprite>("gold"));

            case 3:
                return new MissionData(
                    d.missions.mission_5_value,
                    d.missions.mission_5_max,
                    d.missions.mission_5_reward,
                    "모아이를 통해 돌멩이를 획득하세요.",
                    Resources.Load<Sprite>("gold"));

            case 4:
                return new MissionData(
                    d.missions.mission_6_value,
                    d.missions.mission_6_max,
                    d.missions.mission_6_reward,
                    "행운 시험을 이용하세요.",
                    Resources.Load<Sprite>("dia"));

            case 5:
                return new MissionData(
                    d.missions.mission_7_value,
                    d.missions.mission_7_max,
                    d.missions.mission_7_reward,
                    "깜짝상자를 획득하세요.",
                    Resources.Load<Sprite>("dia"));

            case 6:
                return new MissionData(
                    d.missions.mission_8_value,
                    d.missions.mission_8_max,
                    d.missions.mission_8_reward,
                    "업그레이드를 이용하세요.",
                    Resources.Load<Sprite>("ticket"));
        }

        return null;
    }

    /// <summary>
    /// 보상 수령 버튼 클릭 처리
    /// </summary>
    private void OnMissionClick()
    {
        data = SaveManager.Load();

        if (Setting.IsSFXOn())
            audioSource.Play();

        switch (index_misson)
        {
            case 0:
                data.missions.mission_2_tic++;
                data.missions.mission_2_max += 1;
                data.currency.dia += data.missions.mission_2_reward;
                data.missions.mission_2_reward += 5;
                break;

            case 1:
                HandleGoldMission(
                    ref data.missions.mission_3_value,
                    ref data.missions.mission_3_max,
                    ref data.missions.mission_3_reward,
                    ref data.missions.mission_3_tic,
                    10);
                break;

            case 2:
                HandleGoldMission(
                    ref data.missions.mission_4_value,
                    ref data.missions.mission_4_max,
                    ref data.missions.mission_4_reward,
                    ref data.missions.mission_4_tic,
                    15);
                break;

            case 3:
                data.currency.gold += data.missions.mission_5_reward;
                data.missions.mission_5_value -= data.missions.mission_5_max;
                data.missions.mission_5_tic++;
                data.missions.mission_5_max += data.missions.mission_5_max / 5;
                data.missions.mission_5_reward =
                    data.missions.mission_5_max / 2 + data.missions.mission_5_tic * 20;
                break;

            case 4:
                data.currency.dia += data.missions.mission_6_reward;
                data.missions.mission_6_value -= data.missions.mission_6_max;
                data.missions.mission_6_tic++;
                data.missions.mission_6_max += 1;
                data.missions.mission_6_reward += 5 * (data.missions.mission_6_tic / 5 + 1);
                break;

            case 5:
                data.currency.dia += data.missions.mission_7_reward;
                data.missions.mission_7_value -= data.missions.mission_7_max;
                data.missions.mission_7_tic++;
                data.missions.mission_7_max += 1;
                data.missions.mission_7_reward += 10 * (data.missions.mission_7_tic / 5 + 1);
                break;

            case 6:
                data.missions.mission_8_tic++;
                data.missions.mission_8_max++;
                data.currency.ticket += data.missions.mission_8_reward;
                data.missions.mission_8_reward = data.missions.mission_8_tic / 5 + 1;
                break;
        }

        SaveManager.Save(data);
        Refresh(); // UI 즉시 갱신
    }

    /// <summary>
    /// 골드 보상 계열 미션 공통 처리
    /// </summary>
    private void HandleGoldMission(
        ref int value, ref int max, ref int reward, ref int tic, int rewardMul)
    {
        data.currency.gold += reward;
        value -= max;

        tic++;
        max += 2;

        reward += reward / 20 + tic * rewardMul;

        if (data.currency.gold > 2147483600)
            data.currency.gold = 2147483600;
    }
}

// 미션 UI 표시용 데이터 구조체
public class MissionData
{
    public int current;
    public int max;
    public int reward;
    public string desc;
    public Sprite icon;

    public MissionData(int current, int max, int reward, string desc, Sprite icon)
    {
        this.current = current;
        this.max = max;
        this.reward = reward;
        this.desc = desc;
        this.icon = icon;
    }
}