using UnityEngine;
using TMPro;

public class MobileDebug : MonoBehaviour
{
    public TMP_Text debugText;

    public static MobileDebug Instance;

    void Awake()
    {
        Instance = this;
    }

    public void Log(string msg)
    {
        debugText.text = msg;
        Debug.Log(msg);
    }
}
