using UnityEngine;
using UnityEngine.UI;

// 토큰샵/강화 단계 업다운 UI 제어
// - count(현재 선택 단계)를 upCh(최대 해금 단계) 범위 안에서 조절
// - 변경 시 저장 + 관련 UI 갱신
public class updown : MonoBehaviour
{
    // 외부에서 "업/다운 관련 UI 다시 갱신해!" 할 때 쓰는 이벤트
    public static System.Action OnUpDownChanged;

    [Header("Buttons")]
    public Button btn1; // DOWN
    public Button btn2; // UP

    private GameData data;

    private void OnEnable()
    {
        // 이벤트 중복 등록 방지
        OnUpDownChanged -= RefreshUI;
        OnUpDownChanged += RefreshUI;

        RefreshUI();
    }

    private void OnDisable()
    {
        OnUpDownChanged -= RefreshUI;
    }

    // 버튼 상태 갱신
    // - ★ 주의: 항상 최신 데이터를 읽어야 함 (이벤트로 호출될 수 있음)
    private void RefreshUI()
    {
        data = SaveManager.Load(); // ★ 여기서 매번 로드해야 최신 보장

        int upCh = data.upgrades.upCh;   // 최대 해금 단계
        int count = data.upgrades.count; // 현재 선택 단계

        // 최대 해금이 0이면 업/다운 전부 불가능
        if (upCh <= 0)
        {
            SetInteractable(false, false);
            return;
        }

        // count가 0이면 DOWN 불가, UP만 가능
        if (count <= 0)
        {
            SetInteractable(false, true);
            return;
        }

        // count가 upCh면 UP 불가, DOWN만 가능
        if (count >= upCh)
        {
            SetInteractable(true, false);
            return;
        }

        // 중간값이면 둘 다 가능
        SetInteractable(true, true);
    }

    private void SetInteractable(bool down, bool up)
    {
        if (btn1 != null) btn1.interactable = down;
        if (btn2 != null) btn2.interactable = up;
    }

    // =========================================================
    //  UP 버튼
    // =========================================================
    public void btn_up()
    {
        data = SaveManager.Load();

        // count < upCh 일 때만 증가 가능
        if (data.upgrades.count >= data.upgrades.upCh)
            return;

        data.upgrades.count++;
        data.upgrades.chprefab = data.upgrades.count;

        SaveManager.Save(data);

        // 이 스크립트 UI 갱신
        RefreshUI();

        // 관련 UI 갱신(현재 방식 유지)
        RefreshExternalUI();
    }

    // =========================================================
    //  DOWN 버튼
    // =========================================================
    public void btn_down()
    {
        data = SaveManager.Load();

        // 0 아래로 내려가면 안 됨
        if (data.upgrades.count <= 0)
            return;

        data.upgrades.count--;
        data.upgrades.chprefab = data.upgrades.count;

        SaveManager.Save(data);

        RefreshUI();
        RefreshExternalUI();
    }

    // 현재 구조에서만 쓰는 "외부 UI 강제 갱신"
    // - FindObjectOfType는 비싸서, 최소 1번씩만 찾도록 정리
    private void RefreshExternalUI()
    {
        var clickLimit = FindObjectOfType<ClickLimit>();
        if (clickLimit != null)
            clickLimit.RefreshCharacterImage();

        var viewer = FindObjectOfType<tokenViewer>();
        if (viewer != null)
            viewer.RefreshUI();
    }
}