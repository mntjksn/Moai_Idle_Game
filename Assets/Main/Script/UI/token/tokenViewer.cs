using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 토큰샵 UI 표시 전용(가격/설명/미리보기)
// - tokenShop의 구매 이벤트(OnTokenShopUpdated)로 갱신
// - Update 없이 이벤트 기반으로만 동작
public class tokenViewer : MonoBehaviour
{
    public enum ShopType { Character, Lotto_value, Lotto_reward }
    public ShopType type;

    [Header("UI")]
    public Image thisimg;
    public Button button;
    public TextMeshProUGUI text_ch;     // 캐릭터 변화 표시
    public TextMeshProUGUI text_lotto;  // 로또 정보 표시
    public TextMeshProUGUI text;        // 가격 표시

    private GameData data;

    private void OnEnable()
    {
        // 이벤트 등록 전에 중복 방지
        tokenShop.OnTokenShopUpdated -= RefreshUI;
        tokenShop.OnTokenShopUpdated += RefreshUI;

        RefreshUI(); // 패널 켜질 때 즉시 1회 갱신
    }

    private void OnDisable()
    {
        tokenShop.OnTokenShopUpdated -= RefreshUI;
    }

    // 현재 저장 데이터 기준으로 UI를 최신 상태로 갱신
    // (★ 중요한 포인트: 여기서 Load를 매번 해줘야 최신 반영됨)
    public void RefreshUI()
    {
        data = SaveManager.Load();

        // 기본값: 버튼 활성 (필요 시 아래에서 MAX 처리)
        if (button != null)
            button.interactable = true;

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

    // =========================================================
    //  CHARACTER : 토큰으로 캐릭터 단계 업그레이드
    // =========================================================
    private void UpdateCharacterUI()
    {
        int upch = data.upgrades.count;

        var character = CharacterManager.Instance.GetItem(upch);
        var nextChar = CharacterManager.Instance.GetItem(upch + 1);

        if (character == null) return;

        // 현재 캐릭터 이미지
        if (thisimg != null)
            thisimg.sprite = character.itemimg;

        // 이름 표시 (다음 단계 없으면 MAX)
        if (text_ch != null)
        {
            if (nextChar != null)
                text_ch.text = $"{character.name} ⇒ {nextChar.name}";
            else
                text_ch.text = $"{character.name} ⇒ MAX";
        }

        // 가격 표시
        if (text != null)
            text.text = $"{data.shops.tokenshop_1_price:N0} 토큰";

        // MAX면 버튼 잠금(안전)
        if (nextChar == null && button != null)
        {
            button.interactable = false;
            if (text != null) text.text = "MAX";
        }
    }

    // =========================================================
    //  LOTTO VALUE : 로또 확률 조정(다이아)
    // =========================================================
    private void UpdateLottoValueUI()
    {
        float v1 = data.lottos.lotto_1_value;
        float v2 = data.lottos.lotto_2_value;
        float v3 = data.lottos.lotto_3_value;
        float v4 = data.lottos.lotto_4_value;
        float v5 = data.lottos.lotto_5_value;

        // 가격 표시
        if (text != null)
            text.text = $"{data.shops.tokenshop_2_price:N0} 다이아";

        // MAX 조건은 tokenShop과 동일하게(5등이 10이면 더 못 올림)
        if (v5 <= 10)
        {
            if (text_lotto != null)
            {
                text_lotto.text =
                    $"1등 : {v1:F1}%\n" +
                    $"2등 : {v2:F1}%\n" +
                    $"3등 : {v3:F1}%\n" +
                    $"4등 : {v4:F1}%\n" +
                    $"5등 : {v5:F1}%";
            }

            if (button != null) button.interactable = false;
            if (text != null) text.text = "MAX";
            return;
        }

        // 업그레이드 미리보기(다음 값)
        if (text_lotto != null)
        {
            text_lotto.text =
                $"1등 : {v1:F1}% ⇒ {(v1 + 0.2f):F1}% (+0.2%)\n" +
                $"2등 : {v2:F1}% ⇒ {(v2 + 0.4f):F1}% (+0.4%)\n" +
                $"3등 : {v3:F1}% ⇒ {(v3 + 1.9f):F1}% (+1.9%)\n" +
                $"4등 : {v4:F1}% ⇒ {(v4 + 1.5f):F1}% (+1.5%)\n" +
                $"5등 : {v5:F1}% ⇒ {(v5 - 4f):F1}% (-4%)";
        }
    }

    // =========================================================
    //  LOTTO REWARD : 로또 보상 증가(다이아)
    // =========================================================
    private void UpdateLottoRewardUI()
    {
        string F(int n) => $"{n:N0}";

        // 가격 표시
        if (text != null)
            text.text = $"{data.shops.tokenshop_3_price:N0} 다이아";

        // MAX 조건 (tokenShop과 동일: 1등 보상 10 이상이면 제한)
        if (data.lottos.lotto_1_reward >= 10)
        {
            if (text_lotto != null)
            {
                text_lotto.text =
                    $"1등 : 토큰 {F(data.lottos.lotto_1_reward)}개\n" +
                    $"2등 : 토큰 {F(data.lottos.lotto_2_reward)}개\n" +
                    $"3등 : 다이아 {F(data.lottos.lotto_3_reward)}개\n" +
                    $"4등 : 돌멩이 {F(data.lottos.lotto_4_reward)}개\n" +
                    $"5등 : 꽝";
            }

            if (button != null) button.interactable = false;
            if (text != null) text.text = "MAX";
            return;
        }

        // 업그레이드 미리보기
        if (text_lotto != null)
        {
            text_lotto.text =
                $"1등 : 토큰 {F(data.lottos.lotto_1_reward)}개 ⇒ {F(data.lottos.lotto_1_reward + 2)}개 (+2개)\n" +
                $"2등 : 토큰 {F(data.lottos.lotto_2_reward)}개 ⇒ {F(data.lottos.lotto_2_reward + 1)}개 (+1개)\n" +
                $"3등 : 다이아 {F(data.lottos.lotto_3_reward)}개 ⇒ {F(data.lottos.lotto_3_reward + 100)}개 (+100개)\n" +
                $"4등 : 돌멩이 {F(data.lottos.lotto_4_reward)}개 ⇒ {F(data.lottos.lotto_4_reward + 2000)}개 (+2,000개)\n" +
                $"5등 : 꽝";
        }
    }
}