using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CommonClass;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public delegate void PlaySelectedSoundDelegate(StartSoundControllScript.SoundType type);
public delegate void EndPlayDelegate();
public class GloableAniControllerScript : MonoBehaviour
{
    public static event PlaySelectedSoundDelegate PlaySelectedSound;
    public static event EndPlayDelegate EndPlay;

    public DonScript[] Don_Avatar;
    public Animator[] BranchLevel;
    public BreakingBalloon[] BreakingBalloons;
    public KusudamaShowScript[] Kusudama;
    public ComboBonusScript[] ComboBonus;
    public ExplosionPoolScript[] JudgeExplosion;
    public RapidHitScript[] RapidHit;
    public ComboScript[] Combo;
    public GaugeScript[] Gauge;
    public GameObject QuitMask;
    public SpriteRenderer RestartMask;
    public GameObject[] GoGOTimeBG;
    public Animator[] GoGoFire;
    public GogoTimeSplashControllScript GogoSplash;
    public JumpOutEffectScript[] JumpOut;
    public Animator[] BranchBGs;
    public RunnerPrefebScript Runner;
    public ScoreScript[] ScoreController;
    public OptionScript Option;
    public InputScript Input;
    public SpriteRenderer[] DifficultyImage;
    public SpriteRenderer[] DifficultyChar;
    public SpriteRenderer[] BranchChar;
    public DancerInitScript Dancer;
    public BottomScript Bottom;
    public GameResultStartScript GameResult;
    public RendaControllerScript Renda;
    public NoteTickScript[] Tick;
    public GameObject Lyrics;
    public PracticeScript Practice;
    public PlayNoteScript PlayNote;

    public int ComboCount;
    public Text SongName;

    public GameObject[] Marks;
    public GameObject[] Mark2P;
    public Sprite[] RandomSprites;
    public Sprite[] SpeedSprites;
    public Sprite[] SpecialSprites;
    public Animator[] SuccessfulBg;
    public DonImageScript[] DonImages;
    public ResultNoticeControll ResultNotice;

    public int TotalScore { set; get; }
    public NijiiroRank Rank { set; get; }
    public int MaxCombo { set; get; }
    public int DrumRolls { set; get; }
    public float RestartTime { set; get; }
    public static CourseBranch Branch { set; get; } = CourseBranch.BranchNormal;
    public static CourseBranch Branch2P { set; get; } = CourseBranch.BranchNormal;
    public static GloableAniControllerScript Instance { set; get; }
    public static float GaugePoints { set; get; }
    public static List<NoteReplay> NotesRecord;

    protected int shown_layer = 5;
    protected int hide_layer = 12;
    protected bool playing;
    protected Queue<int> times = new Queue<int>();
    protected Dictionary<float, List<CChip>> chips = new Dictionary<float, List<CChip>>();
    protected Dictionary<HitNoteResult, int> branch_hit_rate = new Dictionary<HitNoteResult, int>
    {
        { HitNoteResult.Perfect, 0 }, { HitNoteResult.Good, 0 },{ HitNoteResult.Bad, 0 }, { HitNoteResult.None, 0 }
    };
    protected int branch_score;
    protected CourseBranch old_branch;
    protected bool branch_level_hold;
    protected int bad_count = 0;
    protected List<NoteReplay> notes_replay;
    protected Dictionary<int, bool[]> global_hide = new Dictionary<int, bool[]> { { 0, new bool[2] { false, false } }, { 1, new bool[2] { false, false } } };

    protected bool rapid_don = false;
    protected bool rapid_ka = false;
    protected NoteSoundScript rapid;
    protected bool rapid_don_2p = false;
    protected bool rapid_ka_2p = false;
    protected NoteSoundScript rapid_2p;
    protected string scene;

    //2P
    public int ComboCount2P { set; get; }
    public static float GaugePoints2P { set; get; }
    public int TotalScore2P { set; get; }
    public NijiiroRank Rank2P { set; get; }
    public int MaxCombo2P { set; get; }
    public int DrumRolls2P { set; get; }
    protected Dictionary<HitNoteResult, int> branch_hit_rate_2p = new Dictionary<HitNoteResult, int>
    {
        { HitNoteResult.Perfect, 0 }, { HitNoteResult.Good, 0 },{ HitNoteResult.Bad, 0 }, { HitNoteResult.None, 0 }
    };
    protected int branch_score_2p;
    protected CourseBranch old_branch_2p;
    protected int bad_count_2p = 0;
    protected bool replay;
    void Start()
    {
        NotesRecord = new List<NoteReplay>();
        Instance = this;
        InputScript.QuitPlay += QuitGame;
        BreakingBalloon.BalloonBroken += ShowAvatar;
        BreakingBalloon.BalloonBroken2P += ShowAvatar2;

        if (GameSetting.Mode != CommonClass.PlayMode.Replay)
        {
            HammerShowScript.BalloonSucceed += HammerBroken;
            KusudamaShowScript.BalloonSucceed += HammerBroken;
        }

        LoaderScript.Play += SetVariant;
        OptionScript.SetBranch += SetBranch;
        OptionScript.SetAuto += SetAutoMark;

        string song_name = GameSetting.SelectedInfo.Title;
        SetSongName(song_name);

        if (GameSetting.SelectedInfo.Branches.ContainsKey(GameSetting.Difficulty) && GameSetting.SelectedInfo.Branches[GameSetting.Difficulty])
        {
            foreach (Animator bg in BranchBGs)
                bg.SetTrigger("Show");
        }

        string dif_path = "Texture/Difficulties";
        scene = SceneManager.GetActiveScene().name;
#if UNITY_ANDROID
        dif_path = "Texture/DifficultiesNijiiro";
#else
        if (scene.Contains("PS4"))
            dif_path = "Texture/DifficultiesPs4";
        else if (scene.Contains("Nijiiro"))
            dif_path = "Texture/DifficultiesNijiiro";
#endif
        foreach (Sprite sprite in Resources.LoadAll<Sprite>(dif_path))
        {
            if (sprite.name == string.Format("lv{0}", (int)GameSetting.Difficulty + 1))
            {
                foreach (SpriteRenderer renderer in DifficultyImage)
                    renderer.sprite = sprite;
                break;
            }
        }
        foreach (SpriteRenderer diff in DifficultyChar)
            diff.sprite = SettingLoader.Diffs[(int)GameSetting.Difficulty];

        BranchChar[0].sprite = SettingLoader.Branches[0];
        BranchChar[1].sprite = SettingLoader.Branches[1];
        if (BranchChar.Length > 2)
        {
            BranchChar[2].sprite = SettingLoader.Branches[0];
            BranchChar[3].sprite = SettingLoader.Branches[1];
        }

        GaugePoints = 0;
        GaugePoints2P = 0;
        SetAutoMark();
        ArrengeMarks();

#if UNITY_ANDROID
        if (!GameSetting.Dancer) Dancer = null;
#endif
        if (GameSetting.Mode == CommonClass.PlayMode.Practice)
            Practice.gameObject.SetActive(true);

        if (GameSetting.Replay != null)
            notes_replay = GameSetting.Replay.Show[GameSetting.Config.ScoreMode][(int)GameSetting.Difficulty];
    }

    protected void SetSongName(string song_name)
    {
        if (song_name.Length < 8)
        {
            int totalLength = 0;
            Font myFont = SongName.font;  //chatText is my Text component
            myFont.RequestCharactersInTexture(song_name, SongName.fontSize, SongName.fontStyle);
            char[] arr = song_name.ToCharArray();
            foreach (char c in arr)
            {
                CharacterInfo characterInfo;
                myFont.GetCharacterInfo(c, out characterInfo, SongName.fontSize);
                totalLength += characterInfo.advance;
            }
            //Debug.Log(totalLength);

            if (totalLength < 290)
                SongName.transform.localPosition -= new Vector3((290 - totalLength) / 2, 0);
        }

        SongName.text = song_name;
    }

    protected virtual void ArrengeMarks()
    {

        List<GameObject> marks_1p = new List<GameObject> { Marks[2] };
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
        Special special = Special.None;
        if (GameSetting.Mode == CommonClass.PlayMode.Replay)
        {
            special = GameSetting.Replay.Config[GameSetting.Config.ScoreMode][(int)GameSetting.Difficulty].Special;
        }
        else if (GameSetting.Mode != CommonClass.PlayMode.Practice)
        {
            special = GameSetting.Config.Special;
        }

        if (GameSetting.Mode == CommonClass.PlayMode.Replay)
        {
            speed = GameSetting.Replay.Config[GameSetting.Config.ScoreMode][(int)GameSetting.Difficulty].Speed;
            random = GameSetting.Replay.Config[GameSetting.Config.ScoreMode][(int)GameSetting.Difficulty].Random;
            steal = GameSetting.Replay.Config[GameSetting.Config.ScoreMode][(int)GameSetting.Difficulty].Steal;
            revers = GameSetting.Replay.Config[GameSetting.Config.ScoreMode][(int)GameSetting.Difficulty].Reverse;
        }
        if (GameSetting.Mode == CommonClass.PlayMode.Practice)
        {
            Marks[0].SetActive(true);
            marks_1p.Add(Marks[0]);
        }
        if (GameSetting.Mode == CommonClass.PlayMode.Replay)
        {
            Marks[1].SetActive(true);
            marks_1p.Add(Marks[1]);
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
        //特殊
        if (special > Special.AutoPlay)
        {
            Marks[9].SetActive(true);
            marks_1p.Add(Marks[9]);

            int index = (int)special - 2;
            SpriteRenderer renderer = Marks[9].GetComponent<SpriteRenderer>();
            renderer.sprite = SpecialSprites[index];
        }

        int x = 0, y = 0;

        int counts_one_line = 2;
        if (scene.Contains("Nijiiro")) counts_one_line = 3;
        foreach (GameObject mark in marks_1p)
        {
            mark.transform.localPosition = new Vector3(x * 0.41f - 0.47f, 0.39f - y * 0.41f);
            x++;
            if (x >= counts_one_line)
            {
                x = 0;
                y++;
            }
        }

        //2P mark
        if (GameSetting.Mode == CommonClass.PlayMode.PlayWithReplay)
        {
            Config config = new Config();
            config.CopyReplayConfig(GameSetting.Replay.Config[GameSetting.Config.ScoreMode][(int)GameSetting.Difficulty]);

            Mark2P[1].SetActive(true);
            marks_2p.Add(Mark2P[1]);

            if ((Score)GameSetting.Config.ScoreMode == Score.Shin)
            {
                Mark2P[3].SetActive(true);
                marks_2p.Add(Mark2P[3]);
            }
            if (config.Speed != Speed.Normal)
            {
                int index = (int)speed - 2;
                if ((int)config.Speed > 10) index -= 6;
                SpriteRenderer renderer = Mark2P[4].GetComponent<SpriteRenderer>();
                renderer.sprite = SpeedSprites[index];
                Mark2P[4].SetActive(true);
                marks_2p.Add(Mark2P[4]);
            }
            if (config.Random != RandomType.None)
            {
                int index = (int)random - 1;
                Mark2P[5].SetActive(true);
                marks_2p.Add(Mark2P[5]);

                SpriteRenderer renderer = Mark2P[5].GetComponent<SpriteRenderer>();
                renderer.sprite = RandomSprites[index];
            }
            if (config.Reverse)
            {
                Mark2P[6].SetActive(true);
                marks_2p.Add(Mark2P[6]);
            }
            if (config.Steal)
            {
                Mark2P[8].SetActive(true);
                marks_2p.Add(Mark2P[8]);
            }

            if (config.Special > Special.AutoPlay)
            {
                Mark2P[9].SetActive(true);
                marks_2p.Add(Mark2P[9]);

                int index = (int)config.Special - 2;
                SpriteRenderer renderer = Mark2P[9].GetComponent<SpriteRenderer>();
                renderer.sprite = SpecialSprites[index];
            }

            x = 0;
            y = 0;
            foreach (GameObject mark in marks_2p)
            {
                mark.transform.localPosition = new Vector3(x * 0.41f - 0.47f, 0.39f - y * 0.41f);
                x++;
                if (x >= counts_one_line)
                {
                    x = 0;
                    y++;
                }
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

    public virtual void SetVariant()
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
        times.Sort((x, y) => { return x < y ? -1 : 1; });
        foreach (int time in times)
            this.times.Enqueue(time);
    }

    public virtual void StartGame()
    {
        if (Dancer != null) Dancer.StartPlay();

        if (GameSetting.Replay != null && !replay)
        {
            replay = true;
            StartCoroutine(DoReplay());
        }
    }

    IEnumerator DoReplay()
    {
        foreach (NoteReplay note in notes_replay)
        {
            yield return new WaitForSeconds(note.Time + (float)GameSetting.Config.NoteAdjust / 1000 - (Time.time - PlayNoteScript.LastStart));
            switch (note.Hit)
            {
                case HitState.Miss:
                    {
                        if (GameSetting.Mode == CommonClass.PlayMode.PlayWithReplay)
                        {
                            CountResult2P(LoaderScript.Notes2P[note.Note]);
                        }
                        else
                        {
                            CountResult(LoaderScript.Notes[note.Note]);
                        }
                    }
                    break;
                case HitState.BalloonFail:
                    {
                        if (GameSetting.Mode == CommonClass.PlayMode.PlayWithReplay)
                        {
                            BrokenBalloon2P(LoaderScript.Notes2P[note.Note], BalloonResult.Fail);
                        }
                        else
                        {
                            BrokenBalloon(LoaderScript.Notes[note.Note], BalloonResult.Fail);
                        }
                    }
                    break;
                case HitState.BalloonPerfect:
                    {
                        if (GameSetting.Mode == CommonClass.PlayMode.PlayWithReplay)
                        {
                            BrokenBalloon2P(LoaderScript.Notes2P[note.Note], BalloonResult.Perfect);
                        }
                        else
                        {
                            BrokenBalloon(LoaderScript.Notes[note.Note], BalloonResult.Perfect);
                        }
                    }
                    break;
                case HitState.BalloonGood:
                    {
                        if (GameSetting.Mode == CommonClass.PlayMode.PlayWithReplay)
                        {
                            BrokenBalloon2P(LoaderScript.Notes2P[note.Note], BalloonResult.Good);
                        }
                        else
                        {
                            BrokenBalloon(LoaderScript.Notes[note.Note], BalloonResult.Good);
                        }
                    }
                    break;
                case HitState.HammerShow:
                    {
                        if (GameSetting.Mode == CommonClass.PlayMode.PlayWithReplay)
                        {
                            //NoteSoundScript script = LoaderScript.Notes2P[note.Note];
                            //ShowBalloon2P(script, 0);
                        }
                        else
                        {
                            NoteSoundScript script = LoaderScript.Notes[note.Note];
                            ShowBalloon(script, 0);
                        }
                    }
                    break;
                case HitState.Perfect:
                    {
                        if (GameSetting.Mode == CommonClass.PlayMode.PlayWithReplay)
                        {
                            NoteSoundScript script = LoaderScript.Notes2P[note.Note];
                            script.Hit();
                            script.gameObject.SetActive(false);
                            ShowResult2P(HitNoteResult.Perfect, script);
                        }
                        else
                        {
                            NoteSoundScript script = LoaderScript.Notes[note.Note];
                            script.Hit();
                            script.gameObject.SetActive(false);
                            ShowResult(HitNoteResult.Perfect, script);
                        }
                    }
                    break;
                case HitState.Good:
                    {
                        if (GameSetting.Mode == CommonClass.PlayMode.PlayWithReplay)
                        {
                            NoteSoundScript script = LoaderScript.Notes2P[note.Note];
                            script.Hit();
                            script.gameObject.SetActive(false);
                            ShowResult2P(HitNoteResult.Good, script);
                        }
                        else
                        {
                            NoteSoundScript script = LoaderScript.Notes[note.Note];
                            script.Hit();
                            script.gameObject.SetActive(false);
                            ShowResult(HitNoteResult.Good, script);
                        }
                    }
                    break;
                case HitState.Bad:
                    {
                        if (GameSetting.Mode == CommonClass.PlayMode.PlayWithReplay)
                        {
                            NoteSoundScript script = LoaderScript.Notes2P[note.Note];
                            script.Hit();
                            script.gameObject.SetActive(false);
                            ShowResult2P(HitNoteResult.Bad, script);
                        }
                        else
                        {
                            NoteSoundScript script = LoaderScript.Notes[note.Note];
                            script.Hit();
                            script.gameObject.SetActive(false);
                            ShowResult(HitNoteResult.Bad, script);
                        }
                    }
                    break;
                case HitState.RapidEnd:
                    if (GameSetting.Mode == CommonClass.PlayMode.PlayWithReplay)
                    {
                        ShowRapid2P(false);
                    }
                    else
                    {
                        ShowRapid(false);
                    }
                    break;
                case HitState.HitDon:
                    {
                        if (GameSetting.Mode == CommonClass.PlayMode.PlayWithReplay)
                        {
                            NoteSoundScript script = LoaderScript.Notes2P[note.Note];
                            script.Hit();
                            if (script.Type != 9)
                                Hit2P(script, true);
                            else
                                CaculaBalloonHit(script);
                        }
                        else
                        {
                            NoteSoundScript script = LoaderScript.Notes[note.Note];
                            script.Hit();
                            Hit(script, true);
                        }
                    }
                    break;
                case HitState.HitKa:
                    {
                        if (GameSetting.Mode == CommonClass.PlayMode.PlayWithReplay)
                        {
                            NoteSoundScript script = LoaderScript.Notes2P[note.Note];
                            script.Hit();
                            if (script.Type != 9)
                                Hit2P(script, false);
                            else
                                CaculaBalloonHit(script);
                        }
                        else
                        {
                            NoteSoundScript script = LoaderScript.Notes[note.Note];
                            script.Hit();
                            Hit(script, false);
                        }
                    }
                    break;
                case HitState.HammerChange:
                    if (GameSetting.Mode == CommonClass.PlayMode.PlayWithReplay)
                        HammerChange2P();
                    else
                        HammerChange();
                    break;
            }
        }
    }

    public virtual void ResetTimeLine(bool animating)
    {
        Time.timeScale = 1;
        foreach (KusudamaShowScript kusu in Kusudama)
            kusu.gameObject.SetActive(false);

        foreach (ScoreScript score in ScoreController)
            score.Counting = false;

        SetNoteCombo(NoteSoundScript.ComboType.Combo_None);
        if (GameSetting.Mode == CommonClass.PlayMode.PlayWithReplay)
            SetNoteCombo2P(NoteSoundScript.ComboType.Combo_None);
        TotalScore = 0;
        Rank = Rank2P = NijiiroRank.None;
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

        branch_level_hold = false;

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

            DonImages[i].SetBalloon(false);
            DonImages[i].SetMax(false);
            DonImages[i].SetMiss(false);
        }

        ComboCount = 0;
        GaugePoints = 0;
        ComboCount2P = 0;
        GaugePoints2P = 0;

        bool gogo_in_time_line = false;
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

                if (time / 1000 <= PlayNoteScript.CurrentTime && end / 1000 >= PlayNoteScript.CurrentTime)
                    gogo_in_time_line = true;
            }

            if (chip.Type == CChip.CType.BPM_CHANGE && time / 1000 <= PlayNoteScript.CurrentTime)
                bpm = chip.Bpm;
        }

        times.Sort((x, y) => { return x < y ? -1 : 1; });
        foreach (int time in times)
            this.times.Enqueue(time);

        if (Dancer != null) Dancer.ResetDancer();
        SetBPM(bpm);
        SetGoGo(gogo_in_time_line);

        foreach (GaugeScript gauge in Gauge)
            gauge.ResetGauge();

        if (Runner != null)
            Runner.ResetTimeLine();

        foreach (ExplosionPoolScript judge in JudgeExplosion)
            judge.ResetTimeLine();

        BranchReset(true);
    }

    protected virtual void SetBPM(float bpm)
    {
        foreach (DonScript don in Don_Avatar)
            don.SetBpm(bpm);

        if (Dancer != null)
            Dancer.SetBpm(bpm);
        if (Bottom != null)
            Bottom.SetBPM(bpm);

        if (Runner != null)
        {
            Runner.SetBpm(bpm, false);
            Runner.SetBpm(bpm, true);
        }
        foreach (BreakingBalloon balloon in BreakingBalloons)
            balloon.SetBpm(bpm);
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
                        SetGoGo(true);
                    }
                    else if (chip.EndTime + GameSetting.Config.NoteAdjust == time)
                    {
                        SetGoGo(false);
                    }
                }
                else if (chip.Type == CChip.CType.BPM_CHANGE)
                {
                    SetBPM(chip.Bpm);
                }
                else if (chip.Type == CChip.CType.BRANCH_RESET)
                {
                    BranchReset(false);
                }
                else if (chip.Type == CChip.CType.BRANCH_NOTICE)
                {
                    BranchInter(chip.BranchIndex);
                }
                else if (chip.Type == CChip.CType.BRANCH_START)
                {
                    SetCurrentBranch();
                }
                else if (chip.Type == CChip.CType.LEVEL_HOLD && chip.Branch == Branch)
                {
                    branch_level_hold = true;
                }
                else if (chip.Type == CChip.CType.END)
                {
                    EndPlaying();
                }
            }
        }
    }

    protected virtual void EndPlaying()
    {
        if (GameSetting.Mode == CommonClass.PlayMode.Practice)
        {
            Input.OpenOption();
        }
        else
        {
            InputScript.Holding = InputScript.HoldStatus.Finished;

            SetGoGo(false);
            if (Lyrics != null)
                Lyrics.SetActive(false);

            if (ResultNotice != null)
                ResultNotice.ShowResult(this);
            else
                GameResult.StartAnimating();
        }
    }

    protected int notice_count;
    public virtual void EndPlayByNotice()
    {
#if UNITY_EDITOR
        EndPlay?.Invoke();
#endif
        notice_count++;
        if (!scene.Contains("Duel") || notice_count == 2)
            GameResult.StartAnimating();
    }

    protected virtual void BranchReset(bool reset)
    {
        branch_score = 0;
        branch_score_2p = 0;
        List<HitNoteResult> keys = new List<HitNoteResult>(branch_hit_rate.Keys);
        foreach (HitNoteResult hit in keys)
        {
            branch_hit_rate[hit] = 0;
            branch_hit_rate_2p[hit] = 0;
        }
        
        if (reset && (Option == null || Option.Branch == CourseBranch.BranchNormal))
        {
            old_branch = old_branch_2p = CourseBranch.BranchNormal;
            Branch = Branch2P = CourseBranch.BranchNormal;

            foreach (Animator bg in BranchBGs)
                bg.SetTrigger("Reset");
            if (GameSetting.SelectedInfo.Branches.ContainsKey(GameSetting.Difficulty) && GameSetting.SelectedInfo.Branches[GameSetting.Difficulty])
            {
                foreach (Animator bg in BranchBGs)
                    bg.SetTrigger("Show");
            }
        }
    }

    protected virtual void SetBranch(CourseBranch branch)
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
    protected virtual void SetBranch2P(CourseBranch branch)
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

    protected virtual void SetCurrentBranch()
    {
        InputScript.CurrentBranch = Branch;
        List<HitNoteResult> keys = new List<HitNoteResult>(branch_hit_rate.Keys);
        foreach (HitNoteResult hit in keys)
        {
            branch_hit_rate[hit] = 0;
            branch_hit_rate_2p[hit] = 0;
        }
        branch_score = 0;
        branch_score_2p = 0;
    }

    protected virtual void BranchInter(int index)
    {
        if (branch_level_hold)
        {
            branch_level_hold = false;
        }
        else if (Option == null || Option.Branch == CourseBranch.BranchNormal)
        {
            CourseBranch result = CourseBranch.BranchNormal;
            CourseBranch result2 = CourseBranch.BranchNormal;
            CBRANCH branch = LoaderScript.ListBRANCH[index];
            /*
            if ((Score)GameSetting.Config.ScoreMode == Score.Nijiiro)
            {
                result = CourseBranch.BranchMaster;
                result2 = CourseBranch.BranchMaster;
            }
            else
            {
            */
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

                        if (BranchLevel.Length > 1)
                        {
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
                        if (BranchLevel.Length > 1)
                        {
                            if (branch_hit_rate_2p[HitNoteResult.None] >= branch.n条件数値B)
                            {
                                result2 = CourseBranch.BranchMaster;
                            }
                            else if (branch_hit_rate_2p[HitNoteResult.None] >= branch.n条件数値A)
                            {
                                result2 = CourseBranch.BranchExpert;
                            }
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

                        if (BranchLevel.Length > 1)
                        {
                            if (branch_score_2p >= branch.n条件数値B)
                            {
                                result2 = CourseBranch.BranchMaster;
                            }
                            else if (branch_score_2p >= branch.n条件数値A)
                            {
                                result2 = CourseBranch.BranchExpert;
                            }
                        }
                        break;
                    case 3:                                 //？？？
                        break;
                }
            //}

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

            if (BranchLevel.Length > 1)
            {
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
    }

    protected void ChangeBranch(int index)
    {
        if (index > 0) NotesSplitScript.Instance.SetBranch(Branch, index);
        if ((Option == null || Option.Branch == CourseBranch.BranchNormal) && Branch != old_branch)
        {
            SetBranch(Branch);
            //Debug.Log(string.Format("into branch {0}, current input branch is {1}", Branch, InputScript.CurrentBranch));
        }
    }

    protected void ChangeBranch2P(int index)
    {
        NotesSplitScript.Instance.SetBranch2P(Branch2P, index);
        if (Branch2P != old_branch_2p)
        {
            SetBranch2P(Branch2P);
            //Debug.Log(string.Format("into branch {0}, current input branch is {1}", Branch2P, InputScript.CurrentBranch2P));
        }
    }
    public virtual void ShowRapid(bool show)
    {
        if (!show && RapidHit[0].gameObject.activeSelf)
        {
#if !UNITY_ANDROID
            if (GameSetting.Mode != CommonClass.PlayMode.Practice && GameSetting.Mode != CommonClass.PlayMode.Replay)
                NotesRecord.Add(new NoteReplay(0, Time.time - PlayNoteScript.LastStart - (float)GameSetting.Config.NoteAdjust / 1000, HitState.RapidEnd));
#endif
            RapidHit[0].Close(false);
        }
    }

    public void ShowRapid2P(bool show)
    {
        if (!show && RapidHit[1].gameObject.activeSelf)
            RapidHit[1].Close(false);
    }

    public virtual void ShowBalloon(NoteSoundScript note, int Phase)
    {
        if (note.Type == 9)
        {
#if !UNITY_ANDROID
            if (GameSetting.Mode != CommonClass.PlayMode.Practice && GameSetting.Mode != CommonClass.PlayMode.Replay)
                NotesRecord.Add(new NoteReplay(note.Index, Time.time - PlayNoteScript.LastStart - (float)GameSetting.Config.NoteAdjust / 1000, HitState.HammerShow));
#endif
            if (Phase == 0 && note is KusudamaNoteScript script)
            {
                for (int i = 0; i < Don_Avatar.Length; i++)
                {
                    global_hide[1][i] = true;
                    Don_Avatar[i].Hide(true, false);
                    DonImages[i].SetBalloon(true);
                }
                Kusudama[0].Play(script.HitCount);
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
                Kusudama[0].Play(hammer.HitCount);
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

    public virtual void ShowBalloon2P(NoteSoundScript note, int Phase)
    {
        if (note.Type == 9)
        {
            /*
            if (GameSetting.Mode != CommonClass.PlayMode.Practice && GameSetting.Mode != CommonClass.PlayMode.Replay)
                NotesRecord.Add(new NoteReplay(note.Index, Time.time - PlayNoteScript.LastStart, HitState.HammerShow));

            if (Phase == 0 && note is KusudamaNoteScript script)
            {
                global_hide[1][1] = true;
                Don_Avatar[1].Hide(true);
                Kusudama[1].Play(script.HitCount);
            }
            if (Phase == 0 && note is HammerScript hammer)
            {
                Don_Avatar[1].Hide(true);
                if (Kusudama[1] is HammerShowScript hammer_show)
                {
                    hammer_show.Silver.SetActive(false);
                    hammer_show.Gold.SetActive(true);
                }
                Kusudama[1].Play(hammer.HitCount);
            }
            else
                Kusudama[1].SetPhase(Phase);
            */
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

    public virtual void HammerChange()
    {
        if (Kusudama[0] is HammerShowScript hammer)
        {
#if !UNITY_ANDROID
            if (GameSetting.Mode != CommonClass.PlayMode.Practice && GameSetting.Mode != CommonClass.PlayMode.Replay)
                NotesRecord.Add(new NoteReplay(0, Time.time - PlayNoteScript.LastStart - (float)GameSetting.Config.NoteAdjust / 1000, HitState.HammerChange));
#endif
            hammer.ChangeSilver();
        }
    }

    public void HammerChange2P()
    {
        /*
        if (Kusudama[1] is HammerShowScript hammer)
        {
            hammer.Silver.SetActive(true);
            hammer.Gold.SetActive(false);
        }
        */
    }

    public virtual void Hit(NoteSoundScript note, bool don)
    {
        if (GameSetting.Mode != CommonClass.PlayMode.Practice && GameSetting.Mode != CommonClass.PlayMode.Replay)
            NotesRecord.Add(new NoteReplay(note.Index, Time.time - PlayNoteScript.LastStart - (float)GameSetting.Config.NoteAdjust / 1000, don ? HitState.HitDon : HitState.HitKa));

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

    public virtual void Hit2P(NoteSoundScript note, bool don)
    {
        switch (note.Type)
        {
            case 5:
            case 6:
                if (!RapidHit[1].Activated) RapidHit[1].Show();

                RapidHit[1].Hit();
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
            if (Renda != null) Renda.DoRapitHit();
        }
        if (rapid_ka)
        {
            rapid_ka = false;
            JumpOut[0].Play(rapid, false);
            if (Renda != null) Renda.DoRapitHit();
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

    public virtual void BrokenBalloon(NoteSoundScript note, BalloonResult result)
    {
#if !UNITY_ANDROID
        if (GameSetting.Mode != CommonClass.PlayMode.Practice && GameSetting.Mode != CommonClass.PlayMode.Replay)
            NotesRecord.Add(new NoteReplay(note.Index, Time.time - PlayNoteScript.LastStart - (float)GameSetting.Config.NoteAdjust / 1000,
                result == BalloonResult.Fail ? HitState.BalloonFail : result == BalloonResult.Perfect ? HitState.BalloonPerfect : HitState.BalloonGood));
#endif
        if (note.Type == 9)
        {
            if (result == BalloonResult.Fail)
                Kusudama[0].Hide();
            else
            {
                bool good = result == BalloonResult.Perfect;
                HammerBroken(result);
                CaculaKusudama(note, good);
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

    protected virtual void HammerBroken(BalloonResult result)
    {
#if !UNITY_ANDROID
        if (GameSetting.Mode != CommonClass.PlayMode.Practice && GameSetting.Mode != CommonClass.PlayMode.Replay)
            NotesRecord.Add(new NoteReplay(InputScript.Notes[InputScript.CurrentBranch][true][0].Index,
                Time.time - PlayNoteScript.LastStart - (float)GameSetting.Config.NoteAdjust / 1000,
                result == BalloonResult.Fail ? HitState.BalloonFail : result == BalloonResult.Perfect ? HitState.BalloonPerfect : HitState.BalloonGood));
#endif
        bool good = result == BalloonResult.Perfect;
        Kusudama[0].ShowResult(good);
        CaculaKusudama(InputScript.Notes[InputScript.CurrentBranch][true][0], good);
    }

    public virtual void BrokenBalloon2P(NoteSoundScript note, BalloonResult result)
    {
        if (note.Type == 9)
        {
            if (result == BalloonResult.Fail)
            {
                //Kusudama[1].Hide();
            }
            else
            {
                bool good = result == BalloonResult.Perfect;
                //Kusudama[1].ShowResult(good);
                CaculaKusudama2P(note, good);
            }
        }
        else
        {
            if (result == BalloonResult.Fail)
            {
                BreakingBalloons[1].Fail((BallonScript)note);
            }
            else
            {
                BreakingBalloons[1].Broken();
                JumpOut[1].Play(note, true);
                CaculaBalloonScore2P(note);
            }
        }
    }

    public void ShowAvatar(bool balloon)
    {
        if (balloon)
            global_hide[0][0] = false;
        else
            global_hide[1][0] = false;

        if (!global_hide[0][0] && !global_hide[1][0] && !balloon)
        {
            Don_Avatar[0].Hide(false, balloon);
            DonImages[0].SetBalloon(false);
        }
    }
    public void ShowAvatar2(bool balloon)
    {
        if (balloon)
            global_hide[0][1] = false;
        else
            global_hide[1][1] = false;

        if (Don_Avatar.Length > 1 && !global_hide[0][1] && !global_hide[1][1] && !balloon)
        {
            Don_Avatar[1].Hide(false, balloon);
            DonImages[1].SetBalloon(false);
        }
    }
    protected void CaculaBalloonScore(NoteSoundScript note)
    {
        int score = 5000;
        if ((Score)GameSetting.Config.ScoreMode == Score.Nijiiro)
            score = 100;
        else if ((Score)GameSetting.Config.ScoreMode != Score.Shin && note.Gogo)
            score = 6000;
        ScoreController[0].SetScore(score);
        TotalScore += score;
        branch_score += score;

        //Debug.Log(TotalScore);
    }

    protected void CaculaBalloonScore2P(NoteSoundScript note)
    {
        int score = 5000;
        if ((Score)GameSetting.Config.ScoreMode == Score.Nijiiro)
            score = 100;
        else if ((Score)GameSetting.Config.ScoreMode != Score.Shin && note.Gogo)
            score = 6000;
        ScoreController[1].SetScore(score);
        TotalScore2P += score;
        branch_score_2p += score;
        //Debug.Log(TotalScore);
    }
    protected void CaculaBalloonHit(NoteSoundScript note)
    {
        int score = 300;
        if ((Score)GameSetting.Config.ScoreMode == Score.Nijiiro)
            score = 100;
        else if ((Score)GameSetting.Config.ScoreMode != Score.Shin && note.Gogo)
            score = 360;
        ScoreController[0].SetScore(score);
        TotalScore += score;
        branch_score += score;

        //Debug.Log(TotalScore);
    }
    protected void CaculaBalloonHit2P(NoteSoundScript note)
    {
        int score = 300;
        if ((Score)GameSetting.Config.ScoreMode == Score.Nijiiro)
            score = 100;
        else if ((Score)GameSetting.Config.ScoreMode != Score.Shin && note.Gogo)
            score = 360;
        ScoreController[1].SetScore(score);
        TotalScore2P += score;
        branch_score_2p += score;

    }

    protected void CaculaKusudama(NoteSoundScript note, bool good)
    {
        int score = good ? 5000 : 1000;
        if ((Score)GameSetting.Config.ScoreMode == Score.Nijiiro)
            score = 100;
        else if (note.Gogo)
            score = (int)(score * 1.2f);

        ScoreController[0].SetScore(score);
        TotalScore += score;
        branch_score += score;

        //Debug.Log(TotalScore);
    }

    protected void CaculaKusudama2P(NoteSoundScript note, bool good)
    {
        int score = good ? 5000 : 1000;
        if ((Score)GameSetting.Config.ScoreMode == Score.Nijiiro)
            score = 100;
        else if (note.Gogo)
            score = (int)(score * 1.2f);

        ScoreController[1].SetScore(score);
        TotalScore2P += score;
        branch_score_2p += score;

    }

    protected virtual void CaculateScore(NoteSoundScript note, HitNoteResult result)
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

    protected virtual void CaculateScore2P(NoteSoundScript note, HitNoteResult result)
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
                points = PlayNoteScript.ShinScorePerNote;
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

    protected virtual void CaculateRapiHit(NoteSoundScript note)
    {
        if ((Score)GameSetting.Config.ScoreMode == Score.Shin)
        {
            int point = note.Type == 5 ? 100 : 200;
            ScoreController[0].SetScore(point);
            branch_score += point;
            TotalScore += point;
        }
        else if ((Score)GameSetting.Config.ScoreMode == Score.Nijiiro)
        {
            ScoreController[0].SetScore(100);
            TotalScore += 100;
        }
        else
        {
            if (LoaderScript.ScoreModeTmp == 2)
            {
                double score = 100;
                if (note.Type == 6)
                {
                    score = 200;
                }
                if (note.Gogo) score *= 1.2;
                int new_p = (int)score / 10;

                TotalScore += new_p * 10;
                ScoreController[0].SetScore(new_p * 10);
                branch_score += new_p * 10;
            }
            else
            {
                double score = 300;
                if (note.Type == 6)
                {
                    score = 360;
                }
                if (note.Gogo) score *= 1.2;
                int new_p = (int)score / 10;

                TotalScore += new_p * 10;
                ScoreController[0].SetScore(new_p * 10);
                branch_score += new_p * 10;
            }
        }

        //Debug.Log(TotalScore);
    }

    protected void CaculateRapiHit2P(NoteSoundScript note)
    {
        if ((Score)GameSetting.Config.ScoreMode == Score.Shin)
        {
            int point = note.Type == 5 ? 100 : 200;
            ScoreController[1].SetScore(point);
            branch_score_2p += point;
            TotalScore2P += point;
        }
        else if ((Score)GameSetting.Config.ScoreMode == Score.Nijiiro)
        {
            ScoreController[1].SetScore(100);
            TotalScore2P += 100;
        }
        else
        {
            if (LoaderScript.ScoreModeTmp == 2)
            {
                double score = 100;
                if (note.Type == 6)
                {
                    score = 200;
                }
                if (note.Gogo) score *= 1.2;
                int new_p = (int)score / 10;

                TotalScore2P += new_p * 10;
                ScoreController[1].SetScore(new_p * 10);
                branch_score_2p += new_p * 10;
            }
            else
            {
                double score = 300;
                if (note.Type == 6)
                {
                    score = 360;
                }
                if (note.Gogo) score *= 1.2;
                int new_p = (int)score / 10;

                TotalScore2P += new_p * 10;
                ScoreController[1].SetScore(new_p * 10);
                branch_score_2p += new_p * 10;
            }
        }
    }
    public virtual void ShowResult(HitNoteResult state, NoteSoundScript note)
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

#if !UNITY_ANDROID
        if (GameSetting.Mode != CommonClass.PlayMode.Practice && GameSetting.Mode != CommonClass.PlayMode.Replay)
        {
            switch (state)
            {
                case HitNoteResult.Bad:
                    NotesRecord.Add(new NoteReplay(note.Index, Time.time - PlayNoteScript.LastStart - (float)GameSetting.Config.NoteAdjust / 1000, HitState.Bad));
                    break;
                case HitNoteResult.Perfect:
                    NotesRecord.Add(new NoteReplay(note.Index, Time.time - PlayNoteScript.LastStart - (float)GameSetting.Config.NoteAdjust / 1000, HitState.Perfect));
                    break;
                case HitNoteResult.Good:
                    NotesRecord.Add(new NoteReplay(note.Index, Time.time - PlayNoteScript.LastStart - (float)GameSetting.Config.NoteAdjust / 1000, HitState.Good));
                    break;
            }
        }
#endif
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

            //Debug.Log(TotalScore);
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

    public virtual void ShowResult2P(HitNoteResult state, NoteSoundScript note)
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

    protected virtual void CountPoints(HitNoteResult state, NoteSoundScript note)
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

    protected bool clear;
    protected bool max;
    protected bool clear_2p;
    protected bool max_2p;
    protected bool gogo;
    protected bool gogo_2p;
    public void SetClearIn(bool clear)
    {
        if (this.clear != clear)
        {
            this.clear = clear;
            Don_Avatar[0].SetClear(clear);
        }
    }

    public void SetClearIn2P(bool clear)
    {
        if (this.clear_2p != clear)
        {
            this.clear_2p = clear;
            Don_Avatar[1].SetClear(clear);
        }
    }

    public void SetMaxIn(bool max)
    {
        if (this.max != max)
        {
            this.max = max;
            Don_Avatar[0].Max(max);
            DonImages[0].SetMax(max);
        }
    }

    public void SetMaxIn2P(bool max)
    {
        if (this.max_2p != max)
        {
            this.max_2p = max;
            Don_Avatar[1].Max(max);
            DonImages[1].SetMax(max);
        }
    }

    public virtual void SetGoGo(bool go)
    {
        if (this.gogo != go)
        {
            this.gogo = go;
            foreach (DonScript don in Don_Avatar)
                don.Gogo(go);

            if (go)
            {
                foreach (Animator fire in GoGoFire)
                {
                    fire.gameObject.layer = shown_layer;
                    fire.enabled = true;
                    fire.SetTrigger("Fire");
                }
            }
            else
            {
                foreach (Animator fire in GoGoFire)
                {
                    fire.gameObject.layer = hide_layer;
                    fire.enabled = false;
                }
            }

            foreach (GameObject bg in GoGOTimeBG)
                bg.SetActive(go);

            if (go && GogoSplash != null) GogoSplash.ShowSplash();
        }
    }

    public virtual void CountResult(NoteSoundScript note)
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

#if !UNITY_ANDROID
            if (GameSetting.Mode != CommonClass.PlayMode.Practice && GameSetting.Mode != CommonClass.PlayMode.Replay)
                NotesRecord.Add(new NoteReplay(note.Index, Time.time - PlayNoteScript.LastStart - (float)GameSetting.Config.NoteAdjust / 1000, HitState.Miss));
#endif
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

    public virtual void CountResult2P(NoteSoundScript note)
    {
        if (note.Type < 5)
        {
            SetNoteCombo2P(NoteSoundScript.ComboType.Combo_None);

            ComboCount2P = 0;
            CountPoints2P(HitNoteResult.Bad, note);

            if (Runner != null)
                Runner.OnPlay(false, false);

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

    protected virtual void QuitGame()
    {
        PlayCancelSound();
        foreach (BreakingBalloon balloon in BreakingBalloons)
            balloon.gameObject.SetActive(false);

        StopAllCoroutines();
        QuitMask.SetActive(true);
        StartCoroutine(Quit());
    }
    IEnumerator Quit()
    {
        yield return new WaitForSecondsRealtime(1.5f);
        string scene = GameSetting.Style == GameStyle.NijiiroStyle ? "SongSelectVertical" : "SongSelect"; ;
#if UNITY_ANDROID
        scene = "AndroidSelectVertical";
        //scene = "AndroidSongSelect";
#endif
        Time.timeScale = 1;
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
    }

    protected void PlayCancelSound()
    {
        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Cancel);

    }

    public virtual void Restart(int from_chapter)
    {
        replay = false;
        if (GameSetting.Replay == null && NotesRecord != null) NotesRecord.Clear();
        Input.Restart();
        StopAllCoroutines();
        StartCoroutine(Mask(from_chapter));
    }

    protected void QuitGameByMiss()
    {
        InputScript.Holding = InputScript.HoldStatus.None;
        PlayNote.Hold(true);
        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Miss1p);
        StartCoroutine(CoroutineQuitGame());
    }

    IEnumerator CoroutineQuitGame()
    {
        //yield return new WaitForSeconds(0.05f);
        Time.timeScale = 0;

        yield return new WaitForSecondsRealtime(1);
        QuitGame();
    }

    protected void RestartByMiss()
    {
        InputScript.Holding = InputScript.HoldStatus.None;
        PlayNote.Hold(true);
        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Retry);
        StartCoroutine(CoroutineRestart());
    }
    IEnumerator CoroutineRestart()
    {
        //yield return new WaitForSeconds(0.05f);
        Time.timeScale = 0;

        yield return new WaitForSecondsRealtime(1.5f);
        Restart(0);
    }

    IEnumerator Mask(int from_chapter)
    {
        RestartMask.gameObject.SetActive(true);
        RestartMask.color = new Color(0, 0, 0, 0);
        while (RestartMask.color.a < 1)
        {
            yield return new WaitForEndOfFrame();
            RestartMask.color += new Color(0, 0, 0, 0.1f);
        }
        PlayNote.RestartPrepare(from_chapter);
        if (Practice != null)
        {
            Practice.SetChapter(from_chapter);
            Practice.ResetResult();
        }

        yield return new WaitForSeconds(1);
        while (RestartMask.color.a > 0)
        {
            yield return new WaitForEndOfFrame();
            RestartMask.color -= new Color(0, 0, 0, 0.05f);
        }
        RestartMask.gameObject.SetActive(false);
        SendRestart();
    }

    protected virtual void SendRestart()
    {
        Input.TimeReset();
        PlayNote.Restart();
    }

    protected void SetNoteCombo(NoteSoundScript.ComboType combo)
    {
        Tick[0].SetCombo(combo);
    }

    protected void SetNoteCombo2P(NoteSoundScript.ComboType combo)
    {
        Tick[1].SetCombo(combo);
    }

    protected void SetAutoMark()
    {
        Marks[2].SetActive(GameSetting.Mode != CommonClass.PlayMode.Replay && GameSetting.Config.Special == Special.AutoPlay);
    }
}
