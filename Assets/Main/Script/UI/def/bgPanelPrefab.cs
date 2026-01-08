using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class bgPanelPrefab : MonoBehaviour
{
    public enum PanelType { Bg_panel, Book2_panel }
    public PanelType panelType;

    public Image thisimg;
    public TextMeshProUGUI chname, sub;

    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        if (Setting.IsSFXOn())
            audioSource.Play();

        // 이벤트 등록
        BackgroundManager.Instance.OnBackgroundSelected += Refresh;

        // 첫 적용
        Refresh(BackgroundManager.Instance.SelectedIndex);
    }

    private void OnDestroy()
    {
        // 이벤트 해제 (메모리 누수 방지)
        if (BackgroundManager.Instance != null)
            BackgroundManager.Instance.OnBackgroundSelected -= Refresh;
    }

    private void Refresh(int _)
    {
        GameData data = SaveManager.Load();

        int bgIndex = data.upgrades.background;
        int book2Index = data.upgrades.backgroundcheck;

        Item2 bg = BackgroundManager.Instance.GetItem(bgIndex);
        Item2 book2 = BackgroundManager.Instance.GetItem(book2Index);

        switch (panelType)
        {
            case PanelType.Bg_panel:
                Apply(bg);
                break;

            case PanelType.Book2_panel:
                Apply(book2);
                break;
        }
    }

    private void Apply(Item2 data)
    {
        if (data == null) return;

        if (thisimg != null)
            thisimg.sprite = data.itemimg;

        if (chname != null)
            chname.text = data.name;

        if (sub != null)
            sub.text = data.sub;
    }
}