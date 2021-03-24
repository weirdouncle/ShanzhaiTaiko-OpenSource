using CommonClass;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public delegate void SkinClickDelegate(GameSetting.SkinPosition position, int index);
public delegate void SkinConfirmDelegate();
public class SkinButtonScript : MonoBehaviour
{
    public static event SkinClickDelegate SkinClick;

    public static event SkinConfirmDelegate SkinConfirm;

    public Sprite[] Sprites;
    public GameSetting.SkinPosition Position;
    public Image Image;
    public GameObject SelectedMark;
    public GameObject FocusFrame;
    public Text Text;
    public int Index;

    private SkinInfo info;

    public bool Selected
    {
        get => selected;
        set
        {
            selected = value;
            SetState();
        }
    }

    public bool Focus
    {
        get => focus;
        set
        {
            focus = value;
            SetState();
        }
    }

    private bool selected;
    private bool focus;

    private void SetState()
    {
        SelectedMark.SetActive(selected);
        Image.sprite = focus ? Sprites[0] : Sprites[1];
        FocusFrame.SetActive(focus);
    }

    public void SetText(int index, string text, GameSetting.SkinPosition position)
    {
        Position = position;
        Index = index;
        Text.text = text;
    }

    public void OnClick(BaseEventData data)
    {
        if (data is PointerEventData pointer && pointer.button == PointerEventData.InputButton.Left)
        {
            if (focus && !selected)
            {
                SkinConfirm?.Invoke();
            }
            else if (!focus)
            {
                SkinClick?.Invoke(Position, Index);
            }
        }
    }
}
