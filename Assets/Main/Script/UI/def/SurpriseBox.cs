using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SurpriseBox : MonoBehaviour
{
    public static SurpriseBox Instance;

    [Header("UI")]
    [SerializeField] private GameObject boxObject;
    [SerializeField] private Button boxButton;

    [Header("Time Settings")]
    public float minSpawnTime = 180f;
    public float maxSpawnTime = 600f;
    public float aliveTime = 30f;

    private float timer = 0f;
    private float nextSpawnTime = 0f;

    [SerializeField] private Image boxImage;
    private Color targetColor;
    private float colorChangeSpeed = 3f;

    [SerializeField] private TextMeshProUGUI appearText;
    [SerializeField] private float textFadeDuration = 1.2f;

    [SerializeField] private AudioSource audioSource;

    private RectTransform boxRect;        // ★ 캐싱
    private RectTransform parentRect;     // ★ 캐싱
    private Camera cam;

    void Awake()
    {
        Instance = this;

        boxObject.SetActive(false);

        boxButton.onClick.RemoveAllListeners();
        boxButton.onClick.AddListener(OnClick);

        boxImage = boxObject.GetComponent<Image>();
        targetColor = GetRandomColor();

        boxRect = boxObject.GetComponent<RectTransform>(); // ★ 한번만 캐싱
        parentRect = boxObject.transform.parent.GetComponent<RectTransform>();
        cam = Camera.main;
    }

    void Start()
    {
        SetNextSpawn();
    }

    void Update()
    {
        timer += Time.deltaTime;

        // 색 변화: active일 때만 실행되도록 최적화
        if (boxObject.activeSelf)
        {
            boxImage.color = Color.Lerp(boxImage.color, targetColor, Time.deltaTime * colorChangeSpeed);

            if (((Vector4)boxImage.color - (Vector4)targetColor).sqrMagnitude < 0.0025f)
            {
                targetColor = GetRandomColor();
            }
        }

        // 등장 타이밍 도달
        if (!boxObject.activeSelf && timer >= nextSpawnTime)
            ShowBox();
    }

    Color GetRandomColor()
    {
        float h = Random.Range(0f, 1f);      // 아무 색상
        float s = Random.Range(0.25f, 0.4f); // 낮은 채도 (파스텔 핵심)
        float v = Random.Range(0.85f, 1f);     // 높은 명도

        Color color = Color.HSVToRGB(h, s, v);
        color.a = 1f;
        return color;
    }

    void ShowBox()
    {
        if (Setting.IsSFXOn())
            audioSource.Play();

        CancelInvoke(nameof(HideBox));

        SetRandomPosition();
        boxObject.SetActive(true);
        timer = 0f;

        // ▶ 등장 텍스트 표시 + 페이드 아웃 시작
        if (appearText != null)
        {
            appearText.gameObject.SetActive(true);
            appearText.color = new Color(appearText.color.r, appearText.color.g, appearText.color.b, 1f); // 완전 보이게
            StartCoroutine(FadeOutAppearText());
        }

        Invoke(nameof(HideBox), aliveTime);
    }

    private IEnumerator FadeOutAppearText()
    {
        float t = 0f;
        Color startColor = appearText.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (t < textFadeDuration)
        {
            t += Time.deltaTime;
            appearText.color = Color.Lerp(startColor, endColor, t / textFadeDuration);
            yield return null;
        }

        appearText.gameObject.SetActive(false);
    }

    void SetRandomPosition()
    {
        float xMin = 0.075f;
        float xMax = 0.925f;
        float yMin = 0.195f;
        float yMax = 0.75f;

        float randX = Random.Range(xMin, xMax);
        float randY = Random.Range(yMin, yMax);

        Vector3 worldPos = cam.ViewportToWorldPoint(new Vector3(randX, randY, 0f));

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            cam.WorldToScreenPoint(worldPos),
            cam,
            out Vector2 uiPos
        );

        boxRect.anchoredPosition = uiPos;   // ★ GetComponent 제거 → 캐싱 사용
    }

    void HideBox()
    {
        CancelInvoke(nameof(HideBox));

        if (boxObject.activeSelf)
            boxObject.SetActive(false);

        SetNextSpawn();
    }

    void SetNextSpawn()
    {
        timer = 0f;
        nextSpawnTime = Random.Range(minSpawnTime, maxSpawnTime);
    }

    void OnClick()
    {
        GiveReward();
    }

    void GiveReward()
    {
        GameData data = SaveManager.Load();

        int r = Random.Range(0, 100);

        string msg = "";
        Sprite icon = null;

        if (r < 50)
        {
            int reward = Random.Range(100, 10001);
            data.currency.gold += reward;

            msg = $"+ {reward:N0}개";
            icon = RewardPopup.Instance.goldIcon;
        }
        else if (r < 80)
        {
            int reward = Random.Range(10, 101);
            data.currency.dia += reward;

            msg = $"+ {reward:N0}개";
            icon = RewardPopup.Instance.diaIcon;
        }
        else
        {
            int reward = Random.Range(1, 6);
            data.currency.ticket += reward;

            msg = $"+ {reward:N0}개";
            icon = RewardPopup.Instance.ticketIcon;
        }

        data.missions.mission_7_value++;

        if (data.background.box_check <= 1000)
            data.background.box_check++;

        if (data.currency.gold > 2147483600)
            data.currency.gold = 2147483600;

        SaveManager.Save(data);

        RewardPopup.Instance.ShowReward(msg, icon);

        HideBox();
    }
}