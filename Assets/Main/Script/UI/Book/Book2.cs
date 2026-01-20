using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 배경 도감(Book2) UI 한 칸을 담당
// - 배경 해금 여부에 따라 잠금 표시/버튼 활성화 처리
// - 클릭 시 배경 선택 + 상세 패널 오픈
public class Book2 : MonoBehaviour
{
    // 배경 인덱스(BackgroundManager의 인덱스와 동일하게 사용)
    public int index_book2;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI book_name;   // 배경 이름
    [SerializeField] private TextMeshProUGUI sub_text;    // 배경 설명
    [SerializeField] private Image thumbImage;            // 썸네일 이미지
    [SerializeField] private GameObject lockCover;        // 잠금 커버(잠금 상태일 때 표시)

    // 상세 정보 패널 프리팹
    public GameObject Book2_prefab;

    private Button btn;

    // 현재 해금 여부(캐시)
    private bool isUnlocked;

    // 상세 패널을 띄울 캔버스
    private Transform canvas2;

    // 현재 표시 중인 배경 데이터(해금 상태 추적용)
    private BackgroundItem currentData;

    private void Awake()
    {
        // 버튼 이벤트 연결
        btn = GetComponent<Button>();
        if (btn != null)
            btn.onClick.AddListener(OnClick);

        // 패널을 올릴 캔버스
        canvas2 = GameObject.Find("Canvas2")?.transform;

        // 멀티 터치 비활성화
        Input.multiTouchEnabled = false;
    }

    // 외부에서 배경 데이터 주입 + UI 초기 세팅
    public void Setup(BackgroundItem data, int index)
    {
        index_book2 = index;
        currentData = data;

        ApplyDataToUI();
    }

    // currentData를 UI에 반영
    private void ApplyDataToUI()
    {
        // 이름 표시
        if (book_name != null)
            book_name.text = currentData != null ? currentData.name : "이름없음";

        // 설명 표시
        if (sub_text != null)
            sub_text.text = currentData != null ? currentData.sub2 : "";

        // 썸네일 표시
        if (thumbImage != null)
        {
            if (currentData != null && currentData.itemimg != null)
            {
                thumbImage.sprite = currentData.itemimg;
                thumbImage.gameObject.SetActive(true);
            }
            else
            {
                thumbImage.gameObject.SetActive(false);
            }
        }

        // 해금 여부 갱신
        isUnlocked = (currentData != null && currentData.spawncheck);

        // 잠금 커버 표시
        if (lockCover != null)
            lockCover.SetActive(!isUnlocked);

        // 잠금 상태면 버튼 클릭 불가 처리
        if (btn != null)
            btn.interactable = isUnlocked;
    }

    private void Update()
    {
        // 이미 해금 상태면 더 체크할 필요 없음
        if (isUnlocked)
            return;

        // 최신 배경 데이터 확인(해금 여부 반영 목적)
        var latest = BackgroundManager.Instance.GetItem(index_book2);

        // 해금 상태가 바뀌었으면 UI 갱신
        if (latest != null && latest.spawncheck)
        {
            currentData = latest;
            ApplyDataToUI();
        }
    }

    // 배경 칸 클릭 시 호출
    private void OnClick()
    {
        if (!isUnlocked)
            return;

        // 배경 선택 반영
        BackgroundManager.Instance.SelectBackground(index_book2);

        // 선택한 배경 인덱스 저장
        GameData data = SaveManager.Load();
        data.upgrades.background = index_book2;
        SaveManager.Save(data);

        // 상세 패널 생성
        if (canvas2 != null && Book2_prefab != null)
            Instantiate(Book2_prefab, Vector3.zero, Quaternion.identity, canvas2);
    }
}