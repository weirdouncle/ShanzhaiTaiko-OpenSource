using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class MainSceenScript : MonoBehaviour
{
    public static event PlaySelectedSoundDelegate PlaySelectedSound;

    public MainButtonScript[] Buttons;
    public CriAtomSource Audio;

    private int index = 0;
    private bool accept = true;
    private AsyncOperation async;
    void Start()
    {
#if !UNITY_ANDROID
        Buttons[0].Selected = true;

        BasicInputScript.Input.Player.Up.performed += ClickMove;
        BasicInputScript.Input.Player.Down.performed += ClickMove;
        BasicInputScript.Input.Player.Enter.performed += ClickMove;
        BasicInputScript.Input.Player.RightDon.performed += ClickMove;

        if (GameSetting.KaLeft != null)
            GameSetting.KaLeft.performed += ClickMove2;
        if (GameSetting.KaRight != null)
            GameSetting.KaRight.performed += ClickMove2;

        StartCoroutine(DelayInput());
#endif
    }

    IEnumerator DelayInput()
    {
        yield return new WaitForEndOfFrame();
        if (GameSetting.Config.DirectInput)
            BasicInputScript.KeyInvoke += DirectInput;
    }

#if !UNITY_ANDROID
    private void OnDestroy()
    {
        if (GameSetting.KaLeft != null)
            GameSetting.KaLeft.performed -= ClickMove2;
        if (GameSetting.KaRight != null)
            GameSetting.KaRight.performed -= ClickMove2;

        BasicInputScript.KeyInvoke -= DirectInput;
    }
#endif

    private void DirectInput(GameSetting.KeyType key, bool pressed)
    {
        if (!accept) return;
        switch (key)
        {
            case GameSetting.KeyType.Confirm:
                OnClick(index);
                break;
            case GameSetting.KeyType.Up:
                Move(true);
                break;
            case GameSetting.KeyType.Down:
                Move(false);
                break;
        }
    }

    public void OnHavorIn(int index)
    {
        if (!accept || this.index == index) return;
        PlaySound(StartSoundControllScript.SoundType.Ka);
        this.index = index;
        foreach (MainButtonScript button in Buttons)
            button.Selected = button.Index == index;
    }

    private bool audio_freeze;
    protected void PlaySound(StartSoundControllScript.SoundType type)
    {
        if (!audio_freeze)
        {
            StartCoroutine("AudioDelay");
            PlaySelectedSound?.Invoke(type);
        }
    }

    IEnumerator AudioDelay()
    {
        audio_freeze = true;
        yield return new WaitForSeconds(0.1f);
        audio_freeze = false;
    }


    public void ClickButtom()
    {
        OnClick(index);
    }

    public void OnClick(int index)
    {
        if (index == 4)
            Application.Quit();
        else
        {
            if (accept)
            {
                accept = false;
                string scene;
                switch (index)
                {
                    case 2:
                        scene = "CosRoom";
                        break;
                    case 3:
                        scene = "Resolution";
                        break;
                    default:
                        scene = "SongSelect";
                        break;
                }
                Buttons[index].Click();
                StartCoroutine(Load(scene));
            }
        }
    }

    IEnumerator Load(string scene)
    {
        if (pad != null)
            pad.SetMotorSpeeds(0.8f, 0.8f);

        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Don);
        yield return new WaitForSeconds(0.3f);

#if !UNITY_ANDROID
        async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scene);

        if (pad != null)
            pad.ResetHaptics();

        async.allowSceneActivation = true;
        yield return async;

#else
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
#endif
    }

    Gamepad pad;

    private void ClickMove2(CallbackContext context)
    {
        if (!Application.isFocused) return;
        if (context.action == GameSetting.KaLeft && context.control.device == Keyboard.current)
            Move(true);
        else if (context.action == GameSetting.KaRight && context.control.device == Keyboard.current)
            Move(false);
    }

    private void ClickMove(CallbackContext context)
    {
        if (!Application.isFocused) return;
        if (context.action == BasicInputScript.Input.Player.Enter || context.action == BasicInputScript.Input.Player.RightDon)
        {
            if (context.control.device is Gamepad pad)
                this.pad = pad;
            else
                this.pad = null;

            OnClick(index);
        }
        else if ((context.action == GameSetting.KaLeft && context.control.device == Keyboard.current) || context.action == BasicInputScript.Input.Player.Up)
            Move(true);
        else if ((context.action == GameSetting.KaRight && context.control.device == Keyboard.current) || context.action == BasicInputScript.Input.Player.Down)
            Move(false);
    }
    
    public void Move(bool up)
    {
        if (up)
        {
            int index = this.index - 1;
            if (index < 0) index = Buttons.Length - 1;
            OnHavorIn(index);
        }
        else
        {
            int index = this.index + 1;
            if (index > Buttons.Length - 1) index = 0;
            OnHavorIn(index);
        }
    }
}
