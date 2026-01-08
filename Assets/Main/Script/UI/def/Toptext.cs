using System.Collections;
using TMPro;
using UnityEngine;
using static ShopButton;

public class Toptext : MonoBehaviour
{
    public enum StatType { Gold, TotalGold, Dia, Child, Ticket, Token }
    public StatType statType;

    private TextMeshProUGUI textScore;
    private Transform chpTransform;

    private GameData cachedData;
    private float refreshTimer = 0f;
    private float refreshInterval = 0.1f;

    private int lastIntValue = -999999999;
    private string lastStr = "";
    private int lastChildCount = -1;

    private Coroutine blinkCoroutine;

    private void Awake()
    {
        textScore = GetComponent<TextMeshProUGUI>();
        chpTransform = GameObject.Find("chp")?.transform;

        cachedData = SaveManager.Load();
    }

    private void OnEnable()
    {
        GameEvents.OnSettingsChanged += OnSettingsChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnSettingsChanged -= OnSettingsChanged;
    }

    private void Update()
    {
        refreshTimer += Time.deltaTime;

        if (refreshTimer >= refreshInterval)
        {
            refreshTimer = 0f;
            cachedData = SaveManager.Load();
            UpdateUI();
        }
    }

    private void OnSettingsChanged()
    {
        cachedData = SaveManager.Load();
        UpdateUI(true); // 강제로 최신 UI 갱신
    }

    private void UpdateUI(bool force = false)
    {
        switch (statType)
        {
            case StatType.Gold:
                int gold = cachedData.currency.gold;
                if (force || gold != lastIntValue)
                {
                    lastIntValue = gold;
                    textScore.text = $" : {gold:N0}";
                }

                // 깜빡이기 조건 추가
                if (gold >= 2147483600)
                {
                    if (blinkCoroutine == null)
                        blinkCoroutine = StartCoroutine(BlinkTMPText(textScore));
                }
                else
                {
                    if (blinkCoroutine != null)
                    {
                        StopCoroutine(blinkCoroutine);
                        blinkCoroutine = null;
                        textScore.color = Color.black; // 기본색 복원
                    }
                }
                break;

            case StatType.TotalGold:
                string tg = $"현재 골드 수급량 (+{cachedData.settings.ChgetGold:N0}개)";
                if (force || tg != lastStr)
                {
                    lastStr = tg;
                    textScore.text = tg;
                }
                break;

            case StatType.Dia:
                int dia = cachedData.currency.dia;
                if (force || dia != lastIntValue)
                {
                    lastIntValue = dia;
                    textScore.text = $" : {dia:N0}";
                }
                break;

            case StatType.Child:
                if (chpTransform != null)
                {
                    int activeCount = 0;
                    foreach (Transform child in chpTransform)
                        if (child.gameObject.activeSelf)
                            activeCount++;

                    if (force || activeCount != lastChildCount)
                    {
                        lastChildCount = activeCount;
                        textScore.text = $" : {activeCount} / {cachedData.settings.childMax}";
                    }
                }
                break;

            case StatType.Ticket:
                int t = cachedData.currency.ticket;
                if (force || t != lastIntValue)
                {
                    lastIntValue = t;
                    textScore.text = $" : {t:N0}";
                }
                break;

            case StatType.Token:
                int token = cachedData.currency.token;
                if (force || token != lastIntValue)
                {
                    lastIntValue = token;
                    textScore.text = $" : {token:N0}";
                }
                break;
        }
    }

    private IEnumerator BlinkTMPText(TextMeshProUGUI text)
    {
        Color originalColor = text.color;
        Color redColor = Color.red;

        while (true)
        {
            text.color = redColor;
            yield return new WaitForSeconds(0.5f);
            text.color = originalColor;
            yield return new WaitForSeconds(0.5f);
        }
    }
}