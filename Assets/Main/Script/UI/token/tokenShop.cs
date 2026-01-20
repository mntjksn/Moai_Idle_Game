using UnityEngine;
using UnityEngine.UI;

// 토큰/다이아로 강화(캐릭터 단계), 로또 확률, 로또 보상 등을 업그레이드하는 상점 버튼
// - type에 따라 서로 다른 상품을 처리
// - 구매 성공 시 저장 + UI 갱신 이벤트 발행
public class tokenShop : MonoBehaviour
{
    // 상점 버튼 종류
    // Character    : 토큰으로 캐릭터 강화 단계 증가
    // Lotto_value  : 다이아로 로또 확률 조정
    // Lotto_reward : 다이아로 로또 보상 증가
    public enum ShopType { Character, Lotto_value, Lotto_reward }
    public ShopType type;

    // 토큰 상점 관련 UI를 갱신해야 할 때 외부에 알리는 이벤트
    // (예: 가격 표시, 버튼 interactable 갱신 등)
    public static System.Action OnTokenShopUpdated;

    // 버튼 참조 캐싱
    private Button button;

    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        Input.multiTouchEnabled = false;

        // 같은 오브젝트의 Button 캐싱
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        // 패널/오브젝트가 켜질 때마다 버튼 잠금 상태 최신화
        RefreshButtonState();
    }

    private void Start()
    {
        // 게임 시작 시 1회 잠금 체크
        RefreshButtonState();
    }

    // Character 타입일 때만 "더 이상 구매 불가능" 상태면 버튼 잠금
    // (예: 최대 캐릭터 단계 도달)
    private void RefreshButtonState()
    {
        if (type != ShopType.Character) return;
        if (button == null) return;

        GameData data = SaveManager.Load();

        // 마지막 캐릭터 인덱스(최대 단계)
        int maxIndex = CharacterManager.Instance.GetCount() - 1;

        // upCh가 maxIndex 이상이면 더 이상 강화 불가
        button.interactable = data.upgrades.upCh < maxIndex;
    }

    // 버튼 클릭 처리 (Inspector OnClick 연결)
    public void OnClick()
    {
        GameData data = SaveManager.Load();
        bool purchased = false;

        switch (type)
        {
            // =========================================================
            // 캐릭터 단계 구매 (토큰 소비)
            // =========================================================
            case ShopType.Character:
                purchased = TryPurchase(
                    ref data.shops.tokenshop_1_price,   // 가격(증가형)
                    increment: 1,                       // 구매 후 가격 증가량
                    ref data.currency.token,            // 결제 재화(토큰)
                    () =>
                    {
                        PlaySFX();

                        int maxIndex = CharacterManager.Instance.GetCount() - 1;

                        // 강화 단계 증가
                        data.upgrades.upCh++;

                        // 최대치 클램프
                        if (data.upgrades.upCh > maxIndex)
                            data.upgrades.upCh = maxIndex;

                        // 게임에서 쓰는 강화 카운트 동기화
                        data.upgrades.count = data.upgrades.upCh;
                        data.upgrades.chprefab = data.upgrades.count;

                        // 구매 후 버튼 상태 갱신
                        if (button != null)
                            button.interactable = data.upgrades.upCh < maxIndex;

                        // 다른 UI/시스템 갱신 이벤트
                        ShopButton.GameEvents.OnSettingsChanged?.Invoke();
                        updown.OnUpDownChanged?.Invoke();
                    });

                if (!purchased)
                {
                    AppearTextManager.Instance.Show("토큰이 부족합니다!");
                    return;
                }
                break;

            // =========================================================
            // 로또 확률 업그레이드 (다이아 소비)
            // =========================================================
            case ShopType.Lotto_value:
                // 5등 확률이 0이면 더 이상 올릴 수 없음
                if (data.lottos.lotto_5_value <= 0f)
                {
                    AppearTextManager.Instance.Show("이미 최대치입니다!");
                    return;
                }

                purchased = TryPurchase(
                    ref data.shops.tokenshop_2_price,
                    increment: 500,
                    ref data.currency.dia,
                    () =>
                    {
                        PlaySFX();

                        // 확률 분배 조정
                        data.lottos.lotto_1_value += 0.2f;
                        data.lottos.lotto_2_value += 0.4f;
                        data.lottos.lotto_3_value += 1.9f;
                        data.lottos.lotto_4_value += 1.5f;
                        data.lottos.lotto_5_value -= 4f;

                        // 확률 방지
                        if (data.lottos.lotto_5_value < 10)
                            data.lottos.lotto_5_value = 10;
                    });

                if (!purchased)
                {
                    AppearTextManager.Instance.Show("다이아가 부족합니다!");
                    return;
                }
                break;

            // =========================================================
            // 로또 보상 업그레이드 (다이아 소비)
            // =========================================================
            case ShopType.Lotto_reward:
                purchased = TryPurchase(
                    ref data.shops.tokenshop_3_price,
                    increment: 1000,
                    ref data.currency.dia,
                    () =>
                    {
                        PlaySFX();

                        // 보상 증가
                        data.lottos.lotto_1_reward += 2;
                        data.lottos.lotto_2_reward += 1;
                        data.lottos.lotto_3_reward += 100;
                        data.lottos.lotto_4_reward += 2000;

                        // 1등 보상 상한
                        if (data.lottos.lotto_1_reward > 10)
                            data.lottos.lotto_1_reward = 10;
                    });

                if (!purchased)
                {
                    AppearTextManager.Instance.Show("다이아가 부족합니다!");
                    return;
                }
                break;
        }

        // 구매 성공한 경우만 저장
        SaveManager.Save(data);

        // 토큰 상점 UI 갱신 이벤트
        OnTokenShopUpdated?.Invoke();

        // Character 타입이면 버튼 잠금 상태도 즉시 재평가(안전)
        RefreshButtonState();
    }

    // 공통 구매 로직
    // - 재화가 부족하면 false
    // - 성공하면 재화 차감 + 가격 증가 + onBuy 실행
    private bool TryPurchase(ref int price, int increment, ref int currency, System.Action onBuy)
    {
        if (currency < price)
            return false;

        currency -= price;
        price += increment;

        onBuy?.Invoke();
        return true;
    }

    // 효과음 재생 (SFX ON일 때만)
    private void PlaySFX()
    {
        if (Setting.IsSFXOn() && audioSource != null)
            audioSource.Play();
    }
}