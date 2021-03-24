using CommonClass;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class DualNotesSplitScript : NotesSplitScript
{
    public static Dictionary<bool, List<NoteSoundScript>> NormalNotes2P = new Dictionary<bool, List<NoteSoundScript>> { { true, new List<NoteSoundScript>() }, { false, new List<NoteSoundScript>() } };
    public static Dictionary<bool, List<NoteSoundScript>> ExpertNotes2P = new Dictionary<bool, List<NoteSoundScript>> { { true, new List<NoteSoundScript>() }, { false, new List<NoteSoundScript>() } };
    public static Dictionary<bool, List<NoteSoundScript>> MasterNotes2P = new Dictionary<bool, List<NoteSoundScript>> { { true, new List<NoteSoundScript>() }, { false, new List<NoteSoundScript>() } };

    protected Dictionary<int, List<ChapterLineScript>> lines_shown_2p = new Dictionary<int, List<ChapterLineScript>>();
    void Start()
    {
        Instance = this;
        ChapterLineScript.SetChapter += SetChapter;
        ChapterLineScript.SetChapter2P += SetChapter2P;
    }

    void OnDestroy()
    {
        ChapterLineScript.SetChapter -= SetChapter;
        ChapterLineScript.SetChapter2P -= SetChapter2P;

        NormalNotes = new Dictionary<bool, List<NoteSoundScript>> { { true, new List<NoteSoundScript>() }, { false, new List<NoteSoundScript>() } };
        ExpertNotes = new Dictionary<bool, List<NoteSoundScript>> { { true, new List<NoteSoundScript>() }, { false, new List<NoteSoundScript>() } };
        MasterNotes = new Dictionary<bool, List<NoteSoundScript>> { { true, new List<NoteSoundScript>() }, { false, new List<NoteSoundScript>() } };

        NormalNotes2P = new Dictionary<bool, List<NoteSoundScript>> { { true, new List<NoteSoundScript>() }, { false, new List<NoteSoundScript>() } };
        ExpertNotes2P = new Dictionary<bool, List<NoteSoundScript>> { { true, new List<NoteSoundScript>() }, { false, new List<NoteSoundScript>() } };
        MasterNotes2P = new Dictionary<bool, List<NoteSoundScript>> { { true, new List<NoteSoundScript>() }, { false, new List<NoteSoundScript>() } };
    }
    protected override void SetChapter(int chapter)
    {
        if (normal_notes_show.ContainsKey(chapter + 2))
        {
            StartCoroutine(Release(normal_notes_show, chapter + 2));
        }
        if (expert_notes_show.ContainsKey(chapter + 2))
        {
            StartCoroutine(Release(expert_notes_show, chapter + 2));
        }
        if (master_notes_show.ContainsKey(chapter + 2))
        {
            StartCoroutine(Release(master_notes_show, chapter + 2));
        }
        if (lines_shown.TryGetValue(chapter, out List<ChapterLineScript> chapters))
            StartCoroutine(Release(chapters));
    }

    private void SetChapter2P(int chapter)
    {
        if (normal_notes_show2P.ContainsKey(chapter + 2))
        {
            StartCoroutine(Release(normal_notes_show2P, chapter + 2));
        }
        if (expert_notes_show2P.ContainsKey(chapter + 2))
        {
            StartCoroutine(Release(expert_notes_show2P, chapter + 2));
        }
        if (master_notes_show2P.ContainsKey(chapter + 2))
        {
            StartCoroutine(Release(master_notes_show2P, chapter + 2));
        }

        if (lines_shown_2p.TryGetValue(chapter, out List<ChapterLineScript> chapters_2p))
            StartCoroutine(Release(chapters_2p));
    }
    IEnumerator Release(List<ChapterLineScript> chapters)
    {
        yield return wait;
        foreach (ChapterLineScript note in chapters)
        {
            yield return wait;
            note.gameObject.SetActive(true);
        }
    }

    IEnumerator Release(Dictionary<int, List<NoteSoundScript>> container, int chapter)
    {
        yield return wait;
        yield return wait;
        foreach (NoteSoundScript note in container[chapter])
        {
            yield return wait;
            note.gameObject.SetActive(true);
        }
    }

    public override void SetBranch(CourseBranch branch, int start_chapter)
    {
        StartCoroutine(DelayBranch(branch, start_chapter));
    }
 
    IEnumerator DelayBranch(CourseBranch branch, int start_chapter)
    {
        for (int i = start_chapter; i <= LoaderScript.Lines.Count; i++)
        {
            foreach (NoteSoundScript script in normal_notes[i])
            {
                if (!script.BranchRelated) continue;
                script.Show = branch == CourseBranch.BranchNormal;
            }

            foreach (NoteSoundScript script in expert_notes[i])
            {
                if (!script.BranchRelated) continue;
                script.Show = branch == CourseBranch.BranchExpert;
            }

            foreach (NoteSoundScript script in master_notes[i])
            {
                if (!script.BranchRelated) continue;
                script.Show = branch == CourseBranch.BranchMaster;
            }

            yield return wait;
        }
    }

    public override void SetBranch2P(CourseBranch branch, int start_chapter)
    {
        StartCoroutine(DelayBranch2P(branch, start_chapter));
    }

    IEnumerator DelayBranch2P(CourseBranch branch, int start_chapter)
    {
        for (int i = start_chapter; i <= LoaderScript.Lines2P.Count; i++)
        {
            foreach (NoteSoundScript script in normal_notes2P[i])
            {
                if (!script.BranchRelated) continue;
                script.Show = branch == CourseBranch.BranchNormal;
            }

            foreach (NoteSoundScript script in expert_notes2P[i])
            {
                if (!script.BranchRelated) continue;
                script.Show = branch == CourseBranch.BranchExpert;
            }

            foreach (NoteSoundScript script in master_notes2P[i])
            {
                if (!script.BranchRelated) continue;
                script.Show = branch == CourseBranch.BranchMaster;
            }

            yield return wait;
        }
    }

    public override void ResetTimeLine(bool animating)
    {
        StopAllCoroutines();
        NormalNotes = new Dictionary<bool, List<NoteSoundScript>> { { true, new List<NoteSoundScript>() }, { false, new List<NoteSoundScript>() } };
        ExpertNotes = new Dictionary<bool, List<NoteSoundScript>> { { true, new List<NoteSoundScript>() }, { false, new List<NoteSoundScript>() } };
        MasterNotes = new Dictionary<bool, List<NoteSoundScript>> { { true, new List<NoteSoundScript>() }, { false, new List<NoteSoundScript>() } };

        foreach (NoteSoundScript script in notes)
            script.gameObject.SetActive(true);

        CourseBranch current = InputScript.CurrentBranch = InputScript.CurrentBranch2P = CourseBranch.BranchNormal;

        foreach (NoteSoundScript script in notes)
        {
            bool add = false;
            if (script.Chapter < PlayNoteScript.Chapter + 5)
            {
                if (script is BallonScript ball)
                {
                    if (ball.EndTime / 1000 >= PlayNoteScript.CurrentTime)
                        add = true;
                }
                else if (script is RapidScript rapid)
                {
                    if (rapid.EndTime / 1000 >= PlayNoteScript.CurrentTime)
                        add = true;
                }
                else
                {
                    float time = (script.JudgeTime + GameSetting.Config.JudgeRange[HitNoteResult.Good]) / 1000;
                    if (time >= PlayNoteScript.CurrentTime) add = true;
                }
            }
            else
                add = true;

            if (add)
            {
                bool don = script.Type == 1 || script.Type == 3 || script.Type > 4;
                bool ka = script.Type == 2 || (script.Type >= 4 && script.Type <= 6);
                if (script.BranchRelated)
                {
                    script.Show = script.Branch == current;
                    switch (script.Branch)
                    {
                        case CourseBranch.BranchNormal:
                            if (don)
                                NormalNotes[true].Add(script);
                            if (ka)
                                NormalNotes[false].Add(script);
                            break;
                        case CourseBranch.BranchExpert:
                            if (don)
                                ExpertNotes[true].Add(script);
                            if (ka)
                                ExpertNotes[false].Add(script);
                            break;
                        case CourseBranch.BranchMaster:
                            if (don)
                                MasterNotes[true].Add(script);
                            if (ka)
                                MasterNotes[false].Add(script);
                            break;
                    }
                }
                else
                {
                    if (don)
                        NormalNotes[true].Add(script);
                    if (ka)
                        NormalNotes[false].Add(script);
                    if (GameSetting.SelectedInfo.Branches.ContainsKey(GameSetting.Difficulty) && GameSetting.SelectedInfo.Branches[GameSetting.Difficulty])
                    {
                        if (don)
                        {
                            ExpertNotes[true].Add(script);
                            MasterNotes[true].Add(script);
                        }
                        if (ka)
                        {
                            ExpertNotes[false].Add(script);
                            MasterNotes[false].Add(script);
                        }
                    }
                }
            }
        }

        InputScript.Notes[CourseBranch.BranchNormal] = NormalNotes;
        InputScript.Notes[CourseBranch.BranchExpert] = ExpertNotes;
        InputScript.Notes[CourseBranch.BranchMaster] = MasterNotes;

        foreach (NoteSoundScript script in notes)
        {
            if (animating)
                script.Move2Chapter(0.3f);
            else
                script.Reposition();
        }

        //2P
        NormalNotes2P = new Dictionary<bool, List<NoteSoundScript>> { { true, new List<NoteSoundScript>() }, { false, new List<NoteSoundScript>() } };
        ExpertNotes2P = new Dictionary<bool, List<NoteSoundScript>> { { true, new List<NoteSoundScript>() }, { false, new List<NoteSoundScript>() } };
        MasterNotes2P = new Dictionary<bool, List<NoteSoundScript>> { { true, new List<NoteSoundScript>() }, { false, new List<NoteSoundScript>() } };

        foreach (NoteSoundScript script in notes2P)
        {
            bool add = false;
            if (script.Chapter < PlayNoteScript.Chapter + 5)
            {
                if (script is BallonScript ball)
                {
                    if (ball.EndTime / 1000 >= PlayNoteScript.CurrentTime)
                        add = true;
                }
                else if (script is RapidScript rapid)
                {
                    if (rapid.EndTime / 1000 >= PlayNoteScript.CurrentTime)
                        add = true;
                }
                else
                {
                    float time = (script.JudgeTime + GameSetting.Config.JudgeRange[HitNoteResult.Good]) / 1000;
                    if (time >= PlayNoteScript.CurrentTime) add = true;
                }
            }
            else
                add = true;

            if (add)
            {
                bool don = script.Type == 1 || script.Type == 3 || script.Type > 4;
                bool ka = script.Type == 2 || (script.Type >= 4 && script.Type <= 6);
                if (script.BranchRelated)
                {
                    script.Show = script.Branch == current;
                    switch (script.Branch)
                    {
                        case CourseBranch.BranchNormal:
                            if (don)
                                NormalNotes2P[true].Add(script);
                            if (ka)
                                NormalNotes2P[false].Add(script);
                            break;
                        case CourseBranch.BranchExpert:
                            if (don)
                                ExpertNotes2P[true].Add(script);
                            if (ka)
                                ExpertNotes2P[false].Add(script);
                            break;
                        case CourseBranch.BranchMaster:
                            if (don)
                                MasterNotes2P[true].Add(script);
                            if (ka)
                                MasterNotes2P[false].Add(script);
                            break;
                    }
                }
                else
                {
                    if (don)
                        NormalNotes2P[true].Add(script);
                    if (ka)
                        NormalNotes2P[false].Add(script);
                    if (GameSetting.SelectedInfo.Branches.ContainsKey(GameSetting.Difficulty2P) && GameSetting.SelectedInfo.Branches[GameSetting.Difficulty2P])
                    {
                        if (don)
                        {
                            ExpertNotes2P[true].Add(script);
                            MasterNotes2P[true].Add(script);
                        }
                        if (ka)
                        {
                            ExpertNotes2P[false].Add(script);
                            MasterNotes2P[false].Add(script);
                        }
                    }
                }
            }
        }

        DualInputScript.Notes2P[CourseBranch.BranchNormal] = NormalNotes2P;
        DualInputScript.Notes2P[CourseBranch.BranchExpert] = ExpertNotes2P;
        DualInputScript.Notes2P[CourseBranch.BranchMaster] = MasterNotes2P;

        foreach (NoteSoundScript script in notes2P)
            script.gameObject.SetActive(true);

        foreach (NoteSoundScript script in notes2P)
        {
            if (animating)
                script.Move2Chapter(0.3f);
            else
                script.Reposition();
        }
    }

    public override void StartPlay()
    {
        notes = new List<NoteSoundScript>(LoaderScript.Notes.Values);
        notes.Sort((x, y) => { return x.JudgeTime < y.JudgeTime ? -1 : 1; });
        for (int i = 0; i < LoaderScript.Lines.Count; i++)
        {
            normal_notes[LoaderScript.Lines[i].Chapter] = new List<NoteSoundScript>();
            normal_notes_show[LoaderScript.Lines[i].Chapter] = new List<NoteSoundScript>();
            if (GameSetting.SelectedInfo.Branches.ContainsKey(GameSetting.Difficulty) && GameSetting.SelectedInfo.Branches[GameSetting.Difficulty])
            {
                expert_notes[LoaderScript.Lines[i].Chapter] = new List<NoteSoundScript>();
                master_notes[LoaderScript.Lines[i].Chapter] = new List<NoteSoundScript>();
                expert_notes_show[LoaderScript.Lines[i].Chapter] = new List<NoteSoundScript>();
                master_notes_show[LoaderScript.Lines[i].Chapter] = new List<NoteSoundScript>();
            }

            if (i > 0)
            {
                ChapterLineScript script = LoaderScript.Lines[i];
                int index = 1;

                if (i > 1)
                {
                    if (script.Scroll >= 0)
                    {
                        //进入临界点经过的时间
                        for (int y = script.Chapter - 1; y >= 1; y--)
                        {
                            index = y;
                            float x = (float)((script.JudgeTime - LoaderScript.Lines[y - 1].JudgeTime) * script.Bpm * script.Scroll * (1 + 1.5f)) / 628.7f * 1.5f;
                            if (x > 1400)
                                break;
                        }
                    }
                    else
                    {
                        //进入临界点经过的时间
                        for (int y = 1; y <= script.Chapter - 1; y++)
                        {
                            index = y;
                            float x = (float)((script.JudgeTime - LoaderScript.Lines[y - 1].JudgeTime) * script.Bpm * script.Scroll * (1 + 1.5f)) / 628.7f * 1.5f;
                            if (x < -400)
                                break;
                        }
                    }
                }

                if (lines_shown.TryGetValue(index, out List<ChapterLineScript> lines))
                    lines_shown[index].Add(script);
                else
                    lines_shown[index] = new List<ChapterLineScript> { script };

                //Debug.Log(string.Format("chapter {0} shown at {1}", script.Chapter, index));
            }
        }

        for (int z = 0; z < notes.Count; z++)
        {
            NoteSoundScript script = notes[z];
            int index = script.Chapter;
            if (!script.BranchRelated)
            {
                normal_notes[index].Add(script);
                if (GameSetting.SelectedInfo.Branches.ContainsKey(GameSetting.Difficulty) && GameSetting.SelectedInfo.Branches[GameSetting.Difficulty])
                {
                    expert_notes[index].Add(script);
                    master_notes[index].Add(script);
                }
            }
            else
            {
                switch (script.Branch)
                {
                    case CourseBranch.BranchNormal:
                        script.Show = true;
                        normal_notes[index].Add(script);
                        break;
                    case CourseBranch.BranchExpert:
                        script.Show = false;
                        expert_notes[index].Add(script);
                        break;
                    case CourseBranch.BranchMaster:
                        script.Show = false;
                        master_notes[index].Add(script);
                        break;
                }
            }

            if (index > 2)
            {
                if (script.Scroll >= 0)
                {
                    //移动1400距离需要的时间
                    float time = 1400 / 1.5f * 628.7f / ((1 + 1.5f) * (float)script.Scroll * (float)script.Bpm);
                    if (time >= script.JudgeTime)
                    {
                        index = 0;
                    }
                    else
                    {
                        //进入临界点经过的时间
                        float last = script.JudgeTime - time;
                        for (int i = LoaderScript.Lines.Count - 1; i >= 0; i--)
                        {
                            index = LoaderScript.Lines[i].Chapter;
                            if (last > LoaderScript.Lines[i].JudgeTime)
                                break;
                        }
                    }
                }
                else
                {
                    //移动700距离需要的时间
                    float time = 700 / 1.5f * 628.7f / ((1 + 1.5f) * Math.Abs((float)script.Scroll) * (float)script.Bpm);
                    if (time >= script.JudgeTime)
                    {
                        index = 0;
                    }
                    else
                    {
                        //进入临界点经过的时间
                        float last = script.JudgeTime - time;
                        for (int i = LoaderScript.Lines.Count - 1; i >= 0; i--)
                        {
                            index = LoaderScript.Lines[i].Chapter;
                            if (last > LoaderScript.Lines[i].JudgeTime)
                                break;
                        }
                    }
                }
            }

            script.ShowChapter = index;

            if (!script.BranchRelated)
            {
                normal_notes_show[index].Add(script);
                if (GameSetting.SelectedInfo.Branches.ContainsKey(GameSetting.Difficulty) && GameSetting.SelectedInfo.Branches[GameSetting.Difficulty])
                {
                    expert_notes_show[index].Add(script);
                    master_notes_show[index].Add(script);
                }
            }
            else
            {
                switch (script.Branch)
                {
                    case CourseBranch.BranchNormal:
                        script.Show = true;
                        normal_notes_show[index].Add(script);
                        break;
                    case CourseBranch.BranchExpert:
                        script.Show = false;
                        expert_notes_show[index].Add(script);
                        break;
                    case CourseBranch.BranchMaster:
                        script.Show = false;
                        master_notes_show[index].Add(script);
                        break;
                }
            }

            if (index > 2) script.gameObject.SetActive(false);
        }

        foreach (int key in normal_notes.Keys)
        {
            List<NoteSoundScript> notes = normal_notes[key];
            foreach (NoteSoundScript script in notes)
            {
                bool don = script.Type == 1 || script.Type == 3 || script.Type > 4;
                bool ka = script.Type == 2 || (script.Type >= 4 && script.Type <= 6);
                if (don)
                    NormalNotes[true].Add(script);
                if (ka)
                    NormalNotes[false].Add(script);
            }

        }
        foreach (int key in expert_notes.Keys)
        {
            List<NoteSoundScript> notes = expert_notes[key];
            foreach (NoteSoundScript script in notes)
            {
                bool don = script.Type == 1 || script.Type == 3 || script.Type > 4;
                bool ka = script.Type == 2 || (script.Type >= 4 && script.Type <= 6);
                if (don)
                    ExpertNotes[true].Add(script);
                if (ka)
                    ExpertNotes[false].Add(script);
            }
        }
        foreach (int key in master_notes.Keys)
        {
            List<NoteSoundScript> notes = master_notes[key];
            foreach (NoteSoundScript script in notes)
            {
                bool don = script.Type == 1 || script.Type == 3 || script.Type > 4;
                bool ka = script.Type == 2 || (script.Type >= 4 && script.Type <= 6);
                if (don)
                    MasterNotes[true].Add(script);
                if (ka)
                    MasterNotes[false].Add(script);
            }
        }

        //2P
        notes2P = new List<NoteSoundScript>(LoaderScript.Notes2P.Values);
        notes2P.Sort((x, y) => { return x.JudgeTime < y.JudgeTime ? -1 : 1; });
        for (int i = 0; i < LoaderScript.Lines2P.Count; i++)
        {
            normal_notes2P[LoaderScript.Lines2P[i].Chapter] = new List<NoteSoundScript>();
            normal_notes_show2P[LoaderScript.Lines2P[i].Chapter] = new List<NoteSoundScript>();
            if (GameSetting.SelectedInfo.Branches.ContainsKey(GameSetting.Difficulty2P) && GameSetting.SelectedInfo.Branches[GameSetting.Difficulty2P])
            {
                expert_notes2P[LoaderScript.Lines2P[i].Chapter] = new List<NoteSoundScript>();
                master_notes2P[LoaderScript.Lines2P[i].Chapter] = new List<NoteSoundScript>();
                expert_notes_show2P[LoaderScript.Lines2P[i].Chapter] = new List<NoteSoundScript>();
                master_notes_show2P[LoaderScript.Lines2P[i].Chapter] = new List<NoteSoundScript>();
            }

            if (i > 0)
            {
                ChapterLineScript script = LoaderScript.Lines2P[i];
                int index = 1;

                if (i > 1)
                {
                    if (script.Scroll >= 0)
                    {
                        //进入临界点经过的时间
                        for (int y = script.Chapter - 1; y >= 1; y--)
                        {
                            index = y;
                            float x = (float)((script.JudgeTime - LoaderScript.Lines2P[y - 1].JudgeTime) * script.Bpm * script.Scroll * (1 + 1.5f)) / 628.7f * 1.5f;
                            if (x > 1400)
                                break;
                        }
                    }
                    else
                    {
                        //进入临界点经过的时间
                        for (int y = 1; y <= script.Chapter - 1; y++)
                        {
                            index = y;
                            float x = (float)((script.JudgeTime - LoaderScript.Lines2P[y - 1].JudgeTime) * script.Bpm * script.Scroll * (1 + 1.5f)) / 628.7f * 1.5f;
                            if (x < -400)
                                break;
                        }
                    }
                }

                if (lines_shown_2p.TryGetValue(index, out List<ChapterLineScript> lines))
                    lines_shown_2p[index].Add(script);
                else
                    lines_shown_2p[index] = new List<ChapterLineScript> { script };
            }
        }

        foreach (NoteSoundScript script in notes2P)
        {
            int index = script.Chapter;
            if (!script.BranchRelated)
            {
                normal_notes2P[index].Add(script);
                if (GameSetting.SelectedInfo.Branches.ContainsKey(GameSetting.Difficulty2P) && GameSetting.SelectedInfo.Branches[GameSetting.Difficulty2P])
                {
                    expert_notes2P[index].Add(script);
                    master_notes2P[index].Add(script);
                }
            }
            else
            {
                switch (script.Branch)
                {
                    case CourseBranch.BranchNormal:
                        script.Show = true;
                        normal_notes2P[index].Add(script);
                        break;
                    case CourseBranch.BranchExpert:
                        script.Show = false;
                        expert_notes2P[index].Add(script);
                        break;
                    case CourseBranch.BranchMaster:
                        script.Show = false;
                        master_notes2P[index].Add(script);
                        break;
                }
            }

            if (index > 2)
            {
                if (script.Scroll >= 0)
                {
                    //移动1400距离需要的时间
                    float time = 1400 / 1.5f * 628.7f / ((1 + 1.5f) * (float)script.Scroll * (float)script.Bpm);
                    if (time >= script.JudgeTime)
                    {
                        index = 0;
                    }
                    else
                    {
                        //进入临界点经过的时间
                        float last = script.JudgeTime - time;
                        for (int i = LoaderScript.Lines2P.Count - 1; i >= 0; i--)
                        {
                            index = LoaderScript.Lines2P[i].Chapter;
                            if (last > LoaderScript.Lines2P[i].JudgeTime)
                                break;
                        }
                    }
                }
                else
                {
                    //移动700距离需要的时间
                    float time = 700 / 1.5f * 628.7f / ((1 + 1.5f) * Math.Abs((float)script.Scroll) * (float)script.Bpm);
                    if (time >= script.JudgeTime)
                    {
                        index = 0;
                    }
                    else
                    {
                        //进入临界点经过的时间
                        float last = script.JudgeTime - time;
                        for (int i = LoaderScript.Lines2P.Count - 1; i >= 0; i--)
                        {
                            index = LoaderScript.Lines2P[i].Chapter;
                            if (last > LoaderScript.Lines2P[i].JudgeTime)
                                break;
                        }
                    }
                }
            }

            script.ShowChapter = index;

            if (!script.BranchRelated)
            {
                normal_notes_show2P[index].Add(script);
                if (GameSetting.SelectedInfo.Branches.ContainsKey(GameSetting.Difficulty2P) && GameSetting.SelectedInfo.Branches[GameSetting.Difficulty2P])
                {
                    expert_notes_show2P[index].Add(script);
                    master_notes_show2P[index].Add(script);
                }
            }
            else
            {
                switch (script.Branch)
                {
                    case CourseBranch.BranchNormal:
                        script.Show = true;
                        normal_notes_show2P[index].Add(script);
                        break;
                    case CourseBranch.BranchExpert:
                        script.Show = false;
                        expert_notes_show2P[index].Add(script);
                        break;
                    case CourseBranch.BranchMaster:
                        script.Show = false;
                        master_notes_show2P[index].Add(script);
                        break;
                }
            }

            if (index > 2) script.gameObject.SetActive(false);
        }

        foreach (int key in normal_notes2P.Keys)
        {
            List<NoteSoundScript> notes = normal_notes2P[key];
            foreach (NoteSoundScript script in notes)
            {
                bool don = script.Type == 1 || script.Type == 3 || script.Type > 4;
                bool ka = script.Type == 2 || (script.Type >= 4 && script.Type <= 6);
                if (don)
                    NormalNotes2P[true].Add(script);
                if (ka)
                    NormalNotes2P[false].Add(script);
            }

        }
        foreach (int key in expert_notes2P.Keys)
        {
            List<NoteSoundScript> notes = expert_notes2P[key];
            foreach (NoteSoundScript script in notes)
            {
                bool don = script.Type == 1 || script.Type == 3 || script.Type > 4;
                bool ka = script.Type == 2 || (script.Type >= 4 && script.Type <= 6);
                if (don)
                    ExpertNotes2P[true].Add(script);
                if (ka)
                    ExpertNotes2P[false].Add(script);
            }
        }
        foreach (int key in master_notes2P.Keys)
        {
            List<NoteSoundScript> notes = master_notes2P[key];
            foreach (NoteSoundScript script in notes)
            {
                bool don = script.Type == 1 || script.Type == 3 || script.Type > 4;
                bool ka = script.Type == 2 || (script.Type >= 4 && script.Type <= 6);
                if (don)
                    MasterNotes2P[true].Add(script);
                if (ka)
                    MasterNotes2P[false].Add(script);
            }
        }

        InputScript.Notes[CourseBranch.BranchNormal] = NormalNotes;
        InputScript.Notes[CourseBranch.BranchExpert] = ExpertNotes;
        InputScript.Notes[CourseBranch.BranchMaster] = MasterNotes;

        DualInputScript.Notes2P[CourseBranch.BranchNormal] = NormalNotes2P;
        DualInputScript.Notes2P[CourseBranch.BranchExpert] = ExpertNotes2P;
        DualInputScript.Notes2P[CourseBranch.BranchMaster] = MasterNotes2P;
    }
}