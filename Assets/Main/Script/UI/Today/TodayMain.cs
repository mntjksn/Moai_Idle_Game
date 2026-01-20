using UnityEngine;
using TMPro;
using UnityEngine.UI;

// "오늘 보상" 결과 화면 컨트롤러
// - GenerateGift()로 보상 종류/수량 결정
// - DoubleGift()로 확률에 따라 2배
// - ClosePanel()에서 실제 재화 반영 후 패널 파괴
public class TodayMain : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    [Header("Buttons")]
    public Button buttonNext;
    public Button buttonReset;
    public Button buttonDouble;

    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject resultPanel;

    [Header("UI")]
    public Image image;
    public TextMeshProUGUI mainText;
    public TextMeshProUGUI resetText;
    public TextMeshProUGUI doubleText;

    // 현재 확정된 보상 수량
    private int gift;

    // 재굴림(리셋) 가능 횟수
    private int resetPoint = 3;

    // 더블 확률(%)
    private int doublePoint = 40;

    // 더블 남은 횟수
    private int doubleLeft = 2;

    // "gold" / "dia" / "ticket"
    private string giftType;

    // 리소스 스프라이트 캐싱
    private Sprite goldIcon;
    private Sprite diaIcon;
    private Sprite ticketIcon;

    private bool closing = false;

    private void Awake()
    {
        // 아이콘 리소스 캐싱 (Resources.Load는 비용이 있으니 1회만)
        goldIcon = Resources.Load<Sprite>("gold");
        diaIcon = Resources.Load<Sprite>("dia");
        ticketIcon = Resources.Load<Sprite>("ticket");

        // 초기 UI 반영
        UpdateUI();
    }

    // UI 텍스트/버튼 상태 갱신
    private void UpdateUI()
    {
        if (mainText != null)
            mainText.text = $"+{gift:N0}개";

        // resetPoint는 "남은 재굴림 횟수"로 보임
        if (resetText != null)
            resetText.text = $"({resetPoint}/2)";

        if (buttonReset != null)
            buttonReset.interactable = resetPoint > 0;

        // 더블: 남은 횟수 / 확률
        if (doubleText != null)
            doubleText.text = $"({doubleLeft}/2)  ({doublePoint}%)";

        if (buttonDouble != null)
            buttonDouble.interactable = (doubleLeft > 0 && doublePoint > 0);
    }

    // ==============================
    //      결과 화면 열기
    // ==============================
    public void OpenResult()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(true);
    }

    // ==============================
    //      보상 생성(랜덤)
    // ==============================
    public void GenerateGift()
    {
        PlaySFX();

        int normal = Random.Range(0, 100);

        // 0~49: 골드(50%)
        if (normal < 50)
        {
            giftType = "gold";
            if (image != null) image.sprite = goldIcon;

            gift = Random.Range(1, 100001);
        }
        // 50~79: 다이아(30%)
        else if (normal < 80)
        {
            giftType = "dia";
            if (image != null) image.sprite = diaIcon;

            gift = Random.Range(1, 1001);
        }
        // 80~99: 티켓(20%)
        else
        {
            giftType = "ticket";
            if (image != null) image.sprite = ticketIcon;

            gift = Random.Range(1, 11);
        }

        // 재굴림 포인트 감소 (0 아래로 내려갈 수 있음 → 최소 0으로 방어할 수도 있음)
        resetPoint--;

        UpdateUI();
    }

    // ==============================
    //      더블 찬스
    // ==============================
    public void DoubleGift()
    {
        PlaySFX();

        if (doubleLeft <= 0) return;

        int r = Random.Range(0, 100);

        // 확률 성공 시 보상 2배 + 다음 확률 감소
        if (r < doublePoint)
        {
            gift *= 2;
            doublePoint -= 20; // 다음 확률 다운
        }

        doubleLeft--;

        UpdateUI();
    }

    // ==============================
    //      닫기 버튼(실제 지급)
    // ==============================
    public void ClosePanel()
    {
        if (closing) return;
        closing = true;

        PlaySFX();

        GameData data = SaveManager.Load();

        // 보상 지급
        switch (giftType)
        {
            case "gold":
                data.currency.gold += gift;
                break;

            case "dia":
                data.currency.dia += gift;
                break;

            case "ticket":
                data.currency.ticket += gift;
                break;

            default:
                // giftType이 비어있으면(GenerateGift 안하고 닫은 경우)
                // 아무것도 지급하지 않음
                break;
        }

        // 골드 상한 처리
        if (data.currency.gold > 2147483600)
            data.currency.gold = 2147483600;

        SaveManager.Save(data);

        // 효과음 끝나고 오브젝트 제거(있으면)
        float delay = 0.15f;
        if (audioSource != null && audioSource.clip != null)
            delay = audioSource.clip.length;

        Destroy(gameObject, delay);
    }

    // 효과음 재생
    private void PlaySFX()
    {
        if (audioSource == null) return;

        if (Setting.IsSFXOn())
            audioSource.Play();
    }
}