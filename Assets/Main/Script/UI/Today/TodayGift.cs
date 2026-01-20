using UnityEngine;
using TMPro;
using UnityEngine.UI;

// 하루 1회, "오늘 플레이 시간"이 목표 시간(분)만큼 채워지면 보상을 지급하는 스크립트
// - 날짜가 바뀌면 플레이 시간/수령 여부를 초기화
// - 플레이 중에는 일정 주기로 저장해서 진행도 보존
public class TodayGift : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    [Header("Reward")]
    public GameObject gift; // 목표 달성 시 생성할 보상 프리팹

    [Header("UI")]
    public TextMeshProUGUI sliderText; // "현재분 / 목표분" 표시
    public Slider slider;              // 플레이 타임 게이지

    // 세이브 데이터
    private GameData data;

    // 저장 타이머(너무 자주 저장하면 부담이므로 주기 저장)
    private float saveTimer = 0f;

    private void Awake()
    {
        // 데이터 로드 후 날짜 변경 여부 확인
        data = SaveManager.Load();
        CheckNewDay();
    }

    private void Update()
    {
        saveTimer += Time.deltaTime;

        // 오늘 보상을 아직 안 받았으면 플레이 타임 누적
        if (!data.dailyReward.rewardGivenToday)
            data.dailyReward.playTimeToday += Time.deltaTime;

        // 현재 누적 플레이 시간(초)
        float playedSeconds = data.dailyReward.playTimeToday;

        // 슬라이더 목표값(초) = 목표 분 * 60
        slider.maxValue = data.dailyReward.playTimeTodayMax * 60f;
        slider.value = playedSeconds;

        // 표시 텍스트는 분 단위(내림)
        int currentMin = Mathf.FloorToInt(playedSeconds / 60f);
        sliderText.text = $"{currentMin}분 / {data.dailyReward.playTimeTodayMax}분";

        // 목표 시간 달성 시 보상 지급(딱 1번만)
        if (!data.dailyReward.rewardGivenToday && playedSeconds >= slider.maxValue)
        {
            GiveReward();
        }

        // 5초마다 저장(진행도 보존)
        if (saveTimer >= 5f)
        {
            SaveManager.Save(data);
            saveTimer = 0f;
        }
    }

    // 날짜가 바뀌었는지 확인해서 오늘 보상 상태를 초기화
    private void CheckNewDay()
    {
        string today = System.DateTime.Now.ToString("yyyy-MM-dd");

        // 마지막 저장 날짜와 다르면 "새로운 날"로 판단
        if (data.dailyReward.lastRewardDate != today)
        {
            data.dailyReward.lastRewardDate = today;
            data.dailyReward.playTimeToday = 0f;
            data.dailyReward.rewardGivenToday = false;

            SaveManager.Save(data);
        }
    }

    // 보상 지급 처리
    private void GiveReward()
    {
        PlaySFX();

        data.dailyReward.rewardGivenToday = true;
        data.dailyReward.rewardCheck++;

        // 보상 팝업 생성(캔버스 하위에)
        var canvas2 = GameObject.Find("Canvas2")?.transform;
        if (canvas2 != null && gift != null)
            Instantiate(gift, Vector3.zero, Quaternion.identity, canvas2);

        SaveManager.Save(data);
    }

    // 효과음 재생
    private void PlaySFX()
    {
        if (audioSource == null) return;

        if (Setting.IsSFXOn())
            audioSource.Play();
    }
}