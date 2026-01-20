using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

// 보상 획득 시 팝업 UI를 표시하는 매니저
// - 일정 시간 후 자동으로 닫힘
// - 싱글톤으로 관리
public class RewardPopup : MonoBehaviour
{
    // 전역 접근용 싱글톤 인스턴스
    public static RewardPopup Instance;

    [Header("UI")]
    [SerializeField] private GameObject panel;     // 팝업 패널 전체
    [SerializeField] private TextMeshProUGUI text; // 보상 메시지 텍스트
    [SerializeField] private Image icon;           // 보상 아이콘

    [Header("Icons")]
    public Sprite goldIcon;     // 골드 아이콘
    public Sprite diaIcon;      // 다이아 아이콘
    public Sprite ticketIcon;   // 티켓 아이콘

    // 자동 숨김 코루틴
    private Coroutine routine;

    private void Awake()
    {
        // 싱글톤 중복 생성 방지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 시작 시 패널 비활성화
        if (panel != null)
            panel.SetActive(false);
    }

    // 보상 팝업 표시
    // msg        : 표시할 텍스트
    // iconSprite : 보상 아이콘
    // duration   : 자동으로 닫히기까지의 시간
    public void ShowReward(string msg, Sprite iconSprite, float duration = 2.0f)
    {
        if (panel == null || text == null)
            return;

        // 텍스트 설정
        text.text = msg;

        // 아이콘 설정
        if (icon != null)
            icon.sprite = iconSprite;

        // 팝업 표시
        panel.SetActive(true);

        // 기존 자동 숨김 코루틴 중지
        if (routine != null)
            StopCoroutine(routine);

        // 새로운 자동 숨김 코루틴 시작
        routine = StartCoroutine(AutoHide(duration));
    }

    // 일정 시간 후 팝업을 숨김
    private IEnumerator AutoHide(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (panel != null)
            panel.SetActive(false);

        routine = null;
    }
}