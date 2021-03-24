using UnityEngine;
using UnityEngine.UI;
using CommonClass;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.InputSystem;
using System;

public class GameConfigScript : MonoBehaviour
{
    public static event PlaySelectedSoundDelegate PlaySelectedSound;

    public InputKeyListningScript ButtonInput;
    public Animator Controll;
    public Animator Animator;
    public SettingButtonScript[] Buttons;
    public Text Description;
    public CriAtomSource Audio;
    public Button NameConfirm;

    public Text[] Titles;
    public Text[] Options;
    public GameObject CloseButton;

    public static bool Setting;

    private Dictionary<int, string> descri = new Dictionary<int, string>
    {
        { 0, GameSetting.Translate("press enter to change or save and esc to resume") },
        { 1, GameSetting.Translate("press←→ or ‘ka’ to change judge time range") },
        { 2, GameSetting.Translate("press←→ or ‘ka’ to change judge time range") },
        { 3, GameSetting.Translate("press←→ or ‘ka’ to change judge time range") },
        { 4, GameSetting.Translate("press 'enter' to config page") },
        { 5, GameSetting.Translate("change drum sound or press enter to preview") },
        { 6, GameSetting.Translate("advance or delay the time of judging") },
        { 7, GameSetting.Translate("advance or delay the time notes move to the point") },
        { 8, GameSetting.Translate("adjust music volume") },
        { 9, GameSetting.Translate("adjust drum volume") },
    };
    private bool input_accept;
    private bool init;
    private bool changing_name;
    private int index = 0;
    private bool drum;

    void Start()
    {
        Titles[0].text = GameSetting.Translate("Game Config");

        //游戏设置
        Options[0].text = GameSetting.Translate("judge time(ms)");
        Options[1].text = GameSetting.Translate("perfect");
        Options[2].text = GameSetting.Translate("good");
        Options[3].text = GameSetting.Translate("bad");
        Options[4].text = GameSetting.Translate("remark: judging is looser in easy or normal");
        Options[5].text = GameSetting.Translate("key config");
        Options[6].text = GameSetting.Translate("press 'enter' to config page");
        Options[7].text = GameSetting.Translate("drum sound");
        Options[8].text = GameSetting.Translate("judge time adjust(ms)");
        Options[9].text = GameSetting.Translate("note position adjust(ms)");
        Options[10].text = GameSetting.Translate("player name");
        Options[11].text = GameSetting.Translate("music volume");
        Options[12].text = GameSetting.Translate("drum volume");
        Options[13].text = GameSetting.Translate("confirm");

#if !UNITY_ANDROID
        foreach (SettingButtonScript button in Buttons)
            button.MouseHavor += ChangeSelected;
#endif
        AnimatorFinishScript script = Controll.GetComponent<AnimatorFinishScript>();
        script.Finish += DelayResume;

        //名字
        Buttons[0].SetValue1(GameSetting.Config.PlayerName);
        //判定
        //良
        int value_perfect = GameSetting.Config.JudgeRange[HitNoteResult.Perfect];
        //可
        int value_good = GameSetting.Config.JudgeRange[HitNoteResult.Good];
        //不可
        int value_bad = GameSetting.Config.JudgeRange[HitNoteResult.Bad];

        Buttons[1].Slider.maxValue = value_good - 1;
        Buttons[3].Slider.minValue = value_good + 1;
        Buttons[2].Slider.minValue = value_perfect + 1;
        Buttons[2].Slider.maxValue = value_bad - 1;

        Buttons[1].SetSliderValue(value_perfect);
        Buttons[2].SetSliderValue(value_good);
        Buttons[3].SetSliderValue(value_bad);

#if !UNITY_ANDROID
        if (CloseButton != null) CloseButton.SetActive(false);

        //鼓声类型
        drum = true;
        int drum_type = GameSetting.Config.DrumSoundType;
        Buttons[5].SetValue1(drum_type.ToString());
        Buttons[5].IntValue = drum_type;
#else
        Buttons[4].gameObject.SetActive(false);
        Buttons[5].gameObject.SetActive(false);
#endif

        //判定调整
        int value_adujust = GameSetting.Config.JudgeTimeAdjust;
        Buttons[6].SetSliderValue(value_adujust);
        Buttons[6].SetValue1(value_adujust.ToString());
        Buttons[6].IntValue = value_adujust;
        //位置调整
        int value_note = GameSetting.Config.NoteAdjust;
        Buttons[7].SetSliderValue(value_note);
        Buttons[7].SetValue1(value_note.ToString());
        Buttons[7].IntValue = value_note;


        //volume
        float volume = GameSetting.Config.MusicVolume;
        Buttons[8].SetSliderValue((int)(10 * volume));

        volume = GameSetting.Config.EffectVolume;
        Buttons[9].SetSliderValue((int)(10 * volume));

        init = true;

        BasicInputScript.Input.Player.Esc.performed += Press;
#if !UNITY_ANDROID
        BasicInputScript.Input.Player.Up.performed += Press;
        BasicInputScript.Input.Player.Down.performed += Press;
        BasicInputScript.Input.Player.Left.performed += Press;
        BasicInputScript.Input.Player.Right.performed += Press;
        BasicInputScript.Input.Player.LeftKa.performed += Press;
        BasicInputScript.Input.Player.RightKa.performed += Press;
        BasicInputScript.Input.Player.Cancel.performed += Press;
        BasicInputScript.Input.Player.Enter.performed += Press;
        BasicInputScript.Input.Player.RightDon.performed += Press;
        BasicInputScript.Input.Player.Option.performed += Press;
        BasicInputScript.Input.Player.LeftRelease.performed += Release;
        BasicInputScript.Input.Player.RightRelease.performed += Release;
        BasicInputScript.Input.Player.LeftKaRelease.performed += Release;
        BasicInputScript.Input.Player.RightKaRelease.performed += Release;

        InputKeyListningScript.RebindKeys += Rebingkeys;
#endif
    }

    void OnDestroy()
    {
#if !UNITY_ANDROID
        foreach (SettingButtonScript button in Buttons)
            button.MouseHavor -= ChangeSelected;

        InputKeyListningScript.RebindKeys -= Rebingkeys;
#endif
        BasicInputScript.KeyInvoke -= DirectInput;

        AnimatorFinishScript script = Controll.GetComponent<AnimatorFinishScript>();
        script.Finish -= DelayResume;
    }

    void OnEnable()
    {
        if (GameSetting.Config.DirectInput)
            BasicInputScript.KeyInvoke += DirectInput;
    }

    void OnDisable()
    {
        BasicInputScript.KeyInvoke -= DirectInput;
    }

    private float time;
    void Update()
    {
        time += Time.deltaTime;
        if (time >= 1)
        {
            time = 0;
            GC.Collect();
        }
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

    private void Press(CallbackContext context)
    {
        if (!Application.isFocused || !Setting || !input_accept) return;
        StopAllCoroutines();

        if (changing_name)
        {
            if (context.action == BasicInputScript.Input.Player.Esc
                || (context.control.device != Keyboard.current && context.action == BasicInputScript.Input.Player.Cancel))
                SetPlayerName(false);
        }
        else
        {
            if (context.action == BasicInputScript.Input.Player.Enter || context.action == BasicInputScript.Input.Player.RightDon)
            {
                SetButton(index);
            }
            else if (context.action == BasicInputScript.Input.Player.Left || context.action == BasicInputScript.Input.Player.LeftKa)
            {
                SettingAdjust(false);
            }
            else if (context.action == BasicInputScript.Input.Player.Right || context.action == BasicInputScript.Input.Player.RightKa)
            {
                SettingAdjust(true);
            }
            else if (context.action == BasicInputScript.Input.Player.Up)
            {
                ChangeSelect(true);
            }
            else if (context.action == BasicInputScript.Input.Player.Option || context.action == BasicInputScript.Input.Player.Down)
            {
                ChangeSelect(false);
            }
            else if (context.action == BasicInputScript.Input.Player.Esc || context.action == BasicInputScript.Input.Player.Cancel)
                Show(false);
        }
    }

    private void DirectInput(GameSetting.KeyType key, bool pressed)
    {
        if (!Setting || !input_accept) return;
        StopAllCoroutines();

        if (changing_name)
        {
            if (key == GameSetting.KeyType.Escape)
                SetPlayerName(false);
        }
        else
        {
            if (key == GameSetting.KeyType.Confirm || key == GameSetting.KeyType.RightDon)
            {
                SetButton(index);
            }
            else if (key == GameSetting.KeyType.Left && pressed)
            {
                SettingAdjust(false);
            }
            else if (key == GameSetting.KeyType.Right && pressed)
            {
                SettingAdjust(true);
            }
            else if (key == GameSetting.KeyType.Left && !pressed)
            {
                ChangeSetting(true);
            }
            else if (key == GameSetting.KeyType.Right && !pressed)
            {
                ChangeSetting(false);
            }
            else if (key == GameSetting.KeyType.Up)
            {
                ChangeSelect(true);
            }
            else if (key == GameSetting.KeyType.Option || key == GameSetting.KeyType.Down)
            {
                ChangeSelect(false);
            }
            else if (key == GameSetting.KeyType.Escape || key == GameSetting.KeyType.Cancel)
                Show(false);
        }
    }

    private void Release(CallbackContext context)
    {
        if (!Application.isFocused || !Setting || !input_accept || changing_name) return;

        StopAllCoroutines();
        if (context.action == BasicInputScript.Input.Player.LeftRelease || context.action == BasicInputScript.Input.Player.LeftKaRelease)
        {
            ChangeSetting(true);
        }
        else
        {
            ChangeSetting(false);
        }
    }
    private void ChangeSelect(bool up)
    {
        if (up)
        {
            if (index > 0)
            {
                ChangeSelected(index - 1);
            }
            else
                ChangeSelected(Buttons.Length - 1);
        }
        else
        {
            if (index < Buttons.Length - 1)
            {
                ChangeSelected(index + 1);
            }
            else
                ChangeSelected(0);
        }
    }

    private void SettingAdjust(bool forward)
    {
        if (forward)
        {
            if (index >= 1 && index <= 3 && Buttons[index].IntValue + 1 <= Buttons[index].Slider.maxValue)
            {
                Buttons[index].Press(true, true);
                switch (index)
                {
                    case 1:
                        StartCoroutine(JudgeTimeAdjust(HitNoteResult.Perfect, true));
                        break;
                    case 2:
                        StartCoroutine(JudgeTimeAdjust(HitNoteResult.Good, true));
                        break;
                    case 3:
                        StartCoroutine(JudgeTimeAdjust(HitNoteResult.Bad, true));
                        break;
                }
            }
        }
        else
        {
            if (index >= 1 && index <= 3 && Buttons[index].IntValue - 1 >= Buttons[index].Slider.minValue)
            {
                Buttons[index].Press(false, true);
                switch (index)
                {
                    case 1:
                        StartCoroutine(JudgeTimeAdjust(HitNoteResult.Perfect, false));
                        break;
                    case 2:
                        StartCoroutine(JudgeTimeAdjust(HitNoteResult.Good, false));
                        break;
                    case 3:
                        StartCoroutine(JudgeTimeAdjust(HitNoteResult.Bad, false));
                        break;
                }
            }
        }
    }

    public void Show(bool show)
    {
        EventSystem.current.SetSelectedGameObject(null);
        if (SongScanScript.random_freeze) return;

        if (show && !Setting)
        {
            Setting = true;
            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Option);
            Controll.SetTrigger("Show");
            index = 0;
#if !UNITY_ANDROID
            for (int i = 0; i < Buttons.Length; i++)
            {
                Buttons[i].Selected = (i == index);
            }
#endif
            Description.text = descri[index];
        }
        else if (!show && Setting)
        {
            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Option);
            input_accept = false;
            StopAllCoroutines();
            Controll.SetTrigger("Hide");
        }
    }

    private void DelayResume(int type)
    {
        if (type == 0)
        {
            input_accept = true;
        }
        else
        {
            Setting = false;
        }
    }

    public void ChangeSelected(int index)
    {
        if (changing_name) return;
        EventSystem.current.SetSelectedGameObject(null);
        StopAllCoroutines();

        if (this.index != index)
        {
            this.index = index;
#if !UNITY_ANDROID
            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
            for (int i = 0; i < Buttons.Length; i++)
            {
                Buttons[i].Selected = (i == index);
            }
#endif
            Description.text = descri[index];
        }
    }

    private void SetButton(int index)
    {
        EventSystem.current.SetSelectedGameObject(null);
        StopAllCoroutines();

        switch (index)
        {
            case 0:
                PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Don);
                changing_name = true;
                Buttons[index].OnClick();
                NameConfirm.gameObject.SetActive(true);
                break;
            case 4:
                InterButtonSetup();
                break;
            case 5:
                PlayDrum();
                break;
        }
    }

    private void PlayDrum()
    {
        string cue_sheet = string.Empty;
        if (drum)
        {
            cue_sheet = "TaikoDon";
        }
        else
        {
            cue_sheet = "TaikoKa";
        }
        Audio.cueSheet = cue_sheet;
        Audio.Play(GameSetting.DrumTypes[GameSetting.Config.DrumSoundType]);
        drum = !drum;
    }

    IEnumerator JudgeTimeAdjust(HitNoteResult note, bool forward)
    {
        bool fast = false;
        float time = Time.time;

        switch (note)
        {
            case HitNoteResult.Perfect:
                {
                    if (!forward)
                    {
                            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                            Buttons[index].SetSliderValue(Buttons[index].IntValue - 1);
                            GameSetting.SetJudgeTime(HitNoteResult.Perfect, Buttons[index].IntValue);
                    }
                    else
                    {
                        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                        Buttons[index].SetSliderValue(Buttons[index].IntValue + 1);
                        GameSetting.SetJudgeTime(HitNoteResult.Perfect, Buttons[index].IntValue);
                    }
                    Buttons[2].Slider.minValue = Buttons[index].IntValue + 1;
                    Buttons[2].CheckLimit();
                }
                break;
            case HitNoteResult.Good:
                {
                    if (!forward)
                    {
                        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                        Buttons[index].SetSliderValue(Buttons[index].IntValue - 1);
                        GameSetting.SetJudgeTime(HitNoteResult.Good, Buttons[index].IntValue);
                    }
                    else
                    {
                        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                        Buttons[index].SetSliderValue(Buttons[index].IntValue + 1);
                        GameSetting.SetJudgeTime(HitNoteResult.Good, Buttons[index].IntValue);
                    }
                    Buttons[1].Slider.maxValue = Buttons[index].IntValue - 1;
                    Buttons[1].CheckLimit();
                    Buttons[3].Slider.minValue = Buttons[index].IntValue + 1;
                    Buttons[3].CheckLimit();
                }
                break;
            case HitNoteResult.Bad:
                {
                    if (!forward)
                    {
                        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                        Buttons[index].SetSliderValue(Buttons[index].IntValue - 1);
                        GameSetting.SetJudgeTime(HitNoteResult.Bad, Buttons[index].IntValue);
                    }
                    else
                    {
                        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                        Buttons[index].SetSliderValue(Buttons[index].IntValue + 1);
                        GameSetting.SetJudgeTime(HitNoteResult.Bad, Buttons[index].IntValue);
                    }
                    Buttons[2].Slider.maxValue = Buttons[index].IntValue - 1;
                    Buttons[2].CheckLimit();
                }
                break;
        }
        yield return new WaitForSeconds(0.3f);
        bool end = false;
        while (!end)
        {
            switch (note)
            {
                case HitNoteResult.Perfect:
                    {
                        if (!forward)
                        {
                            if (Buttons[index].IntValue > 0)
                            {
                                PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                                Buttons[index].SetSliderValue(Buttons[index].IntValue - 1);
                                GameSetting.SetJudgeTime(HitNoteResult.Perfect, Buttons[index].IntValue);
                            }
                            else
                                end = true;
                        }
                        else
                        {
                            if (Buttons[index].IntValue < Buttons[2].IntValue - 1)
                            {
                                PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                                Buttons[index].SetSliderValue(Buttons[index].IntValue + 1);
                                GameSetting.SetJudgeTime(HitNoteResult.Perfect, Buttons[index].IntValue);
                            }
                            else
                                end = true;
                        }
                        Buttons[2].Slider.minValue = Buttons[index].IntValue + 1;
                        Buttons[2].CheckLimit();
                    }
                    break;
                case HitNoteResult.Good:
                    {
                        if (!forward)
                        {
                            if (Buttons[index].IntValue > Buttons[1].IntValue + 1)
                            {
                                PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                                Buttons[index].SetSliderValue(Buttons[index].IntValue - 1);
                                GameSetting.SetJudgeTime(HitNoteResult.Good, Buttons[index].IntValue);
                            }
                            else
                                end = true;
                        }
                        else
                        {
                            if (Buttons[index].IntValue < Buttons[3].IntValue - 1)
                            {
                                PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                                Buttons[index].SetSliderValue(Buttons[index].IntValue + 1);
                                GameSetting.SetJudgeTime(HitNoteResult.Good, Buttons[index].IntValue);
                            }
                            else
                                end = true;
                        }
                        Buttons[1].Slider.maxValue = Buttons[index].IntValue - 1;
                        Buttons[1].CheckLimit();
                        Buttons[3].Slider.minValue = Buttons[index].IntValue + 1;
                        Buttons[3].CheckLimit();
                    }
                    break;
                case HitNoteResult.Bad:
                    {
                        if (!forward)
                        {
                            if (Buttons[index].IntValue > Buttons[2].IntValue + 1)
                            {
                                PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                                Buttons[index].SetSliderValue(Buttons[index].IntValue - 1);
                                GameSetting.SetJudgeTime(HitNoteResult.Bad, Buttons[index].IntValue);
                            }
                            else
                                end = true;
                        }
                        else
                        {
                            if (Buttons[index].IntValue < 999)
                            {
                                PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                                Buttons[index].SetSliderValue(Buttons[index].IntValue + 1);
                                GameSetting.SetJudgeTime(HitNoteResult.Bad, Buttons[index].IntValue);
                            }
                            else
                                end = true;
                        }
                        Buttons[2].Slider.maxValue = Buttons[index].IntValue - 1;
                        Buttons[2].CheckLimit();
                    }
                    break;
            }
            
            if (!fast && Time.time - time >= 1.5f) fast = true;

            yield return new WaitForSeconds(fast ? 0.1f : 0.3f);
        }
    }

    public void InterButtonSetup()
    {
        if (changing_name) return;

        input_accept = false;
        Buttons[4].OnClick();
        Animator.SetTrigger("Show");
        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.PopUp);
        foreach (SettingButtonScript button in Buttons)
            button.Selected = false;
    }

    public void QuitButton()
    {
        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.PopUp);
        Animator.SetTrigger("Hide");
    }

    public void SetPlayerName(bool save)
    {
        StopAllCoroutines();
        if (changing_name)
        {
            NameConfirm.gameObject.SetActive(false);
            if (save)
            {
                PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Don);
                Buttons[0].OnClick();
            }
            else if (Buttons[0] is InputNameScript input)
                input.Resume();
            
            changing_name = false;
        }
    }

    public void ChangeLeftSetting(int index)
    {
        if (!Setting || !input_accept || changing_name) return;

        StopAllCoroutines();
        ChangeSelected(index);
        ChangeSetting(true);
    }
    public void ChangeRightSetting(int index)
    {
        if (!Setting || !input_accept || changing_name)  return;
        ChangeSelected(index);
        ChangeSetting(false);
    }

    public void PressLeft(int index)
    {
        if (!Setting || !input_accept || changing_name) return;
        ChangeSelected(index);

        if (Buttons[index].IntValue - 1 >= Buttons[index].Slider.minValue)
        {
            Buttons[index].Press(false, true);
            switch (index)
            {
                case 1:
                    StartCoroutine(JudgeTimeAdjust(HitNoteResult.Perfect, false));
                    break;
                case 2:
                    StartCoroutine(JudgeTimeAdjust(HitNoteResult.Good, false));
                    break;
                case 3:
                    StartCoroutine(JudgeTimeAdjust(HitNoteResult.Bad, false));
                    break;
            }
        }
    }
    public void PressRight(int index)
    {
        if (!Setting || !input_accept || changing_name) return;
        ChangeSelected(index);

        if (Buttons[index].IntValue + 1 <= Buttons[index].Slider.maxValue)
        {
            Buttons[index].Press(true, true);
            switch (index)
            {
                case 1:
                    StartCoroutine(JudgeTimeAdjust(HitNoteResult.Perfect, true));
                    break;
                case 2:
                    StartCoroutine(JudgeTimeAdjust(HitNoteResult.Good, true));
                    break;
                case 3:
                    StartCoroutine(JudgeTimeAdjust(HitNoteResult.Bad, true));
                    break;
            }
            return;
        }
    }

    public void PointerRelease(bool left)
    {
        switch (index)
        {
            case 1:
            case 2:
            case 3:
            case 6:
            case 7:
                StopAllCoroutines();
                Buttons[index].Press(left, false);
                break;
        }
    }

    private void ChangeSetting(bool left)
    {
        Description.text = descri[index];
        StopAllCoroutines();

        switch (index)
        {
            case 1:
            case 2:
            case 3:
                Buttons[index].Press(!left, false);
                break;
            case 5:
                PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                if (left)
                {
                    int type = Buttons[index].IntValue;
                    type--;
                    if (type < 0) type = GameSetting.DrumTypes.Count - 1;
                    Buttons[index].IntValue = type;
                    Buttons[index].SetValue1(type.ToString());
                    GameSetting.SetDrumSoundType(type);
                }
                else
                {
                    int type = Buttons[index].IntValue;
                    type++;
                    if (type >= GameSetting.DrumTypes.Count) type = 0;
                    Buttons[index].IntValue = type;
                    Buttons[index].SetValue1(type.ToString());
                    GameSetting.SetDrumSoundType(type);
                }
                drum = true;
                break;
            case 6:
                if (left)
                {
                    if (Buttons[index].IntValue > -99)
                    {
                        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                        Buttons[index].LeftClick();
                        Buttons[index].SetSliderValue(Buttons[index].IntValue - 1);
                        GameSetting.SetJudgeTimeAdjust(Buttons[index].IntValue);
                    }
                }
                else
                {
                    if (Buttons[index].IntValue < 99)
                    {
                        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                        Buttons[index].RightClick();
                        Buttons[index].SetSliderValue(Buttons[index].IntValue + 1);
                        GameSetting.SetJudgeTimeAdjust(Buttons[index].IntValue);
                    }
                }
                break;
            case 7:

                if (left)
                {
                    if (Buttons[index].IntValue > -99)
                    {
                        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                        Buttons[index].LeftClick();
                        Buttons[index].SetSliderValue(Buttons[index].IntValue - 1);
                        GameSetting.SetNoteAdjust(Buttons[index].IntValue);
                    }
                }
                else
                {
                    if (Buttons[index].IntValue < 99)
                    {
                        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                        Buttons[index].RightClick();
                        Buttons[index].SetSliderValue(Buttons[index].IntValue + 1);
                        GameSetting.SetNoteAdjust(Buttons[index].IntValue);
                    }
                }
                break;
            case 8:
                ChangeMusicVolume(!left);
                break;
            case 9:
                ChangeDrumVolume(!left);
                break;
        }
    }

    public void SliderChange(int index)
    {
        if (!init || changing_name) return;

        EventSystem.current.SetSelectedGameObject(null);
        int value = (int)Buttons[index].Slider.value;
        Buttons[index].IntValue = value;
        Buttons[index].SetValue1(value.ToString());

        switch (index)
        {
            case 1:
                GameSetting.SetJudgeTime(HitNoteResult.Perfect, Buttons[index].IntValue);
                Buttons[2].Slider.minValue = value + 1;
                break;
            case 2:
                GameSetting.SetJudgeTime(HitNoteResult.Good, Buttons[index].IntValue);
                Buttons[1].Slider.maxValue = value - 1;
                Buttons[3].Slider.minValue = value + 1;
                break;
            case 3:
                GameSetting.SetJudgeTime(HitNoteResult.Bad, Buttons[index].IntValue);
                Buttons[2].Slider.maxValue = value - 1;
                break;
            case 6:
                GameSetting.SetJudgeTimeAdjust(value);
                break;
            case 7:
                GameSetting.SetNoteAdjust(value);
                break;
        }
    }

    public void ShowInput()
    {
        ButtonInput.Show();
    }

    public void ButtonHide()
    {
        input_accept = true;
        for (int i = 0; i < Buttons.Length; i++)
        {
            Buttons[i].Selected = (i == index);
        }
    }

    public void SetFirst()
    {
        ButtonInput.transform.SetAsFirstSibling();
    }

    public void SetLast()
    {
        ButtonInput.transform.SetAsLastSibling();
    }

    public void SetPlayerName()
    {
        if (!changing_name)
        {
            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Don);
            changing_name = true;
            Buttons[0].OnClick();
            NameConfirm.gameObject.SetActive(true);
        }
    }

    public void ChangeMusicVolume(bool forward)
    {
        if (forward && Buttons[8].IntValue + 1 <= 10)
        {
            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
            Buttons[8].RightClick();
            Buttons[8].SetSliderValue(Buttons[8].IntValue + 1);

            GameSetting.SetMusicVolume((float)Buttons[8].IntValue / 10);
        }
        else if (!forward && Buttons[8].IntValue - 1 >= 0)
        {
            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
            Buttons[8].LeftClick();
            Buttons[8].SetSliderValue(Buttons[8].IntValue - 1);

            GameSetting.SetMusicVolume((float)Buttons[8].IntValue / 10);
        }
    }
    public void ChangeDrumVolume(bool forward)
    {
        if (forward && Buttons[9].IntValue + 1 <= 10)
        {
            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
            Buttons[9].LeftClick();
            Buttons[9].SetSliderValue(Buttons[9].IntValue + 1);

            GameSetting.SetDrumVolume((float)Buttons[9].IntValue / 10);
        }
        else if (!forward && Buttons[9].IntValue - 1 >= 0)
        {
            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
            Buttons[9].LeftClick();
            Buttons[9].SetSliderValue(Buttons[9].IntValue - 1);

            GameSetting.SetDrumVolume((float)Buttons[9].IntValue / 10);
        }
    }
}
