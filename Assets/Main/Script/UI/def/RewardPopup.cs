using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class RewardPopup : MonoBehaviour
{
    public static RewardPopup Instance;

    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image icon;

    public Sprite goldIcon;
    public Sprite diaIcon;
    public Sprite ticketIcon;

    private Coroutine routine;

    private void Awake()
    {
        //  싱글톤 중복 제거
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 씬 이동 시 유지하고 싶으면 추가:
        // DontDestroyOnLoad(gameObject);

        panel.SetActive(false);
    }

    public void ShowReward(string msg, Sprite iconSprite, float duration = 2.0f)
    {
        // UI 갱신
        text.text = msg;

        if (icon != null)
            icon.sprite = iconSprite;

        panel.SetActive(true);

        // 기존 코루틴 정지
        if (routine != null)
            StopCoroutine(routine);

        // 새로운 숨김 타이머 시작
        routine = StartCoroutine(AutoHide(duration));
    }

    private IEnumerator AutoHide(float delay)
    {
        yield return new WaitForSeconds(delay);
        panel.SetActive(false);
    }
}