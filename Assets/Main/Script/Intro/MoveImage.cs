using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MoveImage : MonoBehaviour
{
    public float moveDistance = 100f;
    public float duration = 1f;
    public float waitTime = 0.5f;

    private RectTransform rectTransform;
    private Image image;

    private Vector2 originalPos;
    private Vector2 endPos;

    private Color startColor;
    private Color endColor;

    private bool isRunning = true;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        originalPos = rectTransform.anchoredPosition;
        endPos = originalPos + new Vector2(moveDistance, 0);

        startColor = image.color;
        endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        StartCoroutine(FadeMoveLoop());
    }

    private IEnumerator FadeMoveLoop()
    {
        float t;

        while (isRunning)
        {
            // 이동 + Fade Out
            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                float lerp = Mathf.Clamp01(t);

                rectTransform.anchoredPosition = Vector2.Lerp(originalPos, endPos, lerp);
                image.color = Color.Lerp(startColor, endColor, lerp);

                yield return null;
            }

            yield return new WaitForSeconds(waitTime);

            // 위치 리셋 + Fade In
            rectTransform.anchoredPosition = originalPos;
            t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                float lerp = Mathf.Clamp01(t);

                image.color = Color.Lerp(endColor, startColor, lerp);

                yield return null;
            }

            yield return new WaitForSeconds(waitTime);
        }
    }

    public void StopEffect()
    {
        isRunning = false;
    }
}