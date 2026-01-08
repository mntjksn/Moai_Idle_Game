using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Mission : MonoBehaviour
{
    public int index_misson;

    [Header("UI")]
    public GameObject gift;
    public Button button;
    public Slider slider;
    public TextMeshProUGUI main_text, button_text, slider_text;
    public Image image;

    [SerializeField] private AudioSource audioSource;

    private MissionData mission;  // ★ 캐싱용
    private GameData data;        // ★ 캐싱용

    private void Awake()
    {
        Input.multiTouchEnabled = false;
        button.interactable = false;
        button.onClick.AddListener(OnMissionClick);
    }

    // ★ MissonManager에서 index 받아서 설정
    public void Setup(int index)
    {
        index_misson = index;
        Refresh();  // UI 1회 갱신
    }

    private void OnEnable()
    {
        Refresh();
    }

    /// <summary>
    /// ★ 데이터 & UI 한 번만 갱신
    /// </summary>
    private void Refresh()
    {
        data = SaveManager.Load();
        mission = GetMissionData(data);

        if (mission == null) return;

        // 텍스트
        main_text.text = mission.desc;
        button_text.text = $"{mission.reward:#,0}개";
        slider_text.text = $"{mission.current:#,0} / {mission.max:#,0}";

        // 슬라이더
        slider.maxValue = mission.max;
        slider.value = mission.current;

        // 보상 아이콘
        if (mission.icon != null)
            image.sprite = mission.icon;

        // 버튼 활성화
        button.interactable = mission.current >= mission.max;
    }

    /// <summary>
    /// 각 미션 데이터를 구조화해서 가져오기
    /// </summary>
    private MissionData GetMissionData(GameData d)
    {
        switch (index_misson)
        {
            case 0:
                return new MissionData(
                    d.missions.mission_2_value, d.missions.mission_2_max,
                    d.missions.mission_2_reward,
                    "신규 모아이를 획득하세요.",
                    Resources.Load<Sprite>("dia"));

            case 1:
                return new MissionData(
                    d.missions.mission_3_value, d.missions.mission_3_max,
                    d.missions.mission_3_reward,
                    "모아이를 소환하세요.",
                    Resources.Load<Sprite>("gold"));

            case 2:
                return new MissionData(
                    d.missions.mission_4_value, d.missions.mission_4_max,
                    d.missions.mission_4_reward,
                    "모아이를 합치세요.",
                    Resources.Load<Sprite>("gold"));

            case 3:
                return new MissionData(
                    d.missions.mission_5_value, d.missions.mission_5_max,
                    d.missions.mission_5_reward,
                    "모아이를 통해 돌멩이를 획득하세요.",
                    Resources.Load<Sprite>("gold"));

            case 4:
                return new MissionData(
                    d.missions.mission_6_value, d.missions.mission_6_max,
                    d.missions.mission_6_reward,
                    "행운 시험을 이용하세요.",
                    Resources.Load<Sprite>("dia"));

            case 5:
                return new MissionData(
                    d.missions.mission_7_value, d.missions.mission_7_max,
                    d.missions.mission_7_reward,
                    "깜짝상자를 획득하세요.",
                    Resources.Load<Sprite>("dia"));

            case 6:
                return new MissionData(
                    d.missions.mission_8_value, d.missions.mission_8_max,
                    d.missions.mission_8_reward,
                    "업그레이드를 이용하세요.",
                    Resources.Load<Sprite>("ticket"));
        }

        return null;
    }

    /// <summary>
    /// 보상 버튼 클릭
    /// </summary>
    public void OnMissionClick()
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
                data.currency.gold += data.missions.mission_3_reward;
                data.missions.mission_3_value -= data.missions.mission_3_max;

                if (data.missions.mission_3_max >= 100)
                    data.missions.mission_3_max = 100;
                else
                {
                    data.missions.mission_3_tic++;
                    data.missions.mission_3_max += 2;

                    if (data.currency.gold < 2147483600)
                        data.missions.mission_3_reward += data.missions.mission_3_reward / 20 + data.missions.mission_3_tic * 10;
                    else
                        data.currency.gold = 2147483600;
                }
                break;

            case 2:
                data.currency.gold += data.missions.mission_4_reward;
                data.missions.mission_4_value -= data.missions.mission_4_max;

                if (data.missions.mission_4_max >= 100)
                    data.missions.mission_4_max = 100;
                else
                {
                    data.missions.mission_4_tic++;
                    data.missions.mission_4_max += 2;

                    if (data.currency.gold < 2147483600)
                        data.missions.mission_4_reward += data.missions.mission_4_reward / 20 + data.missions.mission_4_tic * 15;
                    else
                        data.currency.gold = 2147483600;
                }
                break;

            case 3:
                data.currency.gold += data.missions.mission_5_reward;
                data.missions.mission_5_value -= data.missions.mission_5_max;

                if (data.missions.mission_5_max >= 1000000)
                    data.missions.mission_5_max = 1000000;
                else
                {
                    data.missions.mission_5_tic++;
                    data.missions.mission_5_max += data.missions.mission_5_max / 5;

                    if (data.currency.gold < 2147483600)
                        data.missions.mission_5_reward = data.missions.mission_5_max / 2 + data.missions.mission_5_tic * 20;
                    else
                        data.currency.gold = 2147483600;
                }
                break;

            case 4:
                data.currency.dia += data.missions.mission_6_reward;
                data.missions.mission_6_value -= data.missions.mission_6_max;

                if (data.missions.mission_6_max >= 10)
                    data.missions.mission_6_max = 10;
                else
                {
                    data.missions.mission_6_tic++;
                    data.missions.mission_6_max += 1;
                    data.missions.mission_6_reward += 5 * (data.missions.mission_6_tic / 5 + 1);
                }

                break;

            case 5:
                data.currency.dia += data.missions.mission_7_reward;
                data.missions.mission_7_value -= data.missions.mission_7_max;

                if (data.missions.mission_7_max >= 10)
                    data.missions.mission_7_max = 10;
                else
                {
                    data.missions.mission_7_tic++;
                    data.missions.mission_7_max += 1;
                    data.missions.mission_7_reward += 10 * (data.missions.mission_7_tic / 5 + 1);
                }

                break;

            case 6:
                data.missions.mission_8_tic++;
                data.missions.mission_8_max++;
                data.currency.ticket += data.missions.mission_8_reward;
                data.missions.mission_8_reward = data.missions.mission_8_tic / 5 + 1;
                break;
        }

        SaveManager.Save(data);

        // UI 갱신
        Refresh();
    }
}

public class MissionData
{
    public int current, max, reward;
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