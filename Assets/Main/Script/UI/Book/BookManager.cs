using System.Collections.Generic;
using UnityEngine;

public class BookManager : MonoBehaviour
{
    public static BookManager Instance;

    public GameObject buttonPrefab;       // Book 버튼 프리팹
    public Transform contentParent;       // Content 영역

    private List<Book> cachedBooks = new List<Book>();   // 생성된 버튼 캐싱

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
    /// Book UI 리스트 갱신 (캐싱 방식)
    /// </summary>
    public void Refresh()
    {
        var characterList = CharacterManager.Instance.characters;
        int count = characterList.Count;

        // 1) 부족하면 새로 생성
        while (cachedBooks.Count < count)
        {
            GameObject obj = Instantiate(buttonPrefab, contentParent);
            Book book = obj.GetComponent<Book>();

            if (book == null)
            {
                Debug.LogError("buttonPrefab에 Book 컴포넌트가 없습니다!");
                break;
            }

            cachedBooks.Add(book);
        }

        // 2) 데이터 갱신
        for (int i = 0; i < count; i++)
        {
            cachedBooks[i].gameObject.SetActive(true);
            cachedBooks[i].index_book = i;     // Book 내부에서 SpawnCheck 자동 반응
        }

        // 3) 남는 객체 비활성화(삭제 X)
        for (int i = count; i < cachedBooks.Count; i++)
        {
            cachedBooks[i].gameObject.SetActive(false);
        }
    }
}