using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class tokenViewer : MonoBehaviour
{
    public enum ShopType { Character, Lotto_value, Lotto_reward }
    public ShopType type;

    public Image thisimg;
    public Button button;
    public TextMeshProUGUI text_ch, text_lotto, text;

    private GameData data;

    private void OnEnable()
    {
        data = SaveManager.Load();
        RefreshUI();

        tokenShop.OnTokenShopUpdated += RefreshUI;   // ★ 이벤트 등록
    }

    private void OnDisable()
    {
        tokenShop.OnTokenShopUpdated -= RefreshUI;   // ★ 이벤트 해제
    }

    public void RefreshUI()
    {
        switch (type)
        {
            case ShopType.Character:
                UpdateCharacterUI();
                break;

            case ShopType.Lotto_value:
                UpdateLottoValueUI();
                break;

            case ShopType.Lotto_reward:
                UpdateLottoRewardUI();
                break;
        }
    }

    // ---------------------------------------------------------
    //                        CHARACTER
    // ---------------------------------------------------------
    private void UpdateCharacterUI()
    {
        int upch = data.upgrades.count;

        var character = CharacterManager.Instance.GetItem(upch);
        var nextChar = CharacterManager.Instance.GetItem(upch + 1);

        if (character == null) return;

        thisimg.sprite = character.itemimg;

        if (nextChar != null)
            text_ch.text = $"{character.name} ⇒ {nextChar.name}";
        else
            text_ch.text = $"{character.name} ⇒ MAX";

        text.text = $"{data.shops.tokenshop_1_price:N0} 토큰";
    }

    // ---------------------------------------------------------
    //                        LOTTO VALUE
    // ---------------------------------------------------------
    private void UpdateLottoValueUI()
    {
        float v1 = data.lottos.lotto_1_value;
        float v2 = data.lottos.lotto_2_value;
        float v3 = data.lottos.lotto_3_value;
        float v4 = data.lottos.lotto_4_value;
        float v5 = data.lottos.lotto_5_value;

        text_lotto.text =
            $"1등 : {v1:F1}% ⇒ {(v1 + 0.2f):F1}% (+0.2%)\n" +
            $"2등 : {v2:F1}% ⇒ {(v2 + 0.4f):F1}% (+0.4%)\n" +
            $"3등 : {v3:F1}% ⇒ {(v3 + 1.9f):F1}% (+1.9%)\n" +
            $"4등 : {v4:F1}% ⇒ {(v4 + 1.5f):F1}% (+1.5%)\n" +
            $"5등 : {v5:F1}% ⇒ {(v5 - 4f):F1}% (-4%)";

        text.text = $"{data.shops.tokenshop_2_price:N0} 다이아";

        // MAX 조건 예: 5등 확률이 0 이하 내려가면 제한
        if (v5 <= 10)
        {
            text_lotto.text =
            $"1등 : {v1:F1}%\n" +
            $"2등 : {v2:F1}%\n" +
            $"3등 : {v3:F1}%\n" +
            $"4등 : {v4:F1}%\n" +
            $"5등 : {v5:F1}%";

            button.interactable = false;
            text.text = "MAX";
        }
    }

    // ---------------------------------------------------------
    //                        LOTTO REWARD
    // ---------------------------------------------------------
    private void UpdateLottoRewardUI()
    {
        string F(int n) => $"{n:N0}";

        text_lotto.text =
            $"1등 : 토큰 {F(data.lottos.lotto_1_reward)}개 ⇒ {F(data.lottos.lotto_1_reward + 2)}개 (+{F(2)}개)\n" +
            $"2등 : 토큰 {F(data.lottos.lotto_2_reward)}개 ⇒ {F(data.lottos.lotto_2_reward + 1)}개 (+{F(1)}개)\n" +
            $"3등 : 다이아 {F(data.lottos.lotto_3_reward)}개 ⇒ {F(data.lottos.lotto_3_reward + 100)}개 (+100개)\n" +
            $"4등 : 돌멩이 {F(data.lottos.lotto_4_reward)}개 ⇒ {F(data.lottos.lotto_4_reward + 2000)}개 (+2,000개)\n" +
            $"5등 : 꽝";

        text.text = $"{data.shops.tokenshop_3_price:N0} 다이아";

        if (data.lottos.lotto_1_reward >= 10)
        {
            text_lotto.text =
            $"1등 : 토큰 {F(data.lottos.lotto_1_reward)}개\n" +
            $"2등 : 토큰 {F(data.lottos.lotto_2_reward)}개\n" +
            $"3등 : 다이아 {F(data.lottos.lotto_3_reward)}개\n" +
            $"4등 : 돌멩이 {F(data.lottos.lotto_4_reward)}개\n" +
            $"5등 : 꽝";

            button.interactable = false;
            text.text = "MAX";
        }
    }
}