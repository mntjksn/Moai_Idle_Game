using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BookChange : MonoBehaviour
{
    public GameObject ch_panel, bg_panel;

    // Start is called before the first frame update

    private void OnEnable()
    {
        ch_panel.SetActive(true);
        bg_panel.SetActive(false);
    }

    public void ClickBook(string name)
    {
        if (name == "ch")
        {
            ch_panel.SetActive(true);
            bg_panel.SetActive(false);
        }
        else if (name == "bg")
        {
            ch_panel.SetActive(false);
            bg_panel.SetActive(true);
        }
        else
            return;
    }
}
