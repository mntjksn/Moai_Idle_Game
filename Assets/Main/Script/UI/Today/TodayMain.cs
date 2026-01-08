using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TodayMain : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    public Button buttonNext, buttonReset, buttonDouble;
    public GameObject mainPanel, resultPanel;

    public Image image;
    public TextMeshProUGUI mainText, resetText, doubleText;

    private int gift;
    private int resetPoint = 3;
    private int doublePoint = 40;
    private int doubleLeft = 2;

    private string giftType;

    private Sprite goldIcon;
    private Sprite diaIcon;
    private Sprite ticketIcon;

    private void Awake()
    {
        // 미리 캐싱 → 성능 UP
        goldIcon = Resources.Load<Sprite>("gold");
        diaIcon = Resources.Load<Sprite>("dia");
        ticketIcon = Resources.Load<Sprite>("ticket");

        UpdateUI();
    }

    private void UpdateUI()
    {
        mainText.text = $"+{gift:N0}개";

        resetText.text = $"({resetPoint}/2)";
        buttonReset.interactable = resetPoint > 0;

        doubleText.text = $"({doubleLeft}/2)  ({doublePoint}%)";
        buttonDouble.interactable = (doubleLeft > 0 && doublePoint > 0);
    }

    //==============================
    //      보상 화면 열기
    //==============================
    public void OpenResult()
    {
        mainPanel.SetActive(false);
        resultPanel.SetActive(true);
    }

    //==============================
    //      보상 생성
    //==============================
    public void GenerateGift()
    {
        PlaySFX();

        GameData data = SaveManager.Load();
        SaveManager.Save(data);

        int normal = Random.Range(0, 100);

        if (normal < 50)
        {
            giftType = "gold";
            image.sprite = goldIcon;

            gift = Random.Range(1, 100001);
        }

        else if (normal < 80)
        {
            giftType = "dia";
            image.sprite = diaIcon;

            gift = Random.Range(1, 1001);
        }

        else
        {
            giftType = "ticket";
            image.sprite = ticketIcon;

            gift = Random.Range(1, 11);
        }

        resetPoint--;
        UpdateUI();
    }

    //==============================
    //      더블 찬스
    //==============================
    public void DoubleGift()
    {
        PlaySFX();

        if (doubleLeft <= 0) return;

        int r = Random.Range(0, 100);

        if (r < doublePoint)
        {
            gift *= 2;
            doublePoint -= 20;  // 다음 확률 다운
        }

        doubleLeft--;

        UpdateUI();
    }

    //==============================
    //      닫기 버튼
    //==============================
    public void ClosePanel()
    {
        GameData data = SaveManager.Load();

        switch (giftType)
        {
            case "gold": data.currency.gold += gift; break;
            case "dia": data.currency.dia += gift; break;
            case "ticket": data.currency.ticket += gift; break;
        }

        if (data.currency.gold > 2147483600)
            data.currency.gold = 2147483600;

        SaveManager.Save(data);
        Destroy(gameObject);
    }

    private void PlaySFX()
    {
        if (Setting.IsSFXOn())
            audioSource.Play();
    }
}