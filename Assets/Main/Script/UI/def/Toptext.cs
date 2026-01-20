using System.Collections;
using TMPro;
using UnityEngine;
using static ShopButton;

// 상단 UI 텍스트(재화/상태)를 표시하는 스크립트
// - StatType에 따라 표시 내용이 달라짐
// - 너무 자주 SaveManager.Load()를 호출하지 않도록 주기적으로 갱신
// - 값이 변했을 때만 텍스트를 바꿔서 불필요한 UI 갱신을 줄임
public class Toptext : MonoBehaviour
{
    // 표시할 항목 종류
    public enum StatType
    {
        Gold,       // 보유 골드
        TotalGold,  // 초당 수급량 표시(문자열)
        Dia,        // 보유 다이아
        Child,      // 배치된 캐릭터 수 / 최대치
        Ticket,     // 보유 티켓
        Token       // 보유 토큰
    }

    public StatType statType;

    // 표시 대상 텍스트
    private TextMeshProUGUI textScore;

    // 배치된 캐릭터(자식) 계산용 부모 트랜스폼
    private Transform chpTransform;

    // 세이브 데이터 캐싱(주기적으로 다시 로드)
    private GameData cachedData;

    // 갱신 주기(초)
    private float refreshTimer = 0f;
    private float refreshInterval = 0.1f;

    // 변경 감지용 캐시(값이 바뀔 때만 텍스트 갱신)
    private int lastIntValue = -999999999;
    private string lastStr = "";
    private int lastChildCount = -1;

    // 골드가 상한치일 때 깜빡이는 코루틴
    private Coroutine blinkCoroutine;

    // 깜빡이기 종료 시 복구할 기본 색
    private Color defaultColor;

    private void Awake()
    {
        // 텍스트 캐싱
        textScore = GetComponent<TextMeshProUGUI>();

        // 기본 색상 저장(나중에 깜빡이기 종료 시 복구)
        if (textScore != null)
            defaultColor = textScore.color;

        // 캐릭터 부모 트랜스폼 찾기
        chpTransform = GameObject.Find("chp")?.transform;

        // 최초 1회 로드
        cachedData = SaveManager.Load();
    }

    private void OnEnable()
    {
        // 설정/재화가 바뀌었을 때 즉시 갱신하기 위한 이벤트 구독
        GameEvents.OnSettingsChanged += OnSettingsChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnSettingsChanged -= OnSettingsChanged;

        // 오브젝트가 꺼질 때 깜빡이기 코루틴 정리(누적 방지)
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        // 색상 복구
        if (textScore != null)
            textScore.color = defaultColor;
    }

    private void Update()
    {
        // 너무 자주 갱신하지 않도록 타이머로 제어
        refreshTimer += Time.deltaTime;

        if (refreshTimer < refreshInterval)
            return;

        refreshTimer = 0f;

        // 주기적으로 최신 데이터 로드 후 UI 반영
        cachedData = SaveManager.Load();
        UpdateUI();
    }

    // 설정 변경 이벤트 발생 시 즉시 갱신
    private void OnSettingsChanged()
    {
        cachedData = SaveManager.Load();
        UpdateUI(true); // 강제 갱신
    }

    // StatType별 UI 갱신
    private void UpdateUI(bool force = false)
    {
        if (textScore == null || cachedData == null)
            return;

        switch (statType)
        {
            case StatType.Gold:
                {
                    int gold = cachedData.currency.gold;

                    // 값이 바뀌었을 때만 텍스트 갱신
                    if (force || gold != lastIntValue)
                    {
                        lastIntValue = gold;
                        textScore.text = " : " + gold.ToString("N0");
                    }

                    // 골드 상한 도달 시 깜빡이기 연출
                    if (gold >= 2147483600)
                    {
                        if (blinkCoroutine == null)
                            blinkCoroutine = StartCoroutine(BlinkTMPText(textScore));
                    }
                    else
                    {
                        // 상한에서 내려오면 깜빡이기 중단 + 색 복구
                        if (blinkCoroutine != null)
                        {
                            StopCoroutine(blinkCoroutine);
                            blinkCoroutine = null;
                            textScore.color = defaultColor;
                        }
                    }
                    break;
                }

            case StatType.TotalGold:
                {
                    // 초당 수급량 안내 문자열
                    string tg = "현재 골드 수급량 (+" + cachedData.settings.ChgetGold.ToString("N0") + "개)";

                    if (force || tg != lastStr)
                    {
                        lastStr = tg;
                        textScore.text = tg;
                    }
                    break;
                }

            case StatType.Dia:
                {
                    int dia = cachedData.currency.dia;

                    if (force || dia != lastIntValue)
                    {
                        lastIntValue = dia;
                        textScore.text = " : " + dia.ToString("N0");
                    }
                    break;
                }

            case StatType.Child:
                {
                    if (chpTransform == null)
                        break;

                    // 실제 활성화된 자식 수만 계산
                    int activeCount = 0;
                    foreach (Transform child in chpTransform)
                    {
                        if (child.gameObject.activeSelf)
                            activeCount++;
                    }

                    if (force || activeCount != lastChildCount)
                    {
                        lastChildCount = activeCount;
                        textScore.text = " : " + activeCount + " / " + cachedData.settings.childMax;
                    }
                    break;
                }

            case StatType.Ticket:
                {
                    int t = cachedData.currency.ticket;

                    if (force || t != lastIntValue)
                    {
                        lastIntValue = t;
                        textScore.text = " : " + t.ToString("N0");
                    }
                    break;
                }

            case StatType.Token:
                {
                    int token = cachedData.currency.token;

                    if (force || token != lastIntValue)
                    {
                        lastIntValue = token;
                        textScore.text = " : " + token.ToString("N0");
                    }
                    break;
                }
        }
    }

    // 골드 상한 도달 시 텍스트 깜빡이기
    private IEnumerator BlinkTMPText(TextMeshProUGUI text)
    {
        // 시작 시점의 색을 원본으로 사용
        Color originalColor = text.color;

        while (true)
        {
            text.color = Color.red;
            yield return new WaitForSeconds(0.5f);

            text.color = originalColor;
            yield return new WaitForSeconds(0.5f);
        }
    }
}