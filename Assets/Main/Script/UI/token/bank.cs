using UnityEngine;
using UnityEngine.UI;

// 다이아를 사용해서 티켓/토큰을 구매하는 교환(은행) 버튼 로직
// - 버튼 1개가 BankType에 따라 서로 다른 상품을 판매
// - 구매 실패 시 안내 텍스트 출력
// - 구매 성공 시 사운드 + 저장
public class bank : MonoBehaviour
{
    // 어떤 상품을 구매할지 구분
    public enum BankType { ticket, token }
    public BankType bankType;

    [SerializeField] private AudioSource audioSource;

    // 버튼 참조 캐싱
    private Button button;

    private void Awake()
    {
        // 멀티터치 방지(연타/중복 클릭 방지 목적)
        Input.multiTouchEnabled = false;

        // 버튼 컴포넌트 캐싱
        button = GetComponent<Button>();
    }

    // 버튼 클릭 시 호출 (Inspector 이벤트 연결용)
    public void but_event()
    {
        // 최신 데이터 로드
        GameData data = SaveManager.Load();

        // 자주 쓰는 값은 로컬로 빼서 작업 (가독성 + 실수 방지)
        int dia = data.currency.dia;
        int ticket = data.currency.ticket;
        int token = data.currency.token;

        bool purchased = false;  // 구매 성공 여부
        string failMsg = "";     // 실패 메시지

        // BankType에 따라 다른 상품 구매 시도
        switch (bankType)
        {
            case BankType.ticket:
                // 다이아 1000 → 티켓 1
                purchased = TryBuy(ref dia, 1000, ref ticket, 1);
                if (!purchased) failMsg = "다이아가 부족합니다!";
                break;

            case BankType.token:
                // 다이아 10000 → 토큰 1
                purchased = TryBuy(ref dia, 10000, ref token, 1);
                if (!purchased) failMsg = "다이아가 부족합니다!";
                break;
        }

        // 구매 실패 시: 안내만 띄우고 종료(저장 X)
        if (!purchased)
        {
            AppearTextManager.Instance.Show(failMsg);
            return;
        }

        // 구매 성공 시: 사운드 재생
        PlaySFX();

        // 결과 값을 다시 데이터에 반영 후 저장
        data.currency.dia = dia;
        data.currency.ticket = ticket;
        data.currency.token = token;

        SaveManager.Save(data);
    }

    // 공통 구매 처리 함수
    // costType: 차감할 재화 (ref)
    // required: 필요한 비용
    // rewardType: 지급할 재화 (ref)
    // give: 지급량
    private bool TryBuy(ref int costType, int required, ref int rewardType, int give)
    {
        // 비용 부족 시 실패
        if (costType < required)
            return false;

        // 비용 차감 + 보상 지급
        costType -= required;
        rewardType += give;
        return true;
    }

    // 효과음 재생 (SFX 설정 ON일 때만)
    private void PlaySFX()
    {
        if (Setting.IsSFXOn() && audioSource != null)
            audioSource.Play();
    }
}