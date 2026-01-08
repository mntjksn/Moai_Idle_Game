using UnityEngine;
using UnityEngine.UI;

public class updown : MonoBehaviour
{
    public static System.Action OnUpDownChanged;

    public Button btn1;
    public Button btn2;

    GameData data;

    private void OnEnable()
    {
        data = SaveManager.Load();
        RefreshUI();

        OnUpDownChanged += RefreshUI;
    }

    private void OnDisable()
    {
        OnUpDownChanged -= RefreshUI;
    }

    private void RefreshUI()
    {
        int upCh = data.upgrades.upCh;
        int count = data.upgrades.count;

        if (upCh == 0)
        {
            btn1.interactable = false;
            btn2.interactable = false;
            return;
        }

        if (count == 0)
        {
            btn1.interactable = true;
            btn2.interactable = false;
            return;
        }

        if (count == upCh)
        {
            btn1.interactable = false;
            btn2.interactable = true;
            return;
        }

        btn1.interactable = true;
        btn2.interactable = true;
    }

    public void btn_up()
    {
        data = SaveManager.Load();

        if (data.upgrades.count < data.upgrades.upCh)
        {
            data.upgrades.count++;
            data.upgrades.chprefab = data.upgrades.count;
            SaveManager.Save(data);

            RefreshUI(); // 기존 코드

            //  ClickLimit 이미지 갱신
            var clickLimit = FindObjectOfType<ClickLimit>();
            if (clickLimit != null)
                clickLimit.RefreshCharacterImage();

            //  tokenViewer UI 갱신
            var viewer = FindObjectOfType<tokenViewer>();
            if (viewer != null)
                viewer.RefreshUI();
        }
    }

    public void btn_down()
    {
        data = SaveManager.Load();

        if (data.upgrades.count > 0)
        {
            data.upgrades.count--;
            data.upgrades.chprefab = data.upgrades.count;
            SaveManager.Save(data);

            RefreshUI();

            var clickLimit = FindObjectOfType<ClickLimit>();
            if (clickLimit != null)
                clickLimit.RefreshCharacterImage();

            var viewer = FindObjectOfType<tokenViewer>();
            if (viewer != null)
                viewer.RefreshUI();
        }
    }
}