using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MergeItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private SpriteRenderer sr;
    private Item item;

    private bool isSelect;
    private MergeItem contactItem;

    private Transform chp;
    private Transform canvas;
    private Camera cam;

    private Coroutine goldRoutine;
    private Vector3 offset;

    [Header("Prefabs")]
    public GameObject goldtextprefab;

    [Header("Item Data")]
    public int iN;
    public bool SC;
    public bool UC;
    public float chgetgold;
    private int chgold;

    private bool isInitialized = false;  // ★ 풀링 초기 생성 시 OnEnable 방지

    private void Awake()
    {
        chp = GameObject.FindGameObjectWithTag("chp")?.transform;
        canvas = GameObject.Find("Canvas")?.transform;
        cam = Camera.main;
    }

    public void InitItem(Item i)
    {
        item = i;
        sr = GetComponent<SpriteRenderer>();

        sr.sprite = item.itemimg;
        iN = item.itemNum;
        SC = item.spawncheck;
        UC = item.upgrade;

        chgold = item.itemgold;
        chgetgold = item.itemgold;

        isInitialized = true;   // ★ 이제부터 OnEnable 작동 허용
    }

    private void OnEnable()
    {
        if (!isInitialized || item == null) return;

        if (goldRoutine != null)
            StopCoroutine(goldRoutine);

        goldRoutine = StartCoroutine(GetGoldCoroutine());
    }

    private void OnDisable()
    {
        if (goldRoutine != null)
        {
            StopCoroutine(goldRoutine);
            goldRoutine = null;
        }
    }

    private IEnumerator GetGoldCoroutine()
    {
        GameData data = SaveManager.Load();
        float delayTime = data.settings.getGoldTime;
        WaitForSeconds delay = new WaitForSeconds(delayTime);

        while (true)
        {
            data = SaveManager.Load();

            chgold = item.itemgold;
            int earned = item.upgrade ? chgold * 2 : chgold;

            if (earned > 0 && data.currency.gold < 2147483600)
            {
                if (Setting.IsTEXTOn())
                    CreateGoldText(earned);

                data.currency.gold += earned;
                data.missions.mission_5_value += earned;
                SaveManager.Save(data);
            }

            // 만약 getGoldTime이 설정에서 변경되면 delay 재생성해야 할 수도 있음
            if (delayTime != data.settings.getGoldTime)
            {
                delayTime = data.settings.getGoldTime;
                delay = new WaitForSeconds(delayTime);
            }

            yield return delay;
        }
    }

    private void CreateGoldText(int amount)
    {
        if (goldtextprefab == null || canvas == null) return;

        GameObject go = Instantiate(goldtextprefab, transform.position + new Vector3(0.4f, 0.85f), Quaternion.identity, canvas);
        go.GetComponent<TextMeshProUGUI>().text = $"+ {amount:N0}";
        Destroy(go, 0.3f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isSelect) return;

        MergeItem other = collision.GetComponent<MergeItem>();
        if (other != null && other != this && other.iN == this.iN)
            contactItem = other;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        MergeItem other = collision.GetComponent<MergeItem>();
        if (other != null && other == contactItem)
            contactItem = null;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isSelect = true;
        Vector3 worldPos = cam.ScreenToWorldPoint(eventData.position);
        offset = transform.position - worldPos;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isSelect = false;

        if (contactItem != null)
        {
            int nextIndex = item.itemNum + 1;

            ObjectPool.Instance.ReturnToPool(contactItem.iN, contactItem.gameObject);
            ObjectPool.Instance.ReturnToPool(iN, gameObject);

            Merge merge = GameObject.Find("ItemData")?.GetComponent<Merge>();
            if (merge != null)
                merge.itemCreate(nextIndex);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (cam == null) cam = Camera.main;

        Vector3 worldPos = cam.ScreenToWorldPoint(eventData.position);
        worldPos.z = 0;
        transform.position = worldPos + offset;
    }
}