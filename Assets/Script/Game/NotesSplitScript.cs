using CommonClass;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.XR;

public class NotesSplitScript : MonoBehaviour
{
    public OptionScript Option;

    public static Dictionary<bool, List<NoteSoundScript>> NormalNotes = new Dictionary<bool, List<NoteSoundScript>> { { true, new List<NoteSoundScript>() }, { false, new List<NoteSoundScript>() } };
    public static Dictionary<bool, List<NoteSoundScript>> ExpertNotes = new Dictionary<bool, List<NoteSoundScript>> { { true, new List<NoteSoundScript>() }, { false, new List<NoteSoundScript>() } };
    public static Dictionary<bool, List<NoteSoundScript>> MasterNotes = new Dictionary<bool, List<NoteSoundScript>> { { true, new List<NoteSoundScript>() }, { false, new List<NoteSoundScript>() } };
    /*
    public static List<NoteSoundScript> NormalNotes2P = new List<NoteSoundScript>();
    public static List<NoteSoundScript> ExpertNotes2P = new List<NoteSoundScript>();
    public static List<NoteSoundScript> MasterNotes2P = new List<NoteSoundScript>();
    */
    protected List<NoteSoundScript> notes = new List<NoteSoundScript>();
    protected Dictionary<int, List<NoteSoundScript>> normal_notes = new Dictionary<int, List<NoteSoundScript>>();
    protected Dictionary<int, List<NoteSoundScript>> expert_notes = new Dictionary<int, List<NoteSoundScript>>();
    protected Dictionary<int, List<NoteSoundScript>> master_notes = new Dictionary<int, List<NoteSoundScript>>();
    protected Dictionary<int, List<NoteSoundScript>> normal_notes_show = new Dictionary<int, List<NoteSoundScript>>();
    protected Dictionary<int, List<NoteSoundScript>> expert_notes_show = new Dictionary<int, List<NoteSoundScript>>();
    protected Dictionary<int, List<NoteSoundScript>> master_notes_show = new Dictionary<int, List<NoteSoundScript>>();

    protected List<NoteSoundScript> notes2P = new List<NoteSoundScript>();
    protected Dictionary<int, List<NoteSoundScript>> normal_notes2P = new Dictionary<int, List<NoteSoundScript>>();
    protected Dictionary<int, List<NoteSoundScript>> expert_notes2P = new Dictionary<int, List<NoteSoundScript>>();
    protected Dictionary<int, List<NoteSoundScript>> master_notes2P = new Dictionary<int, List<NoteSoundScript>>();
    protected Dictionary<int, List<NoteSoundScript>> normal_notes_show2P = new Dictionary<int, List<NoteSoundScript>>();
    protected Dictionary<int, List<NoteSoundScript>> expert_notes_show2P = new Dictionary<int, List<NoteSoundScript>>();
    protected Dictionary<int, List<NoteSoundScript>> master_notes_show2P = new Dictionary<int, List<NoteSoundScript>>();

    protected Dictionary<int, List<ChapterLineScript>> lines_shown = new Dictionary<int, List<ChapterLineScript>>();
    protected WaitForEndOfFrame wait = new WaitForEndOfFrame();

    public static NotesSplitScript Instance;
    void Start()
    {
        Instance = this;
        OptionScript.SetBranch += ChangeBranchByOrder;
        ChapterLineScript.SetChapter += SetChapter;
    }

    void OnDestroy()
    {
        OptionScript.SetBranch -= ChangeBranchByOrder;
        ChapterLineScript.SetChapter -= SetChapter;
        //NoteSoundScript.NoteShow -= ManageNotes;

        NormalNotes = new Dictionary<bool, List<NoteSoundScript>> { { true, new List<NoteSoundScript>() }, { false, new List<NoteSoundScript>() } };
        ExpertNotes = new Dictionary<bool, List<NoteSoundScript>> { { true, new List<NoteSoundScript>() }, { false, new List<NoteSoundScript>() } };
        MasterNotes = new Dictionary<bool, List<NoteSoundScript>> { { true, new List<NoteSoundScript>() }, { false, new List<NoteSoundScript>() } };
        /*
        times.Dispose();
        times2.Dispose();
        all_notes.Dispose();
        types.Dispose();
        ends.Dispose();
        judges.Dispose();
        bpm.Dispose();
        scroll.Dispose();
        origin.Dispose();
        activate.Dispose();
        */
    }
    /*
    NativeArray<float> times;
    NativeArray<float> times2;
    TransformAccessArray all_notes;
    NativeArray<int> types;
    NativeArray<float> ends;
    NativeArray<float> judges;
    NativeArray<double> bpm;
    NativeArray<double> scroll;
    NativeArray<float> origin;
    NativeArray<bool> activate;
    JobHandle handle;
    bool job_start;

    private void ManageNotes(NoteSoundScript note, bool add)
    {
        int i = notes.IndexOf(note);
        activate[i] = add;
    }
    private void Update()
    {
        if (job_start)
        {
            NotesJob job = new NotesJob()
            {
                Times = times,
                Times2 = times2,
                JudgeTime = judges,
                EndTimes = ends,
                Types = types,
                Current = PlayNoteScript.CurrentTime,
                SystemTime = Time.time,
                Move2Chapter = false,
                Move2Time = 0,
                Playing = true,
                TimeScale = PlayNoteScript.TimeScale,
                OriginPostion = origin,
                Bpm = bpm,
                Scroll = scroll,
                LastStart = PlayNoteScript.LastStart,
                Activate = activate,
            };

            handle = job.Schedule(all_notes);
            handle.Complete();

            for (int i = 0; i < notes.Count; i++)
            {
                //if (activate[i]) Debug.Log(times[i]);
                notes[i].UpdateStatus(times[i], times2[i]);
            }
        }
    }
    */
    protected virtual void SetChapter(int chapter)
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

        //2P
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

        if (lines_shown.TryGetValue(chapter, out List<ChapterLineScript> chapters))
            StartCoroutine(Release(chapters));
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

    private void ChangeBranchByOrder(CourseBranch branch)
    {
        InputScript.CurrentBranch = branch;
        foreach (NoteSoundScript sound in notes)
        {
            if (sound.BranchRelated)
                sound.Show = sound.Branch == branch;
        }
    }

    public virtual void SetBranch(CourseBranch branch, int start_chapter)
    {
        StartCoroutine(DelayBranch(branch, start_chapter));
        //DoBranch(branch, start_chapter);
    }
    /*
    private void DoBranch(CourseBranch branch, int start_chapter)
    {
        for (int i = start_chapter - 1; i > 0; i--)
        {
            switch (InputScript.CurrentBranch)
            {
                case CourseBranch.BranchNormal:
                    foreach (NoteSoundScript script in normal_notes[i])
                    {
                        if (!script.BranchRelated || !script.gameObject.activeSelf) continue;
                        script.Show = false;
                    }
                    break;
                case CourseBranch.BranchExpert:
                    foreach (NoteSoundScript script in expert_notes[i])
                    {
                        if (script.Index == 488) Debug.Log(start_chapter);
                        if (!script.BranchRelated || !script.gameObject.activeSelf) continue;
                        script.Show = false;
                    }
                    break;
                case CourseBranch.BranchMaster:
                    foreach (NoteSoundScript script in master_notes[i])
                    {
                        if (!script.BranchRelated || !script.gameObject.activeSelf) continue;
                        script.Show = false;
                    }
                    break;
            }
        }

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
        }
    }
    */

    IEnumerator DelayBranch(CourseBranch branch, int start_chapter)
    {
        for (int i = start_chapter; i <= LoaderScript.Lines.Count; i++)
        {
            if (!normal_notes.ContainsKey(i)) Debug.Log(i);
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

    public virtual void SetBranch2P(CourseBranch branch, int start_chapter)
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

    public virtual void ResetTimeLine(bool animating)
    {
        StopAllCoroutines();
        NormalNotes = new Dictionary<bool, List<NoteSoundScript>> { { true, new List<NoteSoundScript>() }, { false, new List<NoteSoundScript>() } };
        ExpertNotes = new Dictionary<bool, List<NoteSoundScript>> { { true, new List<NoteSoundScript>() }, { false, new List<NoteSoundScript>() } };
        MasterNotes = new Dictionary<bool, List<NoteSoundScript>> { { true, new List<NoteSoundScript>() }, { false, new List<NoteSoundScript>() } };

        foreach (NoteSoundScript script in notes)
            script.gameObject.SetActive(true);

        CourseBranch current = CourseBranch.BranchNormal;
        if (Option != null && Option.Branch != CourseBranch.BranchNormal) current = Option.Branch;
        InputScript.CurrentBranch = current;
        
        foreach (NoteSoundScript script in notes)
        {
            bool add = false;
            //script.SetTrigger(NoteSoundScript.ComboType.Combo_None, 0);
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
        /*
        NormalNotes2P.Clear();
        ExpertNotes2P.Clear();
        MasterNotes2P.Clear();

        current = CourseBranch.Normal;
        foreach (NoteSoundScript script in notes2P)
        {
            bool add = false;
            //script.SetTrigger(NoteSoundScript.ComboType.Combo_None, 0);
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
                if (script.BranchRelated)
                {
                    script.Show = script.Branch == current;
                    switch (script.Branch)
                    {
                        case CourseBranch.Normal:
                            NormalNotes2P.Add(script);
                            break;
                        case CourseBranch.Expert:
                            ExpertNotes2P.Add(script);
                            break;
                        case CourseBranch.Master:
                            MasterNotes2P.Add(script);
                            break;
                    }
                }
                else
                {
                    NormalNotes2P.Add(script);
                    if (GameSetting.SelectedInfo.Branches.ContainsKey(GameSetting.Difficulty) && GameSetting.SelectedInfo.Branches[GameSetting.Difficulty])
                    {
                        ExpertNotes2P.Add(script);
                        MasterNotes2P.Add(script);
                    }
                }
            }
        }

        InputScript.Notes[CourseBranch.Normal] = NormalNotes2P;
        InputScript.Notes[CourseBranch.Expert] = ExpertNotes2P;
        InputScript.Notes[CourseBranch.Master] = MasterNotes2P;
        */
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

    public virtual void StartPlay()
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
                if (LoaderScript.Lines2P.Count > 0)
                    lines_shown[index].Add(LoaderScript.Lines2P[i]);

                //Debug.Log(string.Format("chapter {0} shown at {1}", script.Chapter, index));
            }
        }

        /*
        times = new NativeArray<float>(notes.Count + notes2P.Count, Allocator.Persistent);
        times2 = new NativeArray<float>(notes.Count + notes2P.Count, Allocator.Persistent);
        types = new NativeArray<int>(notes.Count + notes2P.Count, Allocator.Persistent);
        ends = new NativeArray<float>(notes.Count + notes2P.Count, Allocator.Persistent);
        judges = new NativeArray<float>(notes.Count + notes2P.Count, Allocator.Persistent);
        bpm = new NativeArray<double>(notes.Count + notes2P.Count, Allocator.Persistent);
        scroll = new NativeArray<double>(notes.Count + notes2P.Count, Allocator.Persistent);
        origin = new NativeArray<float>(notes.Count + notes2P.Count, Allocator.Persistent);
        activate = new NativeArray<bool>(notes.Count + notes2P.Count, Allocator.Persistent);
        all_notes = new TransformAccessArray(notes.Count + notes2P.Count);
        */
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

            //多线程代码
            /*
            types[z] = script.Type;
            judges[z] = times[z] = script.JudgeTime + script.Adjust;
            if (script is BallonScript balloon)
                ends[z] = times2[z] = balloon.EndTime + script.Adjust;
            else if (script is RapidScript rapid)
                ends[z] = times2[z] = rapid.EndTime + script.Adjust;
            else
                ends[z] = times2[z] = 0;
            all_notes.Add(script.transform);
            bpm[z] = script.Bpm;
            scroll[z] = script.Scroll;
            origin[z] = 0;
            activate[z] = true;
            */
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
            if (GameSetting.SelectedInfo.Branches.ContainsKey(GameSetting.Difficulty) && GameSetting.SelectedInfo.Branches[GameSetting.Difficulty])
            {
                expert_notes2P[LoaderScript.Lines2P[i].Chapter] = new List<NoteSoundScript>();
                master_notes2P[LoaderScript.Lines2P[i].Chapter] = new List<NoteSoundScript>();
                expert_notes_show2P[LoaderScript.Lines2P[i].Chapter] = new List<NoteSoundScript>();
                master_notes_show2P[LoaderScript.Lines2P[i].Chapter] = new List<NoteSoundScript>();
            }
        }

        foreach (NoteSoundScript script in notes2P)
        {
            int index = script.Chapter;
            if (!script.BranchRelated)
            {
                normal_notes2P[index].Add(script);
                if (GameSetting.SelectedInfo.Branches.ContainsKey(GameSetting.Difficulty) && GameSetting.SelectedInfo.Branches[GameSetting.Difficulty])
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
                if (GameSetting.SelectedInfo.Branches.ContainsKey(GameSetting.Difficulty) && GameSetting.SelectedInfo.Branches[GameSetting.Difficulty])
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
        /*
        for (int i = 0; i < LoaderScript.Lines.Count; i++)
        {
            foreach (NoteSoundScript sound in normal_notes[LoaderScript.Lines[i].Chapter])
            {
                if (!sound.BranchRelated)
                    Debug.Log(string.Format("index {0} branch {1}", sound.Index, sound.Branch));
            }
            foreach (NoteSoundScript sound in expert_notes[LoaderScript.Lines[i].Chapter])
            {
                if (!sound.BranchRelated)
                    Debug.Log(string.Format("index {0} branch {1}", sound.Index, sound.Branch));
            }
            foreach (NoteSoundScript sound in master_notes[LoaderScript.Lines[i].Chapter])
            {
                if (!sound.BranchRelated)
                    Debug.Log(string.Format("index {0} branch {1}", sound.Index, sound.Branch));
            }
        }
        */
        /*
        foreach (int key in normal_notes2P.Keys)
        {
            NormalNotes2P.AddRange(normal_notes2P[key]);
        }
        foreach (int key in expert_notes2P.Keys)
        {
            ExpertNotes2P.AddRange(expert_notes2P[key]);
        }
        foreach (int key in master_notes2P.Keys)
        {
            MasterNotes2P.AddRange(master_notes2P[key]);
        }
        */

        InputScript.Notes[CourseBranch.BranchNormal] = NormalNotes;
        InputScript.Notes[CourseBranch.BranchExpert] = ExpertNotes;
        InputScript.Notes[CourseBranch.BranchMaster] = MasterNotes;

        //NoteSoundScript.NoteShow += ManageNotes;
    }
    /*
    public virtual void Play()
    {
        job_start = true;
    }
    */
}

/*
[BurstCompile]
public struct NotesJob : IJobParallelForTransform
{
    public NativeArray<float> Times;
    public NativeArray<float> Times2;
    public NativeArray<bool> Activate;
    [ReadOnly] public float Current;
    [ReadOnly] public float SystemTime;
    [ReadOnly] public float LastStart;
    public NativeArray<float> JudgeTime;
    [ReadOnly] public bool Move2Chapter;
    [ReadOnly] public bool Playing;
    [ReadOnly] public float Move2Time;
    public NativeArray<int> Types;
    public NativeArray<float> EndTimes;
    public NativeArray<double> Bpm;
    public NativeArray<double> Scroll;
    public NativeArray<float> OriginPostion;
    [ReadOnly] public float TimeScale;

    public void Execute(int index, TransformAccess transform)
    {
        if (!Activate[index]) return;
        switch (Types[index])
        {
            case 7:
            case 9:
                {
                    if (Move2Chapter && Move2Time > SystemTime)
                    {
                        float target_time = JudgeTime[index] - Current * 1000;
                        float target_x;
                        if (target_time >= 0)
                            target_x = (float)(target_time * Bpm[index] * Scroll[index] * (1 + 1.5f)) / 628.7f * 1.5f / TimeScale / 100;
                        else
                        {
                            target_time = EndTimes[index] - Current * 1000;
                            if (target_time >= 0)
                                target_x = 0;
                            else
                                target_x = (float)(target_time * Bpm[index] * Scroll[index] * (1 + 1.5f)) / 628.7f * 1.5f / TimeScale / 100;
                        }

                        transform.localPosition = new Vector3(OriginPostion[index] + (target_x - OriginPostion[index]) * (1 - (Move2Time - SystemTime) / 0.3f), 0, transform.localPosition.z);
                    }

                    if (!Playing) return;
                    float time = JudgeTime[index] - ((SystemTime - LastStart) * 1000 * TimeScale);

                    Times[index] = time;
                    Times2[index] = EndTimes[index] - ((SystemTime - LastStart) * 1000 * TimeScale);
                    if (time >= 0)
                        transform.localPosition = new Vector3((float)(time * Bpm[index] * Scroll[index] * (1 + 1.5f)) / 628.7f * 1.5f / TimeScale / 100, 0, transform.localPosition.z);
                    else
                    {
                        if (Times2[index] >= 0)
                            transform.localPosition = new Vector3(0, 0, transform.localPosition.z);
                        else
                            transform.localPosition = new Vector3((float)(Times2[index] * Bpm[index] * Scroll[index] * (1 + 1.5f)) / 628.7f * 1.5f / TimeScale / 100, 0, transform.localPosition.z);
                    }
                }
                break;
            case 5:
            case 6:
                {
                    if (Move2Chapter && Move2Time > SystemTime)
                    {
                        float target_time = JudgeTime[index] - Current * 1000;
                        float target_x = (float)(target_time * Bpm[index] * Scroll[index] * (1 + 1.5f)) / 628.7f * 1.5f / TimeScale / 100;

                        transform.localPosition = new Vector3(OriginPostion[index] + (target_x - OriginPostion[index]) * (1 - (Move2Time - SystemTime) / 0.3f), 0, transform.localPosition.z);
                    }

                    if (!Playing) return;
                    float time = JudgeTime[index] - ((SystemTime - LastStart) * 1000 * TimeScale);
                    Times[index] = time;
                    EndTimes[index] = EndTimes[index] - ((SystemTime - LastStart) * 1000 * TimeScale);
                    transform.localPosition = new Vector3((float)(time * Bpm[index] * Scroll[index] * (1 + 1.5f)) / 628.7f * 1.5f / TimeScale / 100, 0, transform.localPosition.z);
                }
                break;
            default:
                {
                    if (Move2Chapter && Move2Time > SystemTime)
                    {
                        float target_time = JudgeTime[index] - Current * 1000;
                        float target_x = (float)(target_time * Bpm[index] * Scroll[index] * (1 + 1.5f)) / 628.7f * 1.5f / TimeScale / 100;

                        transform.localPosition = new Vector3(OriginPostion[index] + (target_x - OriginPostion[index]) * (1 - (Move2Time - SystemTime) / 0.3f), 0, transform.localPosition.z);
                    }

                    if (!Playing) return;
                    float time = JudgeTime[index] - ((SystemTime - LastStart) * 1000 * TimeScale);
                    Times[index] = time;
                    transform.localPosition = new Vector3((float)(time * Bpm[index] * Scroll[index] * (1 + 1.5f)) / 628.7f * 1.5f / TimeScale / 100, 0, transform.localPosition.z);
                }
                break;
        }
    }
}
*/