using UnityEngine;
using UnityEngine.UI;

public class tokenShop : MonoBehaviour
{
    public enum ShopType { Character, Lotto_value, Lotto_reward }
    public ShopType type;

    public static System.Action OnTokenShopUpdated;   // ★ UI 업데이트 이벤트

    private Button button;
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        Input.multiTouchEnabled = false;
        button = GetComponent<Button>();
    }

    private void Start()
    {
        RefreshButtonState();     // ← 게임 시작 시 잠금 체크
    }

    private void Update()
    {
        if (type != ShopType.Character) return;
    }

    private void RefreshButtonState()
    {
        if (type != ShopType.Character) return;

        GameData data = SaveManager.Load();
        int maxIndex = CharacterManager.Instance.GetCount() - 1;

        button.interactable = data.upgrades.upCh < maxIndex;
    }

    public void OnClick()
    {
        GameData data = SaveManager.Load();
        bool purchased = false;

        switch (type)
        {
            case ShopType.Character:
                purchased = TryPurchase(
                    ref data.shops.tokenshop_1_price,
                    1,
                    ref data.currency.token,
                    () =>
                    {
                        PlaySFX();

                        int maxIndex = CharacterManager.Instance.GetCount() - 1;

                        data.upgrades.upCh++;

                        // 지나친 증가 방지
                        if (data.upgrades.upCh > maxIndex)
                            data.upgrades.upCh = maxIndex;

                        data.upgrades.count = data.upgrades.upCh;
                        data.upgrades.chprefab = data.upgrades.count;

                        // 구매 후 버튼 잠금 (61개면 잠김)
                        button.interactable = data.upgrades.upCh < maxIndex;

                        ShopButton.GameEvents.OnSettingsChanged?.Invoke();
                        updown.OnUpDownChanged?.Invoke();
                    });

                if (!purchased)
                {
                    AppearTextManager.Instance.Show("토큰이 부족합니다!");
                    return;
                }
                break;


            case ShopType.Lotto_value:

                if (data.lottos.lotto_5_value <= 0f)
                {
                    AppearTextManager.Instance.Show("이미 최대치입니다!");
                    return;
                }

                purchased = TryPurchase(
                    ref data.shops.tokenshop_2_price,
                    500,
                    ref data.currency.dia,
                    () =>
                    {
                        PlaySFX();

                        data.lottos.lotto_1_value += 0.2f;
                        data.lottos.lotto_2_value += 0.4f;
                        data.lottos.lotto_3_value += 1.9f;
                        data.lottos.lotto_4_value += 1.5f;
                        data.lottos.lotto_5_value -= 4f;

                        if (data.lottos.lotto_5_value < 0)
                            data.lottos.lotto_5_value = 0;
                    });

                if (!purchased)
                {
                    AppearTextManager.Instance.Show("다이아가 부족합니다!");
                    return;
                }
                break;


            case ShopType.Lotto_reward:
                purchased = TryPurchase(
                    ref data.shops.tokenshop_3_price,
                    1000,
                    ref data.currency.dia,
                    () =>
                    {
                        PlaySFX();

                        data.lottos.lotto_1_reward += 2;
                        data.lottos.lotto_2_reward += 1;
                        data.lottos.lotto_3_reward += 100;
                        data.lottos.lotto_4_reward += 2000;

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

        SaveManager.Save(data);

        OnTokenShopUpdated?.Invoke();
    }

    private bool TryPurchase(ref int price, int increment, ref int currency, System.Action onBuy)
    {
        if (currency < price)
            return false;

        currency -= price;
        price += increment;

        onBuy.Invoke();
        return true;
    }

    private void PlaySFX()
    {
        if (Setting.IsSFXOn() && audioSource != null)
            audioSource.Play();
    }
}