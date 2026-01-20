using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// 개별 캐릭터(모아이) 오브젝트의 동작을 담당
// - 드래그
// - 머지 판정
// - 주기적인 골드 획득
public class MergeItem : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IDragHandler
{
    // 스프라이트 렌더러
    private SpriteRenderer sr;

    // 캐릭터 데이터
    private CharacterItem item;

    // 드래그 선택 상태
    private bool isSelect;

    // 머지 대상이 된 다른 아이템
    private MergeItem contactItem;

    // 캐릭터 부모 / UI 캔버스 / 카메라
    private Transform chp;
    private Transform canvas;
    private Camera cam;

    // 골드 획득 코루틴
    private Coroutine goldRoutine;

    // 드래그 시 마우스와의 오프셋
    private Vector3 offset;

    [Header("Prefabs")]
    // 골드 획득 시 표시할 텍스트 프리팹
    public GameObject goldtextprefab;

    [Header("Item Data")]
    // 아이템 번호
    public int iN;

    // 도감 해금 여부
    public bool SC;

    // 강화 여부
    public bool UC;

    // 캐릭터 초당 골드(표시용)
    public float chgetgold;

    // 기본 골드 값
    private int chgold;

    // 풀링 초기 생성 시 OnEnable 중복 실행 방지용
    private bool isInitialized = false;

    // 머지 중인지 여부(외부 제어용)
    public bool isMerging;

    private void Awake()
    {
        // 필수 참조 캐싱
        chp = GameObject.FindGameObjectWithTag("chp")?.transform;
        canvas = GameObject.Find("Canvas")?.transform;
        cam = Camera.main;
    }

    // 풀에서 꺼낼 때 캐릭터 데이터 초기화
    public void InitItem(CharacterItem i)
    {
        item = i;
        sr = GetComponent<SpriteRenderer>();

        // 스프라이트 및 기본 정보 세팅
        sr.sprite = item.itemimg;
        iN = item.itemNum;
        SC = item.spawncheck;
        UC = item.upgrade;

        chgold = item.itemgold;
        chgetgold = item.itemgold;

        // 초기화 완료 이후부터 OnEnable 허용
        isInitialized = true;
    }

    private void OnEnable()
    {
        // 풀링으로 생성될 때 InitItem 이전이면 실행하지 않음
        if (!isInitialized || item == null)
            return;

        // 기존 코루틴 정리
        if (goldRoutine != null)
            StopCoroutine(goldRoutine);

        // 골드 획득 루프 시작
        goldRoutine = StartCoroutine(GetGoldCoroutine());
    }

    private void OnDisable()
    {
        // 비활성화 시 골드 획득 중지
        if (goldRoutine != null)
        {
            StopCoroutine(goldRoutine);
            goldRoutine = null;
        }
    }

    // 일정 시간마다 골드 획득
    private IEnumerator GetGoldCoroutine()
    {
        GameData data = SaveManager.Load();

        float delayTime = data.settings.getGoldTime;
        WaitForSeconds delay = new WaitForSeconds(delayTime);

        while (true)
        {
            data = SaveManager.Load();

            // 현재 캐릭터 골드 값
            chgold = item.itemgold;
            int earned = item.upgrade ? chgold * 2 : chgold;

            // 골드 최대치 방어
            if (earned > 0 && data.currency.gold < 2147483600)
            {
                // 골드 텍스트 표시 여부
                if (Setting.IsTEXTOn())
                    CreateGoldText(earned);

                data.currency.gold += earned;
                data.missions.mission_5_value += earned;

                SaveManager.Save(data);
            }

            // 설정에서 골드 획득 시간이 바뀌었을 경우 반영
            if (delayTime != data.settings.getGoldTime)
            {
                delayTime = data.settings.getGoldTime;
                delay = new WaitForSeconds(delayTime);
            }

            yield return delay;
        }
    }

    // 골드 획득 텍스트 생성
    private void CreateGoldText(int amount)
    {
        if (goldtextprefab == null || canvas == null)
            return;

        GameObject go = Instantiate(
            goldtextprefab,
            transform.position + new Vector3(0.4f, 0.85f),
            Quaternion.identity,
            canvas
        );

        go.GetComponent<TextMeshProUGUI>().text = "+ " + amount.ToString("N0");
        Destroy(go, 0.3f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 드래그 중이 아니면 머지 판정 안 함
        if (!isSelect)
            return;

        // 같은 레벨의 아이템과 닿았는지 확인
        MergeItem other = collision.GetComponent<MergeItem>();
        if (other != null && other != this && other.iN == this.iN)
            contactItem = other;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // 접촉 대상 해제
        MergeItem other = collision.GetComponent<MergeItem>();
        if (other != null && other == contactItem)
            contactItem = null;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 드래그 시작
        isSelect = true;

        Vector3 worldPos = cam.ScreenToWorldPoint(eventData.position);
        offset = transform.position - worldPos;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 드래그 종료
        isSelect = false;

        // 머지 대상이 있으면 합치기 실행
        if (contactItem != null)
        {
            int nextIndex = item.itemNum + 1;

            // 두 아이템을 풀로 반환
            ObjectPool.Instance.ReturnToPool(contactItem.iN, contactItem.gameObject);
            ObjectPool.Instance.ReturnToPool(iN, gameObject);

            // 다음 단계 아이템 생성
            Merge merge = GameObject.Find("ItemData")?.GetComponent<Merge>();
            if (merge != null)
                merge.itemCreate(nextIndex);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 카메라가 없으면 다시 찾기
        if (cam == null)
            cam = Camera.main;

        // 마우스 위치를 월드 좌표로 변환하여 이동
        Vector3 worldPos = cam.ScreenToWorldPoint(eventData.position);
        worldPos.z = 0;

        transform.position = worldPos + offset;
    }
}