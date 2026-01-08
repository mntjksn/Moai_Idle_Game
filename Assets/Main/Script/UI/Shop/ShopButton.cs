using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopButton : MonoBehaviour
{
    public enum ShopType { ChildMax, GetGoldTime, ClickMax, SpawnTime }
    public ShopType shopType;

    [SerializeField] private AudioSource audioSource;

    private Button button;

    public static class GameEvents
    {
        public static System.Action OnSettingsChanged;
    }


    private void Awake()
    {
        Input.multiTouchEnabled = false;
        button = GetComponent<Button>();
    }

    public void but_event()
    {
        GameData data = SaveManager.Load();

        // 로컬 변수로 캐싱 (성능 ↓ 방지)
        int gold = data.currency.gold;

        switch (shopType)
        {
            case ShopType.ChildMax:
                BuyChildMax(data, ref gold);
                break;

            case ShopType.GetGoldTime:
                BuyGetGoldTime(data, ref gold);
                break;

            case ShopType.ClickMax:
                BuyClickMax(data, ref gold);
                break;

            case ShopType.SpawnTime:
                BuySpawnTime(data, ref gold);
                break;
        }

        data.currency.gold = gold;

        SaveManager.Save(data);

        ShopItemInfoViewer.RefreshAll();

        GameEvents.OnSettingsChanged?.Invoke();
    }

    private void PlaySFX()
    {
        if (Setting.IsSFXOn())
            audioSource.Play();
    }

    //-----------------------------------------
    //      ChildMax 구매
    //-----------------------------------------
    private void BuyChildMax(GameData data, ref int gold)
    {
        int price = data.shops.shop_1_price;
        int childMax = data.settings.childMax;
        int level = data.shops.shop_1_level;

        if (gold < price)
        {
            AppearTextManager.Instance.Show("돌멩이가 부족합니다!");
            return;
        }

        PlaySFX();

        gold -= price;
        childMax += 1;

        data.settings.childMax = childMax;
        data.shops.shop_1_level++;
        data.shops.shop_1_price = data.shops.shop_1_price + Mathf.RoundToInt(55 + Mathf.Pow(level, 3.45f) * 8);

        // 최대치 도달 시 비활성화
        if (childMax >= CharacterManager.Instance.characters.Count)
            button.interactable = false;
    }

    //-----------------------------------------
    //      GoldTime 구매
    //-----------------------------------------
    private void BuyGetGoldTime(GameData data, ref int gold)
    {
        int price = data.shops.shop_2_price;
        float time = data.settings.getGoldTime;
        int level = data.shops.shop_2_level;

        if (gold < price)
        {
            AppearTextManager.Instance.Show("돌멩이가 부족합니다!");
            return;
        }

        PlaySFX();

        gold -= price;
        time = Mathf.Max(time - 0.1f, 1.1f);

        data.settings.getGoldTime = time;
        data.shops.shop_2_level++;
        data.shops.shop_2_price = Mathf.RoundToInt(level + 52 + Mathf.Pow(level, 4.25f) * 9);

        if (time <= 1.1f)
            button.interactable = false;
    }

    //-----------------------------------------
    //      ClickMax 구매
    //-----------------------------------------
    private void BuyClickMax(GameData data, ref int gold)
    {
        int price = data.shops.shop_3_price;
        int clickMax = data.settings.clickMax;
        int level = data.shops.shop_3_level;

        if (gold < price)
        {
            AppearTextManager.Instance.Show("돌멩이가 부족합니다!");
            return;
        }

        PlaySFX();

        gold -= price;
        clickMax++;

        data.settings.clickMax = clickMax;
        data.shops.shop_3_level++;
        data.shops.shop_3_price = Mathf.RoundToInt(45 + Mathf.Pow(level, 5) * 6);
    }

    //-----------------------------------------
    //      SpawnTime 구매
    //-----------------------------------------
    private void BuySpawnTime(GameData data, ref int gold)
    {
        int price = data.shops.shop_4_price;
        float time = data.settings.spawnTime;
        int level = data.shops.shop_4_level;

        if (gold < price)
        {
            AppearTextManager.Instance.Show("돌멩이가 부족합니다!");
            return;
        }

        PlaySFX();

        gold -= price;
        time = Mathf.Max(time - 0.1f, 1.1f);

        data.settings.spawnTime = time;
        data.shops.shop_4_level++;
        data.shops.shop_4_price =
            Mathf.RoundToInt(Random.Range(30f, 50f) + level + Mathf.Pow(level, 3.95f) * 14);

        if (time <= 1.1f)
            button.interactable = false;
    }
}