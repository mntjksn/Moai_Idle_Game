using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 게임 설정 UI를 담당
// - BGM / SFX / MOVE / TEXT On-Off를 PlayerPrefs로 저장
// - 패널이 열릴 때 현재 상태를 불러와 UI만 갱신
public class Setting : MonoBehaviour
{
    // PlayerPrefs 키(설정 저장용)
    private const string PREF_BGM = "BGMOnOff";
    private const string PREF_SFX = "SFXOnOff";
    private const string PREF_MOVE = "MOVEOnOff";
    private const string PREF_TEXT = "TEXTOnOff";

    // 현재 설정 상태(패널 내부에서만 사용하는 캐시)
    private bool bgmOn;
    private bool sfxOn;
    private bool moveOn;
    private bool textOn;

    [Header("UI")]
    [SerializeField] private Button bgmButton;
    [SerializeField] private Button sfxButton;
    [SerializeField] private Button moveButton;
    [SerializeField] private Button textButton;

    [SerializeField] private TextMeshProUGUI bgmText;
    [SerializeField] private TextMeshProUGUI sfxText;
    [SerializeField] private TextMeshProUGUI moveText;
    [SerializeField] private TextMeshProUGUI textText;

    private void Start()
    {
        // 버튼 리스너는 Start에서 1회만 등록
        // (OnEnable에서 등록하면 패널을 열 때마다 중복 등록될 수 있음)
        if (bgmButton != null)
            bgmButton.onClick.AddListener(() => Toggle(ref bgmOn, PREF_BGM, UpdateUI_BGM));

        if (sfxButton != null)
            sfxButton.onClick.AddListener(() => Toggle(ref sfxOn, PREF_SFX, UpdateUI_SFX));

        if (moveButton != null)
            moveButton.onClick.AddListener(() => Toggle(ref moveOn, PREF_MOVE, UpdateUI_MOVE));

        if (textButton != null)
            textButton.onClick.AddListener(() => Toggle(ref textOn, PREF_TEXT, UpdateUI_TEXT));
    }

    private void OnEnable()
    {
        // 패널이 열릴 때 저장된 설정을 로드
        bgmOn = PlayerPrefs.GetInt(PREF_BGM, 1) == 1;
        sfxOn = PlayerPrefs.GetInt(PREF_SFX, 1) == 1;
        moveOn = PlayerPrefs.GetInt(PREF_MOVE, 1) == 1;
        textOn = PlayerPrefs.GetInt(PREF_TEXT, 1) == 1;

        // UI만 갱신
        UpdateUI();
    }

    // 공통 토글 처리
    // - flag 뒤집기
    // - PlayerPrefs 저장
    // - 해당 UI 갱신 호출
    private void Toggle(ref bool flag, string pref, System.Action updateUI)
    {
        flag = !flag;

        PlayerPrefs.SetInt(pref, flag ? 1 : 0);
        PlayerPrefs.Save();

        updateUI?.Invoke();
    }

    // 전체 UI 갱신
    private void UpdateUI()
    {
        UpdateUI_BGM();
        UpdateUI_SFX();
        UpdateUI_MOVE();
        UpdateUI_TEXT();
    }

    // BGM UI 갱신 + 실제 BGM 적용
    private void UpdateUI_BGM()
    {
        if (bgmText != null)
            bgmText.text = bgmOn ? "BGM: ON" : "BGM: OFF";

        // 실제 BGM은 BGMBootstrap에서 관리
        BGMBootstrap.SetBGM(bgmOn);
    }

    // SFX UI 갱신
    private void UpdateUI_SFX()
    {
        if (sfxText != null)
            sfxText.text = sfxOn ? "SFX: ON" : "SFX: OFF";
    }

    // MOVE UI 갱신
    private void UpdateUI_MOVE()
    {
        if (moveText != null)
            moveText.text = moveOn ? "MOVE: ON" : "MOVE: OFF";
    }

    // TEXT UI 갱신
    private void UpdateUI_TEXT()
    {
        if (textText != null)
            textText.text = textOn ? "TEXT: ON" : "TEXT: OFF";
    }

    // 다른 스크립트에서 설정 값을 쉽게 확인하기 위한 정적 함수
    // PlayerPrefs를 직접 읽으므로 항상 최신 값을 가져옴
    public static bool IsSFXOn() => PlayerPrefs.GetInt(PREF_SFX, 1) == 1;
    public static bool IsMOVEOn() => PlayerPrefs.GetInt(PREF_MOVE, 1) == 1;
    public static bool IsTEXTOn() => PlayerPrefs.GetInt(PREF_TEXT, 1) == 1;
}