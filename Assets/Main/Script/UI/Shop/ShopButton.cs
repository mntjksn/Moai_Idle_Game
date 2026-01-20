using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 상점 업그레이드 버튼(4종)을 담당
public class ShopButton : MonoBehaviour
{
    public enum ShopType { ChildMax, GetGoldTime, ClickMax, SpawnTime }
    public ShopType shopType;

    [SerializeField] private AudioSource audioSource;

    private Button button;

    // 설정값이 바뀌었음을 다른 UI에 알리기 위한 전역 이벤트
    public static class GameEvents
    {
        public static System.Action OnSettingsChanged;
    }

    private void Awake()
    {
        Input.multiTouchEnabled = false;
        button = GetComponent<Button>();
    }

    // 버튼 클릭 이벤트(Inspector에서 연결하거나 Button.onClick으로 연결)
    public void but_event()
    {
        // 데이터 로드
        GameData data = SaveManager.Load();

        // 골드 값을 로컬로 캐싱(계산 중 data.currency.gold를 계속 접근하지 않도록)
        int gold = data.currency.gold;

        // 구매 성공 여부(돈 부족이면 저장/갱신/이벤트를 막기 위함)
        bool purchased = false;

        switch (shopType)
        {
            case ShopType.ChildMax:
                purchased = BuyChildMax(data, ref gold);
                break;

            case ShopType.GetGoldTime:
                purchased = BuyGetGoldTime(data, ref gold);
                break;

            case ShopType.ClickMax:
                purchased = BuyClickMax(data, ref gold);
                break;

            case ShopType.SpawnTime:
                purchased = BuySpawnTime(data, ref gold);
                break;
        }

        // 구매 실패(돈 부족 등)면 여기서 종료
        if (!purchased)
            return;

        // 최종 골드 반영 후 저장
        data.currency.gold = gold;
        SaveManager.Save(data);

        // 상점 정보 UI 갱신
        ShopItemInfoViewer.RefreshAll();

        // 다른 UI(텍스트, 버튼 등) 갱신 요청
        GameEvents.OnSettingsChanged?.Invoke();
    }

    // 효과음 재생(설정 OFF면 재생 안 함)
    private void PlaySFX()
    {
        if (audioSource == null)
            return;

        if (Setting.IsSFXOn())
            audioSource.Play();
    }

    //=====================================================
    // ChildMax 구매: 배치 가능한 캐릭터 최대치 증가
    //=====================================================
    private bool BuyChildMax(GameData data, ref int gold)
    {
        if (button == null)
            button = GetComponent<Button>();

        int price = data.shops.shop_1_price;
        int childMax = data.settings.childMax;
        int level = data.shops.shop_1_level;

        // 돈 부족
        if (gold < price)
        {
            AppearTextManager.Instance.Show("돌멩이가 부족합니다!");
            return false;
        }

        PlaySFX();

        // 결제 및 적용
        gold -= price;
        childMax += 1;

        data.settings.childMax = childMax;
        data.shops.shop_1_level++;

        // 가격 증가(기존 공식 유지)
        data.shops.shop_1_price = data.shops.shop_1_price + Mathf.RoundToInt(55 + Mathf.Pow(level, 3.45f) * 8);

        return true;
    }

    //=====================================================
    // GetGoldTime 구매: 골드 획득 주기(getGoldTime) 감소
    //=====================================================
    private bool BuyGetGoldTime(GameData data, ref int gold)
    {
        if (button == null)
            button = GetComponent<Button>();

        int price = data.shops.shop_2_price;
        float time = data.settings.getGoldTime;
        int level = data.shops.shop_2_level;

        if (gold < price)
        {
            AppearTextManager.Instance.Show("돌멩이가 부족합니다!");
            return false;
        }

        PlaySFX();

        gold -= price;

        // 최소 1.1초까지 감소
        time = Mathf.Max(time - 0.1f, 1.1f);

        data.settings.getGoldTime = time;
        data.shops.shop_2_level++;

        // 가격 증가(기존 공식 유지)
        data.shops.shop_2_price = Mathf.RoundToInt(level + 52 + Mathf.Pow(level, 4.25f) * 9);

        // 최소치 도달 시 더 이상 구매 불가
        if (time <= 1.1f)
            button.interactable = false;

        return true;
    }

    //=====================================================
    // ClickMax 구매: 소환 버튼 횟수 최대치 증가
    //=====================================================
    private bool BuyClickMax(GameData data, ref int gold)
    {
        int price = data.shops.shop_3_price;
        int clickMax = data.settings.clickMax;
        int level = data.shops.shop_3_level;

        if (gold < price)
        {
            AppearTextManager.Instance.Show("돌멩이가 부족합니다!");
            return false;
        }

        PlaySFX();

        gold -= price;
        clickMax++;

        data.settings.clickMax = clickMax;
        data.shops.shop_3_level++;

        // 가격 증가(기존 공식 유지)
        data.shops.shop_3_price = Mathf.RoundToInt(45 + Mathf.Pow(level, 5) * 6);

        return true;
    }

    //=====================================================
    // SpawnTime 구매: 소환 쿨타임(spawnTime) 감소
    //=====================================================
    private bool BuySpawnTime(GameData data, ref int gold)
    {
        if (button == null)
            button = GetComponent<Button>();

        int price = data.shops.shop_4_price;
        float time = data.settings.spawnTime;
        int level = data.shops.shop_4_level;

        if (gold < price)
        {
            AppearTextManager.Instance.Show("돌멩이가 부족합니다!");
            return false;
        }

        PlaySFX();

        gold -= price;

        // 최소 1.1초까지 감소
        time = Mathf.Max(time - 0.1f, 1.1f);

        data.settings.spawnTime = time;
        data.shops.shop_4_level++;

        // 가격 증가(기존 공식 유지)
        data.shops.shop_4_price =
            Mathf.RoundToInt(Random.Range(30f, 50f) + level + Mathf.Pow(level, 3.95f) * 14);

        if (time <= 1.1f)
            button.interactable = false;

        return true;
    }
}