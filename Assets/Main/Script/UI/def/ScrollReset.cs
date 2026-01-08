using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScrollReset : MonoBehaviour
{
    private ScrollRect scrollRect;

    private void OnEnable()
    {
        scrollRect = GetComponent<ScrollRect>();
        StartCoroutine(ResetNextFrame());
    }

    private IEnumerator ResetNextFrame()
    {
        yield return null;
        scrollRect.verticalNormalizedPosition = 1f;
    }
}
