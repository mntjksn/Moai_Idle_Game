using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStart : MonoBehaviour, IDragHandler
{
    public Image FadePanel;
    public string nextSceneName = "Main";

    private bool isTransitioning = false;
    private Camera mainCam;
    private float fadeDuration = 0.5f;

    private void Start()
    {
        // Camera.main 캐싱 → 반복 호출 방지
        mainCam = Camera.main;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isTransitioning) return; // 중복 실행 방지
        if (!collision.CompareTag("Start")) return;

        StartCoroutine(FadeAndLoad());
    }

    private IEnumerator FadeAndLoad()
    {
        isTransitioning = true;

        if (FadePanel != null)
        {
            FadePanel.gameObject.SetActive(true);
            Color c = FadePanel.color;
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime / fadeDuration;
                c.a = Mathf.Clamp01(t);
                FadePanel.color = c;
                yield return null;
            }
        }

        yield return new WaitForSeconds(0.05f);
        SceneManager.LoadScene(nextSceneName);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isTransitioning) return; // 씬 전환 중 드래그 차단

        if (mainCam == null) mainCam = Camera.main;

        Vector3 screenPos = eventData.position;
        screenPos.z = 10f;

        transform.position = mainCam.ScreenToWorldPoint(screenPos);
    }
}