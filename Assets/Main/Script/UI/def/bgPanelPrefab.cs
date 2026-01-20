using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 배경 관련 패널(Bg / Book2)의 UI 표시를 담당
// - 현재 선택된 배경 또는 최근 해금된 배경 정보를 표시
// - BackgroundManager 이벤트를 구독하여 자동 갱신
public class bgPanelPrefab : MonoBehaviour
{
    // 패널 타입 구분
    public enum PanelType
    {
        Bg_panel,      // 현재 선택된 배경 표시
        Book2_panel    // 최근 해금된 배경 표시
    }

    public PanelType panelType;

    // UI 요소
    public Image thisimg;                 // 배경 이미지
    public TextMeshProUGUI chname;        // 배경 이름
    public TextMeshProUGUI sub;           // 배경 설명

    // 패널 등장 시 효과음
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        // 효과음 재생
        if (audioSource != null && Setting.IsSFXOn())
            audioSource.Play();

        // 배경 변경 이벤트 등록
        if (BackgroundManager.Instance != null)
            BackgroundManager.Instance.OnBackgroundSelected += Refresh;

        // 현재 상태 즉시 반영
        if (BackgroundManager.Instance != null)
            Refresh(BackgroundManager.Instance.SelectedIndex);
    }

    private void OnDestroy()
    {
        // 이벤트 해제 (메모리 누수 방지)
        if (BackgroundManager.Instance != null)
            BackgroundManager.Instance.OnBackgroundSelected -= Refresh;
    }

    // 배경 변경 시 호출되는 갱신 함수
    // 이벤트 시그니처에 맞추기 위해 int 파라미터를 받지만 내부에서는 사용하지 않음
    private void Refresh(int _)
    {
        // 최신 세이브 데이터 로드
        GameData data = SaveManager.Load();

        // 현재 선택된 배경 인덱스
        int bgIndex = data.upgrades.background;

        // 최근 해금된 배경 인덱스
        int book2Index = data.upgrades.backgroundcheck;

        // 각 배경 데이터 가져오기
        BackgroundItem bg = BackgroundManager.Instance.GetItem(bgIndex);
        BackgroundItem book2 = BackgroundManager.Instance.GetItem(book2Index);

        // 패널 타입에 따라 표시할 데이터 선택
        switch (panelType)
        {
            case PanelType.Bg_panel:
                Apply(bg);
                break;

            case PanelType.Book2_panel:
                Apply(book2);
                break;
        }
    }

    // UI에 배경 데이터 적용
    private void Apply(BackgroundItem data)
    {
        if (data == null)
            return;

        if (thisimg != null)
            thisimg.sprite = data.itemimg;

        if (chname != null)
            chname.text = data.name;

        if (sub != null)
            sub.text = data.sub;
    }
}