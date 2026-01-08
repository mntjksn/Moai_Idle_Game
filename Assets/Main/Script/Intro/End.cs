using UnityEngine;

public class End : MonoBehaviour
{
    public void but_event()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif    
    }
}   