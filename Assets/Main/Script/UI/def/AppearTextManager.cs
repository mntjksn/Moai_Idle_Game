using System.Collections;
using TMPro;
using UnityEngine;

public class AppearTextManager : MonoBehaviour
{
    public static AppearTextManager Instance;

    [SerializeField] private TextMeshProUGUI appearText;
    [SerializeField] private float fadeTime = 0.5f;

    private Coroutine runCoroutine;

    private void Awake()
    {
        Instance = this;

        if (appearText == null)
            appearText = GameObject.Find("appearText")?.GetComponent<TextMeshProUGUI>();

        if (appearText == null)
        {
            Debug.LogError("[AppearTextManager] appearText assigned 없음!");
            return;
        }

        ResetText();
        appearText.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        // 패널 꺼질 때 텍스트 원상복구
        ResetText();

        if (runCoroutine != null)
            StopCoroutine(runCoroutine);
    }

    // ★ 항상 알파 1로 되돌리는 함수
    private void ResetText()
    {
        if (appearText == null) return;

        Color c = appearText.color;
        c.a = 1f;
        appearText.color = c;
    }

    public void Show(string msg)
    {
        if (appearText == null) return;

        // 텍스트 리셋 (반투명 방지)
        ResetText();
        appearText.text = msg;

        if (runCoroutine != null)
            StopCoroutine(runCoroutine);

        appearText.gameObject.SetActive(true);
        runCoroutine = StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        Color origin = appearText.color;
        Color fade = new Color(origin.r, origin.g, origin.b, 0);

        for (int i = 0; i < 2; i++)
        {
            float t = 0;
            while (t < fadeTime)
            {
                t += Time.deltaTime;
                appearText.color = Color.Lerp(origin, fade, t / fadeTime);
                yield return null;
            }

            yield return new WaitForSeconds(0.1f);
            appearText.color = origin;
        }

        // ★ 코루틴 끝날 때 알파 완전 복구
        ResetText();
        appearText.gameObject.SetActive(false);

        runCoroutine = null;
    }
}