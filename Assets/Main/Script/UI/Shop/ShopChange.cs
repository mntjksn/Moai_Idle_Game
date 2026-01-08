using UnityEngine;

public class ShopChange : MonoBehaviour
{
    public GameObject gold_panel, dia_panel, token_panel;

    // Start is called before the first frame update

    private void OnEnable()
    {
        gold_panel.SetActive(true);
        dia_panel.SetActive(false);
        token_panel.SetActive(false);
    }

public void ClickShop(string name)
    {
        if (name == "gold")
        {
            gold_panel.SetActive(true);
            dia_panel.SetActive(false);
            token_panel.SetActive(false);
        }
        else if (name == "dia")
        {
            gold_panel.SetActive(false);
            dia_panel.SetActive(true);
            token_panel.SetActive(false);
        }
        else if (name == "token")
        {
            gold_panel.SetActive(false);
            dia_panel.SetActive(false);
            token_panel.SetActive(true);
        }
        else
            return;
    }
}
