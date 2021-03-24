using CommonClass;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputAction;

public class AndroidConfigScript : MonoBehaviour
{
    public static event PlaySelectedSoundDelegate PlaySelectedSound;

    public Toggle Full;
    public Toggle Dancer;
    public Toggle Outline;
    public Text Title;
    public Text[] ButtonsTexts;
    public Dropdown Language;

    private WASDInput BasicInput;
    private bool quit;

    private readonly List<string> lang = new List<string>
    {
        Locale.Chinese.ToString(),
        Locale.English.ToString(),
        Locale.Japanese.ToString(),
        Locale.Korean.ToString(),
        Locale.Spanish.ToString(),
    };
    void Start()
    {
        ResetTranslate();

        Full.isOn = GameSetting.Config.FullScreen;
        Dancer.isOn = GameSetting.Dancer;
        Outline.isOn = GameSetting.Config.Outline;

        List<string> langs = new List<string>();
        foreach (string str in lang)
            langs.Add(GameSetting.Translate(str));
        Language.ClearOptions();
        Language.AddOptions(langs);
        Language.value = (int)GameSetting.Config.Locale;

        BasicInput = new WASDInput();
        BasicInput.Player.Esc.performed += Quit;
        BasicInput.Enable();
    }

    private void ResetTranslate()
    {
        Title.text = GameSetting.Translate("Config");
        ButtonsTexts[0].text = GameSetting.Translate("Stretch to full screen");
        ButtonsTexts[1].text = GameSetting.Translate("Show Dancer");
        ButtonsTexts[2].text = GameSetting.Translate("Language");
        ButtonsTexts[3].text = GameSetting.Translate("Hardware Outline");
    }

    void Quit(CallbackContext context)
    {
        Quit();
    }

    public void Quit()
    {
        if (!Application.isFocused || quit) return;
        StartCoroutine(LoadMain());
    }

    IEnumerator LoadMain()
    {
        quit = true;
        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Cancel);
        yield return new WaitForSeconds(0.3f);

        AsyncOperation async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("AndroidMainScreen");
        async.allowSceneActivation = true;
        yield return async;
    }

    public void SetFullScreen()
    {
        GameSetting.SetFullScreen(Full.isOn);

        float x = (float)SettingLoader.Width / 16;
        float y = (float)SettingLoader.Height / 9;
        int width = 0;
        int height = 0;
        if (GameSetting.Config.FullScreen)
        {
            if (x > y)
            {
                y = Math.Min(120, y);
                width = (int)Math.Ceiling(y * 16);
                height = (int)Math.Ceiling(y * 9);
            }
            else if (y > x)
            {
                x = Math.Min(120, x);
                width = (int)Math.Ceiling(x * 16);
                height = (int)Math.Ceiling(x * 9);
            }
            Screen.SetResolution(width, height, true);
        }
        else
        {
            if (x > y)
            {
                y = Math.Min(120, y);
                width = (int)Math.Ceiling(y * 9 * SettingLoader.Width / SettingLoader.Height);
                height = (int)Math.Ceiling(y * 9);
            }
            else if (y > x)
            {
                x = Math.Min(120, x);
                width = (int)Math.Ceiling(x * 16);
                height = (int)Math.Ceiling(x * 16 * SettingLoader.Height / SettingLoader.Width);
            }
            Screen.SetResolution(width, height, true);
        }
    }

    public void SetDancer()
    {
        GameSetting.Dancer = Dancer.isOn;
    }
    public void SetOutline()
    {
        GameSetting.Config.Outline = Outline.isOn;
        GameSetting.SaveConfig();
    }
    public void ChangeLang()
    {
        GameSetting.ChangeLanguage((Locale)Enum.Parse(typeof(Locale), lang[Language.value]));
        ResetTranslate();

        ReLoadConfig();
        SettingLoader.LoadPics();
    }

    private void ReLoadConfig()
    {
        List<string> langs = new List<string>();
        foreach (string str in lang)
            langs.Add(GameSetting.Translate(str));
        for (int i = 0; i < Language.options.Count; i++)
            Language.options[i].text = langs[i];
        Language.captionText.text = langs[Language.value];
    }
}
