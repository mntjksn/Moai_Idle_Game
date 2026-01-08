using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    public GameObject slotPrefab;
    public Transform content;

    public List<UpgradeSlot> slots = new List<UpgradeSlot>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        int max = CharacterManager.Instance.characters.Count;

        for (int i = 1; i < max; i++)
        {
            GameObject obj = Instantiate(slotPrefab, content);
            UpgradeSlot slot = obj.GetComponent<UpgradeSlot>();
            slot.index = i;
            slots.Add(slot);
        }

        UpdateAllSlots();
    }

    public void UpdateAllSlots()
    {
        foreach (var slot in slots)
        {
            if (slot != null)
                slot.UpdateSlot();
        }
    }
}