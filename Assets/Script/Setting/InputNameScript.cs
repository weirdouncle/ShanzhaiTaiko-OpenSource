using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputNameScript : SettingButtonScript
{
    public InputField Field;

    private bool changing_name;
    public override void OnClick()
    {
        OnChangeName();
    }

    protected override void SetSelected(bool selected)
    {
        SelectedFrame.SetActive(selected);
        if (!selected)
        {
            changing_name = false;
            ValueText1.gameObject.SetActive(true);
            Field.gameObject.SetActive(false);
            ValueText1.text = GameSetting.Config.PlayerName;
        }
    }

    public void OnChangeName()
    {
        if (!changing_name)
        {
            changing_name = true;
            ValueText1.gameObject.SetActive(false);
            Field.gameObject.SetActive(true);
            Field.text = GameSetting.Config.PlayerName;
            Field.ActivateInputField();
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
            changing_name = false;
            ValueText1.gameObject.SetActive(true);
            if (!string.IsNullOrEmpty(Field.text)) GameSetting.SetPlayerName(Field.text);
            Field.gameObject.SetActive(false);
            ValueText1.text = GameSetting.Config.PlayerName;
        }
    }

    public void Resume()
    {
        EventSystem.current.SetSelectedGameObject(null);
        changing_name = false;
        ValueText1.gameObject.SetActive(true);
        Field.gameObject.SetActive(false);
        ValueText1.text = GameSetting.Config.PlayerName;
    }
}
