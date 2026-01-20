using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 캐릭터 관련 패널(현재 선택 캐릭터 / 도감 캐릭터)을 표시하는 UI 프리팹
// - 패널 타입에 따라 서로 다른 캐릭터 정보를 표시
// - 생성 시 한 번만 갱신(Update 사용 안 함)
public class chPanelPrefab : MonoBehaviour
{
    // 패널 표시 타입
    public enum PanelType
    {
        Ch_panel,     // 현재 선택된 캐릭터 정보
        Book_panel    // 도감에서 선택된 캐릭터 정보
    }

    public PanelType panelType;

    // UI 요소
    public Image thisimg;                 // 캐릭터 이미지
    public TextMeshProUGUI chname;        // 캐릭터 이름
    public TextMeshProUGUI sub;           // 캐릭터 설명
    public TextMeshProUGUI getgold;       // 캐릭터 획득 골드 표시

    // 패널 등장 시 효과음
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        // 효과음 재생
        if (audioSource != null && Setting.IsSFXOn())
            audioSource.Play();

        // 패널 생성 시 즉시 UI 갱신
        Refresh();
    }

    // 패널 UI를 한 번 갱신
    // (실시간 갱신이 필요 없으므로 Update 사용 안 함)
    public void Refresh()
    {
        // 최신 세이브 데이터 로드
        GameData data = SaveManager.Load();

        // 현재 선택된 캐릭터 인덱스
        int chIndex = data.upgrades.chprefab;

        // 도감에서 선택된 캐릭터 인덱스
        int bookIndex = data.upgrades.booknum;

        // 캐릭터 데이터 가져오기
        CharacterItem ch = CharacterManager.Instance.GetItem(chIndex);
        CharacterItem book = CharacterManager.Instance.GetItem(bookIndex);

        // 패널 타입에 따라 적용할 데이터 선택
        switch (panelType)
        {
            case PanelType.Ch_panel:
                Apply(ch, chIndex);
                break;

            case PanelType.Book_panel:
                Apply(book, bookIndex);
                break;
        }
    }

    // 캐릭터 데이터를 UI에 적용
    private void Apply(CharacterItem item, int index)
    {
        if (item == null)
            return;

        // 캐릭터 이미지
        if (thisimg != null)
            thisimg.sprite = item.itemimg;

        // 캐릭터 이름 (0번은 번호 표시 안 함)
        if (chname != null)
            chname.text = (index == 0) ? item.name : index + ". " + item.name;

        // 캐릭터 설명
        if (sub != null)
            sub.text = item.sub;

        // 캐릭터 골드 정보
        if (getgold != null)
            getgold.text = "획득 돌멩이 : " + item.itemgold.ToString("N0") + "개";
    }
}