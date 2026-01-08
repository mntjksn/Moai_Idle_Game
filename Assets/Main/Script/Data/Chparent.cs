using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class chparent : MonoBehaviour
{
    private void Awake()
    {
        // ½Ì±ÛÅæ À¯Áö
        var objs = FindObjectsOfType<chparent>();
        if (objs.Length == 1)
        {
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
}
