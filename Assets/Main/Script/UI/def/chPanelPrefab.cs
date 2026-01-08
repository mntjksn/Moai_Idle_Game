using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class chPanelPrefab : MonoBehaviour
{
    public enum PanelType { Ch_panel, Book_panel }
    public PanelType panelType;

    public Image thisimg;
    public TextMeshProUGUI chname, sub, getgold;

    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        if (Setting.IsSFXOn())
            audioSource.Play();

        // 패널 생성 시 즉시 갱신
        Refresh();
    }

    /// <summary>
    /// 패널 UI 한 번만 갱신 (Update 제거)
    /// </summary>
    public void Refresh()
    {
        GameData data = SaveManager.Load();

        int chIndex = data.upgrades.chprefab;
        int bookIndex = data.upgrades.booknum;

        Item ch = CharacterManager.Instance.GetItem(chIndex);
        Item book = CharacterManager.Instance.GetItem(bookIndex);

        switch (panelType)
        {
            case PanelType.Ch_panel:
                Apply(ch, chIndex);
                break;

            case PanelType.Book_panel:
                Apply(book, bookIndex);
                break;
        }
    }

    private void Apply(Item item, int index)
    {
        if (item == null) return;

        if (thisimg != null)
            thisimg.sprite = item.itemimg;

        if (chname != null)
            chname.text = (index == 0) ? $"{item.name}" : $"{index}. {item.name}";

        if (sub != null)
            sub.text = item.sub;

        if (getgold != null)
            getgold.text = $"획득 돌멩이 : {item.itemgold:N0}개";
    }
}