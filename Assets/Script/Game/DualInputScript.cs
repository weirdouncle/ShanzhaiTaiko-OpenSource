using CommonClass;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class DualInputScript : InputScript
{
    public static Dictionary<CourseBranch, Dictionary<bool, List<NoteSoundScript>>> Notes2P = new Dictionary<CourseBranch, Dictionary<bool, List<NoteSoundScript>>>();

    private InputDevice last_device_2p = null;
    private int last_note_2p;
    private float last_time_2p;
    private bool last_left_2p;

    private int judge_good_2p;
    private int judge_perfect_2p;
    private int judge_bad_2p;

    void Start()
    {
        Instance = this;
        InputRecord = new List<InputReplay>();
        config = GameSetting.Config;

        judge_bad = config.JudgeRange[HitNoteResult.Bad] + (GameSetting.Difficulty < Difficulty.Hard ? diff_adjust_bad : 0);
        judge_good = config.JudgeRange[HitNoteResult.Good] + (GameSetting.Difficulty < Difficulty.Hard ? diff_adjust_good : 0);
        judge_perfect = config.JudgeRange[HitNoteResult.Perfect] + (GameSetting.Difficulty < Difficulty.Hard ? diff_adjust_perfect : 0);

        judge_bad_2p = config.JudgeRange[HitNoteResult.Bad] + (GameSetting.Difficulty2P < Difficulty.Hard ? diff_adjust_bad : 0);
        judge_good_2p = config.JudgeRange[HitNoteResult.Good] + (GameSetting.Difficulty2P < Difficulty.Hard ? diff_adjust_good : 0);
        judge_perfect_2p = config.JudgeRange[HitNoteResult.Perfect] + (GameSetting.Difficulty2P < Difficulty.Hard ? diff_adjust_perfect : 0);

        CurrentBranch = CurrentBranch2P = CourseBranch.BranchNormal;
        Quiting = false;
        Holding = HoldStatus.None;

        LoadingScreenScript.Play += PlayStart;

        if (GameSetting.Config.Special != Special.AutoPlay)
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

        if (GameSetting.Special2P != Special.AutoPlay)
        {
            if (GameSetting.DonLeft2P != null)
                GameSetting.DonLeft2P.started += LeftDon2PInvoke;
            if (GameSetting.DonRight2P != null)
                GameSetting.DonRight2P.started += RightDon2PInvoke;
            if (GameSetting.KaLeft2P != null)
                GameSetting.KaLeft2P.started += LeftKa2PInvoke;
            if (GameSetting.KaRight2P != null)
                GameSetting.KaRight2P.started += RightKa2PInvoke;
        }

        if (GameSetting.Config.DirectInput)
        {
            BasicInputScript.KeyInvoke += DirectKey;
            BasicInputScript.KeyInvoke2P += DirectKey2P;
        }
        BasicInputScript.Input.Player.Option.performed += OpenOption;
        BasicInputScript.Input.Player.Esc.performed += QuitGame;
    }

    private void OnDestroy()
    {
        Notes.Clear();
        Notes2P.Clear();

        LoadingScreenScript.Play -= PlayStart;

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

        if (GameSetting.DonLeft2P != null)
            GameSetting.DonLeft2P.started -= LeftDon2PInvoke;
        if (GameSetting.DonRight2P != null)
            GameSetting.DonRight2P.started -= RightDon2PInvoke;
        if (GameSetting.KaLeft2P != null)
            GameSetting.KaLeft2P.started -= LeftKa2PInvoke;
        if (GameSetting.KaRight2P != null)
            GameSetting.KaRight2P.started -= RightKa2PInvoke;

        BasicInputScript.KeyInvoke -= DirectKey;
        BasicInputScript.KeyInvoke2P -= DirectKey2P;
    }

    protected override void PlayStart()
    {
        Holding = HoldStatus.Playing;
        load_finished = true;
    }

    protected override void DirectKey(GameSetting.KeyType key, bool press)
    {
        if (Quiting || Holding != HoldStatus.Playing) return;
        switch (key)
        {
            case GameSetting.KeyType.LeftDon when GameSetting.Config.Special != Special.AutoPlay:
                {
                    Judge(true, true);
                    ShowDon(true);
                }
                break;
            case GameSetting.KeyType.RightDon when GameSetting.Config.Special != Special.AutoPlay:
                {
                    Judge(true, false);
                    ShowDon(false);
                }
                break;
            case GameSetting.KeyType.LeftKa when press && GameSetting.Config.Special != Special.AutoPlay:
                {
                    Judge(false, true);
                    ShowKa(true);
                }
                break;
            case GameSetting.KeyType.RightKa when press && GameSetting.Config.Special != Special.AutoPlay:
                {
                    Judge(false, true);
                    ShowKa(false);
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
    private void DirectKey2P(GameSetting.KeyType key, bool press)
    {
        if (GameSetting.Special2P == Special.AutoPlay || Quiting || Holding != HoldStatus.Playing || !press) return;
        switch (key)
        {
            case GameSetting.KeyType.LeftDon:
                {
                    Judge2P(true, true);
                    ShowDon2P(true);
                }
                break;
            case GameSetting.KeyType.RightDon:
                {
                    Judge2P(true, false);
                    ShowDon2P(false);
                }
                break;
            case GameSetting.KeyType.LeftKa:
                {
                    Judge2P(false, true);
                    ShowKa2P(true);
                }
                break;
            case GameSetting.KeyType.RightKa:
                {
                    Judge2P(false, true);
                    ShowKa2P(false);
                }
                break;
        }
    }

    protected override void Judge(bool don, bool left)
    {
        float current_time = config.NoteAdjust - (Time.time - PlayNoteScript.LastStart) * 1000 * PlayNoteScript.TimeScale + (config.Special == Special.AutoPlay ? 0 : config.JudgeTimeAdjust);
        while (true)
        {
            if (Notes[CurrentBranch][don].Count == 0) return;
            NoteSoundScript note = Notes[CurrentBranch][don][0];
            float time = note.JudgeTime + current_time;

            if (time >= judge_bad)
            {
                last_note = 0;
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

                        SendResult(CommonClass.HitNoteResult.Perfect, note, don);
                    }
                    else if (Math.Abs(time) <= judge_good)    //good
                    {
                        NotesSplitScript.NormalNotes[don].Remove(note);
                        NotesSplitScript.MasterNotes[don].Remove(note);
                        NotesSplitScript.ExpertNotes[don].Remove(note);
                        note.Hit();
                        note.State = time < 0 ? HitPracticeResult.Late : HitPracticeResult.Fast;
                        note.gameObject.SetActive(false);

                        SendResult(CommonClass.HitNoteResult.Good, note, don);
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

                            SendResult(CommonClass.HitNoteResult.Bad, note, don);
                        }
                    }
                    else if (time < 0)
                    {
                        NotesSplitScript.NormalNotes[don].Remove(note);
                        NotesSplitScript.MasterNotes[don].Remove(note);
                        NotesSplitScript.ExpertNotes[don].Remove(note);
                        succeed = false;
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

                        SendResult(CommonClass.HitNoteResult.Perfect, note, don);

                        if (last_device != null && last_device is Gamepad pad)
                            StartCoroutine(Vibration(pad));
                    }
                    else if (Math.Abs(time) <= judge_good)    //good
                    {
                        NotesSplitScript.NormalNotes[don].Remove(note);
                        NotesSplitScript.MasterNotes[don].Remove(note);
                        NotesSplitScript.ExpertNotes[don].Remove(note);
                        note.Hit();
                        note.State = time < 0 ? HitPracticeResult.Late : HitPracticeResult.Fast;
                        note.gameObject.SetActive(false);

                        SendResult(CommonClass.HitNoteResult.Good, note, don);

                        if (last_device != null && last_device is Gamepad pad)
                            StartCoroutine(Vibration(pad));
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

                            SendResult(CommonClass.HitNoteResult.Bad, note, don);
                        }
                    }
                    else if (time < 0)
                    {
                        NotesSplitScript.NormalNotes[don].Remove(note);
                        NotesSplitScript.MasterNotes[don].Remove(note);
                        NotesSplitScript.ExpertNotes[don].Remove(note);
                        succeed = false;
                    }
                    break;
                case 5 when note is RapidScript rapid:         //rapid hit
                    if (time <= 0 && rapid.EndTime + config.NoteAdjust - ((Time.time - PlayNoteScript.LastStart) * 1000 * PlayNoteScript.TimeScale) > 0)
                    {
                        SendResult(CommonClass.HitNoteResult.None, note, true);
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
                        SendResult(CommonClass.HitNoteResult.None, note, true);
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
                        SendResult(CommonClass.HitNoteResult.None, note, true);

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

    private void Judge2P(bool don, bool left)
    {
        float current_time = config.NoteAdjust - (Time.time - PlayNoteScript.LastStart) * 1000 * PlayNoteScript.TimeScale + (GameSetting.Special2P == Special.AutoPlay ? 0 : config.JudgeTimeAdjust);
        while (true)
        {
            if (Notes2P[CurrentBranch2P][don].Count == 0) return;
            NoteSoundScript note = Notes2P[CurrentBranch2P][don][0];
            float time = note.JudgeTime + current_time;

            if (time >= judge_bad_2p)
            {
                last_note_2p = 0;
                return;
            }

            bool succeed = true;
            switch (note.Type)
            {
                case 1:
                case 2:
                    if (Math.Abs(time) <= judge_perfect_2p)   //perfect
                    {
                        DualNotesSplitScript.NormalNotes2P[don].Remove(note);
                        DualNotesSplitScript.MasterNotes2P[don].Remove(note);
                        DualNotesSplitScript.ExpertNotes2P[don].Remove(note);
                        note.Hit();
                        note.State = HitPracticeResult.Perfect;
                        note.gameObject.SetActive(false);

                        GloableAniControllerScript.Instance.ShowResult2P(HitNoteResult.Perfect, note);
                        GloableAniControllerScript.Instance.Hit2P(note, don);
                    }
                    else if (Math.Abs(time) <= judge_good_2p)    //good
                    {
                        DualNotesSplitScript.NormalNotes2P[don].Remove(note);
                        DualNotesSplitScript.MasterNotes2P[don].Remove(note);
                        DualNotesSplitScript.ExpertNotes2P[don].Remove(note);
                        note.Hit();
                        note.State = time < 0 ? HitPracticeResult.Late : HitPracticeResult.Fast;
                        note.gameObject.SetActive(false);

                        GloableAniControllerScript.Instance.ShowResult2P(HitNoteResult.Good, note);
                        GloableAniControllerScript.Instance.Hit2P(note, don);
                    }
                    else if (time > 0 && time < judge_bad_2p)       //bad
                    {
                        if (last_left_2p == left || (!don || last_note_2p != 3) && (don || last_note_2p != 4) || last_time_2p - current_time > 12)
                        {
                            DualNotesSplitScript.NormalNotes2P[don].Remove(note);
                            DualNotesSplitScript.MasterNotes2P[don].Remove(note);
                            DualNotesSplitScript.ExpertNotes2P[don].Remove(note);

                            note.Hit();
                            note.State = HitPracticeResult.Bad;
                            note.gameObject.SetActive(false);

                            GloableAniControllerScript.Instance.ShowResult2P(HitNoteResult.Bad, note);
                        }
                    }
                    else if (time < 0)
                    {
                        DualNotesSplitScript.NormalNotes2P[don].Remove(note);
                        DualNotesSplitScript.MasterNotes2P[don].Remove(note);
                        DualNotesSplitScript.ExpertNotes2P[don].Remove(note);
                        succeed = false;
                    }
                    break;
                case 3:         //big don
                case 4:         //big ka
                    if (Math.Abs(time) <= judge_perfect_2p)   //perfect
                    {
                        DualNotesSplitScript.NormalNotes2P[don].Remove(note);
                        DualNotesSplitScript.MasterNotes2P[don].Remove(note);
                        DualNotesSplitScript.ExpertNotes2P[don].Remove(note);
                        note.Hit();
                        note.State = HitPracticeResult.Perfect;
                        note.gameObject.SetActive(false);

                        GloableAniControllerScript.Instance.ShowResult2P(HitNoteResult.Perfect, note);
                        GloableAniControllerScript.Instance.Hit2P(note, don);

                        if (last_device != null && last_device is Gamepad pad)
                            StartCoroutine(Vibration(pad));
                    }
                    else if (Math.Abs(time) <= judge_good_2p)    //good
                    {
                        DualNotesSplitScript.NormalNotes2P[don].Remove(note);
                        DualNotesSplitScript.MasterNotes2P[don].Remove(note);
                        DualNotesSplitScript.ExpertNotes2P[don].Remove(note);
                        note.Hit();
                        note.State = time < 0 ? HitPracticeResult.Late : HitPracticeResult.Fast;
                        note.gameObject.SetActive(false);

                        GloableAniControllerScript.Instance.ShowResult2P(HitNoteResult.Good, note);
                        GloableAniControllerScript.Instance.Hit2P(note, don);

                        if (last_device_2p != null && last_device_2p is Gamepad pad)
                            StartCoroutine(Vibration(pad));
                    }
                    else if (time < judge_bad_2p)       //bad
                    {
                        if (last_left_2p == left || (!don || last_note_2p != 3) && (don || last_note_2p != 4) || last_time_2p - current_time > 12)
                        {
                            DualNotesSplitScript.NormalNotes2P[don].Remove(note);
                            DualNotesSplitScript.MasterNotes2P[don].Remove(note);
                            DualNotesSplitScript.ExpertNotes2P[don].Remove(note);
                            note.Hit();
                            note.State = HitPracticeResult.Bad;
                            note.gameObject.SetActive(false);

                            GloableAniControllerScript.Instance.ShowResult2P(HitNoteResult.Bad, note);
                        }
                    }
                    else if (time < 0)
                    {
                        DualNotesSplitScript.NormalNotes2P[don].Remove(note);
                        DualNotesSplitScript.MasterNotes2P[don].Remove(note);
                        DualNotesSplitScript.ExpertNotes2P[don].Remove(note);
                        succeed = false;
                    }
                    break;
                case 5 when note is RapidScript rapid:         //rapid hit
                    if (time <= 0 && rapid.EndTime + config.NoteAdjust - ((Time.time - PlayNoteScript.LastStart) * 1000 * PlayNoteScript.TimeScale) > 0)
                    {
                        GloableAniControllerScript.Instance.Hit2P(note, don);
                        note.Hit();
                    }
                    else if (time < 0)
                    {
                        DualNotesSplitScript.NormalNotes2P[don].Remove(note);
                        DualNotesSplitScript.MasterNotes2P[don].Remove(note);
                        DualNotesSplitScript.ExpertNotes2P[don].Remove(note);
                        succeed = false;
                    }
                    break;
                case 6 when note is RapidScript big_rapid:         //big rapid hit
                    if (time <= 0 && big_rapid.EndTime + config.NoteAdjust - ((Time.time - PlayNoteScript.LastStart) * 1000 * PlayNoteScript.TimeScale) > 0)
                    {
                        GloableAniControllerScript.Instance.Hit2P(note, don);
                        note.Hit();

                        if (last_device_2p != null && last_device_2p is Gamepad pad)
                            StartCoroutine(Vibration(pad));
                    }
                    else if (time < 0)
                    {
                        DualNotesSplitScript.NormalNotes2P[don].Remove(note);
                        DualNotesSplitScript.MasterNotes2P[don].Remove(note);
                        DualNotesSplitScript.ExpertNotes2P[don].Remove(note);
                        succeed = false;
                    }
                    break;
                case 7 when note is BallonScript balloon:         //balloon
                    if (time <= 0 && balloon.EndTime + config.NoteAdjust - ((Time.time - PlayNoteScript.LastStart) * 1000 * PlayNoteScript.TimeScale) > 0)
                    {
                        note.Hit();
                        GloableAniControllerScript.Instance.Hit2P(note, don);

                        if (last_device_2p != null && last_device_2p is Gamepad pad)
                            StartCoroutine(Vibration(pad));
                    }
                    else if (time < 0)
                    {
                        DualNotesSplitScript.NormalNotes2P[don].Remove(note);
                        DualNotesSplitScript.MasterNotes2P[don].Remove(note);
                        DualNotesSplitScript.ExpertNotes2P[don].Remove(note);
                        succeed = false;
                    }
                    break;
                case 9 when note is BallonScript balloon:         //kusudama
                    if (time <= 0 && balloon.EndTime + config.NoteAdjust - ((Time.time - PlayNoteScript.LastStart) * 1000 * PlayNoteScript.TimeScale) > 0)
                    {
                        GloableAniControllerScript.Instance.Hit2P(note, don);
                        note.Hit();

                        if (last_device_2p != null && last_device_2p is Gamepad pad)
                            StartCoroutine(Vibration(pad));
                    }
                    else if (time < 0)
                    {
                        DualNotesSplitScript.NormalNotes2P[don].Remove(note);
                        DualNotesSplitScript.MasterNotes2P[don].Remove(note);
                        DualNotesSplitScript.ExpertNotes2P[don].Remove(note);
                        succeed = false;
                    }
                    break;
            }

            if (succeed)
            {
                last_note_2p = note.Type;
                last_time_2p = current_time;
                last_left_2p = left;
                break;
            }
        }
    }

    IEnumerator Vibration(Gamepad pad)
    {
        pad.SetMotorSpeeds(0.8f, 0.8f);
        yield return new WaitForSecondsRealtime(0.3f);
        pad.ResetHaptics();
    }

    protected override void LeftDonInvoke(CallbackContext context)
    {
        if (!Application.isFocused || Quiting || Holding != HoldStatus.Playing) return;

            last_device = context.control.device;
            Judge(true, true);
            ShowDon(true);
    }

    protected override void RightDonInvoke(CallbackContext context)
    {
        if (!Application.isFocused || Quiting || Holding != HoldStatus.Playing) return;

            last_device = context.control.device;
            Judge(true, false);
            ShowDon(false);
    }

    protected override void LeftKaInvoke(CallbackContext context)
    {
        if (!Application.isFocused || Quiting || Holding != HoldStatus.Playing) return;

            last_device = context.control.device;
            Judge(false, true);
            ShowKa(true);
    }

    protected override void RightKaInvoke(CallbackContext context)
    {
        if (!Application.isFocused || Quiting || Holding != HoldStatus.Playing) return;

            last_device = context.control.device;
            Judge(false, false);
            ShowKa(false);
    }

    private void LeftDon2PInvoke(CallbackContext context)
    {
        if (!Application.isFocused || Quiting || Holding != HoldStatus.Playing) return;

            last_device_2p = context.control.device;
            Judge2P(true, true);
            ShowDon2P(true);
    }

    private void RightDon2PInvoke(CallbackContext context)
    {
        if (!Application.isFocused || Quiting || Holding != HoldStatus.Playing) return;


            last_device_2p = context.control.device;
            Judge2P(true, false);
            ShowDon2P(false);
    }

    private void LeftKa2PInvoke(CallbackContext context)
    {
        if (!Application.isFocused || Quiting || Holding != HoldStatus.Playing) return;


            last_device_2p = context.control.device;
            Judge2P(false, true);
            ShowKa2P(true);
    }

    private void RightKa2PInvoke(CallbackContext context)
    {
        if (!Application.isFocused || Quiting || Holding != HoldStatus.Playing) return;

            last_device_2p = context.control.device;
            Judge2P(false, false);
            ShowKa2P(false);
    }

    private bool auto_last_left_2p = false;
    public override void PlayDrum2P(int type)
    {
        if (GameSetting.Special2P != Special.AutoPlay) return;
        switch (type)
        {
            case 1:
                Judge2P(true, auto_last_left_2p);
                ShowDon2P(auto_last_left_2p);
                auto_last_left_2p = !auto_last_left_2p;
                break;
            case 2:
                Judge2P(false, auto_last_left_2p);
                ShowKa2P(auto_last_left_2p);
                auto_last_left_2p = !auto_last_left_2p;
                break;
            case 3:
                Judge2P(true, auto_last_left_2p);
                ShowDon2P(true);
                ShowDon2P(false);
                break;
            case 4:
                Judge2P(false, auto_last_left_2p);
                ShowKa2P(true);
                ShowKa2P(false);
                break;
            case 5:
                Judge2P(true, auto_last_left_2p);
                ShowDon2P(auto_last_left_2p);
                auto_last_left_2p = !auto_last_left_2p;
                break;
            case 6:
                Judge2P(true, auto_last_left_2p);
                ShowDon2P(true);
                ShowDon2P(false);
                break;
            case 7:
            case 9:
                Judge2P(true, auto_last_left_2p);
                ShowDon2P(auto_last_left_2p);
                auto_last_left_2p = !auto_last_left_2p;
                break;
        }
    }
}
