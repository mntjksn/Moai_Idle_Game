using System.Collections.Generic;
using UnityEngine;

// 캐릭터 도감(Book) 목록을 관리하는 매니저
// - 버튼(도감 카드)을 필요 개수만큼 생성 후 캐싱
// - Refresh 시 Destroy 없이 인덱스만 갱신하고 남는 항목은 비활성화
public class BookManager : MonoBehaviour
{
    public static BookManager Instance;

    // Book 컴포넌트가 붙은 버튼 프리팹
    public GameObject buttonPrefab;

    // ScrollView Content 같은 부모
    public Transform contentParent;

    // 생성한 Book 버튼 캐싱
    private List<Book> cachedBooks = new List<Book>();

    private void Awake()
    {
        // 싱글톤 유지(필요하면 DontDestroyOnLoad 추가 가능)
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        // 도감 화면이 켜질 때마다 최신 목록 반영
        Refresh();
    }

    // Book UI 리스트 갱신(캐싱 방식)
    public void Refresh()
    {
        // 캐릭터 리스트 가져오기
        var characterList = CharacterManager.Instance.characters;
        int count = characterList.Count;

        // 1) 캐시가 부족하면 추가 생성
        while (cachedBooks.Count < count)
        {
            GameObject obj = Instantiate(buttonPrefab, contentParent);

            // 프리팹에 Book 컴포넌트가 붙어있어야 함
            Book book = obj.GetComponent<Book>();
            if (book == null)
            {
                Debug.LogError("[BookManager] buttonPrefab에 Book 컴포넌트가 없습니다.");
                break;
            }

            cachedBooks.Add(book);
        }

        // 2) 필요한 개수만큼 활성화 + 인덱스 세팅
        for (int i = 0; i < count; i++)
        {
            cachedBooks[i].gameObject.SetActive(true);

            // Book 내부에서 index_book을 기준으로 해금 여부를 체크함
            cachedBooks[i].index_book = i;
        }

        // 3) 남는 항목은 비활성화(Destroy 금지)
        for (int i = count; i < cachedBooks.Count; i++)
        {
            cachedBooks[i].gameObject.SetActive(false);
        }
    }
}