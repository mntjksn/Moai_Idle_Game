using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class OfflineRewardPopup : MonoBehaviour
{
    [Header("UI Refs")]
    public Slider timeSlider;
    public TextMeshProUGUI timeText;

    public TextMeshProUGUI goldText;
    public TextMeshProUGUI diaText;
    public TextMeshProUGUI ticketText;

    public Button confirmButton;

    private void OnEnable()
    {
        Refresh();

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(OnClickConfirm);
    }

    public void Refresh()
    {
        var sys = OfflineRewardSystem.Instance;
        if (sys == null)
        {
            gameObject.SetActive(false);
            return;
        }

        sys.ComputePending();

        if (!sys.hasPendingReward)
        {
            gameObject.SetActive(false);
            return;
        }

        var r = sys.pending;

        // 최대(12시간) = 분 단위
        int maxMinutes = sys.maxOfflineHours * 60;

        // 경과(캡 적용 후 usedSeconds)
        int elapsedMinutes = Mathf.Clamp((int)Math.Floor(r.usedSeconds / 60.0), 0, maxMinutes);

        // 슬라이더: 0 ~ 720(12시간) 중 600(10시간) 이런 느낌
        timeSlider.minValue = 0;
        timeSlider.maxValue = maxMinutes;
        timeSlider.wholeNumbers = true;
        timeSlider.interactable = false; // 유저 드래그 방지
        timeSlider.SetValueWithoutNotify(elapsedMinutes);

        // 텍스트: "10시간 0분 / 12시간"
        timeText.text = $"{ToHourMin(elapsedMinutes)} / {sys.maxOfflineHours}시간";

        // 보상 텍스트
        goldText.text = $"+ {FormatNumber(r.goldReward)}개";
        diaText.text = $"+ {r.diaReward}개";
        ticketText.text = $"+ {r.ticketReward}개";
    }

    private string ToHourMin(int minutes)
    {
        int h = minutes / 60;
        int m = minutes % 60;
        return $"{h}시간 {m}분";
    }

    private void OnClickConfirm()
    {
        var sys = OfflineRewardSystem.Instance;
        if (sys != null)
            sys.ClaimPending();

        gameObject.SetActive(false);
    }

    private string FormatNumber(long v) => v.ToString("N0");
}