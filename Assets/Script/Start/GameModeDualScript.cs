using CommonClass;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class GameModeDualScript : GameModeScript
{
    public SettingButtonScript[] Buttons2;

    private int index_2p;

    private readonly Dictionary<Special, string> specials = new Dictionary<Special, string>
    {
        { Special.None, GameSetting.Translate("change the special option") },
        { Special.AutoPlay, GameSetting.Translate("set auto play") },
    };
    void Start()
    {
        Options[0].text = GameSetting.Translate("score type");
        Options[1].text = Options[6].text = GameSetting.Translate("note speed");
        Options[2].text = Options[7].text = GameSetting.Translate("random");
        Options[3].text = Options[8].text = GameSetting.Translate("stealth");
        Options[4].text = Options[9].text = GameSetting.Translate("reverse");
        Options[5].text = Options[10].text = GameSetting.Translate("special option");

        BasicInputScript.Input.Player.Option.performed += Press;
        BasicInputScript.Input.Player.Up.performed += Press;
        BasicInputScript.Input.Player.Down.performed += Press;
        BasicInputScript.Input.Player.Left.performed += Press;
        BasicInputScript.Input.Player.Right.performed += Press;
        BasicInputScript.Input.Player.LeftKa.performed += Press;
        BasicInputScript.Input.Player.RightKa.performed += Press;
        BasicInputScript.Input.Player.Enter.performed += Press;
        BasicInputScript.Input.Player.RightDon.performed += Press;

        if (BasicInputScript.Input.Player.Right2P != null)
            BasicInputScript.Input.Player.Right2P.performed += Press;
        if (BasicInputScript.Input.Player.Left2P != null)
            BasicInputScript.Input.Player.Left2P.performed += Press;
        if (BasicInputScript.Input.Player.Confirm2P != null)
            BasicInputScript.Input.Player.Confirm2P.performed += Press;

        InputKeyListningScript.RebindKeys += Rebingkeys;
    }

    private void Rebingkeys()
    {
        BasicInputScript.Input.Player.Cancel.performed -= Press;
        BasicInputScript.Input.Player.Option.performed -= Press;
        BasicInputScript.Input.Player.RightDon.performed -= Press;

        BasicInputScript.Input.Player.Right2P.performed -= Press;
        BasicInputScript.Input.Player.Left2P.performed -= Press;
        BasicInputScript.Input.Player.Confirm2P.performed -= Press;

        BasicInputScript.Input.Player.Cancel.performed += Press;
        BasicInputScript.Input.Player.Option.performed += Press;
        BasicInputScript.Input.Player.RightDon.performed += Press;

        if (BasicInputScript.Input.Player.Right2P != null)
            BasicInputScript.Input.Player.Right2P.performed += Press;
        if (BasicInputScript.Input.Player.Left2P != null)
            BasicInputScript.Input.Player.Left2P.performed += Press;
        if (BasicInputScript.Input.Player.Confirm2P != null)
            BasicInputScript.Input.Player.Confirm2P.performed += Press;
    }

    void OnDisable()
    {
        inputing = false;
        PlaySound(StartSoundControllScript.SoundType.Option);
        transform.localPosition = new Vector3(0, -900);

        BasicInputScript.KeyInvoke -= DirectKey;
        BasicInputScript.KeyInvoke2P -= DirectKey2P;
    }

    void OnEnable()
    {
        PlaySound(StartSoundControllScript.SoundType.Option);
        StartCoroutine(Show());
        StartCoroutine(DelayValue());

        if (GameSetting.Config.DirectInput)
        {
            BasicInputScript.KeyInvoke += DirectKey;
            BasicInputScript.KeyInvoke2P += DirectKey2P;
        }
    }
    void OnDestroy()
    {
        InputKeyListningScript.RebindKeys -= Rebingkeys;
        BasicInputScript.KeyInvoke -= DirectKey;
        BasicInputScript.KeyInvoke2P -= DirectKey2P;
    }

    IEnumerator DelayValue()
    {
        yield return new WaitForEndOfFrame();

        index = index_2p = 0;

        //score
        Buttons[0].SetValue1(score_str[(Score)GameSetting.Config.ScoreMode]);
        //speed
        Buttons[1].SetValue1(speed_str[GameSetting.Config.Speed]);

        Buttons2[0].SetValue1(speed_str[GameSetting.Speed2P]);

        //random
        Buttons[2].SetValue1(random_str[GameSetting.Config.Random]);
        Buttons2[1].SetValue1(random_str[GameSetting.Random2P]);
        //steal
        Buttons[3].BoolValue = GameSetting.Config.Steal;
        Buttons[3].SetValue1(GameSetting.Config.Steal ? GameSetting.Translate(Buttons[3].on_string) : GameSetting.Translate(Buttons[3].off_string));

        Buttons2[2].BoolValue = GameSetting.Steal2P;
        Buttons2[2].SetValue1(GameSetting.Steal2P ? GameSetting.Translate(Buttons2[2].on_string) : GameSetting.Translate(Buttons2[2].off_string));

        Buttons[4].BoolValue = GameSetting.Config.Reverse;
        Buttons[4].SetValue1(GameSetting.Config.Reverse ? GameSetting.Translate(Buttons[4].on_string) : GameSetting.Translate(Buttons[4].off_string));

        Buttons2[3].BoolValue = GameSetting.Revers2P;
        Buttons2[3].SetValue1(GameSetting.Revers2P ? GameSetting.Translate(Buttons2[3].on_string) : GameSetting.Translate(Buttons2[3].off_string));

        //special
        Buttons[5].SetValue1(GameSetting.Translate(GameSetting.Config.Special.ToString()));
        Buttons2[4].SetValue1(GameSetting.Translate(GameSetting.Special2P.ToString()));

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

    private void SetCurrent()
    {
        for (int i = 0; i < Buttons.Length; i++)
            Buttons[i].Selected = index == i;

        for (int i = 0; i < Buttons2.Length; i++)
            Buttons2[i].Selected = index_2p == i;
    }

    private void DirectKey(GameSetting.KeyType key, bool press)
    {
        if (!inputing) return;
        EventSystem.current.SetSelectedGameObject(null);

        if (key == GameSetting.KeyType.Confirm || key == GameSetting.KeyType.RightDon)
        {
            if (index == 3) SetSteal();
            if (index == 4) SetReverse();
        }
        else if (key == GameSetting.KeyType.Left && press)
        {
            switch (index)
            {
                case 0:
                    ChangeScore(false);
                    break;
                case 1:
                    ChangeSpeed(false);
                    break;
                case 2:
                    ChangeRandom(false);
                    break;
                case 5:
                    ChangeSpecial(false);
                    break;
            }
        }
        else if (key == GameSetting.KeyType.Right && press)
        {
            switch (index)
            {
                case 0:
                    ChangeScore(true);
                    break;
                case 1:
                    ChangeSpeed(true);
                    break;
                case 2:
                    ChangeRandom(true);
                    break;
                case 5:
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

            PlaySound(StartSoundControllScript.SoundType.Ka);
            SetCurrent();
        }
        else if (key == GameSetting.KeyType.Option || key == GameSetting.KeyType.Down)
        {
            if (index == Buttons.Length - 1)
                index = 0;
            else
                index++;

            PlaySound(StartSoundControllScript.SoundType.Ka);
            SetCurrent();
        }
    }

    private void DirectKey2P(GameSetting.KeyType key, bool press)
    {
        if (!inputing) return;
        if (key == GameSetting.KeyType.RightDon)
        {
            switch (index_2p)
            {
                case 0:
                    ChangeSpeed2P(true);
                    break;
                case 1:
                    ChangeRandom2P(true);
                    break;
                case 2:
                    SetSteal2P();
                    break;
                case 3:
                    SetReverse2P();
                    break;
                case 4:
                    ChangeSpecial2P(true);
                    break;
            }
        }
        else if (key == GameSetting.KeyType.LeftKa)
        {
            if (index_2p == 0)
                index_2p = Buttons2.Length - 1;
            else
                index_2p--;

            PlaySound(StartSoundControllScript.SoundType.Ka);
            SetCurrent();
        }
        else if (key == GameSetting.KeyType.RightKa)
        {
            if (index_2p == Buttons2.Length - 1)
                index_2p = 0;
            else
                index_2p++;

            PlaySound(StartSoundControllScript.SoundType.Ka);
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
            if (index == 3) SetSteal();
            if (index == 4) SetReverse();
        }
        else if (path == BasicInputScript.Input.Player.Left || path == BasicInputScript.Input.Player.LeftKa)
        {
            switch (index)
            {
                case 0:
                    ChangeScore(false);
                    break;
                case 1:
                    ChangeSpeed(false);
                    break;
                case 2:
                    ChangeRandom(false);
                    break;
                case 5:
                    ChangeSpecial(false);
                    break;
            }
        }
        else if (path == BasicInputScript.Input.Player.Right || path == BasicInputScript.Input.Player.RightKa)
        {
            switch (index)
            {
                case 0:
                    ChangeScore(true);
                    break;
                case 1:
                    ChangeSpeed(true);
                    break;
                case 2:
                    ChangeRandom(true);
                    break;
                case 5:
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

            PlaySound(StartSoundControllScript.SoundType.Ka);
            SetCurrent();
        }
        else if (path == BasicInputScript.Input.Player.Option || path == BasicInputScript.Input.Player.Down)
        {
            if (index == Buttons.Length - 1)
                index = 0;
            else
                index++;

            PlaySound(StartSoundControllScript.SoundType.Ka);
            SetCurrent();
        }
        else if (path == BasicInputScript.Input.Player.Left2P)
        {
            if (index_2p == 0)
                index = Buttons2.Length - 1;
            else
                index_2p--;

            PlaySound(StartSoundControllScript.SoundType.Ka);
            SetCurrent();
        }
        else if (path == BasicInputScript.Input.Player.Right2P)
        {
            if (index_2p == Buttons2.Length - 1)
                index_2p = 0;
            else
                index_2p++;

            PlaySound(StartSoundControllScript.SoundType.Ka);
            SetCurrent();
        }
        else if (path == BasicInputScript.Input.Player.Confirm2P)
        {
            switch (index_2p)
            {
                case 0:
                    ChangeSpeed2P(true);
                    break;
                case 1:
                    ChangeRandom2P(true);
                    break;
                case 2:
                    SetSteal2P();
                    break;
                case 3:
                    SetReverse2P();
                    break;
                case 4:
                    ChangeSpecial2P(true);
                    break;
            }
        }
    }

    public override void ChangeScore(bool forward)
    {
        if (!inputing) return;
        PlaySound(StartSoundControllScript.SoundType.Ka);
        List<Score> types = new List<Score>(score_str.Keys);
        int i = types.IndexOf((Score)GameSetting.Config.ScoreMode);

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

        GameSetting.Config.ScoreMode = (int)types[i];
        GameSetting.SaveConfig();
        Buttons[0].SetValue1(score_str[(Score)GameSetting.Config.ScoreMode]);
        DiffSelect.RefreshModeMark();
    }

    public override void ChangeSpeed(bool forward)
    {
        if (!inputing) return;
        PlaySound(StartSoundControllScript.SoundType.Ka);
        List<Speed> types = new List<Speed>(speed_str.Keys);
        int i = types.IndexOf(GameSetting.Config.Speed);

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

        GameSetting.Config.Speed = types[i];
        Buttons[1].SetValue1(speed_str[GameSetting.Config.Speed]);
        DiffSelect.RefreshModeMark();
    }

    public override void ChangeRandom(bool forward)
    {
        if (!inputing) return;
        PlaySound(StartSoundControllScript.SoundType.Ka);
        List<RandomType> types = new List<RandomType>(random_str.Keys);
        int i = types.IndexOf(GameSetting.Config.Random);

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

        GameSetting.Config.Random = types[i];
        Buttons[2].SetValue1(random_str[GameSetting.Config.Random]);
        DiffSelect.RefreshModeMark();
    }

    public override void SetSteal()
    {
        if (!inputing) return;
        EventSystem.current.SetSelectedGameObject(null);
        PlaySound(StartSoundControllScript.SoundType.Don);
        GameSetting.Config.Steal = (!GameSetting.Config.Steal);

        Buttons[3].OnClick();
        DiffSelect.RefreshModeMark();
    }
    public override void SetReverse()
    {
        if (!inputing) return;
        EventSystem.current.SetSelectedGameObject(null);
        PlaySound(StartSoundControllScript.SoundType.Don);
        GameSetting.Config.Reverse = (!GameSetting.Config.Reverse);

        Buttons[4].OnClick();
        DiffSelect.RefreshModeMark();
    }

    public override void ChangeSpecial(bool forward)
    {
        if (!inputing) return;
        PlaySound(StartSoundControllScript.SoundType.Ka);
        List<Special> types = new List<Special>(specials.Keys);
        int i = types.IndexOf(GameSetting.Config.Special);

        if (forward)
        {
            Buttons[5].RightClick();
            i++;
            if (i >= types.Count)
                i = 0;
        }
        else
        {
            Buttons[5].LeftClick();
            i--;
            if (i < 0)
                i = types.Count - 1;
        }

        GameSetting.SetSpecialOption(types[i]);
        Buttons[5].SetValue1(GameSetting.Translate(GameSetting.Config.Special.ToString()));
        DiffSelect.RefreshModeMark();
    }

    private void ChangeSpeed2P(bool forward)
    {
        if (!inputing) return;
        PlaySound(StartSoundControllScript.SoundType.Ka);
        List<Speed> types = new List<Speed>(speed_str.Keys);
        int i = types.IndexOf(GameSetting.Speed2P);

        if (forward)
        {
            Buttons2[0].RightClick();
            i++;
            if (i >= types.Count)
                i = 0;
        }
        else
        {
            Buttons2[0].LeftClick();
            i--;
            if (i < 0)
                i = types.Count - 1;
        }

        GameSetting.Speed2P = types[i];
        Buttons2[0].SetValue1(speed_str[GameSetting.Speed2P]);
        DiffSelect.RefreshModeMark();
    }

    private void ChangeRandom2P(bool forward)
    {
        if (!inputing) return;
        PlaySound(StartSoundControllScript.SoundType.Ka);
        List<RandomType> types = new List<RandomType>(random_str.Keys);
        int i = types.IndexOf(GameSetting.Random2P);

        if (forward)
        {
            Buttons2[1].RightClick();
            i++;
            if (i >= types.Count)
                i = 0;
        }
        else
        {
            Buttons2[1].LeftClick();
            i--;
            if (i < 0)
                i = types.Count - 1;
        }

        GameSetting.Random2P = types[i];
        Buttons2[1].SetValue1(random_str[GameSetting.Random2P]);
        DiffSelect.RefreshModeMark();
    }

    private void SetSteal2P()
    {
        if (!inputing) return;
        EventSystem.current.SetSelectedGameObject(null);
        PlaySound(StartSoundControllScript.SoundType.Don);
        GameSetting.Steal2P = !GameSetting.Steal2P;

        Buttons2[2].OnClick();
        DiffSelect.RefreshModeMark();
    }
    private void SetReverse2P()
    {
        if (!inputing) return;
        EventSystem.current.SetSelectedGameObject(null);
        PlaySound(StartSoundControllScript.SoundType.Don);
        GameSetting.Revers2P = (!GameSetting.Revers2P);

        Buttons2[3].OnClick();
        DiffSelect.RefreshModeMark();
    }

    private void ChangeSpecial2P(bool forward)
    {
        if (!inputing) return;
        PlaySound(StartSoundControllScript.SoundType.Ka);
        List<Special> types = new List<Special>(specials.Keys);
        int i = types.IndexOf(GameSetting.Special2P);

        if (forward)
        {
            Buttons2[4].RightClick();
            i++;
            if (i >= types.Count)
                i = 0;
        }
        else
        {
            Buttons2[4].LeftClick();
            i--;
            if (i < 0)
                i = types.Count - 1;
        }

        GameSetting.Special2P = types[i];
        Buttons2[4].SetValue1(GameSetting.Translate(GameSetting.Special2P.ToString()));
        DiffSelect.RefreshModeMark();
    }
}
