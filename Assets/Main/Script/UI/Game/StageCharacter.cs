using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StageCharacter : MonoBehaviour, IPointerClickHandler
{
    public Slider hpBar;
    public Image image;
    public TextMeshProUGUI subText, butText, dmgText, nameText, hpText, giftText;
    public GameObject game_panel, gift, gift_panel, main_panel, lock_panel;
    public Button Button;

    private Color originalColor;
    private Color hitColor;
    private float hitDuration = 0.25f;
    private float hitTimer = 0f;
    private bool isHit = false;

    private GameData data;
    private Item character;

    private int lastStageLevel = -1;   // UI 변경 감지용

    private bool rewardPending = false;

    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        Input.multiTouchEnabled = false;

        // 초기 1회 로드
        data = SaveManager.Load();

        originalColor = image.color;
        hitColor = new Color(1f, 0f, 0f, originalColor.a);
    }

    private void OnEnable()
    {
        data = SaveManager.Load();
        lastStageLevel = data.clickclick.stageLevel;
        character = CharacterManager.Instance.GetItem(lastStageLevel);

        UpdateStaticUI();
        UpdateLockPanel();   // ★ 패널 열릴 때 항상 최신 적용!
    }

    private void Update()
    {
        // 피해 애니메이션 처리
        HandleHitFlash();

        // HP바만 실시간 갱신 (변화가 자주 있음)
        UpdateHPBar();

        // stageLevel이 바뀌었을 때만 UI 정보 다시 로드
        if (lastStageLevel != data.clickclick.stageLevel)
        {
            lastStageLevel = data.clickclick.stageLevel;
            character = CharacterManager.Instance.GetItem(lastStageLevel);

            UpdateStaticUI();
            UpdateLockPanel();
        }
    }

    private void UpdateStaticUI()
    {
        if (character == null) return;

        nameText.text = character.name;
        image.sprite = character.itemimg;

        subText.text = $"모아이 체력 : {data.clickclick.maxHp:N0}\n클릭 공격력 : {data.clickclick.damage:N0}";
        dmgText.text = $"공격력 업그레이드 + {data.clickclick.damage_check / 5 + 1:N0}";
        butText.text = $"{data.clickclick.damage_upgrade:N0} 돌멩이";
    }

    private void UpdateHPBar()
    {
        hpBar.maxValue = data.clickclick.maxHp;
        hpBar.value = data.clickclick.hp;
        hpText.text = $"{data.clickclick.hp:N0} / {data.clickclick.maxHp:N0}";
    }

    private void UpdateLockPanel()
    {
        // 조건을 만족하면 해금 상태
        if ((character.upgrade && character.spawncheck) || data.clickclick.stageLevel == 0)
        {
            main_panel.SetActive(true);
            lock_panel.SetActive(false);

            // 이름 정상 표시
            nameText.text = character.name;
        }
        else
        {
            // 잠금 상태
            main_panel.SetActive(false);
            lock_panel.SetActive(true);

            // 이름 ??? 표시
            nameText.text = "???";
        }
    }

    // ============================================
    // 클릭 처리
    // ============================================
    public void OnPointerClick(PointerEventData eventData)
    {
        if (rewardPending)
            return;

        data = SaveManager.Load();

        int lastIndex = CharacterManager.Instance.GetCount() - 1;

        if (main_panel.activeSelf)
        {
            data.clickclick.hp -= data.clickclick.damage;
            HitFlash();
        }

        // ===== ★ 마지막 스테이지 처리 (보상은 주되 stageLevel 증가 없음) =====
        if (data.clickclick.stageLevel >= lastIndex)
        {
            if (data.clickclick.hp < 1)
            {
                rewardPending = true;

                if (Setting.IsSFXOn()) audioSource.Play();

                gift.SetActive(true);
                Button.interactable = true;
            }

            SaveManager.Save(data);
            return;
        }
        // ===============================================================


        // ★ 평상시 스테이지 업 처리
        if (data.clickclick.hp < 1)
        {
            rewardPending = true;

            if (Setting.IsSFXOn()) audioSource.Play();

            gift.SetActive(true);
            Button.interactable = true;

            data.clickclick.stageLevel++;
            int max = data.clickclick.maxHp;
            data.clickclick.maxHp += (int)(max * 0.27f);
            data.clickclick.hp = data.clickclick.maxHp;
        }

        SaveManager.Save(data);
    }

    private void HandleHitFlash()
    {
        if (!isHit) return;

        hitTimer += Time.deltaTime;
        float t = hitTimer / hitDuration;
        image.color = Color.Lerp(hitColor, originalColor, t);

        if (t >= 1f)
        {
            isHit = false;
            image.color = originalColor;
        }
    }

    private void HitFlash()
    {
        isHit = true;
        hitTimer = 0f;
        image.color = hitColor;
    }

    // ============================================
    // 보상 처리
    // ============================================
    public void GiveReward()
    {
        rewardPending = false;

        data = SaveManager.Load();

        Input.multiTouchEnabled = false;
        Button.interactable = false;

        gift_panel.SetActive(true);
        Invoke(nameof(HidePanel), 1f);

        giftText.text =
            $"돌멩이 {data.clickclick.rewardGold:N0}개 !!\n" +
            $"다이아 {data.clickclick.rewardDia:N0}개 !!\n" +
            $"티켓 {data.clickclick.rewardTicket:N0}개 !!";

        data.currency.gold += data.clickclick.rewardGold;
        data.currency.dia += data.clickclick.rewardDia;
        data.currency.ticket += data.clickclick.rewardTicket;

        data.clickclick.rewardGold += 500 * data.clickclick.stageLevel;
        data.clickclick.rewardDia += 5 + ((data.clickclick.stageLevel / 5 + 1) * 5);
        data.clickclick.rewardTicket = (data.clickclick.stageLevel / 3 + 1);

        if (data.currency.gold > 2147483600)
            data.currency.gold = 2147483600;

        SaveManager.Save(data);

        Invoke(nameof(HideGift), 1.5f);

        // ★ 마지막 단계면 자동 패널 닫기
        int lastIndex = CharacterManager.Instance.GetCount() - 1;
        if (data.clickclick.stageLevel >= lastIndex)
        {
            // 잠금 상태
            main_panel.SetActive(false);
            lock_panel.SetActive(true);

            // 이름 ??? 표시
            nameText.text = "???";
        }
    }

    private void HidePanel() => gift_panel.SetActive(false);
    private void HideGift() => gift.SetActive(false);

    public void damageup()
    {
        data = SaveManager.Load();

        if (data.currency.gold > data.clickclick.damage_upgrade)
        {
            data.currency.gold -= data.clickclick.damage_upgrade;
            data.clickclick.damage += data.clickclick.damage_check / 5 + 1;
            data.clickclick.damage_upgrade += (data.clickclick.damage_check / 10 * 10) + 50;
            data.clickclick.damage_check++;
        }

        else
        {
            AppearTextManager.Instance.Show("돌멩이가 부족합니다!");
        }

        SaveManager.Save(data);
        UpdateStaticUI(); // 즉시 UI 반영
    }

    public void close()
    {
        game_panel.SetActive(false);
    }
}