using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

// 오프라인 보상 내용을 UI로 표시하는 팝업
public class OfflineRewardPopup : MonoBehaviour
{
    [Header("UI Refs")]
    public Slider timeSlider;              // 오프라인 경과 시간 표시 슬라이더
    public TextMeshProUGUI timeText;        // "10시간 20분 / 12시간" 텍스트

    public TextMeshProUGUI goldText;        // 골드 보상 텍스트
    public TextMeshProUGUI diaText;         // 다이아 보상 텍스트
    public TextMeshProUGUI ticketText;      // 티켓 보상 텍스트

    public Button confirmButton;            // 확인 버튼

    private void OnEnable()
    {
        // 팝업 활성화 시 데이터 갱신
        Refresh();

        // 버튼 리스너 중복 방지
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(OnClickConfirm);
    }

    /// <summary>
    /// 오프라인 보상 데이터를 UI에 반영
    /// </summary>
    public void Refresh()
    {
        var sys = OfflineRewardSystem.Instance;

        // 시스템이 없으면 팝업 비활성화
        if (sys == null)
        {
            gameObject.SetActive(false);
            return;
        }

        // 오프라인 보상 재계산
        sys.ComputePending();

        // 받을 보상이 없으면 팝업 숨김
        if (!sys.hasPendingReward)
        {
            gameObject.SetActive(false);
            return;
        }

        var r = sys.pending;

        // 최대 오프라인 시간(시간 → 분)
        int maxMinutes = sys.maxOfflineHours * 60;

        // 실제 반영된 경과 시간(초 → 분, 상한 적용)
        int elapsedMinutes = Mathf.Clamp(
            (int)Math.Floor(r.usedSeconds / 60.0),
            0,
            maxMinutes
        );

        // 슬라이더 세팅 (유저 조작 불가)
        timeSlider.minValue = 0;
        timeSlider.maxValue = maxMinutes;
        timeSlider.wholeNumbers = true;
        timeSlider.interactable = false;
        timeSlider.SetValueWithoutNotify(elapsedMinutes);

        // 시간 텍스트 ("10시간 0분 / 12시간")
        timeText.text = $"{ToHourMin(elapsedMinutes)} / {sys.maxOfflineHours}시간";

        // 보상 텍스트 표시
        goldText.text = $"+ {FormatNumber(r.goldReward)}개";
        diaText.text = $"+ {r.diaReward}개";
        ticketText.text = $"+ {r.ticketReward}개";
    }

    // 분 단위를 "시간 분" 문자열로 변환
    private string ToHourMin(int minutes)
    {
        int h = minutes / 60;
        int m = minutes % 60;
        return $"{h}시간 {m}분";
    }

    // 확인 버튼 클릭 시 보상 지급
    private void OnClickConfirm()
    {
        var sys = OfflineRewardSystem.Instance;
        if (sys != null)
            sys.ClaimPending();

        // 팝업 닫기
        gameObject.SetActive(false);
    }

    // 숫자 포맷 (1,000 단위 콤마)
    private string FormatNumber(long v) => v.ToString("N0");
}