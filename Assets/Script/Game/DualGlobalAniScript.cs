using CommonClass;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DualGlobalAniScript : GloableAniControllerScript
{
    private bool branch_level_hold_2p;
    void Start()
    {
        Instance = this;
        InputScript.QuitPlay += QuitGame;

        BreakingBalloon.BalloonBroken += ShowAvatar;
        BreakingBalloon.BalloonBroken2P += ShowAvatar2;
        KusudamaShowScript.BalloonSucceed += HammerBroken;
        HammerShowScript.BalloonSucceed += HammerBroken;

        LoaderScript.Play += SetVariant;
        OptionScript.SetBranch += SetBranch;
        OptionScript.SetAuto += SetAutoMark;

        string song_name = GameSetting.SelectedInfo.Title;
        SetSongName(song_name);

        if (GameSetting.SelectedInfo.Branches.ContainsKey(GameSetting.Difficulty) && GameSetting.SelectedInfo.Branches[GameSetting.Difficulty])
            BranchBGs[0].SetTrigger("Show");
        if (GameSetting.SelectedInfo.Branches.ContainsKey(GameSetting.Difficulty2P) && GameSetting.SelectedInfo.Branches[GameSetting.Difficulty2P])
            BranchBGs[1].SetTrigger("Show");

        scene = SceneManager.GetActiveScene().name;
        string dif_path = "Texture/Difficulties";
        if (scene.Contains("PS4"))
            dif_path = "Texture/DifficultiesPs4";
        else if (scene.Contains("Nijiiro"))
            dif_path = "Texture/DifficultiesNijiiro";

        foreach (Sprite sprite in Resources.LoadAll<Sprite>(dif_path))
        {
            if (sprite.name == string.Format("lv{0}", (int)GameSetting.Difficulty + 1))
            {
                DifficultyImage[0].sprite = sprite;
            }
            if (sprite.name == string.Format("lv{0}", (int)GameSetting.Difficulty2P + 1))
            {
                DifficultyImage[1].sprite = sprite;
            }
        }
        DifficultyChar[0].sprite = SettingLoader.Diffs[(int)GameSetting.Difficulty];
        DifficultyChar[1].sprite = SettingLoader.Diffs[(int)GameSetting.Difficulty2P];

        BranchChar[0].sprite = SettingLoader.Branches[0];
        BranchChar[1].sprite = SettingLoader.Branches[1];
        if (BranchChar.Length > 2)
        {
            BranchChar[2].sprite = SettingLoader.Branches[0];
            BranchChar[3].sprite = SettingLoader.Branches[1];
        }

        GaugePoints = 0;
        GaugePoints2P = 0;

        ArrengeMarks();

#if UNITY_ANDROID
        if (!GameSetting.Dancer) Dancer = null;
#endif
        if (GameSetting.Mode == CommonClass.PlayMode.Practice)
            Practice.gameObject.SetActive(true);

        if (GameSetting.Replay != null)
            notes_replay = GameSetting.Replay.Show[GameSetting.Config.ScoreMode][(int)GameSetting.Difficulty];
    }

    protected override void ArrengeMarks()
    {

        List<GameObject> marks_1p = new List<GameObject>();
        List<GameObject> marks_2p = new List<GameObject>();
        /*
        marks
        0 练习模式
        1 录像模式
        2 auto
        3 真打
        4 倍速
        5 随机
        6 颠倒
        7 支援
        8 隐身
        9 特殊
        */
        Speed speed = GameSetting.Config.Speed;
        RandomType random = GameSetting.Config.Random;
        bool steal = GameSetting.Config.Steal;
        bool revers = GameSetting.Config.Reverse;
        if (GameSetting.Config.Special == Special.AutoPlay)
        {
            marks_1p.Add(Marks[2]);
            Marks[2].SetActive(true);
        }
        if (GameSetting.Mode == CommonClass.PlayMode.Practice)
        {
            Marks[0].SetActive(true);
            marks_1p.Add(Marks[0]);
        }
        if ((Score)GameSetting.Config.ScoreMode == Score.Shin)
        {
            Marks[3].SetActive(true);
            marks_1p.Add(Marks[3]);
        }
        //速度
        if (speed != Speed.Normal)
        {
            int index = (int)speed - 2;
            if ((int)speed > 10) index -= 6;
            SpriteRenderer renderer = Marks[4].GetComponent<SpriteRenderer>();
            renderer.sprite = SpeedSprites[index];
            Marks[4].SetActive(true);
            marks_1p.Add(Marks[4]);
        }
        //随机
        if (random != RandomType.None)
        {
            int index = (int)random - 1;
            Marks[5].SetActive(true);
            marks_1p.Add(Marks[5]);

            SpriteRenderer renderer = Marks[5].GetComponent<SpriteRenderer>();
            renderer.sprite = RandomSprites[index];
        }
        if (revers)
        {
            Marks[6].SetActive(true);
            marks_1p.Add(Marks[6]);
        }
        if (steal)
        {
            Marks[8].SetActive(true);
            marks_1p.Add(Marks[8]);
        }

        int x = 0, y = 0;
        foreach (GameObject mark in marks_1p)
        {
            mark.transform.localPosition = new Vector3(x * 0.41f - 0.47f, 0.39f - y * 0.41f);
            x++;
            if (x > 2)
            {
                x = 0;
                y++;
            }
        }

        //2P mark
        if (GameSetting.Special2P == Special.AutoPlay)
        {
            Mark2P[2].SetActive(true);
            marks_2p.Add(Mark2P[2]);
        }
        if ((Score)GameSetting.Config.ScoreMode == Score.Shin)
        {
            Mark2P[3].SetActive(true);
            marks_2p.Add(Mark2P[3]);
        }
        if (GameSetting.Speed2P != Speed.Normal)
        {
            int index = (int)GameSetting.Speed2P - 2;
            if ((int)GameSetting.Speed2P > 10) index -= 6;
            SpriteRenderer renderer = Mark2P[4].GetComponent<SpriteRenderer>();
            renderer.sprite = SpeedSprites[index];
            Mark2P[4].SetActive(true);
            marks_2p.Add(Mark2P[4]);
        }
        if (GameSetting.Random2P != RandomType.None)
        {
            int index = (int)GameSetting.Random2P - 1;
            Mark2P[5].SetActive(true);
            marks_2p.Add(Mark2P[5]);

            SpriteRenderer renderer = Mark2P[5].GetComponent<SpriteRenderer>();
            renderer.sprite = RandomSprites[index];
        }
        if (GameSetting.Revers2P)
        {
            Mark2P[6].SetActive(true);
            marks_2p.Add(Mark2P[6]);
        }
        if (GameSetting.Steal2P)
        {
            Mark2P[8].SetActive(true);
            marks_2p.Add(Mark2P[8]);
        }

        x = 0;
        y = 0;
        foreach (GameObject mark in marks_2p)
        {
            mark.transform.localPosition = new Vector3(x * 0.41f - 0.47f, 0.39f - y * 0.41f);
            x++;
            if (x > 2)
            {
                x = 0;
                y++;
            }
        }
    }

    private void OnDestroy()
    {
        InputScript.QuitPlay -= QuitGame;

        BreakingBalloon.BalloonBroken -= ShowAvatar;
        BreakingBalloon.BalloonBroken2P -= ShowAvatar2;
        HammerShowScript.BalloonSucceed -= HammerBroken;
        KusudamaShowScript.BalloonSucceed -= HammerBroken;

        LoaderScript.Play -= SetVariant;
        OptionScript.SetBranch -= SetBranch;
        OptionScript.SetAuto -= SetAutoMark;
    }

    public override void SetVariant()
    {
        Branch = Branch2P = CourseBranch.BranchNormal;
        LoaderScript.Others.Sort((x, y) => { return x.JudgeTime < y.JudgeTime ? -1 : (x.JudgeTime == y.JudgeTime ? (x.Type > y.Type ? -1 : 1) : 1); });
        List<int> times = new List<int>();
        foreach (CChip chip in LoaderScript.Others)
        {
            int time = chip.JudgeTime + GameSetting.Config.NoteAdjust;
            if (!times.Contains(time))
            {
                times.Add(time);
            }
            if (chips.ContainsKey(time))
                chips[time].Add(chip);
            else
            {
                chips[time] = new List<CChip> { chip };
            }

            if (chip.Type == CChip.CType.GOGO_TIME)
            {
                int end = chip.EndTime + GameSetting.Config.NoteAdjust;
                if (!times.Contains(end))
                {
                    times.Add(end);
                }
                if (chips.ContainsKey(end))
                    chips[end].Add(chip);
                else
                {
                    chips[end] = new List<CChip> { chip };
                }
            }
        }

        DualParse.Others2P.Sort((x, y) => { return x.JudgeTime < y.JudgeTime ? -1 : (x.JudgeTime == y.JudgeTime ? (x.Type > y.Type ? -1 : 1) : 1); });
        for (int i = 0; i < DualParse.Others2P.Count; i++)
        {
            CChip chip = DualParse.Others2P[i];
            chip.Player2 = true;
            int time = chip.JudgeTime + GameSetting.Config.NoteAdjust;
            if (!times.Contains(time))
            {
                times.Add(time);
            }
            if (chips.ContainsKey(time))
                chips[time].Add(chip);
            else
            {
                chips[time] = new List<CChip> { chip };
            }

            if (chip.Type == CChip.CType.GOGO_TIME)
            {
                int end = chip.EndTime + GameSetting.Config.NoteAdjust;
                if (!times.Contains(end))
                {
                    times.Add(end);
                }
                if (chips.ContainsKey(end))
                    chips[end].Add(chip);
                else
                {
                    chips[end] = new List<CChip> { chip };
                }
            }
        }

        times.Sort((x, y) => { return x < y ? -1 : 1; });
        foreach (int time in times)
            this.times.Enqueue(time);
    }

    public override void StartGame()
    {
    }

    public override void ResetTimeLine(bool animating)
    {
        Time.timeScale = 1;
        ending_count = 0;
        foreach (KusudamaShowScript kusu in Kusudama)
            kusu.gameObject.SetActive(false);

        foreach (ScoreScript score in ScoreController)
            score.Counting = false;

        SetNoteCombo(NoteSoundScript.ComboType.Combo_None);
        if (GameSetting.Mode == CommonClass.PlayMode.PlayWithReplay)
            SetNoteCombo2P(NoteSoundScript.ComboType.Combo_None);
        TotalScore = 0;
        MaxCombo = 0;
        DrumRolls = 0;
        RestartTime = PlayNoteScript.CurrentTime;
        branch_score = 0;
        bad_count = 0;

        TotalScore2P = 0;
        MaxCombo2P = 0;
        DrumRolls2P = 0;
        branch_score_2p = 0;
        bad_count_2p = 0;
        Rank = Rank2P = NijiiroRank.None;
        branch_level_hold = branch_level_hold_2p = false;
        
        foreach (DonScript don in Don_Avatar)
            don.OnReset();

        StopCoroutine("AddBunus");
        StopCoroutine("AddBunus2");

        foreach (RapidHitScript hit in RapidHit)
            hit.Close(true);

        foreach (ComboScript combo in Combo)
            combo.SetCombo(0);

        foreach (BreakingBalloon balloon in BreakingBalloons)
            balloon.gameObject.SetActive(false);

        for (int i = 0; i < Don_Avatar.Length; i++)
        {
            global_hide[0][i] = false;
            global_hide[1][i] = false;
            Don_Avatar[i].Hide(false, false);

            DonImages[i].SetMax(false);
            DonImages[i].SetMiss(false);
        }

        ComboCount = 0;
        GaugePoints = 0;
        ComboCount2P = 0;
        GaugePoints2P = 0;

        this.times.Clear();
        float bpm = 0;

        List<int> times = new List<int>();
        foreach (CChip chip in LoaderScript.Others)
        {
            int time = chip.JudgeTime + GameSetting.Config.NoteAdjust;
            if ((float)time / 1000 >= PlayNoteScript.CurrentTime && !times.Contains(time))
                times.Add(time);

            if (chip.Type == CChip.CType.GOGO_TIME)
            {
                int end = chip.EndTime + GameSetting.Config.NoteAdjust;
                if ((float)end / 1000 >= PlayNoteScript.CurrentTime && !times.Contains(end))
                {
                    times.Add(end);
                }
            }

            if (chip.Type == CChip.CType.BPM_CHANGE && time / 1000 <= PlayNoteScript.CurrentTime)
                bpm = chip.Bpm;
        }

        foreach (CChip chip in DualParse.Others2P)
        {
            int time = chip.JudgeTime + GameSetting.Config.NoteAdjust;
            if ((float)time / 1000 >= PlayNoteScript.CurrentTime && !times.Contains(time))
                times.Add(time);

            if (chip.Type == CChip.CType.GOGO_TIME)
            {
                int end = chip.EndTime + GameSetting.Config.NoteAdjust;
                if ((float)end / 1000 >= PlayNoteScript.CurrentTime && !times.Contains(end))
                {
                    times.Add(end);
                }
            }
        }

        times.Sort((x, y) => { return x < y ? -1 : 1; });
        foreach (int time in times)
            this.times.Enqueue(time);

        SetBPM(bpm);
        SetBPM2P(bpm);
        SetGoGo(false);
        SetGoGo2P(false);

        foreach (GaugeScript gauge in Gauge)
            gauge.ResetGauge();
        if (Runner != null)
            Runner.ResetTimeLine();

        foreach (ExplosionPoolScript judge in JudgeExplosion)
            judge.ResetTimeLine();

        BranchReset(true);
        BranchReset2P(true);
    }

    protected override void SetBPM(float bpm)
    {
        Don_Avatar[0].SetBpm(bpm);
        if (Runner != null)
            Runner.SetBpm(bpm, false);

        foreach (BreakingBalloon balloon in BreakingBalloons)
            balloon.SetBpm(bpm);
    }

    private void SetBPM2P(float bpm)
    {
        Don_Avatar[1].SetBpm(bpm);
        if (Runner != null)
            Runner.SetBpm(bpm, true);
    }

    void Update()
    {
        if (InputScript.Holding != InputScript.HoldStatus.Playing) return;
        if (times.Count > 0 && times.Peek() - (Time.time - PlayNoteScript.LastStart) * 1000 * PlayNoteScript.TimeScale <= 0.02f)
        {
            int time = times.Dequeue();
            foreach (CChip chip in chips[time])
            {
                if (chip.Type == CChip.CType.GOGO_TIME)
                {
                    if (chip.JudgeTime + GameSetting.Config.NoteAdjust == time)
                    {
                        if (chip.Player2)
                            SetGoGo2P(true);
                        else
                            SetGoGo(true);
                    }
                    else if (chip.EndTime + GameSetting.Config.NoteAdjust == time)
                    {
                        if (chip.Player2)
                            SetGoGo2P(false);
                        else
                            SetGoGo(false);
                    }
                }
                else if (chip.Type == CChip.CType.BPM_CHANGE)
                {
                    if (chip.Player2)
                        SetBPM2P(chip.Bpm);
                    else
                        SetBPM(chip.Bpm);
                }
                else if (chip.Type == CChip.CType.BRANCH_RESET)
                {
                    if (chip.Player2)
                        BranchReset2P(false);
                    else
                        BranchReset(false);
                }
                else if (chip.Type == CChip.CType.BRANCH_NOTICE)
                {
                    if (chip.Player2)
                        BranchInter2P(chip.BranchIndex);
                    else
                        BranchInter(chip.BranchIndex);
                }
                else if (chip.Type == CChip.CType.BRANCH_START)
                {
                    if (!chip.Player2)
                        SetCurrentBranch();
                    else
                        SetCurrentBranch2P();
                }
                else if (chip.Type == CChip.CType.LEVEL_HOLD && chip.Branch == Branch)
                {
                    if (chip.Player2)
                        branch_level_hold_2p = true;
                    else
                        branch_level_hold = true;
                }
                else if (chip.Type == CChip.CType.END)
                {
                    EndPlaying();
                }
            }
        }
    }

    private int ending_count;
    protected override void EndPlaying()
    {
        SetGoGo(false);
        SetGoGo2P(false);
        ending_count++;
        if (ending_count == 2)
        {
            InputScript.Holding = InputScript.HoldStatus.Finished;
            ResultNotice.ShowResult(this);
        }
    }
    public override void EndPlayByNotice()
    {
        notice_count++;
        if (notice_count == 2)
            GameResult.StartAnimating();
    }

    protected override void BranchReset(bool reset)
    {
        branch_score = 0;
        List<HitNoteResult> keys = new List<HitNoteResult>(branch_hit_rate.Keys);
        foreach (HitNoteResult hit in keys)
            branch_hit_rate[hit] = 0;

        if (reset)
        {
            old_branch = Branch = CourseBranch.BranchNormal;
            BranchBGs[0].SetTrigger("Reset");
            if (GameSetting.SelectedInfo.Branches.ContainsKey(GameSetting.Difficulty) && GameSetting.SelectedInfo.Branches[GameSetting.Difficulty])
                BranchBGs[0].SetTrigger("Show");
        }
    }
    private void BranchReset2P(bool reset)
    {
        branch_score_2p = 0;
        List<HitNoteResult> keys = new List<HitNoteResult>(branch_hit_rate_2p.Keys);
        foreach (HitNoteResult hit in keys)
            branch_hit_rate_2p[hit] = 0;

        if (reset)
        {
            old_branch_2p = Branch2P = CourseBranch.BranchNormal;
            BranchBGs[1].SetTrigger("Reset");
            if (GameSetting.SelectedInfo.Branches.ContainsKey(GameSetting.Difficulty2P) && GameSetting.SelectedInfo.Branches[GameSetting.Difficulty2P])
                BranchBGs[1].SetTrigger("Show");
        }
    }

    protected override void SetBranch(CourseBranch branch)
    {
        old_branch = Branch = branch;
        switch (branch)
        {
            case CourseBranch.BranchNormal:
                BranchBGs[0].SetTrigger("Level2N");
                break;
            case CourseBranch.BranchExpert:
                BranchBGs[0].SetTrigger("Level2E");
                break;
            case CourseBranch.BranchMaster:
                BranchBGs[0].SetTrigger("Level2M");
                break;
        }
    }
    protected override void SetBranch2P(CourseBranch branch)
    {
        old_branch_2p = Branch2P = branch;
        switch (branch)
        {
            case CourseBranch.BranchNormal:
                BranchBGs[1].SetTrigger("Level2N");
                break;
            case CourseBranch.BranchExpert:
                BranchBGs[1].SetTrigger("Level2E");
                break;
            case CourseBranch.BranchMaster:
                BranchBGs[1].SetTrigger("Level2M");
                break;
        }
    }

    protected override void SetCurrentBranch()
    {
        InputScript.CurrentBranch = Branch;
        List<HitNoteResult> keys = new List<HitNoteResult>(branch_hit_rate.Keys);
        foreach (HitNoteResult hit in keys)
            branch_hit_rate[hit] = 0;

        branch_score = 0;
    }
    private void SetCurrentBranch2P()
    {
        InputScript.CurrentBranch2P = Branch2P;
        List<HitNoteResult> keys = new List<HitNoteResult>(branch_hit_rate_2p.Keys);
        foreach (HitNoteResult hit in keys)
            branch_hit_rate_2p[hit] = 0;

        branch_score_2p = 0;
    }
    protected override void BranchInter(int index)
    {
        if (branch_level_hold)
        {
            branch_level_hold = false;
        }
        else
        {
            CourseBranch result = CourseBranch.BranchNormal;
            CBRANCH branch = LoaderScript.ListBRANCH[index];
            switch (branch.n分岐の種類)
            {
                case 0:                                 //良率
                    int count = branch.NoteCount + branch.BranchCount[Branch];
                    float good = 0;
                    List<HitNoteResult> keys = new List<HitNoteResult>(branch_hit_rate.Keys);
                    foreach (HitNoteResult hit in keys)
                    {
                        if (hit == HitNoteResult.None) continue;
                        good += (hit == HitNoteResult.Perfect ? 1 : hit == HitNoteResult.Good ? 0.5f : 0) * branch_hit_rate[hit];
                    }
                    //Debug.Log(string.Format("good {0} count {1}",good, count));

                    float rate = count > 0 ? good / count * 100 : 0;
                    //Debug.Log(string.Format("branch index {3} hit rate {0} condition1 {1} congdition2 {2}", rate, branch.n条件数値B, branch.n条件数値A, index));

                    if (rate >= branch.n条件数値B)
                    {
                        result = CourseBranch.BranchMaster;
                    }
                    else if (rate >= branch.n条件数値A)
                    {
                        result = CourseBranch.BranchExpert;
                    }
                    break;
                case 1:                                 //连打数
                    //Debug.Log(string.Format("hit count {0} condition1 {1} congdition2 {2}", branch_hit_rate[HitNoteResult.None], branch.n条件数値B, branch.n条件数値A));
                    if (branch_hit_rate[HitNoteResult.None] >= branch.n条件数値B)
                    {
                        result = CourseBranch.BranchMaster;
                    }
                    else if (branch_hit_rate[HitNoteResult.None] >= branch.n条件数値A)
                    {
                        result = CourseBranch.BranchExpert;
                    }
                    break;
                case 2:                                 //分数
                    if (branch_score >= branch.n条件数値B)
                    {
                        result = CourseBranch.BranchMaster;
                    }
                    else if (branch_score >= branch.n条件数値A)
                    {
                        result = CourseBranch.BranchExpert;
                    }
                    break;
                case 3:                                 //？？？
                    break;
            }

            old_branch = Branch;
            Branch = result;
            if (old_branch < Branch)
            {
                BranchLevel[0].SetTrigger("LevelUp");
            }
            else if (old_branch > Branch)
            {
                BranchLevel[0].SetTrigger("LevelDown");
            }

            if ((Option == null || Option.Branch == CourseBranch.BranchNormal) && Branch != old_branch)
            {
                ChangeBranch(branch.n現在の小節);
            }
        }
    }

    private void BranchInter2P(int index)
    {
        if (branch_level_hold_2p)
        {
            branch_level_hold_2p = false;
        }
        else
        {
            CourseBranch result2 = CourseBranch.BranchNormal;
            CBRANCH branch = DualParse.ListBRANCH2P[index];
            /*
            if ((Score)GameSetting.Config.ScoreMode == Score.Nijiiro)
            {
                result2 = CourseBranch.BranchMaster;
            }
            else
            {
            */
                switch (branch.n分岐の種類)
                {
                    case 0:                                 //良率
                        int count = branch.NoteCount + branch.BranchCount[Branch];
                        float good2 = 0;
                        List<HitNoteResult> keys2 = new List<HitNoteResult>(branch_hit_rate_2p.Keys);
                        foreach (HitNoteResult hit in keys2)
                        {
                            if (hit == HitNoteResult.None) continue;
                            good2 += (hit == HitNoteResult.Perfect ? 1 : hit == HitNoteResult.Good ? 0.5f : 0) * branch_hit_rate_2p[hit];
                        }
                        float rate2 = count > 0 ? good2 / count * 100 : 0;

                        //Debug.Log(rate2);
                        if (rate2 >= branch.n条件数値B)
                        {
                            result2 = CourseBranch.BranchMaster;
                        }
                        else if (rate2 >= branch.n条件数値A)
                        {
                            result2 = CourseBranch.BranchExpert;
                        }
                        break;
                    case 1:                                 //连打数
                        if (branch_hit_rate_2p[HitNoteResult.None] >= branch.n条件数値B)
                        {
                            result2 = CourseBranch.BranchMaster;
                        }
                        else if (branch_hit_rate_2p[HitNoteResult.None] >= branch.n条件数値A)
                        {
                            result2 = CourseBranch.BranchExpert;
                        }

                        break;
                    case 2:                                 //分数
                        if (branch_score_2p >= branch.n条件数値B)
                        {
                            result2 = CourseBranch.BranchMaster;
                        }
                        else if (branch_score_2p >= branch.n条件数値A)
                        {
                            result2 = CourseBranch.BranchExpert;
                        }
                        break;
                    case 3:                                 //？？？
                        break;
                }
            //}

            old_branch_2p = Branch2P;
            Branch2P = result2;
            if (old_branch_2p < Branch2P)
            {
                BranchLevel[1].SetTrigger("LevelUp");
            }
            else if (old_branch_2p > Branch2P)
            {
                BranchLevel[1].SetTrigger("LevelDown");
            }

            if (Branch2P != old_branch_2p)
            {
                ChangeBranch2P(branch.n現在の小節);
            }
        }
    }

    public override void ShowRapid(bool show)
    {
        if (!show && RapidHit[0].gameObject.activeSelf)
        {
            RapidHit[0].Close(false);
        }
    }
    
    public override void ShowBalloon(NoteSoundScript note, int Phase)
    {
        if (note.Type == 9)
        {
            if (Phase == 0 && note is KusudamaNoteScript script)
            {
                for (int i = 0; i < Don_Avatar.Length; i++)
                {
                    global_hide[1][i] = true;
                    Don_Avatar[i].Hide(true, false);
                    DonImages[i].SetBalloon(true);
                }
                Kusudama[0].Play(script.HitCount * 2);
            }
            else if (Phase == 0 && note is HammerScript hammer)
            {
                for (int i = 0; i < Don_Avatar.Length; i++)
                {
                    global_hide[1][i] = true;
                    Don_Avatar[i].Hide(true, false);
                    DonImages[i].SetBalloon(true);
                }

                if (Kusudama[0] is HammerShowScript hammer_show)
                {
                    int adjust = GameSetting.Config.NoteAdjust;
                    if (hammer.MidTime + adjust - ((Time.time - PlayNoteScript.LastStart) * 1000 * PlayNoteScript.TimeScale)
                    + (GameSetting.Config.Special == Special.AutoPlay ? 0 : GameSetting.Config.JudgeTimeAdjust) < 0)
                    {
                        hammer_show.Silver.SetActive(true);
                        hammer_show.Gold.SetActive(false);
                    }
                    else
                    {
                        hammer_show.Silver.SetActive(false);
                        hammer_show.Gold.SetActive(true);
                    }
                }
                Kusudama[0].Play(hammer.HitCount * 2);
            }
        }
        else if (note is BallonScript balloon)
        {
            switch (Phase)
            {
                case 0 when note is BallonScript script:
                    global_hide[0][0] = true;
                    BreakingBalloons[0].ReStart(script);
                    break;
                case 1:
                    BreakingBalloons[0].SetPhase(BreakingBalloon.Phase.Start, balloon);
                    break;
                case 2:
                    BreakingBalloons[0].SetPhase(BreakingBalloon.Phase.More, balloon);
                    break;
                case 3:
                    BreakingBalloons[0].SetPhase(BreakingBalloon.Phase.Nearly, balloon);
                    break;
                case 4:
                    BreakingBalloons[0].SetPhase(BreakingBalloon.Phase.Almost, balloon);
                    break;
            }
        }
    }

    public override void ShowBalloon2P(NoteSoundScript note, int Phase)
    {
        if (note.Type == 9)
        {
        }
        else if (note is BallonScript balloon)
        {
            switch (Phase)
            {
                case 0 when note is BallonScript script:
                    global_hide[0][1] = true;
                    BreakingBalloons[1].ReStart(script);
                    break;
                case 1:
                    BreakingBalloons[1].SetPhase(BreakingBalloon.Phase.Start, balloon);
                    break;
                case 2:
                    BreakingBalloons[1].SetPhase(BreakingBalloon.Phase.More, balloon);
                    break;
                case 3:
                    BreakingBalloons[1].SetPhase(BreakingBalloon.Phase.Nearly, balloon);
                    break;
                case 4:
                    BreakingBalloons[1].SetPhase(BreakingBalloon.Phase.Almost, balloon);
                    break;
            }
        }
    }

    public override void HammerChange()
    {
        if (Kusudama[0] is HammerShowScript hammer)
            hammer.ChangeSilver();
    }

    public override void Hit(NoteSoundScript note, bool don)
    {
        switch (note.Type)
        {
            case 5:
            case 6:
                if (!RapidHit[0].Activated) RapidHit[0].Show();

                RapidHit[0].Hit();
                if (SuccessfulBg.Length > 1) SuccessfulBg[1].SetTrigger("Hit");
                DrumRolls++;
                branch_hit_rate[HitNoteResult.None]++;
                CaculateRapiHit(note);

                if (don)
                    rapid_don = true;
                else
                    rapid_ka = true;
                rapid = note;

                break;
            case 7:
                BreakingBalloons[0].Hit((BallonScript)note);
                if (BreakingBalloons[0].Number.Count > 0)
                    CaculaBalloonHit(note);
                break;
            case 9:
                Kusudama[0].Hit();
                if (Kusudama[0].Number.Count > 0)
                    CaculaBalloonHit(note);
                break;
        }
    }

    public override void Hit2P(NoteSoundScript note, bool don)
    {
        switch (note.Type)
        {
            case 5:
            case 6:
                if (!RapidHit[1].Activated) RapidHit[1].Show();

                RapidHit[1].Hit();
                if (SuccessfulBg.Length > 1) SuccessfulBg[3].SetTrigger("Hit");
                DrumRolls2P++;
                branch_hit_rate_2p[HitNoteResult.None]++;
                CaculateRapiHit2P(note);

                if (don)
                    rapid_don_2p = true;
                else
                    rapid_ka_2p = true;
                rapid_2p = note;

                break;
            case 7:
                BreakingBalloons[1].Hit((BallonScript)note);
                if (BreakingBalloons[1].Number.Count > 0)
                    CaculaBalloonHit2P(note);
                break;
            case 9:
                Kusudama[0].Hit(true);
                if (note is HammerScript hammer && hammer.HitCount > 0)
                    CaculaBalloonHit2P(note);
                break;
        }
    }

    void LateUpdate()
    {
        if (rapid_don)
        {
            rapid_don = false;
            JumpOut[0].Play(rapid, true);
        }
        if (rapid_ka)
        {
            rapid_ka = false;
            JumpOut[0].Play(rapid, false);
        }
        if (rapid_don_2p)
        {
            rapid_don_2p = false;
            JumpOut[1].Play(rapid_2p, true);
        }
        if (rapid_ka_2p)
        {
            rapid_ka_2p = false;
            JumpOut[1].Play(rapid_2p, false);
        }
    }

    public override void BrokenBalloon(NoteSoundScript note, BalloonResult result)
    {
        if (note.Type == 9)
        {
            if (result == BalloonResult.Fail)
            {
                Kusudama[0].Hide();
            }
        }
        else
        {
            if (result == BalloonResult.Fail)
            {
                BreakingBalloons[0].Fail((BallonScript)note);
            }
            else
            {
                BreakingBalloons[0].Broken();
                JumpOut[0].Play(note, true);
                CaculaBalloonScore(note);
            }
        }
    }

    protected override void HammerBroken(BalloonResult result)
    {
        bool good = result == BalloonResult.Perfect;
        Kusudama[0].ShowResult(good);
        CaculaKusudama(InputScript.Notes[InputScript.CurrentBranch][true][0], good);
        CaculaKusudama2P(DualInputScript.Notes2P[InputScript.CurrentBranch2P][true][0], good);
    }

    protected override void CaculateScore(NoteSoundScript note, HitNoteResult result)
    {
        if (result != HitNoteResult.Bad)
        {
            double points;
            if ((Score)GameSetting.Config.ScoreMode == Score.Normal)
            {
                points = LoaderScript.ScoreInit[0, (int)GameSetting.Difficulty];
                double diff = LoaderScript.ScoreDiff[(int)GameSetting.Difficulty];
                if (LoaderScript.ScoreModeTmp == 0)
                {
                    points = ComboCount >= 200 ? 2000 : 1000;
                }
                else if (LoaderScript.ScoreModeTmp == 1)
                {
                    points += Math.Max(0, diff * Mathf.Floor((Math.Min(ComboCount, 100) - 1) / 10));
                }
                else if (LoaderScript.ScoreModeTmp == 2)
                {
                    int multi = 0;
                    if (ComboCount >= 100)
                        multi = 8;
                    else if (ComboCount >= 50)
                        multi = 4;
                    else if (ComboCount >= 30)
                        multi = 2;
                    else if (ComboCount >= 10)
                        multi = 1;
                    points += multi * diff;
                }
            }
            else
            {
                points = PlayNoteScript.ShinScorePerNote;
            }

            if ((Score)GameSetting.Config.ScoreMode == Score.Normal && note.Gogo) points *= 1.2;
            if ((Score)GameSetting.Config.ScoreMode != Score.Nijiiro && (note.Type == 3 || note.Type == 4))
                points *= 2;
            if (result == HitNoteResult.Good) points /= 2;
            int new_p = (int)points / 10;
            ScoreController[0].SetScore(new_p * 10);
            TotalScore += new_p * 10;
            branch_score += new_p * 10;
        }

        //Debug.Log(TotalScore);
    }

    protected override void CaculateScore2P(NoteSoundScript note, HitNoteResult result)
    {
        if (result != HitNoteResult.Bad)
        {
            double points;
            if ((Score)GameSetting.Config.ScoreMode == Score.Normal)
            {
                points = LoaderScript.ScoreInit[0, (int)GameSetting.Difficulty2P];
                double diff = LoaderScript.ScoreDiff[(int)GameSetting.Difficulty2P];
                if (LoaderScript.ScoreModeTmp == 0)
                {
                    points = ComboCount2P >= 200 ? 2000 : 1000;
                }
                else if (LoaderScript.ScoreModeTmp == 1)
                {
                    points += Math.Max(0, diff * Mathf.Floor((Math.Min(ComboCount2P, 100) - 1) / 10));
                }
                else if (LoaderScript.ScoreModeTmp == 2)
                {
                    int multi = 0;
                    if (ComboCount2P >= 100)
                        multi = 8;
                    else if (ComboCount2P >= 50)
                        multi = 4;
                    else if (ComboCount2P >= 30)
                        multi = 2;
                    else if (ComboCount2P >= 10)
                        multi = 1;
                    points += multi * diff;
                }
            }
            else
            {
                points = PlayNoteScript.ShinScorePerNote2P;
            }

            if ((Score)GameSetting.Config.ScoreMode == Score.Normal && note.Gogo) points *= 1.2;
            if ((Score)GameSetting.Config.ScoreMode != Score.Nijiiro && (note.Type == 3 || note.Type == 4)) 
                points *= 2;
            if (result == HitNoteResult.Good) points /= 2;
            int new_p = (int)points / 10;
            ScoreController[1].SetScore(new_p * 10);
            TotalScore2P += new_p * 10;
            branch_score_2p += new_p * 10;

        }
    }

    public override void ShowResult(HitNoteResult state, NoteSoundScript note)
    {
        if (GameSetting.Mode != CommonClass.PlayMode.Practice && GameSetting.Config.Special != Special.None)
        {
            switch (GameSetting.Config.Special)
            {
                case Special.AllPerfect when state == HitNoteResult.Bad:
                case Special.DAllPerfect when state != HitNoteResult.Perfect:
                    note.gameObject.SetActive(true);
                    QuitGameByMiss();
                    return;
                case Special.Training when state == HitNoteResult.Bad:
                case Special.DTraining when state != HitNoteResult.Perfect:
                    note.gameObject.SetActive(true);
                    RestartByMiss();
                    return;
            }
        }

        bool don = note.Type == 1 || note.Type == 3;
        JudgeExplosion[0].Show(state, note);
        switch (state)
        {
            case HitNoteResult.Bad:
                ComboCount = 0;
                branch_hit_rate[HitNoteResult.Bad]++;
                Don_Avatar[0].Miss();
                break;
            case HitNoteResult.Perfect:
                branch_hit_rate[HitNoteResult.Perfect]++;
                ComboCount++;
                JumpOut[0].Play(note, don);
                if (SuccessfulBg.Length > 1) SuccessfulBg[0].SetTrigger("Hit");
                if (bad_count == 6)
                {
                    Don_Avatar[0].SetMiss(false);
                    DonImages[0].SetMiss(false);
                }
                bad_count = 0;
                break;
            case HitNoteResult.Good:
                branch_hit_rate[HitNoteResult.Good]++;
                ComboCount++;
                JumpOut[0].Play(note, don);
                if (SuccessfulBg.Length > 1) SuccessfulBg[0].SetTrigger("Hit");
                if (bad_count == 6)
                {
                    Don_Avatar[0].SetMiss(false);
                    DonImages[0].SetMiss(false);
                }
                bad_count = 0;
                break;
        }
        if (Runner != null)
            Runner.OnPlay(state != HitNoteResult.Bad);
        if (ComboCount > MaxCombo) MaxCombo = ComboCount;
        if (scene.Contains("Nijiiro"))
        {
            if (ComboCount == 0)
            {
                SetNoteCombo(NoteSoundScript.ComboType.Combo_None);
            }
            else if (ComboCount == 100)
            {
                SetNoteCombo(NoteSoundScript.ComboType.Combo_300);
            }
            else if (ComboCount == 50)
            {
                SetNoteCombo(NoteSoundScript.ComboType.Combo_150);
            }
            else if (ComboCount == 10)
            {
                SetNoteCombo(NoteSoundScript.ComboType.Combo_50);
            }
        }
        else
        {
            if (ComboCount == 0)
            {
                SetNoteCombo(NoteSoundScript.ComboType.Combo_None);
            }
            else if (ComboCount == 300)
            {
                SetNoteCombo(NoteSoundScript.ComboType.Combo_300);
            }
            else if (ComboCount == 150)
            {
                SetNoteCombo(NoteSoundScript.ComboType.Combo_150);
            }
            else if (ComboCount == 50)
            {
                SetNoteCombo(NoteSoundScript.ComboType.Combo_50);
            }
        }

        Combo[0].SetCombo(ComboCount);
        CountPoints(state, note);

        if (ComboCount >= 10 && ComboCount % 10 == 0)
        {
            //Avatar.SetTrigger("Combo");
            Don_Avatar[0].Combo();
        }

        if (ComboCount >= 50 && (ComboCount == 50 || ComboCount % 100 == 0))
            ComboBonus[0].Show(ComboCount);

        CaculateScore(note, state);
        if (ComboCount >= 100 && (Score)GameSetting.Config.ScoreMode == Score.Normal && LoaderScript.ScoreModeTmp == 2 && ComboCount % 100 == 0)
        {
            StartCoroutine("AddBunus");
            TotalScore += 10000;
            branch_score += 10000;
        }
    }

    IEnumerator AddBunus()
    {
        if (scene.Contains("PS4") || scene.Contains("Nijiiro"))
        {
            ScoreController[0].AddBonusDiretly(10000);
        }
        else
        {
            yield return new WaitForSeconds(1.8f);
            ScoreController[0].AddBonus(10000);
        }
    }

    IEnumerator AddBunus2()
    {
        if (scene.Contains("PS4") || scene.Contains("Nijiiro"))
        {
            ScoreController[1].AddBonusDiretly(10000);
        }
        else
        {
            yield return new WaitForSeconds(1.8f);
            ScoreController[1].AddBonus(10000);
        }
    }

    public override void ShowResult2P(HitNoteResult state, NoteSoundScript note)
    {
        bool don = note.Type == 1 || note.Type == 3;
        JudgeExplosion[1].Show(state, note);
        switch (state)
        {
            case HitNoteResult.Bad:
                ComboCount2P = 0;
                branch_hit_rate_2p[HitNoteResult.Bad]++;
                Don_Avatar[1].Miss();
                break;
            case HitNoteResult.Perfect:
                branch_hit_rate_2p[HitNoteResult.Perfect]++;
                ComboCount2P++;
                JumpOut[1].Play(note, don);
                if (SuccessfulBg.Length > 1) SuccessfulBg[2].SetTrigger("Hit");
                if (bad_count_2p == 6)
                {
                    Don_Avatar[1].SetMiss(false);
                    DonImages[1].SetMiss(false);
                }
                bad_count_2p = 0;
                break;
            case HitNoteResult.Good:
                branch_hit_rate_2p[HitNoteResult.Good]++;
                ComboCount2P++;
                JumpOut[1].Play(note, don);
                if (SuccessfulBg.Length > 1) SuccessfulBg[2].SetTrigger("Hit");
                if (bad_count_2p == 6)
                {
                    Don_Avatar[1].SetMiss(false);
                    DonImages[1].SetMiss(false);
                }
                bad_count_2p = 0;
                break;
        }
        if (Runner != null)
            Runner.OnPlay(state != HitNoteResult.Bad, false);
        if (ComboCount2P > MaxCombo2P) MaxCombo2P = ComboCount2P;

        if (scene.Contains("Nijiiro"))
        {
            if (ComboCount2P == 0)
            {
                SetNoteCombo2P(NoteSoundScript.ComboType.Combo_None);
            }
            else if (ComboCount2P == 100)
            {
                SetNoteCombo2P(NoteSoundScript.ComboType.Combo_300);
            }
            else if (ComboCount2P == 50)
            {
                SetNoteCombo2P(NoteSoundScript.ComboType.Combo_150);
            }
            else if (ComboCount2P == 10)
            {
                SetNoteCombo2P(NoteSoundScript.ComboType.Combo_50);
            }
        }
        else
        {
            if (ComboCount2P == 0)
            {
                SetNoteCombo2P(NoteSoundScript.ComboType.Combo_None);
            }
            else if (ComboCount2P == 300)
            {
                SetNoteCombo2P(NoteSoundScript.ComboType.Combo_300);
            }
            else if (ComboCount2P == 150)
            {
                SetNoteCombo2P(NoteSoundScript.ComboType.Combo_150);
            }
            else if (ComboCount2P == 50)
            {
                SetNoteCombo2P(NoteSoundScript.ComboType.Combo_50);
            }
        }

        Combo[1].SetCombo(ComboCount2P);
        CountPoints2P(state, note);

        if (ComboCount >= 10 && ComboCount2P % 10 == 0)
        {
            Don_Avatar[1].Combo();
        }

        if (ComboCount2P >= 50 && (ComboCount2P == 50 || ComboCount2P % 100 == 0))
            ComboBonus[1].Show(ComboCount2P);

        CaculateScore2P(note, state);
        if (ComboCount >= 100 && (Score)GameSetting.Config.ScoreMode == Score.Normal && LoaderScript.ScoreModeTmp == 2 && ComboCount2P % 100 == 0)
        {
            StartCoroutine("AddBunus2");
            TotalScore2P += 10000;
            branch_score_2p += 10000;
        }
    }

    protected override void CountPoints(HitNoteResult state, NoteSoundScript note)
    {
        float every = (float)13000 / PlayNoteScript.Normal_notes_count;
        float variation = 0;

        float scale = 1;
        if (note.BranchRelated)
        {
            switch (note.Branch)
            {
                case CourseBranch.BranchNormal:
                    scale = 2;
                    break;
                case CourseBranch.BranchExpert:
                    scale = 1.5f;
                    break;
                case CourseBranch.BranchMaster:
                    scale = 1.2f;
                    break;
            }
        }

        switch (GameSetting.Difficulty)
        {
            case Difficulty.Easy:
                switch (state)
                {
                    case HitNoteResult.Good:
                        variation = 0.75f * scale * every;
                        PlayNoteScript.Result[HitNoteResult.Good].Add(note.Index);
                        break;
                    case HitNoteResult.Perfect:
                        variation += scale * every;
                        PlayNoteScript.Result[HitNoteResult.Perfect].Add(note.Index);
                        break;
                    case HitNoteResult.Bad:
                        variation -= scale * every / 2;
                        PlayNoteScript.Result[HitNoteResult.Bad].Add(note.Index);
                        break;
                }
                break;
            case Difficulty.Normal:
                switch (state)
                {
                    case HitNoteResult.Good:
                        variation = 0.75f * scale * every;
                        PlayNoteScript.Result[HitNoteResult.Good].Add(note.Index);
                        break;
                    case HitNoteResult.Perfect:
                        variation += scale * every;
                        PlayNoteScript.Result[HitNoteResult.Perfect].Add(note.Index);
                        break;
                    case HitNoteResult.Bad when GameSetting.SelectedInfo.Levels[GameSetting.Difficulty] <= 3:
                        variation -= scale * every / 2;
                        PlayNoteScript.Result[HitNoteResult.Bad].Add(note.Index);
                        break;
                    case HitNoteResult.Bad when GameSetting.SelectedInfo.Levels[GameSetting.Difficulty] == 4:
                        variation -= scale * every * 0.75f;
                        PlayNoteScript.Result[HitNoteResult.Bad].Add(note.Index);
                        break;
                    case HitNoteResult.Bad:
                        variation -= scale * every;
                        PlayNoteScript.Result[HitNoteResult.Bad].Add(note.Index);
                        break;
                }
                break;
            case Difficulty.Hard:
                {
                    switch (state)
                    {
                        case HitNoteResult.Good:
                            variation = 0.75f * scale * every;
                            PlayNoteScript.Result[HitNoteResult.Good].Add(note.Index);
                            break;
                        case HitNoteResult.Perfect:
                            variation += scale * every;
                            PlayNoteScript.Result[HitNoteResult.Perfect].Add(note.Index);
                            break;
                        case HitNoteResult.Bad when GameSetting.SelectedInfo.Levels[GameSetting.Difficulty] <= 2:
                            variation -= 0.75f * scale * every;
                            PlayNoteScript.Result[HitNoteResult.Bad].Add(note.Index);
                            break;
                        case HitNoteResult.Bad when GameSetting.SelectedInfo.Levels[GameSetting.Difficulty] == 3:
                            variation -= scale * every;
                            PlayNoteScript.Result[HitNoteResult.Bad].Add(note.Index);
                            break;
                        case HitNoteResult.Bad when GameSetting.SelectedInfo.Levels[GameSetting.Difficulty] == 4:
                            variation -= scale * every * 7 / 6f;
                            PlayNoteScript.Result[HitNoteResult.Bad].Add(note.Index);
                            break;
                        case HitNoteResult.Bad:
                            variation -= scale * every * 5 / 4f;
                            PlayNoteScript.Result[HitNoteResult.Bad].Add(note.Index);
                            break;
                    }
                }
                break;
            default:
                {
                    switch (state)
                    {
                        case HitNoteResult.Good:
                            variation = 0.5f * scale * every;
                            PlayNoteScript.Result[HitNoteResult.Good].Add(note.Index);
                            break;
                        case HitNoteResult.Perfect:
                            variation += scale * every;
                            PlayNoteScript.Result[HitNoteResult.Perfect].Add(note.Index);
                            break;
                        case HitNoteResult.Bad when GameSetting.SelectedInfo.Levels[GameSetting.Difficulty] <= 7:
                            variation -= scale * every * 8 / 5f;
                            PlayNoteScript.Result[HitNoteResult.Bad].Add(note.Index);
                            break;
                        case HitNoteResult.Bad:
                            variation -= 2 * scale * every;
                            PlayNoteScript.Result[HitNoteResult.Bad].Add(note.Index);
                            break;
                    }
                }
                break;
        }

        GaugePoints += variation;
        GaugePoints = Math.Max(0, GaugePoints);
        GaugePoints = Math.Min(10000, GaugePoints);

        Gauge[0].UpdateGauge();
    }

    private void CountPoints2P(HitNoteResult state, NoteSoundScript note)
    {
        float every = (float)13000 / PlayNoteScript.Normal_notes_count_2p;
        float variation = 0;

        float scale = 1;
        if (note.BranchRelated)
        {
            switch (note.Branch)
            {
                case CourseBranch.BranchNormal:
                    scale = 2;
                    break;
                case CourseBranch.BranchExpert:
                    scale = 1.5f;
                    break;
                case CourseBranch.BranchMaster:
                    scale = 1.2f;
                    break;
            }
        }

        switch (GameSetting.Difficulty2P)
        {
            case Difficulty.Easy:
                switch (state)
                {
                    case HitNoteResult.Good:
                        variation = 0.75f * scale * every;
                        PlayNoteScript.Result2P[HitNoteResult.Good].Add(note.Index);
                        break;
                    case HitNoteResult.Perfect:
                        variation += scale * every;
                        PlayNoteScript.Result2P[HitNoteResult.Perfect].Add(note.Index);
                        break;
                    case HitNoteResult.Bad:
                        variation -= scale * every / 2;
                        PlayNoteScript.Result2P[HitNoteResult.Bad].Add(note.Index);
                        break;
                }
                break;
            case Difficulty.Normal:
                switch (state)
                {
                    case HitNoteResult.Good:
                        variation = 0.75f * scale * every;
                        PlayNoteScript.Result2P[HitNoteResult.Good].Add(note.Index);
                        break;
                    case HitNoteResult.Perfect:
                        variation += scale * every;
                        PlayNoteScript.Result2P[HitNoteResult.Perfect].Add(note.Index);
                        break;
                    case HitNoteResult.Bad when GameSetting.SelectedInfo.Levels[GameSetting.Difficulty] <= 3:
                        variation -= scale * every / 2;
                        PlayNoteScript.Result2P[HitNoteResult.Bad].Add(note.Index);
                        break;
                    case HitNoteResult.Bad when GameSetting.SelectedInfo.Levels[GameSetting.Difficulty] == 4:
                        variation -= scale * every * 0.75f;
                        PlayNoteScript.Result2P[HitNoteResult.Bad].Add(note.Index);
                        break;
                    case HitNoteResult.Bad:
                        variation -= scale * every;
                        PlayNoteScript.Result2P[HitNoteResult.Bad].Add(note.Index);
                        break;
                }
                break;
            case Difficulty.Hard:
                {
                    switch (state)
                    {
                        case HitNoteResult.Good:
                            variation = 0.75f * scale * every;
                            PlayNoteScript.Result2P[HitNoteResult.Good].Add(note.Index);
                            break;
                        case HitNoteResult.Perfect:
                            variation += scale * every;
                            PlayNoteScript.Result2P[HitNoteResult.Perfect].Add(note.Index);
                            break;
                        case HitNoteResult.Bad when GameSetting.SelectedInfo.Levels[GameSetting.Difficulty] <= 2:
                            variation -= 0.75f * scale * every;
                            PlayNoteScript.Result2P[HitNoteResult.Bad].Add(note.Index);
                            break;
                        case HitNoteResult.Bad when GameSetting.SelectedInfo.Levels[GameSetting.Difficulty] == 3:
                            variation -= scale * every;
                            PlayNoteScript.Result2P[HitNoteResult.Bad].Add(note.Index);
                            break;
                        case HitNoteResult.Bad when GameSetting.SelectedInfo.Levels[GameSetting.Difficulty] == 4:
                            variation -= scale * every * 7 / 6f;
                            PlayNoteScript.Result2P[HitNoteResult.Bad].Add(note.Index);
                            break;
                        case HitNoteResult.Bad:
                            variation -= scale * every * 5 / 4f;
                            PlayNoteScript.Result2P[HitNoteResult.Bad].Add(note.Index);
                            break;
                    }
                }
                break;
            default:
                {
                    switch (state)
                    {
                        case HitNoteResult.Good:
                            variation = 0.5f * scale * every;
                            PlayNoteScript.Result2P[HitNoteResult.Good].Add(note.Index);
                            break;
                        case HitNoteResult.Perfect:
                            variation += scale * every;
                            PlayNoteScript.Result2P[HitNoteResult.Perfect].Add(note.Index);
                            break;
                        case HitNoteResult.Bad when GameSetting.SelectedInfo.Levels[GameSetting.Difficulty] <= 7:
                            variation -= scale * every * 8 / 5f;
                            PlayNoteScript.Result2P[HitNoteResult.Bad].Add(note.Index);
                            break;
                        case HitNoteResult.Bad:
                            variation -= 2 * scale * every;
                            PlayNoteScript.Result2P[HitNoteResult.Bad].Add(note.Index);
                            break;
                    }
                }
                break;
        }

        GaugePoints2P += variation;
        GaugePoints2P = Math.Max(0, GaugePoints2P);
        GaugePoints2P = Math.Min(10000, GaugePoints2P);

        Gauge[1].UpdateGauge();
    }

    public override void SetGoGo(bool go)
    {
        if (this.gogo != go)
        {
            this.gogo = go;
            Don_Avatar[0].Gogo(go);

            if (go)
            {
                GoGoFire[0].gameObject.layer = shown_layer;
                GoGoFire[0].enabled = true;
                GoGoFire[0].SetTrigger("Fire");
            }
            else
            {
                GoGoFire[0].gameObject.layer = hide_layer;
                GoGoFire[0].enabled = false;
            }

            GoGOTimeBG[0].SetActive(go);
        }
    }

    private void SetGoGo2P(bool go)
    {
        if (this.gogo_2p != go)
        {
            this.gogo_2p = go;
            Don_Avatar[1].Gogo(go);
            if (go)
            {
                GoGoFire[1].gameObject.layer = shown_layer;
                GoGoFire[1].enabled = true;
                GoGoFire[1].SetTrigger("Fire");
            }
            else
            {
                GoGoFire[1].gameObject.layer = hide_layer;
                GoGoFire[1].enabled = false;
            }
            GoGOTimeBG[1].SetActive(go);
        }
    }

    public override void CountResult(NoteSoundScript note)
    {
        if (note.Type < 5)
        {
            if (GameSetting.Mode != CommonClass.PlayMode.Practice && GameSetting.Config.Special != Special.None)
            {
                switch (GameSetting.Config.Special)
                {
                    case Special.AllPerfect:
                    case Special.DAllPerfect:
                        QuitGameByMiss();
                        return;
                    case Special.Training:
                    case Special.DTraining:
                        RestartByMiss();
                        return;
                }
            }

            SetNoteCombo(NoteSoundScript.ComboType.Combo_None);

            ComboCount = 0;
            CountPoints(HitNoteResult.Bad, note);
            if (Runner != null)
                Runner.OnPlay(false);

            if (bad_count < 6)
            {
                bad_count++;
                if (bad_count == 6)
                {
                    Don_Avatar[0].SetMiss(true);
                    DonImages[0].SetMiss(true);
                }
                else
                    Don_Avatar[0].Miss();
            }
        }
        Combo[0].SetCombo(ComboCount);
    }

    public override void CountResult2P(NoteSoundScript note)
    {
        if (note.Type < 5)
        {
            SetNoteCombo2P(NoteSoundScript.ComboType.Combo_None);

            ComboCount2P = 0;
            CountPoints2P(HitNoteResult.Bad, note);
            if (Runner != null) Runner.OnPlay(false, false);

            if (bad_count_2p < 6)
            {
                bad_count_2p++;
                if (bad_count_2p == 6)
                {
                    Don_Avatar[1].SetMiss(true);
                    DonImages[1].SetMiss(true);
                }
                else
                    Don_Avatar[1].Miss();
            }
        }
        Combo[1].SetCombo(ComboCount2P);
    }
}

