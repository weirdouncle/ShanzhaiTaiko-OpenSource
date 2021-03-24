using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public delegate void LabelClickDelegate(GameSetting.SkinPosition position);
public class LabelButtonScript : MonoBehaviour
{
    public event LabelClickDelegate LabelClick;

    public GameSetting.SkinPosition Position;
    public Image Image;
    public GameObject SelectedImage;

    private bool selected;
    private bool enable = true;
    private bool havor;

    public bool Selected 
    {
        get => selected;
        set
        {
            selected = value;
            SetState();
        }
    }
    public bool Enable
    {
        get => enable;
        set
        {
            enable = value;
            SetState();
        }
    }

    public bool Havor
    {
        get => havor;
        set
        {
            havor = value;
            SetState();
        }
    }

    private void SetState()
    {
        SelectedImage.SetActive(selected);
        Image.transform.localScale = selected ? new Vector3(1, 1) : new Vector3(0.7f, 0.7f);
        Image.color = selected ? new Color(1, 1, 1) : enable ? new Color(0.7f, 0.7f, 0.7f, havor ? 1 : 0.7f) : new Color(0.5f, 0.5f, 0.5f, 0.7f);
    }

    public void OnPointerEnter()
    {
        Havor = true;
    }

    public void OnPointerOut()
    {
        Havor = false;
    }

    public void OnClick(BaseEventData data)
    {
        if (data is PointerEventData pointer && pointer.button == PointerEventData.InputButton.Left && !selected && enable)
            LabelClick?.Invoke(Position);
    }
}
