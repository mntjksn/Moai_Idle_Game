using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 스폰 버튼의 사용 가능 횟수(clickNum)와 쿨타임 충전을 관리
public class ClickLimit : MonoBehaviour
{
    [Header("Refs")]
    public Button btn;          // 스폰 버튼
    public Image image;         // 쿨타임 게이지(fillAmount)
    public Image image_alpha;   // 표시용 이미지(반투명 등)
    public Merge mg;            // 캐릭터 생성 담당

    // 쿨타임 진행값
    private float timer = 0f;
    private bool isCooling = false;

    // 배치된 캐릭터의 부모 오브젝트
    private Transform chp;

    // 세이브 데이터에서 가져오는 값 캐시
    private int upch;           // 현재 생성 레벨(업그레이드 카운트 기반)
    private int childMax;       // 최대 배치 수
    private int clickMax;       // 최대 스폰 횟수(게이지 최대치)
    private float spawnTime;    // 스폰 횟수 1개 충전까지 걸리는 시간

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI spawnText; // (현재/최대) 표시
    [SerializeField] private AudioSource audioSource;   // 효과음

    private Coroutine buttonRoutine;

    private void Start()
    {
        // 세이브 데이터에서 설정값 갱신
        RefreshSettingsFromData();

        // 오브젝트 참조 찾기
        chp = GameObject.FindGameObjectWithTag("chp")?.transform;

        if (mg == null)
            mg = GameObject.Find("ItemData")?.GetComponent<Merge>();

        // 필수 참조가 없으면 이후 로직이 터질 수 있으므로 중단
        if (btn == null || image == null || image_alpha == null || chp == null || mg == null)
        {
            Debug.LogError("[ClickLimit] 필수 참조 누락 (btn/image/image_alpha/chp/mg 확인)");
            return;
        }

        // spawnTime이 0이면 분모 0 문제가 생길 수 있어 최소값 보정
        if (spawnTime <= 0f)
            spawnTime = 0.01f;

        // 캐릭터 이미지 갱신
        UpdateCharacterImage();

        // 버튼 클릭 이벤트 연결
        btn.onClick.AddListener(OnClickButton);

        // 시작 시 텍스트 1회 갱신
        RefreshSpawnText();

        // 버튼 상태 / 쿨타임 시작 조건을 주기적으로 검사
        buttonRoutine = StartCoroutine(UpdateButtonRoutine());
    }

    private void OnEnable()
    {
        // 설정 변경 이벤트 구독
        ShopButton.GameEvents.OnSettingsChanged += OnSettingsChanged;
    }

    private void OnDisable()
    {
        // 설정 변경 이벤트 해제
        ShopButton.GameEvents.OnSettingsChanged -= OnSettingsChanged;
    }

    private void OnSettingsChanged()
    {
        // 설정 변경 시 캐시 및 UI 갱신
        RefreshSettingsFromData();

        if (spawnTime <= 0f)
            spawnTime = 0.01f;

        RefreshSpawnText();
        UpdateCharacterImage();
    }

    // 세이브 데이터에서 필요한 값들 읽어서 캐시에 저장
    private void RefreshSettingsFromData()
    {
        GameData data = SaveManager.Load();

        upch = data.upgrades.count;
        childMax = data.settings.childMax;
        clickMax = data.settings.clickMax;
        spawnTime = data.settings.spawnTime;
    }

    private void Update()
    {
        // 쿨타임 중이 아니면 처리하지 않음
        if (!isCooling)
            return;

        if (image == null)
            return;

        if (spawnTime <= 0f)
            spawnTime = 0.01f;

        timer += Time.deltaTime;

        // 쿨타임 게이지 감소(남은 비율)
        image.fillAmount = 1f - (timer / spawnTime);

        // 쿨타임 종료 시 1회 충전
        if (timer >= spawnTime)
        {
            timer = 0f;
            isCooling = false;
            image.fillAmount = 1f;

            GameData data = SaveManager.Load();

            if (data.settings.clickNum < clickMax)
            {
                data.settings.clickNum++;
                SaveManager.Save(data);

                // UI 갱신
                RefreshSpawnText();

                // 다른 UI 갱신
                ShopButton.GameEvents.OnSettingsChanged?.Invoke();
            }
        }
    }

    private IEnumerator UpdateButtonRoutine()
    {
        WaitForSeconds delay = new WaitForSeconds(0.1f);

        while (true)
        {
            // 참조가 없으면 루프 종료
            if (btn == null || chp == null)
                yield break;

            GameData data = SaveManager.Load();

            int active = GetActiveChildCount();

            // 버튼 클릭 가능 조건
            // - 남은 스폰 횟수가 있고
            // - 현재 배치 수가 최대치 미만일 때
            btn.interactable = (data.settings.clickNum > 0 && active < childMax);

            // 쿨타임 시작 조건
            // - 쿨타임이 아니고
            // - 스폰 횟수가 최대치보다 작으면 충전 시작(배치가 꽉 차도 충전은 진행)
            if (!isCooling &&
                data.settings.clickNum < clickMax)
            {
                isCooling = true;
                timer = 0f;

                if (image != null)
                    image.fillAmount = 1f;
            }

            yield return delay;
        }
    }

    private void OnClickButton()
    {
        if (mg == null || btn == null)
            return;

        GameData data = SaveManager.Load();

        // 남은 스폰 횟수가 없으면 무시
        if (data.settings.clickNum <= 0)
            return;

        // 캐릭터 생성
        mg.itemCreate(upch);

        // 효과음 재생
        if (audioSource != null && Setting.IsSFXOn())
            audioSource.Play();

        // 배경 체크 값 증가(상한 유지)
        if (data.background.spawn_check <= 50000)
            data.background.spawn_check++;

        // 미션 진행도 증가
        data.missions.mission_3_value += 1;

        // 남은 횟수 감소
        data.settings.clickNum = Mathf.Max(0, data.settings.clickNum - 1);

        SaveManager.Save(data);

        // UI 즉시 갱신
        RefreshSpawnText();

        // 다른 UI 갱신
        ShopButton.GameEvents.OnSettingsChanged?.Invoke();

        // 캐시 최신화
        RefreshSettingsFromData();

        if (spawnTime <= 0f)
            spawnTime = 0.01f;

        // 이미지 갱신
        UpdateCharacterImage();
    }

    private void RefreshSpawnText()
    {
        if (spawnText == null)
            return;

        GameData data = SaveManager.Load();

        int num = data.settings.clickNum;
        int max = data.settings.clickMax;

        spawnText.text = "(" + num + " / " + max + ")";
    }

    private void UpdateCharacterImage()
    {
        if (image == null || image_alpha == null)
            return;

        var cm = CharacterManager.Instance;
        if (cm == null)
            return;

        // 로드 완료 전이면 아직 데이터가 비어있을 수 있음
        if (!cm.IsLoaded)
            return;

        var character = cm.GetItem(upch);
        if (character == null || character.itemimg == null)
            return;

        image.sprite = character.itemimg;
        image_alpha.sprite = character.itemimg;
    }

    // 현재 활성(배치된) 캐릭터 수 계산
    private int GetActiveChildCount()
    {
        if (chp == null)
            return 0;

        int count = 0;
        int max = chp.childCount;

        for (int i = 0; i < max; i++)
        {
            if (chp.GetChild(i).gameObject.activeSelf)
                count++;
        }

        return count;
    }

    // 외부에서 캐릭터 이미지 갱신 요청
    public void RefreshCharacterImage()
    {
        RefreshSettingsFromData();

        if (spawnTime <= 0f)
            spawnTime = 0.01f;

        UpdateCharacterImage();
    }

    // 현재 스폰 가능 여부
    public bool IsSpawnable()
    {
        return btn != null && btn.interactable;
    }
}