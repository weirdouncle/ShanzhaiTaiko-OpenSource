using CommonClass;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputAction;

public delegate void SetBranchDelegate(CourseBranch branch);
public delegate void SwitchDelegate(bool forword);
public delegate void SetAutoDelegate();
public class OptionScript : MonoBehaviour
{
    public static event SwitchDelegate Switch;
    public static event SetBranchDelegate SetBranch;
    public static event PlaySelectedSoundDelegate PlaySelectedSound;
    public static event SetAutoDelegate SetAuto;

    public SettingButtonScript[] Buttons;
    public SettingButtonScript[] Buttons2;

    public Text Description;
    public Text LoopStatus;
    public Text LoopText1;
    public Text LoopText2;

    public Animator Animator;

    public LoaderScript Loader;
    public AudioSource Audio;
    public CriAtomSource ChapterSound;
    public PlayNoteScript PlayNote;
    public PracticeScript Practice;
    public GloableAniControllerScript GloableAni;
    public InputScript Input;
    public Text Lyrics;

    public int StartChap { set; get; }
    public int EndChap { set; get; }
    public bool Loop { set; get; }
    public bool PauseOnBad { set; get; }
    public CourseBranch Branch { private set; get; } = CourseBranch.BranchNormal;

    private bool inputing;
    private bool looping;
    private int index;
    private int loop_index;
    private bool branch;
#if UNITY_ANDROID
    private readonly Dictionary<int, string> desecription = new Dictionary<int, string>
    {
        { 0, "调整当前歌曲的章节位置" },
        { 1, "调整当前速度，最低速度为0.5倍。\n*当速度低于1时，不会播放歌曲" },
        { 2, "按右咚可重新游玩当前歌曲" },
        { 3, "歌曲会在你设定的章节中循环播放" },
        { 4, "设定自动游戏" },
        { 5, "设定本歌曲谱面分歧" },
        { 6, "调节歌曲音量" },
        { 7, "调节鼓声音量" },
    };
#else
    private readonly Dictionary<int, string> desecription = new Dictionary<int, string>
    {
        { 0, GameSetting.Translate("switch current chapter") },
        { 1, GameSetting.Translate("set total play speed") },
        { 2, GameSetting.Translate("play again from the beginning") },
        { 3, GameSetting.Translate("repeating a part of the song for practice") },
        { 4, GameSetting.Translate("set auto play") },
        { 5, GameSetting.Translate("change the current branch of the song") },
        { 6, GameSetting.Translate("show the recently 10 results of hit judge") },
    };
#endif

    void Start()
    {
        BasicInputScript.Input.Player.Option.performed += Press;
        BasicInputScript.Input.Player.Up.performed += Press;
        BasicInputScript.Input.Player.Down.performed += Press;
        BasicInputScript.Input.Player.Left.performed += Press;
        BasicInputScript.Input.Player.Right.performed += Press;
        BasicInputScript.Input.Player.LeftKa.performed += Press;
        BasicInputScript.Input.Player.RightKa.performed += Press;
        BasicInputScript.Input.Player.Enter.performed += Press;
        BasicInputScript.Input.Player.RightDon.performed += Press;
        BasicInputScript.Input.Player.LeftRelease.performed += Release;
        BasicInputScript.Input.Player.RightRelease.performed += Release;
        BasicInputScript.Input.Player.LeftKaRelease.performed += Release;
        BasicInputScript.Input.Player.RightKaRelease.performed += Release;
    }

    void OnEnable()
    {
        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Option);
        Lyrics.gameObject.SetActive(false);
        StartCoroutine(Show());
        StartCoroutine(DelayValue());

        if (GameSetting.Config.DirectInput)
            BasicInputScript.KeyInvoke += DirectKey;
    }

    IEnumerator DelayValue()
    {
        yield return new WaitForEndOfFrame();

        index = 0;
        if (Description != null)
            Description.text = desecription[index];

        //speed
        float speed = PlayNoteScript.TimeScale * 10;
        Buttons[1].SetSliderValue((int)speed);

        //loop
        LoopStatus.text = Loop ? "(ON)" : "(OFF)";
        LoopText1.text = StartChap.ToString();
        LoopText2.text = EndChap.ToString();

        Buttons2[0].BoolValue = Loop;
        Buttons2[0].SetValue1(Loop ? GameSetting.Translate(Buttons2[0].on_string) : GameSetting.Translate(Buttons2[0].off_string));
        Buttons2[1].SetValue1(StartChap.ToString());
        Buttons2[2].SetValue1(EndChap.ToString());

        //auto
        Buttons[4].BoolValue = GameSetting.Config.Special == Special.AutoPlay;
        Buttons[4].SetValue1(GameSetting.Config.Special == Special.AutoPlay ? GameSetting.Translate(Buttons[4].on_string) : GameSetting.Translate(Buttons[4].off_string));

        //branch
        branch = Loader.bHasBranch[Loader.LoadCourse];
        Buttons[5].SetValue1(GameSetting.Translate(Branch.ToString()));
        Buttons[5].Left.GetComponent<Image>().sprite = branch ? Buttons[5].Sprites[1] : Buttons[5].Sprites[0];
        Buttons[5].Right.GetComponent<Image>().sprite = branch ? Buttons[5].Sprites[1] : Buttons[5].Sprites[0];

        //hit result
        Buttons[6].BoolValue = false;
        Buttons[6].SetValue1(Buttons[4].off_string);

        SetCurrent();
    }

    IEnumerator Show()
    {
#if UNITY_ANDROID
        while (transform.localPosition.y < -2.5f)
        {
            transform.localPosition += new Vector3(0, 0.5f);
            yield return new WaitForSecondsRealtime(0.01f);
        }
        transform.localPosition = new Vector3(0, -2.5f, -9);
#else
        while (transform.localPosition.y < -3.1f)
        {
            transform.localPosition += new Vector3(0, 0.5f);
            yield return new WaitForSecondsRealtime(0.01f);
        }
        transform.localPosition = new Vector3(0, -3.1f, -9);
#endif

        looping = false;
        inputing = true;
    }

    private void OnDisable()
    {
        inputing = false;
        looping = false;
        Lyrics.gameObject.SetActive(true);
        transform.localPosition = new Vector3(0, -9, -9);

        BasicInputScript.KeyInvoke -= DirectKey;
    }

    private void OnDestroy()
    {
        BasicInputScript.KeyInvoke -= DirectKey;
    }

    private void DirectKey(GameSetting.KeyType key, bool press)
    {
        if (!inputing) return;
        if (looping)
        {
            if (key == GameSetting.KeyType.Left && !press)
            {
                StopAllCoroutines();
                if (loop_index == 1 || loop_index == 2)
                    Buttons2[loop_index].Press(false, false);
            }
            else if (key == GameSetting.KeyType.Right && !press)
            {
                StopAllCoroutines();
                if (loop_index == 1 || loop_index == 2)
                    Buttons2[loop_index].Press(true, false);
            }

            if (key == GameSetting.KeyType.Confirm || key == GameSetting.KeyType.RightDon)
            {
                StopAllCoroutines();
                if (loop_index == 0)
                {
                    SetLoopOn();
                }
                else if (loop_index == 3)
                {
                    OutLoopSetting();
                }
            }
            else if (key == GameSetting.KeyType.Left && press)
            {
                StopAllCoroutines();
                if (loop_index == 1 || loop_index == 2)
                {
                    MoveLoopChapter(false, loop_index);
                }
            }
            else if (key == GameSetting.KeyType.Right && press)
            {
                StopAllCoroutines();
                if (loop_index == 1 || loop_index == 2)
                {
                    MoveLoopChapter(true, loop_index);
                }
            }
            else if (key == GameSetting.KeyType.Up)
            {
                StopAllCoroutines();
                if (loop_index == 1 || loop_index == 2)
                    Buttons2[loop_index].Press(false, false);

                if (loop_index > 0)
                    loop_index--;
                else
                    loop_index = 3;
                PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                SetCurrent();
            }
            else if (key == GameSetting.KeyType.Option || key == GameSetting.KeyType.Down)
            {
                StopAllCoroutines();
                if (loop_index == 1 || loop_index == 2)
                    Buttons2[loop_index].Press(true, false);

                if (loop_index < 3)
                    loop_index++;
                else
                    loop_index = 0;

                PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                SetCurrent();
            }
        }
        else
        {
            if (key == GameSetting.KeyType.Left && !press)
            {
                ReleseMouse();
                switch (index)
                {
                    case 0:
                        Buttons[0].Press(false, false);
                        break;
                    case 1:
                        SetSpeed(false);
                        break;
                    case 5:
                        ChangeBranch(false);
                        break;
                }
            }
            else if (key == GameSetting.KeyType.Right && !press)
            {
                ReleseMouse();
                switch (index)
                {
                    case 0:
                        Buttons[0].Press(true, false);
                        break;
                    case 1:
                        SetSpeed(true);
                        break;
                    case 5:
                        ChangeBranch(true);
                        break;
                }
            }
            else if (key == GameSetting.KeyType.Confirm || key == GameSetting.KeyType.RightDon)
            {
                switch (index)
                {
                    case 2:
                        PlayRestart();
                        break;
                    case 3:
                        InterLoopSetting();
                        break;
                    case 4:
                        SetAutoPlay();
                        break;
                    case 6:
                        ShowHitResult();
                        break;
                }
            }
            else if (key == GameSetting.KeyType.Left && press)
            {
                if (index == 0)
                {
                    MoveChapter(false);
                }
            }
            else if (key == GameSetting.KeyType.Right && press)
            {
                if (index == 0)
                {
                    MoveChapter(true);
                }
            }
            else if (key == GameSetting.KeyType.Up)
            {
                index--;
                if (index < 0)
                    index = Buttons.Length - 1;
                if (index == 5 && !branch)
                    index--;

                PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                SetCurrent();
                if (Description != null)
                    Description.text = desecription[index];
            }
            else if (key == GameSetting.KeyType.Option || key == GameSetting.KeyType.Down)
            {
                index++;
                if (index == 5 && !branch)
                    index++;

                if (index > Buttons.Length - 1)
                    index = 0;

                PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                SetCurrent();
                if (Description != null)
                    Description.text = desecription[index];
            }
        }
    }

    public void OnTurnFinish()
    {
        inputing = true;
        looping = true;
        loop_index = 0;
#if !UNITY_ANDROID
        foreach (SettingButtonScript button in Buttons2)
            button.Selected = button.Index == loop_index;
#endif
    }

    public void OnTurnOffFinish()
    {
        inputing = true;
#if !UNITY_ANDROID
        foreach (SettingButtonScript button in Buttons)
            button.Selected = button.Index == index;
#endif
    }

    private void SetCurrent()
    {
#if !UNITY_ANDROID
        for (int i = 0; i < Buttons.Length; i++)
            Buttons[i].Selected = index == i;

        for (int i = 0; i < Buttons2.Length; i++)
            Buttons2[i].Selected = loop_index == i;
#endif
    }

    private void Press(CallbackContext context)
    {
        if (!Application.isFocused || !looping && !inputing) return;
        EventSystem.current.SetSelectedGameObject(null);
        ReleseMouse();

        InputAction path = context.action;

        if (looping)
        {
            StopAllCoroutines();
            if (path == BasicInputScript.Input.Player.Enter || path == BasicInputScript.Input.Player.RightDon)
            {
                if (loop_index == 0)
                {
                    SetLoopOn();
                }
                else if (loop_index == 3)
                {
                    OutLoopSetting();
                }
            }
            else if (path == BasicInputScript.Input.Player.Left || path == BasicInputScript.Input.Player.LeftKa)
            {
                if (loop_index == 1 || loop_index == 2)
                {
                    MoveLoopChapter(false, loop_index);
                }
            }
            else if (path == BasicInputScript.Input.Player.Right || path == BasicInputScript.Input.Player.RightKa)
            {
                if (loop_index == 1 || loop_index == 2)
                {
                    MoveLoopChapter(true, loop_index);
                }
            }
            else if (path == BasicInputScript.Input.Player.Up)
            {
                if (loop_index == 1 || loop_index == 2)
                    Buttons2[loop_index].Press(false, false);

                if (loop_index > 0)
                    loop_index--;
                else
                    loop_index = 3;
                PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                SetCurrent();
            }
            else if (path == BasicInputScript.Input.Player.Option || path == BasicInputScript.Input.Player.Down)
            {
                if (loop_index == 1 || loop_index == 2)
                    Buttons2[loop_index].Press(true, false);

                if (loop_index < 3)
                    loop_index++;
                else
                    loop_index = 0;

                PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                SetCurrent();
            }
        }
        else
        {
            if (path == BasicInputScript.Input.Player.Enter || path == BasicInputScript.Input.Player.RightDon)
            {
                switch (index)
                {
                    case 2:
                        PlayRestart();
                        break;
                    case 3:
                        InterLoopSetting();
                        break;
                    case 4:
                        SetAutoPlay();
                        break;
                    case 6:
                        ShowHitResult();
                        break;
                }
            }
            else if (path == BasicInputScript.Input.Player.Left || path == BasicInputScript.Input.Player.LeftKa)
            {
                if (index == 0)
                {
                    MoveChapter(false);
                }
            }
            else if (path == BasicInputScript.Input.Player.Right || path == BasicInputScript.Input.Player.RightKa)
            {
                if (index == 0)
                {
                    MoveChapter(true);
                }
            }
            else if (path == BasicInputScript.Input.Player.Up)
            {
                index--;
                if (index < 0)
                    index = Buttons.Length - 1;
                if (index == 5 && !branch)
                    index--;

                PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                SetCurrent();
                if (Description != null)
                    Description.text = desecription[index];
            }
            else if (path == BasicInputScript.Input.Player.Option || path == BasicInputScript.Input.Player.Down)
            {
                index++;
                if (index == 5 && !branch)
                    index++;

                if (index > Buttons.Length - 1)
                    index = 0;

                PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                SetCurrent();
                if (Description != null)
                    Description.text = desecription[index];
            }
        }
    }

    private void Release(CallbackContext context)
    {
        if (!Application.isFocused || !inputing) return;

        InputAction path = context.action;

        if (looping)
        {
            if (path == BasicInputScript.Input.Player.LeftRelease || path == BasicInputScript.Input.Player.LeftKaRelease)
            {
                StopAllCoroutines();
                if (loop_index == 1 || loop_index == 2)
                    Buttons2[loop_index].Press(false, false);
            }
            else
            {
                StopAllCoroutines();
                if (loop_index == 1 || loop_index == 2)
                    Buttons2[loop_index].Press(true, false);
            }
        }
        else
        {
            if (path == BasicInputScript.Input.Player.LeftRelease || path == BasicInputScript.Input.Player.LeftKaRelease)
            {
                ReleseMouse();
                switch (index)
                {
                    case 0:
                        Buttons[0].Press(false, false);
                        break;
                    case 1:
                        SetSpeed(false);
                        break;
                    case 5:
                        ChangeBranch(false);
                        break;
                }
            }
            else
            {
                ReleseMouse();
                switch (index)
                {
                    case 0:
                        Buttons[0].Press(true, false);
                        break;
                    case 1:
                        SetSpeed(true);
                        break;
                    case 5:
                        ChangeBranch(true);
                        break;
                }
            }
        }
    }

    private bool fast = false;
    IEnumerator Move(bool forward)
    {
        ChapterSound.Play(forward ? "forward" : "back");

        fast = false;
        float time = Time.unscaledTime;
        Switch?.Invoke(forward);
        Practice.SetChapter(PlayNoteScript.Chapter);
        yield return new WaitForSecondsRealtime(0.5f);

        while (true && ((forward && PlayNoteScript.Chapter < LoaderScript.Lines.Count) || (!forward && PlayNoteScript.Chapter > 0)))
        {
            if (!fast && Time.unscaledTime - time >= 1.5f)
            {
                fast = true;
                ChapterSound.Play(forward ? "forward_long" : "back_long");
            }
            else if (!fast)
                ChapterSound.Play(forward ? "forward" : "back");

            Switch?.Invoke(forward);
            Practice.SetChapter(PlayNoteScript.Chapter);
            yield return new WaitForSecondsRealtime(fast ? 0.1f : 0.3f);  
        }

        if (fast) ChapterSound.Stop();
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

    public void SetAutoPlay()
    {
        EventSystem.current.SetSelectedGameObject(null);
        ReleseMouse();

        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Don);
        GameSetting.SetAutoPlay(GameSetting.Config.Special != Special.AutoPlay);
        SetAuto?.Invoke();
        Buttons[4].OnClick();
    }

    public void ShowHitResult()
    {
        Practice.ResultPanel.gameObject.SetActive(!Practice.ResultPanel.gameObject.activeSelf);
        EventSystem.current.SetSelectedGameObject(null);
        ReleseMouse();

        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Don);
        Buttons[6].OnClick();
    }

    public void InterLoopSetting()
    {
        EventSystem.current.SetSelectedGameObject(null);
        StopAllCoroutines();
        if (fast)
            ChapterSound.Stop();

        foreach (SettingButtonScript buton in Buttons)
            buton.Selected = false;

        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Don);
        Buttons[3].OnClick();
        inputing = false;
        Animator.SetTrigger("Turn");
    }

    public void SetLoopOn()
    {
        EventSystem.current.SetSelectedGameObject(null);
        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Don);
        StopAllCoroutines();
        Buttons2[0].OnClick();
        Loop = !Loop;

        LoopStatus.text = Loop ? "(ON)" : "(OFF)";

        Practice.SetLoop(Loop, StartChap, EndChap);
    }

    public void OutLoopSetting()
    {
        EventSystem.current.SetSelectedGameObject(null);
        StopAllCoroutines();

        foreach (SettingButtonScript buton in Buttons2)
            buton.Selected = false;

        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Don);
        Buttons2[3].OnClick();
        looping = false;
        inputing = false;
        Animator.SetTrigger("TurnOff");
    }

    IEnumerator SetChapter(bool start, bool forward)
    {
        bool fast = false;
        float time = Time.unscaledTime;
        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
        if (start)
        {
            int value = StartChap;
            if (forward)
            {
                if (value + 1 < EndChap)
                    value++;
                else
                    value = 0;
            }
            else
            {
                if (value - 1 >= 0)
                    value--;
                else
                    value = EndChap - 1;
            }
            StartChap = value;
            Buttons2[1].SetValue1(value.ToString());
            LoopText1.text = StartChap.ToString();
            Practice.SetLoop(Loop, StartChap, EndChap);
        }
        else
        {
            int value = EndChap;
            if (forward)
            {
                if (value + 1 < LoaderScript.Lines.Count)
                    value++;
                else
                    value = StartChap + 1;
            }
            else
            {
                if (value - 1 > StartChap)
                    value--;
                else
                    value = LoaderScript.Lines.Count;
            }
            EndChap = value;
            Buttons2[2].SetValue1(value.ToString());
            LoopText2.text = EndChap.ToString();
            Practice.SetLoop(Loop, StartChap, EndChap);
        }

        yield return new WaitForSecondsRealtime(0.5f);

        while (true)
        {
            if (start)
            {
                int value = StartChap;
                if (forward)
                {
                    if (value + 1 < EndChap)
                        value++;
                    else
                        value = 0;
                }
                else
                {
                    if (value - 1 >= 0)
                        value--;
                    else
                        value = EndChap - 1;
                }

                PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                StartChap = value;
                Buttons2[1].SetValue1(value.ToString());
                LoopText1.text = StartChap.ToString();
                Practice.SetLoop(Loop, StartChap, EndChap);
            }
            else
            {
                int value = EndChap;
                if (forward)
                {
                    if (value + 1 < LoaderScript.Lines.Count)
                        value++;
                    else
                        value = StartChap + 1;
                }
                else
                {
                    if (value - 1 > StartChap)
                        value--;
                    else
                        value = LoaderScript.Lines.Count;
                }
                PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                EndChap = value;
                Buttons2[2].SetValue1(value.ToString());
                LoopText2.text = EndChap.ToString();
                Practice.SetLoop(Loop, StartChap, EndChap);
            }

            if (!fast && Time.unscaledTime - time >= 2)
            {
                fast = true;
            }
            yield return new WaitForSecondsRealtime(fast ? 0.1f : 0.5f);
        }
    }

    public void MoveChapter(bool forward)
    {
        if (!inputing || looping) return;

        if (!forward)
        {
            StartCoroutine(Move(false));
            Buttons[0].Press(false, true);
        }
        else
        {
            StartCoroutine(Move(true));
            Buttons[0].Press(true, true);
        }
    }

    public void ReleseMouse()
    {
        StopAllCoroutines();
        if (fast)
            ChapterSound.Stop();

        Buttons[0].Press(false, false);
        Buttons[0].Press(true, false);
        Buttons2[1].Press(false, false);
        Buttons2[1].Press(true, false);
        Buttons2[2].Press(false, false);
        Buttons2[2].Press(true, false);
    }

    public void MoveLoopLeftChapter(int index)
    {
        if (!inputing || !looping) return;
        MoveLoopChapter(false, index);
    }
    public void MoveLoopRightChapter(int index)
    {
        if (!inputing || !looping) return;
        MoveLoopChapter(true, index);
    }

    private void MoveLoopChapter(bool forward, int index)
    {
        if (!forward)
        {
            StartCoroutine(SetChapter(index == 1, false));
            Buttons2[index].Press(false, true);
        }
        else
        {
            StartCoroutine(SetChapter(index == 1, true));
            Buttons2[index].Press(true, true);
        }
    }

    public void SetSpeed(bool forward)
    {
        if (forward && Buttons[1].IntValue + 1 <= 10)
        {
            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
            Buttons[1].RightClick();
            Buttons[1].SetSliderValue(Buttons[1].IntValue + 1);
            PlayNoteScript.TimeScale = (float)Buttons[1].IntValue / 10;
        }
        else if (!forward && Buttons[1].IntValue - 1 >= Buttons[1].Slider.minValue)
        {
            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
            Buttons[1].LeftClick();
            Buttons[1].SetSliderValue(Buttons[1].IntValue - 1);
            PlayNoteScript.TimeScale = (float)Buttons[1].IntValue / 10;
        }
    }

    public void ChangeBranch(bool forward)
    {
        if (!branch) return;

        if (forward)
        {
            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Branch);
            Buttons[5].RightClick();

            if (Branch == CourseBranch.BranchNormal)
                Branch = CourseBranch.BranchExpert;
            else if (Branch == CourseBranch.BranchExpert)
                Branch = CourseBranch.BranchMaster;
            else
                Branch = CourseBranch.BranchNormal;
            SetBranch?.Invoke(Branch);

            Buttons[5].SetValue1(GameSetting.Translate(Branch.ToString()));
        }
        else
        {
            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Branch);
            Buttons[5].LeftClick();

            if (Branch == CourseBranch.BranchNormal)
                Branch = CourseBranch.BranchMaster;
            else if (Branch == CourseBranch.BranchMaster)
                Branch = CourseBranch.BranchExpert;
            else
                Branch = CourseBranch.BranchNormal;
            SetBranch?.Invoke(Branch);

            Buttons[5].SetValue1(GameSetting.Translate(Branch.ToString()));
        }
    }

    public void Resume()
    {
        if (!inputing) return;
        EventSystem.current.SetSelectedGameObject(null);
        Input.CloseOption();
    }
}
