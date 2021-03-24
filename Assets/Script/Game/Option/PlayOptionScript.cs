using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputAction;

public class PlayOptionScript : MonoBehaviour
{
    public static event PlaySelectedSoundDelegate PlaySelectedSound;

    public SettingButtonScript[] Buttons;

    public AudioSource Audio;
    public PlayNoteScript PlayNote;
    public InputScript Input;
    public Text[] ButtonTexts;
    public Text Lyrics;
    public GloableAniControllerScript GloableAni;

    private bool inputing;
    private int index;

    void Start()
    {
        ButtonTexts[0].text = GameSetting.Translate("resume");
        ButtonTexts[1].text = GameSetting.Translate("restart");
        ButtonTexts[2].text = GameSetting.Translate("quit play");

        BasicInputScript.Input.Player.Option.performed += Press;
        BasicInputScript.Input.Player.Up.performed += Press;
        BasicInputScript.Input.Player.Down.performed += Press;
        BasicInputScript.Input.Player.Left.performed += Press;
        BasicInputScript.Input.Player.Right.performed += Press;
        BasicInputScript.Input.Player.LeftKa.performed += Press;
        BasicInputScript.Input.Player.RightKa.performed += Press;
        BasicInputScript.Input.Player.Enter.performed += Press;
        BasicInputScript.Input.Player.RightDon.performed += Press;
    }

    void OnEnable()
    {
        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Option);
        if (Lyrics != null)
            Lyrics.gameObject.SetActive(false);
        StartCoroutine(Show());
        StartCoroutine(DelayValue());

        if (GameSetting.Config.DirectInput)
            BasicInputScript.KeyInvoke += DirectKey;
    }

    void OnDestroy()
    {
        BasicInputScript.KeyInvoke -= DirectKey;
    }

    IEnumerator DelayValue()
    {
        yield return new WaitForEndOfFrame();
        index = 0;
        SetCurrent();
    }

    IEnumerator Show()
    {
        while (transform.localPosition.y < -2.75f)
        {
            transform.localPosition += new Vector3(0, 0.5f);
            yield return new WaitForSecondsRealtime(0.01f);
        }
        transform.localPosition = new Vector3(0, -2.75f, -9);

        inputing = true;
    }

    private void OnDisable()
    {
        inputing = false;
        if (Lyrics != null)
            Lyrics.gameObject.SetActive(true);
        transform.localPosition = new Vector3(0, -9, -9);

        BasicInputScript.KeyInvoke -= DirectKey;
    }

    private void SetCurrent()
    {
#if !UNITY_ANDROID
        for (int i = 0; i < Buttons.Length; i++)
            Buttons[i].Selected = index == i;
#endif
    }

    private void DirectKey(GameSetting.KeyType key, bool press)
    {
        if (!inputing) return;

        EventSystem.current.SetSelectedGameObject(null);
        if (key == GameSetting.KeyType.Confirm || key == GameSetting.KeyType.RightDon)
        {
            switch (index)
            {
                case 0:
                    Resume();
                    break;
                case 1:
                    PlayRestart();
                    break;
                case 2:
                    QuitGame();
                    break;
            }
        }
        else if ((key == GameSetting.KeyType.Up || key == GameSetting.KeyType.Left) && press)
        {
            if (index == 0)
                index = Buttons.Length - 1;
            else
                index--;

            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
            SetCurrent();
        }
        else if ((key == GameSetting.KeyType.Down || key == GameSetting.KeyType.Right || key == GameSetting.KeyType.Option) && press)
        {
            if (index == Buttons.Length - 1)
                index = 0;
            else
                index++;

            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
            SetCurrent();
        }
    }

    private void Press(CallbackContext context)
    {
        if (!Application.isFocused || !inputing) return;
        EventSystem.current.SetSelectedGameObject(null);
        InputAction path = context.action;
        if (path == BasicInputScript.Input.Player.Enter || path == BasicInputScript.Input.Player.RightDon)
        {
            switch (index)
            {
                case 0:
                    Resume();
                    break;
                case 1:
                    PlayRestart();
                    break;
                case 2:
                    QuitGame();
                    break;
            }
        }
        else if (path == BasicInputScript.Input.Player.Left || path == BasicInputScript.Input.Player.Up || path == BasicInputScript.Input.Player.LeftKa)
        {
            if (index == 0)
                index = Buttons.Length - 1;
            else
                index--;

            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
            SetCurrent();
        }
        else if (path == BasicInputScript.Input.Player.Option || path == BasicInputScript.Input.Player.Down
            || path == BasicInputScript.Input.Player.Right || path == BasicInputScript.Input.Player.RightKa)
        {
            if (index == Buttons.Length - 1)
                index = 0;
            else
                index++;

            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
            SetCurrent();
        }
    }

    public void Resume()
    {
        if (!inputing) return;
        EventSystem.current.SetSelectedGameObject(null);
        Input.CloseOption();
    }

    public void QuitGame()
    {
        if (!inputing) return;
        EventSystem.current.SetSelectedGameObject(null);
        inputing = false;
        Input.QuitGame();
    }

    public void PlayRestart()
    {
        EventSystem.current.SetSelectedGameObject(null);

        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Don);
        Buttons[2].OnClick();
        InputScript.Holding = InputScript.HoldStatus.None;
        GloableAni.Restart(0);
        gameObject.SetActive(false);
    }
}
