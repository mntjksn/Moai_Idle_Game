using UnityEngine;
using System.Collections.Generic;

public class Book2Manager : MonoBehaviour
{
    public static Book2Manager Instance;

    [Header("Prefabs & Parents")]
    public GameObject buttonPrefab;
    public Transform contentParent;

    private List<Book2> cachedCards = new List<Book2>(); // 생성한 카드 캐싱

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        Refresh();
    }

    /// <summary>
    /// 리스트를 재생성하지 않고, 캐시된 버튼들에 데이터만 갱신
    /// </summary>
    public void Refresh()
    {
        var list = BackgroundManager.Instance.backgrounds;

        // 1) 기존보다 적으면 새로 생성
        while (cachedCards.Count < list.Count)
        {
            GameObject go = Instantiate(buttonPrefab, contentParent);
            Book2 card = go.GetComponent<Book2>();
            cachedCards.Add(card);
        }

        // 2) 데이터 갱신
        for (int i = 0; i < list.Count; i++)
        {
            cachedCards[i].gameObject.SetActive(true);
            cachedCards[i].Setup(list[i], i);
        }

        // 3) 남는 항목 비활성화 (Destroy 금지)
        for (int i = list.Count; i < cachedCards.Count; i++)
        {
            cachedCards[i].gameObject.SetActive(false);
        }
    }
}