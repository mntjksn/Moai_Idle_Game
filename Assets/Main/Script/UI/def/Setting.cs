using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Setting : MonoBehaviour
{
    private const string PREF_BGM = "BGMOnOff";
    private const string PREF_SFX = "SFXOnOff";
    private const string PREF_MOVE = "MOVEOnOff";
    private const string PREF_TEXT = "TEXTOnOff";

    private bool bgmOn;
    private bool sfxOn;
    private bool moveOn;
    private bool textOn;

    [Header("UI")]
    [SerializeField] private AudioSource bgmSource;
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
        // ★★★ Listener는 Start에서 단 1번만 등록해야 함 ★★★
        bgmButton.onClick.AddListener(() => Toggle(ref bgmOn, PREF_BGM, UpdateUI_BGM));
        sfxButton.onClick.AddListener(() => Toggle(ref sfxOn, PREF_SFX, UpdateUI_SFX));
        moveButton.onClick.AddListener(() => Toggle(ref moveOn, PREF_MOVE, UpdateUI_MOVE));
        textButton.onClick.AddListener(() => Toggle(ref textOn, PREF_TEXT, UpdateUI_TEXT));
    }

    private void OnEnable()
    {
        // 상태 로드
        bgmOn = PlayerPrefs.GetInt(PREF_BGM, 1) == 1;
        sfxOn = PlayerPrefs.GetInt(PREF_SFX, 1) == 1;
        moveOn = PlayerPrefs.GetInt(PREF_MOVE, 1) == 1;
        textOn = PlayerPrefs.GetInt(PREF_TEXT, 1) == 1;

        // UI 갱신만
        UpdateUI();
    }

    private void Toggle(ref bool flag, string pref, System.Action updateUI)
    {
        flag = !flag;
        PlayerPrefs.SetInt(pref, flag ? 1 : 0);
        PlayerPrefs.Save();
        updateUI();
    }

    //=====================================================

    private void UpdateUI()
    {
        UpdateUI_BGM();
        UpdateUI_SFX();
        UpdateUI_MOVE();
        UpdateUI_TEXT();
    }

    private void UpdateUI_BGM()
    {
        bgmText.text = bgmOn ? "BGM: ON" : "BGM: OFF";

        BGMBootstrap.SetBGM(bgmOn);
    }

    private void UpdateUI_SFX()
    {
        sfxText.text = sfxOn ? "SFX: ON" : "SFX: OFF";
    }

    private void UpdateUI_MOVE()
    {
        moveText.text = moveOn ? "MOVE: ON" : "MOVE: OFF";
    }

    private void UpdateUI_TEXT()
    {
        textText.text = textOn ? "TEXT: ON" : "TEXT: OFF";
    }

    public static bool IsSFXOn() => PlayerPrefs.GetInt(PREF_SFX, 1) == 1;
    public static bool IsMOVEOn() => PlayerPrefs.GetInt(PREF_MOVE, 1) == 1;
    public static bool IsTEXTOn() => PlayerPrefs.GetInt(PREF_TEXT, 1) == 1;
}