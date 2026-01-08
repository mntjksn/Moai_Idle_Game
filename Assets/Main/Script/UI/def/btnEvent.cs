using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class btnEvent : MonoBehaviour
{
    public GameObject panel;

    public void panel_show()
    {
        panel.SetActive(true);
    }

    public void panel_off()
    {
        panel.SetActive(false);
    }

    public void destroy_panel()
    {
       Destroy(panel);
    }
}
