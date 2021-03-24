using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonClass;
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.InputSystem;
using System.Text;
using Gamepad = UnityEngine.InputSystem.Gamepad;

public delegate void QuitPlayDelegate();
public delegate void HoldDelegate(bool hold);

public class InputScript : MonoBehaviour
{
    public static event QuitPlayDelegate QuitPlay;
    public static event HoldDelegate Hold;

    public Animator DonLeft;
    public Animator DonRight;
    public Animator KaLeft;
    public Animator KaRight;
    public OptionScript PracticeOption;
    public PlayOptionScript PlayOption;
    public PracticeScript PracticePanel;

    //2P
    public Animator DonLeft2P;
    public Animator DonRight2P;
    public Animator KaLeft2P;
    public Animator KaRight2P;

    public SoundPool Sound;

    public static bool Quiting;
    public static Dictionary<CourseBranch, Dictionary<bool, List<NoteSoundScript>>> Notes = new Dictionary<CourseBranch, Dictionary<bool, List<NoteSoundScript>>>();
    public static CourseBranch CurrentBranch;
    public static CourseBranch CurrentBranch2P;
    public static List<InputReplay> InputRecord;
    public static InputScript Instance;
    public static HoldStatus Holding { set; get; } = HoldStatus.None;
    public enum HoldStatus
    {
        None,
        Hoding,
        Playing,
        Finished,
    }

    protected bool load_finished;
    protected int diff_adjust_perfect = 17;
    protected int diff_adjust_good = 33;
    protected int diff_adjust_bad = 17;
    protected int judge_good;
    protected int judge_perfect;
    protected int judge_bad;
    protected Config config;
    private List<InputReplay> input_replay;

    protected InputDevice last_device = null;
    protected int last_note;
    protected float last_time;
    protected bool last_left;

    void Start()
    {
        InputRecord = new List<InputReplay>();
        Instance = this;
        config = GameSetting.Config;
        if (GameSetting.Mode == CommonClass.PlayMode.Replay || GameSetting.Mode == CommonClass.PlayMode.PlayWithReplay)
            input_replay = GameSetting.Replay.Input[GameSetting.Config.ScoreMode][(int)GameSetting.Difficulty];

        judge_bad = config.JudgeRange[CommonClass.HitNoteResult.Bad] + (GameSetting.Difficulty < Difficulty.Hard ? diff_adjust_bad : 0);
        judge_good = config.JudgeRange[CommonClass.HitNoteResult.Good] + (GameSetting.Difficulty < Difficulty.Hard ? diff_adjust_good : 0);
        judge_perfect = config.JudgeRange[CommonClass.HitNoteResult.Perfect] + (GameSetting.Difficulty < Difficulty.Hard ? diff_adjust_perfect : 0);

        CurrentBranch = CurrentBranch2P = CourseBranch.BranchNormal;
        Quiting = false;
        Holding = HoldStatus.None;
        LoadingScreenScript.Play += PlayStart;

#if !UNITY_ANDROID
        if (((GameSetting.Mode == CommonClass.PlayMode.Normal || GameSetting.Mode == CommonClass.PlayMode.PlayWithReplay)
            && GameSetting.Config.Special != Special.AutoPlay) || GameSetting.Mode == CommonClass.PlayMode.Practice)
        {
            if (GameSetting.DonLeft != null)
                GameSetting.DonLeft.started += LeftDonInvoke;
            if (GameSetting.DonRight != null)
                GameSetting.DonRight.started += RightDonInvoke;
            if (GameSetting.KaLeft != null)
                GameSetting.KaLeft.started += LeftKaInvoke;
            if (GameSetting.KaRight != null)
                GameSetting.KaRight.started += RightKaInvoke;

            if (GameSetting.DonLeft2 != null)
                GameSetting.DonLeft2.started += LeftDonInvoke;
            if (GameSetting.DonRight2 != null)
                GameSetting.DonRight2.started += RightDonInvoke;
            if (GameSetting.KaLeft2 != null)
                GameSetting.KaLeft2.started += LeftKaInvoke;
            if (GameSetting.KaRight2 != null)
                GameSetting.KaRight2.started += RightKaInvoke;
        }

        if (GameSetting.Config.DirectInput)
            BasicInputScript.KeyInvoke += DirectKey;

        BasicInputScript.Input.Player.Option.performed += OpenOption;
        //BasicInputScript.Input.Player.Cancel.performed += CloseOption;
#endif
        BasicInputScript.Input.Player.Esc.performed += QuitGame;
    }

    private void OnDestroy()
    {
        Notes.Clear();
        LoadingScreenScript.Play -= PlayStart;

#if !UNITY_ANDROID
        if (!GameSetting.Config.DirectInput)
        {
            if (GameSetting.DonLeft != null)
                GameSetting.DonLeft.started -= LeftDonInvoke;
            if (GameSetting.DonRight != null)
                GameSetting.DonRight.started -= RightDonInvoke;
            if (GameSetting.KaLeft != null)
                GameSetting.KaLeft.started -= LeftKaInvoke;
            if (GameSetting.KaRight != null)
                GameSetting.KaRight.started -= RightKaInvoke;

            if (GameSetting.DonLeft2 != null)
                GameSetting.DonLeft2.started -= LeftDonInvoke;
            if (GameSetting.DonRight2 != null)
                GameSetting.DonRight2.started -= RightDonInvoke;
            if (GameSetting.KaLeft2 != null)
                GameSetting.KaLeft2.started -= LeftKaInvoke;
            if (GameSetting.KaRight2 != null)
                GameSetting.KaRight2.started -= RightKaInvoke;
        }

        BasicInputScript.KeyInvoke -= DirectKey;
#endif
        BasicInputScript.Input.Player.Option.performed -= OpenOption;
        BasicInputScript.Input.Player.Esc.performed -= QuitGame;
    }

    protected virtual void PlayStart()
    {
        Holding = HoldStatus.Playing;
        load_finished = true;

        if (GameSetting.Mode == CommonClass.PlayMode.Replay || GameSetting.Mode == CommonClass.PlayMode.PlayWithReplay) StartCoroutine(DoReplay());
    }

    IEnumerator DoReplay()
    {
        bool player2 = GameSetting.Mode == CommonClass.PlayMode.PlayWithReplay;
        for (int i = 0; i < input_replay.Count; i++)
        {
            InputReplay input = input_replay[i];
            yield return new WaitForSeconds(input.Time + (float)config.NoteAdjust / 1000 - (Time.time - PlayNoteScript.LastStart));
            //Debug.Log(Time.time - time - input.Key);
            switch (input.Key)
            {
                case GameSetting.KeyType.LeftDon:
                    if (player2)
                        ShowDon2P(true);
                    else
                        ShowDon(true);
                    break;
                case GameSetting.KeyType.RightDon:
                    if (player2)
                        ShowDon2P(false);
                    else
                        ShowDon(false);
                    break;
                case GameSetting.KeyType.LeftKa:
                    if (player2)
                        ShowKa2P(true);
                    else
                        ShowKa(true);
                    break;
                case GameSetting.KeyType.RightKa:
                    if (player2)
                        ShowKa2P(false);
                    else
                        ShowKa(false);
                    break;
            }
        }
    }

    public virtual void Restart()
    {
        InputRecord.Clear();
        StopAllCoroutines();
    }
    public virtual void TimeReset()
    {
        if (GameSetting.Mode == CommonClass.PlayMode.Replay || GameSetting.Mode == CommonClass.PlayMode.PlayWithReplay)
            StartCoroutine(DoReplay());
    }

    private StringBuilder builder = new StringBuilder();
    protected virtual void Judge(bool don, bool left)
    {
        builder.Clear();
        float current_time = config.NoteAdjust - (Time.time - PlayNoteScript.LastStart) * 1000 * PlayNoteScript.TimeScale + (config.Special == Special.AutoPlay ? 0 : config.JudgeTimeAdjust);
        while (true)
        {
            if (Notes[CurrentBranch][don].Count == 0) return;
            NoteSoundScript note = Notes[CurrentBranch][don][0];
            float time = note.JudgeTime + current_time;

            if (time >= judge_bad)
            {
                last_note = 0;
                if (GameSetting.Mode == CommonClass.PlayMode.Practice && time - judge_bad < 500)
                {
                    builder.Append(string.Format("index {0} {1} ", note.Index, time.ToString("#0.00")));
                    PracticePanel.ShowResult(don, CommonClass.HitNoteResult.None, builder.ToString()); ;
                }
                return;
            }

            bool succeed = true;
            switch (note.Type)
            {
                case 1:
                case 2:
                    if (Math.Abs(time) <= judge_perfect)   //perfect
                    {
                        NotesSplitScript.NormalNotes[don].Remove(note);
                        NotesSplitScript.MasterNotes[don].Remove(note);
                        NotesSplitScript.ExpertNotes[don].Remove(note);
                        note.Hit();
                        note.State = HitPracticeResult.Perfect;
                        note.gameObject.SetActive(false);
                        SendResult(HitNoteResult.Perfect, note, don);

                        if (GameSetting.Mode == CommonClass.PlayMode.Practice)
                        {
                            builder.Append(string.Format("index {0} {1} ", note.Index, time.ToString("#0.00")));
                            PracticePanel.ShowResult(don, HitNoteResult.Perfect, builder.ToString());
                        }
                    }
                    else if (Math.Abs(time) <= judge_good)    //good
                    {
                        NotesSplitScript.NormalNotes[don].Remove(note);
                        NotesSplitScript.MasterNotes[don].Remove(note);
                        NotesSplitScript.ExpertNotes[don].Remove(note);
                        note.Hit();
                        note.State = time < 0 ? HitPracticeResult.Late : HitPracticeResult.Fast;
                        note.gameObject.SetActive(false);
                        GloableAniControllerScript.Instance.ShowResult(HitNoteResult.Good, note);
                        GloableAniControllerScript.Instance.Hit(note, don);

                        if (GameSetting.Mode == CommonClass.PlayMode.Practice)
                        {
                            builder.Append(string.Format("index {0} {1} ", note.Index, time.ToString("#0.00")));
                            PracticePanel.ShowResult(don, HitNoteResult.Good, builder.ToString());
                        }
                    }
                    else if (time > 0 && time < judge_bad)       //bad
                    {
                        if (last_left == left || (!don || last_note != 3) && (don || last_note != 4) || last_time - current_time > 12)
                        {
                            NotesSplitScript.NormalNotes[don].Remove(note);
                            NotesSplitScript.MasterNotes[don].Remove(note);
                            NotesSplitScript.ExpertNotes[don].Remove(note);

                            note.Hit();
                            note.State = HitPracticeResult.Bad;
                            note.gameObject.SetActive(false);
                            GloableAniControllerScript.Instance.ShowResult(HitNoteResult.Bad, note);

                            if (GameSetting.Mode == CommonClass.PlayMode.Practice)
                            {
                                builder.Append(string.Format("index {0} {1} ", note.Index, time.ToString("#0.00")));
                                PracticePanel.ShowResult(don, HitNoteResult.Bad, builder.ToString());
                                if (PracticeOption.PauseOnBad)
                                    OpenOption();
                            }
                        }
                    }
                    else if (time < 0)
                    {
                        NotesSplitScript.NormalNotes[don].Remove(note);
                        NotesSplitScript.MasterNotes[don].Remove(note);
                        NotesSplitScript.ExpertNotes[don].Remove(note);
                        succeed = false;

                        if (GameSetting.Mode == CommonClass.PlayMode.Practice)
                            builder.Append(string.Format("index {0} missed ", note.Index));
                    }
                    break;
                case 3:         //big don
                case 4:         //big ka
                    if (Math.Abs(time) <= judge_perfect)   //perfect
                    {
                        NotesSplitScript.NormalNotes[don].Remove(note);
                        NotesSplitScript.MasterNotes[don].Remove(note);
                        NotesSplitScript.ExpertNotes[don].Remove(note);
                        note.Hit();
                        note.State = HitPracticeResult.Perfect;
                        note.gameObject.SetActive(false);
                        GloableAniControllerScript.Instance.ShowResult(HitNoteResult.Perfect, note);
                        GloableAniControllerScript.Instance.Hit(note, don);

                        if (last_device != null && last_device is UnityEngine.InputSystem.Gamepad pad)
                            StartCoroutine(Vibration(pad));

                        if (GameSetting.Mode == CommonClass.PlayMode.Practice)
                        {
                            builder.Append(string.Format("index {0} {1} ", note.Index, time.ToString("#0.00")));
                            PracticePanel.ShowResult(don, CommonClass.HitNoteResult.Perfect, builder.ToString());
                        }
                    }
                    else if (Math.Abs(time) <= judge_good)    //good
                    {
                        NotesSplitScript.NormalNotes[don].Remove(note);
                        NotesSplitScript.MasterNotes[don].Remove(note);
                        NotesSplitScript.ExpertNotes[don].Remove(note);
                        note.Hit();
                        note.State = time < 0 ? HitPracticeResult.Late : HitPracticeResult.Fast;
                        note.gameObject.SetActive(false);
                        GloableAniControllerScript.Instance.ShowResult(HitNoteResult.Good, note);
                        GloableAniControllerScript.Instance.Hit(note, don);

                        if (last_device != null && last_device is Gamepad pad)
                            StartCoroutine(Vibration(pad));

                        if (GameSetting.Mode == CommonClass.PlayMode.Practice)
                        {
                            builder.Append(string.Format("index {0} {1} ", note.Index, time.ToString("#0.00")));
                            PracticePanel.ShowResult(don, HitNoteResult.Good, builder.ToString());
                        }
                    }
                    else if (time < judge_bad)       //bad
                    {
                        if (last_left == left || (!don || last_note != 3) && (don || last_note != 4) || last_time - current_time > 12)
                        {
                            NotesSplitScript.NormalNotes[don].Remove(note);
                            NotesSplitScript.MasterNotes[don].Remove(note);
                            NotesSplitScript.ExpertNotes[don].Remove(note);
                            note.Hit();
                            note.State = HitPracticeResult.Bad;
                            note.gameObject.SetActive(false);
                            GloableAniControllerScript.Instance.ShowResult(HitNoteResult.Bad, note);

                            if (GameSetting.Mode == CommonClass.PlayMode.Practice)
                            {
                                builder.Append(string.Format("index {0} {1} ", note.Index, time.ToString("#0.00")));
                                PracticePanel.ShowResult(don, HitNoteResult.Bad, builder.ToString());
                                if (PracticeOption.PauseOnBad)
                                    OpenOption();
                            }
                        }
                    }
                    else if (time < 0)
                    {
                        NotesSplitScript.NormalNotes[don].Remove(note);
                        NotesSplitScript.MasterNotes[don].Remove(note);
                        NotesSplitScript.ExpertNotes[don].Remove(note);
                        succeed = false;

                        if (GameSetting.Mode == CommonClass.PlayMode.Practice)
                            builder.Append(string.Format("index {0} missed ", note.Index));
                    }
                    break;
                case 5 when note is RapidScript rapid:         //rapid hit
                    if (time <= 0 && rapid.EndTime + config.NoteAdjust - ((Time.time - PlayNoteScript.LastStart) * 1000 * PlayNoteScript.TimeScale) > 0)
                    {
                        GloableAniControllerScript.Instance.Hit(note, don);
                        note.Hit();
                    }
                    else if (time < 0)
                    {
                        NotesSplitScript.NormalNotes[don].Remove(note);
                        NotesSplitScript.MasterNotes[don].Remove(note);
                        NotesSplitScript.ExpertNotes[don].Remove(note);
                        succeed = false;
                    }
                    break;
                case 6 when note is RapidScript big_rapid:         //big rapid hit
                    if (time <= 0 && big_rapid.EndTime + config.NoteAdjust - ((Time.time - PlayNoteScript.LastStart) * 1000 * PlayNoteScript.TimeScale) > 0)
                    {
                        GloableAniControllerScript.Instance.Hit(note, don);
                        note.Hit();

                        if (last_device != null && last_device is Gamepad pad)
                            StartCoroutine(Vibration(pad));
                    }
                    else if (time < 0)
                    {
                        NotesSplitScript.NormalNotes[don].Remove(note);
                        NotesSplitScript.MasterNotes[don].Remove(note);
                        NotesSplitScript.ExpertNotes[don].Remove(note);
                        succeed = false;
                    }
                    break;
                case 7 when note is BallonScript balloon:         //balloon
                    if (time <= 0 && balloon.EndTime + config.NoteAdjust - ((Time.time - PlayNoteScript.LastStart) * 1000 * PlayNoteScript.TimeScale) > 0)
                    {
                        note.Hit();
                        GloableAniControllerScript.Instance.Hit(note, don);

                        if (last_device != null && last_device is Gamepad pad)
                            StartCoroutine(Vibration(pad));
                    }
                    else if (time < 0)
                    {
                        NotesSplitScript.NormalNotes[don].Remove(note);
                        NotesSplitScript.MasterNotes[don].Remove(note);
                        NotesSplitScript.ExpertNotes[don].Remove(note);
                        succeed = false;
                    }
                    break;
                case 9 when note is BallonScript balloon:         //kusudama
                    if (time <= 0 && balloon.EndTime + config.NoteAdjust - ((Time.time - PlayNoteScript.LastStart) * 1000 * PlayNoteScript.TimeScale) > 0)
                    {
                        SendResult(CommonClass.HitNoteResult.None, note, don);
                        note.Hit();

                        if (last_device != null && last_device is Gamepad pad)
                            StartCoroutine(Vibration(pad));
                    }
                    else if (time < 0)
                    {
                        NotesSplitScript.NormalNotes[don].Remove(note);
                        NotesSplitScript.MasterNotes[don].Remove(note);
                        NotesSplitScript.ExpertNotes[don].Remove(note);
                        succeed = false;
                    }
                    break;
            }
           
            if (succeed)
            {
                last_note = note.Type;
                last_time = current_time;
                last_left = left;
                break;
            }
        }
    }

    protected void SendResult(HitNoteResult result, NoteSoundScript note, bool don)
    {
        if (result != HitNoteResult.None) GloableAniControllerScript.Instance.ShowResult(result, note);
        if (result != HitNoteResult.Bad) GloableAniControllerScript.Instance.Hit(note, don);
    }

    IEnumerator Vibration(UnityEngine.InputSystem.Gamepad pad)
    {
        pad.SetMotorSpeeds(0.8f, 0.8f);
        yield return new WaitForSecondsRealtime(0.3f);
        pad.ResetHaptics();
    }

    IEnumerator ChangeHoldStatus(HoldStatus status)
    {
        Holding = HoldStatus.None;
        yield return new WaitForSecondsRealtime(0.2f);
        Holding = status;
    }

    protected void OpenOption(CallbackContext context)
    {
        if (!Application.isFocused || Quiting || Holding != HoldStatus.Playing) return;

        OpenOption();
    }

    protected virtual void LeftDonInvoke(CallbackContext context)
    {
        if (!Application.isFocused || Quiting || Holding != HoldStatus.Playing) return;

        if (config.Special != Special.AutoPlay)
        {
            last_device = context.control.device;
            Judge(true, true);
            ShowDon(true);
#if !UNITY_ANDROID
            if (GameSetting.Mode != CommonClass.PlayMode.Practice)
                InputRecord.Add(new InputReplay(Time.time - PlayNoteScript.LastStart - (float)config.NoteAdjust / 1000, GameSetting.KeyType.LeftDon));
#endif
        }
    }

    protected virtual void RightDonInvoke(CallbackContext context)
    {
        if (!Application.isFocused || Quiting || Holding != HoldStatus.Playing) return;

        if (config.Special != Special.AutoPlay)
        {
            last_device = context.control.device;
            Judge(true, false);
            ShowDon(false);
#if !UNITY_ANDROID
            if (GameSetting.Mode != CommonClass.PlayMode.Practice)
                InputRecord.Add(new InputReplay(Time.time - PlayNoteScript.LastStart - (float)config.NoteAdjust / 1000, GameSetting.KeyType.RightDon));
#endif
        }
    }

    protected virtual void LeftKaInvoke(CallbackContext context)
    {
        if (!Application.isFocused || Quiting || Holding != HoldStatus.Playing) return;

        if (config.Special != Special.AutoPlay)
        {
            last_device = context.control.device;
            Judge(false, true);
            ShowKa(true);
#if !UNITY_ANDROID
            if (GameSetting.Mode != CommonClass.PlayMode.Practice)
                InputRecord.Add(new InputReplay(Time.time - PlayNoteScript.LastStart - (float)config.NoteAdjust / 1000, GameSetting.KeyType.LeftKa));
#endif
        }
    }

    protected virtual void RightKaInvoke(CallbackContext context)
    {
        if (!Application.isFocused || Quiting || Holding != HoldStatus.Playing) return;

        if (config.Special != Special.AutoPlay)
        {
            last_device = context.control.device;
            Judge(false, false);
            ShowKa(false);
#if !UNITY_ANDROID
            if (GameSetting.Mode != CommonClass.PlayMode.Practice)
                InputRecord.Add(new InputReplay(Time.time - PlayNoteScript.LastStart - (float)config.NoteAdjust / 1000, GameSetting.KeyType.RightKa));
#endif
        }
    }

    protected virtual void DirectKey(GameSetting.KeyType key, bool press)
    {
        if (!GameSetting.Config.DirectInput || Quiting) return;
        switch (key)
        {
            case GameSetting.KeyType.LeftDon when Holding == HoldStatus.Playing && config.Special != Special.AutoPlay:
                {
                    Judge(true, true);
                    ShowDon(true);
#if !UNITY_ANDROID
                    if (GameSetting.Mode != CommonClass.PlayMode.Practice)
                        InputRecord.Add(new InputReplay(Time.time - PlayNoteScript.LastStart - (float)config.NoteAdjust / 1000, GameSetting.KeyType.LeftDon));
#endif
                }
                break;
            case GameSetting.KeyType.RightDon when Holding == HoldStatus.Playing && config.Special != Special.AutoPlay:
                {
                    Judge(true, false);
                    ShowDon(false);
#if !UNITY_ANDROID
                    if (GameSetting.Mode != CommonClass.PlayMode.Practice)
                        InputRecord.Add(new InputReplay(Time.time - PlayNoteScript.LastStart - (float)config.NoteAdjust / 1000, GameSetting.KeyType.RightDon));
#endif
                }
                break;
            case GameSetting.KeyType.LeftKa when press && Holding == HoldStatus.Playing && config.Special != Special.AutoPlay:
                {
                    Judge(false, true);
                    ShowKa(true);
#if !UNITY_ANDROID
                    if (GameSetting.Mode != CommonClass.PlayMode.Practice)
                        InputRecord.Add(new InputReplay(Time.time - PlayNoteScript.LastStart - (float)config.NoteAdjust / 1000, GameSetting.KeyType.LeftKa));
#endif
                }
                break;
            case GameSetting.KeyType.RightKa when press && Holding == HoldStatus.Playing && config.Special != Special.AutoPlay:
                {
                    Judge(false, true);
                    ShowKa(false);
#if !UNITY_ANDROID
                    if (GameSetting.Mode != CommonClass.PlayMode.Practice)
                        InputRecord.Add(new InputReplay(Time.time - PlayNoteScript.LastStart - (float)config.NoteAdjust / 1000, GameSetting.KeyType.RightKa));
#endif
                }
                break;
            case GameSetting.KeyType.Escape:
                if (Holding == HoldStatus.Playing)
                    QuitGame();
                else if (Holding == HoldStatus.Hoding)
                    CloseOption();
                break;
            case GameSetting.KeyType.Option when Holding == HoldStatus.Playing:
                OpenOption();
                break;
            case GameSetting.KeyType.Cancel when !Quiting && Holding == HoldStatus.Hoding:
                CloseOption();
                break;
        }
    }

    protected void CloseOption(CallbackContext context)
    {
        if (!Application.isFocused || Quiting || Holding != HoldStatus.Hoding)
            return;

        CloseOption();
    }
    protected virtual void QuitGame(CallbackContext context)
    {
        if (!Application.isFocused || !load_finished || Quiting) return;

        if (Holding == HoldStatus.Playing)
            QuitGame();
        else if (Holding == HoldStatus.Hoding)
            CloseOption();
    }
    public void QuitGame()
    {
        Quiting = true;
        QuitPlay?.Invoke();
    }
    public void OpenOption()
    {
        if (!Quiting && Holding == HoldStatus.Playing)
        {
            Time.timeScale = 0;

#if !UNITY_ANDROID
            BasicInputScript.Input.Player.Cancel.performed += CloseOption;
#endif
            StartCoroutine(ChangeHoldStatus(HoldStatus.Hoding));
            Hold?.Invoke(true);

            if (GameSetting.Mode == CommonClass.PlayMode.Practice)
                PracticeOption.gameObject.SetActive(true);
            else
                PlayOption.gameObject.SetActive(true);
        }
    }
    public void CloseOption()
    {
#if !UNITY_ANDROID
        BasicInputScript.Input.Player.Cancel.performed -= CloseOption;
#endif

        Time.timeScale = 1;
        StartCoroutine(ChangeHoldStatus(HoldStatus.Playing));
        Hold?.Invoke(false);
        if (GameSetting.Mode == CommonClass.PlayMode.Practice)
            PracticeOption.gameObject.SetActive(false);
        else
            PlayOption.gameObject.SetActive(false);
    }

    protected bool auto_last_left = false;
    public virtual void PlayDrum(int type)
    {
        if (config.Special != Special.AutoPlay) return;
        switch (type)
        {
            case 1:
                Judge(true, auto_last_left);
                ShowDon(auto_last_left);
                auto_last_left = !auto_last_left;
                break;
            case 2:
                Judge(false, auto_last_left);
                ShowKa(auto_last_left);
                auto_last_left = !auto_last_left;
                break;
            case 3:
                Judge(true, auto_last_left);
#if !UNITY_ANDROID
                ShowDon(true);
                ShowDon(false);
#else
                ShowDon(auto_last_left);
                auto_last_left = !auto_last_left;
#endif
                break;
            case 4:
                Judge(false, auto_last_left);
#if !UNITY_ANDROID
                ShowKa(true);
                ShowKa(false);
#else
                ShowKa(auto_last_left);
                auto_last_left = !auto_last_left;
#endif
                break;
            case 5:
                Judge(true, auto_last_left);
                ShowDon(auto_last_left);
                auto_last_left = !auto_last_left;
                break;
            case 6:
                Judge(true, auto_last_left);
#if !UNITY_ANDROID
                ShowDon(true);
                ShowDon(false);
#else
                ShowDon(auto_last_left);
                auto_last_left = !auto_last_left;
#endif
                break;
            case 7:
            case 9:
                Judge(true, auto_last_left);
                ShowDon(auto_last_left);
                auto_last_left = !auto_last_left;
                break;
        }
    }

    public virtual void PlayDrum2P(int type)
    {
    }

    protected void ShowDon(bool left)
    {
        Sound.PlaySound(true);
        if (left)
        {
            DonLeft.SetTrigger("Hit");
        }
        else
        {
            DonRight.SetTrigger("Hit");
        }
        HitBgPoolScript.Instance.Play(true);
    }

    protected void ShowKa(bool left)
    {
        Sound.PlaySound(false);
        if (left)
        {
            KaLeft.SetTrigger("Hit");
        }
        else
        {
            KaRight.SetTrigger("Hit");
        }
        HitBgPoolScript.Instance.Play(false);
    }

    protected void ShowDon2P(bool left)
    {
        Sound.PlaySound2(true);
        if (left)
        {
            DonLeft2P.SetTrigger("Hit");
        }
        else
        {
            DonRight2P.SetTrigger("Hit");
        }
        HitBgPoolScript.Instance2P.Play(true);
    }

    protected void ShowKa2P(bool left)
    {
        Sound.PlaySound2(false);
        if (left)
        {
            KaLeft2P.SetTrigger("Hit");
        }
        else
        {
            KaRight2P.SetTrigger("Hit");
        }
        HitBgPoolScript.Instance2P.Play(false);
    }

    public virtual void AndroidHitDon(bool left)
    {
        if (GameSetting.Mode == CommonClass.PlayMode.Replay) return;
        if (config.Special != Special.AutoPlay && Holding == HoldStatus.Playing)
        {
            Judge(true, left);
            ShowDon(left);

#if !UNITY_ANDROID
            InputRecord.Add(new InputReplay(Time.time - PlayNoteScript.LastStart - (float)config.NoteAdjust / 1000, left ? GameSetting.KeyType.LeftDon : GameSetting.KeyType.RightDon));
#endif
        }
    }
    public virtual void AndroidHitKa(bool left)
    {
        if (GameSetting.Mode == CommonClass.PlayMode.Replay) return;
        if (config.Special != Special.AutoPlay && Holding == HoldStatus.Playing)
        {
            Judge(false, left);
            ShowKa(left);

#if !UNITY_ANDROID
            InputRecord.Add(new InputReplay(Time.time - PlayNoteScript.LastStart - (float)config.NoteAdjust / 1000, left ? GameSetting.KeyType.LeftKa : GameSetting.KeyType.RightKa));
#endif
        }
    }
}
