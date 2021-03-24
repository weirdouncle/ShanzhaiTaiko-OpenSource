using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingButtonScript : MonoBehaviour
{
    public GameObject SelectedFrame;
    public Text ValueText1;
    public Text ValueText2;
    public Slider Slider;
    public Animator Animator;
    public Animator Left;
    public Animator Right;
    public ValueType Type;
    public string on_string;
    public string off_string;
    public int Index;
    public Sprite[] Sprites;

    public event MouseHavorDelegate MouseHavor;
    public delegate void MouseHavorDelegate(int index);

    public enum ValueType
    {
        TypeValue,
        TypeBool,
        TypeFunction,
    }
    public bool Selected
    {
        get => _selected;
        set
        {
            _selected = value;
            SetSelected(_selected);
        }
    }
    public bool BoolValue { set; get; }
    public int IntValue { set; get; }

    protected bool _selected;

    void Start()
    {
        if (Slider != null)
        {
            Slider.onValueChanged.AddListener(CheckLimit);
        }

        on_string = GameSetting.Translate(on_string);
        off_string = GameSetting.Translate(off_string);
    }
    private void CheckLimit(float arg0)
    {
        Left.GetComponent<Image>().sprite = Slider.minValue == arg0 ? Sprites[0] : Sprites[1];
        Right.GetComponent<Image>().sprite = Slider.maxValue == arg0 ? Sprites[0] : Sprites[1];
    }

    public void CheckLimit()
    {
        Left.GetComponent<Image>().sprite = Slider.minValue == IntValue ? Sprites[0] : Sprites[1];
        Right.GetComponent<Image>().sprite = Slider.maxValue == IntValue ? Sprites[0] : Sprites[1];
    }

    protected virtual void SetSelected(bool selected)
    {
        SelectedFrame.SetActive(_selected);
    }

    public void SetValue1(string value)
    {
        if (ValueText1 != null)
            ValueText1.text = value;
    }
    public void SetValue2(string value)
    {
        ValueText2.text = value;
    }

    public void Press(bool right, bool press)
    {
        if (!gameObject.activeInHierarchy) return;
        if (right)
            Right.SetBool("Press", press);
        else
            Left.SetBool("Press", press);
    }

    public virtual void SetSliderValue(int value)
    {
        if (Type == ValueType.TypeValue && Slider != null)
        {
            Slider.value = value;
            IntValue = value;
            if (ValueText1 != null)
                ValueText1.text = IntValue.ToString();
            CheckLimit();
        }
    }

    public virtual void LeftClick()
    {
        Left.SetBool("Press", true);
        StartCoroutine(DelayRelase(Left));
    }
    public virtual void RightClick()
    {
        Right.SetBool("Press", true);
        StartCoroutine(DelayRelase(Right));
    }

    IEnumerator DelayRelase(Animator animator)
    {
        yield return new WaitForEndOfFrame();
        animator.SetBool("Press", false);
    }

    public virtual void OnClick()
    {
        if (Type != ValueType.TypeValue)
        {
            if (gameObject.activeInHierarchy) Animator.SetTrigger("Clicked");
            if (Type == ValueType.TypeBool)
            {
                BoolValue = !BoolValue;
                ValueText1.text = BoolValue ? GameSetting.Translate(on_string) : GameSetting.Translate(off_string);
            }
        }
    }

    public virtual void OnMouseHavor()
    {
        MouseHavor?.Invoke(Index);
    }
}
