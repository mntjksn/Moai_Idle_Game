using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Book2 : MonoBehaviour
{
    public int index_book2;

    [SerializeField] private TextMeshProUGUI book_name;
    [SerializeField] private TextMeshProUGUI sub_text;
    [SerializeField] private Image thumbImage;
    [SerializeField] private GameObject lockCover;

    public GameObject Book2_prefab;

    private Button btn;
    private bool isUnlocked;
    private Transform canvas2;

    private Item2 currentData;   //  해금 상태를 추적하기 위한 캐싱

    private void Awake()
    {
        btn = GetComponent<Button>();
        if (btn != null)
            btn.onClick.AddListener(OnClick);

        canvas2 = GameObject.Find("Canvas2")?.transform;
        Input.multiTouchEnabled = false;
    }

    /// <summary>
    /// 외부에서 데이터 주입 + UI 초기 세팅
    /// </summary>
    public void Setup(Item2 data, int index)
    {
        index_book2 = index;
        currentData = data;

        ApplyDataToUI();
    }

    /// <summary>
    /// UI 반영 로직을 따로 분리 (재사용됨)
    /// </summary>
    private void ApplyDataToUI()
    {
        // 이름
        if (book_name != null)
            book_name.text = currentData?.name ?? "이름없음";

        // 설명
        if (sub_text != null)
            sub_text.text = currentData?.sub2 ?? "";

        // 썸네일
        if (thumbImage != null)
        {
            if (currentData?.itemimg != null)
            {
                thumbImage.sprite = currentData.itemimg;
                thumbImage.gameObject.SetActive(true);
            }
            else
            {
                thumbImage.gameObject.SetActive(false);
            }
        }

        // 잠금 여부 갱신
        isUnlocked = currentData != null && currentData.spawncheck;

        if (lockCover != null)
            lockCover.SetActive(!isUnlocked);

        if (btn != null)
            btn.interactable = isUnlocked;
    }

    /// <summary>
    ///  아직 언락이 안된 상태라면 최소 비용으로 체크
    /// </summary>
    private void Update()
    {
        if (isUnlocked) return;

        // 배경 데이터는 BackgroundManager가 유지하고 있으니 다시 불러와도 OK
        var latest = BackgroundManager.Instance.GetItem(index_book2);

        if (latest != null && latest.spawncheck)
        {
            currentData = latest;
            ApplyDataToUI();    // 실시간 반영 
        }
    }

    private void OnClick()
    {
        if (!isUnlocked) return;

        BackgroundManager.Instance.SelectBackground(index_book2);

        GameData data = SaveManager.Load();
        data.upgrades.background = index_book2;
        SaveManager.Save(data);

        if (canvas2 != null)
            Instantiate(Book2_prefab, Vector3.zero, Quaternion.identity, canvas2);
    }
}