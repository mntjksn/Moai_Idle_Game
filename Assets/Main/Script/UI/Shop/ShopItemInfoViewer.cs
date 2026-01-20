using UnityEngine;
using TMPro;
using UnityEngine.UI;

// 상점 업그레이드 항목의 "가격/설명/버튼 활성화" UI를 갱신하는 스크립트
// shopType에 따라 각 항목(ChildMax, GoldTime, ClickMax, SpawnTime)을 표시함
public class ShopItemInfoViewer : MonoBehaviour
{
    // 상점 항목 타입(버튼/패널마다 지정)
    public enum ShopType { Buy1, Buy2, Buy3, Buy4 }
    public ShopType shopType;

    // 가격 텍스트
    public TextMeshProUGUI priceText;

    // 업그레이드 설명 텍스트 (예: 5개 => 6개)
    public TextMeshProUGUI infoText;

    // 업그레이드 버튼
    public Button upgradeButton;

    private void OnEnable()
    {
        // 패널 열릴 때 UI 한번 갱신
        Refresh();
    }

    // 해당 타입 상점 UI 갱신
    public void Refresh()
    {
        GameData data = SaveManager.Load();

        // 기본은 버튼 활성화
        // 각 타입에서 조건에 따라 비활성화 가능
        upgradeButton.interactable = true;

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

    // --------------------------------------------------
    // Buy1 : 최대 배치 수 증가(ChildMax)
    // --------------------------------------------------
    private void RefreshChildMax(GameData data)
    {
        int childMax = data.settings.childMax;
        int nextValue = childMax + 1;

        infoText.text = $"{childMax}개 ⇒ {nextValue}개";
        priceText.text = $"{data.shops.shop_1_price:N0} 돌멩이";
    }

    // --------------------------------------------------
    // Buy2 : 골드 획득 주기(getGoldTime) 감소
    // --------------------------------------------------
    private void RefreshGetGoldTime(GameData data)
    {
        float t = data.settings.getGoldTime;
        float next = Mathf.Max(1.0f, t - 0.1f);

        infoText.text = $"{t:F1}초 ⇒ {next:F1}초";

        // 최저값(1.1초) 이하로 내려가면 MAX 표시
        priceText.text = t <= 1.1f ? "MAX" : $"{data.shops.shop_2_price:N0} 돌멩이";

        // MAX 도달 시 버튼 비활성화
        if (t <= 1.1f)
        {
            infoText.text = "UPGRADE MAX";
            upgradeButton.interactable = false;
        }
    }

    // --------------------------------------------------
    // Buy3 : 소환 가능 최대 횟수(clickMax) 증가
    // --------------------------------------------------
    private void RefreshClickMax(GameData data)
    {
        int click = data.settings.clickMax;

        infoText.text = $"{click}개 ⇒ {click + 1}개";
        priceText.text = $"{data.shops.shop_3_price:N0} 돌멩이";
    }

    // --------------------------------------------------
    // Buy4 : 소환 쿨타임(spawnTime) 감소
    // --------------------------------------------------
    private void RefreshSpawnTime(GameData data)
    {
        float t = data.settings.spawnTime;
        float next = Mathf.Max(1.0f, t - 0.1f);

        infoText.text = $"{t:F1}초 ⇒ {next:F1}초";
        priceText.text = t <= 1.1f ? "MAX" : $"{data.shops.shop_4_price:N0} 돌멩이";

        // MAX 도달 시 버튼 비활성화
        if (t <= 1.1f)
        {
            infoText.text = "UPGRADE MAX";
            upgradeButton.interactable = false;
        }
    }

    // 씬에 존재하는 모든 ShopItemInfoViewer를 갱신
    public static void RefreshAll()
    {
        foreach (var viewer in FindObjectsOfType<ShopItemInfoViewer>())
            viewer.Refresh();
    }
}