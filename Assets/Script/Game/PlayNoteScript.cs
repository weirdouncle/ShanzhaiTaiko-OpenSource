using CommonClass;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public delegate void SeekDelegate(int position);
public delegate void GamePlayMusicDelegate(float pitch);
public class PlayNoteScript : MonoBehaviour
{
    public static Dictionary<HitNoteResult, List<int>> Result;
    public static Dictionary<HitNoteResult, List<int>> Result2P;
    public static int ShinScorePerNote;
    public static int ShinScoreTotal;
    public static int Normal_notes_count;
    public static int ShinScorePerNote2P;
    public static int ShinScoreTotal2P;
    public static int Normal_notes_count_2p;

    private int Big_notes_count;
    private int Balloon_hit_count;
    private int Balloon_notes_count;

    private int Big_notes_count_2p;
    private int Balloon_hit_count_2p;
    private int Balloon_notes_count_2p;

    public static event GamePlayMusicDelegate PlayMusic;

    //public static event PuaseDelegate Pause;
    //public delegate void PuaseDelegate();

    public static float LastStart { protected set; get; }
    public static float CurrentTime { protected set; get; }
    public static float TimeScale
    {
        get => s_timeScale;
        set {
            s_timeScale = value;
            SetTimeScale();
        }
    }
    public static int Chapter = 0;

    public AudioSource Audio;
    public CriAtomSource AudioSource;
    public OptionScript Option;
    public NotesSplitScript NoteSplit;
    public GloableAniControllerScript AnimationController;
    public ScoreScript[] Score;
    public LyricsScript Lyric;

    protected static float s_timeScale = 1;
    protected InputSettings.UpdateMode last_mode;
    protected bool cri_audio;
    protected float cri_length;

    private string acb_cuename;

    void Awake()
    {
#if !UNITY_ANDROID
        last_mode = InputSystem.settings.updateMode;
#endif
    }

    void Start()
    {
        Chapter = 0;
        TimeScale = 1;

        LoadingScreenScript.Play += Play;
        InputScript.Hold += Hold;
        OptionScript.Switch += Switch;
        ChapterLineScript.SetChapter += SetChapter;
        LoaderScript.Play += PrepareStart;

#if !UNITY_EDITOR
        Cursor.visible = false;
#endif
        if (GameSetting.Selected == null && SettingLoader.HasAcbSong(GameSetting.SelectedInfo.Title))
        {
            cri_audio = true;
            StartCoroutine(LoadAtomAcb(GameSetting.SelectedInfo.Title));
        }
        else
        {
            Audio.volume = Math.Min(GameSetting.Config.MusicVolume, 1);
        }
    }

    void OnDestroy()
    {
        Cursor.visible = true;
#if !UNITY_ANDROID
        InputSystem.settings.updateMode = last_mode;
#endif
        LoadingScreenScript.Play -= Play;
        InputScript.Hold -= Hold;
        OptionScript.Switch -= Switch;
        ChapterLineScript.SetChapter -= SetChapter;
        LoaderScript.Play -= PrepareStart;

        CriAtom.RemoveCueSheet(acb_cuename);

        if (GameSetting.Selected != null)
        {
            Destroy(GameSetting.Selected);
            GameSetting.Selected = null;
        }
    }

    IEnumerator LoadAtomAcb(string title)
    {
        if (!string.IsNullOrEmpty(title))
        {
            KeyValuePair<string, AtomSongInfo> path = SettingLoader.GetSongPath(title);

            var bundleLoadRequest = AssetBundle.LoadFromFileAsync(path.Key);
            yield return bundleLoadRequest;

            var myLoadedAssetBundle = bundleLoadRequest.assetBundle;
            if (myLoadedAssetBundle == null)
            {
                Debug.Log("Failed to load AssetBundle " + path);
            }
            else
            {
                GameObject game = myLoadedAssetBundle.LoadAsset<GameObject>(path.Value.Ab);
                if (game != null)
                {
                    SongPackScript script = game.GetComponent<SongPackScript>();
                    if (script != null)
                    {
                        List<TextAsset> songs = new List<TextAsset>(script.Songs);
                        TextAsset tex = songs.Find(t => t.name == path.Value.Path);
                        if (tex != null)
                        {
                            CriAtomCueSheet sheet = CriAtom.AddCueSheet(title, tex.bytes, string.Empty);
                            sheet.acb.GetCueInfoByIndex(0, out CriAtomEx.CueInfo song_info);
                            sheet.acb.GetWaveFormInfo(song_info.name, out CriAtomEx.WaveformInfo wave_info);
                            cri_length = wave_info.numSamples / (float)wave_info.samplingRate;
                            AudioSource.cueSheet = acb_cuename = sheet.name;
                            AudioSource.cueName  = song_info.name;
                            AudioSource.volume = Math.Min(GameSetting.Config.MusicVolume, 1);
                        }
                        else
                            Debug.Log("Failed to find dasta in " + path.Key);
                    }
                    else
                        Debug.Log("Failed to find script in " + path.Key);
                }
                else
                    Debug.Log("Failed to find object " + path.Value.Ab);

                myLoadedAssetBundle.Unload(true);
            }
        }
    }

    private void Switch(bool forword)
    {
        if (forword)
        {
            if (Chapter < LoaderScript.Lines.Count)
            {
                Chapter++;
            }
            CurrentTime = (LoaderScript.Lines[Chapter - 1].JudgeTime + GameSetting.Config.NoteAdjust) / 1000;
        }
        else
        {
            if (Chapter > 1)
            {
                Chapter--;
                CurrentTime = (LoaderScript.Lines[Chapter - 1].JudgeTime + GameSetting.Config.NoteAdjust) / 1000;
            }
            else
            {
                Chapter = 0;
                CurrentTime = 0;
            }
        }
        
        List<HitNoteResult> keys = new List<HitNoteResult>(Result.Keys);
        foreach (HitNoteResult hit in keys)
        {
            Result[hit].Clear();
            Result2P[hit].Clear();
        }

        foreach (ChapterLineScript script in LoaderScript.Lines)
            script.Reposition();
        foreach (ChapterLineScript script in LoaderScript.Lines2P)
            script.Reposition();

        NoteSplit.ResetTimeLine(true);
        Lyric.ResetTimeLine();
        AnimationController.ResetTimeLine(true);
    }

    public virtual void RestartPrepare(int chapter)
    {
        List<HitNoteResult> keys = new List<HitNoteResult>(Result.Keys);
        foreach (HitNoteResult hit in keys)
        {
            Result[hit].Clear();
            Result2P[hit].Clear();
        }

        Chapter = chapter;
        if (chapter == 0)
            CurrentTime = 0;
        else
            CurrentTime = (LoaderScript.Lines[Chapter - 1].JudgeTime + GameSetting.Config.NoteAdjust) / 1000;

        foreach (ChapterLineScript script in LoaderScript.Lines)
        {
            script.Reposition(true);
        }
        foreach (ChapterLineScript script in LoaderScript.Lines2P)
        {
            script.Reposition(true);
        }

        NoteSplit.ResetTimeLine(false);
        if (Lyric != null)
            Lyric.ResetTimeLine();
        AnimationController.ResetTimeLine(false);
    }

    public virtual void Restart()
    {
        Hold(false);
    }

    protected virtual void SetChapter(int index)
    {
        Chapter = index;

#if !UNITY_ANDROID
#if UNITY_EDITOR
        if (cri_audio)
        {
            if (AudioSource.status == CriAtomSource.Status.Playing)
            {
                CurrentTime = (Time.time - LastStart) * TimeScale;
                float time = CurrentTime - time_before_start;

                float off = Math.Abs(time - (float)AudioSource.time / 1000);
                if (off > 0.05f)
                {
                    AudioSource.Stop();
                    AudioSource.startTime = (int)(time * 1000);
                    AudioSource.Play();
                }
            }
        }
        else if (Audio.isPlaying)
        {
            CurrentTime = (Time.time - LastStart) * TimeScale;
            float time = CurrentTime - time_before_start;
            if (time > 0 && time < Audio.clip.length && Math.Abs(time - Audio.time) > 0.05f)
                Audio.time = time;
        }
#else
        if (GameSetting.Config.AutoRectify)
        { 
            if (cri_audio)
            {
                if (AudioSource.status == CriAtomSource.Status.Playing)
                {
                    CurrentTime = (Time.time - LastStart) * TimeScale;
                    float time = CurrentTime - time_before_start;

                    float off = Math.Abs(time - (float)AudioSource.time / 1000);
                    if (off > 0.05f)
                    {
                        AudioSource.Stop();
                        AudioSource.startTime = (int)(time * 1000);
                        AudioSource.Play();
                    }
                }
            }
            else if (Audio.isPlaying)
            {
                CurrentTime = (Time.time - LastStart) * TimeScale;
                float time = CurrentTime - time_before_start;
                if (time < Audio.clip.length && Math.Abs(time - Audio.time) > 0.05f)
                    Audio.time = time;
            }
        }
#endif
#endif
    }

    protected readonly float time_before_start = 1f + (15000f / 120f * (4f / 4f)) * 16f / 1000;

    public virtual void Hold(bool hold)
    {
        if (hold)
        {
            Cursor.visible = true;
            StopAllCoroutines();
            if (InputSystem.settings.updateMode == InputSettings.UpdateMode.ProcessEventsInFixedUpdate)
            {
                last_mode = InputSettings.UpdateMode.ProcessEventsInFixedUpdate;
                InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;
            }

            CurrentTime = (Time.time - LastStart) * TimeScale;
            if (cri_audio)
            {
                AudioSource.Stop();
            }
            else
            {
                Audio.Stop();
            }

            foreach (NoteSoundScript script in LoaderScript.Notes.Values)
                script.Hold();
            foreach (ChapterLineScript script in LoaderScript.Lines)
                script.Hold();
            foreach (NoteSoundScript script in LoaderScript.Notes2P.Values)
                script.Hold();
            foreach (ChapterLineScript script in LoaderScript.Lines2P)
                script.Hold();
        }
        else
        {
#if !UNITY_EDITOR
        Cursor.visible = false;
#endif

#if !UNITY_ANDROID
            InputSystem.settings.updateMode = last_mode;
#endif
            foreach (ScoreScript score in Score)
                score.Counting = true;

            //StartCoroutine(StartDeley());
            List<NoteSoundScript> sounds = new List<NoteSoundScript>();
            foreach (NoteSoundScript script in LoaderScript.Notes.Values)
            {
                if (!NotesSplitScript.NormalNotes[true].Contains(script) && !NotesSplitScript.ExpertNotes[true].Contains(script)
                    && !NotesSplitScript.MasterNotes[true].Contains(script) && !NotesSplitScript.NormalNotes[false].Contains(script)
                    && !NotesSplitScript.ExpertNotes[false].Contains(script) && !NotesSplitScript.MasterNotes[false].Contains(script))
                {
                    script.gameObject.SetActive(false);
                }
                else
                {
                    sounds.Add(script);
                    if (script.ShowChapter > Chapter + 2)
                        script.gameObject.SetActive(false);
                }
            }

            //2P
            List<NoteSoundScript> sounds2 = new List<NoteSoundScript>();
            string scene = SceneManager.GetActiveScene().name;
            foreach (NoteSoundScript script in LoaderScript.Notes2P.Values)
            {
                if (scene.Contains("DualGame") && !DualNotesSplitScript.NormalNotes2P[true].Contains(script) && !DualNotesSplitScript.ExpertNotes2P[true].Contains(script)
                    && !DualNotesSplitScript.MasterNotes2P[true].Contains(script) && !DualNotesSplitScript.NormalNotes2P[false].Contains(script)
                    && !DualNotesSplitScript.ExpertNotes2P[false].Contains(script) && !DualNotesSplitScript.MasterNotes2P[false].Contains(script))
                {
                    script.gameObject.SetActive(false);
                }
                else
                {
                    sounds2.Add(script);
                    if (script.ShowChapter > Chapter + 2)
                        script.gameObject.SetActive(false);
                }
            }

            LastStart = Time.time - CurrentTime / TimeScale;

            if (cri_audio)
            {
                if (TimeScale == 1)
                {
                    if (CurrentTime < time_before_start)
                    {
                        AudioSource.startTime = 0;
                        StartCoroutine(DelayPlayAtom(time_before_start - CurrentTime));
                    }
                    else
                    {
                        float time = CurrentTime - time_before_start;
                        if (cri_length > time)
                        {
                            AudioSource.startTime = (int)(time * 1000);
                            AudioSource.Play();
                        }
                    }
                }
            }
            else
            {

#if UNITY_ANDROID
            /*
            if (CurrentTime < time_before_start)
            {
                Seek?.Invoke(0);
                StartCoroutine(DelayPlay((time_before_start - CurrentTime) / TimeScale));
            }
            else
            {
                float time = CurrentTime - time_before_start;
                if (time < AndroidMusic.Duration)
                {
                    Seek?.Invoke((int)(time * 1000));
                    PlayMusic?.Invoke(TimeScale);
                }
            }
            */
            if (CurrentTime < time_before_start)
            {
                Audio.time = 0;
                if (TimeScale == 1)
                    Audio.PlayDelayed((time_before_start - CurrentTime) / TimeScale);
            }
            else
            {
                float time = CurrentTime - time_before_start;
                if (time < Audio.clip.length)
                {
                    Audio.time = time;
                    if (TimeScale == 1)
                        Audio.Play();
                }
            }
#else

                if (CurrentTime < time_before_start)
                {
                    Audio.time = 0;
                    Audio.pitch = TimeScale;
                    Audio.PlayDelayed((time_before_start - CurrentTime) / TimeScale);
                }
                else
                {
                    float time = CurrentTime - time_before_start;
                    if (time < Audio.clip.length)
                    {
                        Audio.time = time;
                        Audio.pitch = TimeScale;
                        Audio.Play();
                    }
                }
#endif
            }
            foreach (NoteSoundScript script in sounds)
                script.StartMoving();
            foreach (ChapterLineScript script in LoaderScript.Lines)
                script.StartMoving();
            foreach (NoteSoundScript script in sounds2)
                script.StartMoving();
            foreach (ChapterLineScript script in LoaderScript.Lines2P)
                script.StartMoving();
            InputScript.Holding = InputScript.HoldStatus.Playing;

            AnimationController.StartGame();
        }
    }

    IEnumerator StartDeley()
    {
        List<NoteSoundScript> sounds = new List<NoteSoundScript>();
        foreach (NoteSoundScript script in LoaderScript.Notes.Values)
        {
            if (!NotesSplitScript.NormalNotes[true].Contains(script) && !NotesSplitScript.ExpertNotes[true].Contains(script) && !NotesSplitScript.MasterNotes[true].Contains(script)
                && !NotesSplitScript.NormalNotes[false].Contains(script) && !NotesSplitScript.ExpertNotes[false].Contains(script) && !NotesSplitScript.MasterNotes[false].Contains(script))
            {
                script.gameObject.SetActive(false);
            }
            else
            {
                sounds.Add(script);
                if (script.ShowChapter > Chapter + 2)
                {
                    script.gameObject.SetActive(false);
                }
            }
        }

        yield return new WaitForSeconds(0.1f);

        LastStart = Time.time - CurrentTime / TimeScale;
        if (CurrentTime < time_before_start)
        {
            Audio.time = 0;
            Audio.pitch = TimeScale;
            Audio.PlayDelayed((time_before_start - CurrentTime) / TimeScale);
        }
        else
        {
            float time = CurrentTime - time_before_start;
            if (time < Audio.clip.length)
            {
                Audio.time = time;
                Audio.pitch = TimeScale;
                Audio.Play();
            }
        }

        foreach (NoteSoundScript script in sounds)
            script.StartMoving();
        foreach (ChapterLineScript script in LoaderScript.Lines)
            script.StartMoving();
        InputScript.Holding = InputScript.HoldStatus.Playing;
    }

    protected static void SetTimeScale()
    {
        foreach (NoteSoundScript script in LoaderScript.Notes.Values)
            script.AdjustPostion();
        foreach (ChapterLineScript script in LoaderScript.Lines)
            script.Reposition();
    }

    protected virtual void PrepareStart()
    {
        Result = new Dictionary<HitNoteResult, List<int>>
        {
            { HitNoteResult.Bad, new List<int>() },
            { HitNoteResult.Perfect, new List<int>() },
            { HitNoteResult.Good, new List<int>() },
        };
        Result2P = new Dictionary<HitNoteResult, List<int>>
        {
            { HitNoteResult.Bad, new List<int>() },
            { HitNoteResult.Perfect, new List<int>() },
            { HitNoteResult.Good, new List<int>() },
        };

        CurrentTime = 0;
        Chapter = 0;
        if (Option != null)
        {
            Option.StartChap = 0;
            Option.EndChap = LoaderScript.Lines.Count;
        }
        Normal_notes_count = 0;
        Big_notes_count = 0;
        Balloon_notes_count = 0;
        Balloon_hit_count = 0;
        float rapid_1p = 0;
        float rapid_2p = 0;

        foreach (ScoreScript score in Score)
            score.Counting = true;

        bool master = false;
        foreach (NoteSoundScript script in LoaderScript.Notes.Values)
        {
            if (script is BallonScript balloon)
            {
                float rate = balloon.HitCount * 1000 / (balloon.EndTime - balloon.JudgeTime);
                if (!script.BranchRelated)
                {
                    Balloon_notes_count++;

                    if (rate > 20)
                        Balloon_hit_count += Mathf.CeilToInt(16.6f * (balloon.EndTime - balloon.JudgeTime) / 1000);
                    else
                        Balloon_hit_count += balloon.HitCount;
                }
                else if (script.Branch == CourseBranch.BranchMaster)
                {
                    Balloon_notes_count++;
                    if (rate > 20)
                        Balloon_hit_count += Mathf.CeilToInt(16.6f * (balloon.EndTime - balloon.JudgeTime) / 1000);
                    else
                        Balloon_hit_count += balloon.HitCount;
                }
            }
            else if (script is RapidScript rapid)
            {
                if (!script.BranchRelated || script.Branch == CourseBranch.BranchMaster)
                    rapid_1p += (rapid.EndTime - rapid.JudgeTime);
            }
            else
            {
                if (!script.BranchRelated)
                {
                    Normal_notes_count++;
                    if (script.Type == 3 || script.Type == 4)
                        Big_notes_count++;
                }
                else if (script.Branch == CourseBranch.BranchMaster)
                {
                    master = true;
                    Normal_notes_count++;
                    if (script.Type == 3 || script.Type == 4)
                        Big_notes_count++;
                }
            }
        }
        if (!master)
        {
            foreach (NoteSoundScript script in LoaderScript.Notes.Values)
            {
                if (script.BranchRelated && script.Branch == CourseBranch.BranchExpert)
                {
                    if (!(script is BallonScript) && !(script is RapidScript))
                    {
                        Normal_notes_count++;
                        if (script.Type == 3 || script.Type == 4)
                            Big_notes_count++;
                    }
                    else if (script is BallonScript balloon)
                    {
                        Balloon_notes_count++;
                        Balloon_hit_count += balloon.HitCount;
                    }
                }
            }
        }

        if ((Score)GameSetting.Config.ScoreMode == CommonClass.Score.Shin)
        {
            if (LoaderScript.b配点が指定されている[2, (int)GameSetting.Difficulty])
            {
                ShinScorePerNote = LoaderScript.ScoreInit[0, (int)GameSetting.Difficulty];
                //Debug.Log(string.Format("score by defual {0}", ShinScorePerNote));
            }
            else
            {
                int balloon_score = Balloon_notes_count * 5000 + (Balloon_hit_count - Balloon_notes_count) * 300;

                //Debug.Log(string.Format("balloon count {0} hit {1}, total {2}", Balloon_notes_count, Balloon_hit_count, balloon_score));
                //Debug.Log(string.Format("note count {0}, big count {1}", Normal_notes_count, Big_notes_count));

                double score = (double)(1000000 - balloon_score) / (Normal_notes_count + Big_notes_count);
                score /= 10;
                double score_up = Math.Ceiling(score);
                double score_down = Math.Floor(score);

                int up = Math.Abs((int)score_up * 10 * (Normal_notes_count + Big_notes_count) + balloon_score - 1000000);
                int down = Math.Abs((int)score_down * 10 * (Normal_notes_count + Big_notes_count) + balloon_score - 1000000);

                if (up < down)
                    ShinScorePerNote = (int)score_up * 10;
                else
                    ShinScorePerNote = (int)score_down * 10;

                //Debug.Log(string.Format("score by caculate {0}", ShinScorePerNote));
            }
        }
        else if ((Score)GameSetting.Config.ScoreMode == CommonClass.Score.Nijiiro)
        {
            int balloon_score = Balloon_hit_count * 100;
            int rapid_score = Mathf.CeilToInt(16.6f * rapid_1p / 1000) * 100;

            //Debug.Log(string.Format("balloon count {0} hit {1}, total {2}", Balloon_notes_count, Balloon_hit_count, balloon_score));
            //Debug.Log(string.Format("rapid time {0}, total {1}", rapid_1p, rapid_score));
            //Debug.Log(string.Format("note count {0}, big count {1}", Normal_notes_count, Big_notes_count));

            float score = (float)(1000000 - balloon_score - rapid_score) / Normal_notes_count;
            score /= 10;
            int score_up = Mathf.CeilToInt(score);
            ShinScorePerNote = ShinScorePerNote2P = score_up * 10;

            ShinScoreTotal = ShinScoreTotal2P = ShinScorePerNote * Normal_notes_count + balloon_score + rapid_score;
            //Debug.Log(string.Format("score by caculate {0}, total {1}", ShinScorePerNote, ShinScoreTotal));
        }

        //2p
        string scene = SceneManager.GetActiveScene().name;
        if (scene.Contains("DualGame"))
        {
            Normal_notes_count_2p = 0;
            Big_notes_count_2p = 0;
            Balloon_notes_count_2p = 0;
            Balloon_hit_count_2p = 0;

            master = false;
            foreach (NoteSoundScript script in LoaderScript.Notes2P.Values)
            {
                if (script is BallonScript balloon)
                {
                    float rate = balloon.HitCount * 1000 / (balloon.EndTime - balloon.JudgeTime);
                    if (!script.BranchRelated)
                    {
                        Balloon_notes_count_2p++;
                        if (rate > 20)
                            Balloon_hit_count_2p += Mathf.CeilToInt(16.6f * (balloon.EndTime - balloon.JudgeTime) / 1000);
                        else
                            Balloon_hit_count_2p += balloon.HitCount;
                    }
                    else if (script.Branch == CourseBranch.BranchMaster)
                    {
                        Balloon_notes_count_2p++;
                        if (rate > 20)
                            Balloon_hit_count_2p += Mathf.CeilToInt(16.6f * (balloon.EndTime - balloon.JudgeTime) / 1000);
                        else
                            Balloon_hit_count_2p += balloon.HitCount;
                    }
                }
                else if (script is RapidScript rapid)
                {
                    if (!script.BranchRelated || script.Branch == CourseBranch.BranchMaster)
                        rapid_2p += (rapid.EndTime - rapid.JudgeTime);
                }
                else
                {
                    if (!script.BranchRelated)
                    {
                        Normal_notes_count_2p++;
                        if (script.Type == 3 || script.Type == 4)
                            Big_notes_count_2p++;
                    }
                    else if (script.Branch == CourseBranch.BranchMaster)
                    {
                        master = true;
                        Normal_notes_count_2p++;
                        if (script.Type == 3 || script.Type == 4)
                            Big_notes_count_2p++;
                    }
                }
            }
            if (!master)
            {
                foreach (NoteSoundScript script in LoaderScript.Notes2P.Values)
                {
                    if (script.BranchRelated && script.Branch == CourseBranch.BranchExpert)
                    {
                        if (!(script is BallonScript) && !(script is RapidScript))
                        {
                            Normal_notes_count_2p++;
                            if (script.Type == 3 || script.Type == 4)
                                Big_notes_count_2p++;
                        }
                        else if (script is BallonScript balloon)
                        {
                            Balloon_notes_count_2p++;
                            float rate = balloon.HitCount * 1000 / (balloon.EndTime - balloon.JudgeTime);
                            if (rate > 20)
                                Balloon_hit_count_2p += Mathf.CeilToInt(16.6f * (balloon.EndTime - balloon.JudgeTime) / 1000);
                            else
                                Balloon_hit_count_2p += balloon.HitCount;
                        }
                    }
                }
            }

            if ((Score)GameSetting.Config.ScoreMode == CommonClass.Score.Shin)
            {
                if (LoaderScript.b配点が指定されている[2, (int)GameSetting.Difficulty2P])
                {
                    ShinScorePerNote2P = LoaderScript.ScoreInit[0, (int)GameSetting.Difficulty2P];
                    //Debug.Log(string.Format("score by defual {0}", ShinScorePerNote));
                }
                else
                {
                    int balloon_score = Balloon_notes_count_2p * 5000 + (Balloon_hit_count_2p - Balloon_notes_count_2p) * 300;

                    //Debug.Log(string.Format("balloon count {0} hit {1}, total {2}", Balloon_notes_count, Balloon_hit_count, balloon_score));
                    //Debug.Log(string.Format("note count {0}, big count {1}", Normal_notes_count, Big_notes_count));

                    double score = (double)(1000000 - balloon_score) / (Normal_notes_count_2p + Big_notes_count_2p);
                    score /= 10;
                    double score_up = Math.Ceiling(score);
                    double score_down = Math.Floor(score);

                    int up = Math.Abs((int)score_up * 10 * (Normal_notes_count_2p + Big_notes_count_2p) + balloon_score - 1000000);
                    int down = Math.Abs((int)score_down * 10 * (Normal_notes_count_2p + Big_notes_count_2p) + balloon_score - 1000000);

                    if (up < down)
                        ShinScorePerNote2P = (int)score_up * 10;
                    else
                        ShinScorePerNote2P = (int)score_down * 10;

                    //Debug.Log(string.Format("score by caculate {0}", ShinScorePerNote));
                }
            }
            else if ((Score)GameSetting.Config.ScoreMode == CommonClass.Score.Nijiiro)
            {
                int balloon_score = Balloon_hit_count_2p * 100;
                int rapid_score = Mathf.CeilToInt(16.6f * rapid_2p / 1000) * 100;

                //Debug.Log(string.Format("balloon 2p count {0} hit {1}, total {2}", Balloon_notes_count_2p, Balloon_hit_count_2p, balloon_score));
                //Debug.Log(string.Format("rapid 2p time {0}, total {1}", rapid_2p, rapid_score));
                //Debug.Log(string.Format("note 2p count {0}, big count {1}", Normal_notes_count_2p, Big_notes_count_2p));

                float score = (float)(1000000 - balloon_score - rapid_score) / Normal_notes_count_2p;
                score /= 10;
                int score_up = Mathf.CeilToInt(score);
                ShinScorePerNote2P = score_up * 10;
                ShinScoreTotal2P = ShinScorePerNote2P * Normal_notes_count_2p + balloon_score + rapid_score;

                //Debug.Log(string.Format("score 2p by caculate {0}, total {1}", ShinScorePerNote2P, ShinScoreTotal2P));
            }
        }
    }

    public virtual void Play()
    {
        LastStart = Time.time;

        if (cri_audio)
        {
            AudioSource.volume = GameSetting.Config.MusicVolume * GameSetting.SelectedInfo.Volume / 100;
            AudioSource.startTime = 0;
            if (TimeScale == 1)
                StartCoroutine(DelayPlayAtom(time_before_start));
        }
        else
        {
#if UNITY_ANDROID
        /*
        Seek?.Invoke(0);
        Volume?.Invoke(GameSetting.Config.MusicVolume);
        StartCoroutine(DelayPlay(time_before_start / TimeScale));
        */

        Audio.volume = GameSetting.Config.MusicVolume * GameSetting.SelectedInfo.Volume / 100;
        Audio.clip = GameSetting.Selected;
        if (TimeScale == 1)
            Audio.PlayDelayed(time_before_start / TimeScale);
#else

            Audio.volume = GameSetting.Config.MusicVolume * GameSetting.SelectedInfo.Volume / 100;
            Audio.clip = GameSetting.Selected;
            Audio.pitch = TimeScale;
#if UNITY_EDITOR
            Debug.Log(string.Format("volum {0} delay {1}", Audio.volume, time_before_start));
#endif
            Audio.PlayDelayed(time_before_start / TimeScale);
#endif
        }
        //NoteSplit.Play();
        AnimationController.StartGame();
    }

    IEnumerator DelayPlay(float time)
    {
        yield return new WaitForSeconds(time);
        PlayMusic?.Invoke(TimeScale);
    }

    IEnumerator DelayPlayAtom(float time)
    {
        yield return new WaitForSeconds(time);
        AudioSource.Play();
    }
}
