using CommonClass;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputAction;

public delegate void ResetLanguageDelegate();
public class ResolutionScript : MonoBehaviour
{
    public static event PlaySelectedSoundDelegate PlaySelectedSound;

    public static event ResetLanguageDelegate ResetLanguage;

    public Dropdown Resolutions;
    public Toggle Full;
    public Toggle Music;
    public Slider Refresh;
    public Text RefreshValue;
    public Toggle VSync;
    public ComboScript Combo;
    public Dropdown Input;
    public Dropdown Style;
    public Text InputDescri;
    public Dropdown Language;
    public Toggle Outline;

    public Animator[] Dons;
    public Animator[] Kas;
    public CriAtomSource[] Audios;
    public Text Rate;

    private WASDInput BasicInput;
    private bool quit;
    private List<KeyValuePair<int, int>> resolutions;
    private readonly List<string> input_des = new List<string>
    {
        { "Dynamic refresh according to current FPS" },
        { "Refresh on fixed time step" },
    };
    private readonly List<string> styles = new List<string>
    {
        GameStyle.TraditionalStyle.ToString(),
        GameStyle.Ps4Style.ToString(),
        GameStyle.NijiiroStyle.ToString(),
    };
    private readonly List<string> lang = new List<string>
    {
        Locale.Chinese.ToString(),
        Locale.English.ToString(),
        Locale.Japanese.ToString(),
        Locale.Korean.ToString(),
        Locale.Spanish.ToString(),
    };
    private readonly List<string> input_options = new List<string> { "Depend on FPS", "Fixed time step" };

    private float updateInterval = 1.0f;
    private float lastInterval;
    private int frames = 0;
    void Start()
    {
        lastInterval = Time.realtimeSinceStartup;
        LoadConfig();

        BasicInput = new WASDInput();
        BasicInput.Player.Esc.performed += Quit;
        //BasicInput.Player.Cancel.performed += Quit;
        BasicInput.Enable();

        if (GameSetting.DonLeft != null)
            GameSetting.DonLeft.started += DrumInvoke;
        if (GameSetting.DonRight != null)
            GameSetting.DonRight.started += DrumInvoke;
        if (GameSetting.KaLeft != null)
            GameSetting.KaLeft.started += DrumInvoke;
        if (GameSetting.KaRight != null)
            GameSetting.KaRight.started += DrumInvoke;

        if (GameSetting.Config.DirectInput)
            BasicInputScript.KeyInvoke += DirectKey;
    }

    private void OnDestroy()
    {
        if (GameSetting.DonLeft != null)
            GameSetting.DonLeft.started -= DrumInvoke;
        if (GameSetting.DonRight != null)
            GameSetting.DonRight.started -= DrumInvoke;
        if (GameSetting.KaLeft != null)
            GameSetting.KaLeft.started -= DrumInvoke;
        if (GameSetting.KaRight != null)
            GameSetting.KaRight.started -= DrumInvoke;

        BasicInputScript.KeyInvoke -= DirectKey;
    }

    private void LoadConfig()
    {
        Resolutions.ClearOptions();
        resolutions = new List<KeyValuePair<int, int>>();
        List<string> options = new List<string>();

        float x = (float)Screen.currentResolution.width / 16;
        float y = (float)Screen.currentResolution.height / 9;

        int index = 0;
        for (int i = 0; i < Screen.resolutions.Length; i++)
        {
            Resolution resolution = Screen.resolutions[i];
            int width;
            int height;
            if (x > y)
            {
                height = resolution.height;
                width = (int)Math.Ceiling(y * 16);
            }
            else if (y > x)
            {
                height = (int)Math.Ceiling(x * 9);
                width = resolution.width;
            }
            else
            {
                width = resolution.width;
                height = resolution.height;
            }
            string option = string.Format("{0}x{1}", width, height);
            //Debug.Log(option);
            if (!options.Contains(option))
            {
                options.Add(option);
                resolutions.Add(new KeyValuePair<int, int>(width, height));
            }
        }

        for (int i = 0; i < resolutions.Count; i++)
        {
            if (GameSetting.Config.ResolutionWidth == resolutions[i].Key && GameSetting.Config.ResolutionHeight == resolutions[i].Value)
                index = i;
        }

        Resolutions.AddOptions(options);
        Resolutions.value = index;

        Full.isOn = GameSetting.Config.FullScreen;
        Music.isOn = GameSetting.Config.AutoRectify;

        Refresh.value = (int)InputSystem.pollingFrequency / 10;
        RefreshValue.text = (Refresh.value * 10).ToString();

        VSync.isOn = GameSetting.Config.VSync;

        List<string> input_options = new List<string>();
        foreach (string str in this.input_options)
            input_options.Add(GameSetting.Translate(str));
        Input.ClearOptions();
        Input.AddOptions(input_options);
        int choice = InputSystem.settings.updateMode == InputSettings.UpdateMode.ProcessEventsInDynamicUpdate ? 0 : 1;
        InputDescri.text = GameSetting.Translate(input_des[choice]);
        Input.value = choice;

        List<string> styles = new List<string>();
        foreach (string str in this.styles)
            styles.Add(GameSetting.Translate(str));
        Style.ClearOptions();
        Style.AddOptions(styles);
        Style.value = GameSetting.Config.GameStyle;

        List<string> langs = new List<string>();
        foreach (string str in lang)
            langs.Add(GameSetting.Translate(str));
        Language.ClearOptions();
        Language.AddOptions(langs);
        Language.value = (int)GameSetting.Config.Locale;

        Outline.isOn = GameSetting.Config.Outline;
    }

    int combo_count;
    private void DrumInvoke(CallbackContext context)
    {
        if (!Application.isFocused || quit) return;
        if (context.action == GameSetting.DonLeft)
        {
            Audios[0].Play();
            Dons[0].SetTrigger("Hit");
            combo_count++;
            StopAllCoroutines();
            StartCoroutine(SetCombo());
        }
        else if (context.action == GameSetting.DonRight)
        {
            Audios[0].Play();
            Dons[1].SetTrigger("Hit");
            combo_count++;
            StopAllCoroutines();
            StartCoroutine(SetCombo());
        }
        else if (context.action == GameSetting.KaLeft)
        {
            Audios[1].Play();
            Kas[0].SetTrigger("Hit");
            combo_count++;
            StopAllCoroutines();
            StartCoroutine(SetCombo());
        }
        else if (context.action == GameSetting.KaRight)
        {
            Audios[1].Play();
            Kas[1].SetTrigger("Hit");
            combo_count++;
            StopAllCoroutines();
            StartCoroutine(SetCombo());
        }
    }

    protected virtual void DirectKey(GameSetting.KeyType key, bool press)
    {
        if (quit) return;
        switch (key)
        {
            case GameSetting.KeyType.LeftDon:
                {
                    Audios[0].Play();
                    Dons[0].SetTrigger("Hit");
                    combo_count++;
                    StopAllCoroutines();
                    StartCoroutine(SetCombo());
                }
                break;
            case GameSetting.KeyType.RightDon:
                {
                    Audios[0].Play();
                    Dons[1].SetTrigger("Hit");
                    combo_count++;
                    StopAllCoroutines();
                    StartCoroutine(SetCombo());
                }
                break;
            case GameSetting.KeyType.LeftKa:
                {
                    Audios[1].Play();
                    Kas[0].SetTrigger("Hit");
                    combo_count++;
                    StopAllCoroutines();
                    StartCoroutine(SetCombo());
                }
                break;
            case GameSetting.KeyType.RightKa:
                {
                    Audios[1].Play();
                    Kas[1].SetTrigger("Hit");
                    combo_count++;
                    StopAllCoroutines();
                    StartCoroutine(SetCombo());
                }
                break;
            case GameSetting.KeyType.Escape:
                StartCoroutine(LoadMain());
                break;
        }
    }

    void Update()
    {
        if (InputSystem.settings.updateMode == InputSettings.UpdateMode.ProcessEventsInDynamicUpdate)
        {
            frames++;
            var timeNow = Time.realtimeSinceStartup;
            if (timeNow > lastInterval + updateInterval)
            {
                float fps = frames / (timeNow - lastInterval);
                Rate.text = fps.ToString("#0.0");
                frames = 0;
                lastInterval = timeNow;
            }
        }


    }

    private void FixedUpdate()
    {
        if (!GameSetting.Config.DirectInput && InputSystem.settings.updateMode ==  InputSettings.UpdateMode.ProcessEventsInFixedUpdate)
        {
            frames++;
            var timeNow = Time.realtimeSinceStartup;
            if (timeNow > lastInterval + updateInterval)
            {
                float fps = frames / (timeNow - lastInterval);
                Rate.text = fps.ToString("#0.0");
                frames = 0;
                lastInterval = timeNow;
            }
        }
    }

    void Quit(CallbackContext context)
    {
        if (!Application.isFocused || quit) return;
        StartCoroutine(LoadMain());
    }

    IEnumerator LoadMain()
    {
        quit = true;
        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Cancel);
        yield return new WaitForSeconds(0.3f);

        AsyncOperation async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("MainScreen");
        async.allowSceneActivation = true;
        yield return async;
    }

    public void SetResolution()
    {
        GameSetting.SetResolution(resolutions[Resolutions.value].Key, resolutions[Resolutions.value].Value);
        Screen.SetResolution(resolutions[Resolutions.value].Key, resolutions[Resolutions.value].Value, GameSetting.Config.FullScreen);
    }

    public void SetFullScreen()
    {
        GameSetting.SetFullScreen(Full.isOn);
        Screen.fullScreen = Full.isOn;
    }

    public void SetMusic()
    {
        GameSetting.SetAutoRectify(Music.isOn);
    }

    public void SetKeyRefresh()
    {
        EventSystem.current.SetSelectedGameObject(null);
        int value = (int)Refresh.value * 10;
        //Time.fixedDeltaTime = 1 / (Refresh.value * 10);
        InputSystem.pollingFrequency = value;
        RefreshValue.text = (Refresh.value * 10).ToString();
    }

    IEnumerator SetCombo()
    {
        Combo.SetCombo(combo_count);
        yield return new WaitForSeconds(3);
        combo_count = 0;
        Combo.SetCombo(0);
    }

    public void SetVSync()
    {
        GameSetting.SetVSync(VSync.isOn);
        QualitySettings.vSyncCount = VSync.isOn ? 1 : 0;
    }

    public void ChangeInputOption()
    {
        InputDescri.text = GameSetting.Translate(input_des[Input.value]);
        if (Input.value == 0)
        {
            InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;
            Time.fixedDeltaTime = 0.02f;
            GameSetting.Config.InputMode = 1;
        }
        else
        {
            InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsInFixedUpdate;
            Time.fixedDeltaTime = 0.005f;
        }
        GameSetting.Config.InputMode = (int)InputSystem.settings.updateMode;
        GameSetting.SaveConfig();
        frames = 0;
    }

    public void ChangeStyleOption()
    {
        GameSetting.SetStyle(Style.value);
    }

    public void ChangeLang()
    {
        GameSetting.ChangeLanguage((Locale)Enum.Parse(typeof(Locale), lang[Language.value]));
        ResetLanguage?.Invoke();

        ReLoadConfig();
        SettingLoader.LoadPics();
    }

    private void ReLoadConfig()
    {
        List<string> input_options = new List<string>();
        foreach (string str in this.input_options)
            input_options.Add(GameSetting.Translate(str));
        for (int i = 0; i < Input.options.Count; i++)
            Input.options[i].text = input_options[i];
        Input.captionText.text = input_options[Input.value];

        int choice = InputSystem.settings.updateMode == InputSettings.UpdateMode.ProcessEventsInDynamicUpdate ? 0 : 1;
        InputDescri.text = GameSetting.Translate(input_des[choice]);

        List<string> styles = new List<string>();
        foreach (string str in this.styles)
            styles.Add(GameSetting.Translate(str));
        for (int i = 0; i < Style.options.Count; i++)
            Style.options[i].text = styles[i];
        Style.captionText.text = styles[Style.value];

        List<string> langs = new List<string>();
        foreach (string str in lang)
            langs.Add(GameSetting.Translate(str));
        for (int i = 0; i < Language.options.Count; i++)
            Language.options[i].text = langs[i];
        Language.captionText.text = langs[Language.value];
    }

    public void SetOutline()
    {
        GameSetting.Config.Outline = Outline.isOn;
        GameSetting.SaveConfig();
    }
}
