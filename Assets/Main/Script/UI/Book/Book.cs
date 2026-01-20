using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 도감(Book) UI 한 칸을 담당
// - 캐릭터 해금 여부에 따라 이미지/이름 표시
// - 클릭 시 상세 도감 패널 오픈
public class Book : MonoBehaviour
{
    // 도감 인덱스 (캐릭터 인덱스와 동일하게 사용)
    public int index_book;

    // 도감에 표시될 이름 텍스트
    public TextMeshProUGUI book_name;

    // 캐릭터 이미지 오브젝트
    public GameObject image;

    // 도감 상세 패널 프리팹
    public GameObject Book_prefab;

    private Button btn;
    private bool isUnlocked = false;   // 해금 여부 캐싱
    private Image imgComp;
    private Transform canvas2;

    private void Awake()
    {
        // 버튼 컴포넌트 캐싱 및 클릭 이벤트 등록
        btn = GetComponent<Button>();
        btn.onClick.AddListener(but_event);

        // 멀티 터치 비활성화
        Input.multiTouchEnabled = false;

        // 이미지 컴포넌트 캐싱 및 초기 비활성화
        imgComp = image.GetComponent<Image>();
        image.SetActive(false);

        // 도감 패널을 띄울 캔버스
        canvas2 = GameObject.Find("Canvas2")?.transform;
    }

    private void Start()
    {
        // 시작 시 한 번 해금 여부 체크
        TryUnlock();
    }

    private void Update()
    {
        // 아직 해금되지 않은 경우만 주기적으로 체크
        if (!isUnlocked)
            TryUnlock();
    }

    // 캐릭터 해금 여부 확인 및 UI 갱신
    private void TryUnlock()
    {
        // 캐릭터 데이터 가져오기
        var character = CharacterManager.Instance?.GetItem(index_book);
        if (character == null || !character.spawncheck)
            return;

        // 해금 처리
        isUnlocked = true;

        // 캐릭터 이미지 설정
        if (character.itemimg != null)
        {
            imgComp.sprite = character.itemimg;
            image.SetActive(true);
        }

        // 도감 이름 설정
        if (index_book == 0)
            book_name.text = character.name;
        else
            book_name.text = index_book + ". " + character.name;
    }

    // 도감 버튼 클릭 시 호출
    public void but_event()
    {
        // 해금되지 않았으면 동작하지 않음
        if (!isUnlocked)
            return;

        // 선택된 도감 번호 저장
        GameData data = SaveManager.Load();
        data.upgrades.booknum = index_book;
        SaveManager.Save(data);

        // 도감 상세 패널 생성
        if (canvas2 != null && Book_prefab != null)
            Instantiate(Book_prefab, Vector3.zero, Quaternion.identity, canvas2);
    }
}