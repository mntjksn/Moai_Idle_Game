using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeSlot : MonoBehaviour
{
    public int index;

    public GameObject mainPanel;
    public GameObject endPanel;

    public TextMeshProUGUI nameText, goldText, buyText;
    public Image icon;
    public Button upgradeBtn;

    [SerializeField] private AudioSource audioSource;

    private Item item;

    private void Start()
    {
        item = CharacterManager.Instance?.GetItem(index);
        RefreshTexts();
        UpdateSlot();
    }

    private void Update()
    {
        if (item == null)
            item = CharacterManager.Instance?.GetItem(index);

        if (item == null) return;

        RefreshTexts();
        UpdateSlot();
    }

    private void RefreshTexts()
    {
        icon.sprite = item.itemimg;
        nameText.text = $"{index}. {item.name} 업그레이드";
        goldText.text = $"{item.itemgold:N0} 개 ⇒ {(item.itemgold * 2):N0} 개";
        buyText.text = $"{(index * 15 + (int)Mathf.Pow(index, 2)) + index:N0} 다이아";
    }

    public void UpdateSlot()
    {
        if (item == null) return;

        bool previousUnlocked = (index == 1) || CharacterManager.Instance.GetItem(index - 1).upgrade;
        bool spawned = item.spawncheck;

        // ------------------------------
        // ★ 첫 번째 캐릭터 전용 규칙
        // index == 1 AND spawncheck == true → 항상 열림
        // ------------------------------
        if (index == 1)
        {
            if (!spawned)
            {
                mainPanel.SetActive(false);
                endPanel.SetActive(false);
                return;
            }

            mainPanel.SetActive(true);

            if (item.upgrade)
            {
                endPanel.SetActive(true);
                upgradeBtn.interactable = false;
            }
            else
            {
                endPanel.SetActive(false);
                upgradeBtn.interactable = true;
            }
            return;
        }

        // ------------------------------
        // 2번째 이후 캐릭터
        // ------------------------------

        // 스폰 전이면 아무것도 안 보임
        if (!spawned)
        {
            mainPanel.SetActive(false);
            endPanel.SetActive(false);
            return;
        }

        // 이전 캐릭터가 업그레이드 안 되어 있으면 비활성화
        if (!previousUnlocked)
        {
            mainPanel.SetActive(false);
            endPanel.SetActive(false);
            return;
        }

        // 업그레이드 되었으면 endPanel 보이기
        if (item.upgrade)
        {
            mainPanel.SetActive(true);
            endPanel.SetActive(true);
            upgradeBtn.interactable = false;
            return;
        }

        // 기본: 구매 가능
        mainPanel.SetActive(true);
        endPanel.SetActive(false);
        upgradeBtn.interactable = true;
    }

    public void OnUpgrade()
    {
        GameData data = SaveManager.Load();
        if (item == null) return;

        int cost = (index * 15 + (int)Mathf.Pow(index, 2)) + index;

        if (data.currency.dia < cost || item.upgrade)
        {
            AppearTextManager.Instance.Show("다이아가 부족합니다!");
            return;
        }

            if (Setting.IsSFXOn())
            audioSource.Play();

        data.currency.dia -= cost;
        item.upgrade = true;
        data.upgrades.upgrade++;
        data.missions.mission_8_value++;

        SaveManager.Save(data);

        UpdateSlot();
        UpgradeManager.Instance.UpdateAllSlots();
    }
}