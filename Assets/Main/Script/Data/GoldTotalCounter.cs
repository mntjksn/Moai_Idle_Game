using UnityEngine;
using System.Collections;

public class GoldTotalCounter : MonoBehaviour
{
    private Transform chp;

    private void Start()
    {
        var root = GameObject.FindGameObjectWithTag("chp");
        if (root != null)
            chp = root.transform;

        StartCoroutine(GoldCounterRoutine());
    }

    private IEnumerator GoldCounterRoutine()
    {
        while (true)
        {
            CalculateTotalGold();
            yield return new WaitForSeconds(0.3f);
        }
    }

    private void CalculateTotalGold()
    {
        if (chp == null) return;

        GameData data = SaveManager.Load();

        int totalGold = 0;

        int count = chp.childCount;

        for (int i = 0; i < count; i++)
        {
            Transform child = chp.GetChild(i);

            // 실제 모아이만 계산
            if (!child.gameObject.activeSelf)
                continue;

            MergeItem mi = child.GetComponent<MergeItem>();
            if (mi == null)
                continue;

            // itemData에서 직접 계산 (항상 최신 상태 유지)
            Item item = CharacterManager.Instance.GetItem(mi.iN);
            if (item == null)
                continue;

            int baseGold = item.itemgold;
            int earned = item.upgrade ? baseGold * 2 : baseGold;

            totalGold += earned;
        }

        // 데이터에 갱신만 하고 저장은 하지 않음 (UI 표시용)
        data.settings.ChgetGold = totalGold;
    }
}