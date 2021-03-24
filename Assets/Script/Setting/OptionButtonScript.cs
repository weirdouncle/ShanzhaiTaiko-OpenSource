using UnityEngine;
using UnityEngine.UI;

public class OptionButtonScript : MonoBehaviour
{
    public Text Title;
    public Image[] Arrows;
    private bool _disable;
    private bool _current;

    public bool Disable
    {
        get => _disable;
        set
        {
            _disable = value;
            SetDisable(_disable);
        }
    }
    public bool Current
    {
        get => _current;
        set
        {
            _current = value;
            SetCurrent(_current);
        }
    }

    private void SetDisable(bool disable)
    {
        Title.color = disable ? new Color(0.66f, 0.66f, 0.66f) : new Color(1, 1, 1);
    }
    private void SetCurrent(bool active)
    {
        foreach (Image image in Arrows)
            if (image.gameObject.activeSelf)
                image.color = active ? new Color(0, 0, 0, 0.5f) : new Color(0, 0, 0, 1);
    }
}
