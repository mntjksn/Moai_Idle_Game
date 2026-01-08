using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ShopButton;

public class ClickLimit : MonoBehaviour
{
    public Button btn;
    public Image image;
    public Image image_alpha;
    public Merge mg;

    private float timer = 0f;
    private bool isCooling = false;

    private Transform chp;

    private int upch;
    private int childMax;
    private int clickMax;
    private float spawnTime;

    private Camera mainCam;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI spawnText;   // ← 여기! (Spawntext 대신)
    [SerializeField] private AudioSource audioSource;

    private void Start()
    {
        RefreshSettingsFromData();

        chp = GameObject.Find("chp")?.transform;
        mg = GameObject.Find("ItemData")?.GetComponent<Merge>();
        mainCam = Camera.main;

        UpdateCharacterImage();

        btn.onClick.AddListener(OnClickButton);

        // 시작할 때 한 번 텍스트 갱신
        RefreshSpawnText();

        StartCoroutine(UpdateButtonRoutine());
    }

    private void OnEnable()
    {
        GameEvents.OnSettingsChanged += OnSettingsChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnSettingsChanged -= OnSettingsChanged;
    }

    private void OnSettingsChanged()
    {
        RefreshSettingsFromData();
        RefreshSpawnText();
        UpdateCharacterImage();
    }

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
        if (!isCooling) return;

        timer += Time.deltaTime;
        image.fillAmount = 1f - (timer / spawnTime);

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

                RefreshSpawnText();

                // 전역 UI 즉시 갱신
                ShopButton.GameEvents.OnSettingsChanged?.Invoke();
            }
        }
    }

    private IEnumerator UpdateButtonRoutine()
    {
        WaitForSeconds delay = new WaitForSeconds(0.1f);

        while (true)
        {
            GameData data = SaveManager.Load();

            int active = GetActiveChildCount();

            btn.interactable = (data.settings.clickNum > 0 && active < childMax);

            if (!isCooling &&
                data.settings.clickNum < clickMax &&
                active < childMax)
            {
                isCooling = true;
                timer = 0f;
                image.fillAmount = 1f;
            }

            yield return delay;
        }
    }

    private void OnClickButton()
    {
        GameData data = SaveManager.Load();

        if (data.settings.clickNum <= 0) return;

        mg.itemCreate(upch);

        if (Setting.IsSFXOn())
            audioSource.Play();

        if (data.background.spawn_check <= 50000)
            data.background.spawn_check++;

        data.missions.mission_3_value += 1;
        data.settings.clickNum = Mathf.Max(0, data.settings.clickNum - 1);

        SaveManager.Save(data);

        //  UI 즉시 갱신
        RefreshSpawnText();

        //  다른 UI 갱신
        ShopButton.GameEvents.OnSettingsChanged?.Invoke();

        //  캐싱된 값도 최신화 (이거 빠져 있어서 3/4가 안보였음!)
        RefreshSettingsFromData();
    }

    private void RefreshSpawnText()
    {
        if (spawnText == null) return;

        GameData data = SaveManager.Load();
        int num = data.settings.clickNum;
        int max = data.settings.clickMax;

        spawnText.text = $"({num} / {max})";
    }

    private void UpdateCharacterImage()
    {
        var cm = CharacterManager.Instance;
        if (cm == null) return;

        var character = cm.GetItem(upch);
        if (character == null || character.itemimg == null) return;

        image.sprite = character.itemimg;
        image_alpha.sprite = character.itemimg;
    }

    private int GetActiveChildCount()
    {
        int count = 0;
        int max = chp.childCount;

        for (int i = 0; i < max; i++)
            if (chp.GetChild(i).gameObject.activeSelf)
                count++;

        return count;
    }

    public void RefreshCharacterImage()
    {
        RefreshSettingsFromData();
        UpdateCharacterImage();
    }

    public bool IsSpawnable()
    {
        return btn.interactable;
    }
}