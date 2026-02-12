using UnityEngine;

public class SettingToggleVisual : MonoBehaviour
{
    [Header("State")]
    public bool isOn = true;

    [Header("Visuals")]
    public GameObject checkmark;   // ✓
    public GameObject crossmark;   // X

    public void Toggle()
    {
        isOn = !isOn;
        Refresh();
    }

    public void Set(bool value)
    {
        isOn = value;
        Refresh();
    }

    void Start()
    {
        Refresh();
    }

    void Refresh()
    {
        if (checkmark != null)
            checkmark.SetActive(isOn);

        if (crossmark != null)
            crossmark.SetActive(!isOn);
    }
}
