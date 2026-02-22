using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 스테이지(클릭 전투) UI와 로직을 관리
// - 클릭 시 HP 감소
// - HP 0이면 보상 패널 표시 + 다음 스테이지 진입(마지막 스테이지는 예외)
// - 공격력 업그레이드 처리
public class StageCharacter : MonoBehaviour, IPointerClickHandler
{
    [Header("UI")]
    public Slider hpBar; // HP 바
    public Image image;  // 몬스터(모아이) 이미지

    public TextMeshProUGUI subText;   // 스탯 안내 텍스트
    public TextMeshProUGUI butText;   // 업그레이드 가격 텍스트
    public TextMeshProUGUI dmgText;   // 업그레이드 증가량 표시
    public TextMeshProUGUI nameText;  // 이름 표시
    public TextMeshProUGUI hpText;    // HP 숫자 표시
    public TextMeshProUGUI giftText;  // 보상 표시

    public GameObject game_panel;   // 스테이지 전체 패널
    public GameObject gift;         // 선물 버튼/아이콘
    public GameObject gift_panel;   // 보상 텍스트 패널
    public GameObject main_panel;   // 실제 전투 UI 패널(해금 상태)
    public GameObject lock_panel;   // 잠금 상태 패널

    public Button Button; // 보상 받기 버튼

    [Header("Hit Flash")]
    private Color originalColor;      // 원래 색
    private Color hitColor;           // 맞았을 때 색
    private float hitDuration = 0.25f; // 플래시 지속 시간
    private float hitTimer = 0f;
    private bool isHit = false;

    [Header("Hit Pop FX")]
    [SerializeField] private bool useHitPopFx = true;
    [SerializeField] private float hitPopDuration = 0.12f;
    [SerializeField] private float hitPopStartScale = 0.85f;
    private Coroutine hitPopCo;
    private bool hitPopPlaying = false;

    // 세이브 데이터 / 현재 스테이지 캐릭터 데이터
    private GameData data;
    private CharacterItem character;

    // stageLevel 변경 감지용
    private int lastStageLevel = -1;

    // 보상 패널이 떠 있는 동안 중복 클릭 방지
    private bool rewardPending = false;

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource1; // 업그레이드/클릭 등
    [SerializeField] private AudioSource audioSource2; // 보상 등장

    private void Awake()
    {
        // 멀티 터치 방지(중복 클릭 방지 목적)
        Input.multiTouchEnabled = false;

        // 초기 데이터 로드
        data = SaveManager.Load();

        // 피격 플래시 색상 초기화
        if (image != null)
        {
            originalColor = image.color;
            hitColor = new Color(1f, 0f, 0f, originalColor.a);
        }
    }

    private void OnEnable()
    {
        // 패널 켜질 때 최신 데이터 갱신
        data = SaveManager.Load();

        lastStageLevel = data.clickclick.stageLevel;

        // 스테이지 레벨에 해당하는 캐릭터 데이터 로드
        if (CharacterManager.Instance != null)
            character = CharacterManager.Instance.GetItem(lastStageLevel);

        // 고정 UI 갱신(이름/이미지/공격력/가격 등)
        UpdateStaticUI();

        // 잠금 패널 갱신
        UpdateLockPanel();
    }

    private void Update()
    {
        // 피격 플래시 애니메이션 처리
        HandleHitFlash();

        // HP바는 수시로 바뀌므로 매 프레임 갱신
        UpdateHPBar();

        // stageLevel이 변경되면 UI와 대상 캐릭터를 다시 로드
        // (주의: data.clickclick.stageLevel이 다른 곳에서 바뀔 수 있으므로 필요)
        if (lastStageLevel != data.clickclick.stageLevel)
        {
            lastStageLevel = data.clickclick.stageLevel;

            if (CharacterManager.Instance != null)
                character = CharacterManager.Instance.GetItem(lastStageLevel);

            UpdateStaticUI();
            UpdateLockPanel();
        }
    }

    // 이름/이미지/공격력/가격 등 "자주 바뀌지 않는" UI 갱신
    private void UpdateStaticUI()
    {
        if (character == null)
            return;

        if (nameText != null)
            nameText.text = character.name;

        if (image != null)
            image.sprite = character.itemimg;

        if (subText != null)
            subText.text =
                "모아이 체력 : " + data.clickclick.maxHp.ToString("N0") + "\n" +
                "클릭 공격력 : " + data.clickclick.damage.ToString("N0");

        if (dmgText != null)
            dmgText.text = "공격력 업그레이드 + " + (data.clickclick.damage_check / 5 + 1).ToString("N0");

        if (butText != null)
            butText.text = data.clickclick.damage_upgrade.ToString("N0") + " 돌멩이";
    }

    // HP 바/HP 텍스트 갱신(변화가 잦음)
    private void UpdateHPBar()
    {
        if (hpBar != null)
        {
            hpBar.maxValue = data.clickclick.maxHp;
            hpBar.value = data.clickclick.hp;
        }

        if (hpText != null)
            hpText.text = data.clickclick.hp.ToString("N0") + " / " + data.clickclick.maxHp.ToString("N0");
    }

    // 현재 스테이지가 해금 상태인지(업그레이드 + 도감 해금 조건) 확인 후 패널 표시 전환
    private void UpdateLockPanel()
    {
        if (character == null)
            return;

        // 조건 만족하면 해금 상태
        if ((character.upgrade && character.spawncheck) || data.clickclick.stageLevel == 0)
        {
            if (main_panel != null) main_panel.SetActive(true);
            if (lock_panel != null) lock_panel.SetActive(false);

            if (nameText != null)
                nameText.text = character.name;
        }
        else
        {
            // 잠금 상태
            if (main_panel != null) main_panel.SetActive(false);
            if (lock_panel != null) lock_panel.SetActive(true);

            if (nameText != null)
                nameText.text = "???";
        }
    }

    // =====================================================
    // 클릭 처리(전투)
    // =====================================================
    public void OnPointerClick(PointerEventData eventData)
    {
        // 보상 대기 중이면 클릭 무시
        if (rewardPending)
            return;

        // 최신 데이터 로드
        data = SaveManager.Load();

        // 마지막 스테이지 인덱스
        int lastIndex = (CharacterManager.Instance != null)
            ? CharacterManager.Instance.GetCount() - 1
            : 0;

        // 해금 상태일 때만 공격 가능
        if (main_panel != null && main_panel.activeSelf)
        {
            data.clickclick.hp -= data.clickclick.damage;
            HitFlash();
        }

        // ---------------------------------------------------
        // 마지막 스테이지 처리
        // - 보상은 주되 stageLevel 증가 없음
        // ---------------------------------------------------
        if (data.clickclick.stageLevel >= lastIndex)
        {
            if (data.clickclick.hp < 1)
            {
                rewardPending = true;

                if (audioSource2 != null && Setting.IsSFXOn())
                    audioSource2.Play();

                if (gift != null) gift.SetActive(true);
                if (Button != null) Button.interactable = true;
            }

            SaveManager.Save(data);
            return;
        }

        // ---------------------------------------------------
        // 일반 스테이지 처리
        // - HP 0이면 보상 대기 + 다음 스테이지 진입
        // ---------------------------------------------------
        if (data.clickclick.hp < 1)
        {
            rewardPending = true;

            if (audioSource2 != null && Setting.IsSFXOn())
                audioSource2.Play();

            if (gift != null) gift.SetActive(true);
            if (Button != null) Button.interactable = true;

            // 스테이지 증가
            data.clickclick.stageLevel++;

            // 다음 스테이지 HP 증가 및 회복
            int max = data.clickclick.maxHp;
            data.clickclick.maxHp += (int)(max * 0.27f);
            data.clickclick.hp = data.clickclick.maxHp;
        }

        SaveManager.Save(data);
    }

    // 피격 플래시 애니메이션 진행
    private void HandleHitFlash()
    {
        if (!isHit || image == null)
            return;

        hitTimer += Time.deltaTime;
        float t = hitTimer / hitDuration;

        image.color = Color.Lerp(hitColor, originalColor, t);

        if (t >= 1f)
        {
            isHit = false;
            image.color = originalColor;
        }
    }

    // 피격 플래시 시작
    private void HitFlash()
    {
        if (image == null) return;

        isHit = true;
        hitTimer = 0f;
        image.color = hitColor;

        // 피격 팝 연출 (중복 방지)
        if (useHitPopFx && !hitPopPlaying)
        {
            if (hitPopCo != null) StopCoroutine(hitPopCo);
            hitPopCo = StartCoroutine(HitPopFx(image.transform));
        }
    }

    // 피격 시 스케일 팝 연출
    private IEnumerator HitPopFx(Transform t)
    {
        if (t == null) yield break;

        hitPopPlaying = true;

        Vector3 original = t.localScale; // "진짜 원래 스케일" 고정
        Vector3 start = original * Mathf.Clamp(hitPopStartScale, 0.01f, 1f);

        t.localScale = start;

        float dur = Mathf.Max(0.01f, hitPopDuration);
        float time = 0f;

        while (time < dur)
        {
            if (t == null) { hitPopPlaying = false; yield break; }

            time += Time.deltaTime;
            float x = Mathf.Clamp01(time / dur);

            float eased =
                1f +
                1.70158f * Mathf.Pow(x - 1f, 3f) +
                1.70158f * Mathf.Pow(x - 1f, 2f);

            t.localScale = Vector3.LerpUnclamped(start, original, eased);
            yield return null;
        }

        t.localScale = original;
        hitPopPlaying = false;
        hitPopCo = null;
    }

    // =====================================================
    // 보상 처리
    // =====================================================
    public void GiveReward()
    {
        rewardPending = false;

        data = SaveManager.Load();

        // 보상 받는 동안 조작 방지
        Input.multiTouchEnabled = false;

        if (Button != null)
            Button.interactable = false;

        // 보상 패널 표시(잠시 뒤 닫기)
        if (gift_panel != null)
        {
            gift_panel.SetActive(true);
            Invoke(nameof(HidePanel), 1f);
        }

        // 보상 안내 텍스트 표시
        if (giftText != null)
        {
            giftText.text =
                "돌멩이 " + data.clickclick.rewardGold.ToString("N0") + "개 !!\n" +
                "다이아 " + data.clickclick.rewardDia.ToString("N0") + "개 !!\n" +
                "티켓 " + data.clickclick.rewardTicket.ToString("N0") + "개 !!";
        }

        // 실제 보상 지급
        data.currency.gold += data.clickclick.rewardGold;
        data.currency.dia += data.clickclick.rewardDia;
        data.currency.ticket += data.clickclick.rewardTicket;

        // 다음 보상 수치 갱신
        data.clickclick.rewardGold += 500 * data.clickclick.stageLevel;
        data.clickclick.rewardDia += 5 + ((data.clickclick.stageLevel / 5 + 1) * 5);
        data.clickclick.rewardTicket = (data.clickclick.stageLevel / 3 + 1);

        // 골드 상한 방지
        if (data.currency.gold > 2147483600)
            data.currency.gold = 2147483600;

        SaveManager.Save(data);

        // 선물 아이콘 숨김(조금 늦게)
        Invoke(nameof(HideGift), 1.5f);

        // 마지막 스테이지면 강제로 잠금 패널 형태로 닫기
        int lastIndex = (CharacterManager.Instance != null)
            ? CharacterManager.Instance.GetCount() - 1
            : 0;

        if (data.clickclick.stageLevel >= lastIndex)
        {
            if (main_panel != null) main_panel.SetActive(false);
            if (lock_panel != null) lock_panel.SetActive(true);

            if (nameText != null)
                nameText.text = "???";
        }
    }

    private void HidePanel()
    {
        if (gift_panel != null)
            gift_panel.SetActive(false);
    }

    private void HideGift()
    {
        if (gift != null)
            gift.SetActive(false);
    }

    // =====================================================
    // 공격력 업그레이드
    // =====================================================
    public void damageup()
    {
        data = SaveManager.Load();

        // 골드가 충분할 때만 업그레이드
        if (data.currency.gold > data.clickclick.damage_upgrade)
        {
            if (audioSource1 != null && Setting.IsSFXOn())
                audioSource1.Play();

            data.currency.gold -= data.clickclick.damage_upgrade;

            // 공격력 증가량 규칙 유지
            data.clickclick.damage += data.clickclick.damage_check / 5 + 1;

            // 가격 증가 규칙 유지
            data.clickclick.damage_upgrade += (data.clickclick.damage_check / 10 * 10) + 50;
            data.clickclick.damage_check++;
        }
        else
        {
            // 부족 메시지 표시
            if (AppearTextManager.Instance != null)
                AppearTextManager.Instance.Show("돌멩이가 부족합니다!");
        }

        SaveManager.Save(data);

        // 즉시 UI 반영
        UpdateStaticUI();
    }

    // 패널 닫기 버튼용
    public void close()
    {
        if (game_panel != null)
            game_panel.SetActive(false);
    }
}