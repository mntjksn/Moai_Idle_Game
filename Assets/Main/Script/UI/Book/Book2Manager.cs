using UnityEngine;
using System.Collections.Generic;

// 배경 도감(Book2) 목록을 관리하는 매니저
// - 버튼(카드)을 필요 개수만큼 생성 후 캐싱
// - Refresh 시 Destroy 없이 데이터만 갱신하고 남는 항목은 비활성화
public class Book2Manager : MonoBehaviour
{
    public static Book2Manager Instance;

    [Header("Prefabs & Parents")]
    public GameObject buttonPrefab;   // Book2가 붙은 버튼 프리팹
    public Transform contentParent;   // ScrollView Content 같은 부모

    // 생성한 카드(Book2) 캐싱
    private List<Book2> cachedCards = new List<Book2>();

    private void Awake()
    {
        // 싱글톤 유지(필요 시 DontDestroyOnLoad 추가 가능)
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        // 화면이 켜질 때마다 데이터 최신화
        Refresh();
    }

    // 리스트를 재생성하지 않고, 캐시된 버튼들에 데이터만 갱신
    public void Refresh()
    {
        // 배경 리스트 가져오기
        var list = BackgroundManager.Instance.backgrounds;

        // 1) 현재 리스트 개수보다 캐시가 부족하면 추가 생성
        while (cachedCards.Count < list.Count)
        {
            GameObject go = Instantiate(buttonPrefab, contentParent);

            // 프리팹에 Book2가 붙어있다고 가정
            Book2 card = go.GetComponent<Book2>();

            // 캐시에 추가
            cachedCards.Add(card);
        }

        // 2) 필요한 개수만큼 활성화 + 데이터 주입
        for (int i = 0; i < list.Count; i++)
        {
            cachedCards[i].gameObject.SetActive(true);
            cachedCards[i].Setup(list[i], i);
        }

        // 3) 남는 항목은 비활성화 (Destroy 하지 않음)
        for (int i = list.Count; i < cachedCards.Count; i++)
        {
            cachedCards[i].gameObject.SetActive(false);
        }
    }
}