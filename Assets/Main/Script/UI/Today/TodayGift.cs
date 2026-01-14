using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TodayGift : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    public GameObject gift;

    public TextMeshProUGUI sliderText;
    public Slider slider;

    private GameData data;
    private float saveTimer = 0f;

    private void Awake()
    {
        data = SaveManager.Load();
        CheckNewDay();
    }

    private void Update()
    {
        saveTimer += Time.deltaTime;

        // 오늘 보상 안받았으면 플레이 타임 증가
        if (!data.dailyReward.rewardGivenToday)
            data.dailyReward.playTimeToday += Time.deltaTime;

        // 실제 경과한 시간 (초 단위)
        float playedSeconds = data.dailyReward.playTimeToday;

        // UI 게이지 부드럽게 증가하도록 설정
        slider.maxValue = data.dailyReward.playTimeTodayMax * 60f; // ex) 15분 → 900초
        slider.value = playedSeconds;

        // 텍스트는 분단위 표시
        int currentMin = Mathf.FloorToInt(playedSeconds / 60f);
        sliderText.text = $"{currentMin}분 / {data.dailyReward.playTimeTodayMax}분";

        // 보상 지급 체크
        if (!data.dailyReward.rewardGivenToday &&
            playedSeconds >= slider.maxValue)
        {
            GiveReward();
        }

        // 5초마다 저장
        if (saveTimer >= 5f)
        {
            SaveManager.Save(data);
            saveTimer = 0f;
        }
    }

    private void CheckNewDay()
    {
        string today = System.DateTime.Now.ToString("yyyy-MM-dd");

        if (data.dailyReward.lastRewardDate != today)
        {
            data.dailyReward.lastRewardDate = today;
            data.dailyReward.playTimeToday = 0f;
            data.dailyReward.rewardGivenToday = false;

            SaveManager.Save(data);
        }
    }

    private void GiveReward()
    {
        PlaySFX();

        data.dailyReward.rewardGivenToday = true;
        data.dailyReward.rewardCheck++;

        Instantiate(gift, Vector3.zero, Quaternion.identity,
            GameObject.Find("Canvas2").transform);

        SaveManager.Save(data);
    }

    private void PlaySFX()
    {
        if (Setting.IsSFXOn())
            audioSource.Play();
    }

}