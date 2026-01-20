using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

// 일정 시간마다 등장하는 깜짝 상자 시스템
// - 랜덤 시간 후 등장
// - 일정 시간 유지(aliveTime) 후 자동 사라짐
// - 클릭 시 랜덤 보상 지급 후 팝업 표시
public class SurpriseBox : MonoBehaviour
{
    public static SurpriseBox Instance;

    [Header("UI")]
    [SerializeField] private GameObject boxObject;   // 상자 UI 오브젝트
    [SerializeField] private Button boxButton;       // 상자 클릭 버튼

    [Header("Time Settings")]
    public float minSpawnTime = 180f; // 최소 등장 시간(초)
    public float maxSpawnTime = 600f; // 최대 등장 시간(초)
    public float aliveTime = 30f;     // 등장 후 유지 시간(초)

    // 타이머(현재 경과 시간) / 다음 등장 시간(목표)
    private float timer = 0f;
    private float nextSpawnTime = 0f;

    [Header("Visual")]
    [SerializeField] private Image boxImage;          // 상자 이미지(색 변환용)
    private Color targetColor;                        // 목표 색
    private float colorChangeSpeed = 3f;              // 색 변화 속도

    [Header("Appear Text")]
    [SerializeField] private TextMeshProUGUI appearText; // 등장 안내 텍스트
    [SerializeField] private float textFadeDuration = 1.2f;

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource; // 등장 효과음

    // 좌표 계산용 캐싱
    private RectTransform boxRect;
    private RectTransform parentRect;
    private Camera cam;

    private void Awake()
    {
        // 싱글톤 중복 생성 방지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 시작 시 숨김
        if (boxObject != null)
            boxObject.SetActive(false);

        // 버튼 이벤트 연결(중복 방지)
        if (boxButton != null)
        {
            boxButton.onClick.RemoveAllListeners();
            boxButton.onClick.AddListener(OnClick);
        }

        // 이미지 참조 캐싱
        if (boxObject != null && boxImage == null)
            boxImage = boxObject.GetComponent<Image>();

        // 색상 초기 목표 설정
        targetColor = GetRandomColor();

        // RectTransform 캐싱(반복 GetComponent 방지)
        if (boxObject != null)
            boxRect = boxObject.GetComponent<RectTransform>();

        if (boxObject != null && boxObject.transform.parent != null)
            parentRect = boxObject.transform.parent.GetComponent<RectTransform>();

        // 카메라 캐싱
        cam = Camera.main;
    }

    private void Start()
    {
        // 다음 등장 시간 설정
        SetNextSpawn();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // 상자가 활성화된 동안만 색 변환 연출
        if (boxObject != null && boxObject.activeSelf && boxImage != null)
        {
            boxImage.color = Color.Lerp(boxImage.color, targetColor, Time.deltaTime * colorChangeSpeed);

            // 목표 색에 충분히 가까워지면 다음 목표 색 갱신
            if (((Vector4)boxImage.color - (Vector4)targetColor).sqrMagnitude < 0.0025f)
                targetColor = GetRandomColor();
        }

        // 아직 등장하지 않았고, 목표 시간 도달 시 등장
        if (boxObject != null && !boxObject.activeSelf && timer >= nextSpawnTime)
            ShowBox();
    }

    // 파스텔 느낌 랜덤 색 생성
    private Color GetRandomColor()
    {
        float h = Random.Range(0f, 1f);        // 색상(Hue)
        float s = Random.Range(0.25f, 0.4f);   // 채도(Saturation) 낮게
        float v = Random.Range(0.85f, 1f);     // 명도(Value) 높게

        Color color = Color.HSVToRGB(h, s, v);
        color.a = 1f;
        return color;
    }

    // 상자 등장 처리
    private void ShowBox()
    {
        // 등장 효과음(설정 ON일 때만)
        if (audioSource != null && Setting.IsSFXOn())
            audioSource.Play();

        // 기존 HideBox 예약 제거(중복 방지)
        CancelInvoke(nameof(HideBox));

        // 랜덤 위치 배치 후 활성화
        SetRandomPosition();

        if (boxObject != null)
            boxObject.SetActive(true);

        // 상자가 떠 있는 동안 타이머는 다시 사용(생존 시간/표시 타이밍 관리용)
        timer = 0f;

        // 등장 안내 텍스트 표시 + 페이드 아웃
        if (appearText != null)
        {
            appearText.gameObject.SetActive(true);
            appearText.color = new Color(appearText.color.r, appearText.color.g, appearText.color.b, 1f);

            StartCoroutine(FadeOutAppearText());
        }

        // aliveTime 후 자동 숨김 예약
        Invoke(nameof(HideBox), aliveTime);
    }

    // 등장 텍스트 페이드 아웃 처리
    private IEnumerator FadeOutAppearText()
    {
        if (appearText == null)
            yield break;

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

    // 화면 안에서 랜덤 위치에 상자 배치(뷰포트 기준)
    private void SetRandomPosition()
    {
        // 화면 안전 영역(네 move.cs에서 쓰던 범위와 유사)
        float xMin = 0.075f;
        float xMax = 0.925f;
        float yMin = 0.195f;
        float yMax = 0.75f;

        float randX = Random.Range(xMin, xMax);
        float randY = Random.Range(yMin, yMax);

        if (cam == null || boxRect == null || parentRect == null)
            return;

        // 뷰포트 좌표 -> 월드 좌표 -> 스크린 좌표 -> UI 로컬 좌표 변환
        Vector3 worldPos = cam.ViewportToWorldPoint(new Vector3(randX, randY, 0f));

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            cam.WorldToScreenPoint(worldPos),
            cam,
            out Vector2 uiPos
        );

        // 최종 UI 위치 적용
        boxRect.anchoredPosition = uiPos;
    }

    // 상자 숨김 처리
    private void HideBox()
    {
        // 중복 예약 제거
        CancelInvoke(nameof(HideBox));

        if (boxObject != null && boxObject.activeSelf)
            boxObject.SetActive(false);

        // 다음 등장 시간 다시 설정
        SetNextSpawn();
    }

    // 다음 등장 시간을 랜덤으로 설정
    private void SetNextSpawn()
    {
        timer = 0f;
        nextSpawnTime = Random.Range(minSpawnTime, maxSpawnTime);
    }

    // 상자 클릭 처리
    private void OnClick()
    {
        GiveReward();
    }

    // 보상 지급 처리
    private void GiveReward()
    {
        GameData data = SaveManager.Load();

        // 0~99
        int r = Random.Range(0, 100);

        string msg = "";
        Sprite iconSprite = null;

        // 보상 분기
        // - 50%: 골드
        // - 30%: 다이아
        // - 20%: 티켓
        if (r < 50)
        {
            int reward = Random.Range(100, 10001);
            data.currency.gold += reward;

            msg = "+ " + reward.ToString("N0") + "개";
            if (RewardPopup.Instance != null) iconSprite = RewardPopup.Instance.goldIcon;
        }
        else if (r < 80)
        {
            int reward = Random.Range(10, 101);
            data.currency.dia += reward;

            msg = "+ " + reward.ToString("N0") + "개";
            if (RewardPopup.Instance != null) iconSprite = RewardPopup.Instance.diaIcon;
        }
        else
        {
            int reward = Random.Range(1, 6);
            data.currency.ticket += reward;

            msg = "+ " + reward.ToString("N0") + "개";
            if (RewardPopup.Instance != null) iconSprite = RewardPopup.Instance.ticketIcon;
        }

        // 미션 카운트 증가
        data.missions.mission_7_value++;

        // 배경 해금 조건용 카운트 증가(상한 유지)
        if (data.background.box_check <= 1000)
            data.background.box_check++;

        // 골드 상한(인트 오버플로 방지)
        if (data.currency.gold > 2147483600)
            data.currency.gold = 2147483600;

        SaveManager.Save(data);

        // 보상 팝업 표시
        if (RewardPopup.Instance != null)
            RewardPopup.Instance.ShowReward(msg, iconSprite);

        // 클릭 후 상자 숨김
        HideBox();
    }
}