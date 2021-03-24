using CommonClass;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputAction;
using PlayMode = CommonClass.PlayMode;
using Score = CommonClass.Score;

public class GameModeScript : MonoBehaviour
{
    public static event PlaySelectedSoundDelegate PlaySelectedSound;

    public SettingButtonScript[] Buttons;
    public Text Description;
    public SongDiffSelectScript DiffSelect;
    public Text[] Options;

    protected bool inputing;
    protected int index;

    private readonly Dictionary<int, string> desecription = new Dictionary<int, string>
    {
        { 0, GameSetting.Translate("change game mode") },
        { 1, GameSetting.Translate("change score type") },
        { 2, GameSetting.Translate("change the movement speed of notes") },
        { 3, GameSetting.Translate("the notes 'don' and 'ka' randomly change") },
        { 4, GameSetting.Translate("hide notes") },
        { 5, GameSetting.Translate("reverse the notes 'don' and 'ka'") },
    };

    private readonly Dictionary<Special, string> specials = new Dictionary<Special, string>
    {
        { Special.None, GameSetting.Translate("change the special option") },
        { Special.AutoPlay, GameSetting.Translate("set auto play") },
        { Special.AllPerfect, GameSetting.Translate("quit the game play when get Bad") },
        { Special.Training, GameSetting.Translate("restart when get Bad") },
        { Special.DAllPerfect, GameSetting.Translate("quit the game play when get no Perfect") },
        { Special.DTraining, GameSetting.Translate("restart when get no Perfect") },
    };

    private readonly Dictionary<PlayMode, string> mode_str = new Dictionary<PlayMode, string>
    {
        { PlayMode.Normal, GameSetting.Translate("play normally") },
        { PlayMode.Practice, GameSetting.Translate("practice") },
        { PlayMode.Replay, GameSetting.Translate("replay") },
#if !UNITY_ANDROID
        { PlayMode.PlayWithReplay, GameSetting.Translate("play with replay") },
#endif
    };

    protected readonly Dictionary<Score, string> score_str = new Dictionary<Score, string>
    {
        { Score.Normal, GameSetting.Translate("old AC type") },
        { Score.Shin, GameSetting.Translate("type shin") },
        { Score.Nijiiro, GameSetting.Translate("nijiiro type") },
    };

    protected readonly Dictionary<Speed, string> speed_str = new Dictionary<Speed, string>
    {
        { Speed.Normal, GameSetting.Translate("normal speed") },
        { Speed.potion1, GameSetting.Translate("1.1 speed") },
        { Speed.potion2, GameSetting.Translate("1.2 speed") },
        { Speed.potion3, GameSetting.Translate("1.3 speed") },
        { Speed.potion4, GameSetting.Translate("1.4 speed") },
        { Speed.potion5, GameSetting.Translate("1.5 speed") },
        { Speed.Double, GameSetting.Translate("double speed") },
        { Speed.Triple, GameSetting.Translate("triple speed") },
        { Speed.Quadruple, GameSetting.Translate("quadruple speed") },
    };

    protected readonly Dictionary<RandomType, string> random_str = new Dictionary<RandomType, string>
    {
        { RandomType.None, GameSetting.Translate("none") },
        { RandomType.Normal, GameSetting.Translate("less") },
        { RandomType.More, GameSetting.Translate("more") },
    };

#if !UNITY_ANDROID
    void Start()
    {
        Options[0].text = GameSetting.Translate("game mode");
        Options[1].text = GameSetting.Translate("score type");
        Options[2].text = GameSetting.Translate("note speed");
        Options[3].text = GameSetting.Translate("random");
        Options[4].text = GameSetting.Translate("stealth");
        Options[5].text = GameSetting.Translate("reverse");
        Options[6].text = GameSetting.Translate("special option");

        BasicInputScript.Input.Player.Option.performed += Press;
        BasicInputScript.Input.Player.Up.performed += Press;
        BasicInputScript.Input.Player.Down.performed += Press;
        BasicInputScript.Input.Player.Left.performed += Press;
        BasicInputScript.Input.Player.Right.performed += Press;
        BasicInputScript.Input.Player.LeftKa.performed += Press;
        BasicInputScript.Input.Player.RightKa.performed += Press;
        BasicInputScript.Input.Player.Enter.performed += Press;
        BasicInputScript.Input.Player.RightDon.performed += Press;

        InputKeyListningScript.RebindKeys += Rebingkeys;

    }
#endif
    void OnEnable()
    {
        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Option);
        StartCoroutine(Show());
        StartCoroutine(DelayValue());

        if (GameSetting.Config.DirectInput)
            BasicInputScript.KeyInvoke += DirectKey;
    }

    protected void PlaySound(StartSoundControllScript.SoundType type)
    {
        PlaySelectedSound?.Invoke(type);
    }

    void OnDestroy()
    {
        InputKeyListningScript.RebindKeys -= Rebingkeys;
        BasicInputScript.KeyInvoke -= DirectKey;
    }

    private void Rebingkeys()
    {
        BasicInputScript.Input.Player.Cancel.performed -= Press;
        BasicInputScript.Input.Player.Option.performed -= Press;
        BasicInputScript.Input.Player.RightDon.performed -= Press;

        BasicInputScript.Input.Player.Cancel.performed += Press;
        BasicInputScript.Input.Player.Option.performed += Press;
        BasicInputScript.Input.Player.RightDon.performed += Press;
    }

    IEnumerator DelayValue()
    {
        yield return new WaitForEndOfFrame();

        index = 0;
        Description.text = desecription[index];

        //mode
        Buttons[0].SetValue1(mode_str[GameSetting.Mode]);

        //score
        Buttons[1].SetValue1(score_str[(Score)GameSetting.Config.ScoreMode]);

        //speed
        Buttons[2].SetValue1(speed_str[GameSetting.Config.Speed]);

        //random
        Buttons[3].SetValue1(random_str[GameSetting.Config.Random]);

        //steal
        Buttons[4].BoolValue = GameSetting.Config.Steal;
        Buttons[4].SetValue1(GameSetting.Config.Steal ? GameSetting.Translate(Buttons[4].on_string) : GameSetting.Translate(Buttons[4].off_string));

        Buttons[5].BoolValue = GameSetting.Config.Reverse;
        Buttons[5].SetValue1(GameSetting.Config.Reverse ? GameSetting.Translate(Buttons[5].on_string) : GameSetting.Translate(Buttons[5].off_string));

        //special
        Buttons[6].SetValue1(GameSetting.Translate(GameSetting.Config.Special.ToString()));

#if !UNITY_ANDROID
        SetCurrent();
#endif
    }

    IEnumerator Show()
    {
        while (transform.localPosition.y < 0)
        {
            transform.localPosition += new Vector3(0, 70);
            yield return new WaitForSeconds(0.01f);
        }
        transform.localPosition = new Vector3(transform.localPosition.x, 0);
        inputing = true;
    }

    void OnDisable()
    {
        inputing = false;
        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Option);
        transform.localPosition = new Vector3(0, -900);

        BasicInputScript.KeyInvoke -= DirectKey;
    }

    private void SetCurrent()
    {
        for (int i = 0; i < Buttons.Length; i++)
            Buttons[i].Selected = index == i;
    }

    private void DirectKey(GameSetting.KeyType key, bool press)
    {
        if (!inputing) return;
        EventSystem.current.SetSelectedGameObject(null);

        if (key == GameSetting.KeyType.Confirm || key == GameSetting.KeyType.RightDon)
        {
            if (index == 4) SetSteal();
            if (index == 5) SetReverse();
        }
        else if (key == GameSetting.KeyType.Left && press)
        {
            switch (index)
            {
                case 0:
                    ChangeMode(false);
                    break;
                case 1:
                    ChangeScore(false);
                    break;
                case 2:
                    ChangeSpeed(false);
                    break;
                case 3:
                    ChangeRandom(false);
                    break;
                case 6:
                    ChangeSpecial(false);
                    break;
            }
        }
        else if (key == GameSetting.KeyType.Right && press)
        {
            switch (index)
            {
                case 0:
                    ChangeMode(true);
                    break;
                case 1:
                    ChangeScore(true);
                    break;
                case 2:
                    ChangeSpeed(true);
                    break;
                case 3:
                    ChangeRandom(true);
                    break;
                case 6:
                    ChangeSpecial(true);
                    break;
            }
        }
        else if (key == GameSetting.KeyType.Up)
        {
            if (index == 0)
                index = Buttons.Length - 1;
            else
                index--;

            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
            SetCurrent();
            if (index != 6)
                Description.text = desecription[index];
            else
                Description.text = specials[GameSetting.Config.Special];
        }
        else if (key == GameSetting.KeyType.Option || key == GameSetting.KeyType.Down)
        {
            if (index == Buttons.Length - 1)
                index = 0;
            else
                index++;

            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
            SetCurrent();
            if (index != 6)
                Description.text = desecription[index];
            else
                Description.text = specials[GameSetting.Config.Special];
        }
    }

    private void Press(CallbackContext context)
    {
        if (!Application.isFocused || !inputing) return;
        EventSystem.current.SetSelectedGameObject(null);

        InputAction path = context.action;

        if (path == BasicInputScript.Input.Player.Enter || path == BasicInputScript.Input.Player.RightDon)
        {
            if (index == 4) SetSteal();
            if (index == 5) SetReverse();
        }
        else if (path == BasicInputScript.Input.Player.Left || path == BasicInputScript.Input.Player.LeftKa)
        {
            switch (index)
            {
                case 0:
                    ChangeMode(false);
                    break;
                case 1:
                    ChangeScore(false);
                    break;
                case 2:
                    ChangeSpeed(false);
                    break;
                case 3:
                    ChangeRandom(false);
                    break;
                case 6:
                    ChangeSpecial(false);
                    break;
            }
        }
        else if (path == BasicInputScript.Input.Player.Right || path == BasicInputScript.Input.Player.RightKa)
        {
            switch (index)
            {
                case 0:
                    ChangeMode(true);
                    break;
                case 1:
                    ChangeScore(true);
                    break;
                case 2:
                    ChangeSpeed(true);
                    break;
                case 3:
                    ChangeRandom(true);
                    break;
                case 6:
                    ChangeSpecial(true);
                    break;
            }
        }
        else if (path == BasicInputScript.Input.Player.Up)
        {
            if (index == 0)
                index = Buttons.Length - 1;
            else
                index--;

            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
            SetCurrent();
            if (index != 6)
                Description.text = desecription[index];
            else
                Description.text = specials[GameSetting.Config.Special];
        }
        else if (path == BasicInputScript.Input.Player.Option || path == BasicInputScript.Input.Player.Down)
        {
            if (index == Buttons.Length - 1)
                index = 0;
            else
                index++;

            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
            SetCurrent();
            if (index != 6)
                Description.text = desecription[index];
            else
                Description.text = specials[GameSetting.Config.Special];
        }
    }

    public void ChangeMode(bool forward)
    {
        if (!inputing || GameSetting.Player2) return;
        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
        List<PlayMode> types = new List<PlayMode>(mode_str.Keys);
        int i = types.IndexOf(GameSetting.Mode);

        if (forward)
        {
            Buttons[0].RightClick();
            i++;
            if (i >= types.Count)
                i = 0;

        }
        else
        {
            Buttons[0].LeftClick();
            i--;
            if (i < 0)
                i = types.Count - 1;
        }

        GameSetting.Mode = types[i];
        Buttons[0].SetValue1(mode_str[GameSetting.Mode]);
        DiffSelect.RefreshModeMark();
    }

    public virtual void ChangeScore(bool forward)
    {
        if (!inputing) return;
        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
        List<Score> types = new List<Score>(score_str.Keys);
        int i = types.IndexOf((Score)GameSetting.Config.ScoreMode);

        if (forward)
        {
            Buttons[1].RightClick();
            i++;
            if (i >= types.Count)
                i = 0;
        }
        else
        {
            Buttons[1].LeftClick();
            i--;
            if (i < 0)
                i = types.Count - 1;
        }

        GameSetting.Config.ScoreMode = (int)types[i];
        GameSetting.SaveConfig();
        Buttons[1].SetValue1(score_str[(Score)GameSetting.Config.ScoreMode]);
        DiffSelect.RefreshModeMark();
    }

    public virtual void ChangeSpeed(bool forward)
    {
        if (!inputing) return;
        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
        List<Speed> types = new List<Speed>(speed_str.Keys);
        int i = types.IndexOf(GameSetting.Config.Speed);

        if (forward)
        {
            Buttons[2].RightClick();
            i++;
            if (i >= types.Count)
                i = 0;
        }
        else
        {
            Buttons[2].LeftClick();
            i--;
            if (i < 0)
                i = types.Count - 1;
        }

        GameSetting.Config.Speed = types[i];
        Buttons[2].SetValue1(speed_str[GameSetting.Config.Speed]);
        DiffSelect.RefreshModeMark();
    }

    public virtual void ChangeRandom(bool forward)
    {
        if (!inputing) return;
        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
        List<RandomType> types = new List<RandomType>(random_str.Keys);
        int i = types.IndexOf(GameSetting.Config.Random);

        if (forward)
        {
            Buttons[3].RightClick();
            i++;
            if (i >= types.Count)
                i = 0;
        }
        else
        {
            Buttons[3].LeftClick();
            i--;
            if (i < 0)
                i = types.Count - 1;
        }

        GameSetting.Config.Random = types[i];
        Buttons[3].SetValue1(random_str[GameSetting.Config.Random]);
        DiffSelect.RefreshModeMark();
    }

    public virtual void SetSteal()
    {
        if (!inputing) return;
        EventSystem.current.SetSelectedGameObject(null);
        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Don);
        GameSetting.Config.Steal = (!GameSetting.Config.Steal);

        Buttons[4].OnClick();
        DiffSelect.RefreshModeMark();
    }
    public virtual void SetReverse()
    {
        if (!inputing) return;
        EventSystem.current.SetSelectedGameObject(null);
        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Don);
        GameSetting.Config.Reverse = (!GameSetting.Config.Reverse);

        Buttons[5].OnClick();
        DiffSelect.RefreshModeMark();
    }

    public virtual void ChangeSpecial(bool forward)
    {
        if (!inputing) return;
        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
        List<Special> types = new List<Special>(specials.Keys);
        int i = types.IndexOf(GameSetting.Config.Special);

        if (forward)
        {
            Buttons[6].RightClick();
            i++;
            if (i >= types.Count)
                i = 0;
        }
        else
        {
            Buttons[6].LeftClick();
            i--;
            if (i < 0)
                i = types.Count - 1;
        }

        GameSetting.SetSpecialOption(types[i]);
        Buttons[6].SetValue1(GameSetting.Translate(GameSetting.Config.Special.ToString()));
        DiffSelect.RefreshModeMark();
        Description.text = specials[GameSetting.Config.Special];
    }
}
