using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopItemInfoViewer : MonoBehaviour
{
    public enum ShopType { Buy1, Buy2, Buy3, Buy4 }
    public ShopType shopType;

    public TextMeshProUGUI priceText;
    public TextMeshProUGUI infoText;
    public Button upgradeButton;

    private int charCount;

    private void OnEnable()
    {
        charCount = CharacterManager.Instance.characters.Count;
        Refresh();
    }

    public void Refresh()
    {
        GameData data = SaveManager.Load();

        upgradeButton.interactable = true; // 기본값은 켜짐

        switch (shopType)
        {
            case ShopType.Buy1:
                RefreshChildMax(data);
                break;

            case ShopType.Buy2:
                RefreshGetGoldTime(data);
                break;

            case ShopType.Buy3:
                RefreshClickMax(data);
                break;

            case ShopType.Buy4:
                RefreshSpawnTime(data);
                break;
        }
    }

    private void RefreshChildMax(GameData data)
    {
        int childMax = data.settings.childMax;
        int nextValue = childMax + 1;

        infoText.text = $"{childMax}개 ⇒ {nextValue}개";
        priceText.text = $"{data.shops.shop_1_price:N0} 돌멩이";
    }

    private void RefreshGetGoldTime(GameData data)
    {
        float t = data.settings.getGoldTime;
        float next = Mathf.Max(1.0f, t - 0.1f);

        infoText.text = $"{t:F1}초 ⇒ {next:F1}초";
        priceText.text = t <= 1.1f ? "MAX" : $"{data.shops.shop_2_price:N0} 돌멩이";

        if (t <= 1.1f)
        {
            infoText.text = "UPGRADE MAX";
            upgradeButton.interactable = false;
        }
    }

    private void RefreshClickMax(GameData data)
    {
        int click = data.settings.clickMax;
        infoText.text = $"{click}개 ⇒ {click + 1}개";
        priceText.text = $"{data.shops.shop_3_price:N0} 돌멩이";
    }

    private void RefreshSpawnTime(GameData data)
    {
        float t = data.settings.spawnTime;
        float next = Mathf.Max(1.0f, t - 0.1f);

        infoText.text = $"{t:F1}초 ⇒ {next:F1}초";
        priceText.text = t <= 1.1f ? "MAX" : $"{data.shops.shop_4_price:N0} 돌멩이";

        if (t <= 1.1f)
        {
            infoText.text = "UPGRADE MAX";
            upgradeButton.interactable = false;
        }
    }

    public static void RefreshAll()
    {
        foreach (var viewer in FindObjectsOfType<ShopItemInfoViewer>())
            viewer.Refresh();
    }
}