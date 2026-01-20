using UnityEngine;
using System.Collections;

public class GoldTotalCounter : MonoBehaviour
{
    // 캐릭터들이 배치되어 있는 부모 오브젝트
    private Transform chp;

    private void Start()
    {
        // 캐릭터 부모 오브젝트 탐색
        var root = GameObject.FindGameObjectWithTag("chp");
        if (root != null)
            chp = root.transform;

        // 일정 시간마다 골드 합산 처리
        StartCoroutine(GoldCounterRoutine());
    }

    private IEnumerator GoldCounterRoutine()
    {
        while (true)
        {
            // 현재 배치된 캐릭터 기준 골드 계산
            CalculateTotalGold();

            // 0.3초 간격으로 반복
            yield return new WaitForSeconds(0.3f);
        }
    }

    private void CalculateTotalGold()
    {
        if (chp == null)
            return;

        // 저장 데이터 로드(표시용 값 갱신 목적)
        GameData data = SaveManager.Load();

        int totalGold = 0;

        int count = chp.childCount;

        for (int i = 0; i < count; i++)
        {
            Transform child = chp.GetChild(i);

            // 비활성화된 오브젝트는 제외
            if (!child.gameObject.activeSelf)
                continue;

            // 캐릭터 아이템 컴포넌트 확인
            MergeItem mi = child.GetComponent<MergeItem>();
            if (mi == null)
                continue;

            // 캐릭터 데이터 참조
            CharacterItem item = CharacterManager.Instance.GetItem(mi.iN);
            if (item == null)
                continue;

            // 기본 골드 계산
            int baseGold = item.itemgold;

            // 강화 여부에 따른 골드 보정
            int earned = item.upgrade ? baseGold * 2 : baseGold;

            totalGold += earned;
        }

        // 계산된 골드를 데이터에 반영 (저장은 하지 않음)
        data.settings.ChgetGold = totalGold;
    }
}