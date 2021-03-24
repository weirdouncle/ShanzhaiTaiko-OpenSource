using CommonClass;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

public delegate void PlayDelegate();
public class LoaderScript : MonoBehaviour
{
    public static event PlayDelegate Play;

    public GameObject NormalDon_pre;
    public GameObject NormalKa_pre;
    public GameObject BigDon_pre;
    public GameObject BigKa_pre;
    public GameObject Rapid_pre;
    public GameObject BigRapid_pre;
    public GameObject Ballon_pre;
    public GameObject Kusudama_pre;
    public GameObject Hammer_pre;
    public GameObject ChapterLine_pre;

    public Transform NoteParent;
    public Transform Note2PParent;
    public Transform ChapterLineParent;
    public Transform ChapterLine2PParent;
    public NotesSplitScript NoteSplit;
    public KusudamaDonControll Kusudama;

    public double BASEBPM { set; get; }
    public double BPM { set; get; }
    public bool[] bHasBranch { set; get; } = new bool[(int)Difficulty.Total] { false, false, false, false, false, false, false };
    public bool[] MapExist { set; get; } = new bool[(int)Difficulty.Total];
    public static bool[,] b配点が指定されている { set; get; } = new bool[3, (int)Difficulty.Total]; //2017.06.04 kairera0467 [ x, y ] x=通常(Init)or真打orDiff y=コース
    public int[] LEVELtaiko { set; get; } = new int[(int)Difficulty.Total] { -1, -1, -1, -1, -1, -1, -1 };
    public Dictionary<int, CDELAY> listDELAY { set; get; } = new Dictionary<int, CDELAY>();
    public static Dictionary<int, CBRANCH> ListBRANCH { set; get; } = new Dictionary<int, CBRANCH>();
    public double AppearTime { set; get; }
    public double WaitingTime { set; get; }

    public float fNow_Measure_s { set; get; } = 4.0f;
    public float fNow_Measure_m { set; get; } = 4.0f;
    public float dbNowTime { set; get; } = 0.0f;
    public float dbNowBMScollTime { set; get; } = 0.0f;
    public float dbNowScroll { set; get; } = 1.0f;
    public float dbNowScrollY { set; get; } = 0.0f; //2016.08.13 kairera0467 複素数スクロール
    public float dbLastTime { set; get; } = 0.0f; //直前の小節の開始時間
    public float dbLastBMScrollTime { set; get; } = 0.0f;
    public bool IsEndedBranching { set; get; }  // BRANCHENDが呼び出されたかどうか
    public bool IsEnabledFixSENote { set; get; }
    public int FixSENote { set; get; }
    public bool b小節線を挿入している { set; get; } = false;

    protected float dbBarLength = 1.0f;
    protected int current_line_index = 0;
    protected List<int> chips = new List<int>();
    protected string PATH_WAV;
    protected string ARTIST;
    protected string COMMENT;
    protected string GENRE;
    protected bool HIDDENLEVEL;
    protected string PREVIEW;
    protected string PREIMAGE;
    protected string PANEL;
    protected bool only_header = false;
    protected int current_course_difficulty = 4;
    protected Dictionary<int, List<int>> commonBalloon = new Dictionary<int, List<int>>();
    protected Dictionary<int, List<int>> listBalloon_Normal = new Dictionary<int, List<int>>();
    protected Dictionary<int, List<int>> listBalloon_Expert = new Dictionary<int, List<int>>();
    protected Dictionary<int, List<int>> listBalloon_Master = new Dictionary<int, List<int>>();
    protected string SUBTITLE;
    protected int n内部番号BPM1to;
    protected Dictionary<int, CBPM> listBPM = new Dictionary<int, CBPM>();
    protected Dictionary<int, CSCROLL> listSCROLL = new Dictionary<int, CSCROLL>();
    protected int nOFFSET;
    protected bool offset_minus;
    protected int SongVol;
    protected int n内部番号SCROLL1to;
    protected int n内部番号DELAY1to;
    protected int n内部番号BRANCH1to;
    protected bool bHIDDENBRANCH;
    protected string strBGIMAGE_PATH;
    protected int DemoBgmOffset;
    protected List<CLine> listLine = new List<CLine>();
    protected int nLineCountTemp; //分岐開始時の小節数を記録。
    protected int nLineCountCourseTemp; //現在カウント中のコースを記録。

    protected int n現在の発声時刻;
    protected int n現在の発声時刻ms;
    protected int current_course = 0;

    protected float dbNowBPM = 120;

    protected double[] dbNowSCROLL_Normal;
    protected double[] dbNowSCROLL_Expert;
    protected double[] dbNowSCROLL_Master;
    protected int listBalloon_Normal_数値管理 = 0;
    protected int listBalloon_Expert_数値管理 = 0;
    protected int listBalloon_Master_数値管理 = 0;

    public static List<CChip> Others = new List<CChip>();
    public static Dictionary<int, NoteSoundScript> Notes = new Dictionary<int, NoteSoundScript>();
    public static List<ChapterLineScript> Lines = new List<ChapterLineScript>();
    public static Dictionary<int, NoteSoundScript> Notes2P = new Dictionary<int, NoteSoundScript>();
    public static List<ChapterLineScript> Lines2P = new List<ChapterLineScript>();
    public static int[] ScoreDiff = new int[(int)Difficulty.Total]; //[y]
    public static int[,] ScoreInit = new int[2, (int)Difficulty.Total]; //[ x, y ] x=通常or真打 y=コース
    public static int ScoreModeTmp;

    protected int note_index = 0;
    protected bool ready;
    protected Dictionary<int, NoteChip> notes = new Dictionary<int, NoteChip>();
    protected Config config;
    protected List<ChapterLine> lines = new List<ChapterLine>();
    protected List<ChapterLine> lines_2p = new List<ChapterLine>();

    protected float z_value = 0;
    protected string new_line = Encoding.Default.GetString(new byte[1] { 13 });
    void OnDestroy()
    {
        foreach (NoteSoundScript note in Notes.Values)
            if (note != null && note.gameObject != null)
                Destroy(note.gameObject);
        
        foreach (ChapterLineScript note in Lines)
            if (note != null && note.gameObject != null)
                Destroy(note.gameObject);

        foreach (NoteSoundScript note in Notes2P.Values)
            if (note != null && note.gameObject != null)
                Destroy(note.gameObject);

        foreach (ChapterLineScript note in Lines2P)
            if (note != null && note.gameObject != null)
                Destroy(note.gameObject);

        Others.Clear();
        Notes.Clear();
        Lines.Clear();
        Notes2P.Clear();
        Lines2P.Clear();
        ListBRANCH.Clear();
    }
    void Start()
    {
        config = GameSetting.Config.Copy();
        if (GameSetting.Replay != null)
        {
            ReplayConfig replay_config = GameSetting.Replay.Config[GameSetting.Config.ScoreMode][(int)GameSetting.Difficulty];
            config.CopyReplayConfig(replay_config);
        }

        for (int y = 0; y < (int)Difficulty.Total; y++)
        {
            ScoreInit[0, y] = 300;
            ScoreInit[1, y] = 1000;
            ScoreDiff[y] = 120;
            b配点が指定されている[0, y] = false;
            b配点が指定されている[1, y] = false;
            b配点が指定されている[2, y] = false;
        }
        ScoreModeTmp = 99;

        dbNowScroll = 1.0f;
        dbNowSCROLL_Normal = new double[] { 1.0, 0.0 };
        dbNowSCROLL_Expert = new double[] { 1.0, 0.0 };
        dbNowSCROLL_Master = new double[] { 1.0, 0.0 };
        current_course = 0;

        Thread thread = new Thread(Decode);
        thread.Start();
    }
    

    private void Decode()
    {
        string path = GameSetting.SelectedInfo.Path;
        string str;
        using (var reader = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)))
        {
            str = reader.ReadToEnd();
            reader.Close();
        }

        if (!string.IsNullOrEmpty(str))
        {
            str = str.Replace(Environment.NewLine, "\n");
            str = str.Replace(new_line, "\n");
            str = str.Replace('\t', ' ');
            str += "\n";
            
            this.n内部番号BPM1to = 1;

            CharEnumerator ce = str.GetEnumerator();
            if (ce.MoveNext())
            {
                current_line_index = 1;
                do
                {
                    if (!this.SkipBlankorChangeLine(ref ce))
                    {
                        break;
                    }
                    if (chips.Count == 0)
                    {
                        this.t入力_V4(str);
                    }
                    if (ce.Current == '#')
                    {
                        if (ce.MoveNext())
                        {
                            StringBuilder builder = new StringBuilder(0x20);
                            if (this.AppendCommandString(ref ce, ref builder))
                            {
                                StringBuilder builder2 = new StringBuilder(0x400);
                                if (this.AppendParameterString(ref ce, ref builder2))
                                {
                                    StringBuilder builder3 = new StringBuilder(0x400);
                                    if (this.AppendCommentString(ref ce, ref builder3))
                                    {
                                        DeCode(ref builder, ref builder2, ref builder3);

                                        current_line_index++;
                                        continue;
                                    }
                                }
                            }
                        }
                        break;
                    }
                }
                while (SkipComment(ref ce));

                #region [ BPM/BMP初期化 ]
                CBPM cbpm = null;
                foreach (CBPM cbpm2 in this.listBPM.Values)
                {
                    if (cbpm2.n表記上の番号 == 0)
                    {
                        cbpm = cbpm2;
                        break;
                    }
                }
                if (cbpm == null)
                {
                    cbpm = new CBPM
                    {
                        n内部番号 = this.n内部番号BPM1to++,
                        n表記上の番号 = 0,
                        dbBPM値 = 120
                    };
                    this.listBPM.Add(cbpm.n内部番号, cbpm);

                    CChip chip = new CChip
                    {
                        Type = CChip.CType.BPM_CHANGE,
                        JudgeTime = 0,
                        Bpm = cbpm.dbBPM値,
                        StartChapter = 1
                    };
                    Others.Add(chip);

                    chips.Add(1);
                }

                #endregion
            }
        }
        /*
        foreach (CChip chip in Others)
        {
            foreach (ChapterLineScript line in Lines)
            {
                if (line.Chapter == chip.StartChapter)
                    chip.JudgeTime = (int)line.JudgeTime;
                if (chip.EndChpater == line.Chapter)
                    chip.EndTime = (int)line.JudgeTime;
            }
        }
        */

        if (notes.Count > 0)
        {
            ready = true;
        }
    }

    void LateUpdate()
    {
        if (ready)
        {
            ready = false;
            StartCoroutine(AddNote());
        }
    }

    IEnumerator AddNote()
    {
        bool kusu = false;
        int count = 0;
        WaitForEndOfFrame wait = new WaitForEndOfFrame();

        foreach (ChapterLine line_chip in lines)
        {
            ChapterLineScript duplicated = Lines.Find(t => t.Chapter == line_chip.Chapter);
            if (duplicated == null)
            {
                GameObject line = Instantiate(ChapterLine_pre, ChapterLineParent);
                ChapterLineScript script = line.GetComponent<ChapterLineScript>();
                script.Chapter = line_chip.Chapter;
                script.Bpm = line_chip.Bpm;
                script.Scroll = line_chip.Scroll;
                script.JudgeTime = line_chip.JudgeTime;
                script.AppearTime = line_chip.AppearTime;
                script.MoveTime = line_chip.MoveTime;
                script.WaitingTime = line_chip.WaitingTime;
                script.Adjust = GameSetting.Config.NoteAdjust;
                Lines.Add(script);
            }
        }

        foreach (ChapterLine line_chip in lines_2p)
        {
            ChapterLineScript duplicated = Lines2P.Find(t => t.Chapter == line_chip.Chapter);
            if (duplicated == null)
            {
                GameObject line = Instantiate(ChapterLine_pre, ChapterLine2PParent);
                ChapterLineScript script = line.GetComponent<ChapterLineScript>();
                script.Chapter = line_chip.Chapter;
                script.Bpm = line_chip.Bpm;
                script.Scroll = line_chip.Scroll;

                if (GameSetting.Mode == CommonClass.PlayMode.PlayWithReplay) script.Replay = true;

                script.JudgeTime = line_chip.JudgeTime;
                script.AppearTime = line_chip.AppearTime;
                script.MoveTime = line_chip.MoveTime;
                script.WaitingTime = line_chip.WaitingTime;
                script.Adjust = GameSetting.Config.NoteAdjust;
                Lines2P.Add(script);
            }
        }


        //计算1P随机
        List<NoteChip> p1_notes = new List<NoteChip>();

        if (GameSetting.Mode != CommonClass.PlayMode.Replay && (GameSetting.Config.Random != RandomType.None || GameSetting.Config.Reverse))
        {
            foreach (NoteChip chip in notes.Values)
            {
                NoteChip random = chip.Copy();
                if (random.Type < 5)
                {
                    bool change = false;
                    switch (GameSetting.Config.Random)
                    {
                        case RandomType.None when GameSetting.Config.Reverse:
                            change = true;
                            break;
                        case RandomType.Normal:
                            change = UnityEngine.Random.Range(0, 4) < (GameSetting.Config.Reverse ? 3 : 1);
                            break;
                        case RandomType.More:
                            change = UnityEngine.Random.Range(0, 2) == 0;
                            break;
                    }

                    if (change)
                    {
                        switch (random.Type)
                        {
                            case 1:
                                random.Type = 2;
                                break;
                            case 2:
                                random.Type = 1;
                                break;
                            case 3:
                                random.Type = 4;
                                break;
                            case 4:
                                random.Type = 3;
                                break;
                        }
                    }
                }

                p1_notes.Add(random);
            }
        }
        else if (GameSetting.Mode == CommonClass.PlayMode.Replay)
        {
            foreach (NoteChip chip in notes.Values)
            {
                NoteChip random = chip.Copy();
                int chip_type2 = GameSetting.Replay.Notes[GameSetting.Config.ScoreMode][(int)GameSetting.Difficulty][chip.Index];
                random.Type = chip_type2;
                p1_notes.Add(random);
            }
        }
        else
        {
            p1_notes = new List<NoteChip>(notes.Values);
        }

        //计算1P音符senote
        #region[ seNotes計算 ]
        if (ListBRANCH.Count != 0)
            this.tSetSenotes_branch(p1_notes);
        else
            this.tSetSenotes(p1_notes);
        #endregion

        List<NoteChip> p2_notes = new List<NoteChip>();
        if (GameSetting.Mode == CommonClass.PlayMode.PlayWithReplay)
        {
            foreach (NoteChip chip in notes.Values)
            {
                NoteChip random = chip.Copy();
                int chip_type2 = GameSetting.Replay.Notes[GameSetting.Config.ScoreMode][(int)GameSetting.Difficulty][chip.Index];
                random.Type = chip_type2;
                p2_notes.Add(random);
            }

            //计算2P音符senote
            #region[ seNotes計算 ]
            if (ListBRANCH.Count != 0)
                this.tSetSenotes_branch(p2_notes);
            else
                this.tSetSenotes(p2_notes);
            #endregion
        }


        //生成1p音符
        foreach (NoteChip chip in p1_notes)
        {
            if (count >= 50)
            {
                yield return wait;
                count = 0;
            }

            GameObject sound = null;
            switch (chip.Type)
            {
                case 1:
                    sound = Instantiate(NormalDon_pre, NoteParent);
                    break;
                case 2:
                    sound = Instantiate(NormalKa_pre, NoteParent);
                    break;
                case 3:
                    sound = Instantiate(BigDon_pre, NoteParent);
                    break;
                case 4:
                    sound = Instantiate(BigKa_pre, NoteParent);
                    break;
                case 5:
                    {
                        sound = Instantiate(Rapid_pre, NoteParent);
                        RapidScript rapid = sound.GetComponent<RapidScript>();
                        rapid.EndTime = chip.EndTime;
                    }
                    break;
                case 6:
                    {
                        sound = Instantiate(BigRapid_pre, NoteParent);
                        RapidScript rapid = sound.GetComponent<RapidScript>();
                        rapid.EndTime = chip.EndTime;
                    }
                    break;
                case 7:
                    {
                        sound = Instantiate(Ballon_pre, NoteParent);
                        BallonScript balloon = sound.GetComponent<BallonScript>();
                        balloon.HitCount = chip.HitCount;
                        balloon.EndTime = chip.EndTime;
                    }
                    break;
                case 9:
                    {
                        if ((Score)GameSetting.Config.ScoreMode == Score.Normal)
                        {
                            kusu = true;
                            sound = Instantiate(Hammer_pre, NoteParent);
                            BallonScript balloon = sound.GetComponent<BallonScript>();
                            balloon.HitCount = chip.HitCount;
                            balloon.EndTime = chip.EndTime;
                            if (balloon is HammerScript hammer)
                                hammer.MidTime = chip.MidTime;
                            else if (balloon is KusudamaNoteScript kusudama)
                                kusudama.MidTime = chip.MidTime;
                        }
                        else if ((Score)GameSetting.Config.ScoreMode == Score.Nijiiro)
                        {
                            kusu = true;
                            sound = Instantiate(Kusudama_pre, NoteParent);
                            BallonScript balloon = sound.GetComponent<BallonScript>();
                            balloon.HitCount = chip.HitCount;
                            balloon.EndTime = chip.EndTime;
                        }
                        else
                        {
                            sound = Instantiate(Ballon_pre, NoteParent);
                            BallonScript balloon = sound.GetComponent<BallonScript>();
                            balloon.HitCount = chip.HitCount;
                            balloon.EndTime = chip.EndTime;
                        }
                    }
                    break;
            }

            //sound.transform.SetAsFirstSibling();
            NoteSoundScript script = sound.GetComponent<NoteSoundScript>();
            script.Z_Value = z_value;
            z_value += 0.00001f;
            script.Chapter = chip.Chapter;
            script.Bpm = chip.Bpm;
            script.Steal = GameSetting.Config.Steal;

            float speed = (int)GameSetting.Config.Speed;
            if (speed > 10) speed /= 10;

            script.Scroll = chip.Scroll * speed;
            script.fBMSCROLLTime = chip.fBMSCROLLTime;
            script.JudgeTime = chip.JudgeTime;
            script.AppearTime = chip.AppearTime;
            script.MoveTime = chip.MoveTime;
            script.WaitingTime = chip.WaitingTime;
            script.Gogo = chip.Gogo;
            script.Adjust = GameSetting.Config.NoteAdjust;
            script.Replay = GameSetting.Mode == CommonClass.PlayMode.Replay;

            script.Type = (chip.Type != 9 || (Score)GameSetting.Config.ScoreMode != Score.Shin) ? chip.Type : 7;

            script.BranchRelated = chip.BranchRelated;
            script.Branch = chip.Branch;
            script.IsFixedSENote = chip.IsFixedSENote;
            script.Senote = chip.Senote;

            script.Index = chip.Index;
            Notes.Add(script.Index, script);

            count++;
        }

        if (kusu)
        {
            Kusudama.OnInit();          //确定有小锤后，复制咚模型
        }

        //生成2P音符
        if (GameSetting.Mode == CommonClass.PlayMode.PlayWithReplay)
        {
            z_value = 0;
            foreach (NoteChip chip in p2_notes)
            {
                GameObject sound2 = null;
                //adjust random
                switch (chip.Type)
                {
                    case 1:
                        sound2 = Instantiate(NormalDon_pre, Note2PParent);
                        break;
                    case 2:
                        sound2 = Instantiate(NormalKa_pre, Note2PParent);
                        break;
                    case 3:
                        sound2 = Instantiate(BigDon_pre, Note2PParent);
                        break;
                    case 4:
                        sound2 = Instantiate(BigKa_pre, Note2PParent);
                        break;
                    case 5:
                        {
                            sound2 = Instantiate(Rapid_pre, Note2PParent);
                            RapidScript rapid = sound2.GetComponent<RapidScript>();
                            rapid.EndTime = chip.EndTime;
                        }
                        break;
                    case 6:
                        {
                            sound2 = Instantiate(BigRapid_pre, Note2PParent);
                            RapidScript rapid = sound2.GetComponent<RapidScript>();
                            rapid.EndTime = chip.EndTime;
                        }
                        break;
                    case 7:
                        {
                            sound2 = Instantiate(Ballon_pre, Note2PParent);
                            BallonScript balloon = sound2.GetComponent<BallonScript>();
                            balloon.HitCount = chip.HitCount;
                            balloon.EndTime = chip.EndTime;
                        }
                        break;
                    case 9:
                        {
                            if ((Score)GameSetting.Config.ScoreMode == Score.Normal)
                            {
                                sound2 = Instantiate(Hammer_pre, Note2PParent);
                                BallonScript balloon = sound2.GetComponent<BallonScript>();
                                balloon.HitCount = chip.HitCount;
                                balloon.EndTime = chip.EndTime;
                                if (balloon is HammerScript hammer)
                                    hammer.MidTime = chip.MidTime;
                                else if (balloon is KusudamaNoteScript kusudama)
                                    kusudama.MidTime = chip.MidTime;
                            }
                            else if ((Score)GameSetting.Config.ScoreMode == Score.Nijiiro)
                            {
                                sound2 = Instantiate(Kusudama_pre, Note2PParent);
                                BallonScript balloon = sound2.GetComponent<BallonScript>();
                                balloon.HitCount = chip.HitCount;
                                balloon.EndTime = chip.EndTime;
                            }
                            else
                            {
                                sound2 = Instantiate(Ballon_pre, Note2PParent);
                                BallonScript balloon = sound2.GetComponent<BallonScript>();
                                balloon.HitCount = chip.HitCount;
                                balloon.EndTime = chip.EndTime;
                            }
                        }
                        break;
                }

                //sound.transform.SetAsFirstSibling();
                NoteSoundScript script2 = sound2.GetComponent<NoteSoundScript>();
                script2.Z_Value = z_value;
                z_value += 0.00001f;
                script2.Chapter = chip.Chapter;
                script2.Bpm = chip.Bpm;
                script2.Steal = config.Steal;

                float speed2 = (int)config.Speed;
                if (speed2 > 10) speed2 /= 10;

                script2.Scroll = chip.Scroll * speed2;
                script2.fBMSCROLLTime = chip.fBMSCROLLTime;
                script2.JudgeTime = chip.JudgeTime;
                script2.AppearTime = chip.AppearTime;
                script2.MoveTime = chip.MoveTime;
                script2.WaitingTime = chip.WaitingTime;
                script2.Gogo = chip.Gogo;
                script2.Replay = true;
                script2.Player2 = true;
                script2.Type = (chip.Type != 9 || (Score)GameSetting.Config.ScoreMode == Score.Normal) ? chip.Type : 7;
                script2.Adjust = GameSetting.Config.NoteAdjust;

                script2.BranchRelated = chip.BranchRelated;
                script2.Branch = chip.Branch;
                script2.IsFixedSENote = chip.IsFixedSENote;
                script2.Senote = chip.Senote;

                script2.Index = chip.Index;
                Notes2P.Add(script2.Index, script2);
            }
        }


        //设置小节线显示
        if (GameSetting.Mode != CommonClass.PlayMode.Practice)
        {
            List<CChip> remove = new List<CChip>();
            foreach (CChip chip in Others)
            {
                if (chip.Type == CChip.CType.CHAPTERLINE_OFF)
                {
                    foreach (ChapterLineScript script in LoaderScript.Lines)
                    {
                        if (script.Chapter >= chip.StartChapter && script.Chapter < chip.EndChpater)
                            script.Show = false;
                    }
                    foreach (ChapterLineScript script in LoaderScript.Lines2P)
                    {
                        if (script.Chapter >= chip.StartChapter && script.Chapter < chip.EndChpater)
                            script.Show = false;
                    }
                    remove.Add(chip);
                }
            }
            Others.RemoveAll(t => remove.Contains(t));
        }

        NoteSplit.StartPlay();
        foreach (NoteSoundScript script in LoaderScript.Notes.Values)
        {
            script.Prepare();
        }
        foreach (ChapterLineScript script in LoaderScript.Lines)
        {
            script.Prepare();
        }
        foreach (NoteSoundScript script in LoaderScript.Notes2P.Values)
        {
            script.Prepare();
        }
        foreach (ChapterLineScript script in LoaderScript.Lines2P)
        {
            script.Prepare();
        }
        InvokePlay();
    }

    protected void InvokePlay()
    {
        Play?.Invoke();
    }

    private bool SkipBlankorChangeLine(ref CharEnumerator ce)
    {
        // 空白と改行が続く間はこれらをスキップする。

        while (ce.Current == ' ' || ce.Current == '\n')
        {
            if (ce.Current == '\n')
                current_line_index++;      // 改行文字では行番号が増える。

            if (!ce.MoveNext())
                return false;   // 文字が尽きた
        }

        return true;
    }

    //添加输入字符串中的命令相关的字符串
    private bool AppendCommandString(ref CharEnumerator ce, ref StringBuilder string_builder)
    {
        if (!SkipBlankorChangeLine(ref ce))
            return false;   // 文字が尽きた

        #region [ コマンド終端文字(':')、半角空白、コメント開始文字(';')、改行のいずれかが出現するまでをコマンド文字列と見なし、sb文字列 にコピーする。]
        //-----------------
        while (ce.Current != ':' && ce.Current != ' ' && ce.Current != ';' && ce.Current != '\n')
        {
            string_builder.Append(ce.Current);

            if (!ce.MoveNext())
                return false;   // 文字が尽きた
        }
        //-----------------
        #endregion

        #region [ コマンド終端文字(':')で終端したなら、その次から空白をスキップしておく。]
        //-----------------
        if (ce.Current == ':')
        {
            if (!ce.MoveNext())
                return false;   // 文字が尽きた

            if (!SkipBlank(ref ce))
                return false;   // 文字が尽きた
        }
        //-----------------
        #endregion

        return true;
    }

    private bool SkipBlank(ref CharEnumerator ce)
    {
        // 空白が続く間はこれをスキップする。

        while (ce.Current == ' ')
        {
            if (!ce.MoveNext())
                return false;   // 文字が尽きた
        }

        return true;
    }

    private bool AppendCommentString(ref CharEnumerator ce, ref StringBuilder string_builder)
    {
        if (ce.Current != ';')      // コメント開始文字(';')じゃなければ正常帰還。
            return true;

        if (!ce.MoveNext())     // ';' の次で文字列が終わってたら終了帰還。
            return false;

        #region [ ';' の次の文字から '\n' の１つ前までをコメント文字列と見なし、sb文字列にコピーする。]
        //-----------------
        while (ce.Current != '\n')
        {
            string_builder.Append(ce.Current);

            if (!ce.MoveNext())
                return false;
        }
        //-----------------
        #endregion

        return true;
    }

    private bool AppendParameterString(ref CharEnumerator ce, ref StringBuilder string_builder)
    {
        if (!this.SkipBlank(ref ce))
            return false;   // 文字が尽きた

        #region [ 改行またはコメント開始文字(';')が出現するまでをパラメータ文字列と見なし、sb文字列 にコピーする。]
        //-----------------
        while (ce.Current != '\n' && ce.Current != ';')
        {
            string_builder.Append(ce.Current);

            if (!ce.MoveNext())
                return false;
        }
        //-----------------
        #endregion

        return true;
    }

    private bool SkipComment(ref CharEnumerator ce)
    {
        // 改行が現れるまでをコメントと見なしてスキップする。
        while (ce.Current != '\n')
        {
            if (!ce.MoveNext())
                return false;   // 文字が尽きた
        }

        // 改行の次の文字へ移動した結果を返す。

        return ce.MoveNext();
    }




    private void DeCode(ref StringBuilder command_build, ref StringBuilder paramater_build, ref StringBuilder comment)
    {
        string command_str = command_build.ToString();
        string parameter_str = paramater_build.ToString().Trim();
        string comment_str = comment.ToString();

        // 行頭コマンドの処理

            #region [ PATH_WAV ]
            //-----------------
            if (command_str.StartsWith("PATH_WAV", StringComparison.OrdinalIgnoreCase))
            {
                this.t入力_パラメータ食い込みチェック("PATH_WAV", ref command_str, ref parameter_str);
                PATH_WAV = parameter_str;
            }
            //-----------------
            #endregion
            #region [ TITLE ]
            //-----------------
            else if (command_str.StartsWith("TITLE", StringComparison.OrdinalIgnoreCase))
            {
                //this.t入力_パラメータ食い込みチェック( "TITLE", ref strコマンド, ref strパラメータ );
                //this.TITLE = strパラメータ;
            }
            //-----------------
            #endregion
            #region [ ARTIST ]
            //-----------------
            else if (command_str.StartsWith("ARTIST", StringComparison.OrdinalIgnoreCase))
            {
                this.t入力_パラメータ食い込みチェック("ARTIST", ref command_str, ref parameter_str);
                this.ARTIST = parameter_str;
            }
            //-----------------
            #endregion
            #region [ COMMENT ]
            //-----------------
            else if (command_str.StartsWith("COMMENT", StringComparison.OrdinalIgnoreCase))
            {
                this.t入力_パラメータ食い込みチェック("COMMENT", ref command_str, ref parameter_str);
                this.COMMENT = parameter_str;
            }
            //-----------------
            #endregion
            #region [ GENRE ]
            //-----------------
            else if (command_str.StartsWith("GENRE", StringComparison.OrdinalIgnoreCase))
            {
                this.t入力_パラメータ食い込みチェック("GENRE", ref command_str, ref parameter_str);
                this.GENRE = parameter_str;
            }
            //-----------------
            #endregion
            #region [ HIDDENLEVEL ]
            //-----------------
            else if (command_str.StartsWith("HIDDENLEVEL", StringComparison.OrdinalIgnoreCase))
            {
                this.t入力_パラメータ食い込みチェック("HIDDENLEVEL", ref command_str, ref parameter_str);
                this.HIDDENLEVEL = parameter_str.ToLower().Equals("on");
            }
            //-----------------
            #endregion
            #region [ PREVIEW ]
            //-----------------
            else if (command_str.StartsWith("PREVIEW", StringComparison.OrdinalIgnoreCase))
            {
                this.t入力_パラメータ食い込みチェック("PREVIEW", ref command_str, ref parameter_str);
                this.PREVIEW = parameter_str;
            }
            //-----------------
            #endregion
            #region [ PREIMAGE ]
            //-----------------
            else if (command_str.StartsWith("PREIMAGE", StringComparison.OrdinalIgnoreCase))
            {
                this.t入力_パラメータ食い込みチェック("PREIMAGE", ref command_str, ref parameter_str);
                this.PREIMAGE = parameter_str;
            }
            //-----------------
            #endregion
            #region [ BPM ]
            //-----------------
            else if (command_str.StartsWith("BPM", StringComparison.OrdinalIgnoreCase))
            {
                //this.t入力_行解析_BPM_BPMzz( strコマンド, strパラメータ, strコメント );
            }
            //-----------------
            #endregion
            #region [ DTXVPLAYSPEED ]
            /*
            //-----------------
            else if (command_str.StartsWith("DTXVPLAYSPEED", StringComparison.OrdinalIgnoreCase))
            {
                this.t入力_パラメータ食い込みチェック("DTXVPLAYSPEED", ref command_str, ref parameter_str);

                double dtxvplayspeed = 0.0;
                if (TryParse(parameter_str, out dtxvplayspeed) && dtxvplayspeed > 0.0)
                {
                    this.dbDTXVPlaySpeed = dtxvplayspeed;
                }
            }
            */
            //-----------------
            #endregion
            else if (!only_header)      // ヘッダのみの解析の場合、以下は無視。
            {
                #region [ PANEL ]
                //-----------------
                if (command_str.StartsWith("PANEL", StringComparison.OrdinalIgnoreCase))
                {
                    this.t入力_パラメータ食い込みチェック("PANEL", ref command_str, ref parameter_str);

                    int dummyResult;                                // #23885 2010.12.12 yyagi: not to confuse "#PANEL strings (panel)" and "#PANEL int (panpot of EL)"
                    if (!int.TryParse(parameter_str, out dummyResult))
                    {       // 数値じゃないならPANELとみなす
                        this.PANEL = parameter_str;                          //
                        goto EOL;                                   //
                    }                                               // 数値ならPAN ELとみなす

                }
                //-----------------
                #endregion
                #region [ BASEBPM ]
                //-----------------
                else if (command_str.StartsWith("BASEBPM", StringComparison.OrdinalIgnoreCase))
                {
                    this.t入力_パラメータ食い込みチェック("BASEBPM", ref command_str, ref parameter_str);

                    double basebpm = 0.0;
                    if (TryParse(parameter_str, out basebpm) && basebpm > 0.0)   // #23880 2010.12.30 yyagi: alternative TryParse to permit both '.' and ',' for decimal point
                    {                                                   // #24204 2011.01.21 yyagi: Fix the condition correctly
                        this.BASEBPM = basebpm;
                    }
                }
                //-----------------
                #endregion

                // オブジェクト記述コマンドの処理。

                else
                {
                    this.t入力_行解析_チップ配置(command_str, parameter_str, comment_str);
                }
                EOL:
                Debug.Assert(true);     // #23885 2010.12.12 yyagi: dummy line to exit parsing the line
                                        // 2011.8.17 from: "int xx=0;" から変更。毎回警告が出るので。
            }
    }

    private void t入力_パラメータ食い込みチェック(string command_str, ref string command, ref string parameter)
    {
        if ((command.Length > command_str.Length) && command.StartsWith(command_str, StringComparison.OrdinalIgnoreCase))
        {
            parameter = command.Substring(command_str.Length).Trim();
            command = command.Substring(0, command_str.Length);
        }
    }

    private bool t入力_行解析_チップ配置(string command_str, string parameter_str, string comment_str)
    {
        // (1) コマンドを処理。

        if (command_str.Length != 5)    // コマンドは必ず5文字であること。
            return false;

        #region [ n小節番号 を取得する。]
        //-----------------
        int n小節番号 = CharConverter.n小節番号の文字列3桁を数値に変換して返す(command_str.Substring(0, 3));
        if (n小節番号 < 0)
            return false;

        n小節番号++;    // 先頭に空の1小節を設ける。
                    //-----------------
        #endregion

        #region [ nチャンネル番号 を取得する。]
        //-----------------
        int nチャンネル番号 = -1;

        // ファイルフォーマットによって処理が異なる。
        #region [ (B) その他の場合：チャンネル番号は16進数2桁。]
        //-----------------
        nチャンネル番号 = CharConverter.n16進数2桁の文字列を数値に変換して返す(command_str.Substring(3, 2));
        if (nチャンネル番号 < 0)
            return false;
        //-----------------
        #endregion
        #endregion

        // (2) Ch.02を処理。

        #region [ 小節長変更(Ch.02)は他のチャンネルとはパラメータが特殊なので、先にとっとと終わらせる。 ]
        //-----------------
        if (nチャンネル番号 == 0x02)
        {
            // 小節長倍率を取得する。

            double db小節長倍率 = 1.0;
            //if( !double.TryParse( strパラメータ, out result ) )
            if (!this.TryParse(parameter_str, out db小節長倍率))          // #23880 2010.12.30 yyagi: alternative TryParse to permit both '.' and ',' for decimal point
            {
                Debug.Log(string.Format("小節長倍率に不正な値を指定しました。[{0}: {1}行]", string.Empty, this.current_line_index));
                return false;
            }

            // 小節長倍率チップを配置する。
            /*
            this.listChip.Insert(
                0,
                new CChip()
                {
                    nチャンネル番号 = nチャンネル番号,
                    db実数値 = db小節長倍率,
                    n発声位置 = n小節番号 * 384,
                });
                */

            chips.Add(1);
            return true;    // 配置終了。
        }
        //-----------------
        #endregion


        // (3) パラメータを処理。

        if (string.IsNullOrEmpty(parameter_str))     // パラメータはnullまたは空文字列ではないこと。
            return false;

        #region [ strパラメータ にオブジェクト記述を格納し、その n文字数 をカウントする。]
        //-----------------
        int n文字数 = 0;

        var sb = new StringBuilder(parameter_str.Length);

        // strパラメータを先頭から1文字ずつ見ながら正規化（無効文字('_')を飛ばしたり不正な文字でエラーを出したり）し、sb へ格納する。

        CharEnumerator ce = parameter_str.GetEnumerator();
        while (ce.MoveNext())
        {
            if (ce.Current == '_')      // '_' は無視。
                continue;

            if (CharConverter.str36進数文字.IndexOf(ce.Current) < 0)  // オブジェクト記述は36進数文字であること。
            {
                Debug.Log(string.Format("不正なオブジェクト指定があります。[{0}: {1}行]", string.Empty, this.current_line_index));
                return false;
            }

            sb.Append(ce.Current);
            n文字数++;
        }

        parameter_str = sb.ToString();   // 正規化された文字列になりました。

        if ((n文字数 % 2) != 0)        // パラメータの文字数が奇数の場合、最後の1文字を無視する。
            n文字数--;
        //-----------------
        #endregion


        // (4) パラメータをオブジェクト数値に分解して配置する。

        for (int i = 0; i < (n文字数 / 2); i++)    // 2文字で1オブジェクト数値
        {
            #region [ nオブジェクト数値 を１つ取得する。'00' なら無視。]
            //-----------------
            int nオブジェクト数値 = 0;

            if (nチャンネル番号 == 0x03)
            {
                // Ch.03 のみ 16進数2桁。
                nオブジェクト数値 = CharConverter.n16進数2桁の文字列を数値に変換して返す(parameter_str.Substring(i * 2, 2));
            }
            else
            {
                // その他のチャンネルは36進数2桁。
                nオブジェクト数値 = CharConverter.n36進数2桁の文字列を数値に変換して返す(parameter_str.Substring(i * 2, 2));
            }

            if (nオブジェクト数値 == 0x00)
                continue;
            //-----------------
            #endregion

            // オブジェクト数値に対応するチップを生成。
            /*
            var chip = new CChip();

            chip.nチャンネル番号 = nチャンネル番号;
            chip.n発声位置 = (n小節番号 * 384) + ((384 * i) / (n文字数 / 2));
            chip.n整数値 = nオブジェクト数値;
            chip.n整数値_内部番号 = nオブジェクト数値;

            #region [ 無限定義への対応 → 内部番号の取得。]
            //-----------------
            if (chip.bWAVを使うチャンネルである)
            {
                chip.n整数値_内部番号 = this.n無限管理WAV[nオブジェクト数値];  // これが本当に一意なWAV番号となる。（無限定義の場合、chip.n整数値 は一意である保証がない。）
            }
            else if (chip.bBPMチップである)
            {
                chip.n整数値_内部番号 = this.n無限管理BPM[nオブジェクト数値];  // これが本当に一意なBPM番号となる。（同上。）
            }
            //-----------------
            #endregion

            #region [ フィルインON/OFFチャンネル(Ch.53)の場合、発声位置を少し前後にずらす。]
            //-----------------
            if (nチャンネル番号 == 0x53)
            {
                // ずらすのは、フィルインONチップと同じ位置にいるチップでも確実にフィルインが発動し、
                // 同様に、フィルインOFFチップと同じ位置にいるチップでも確実にフィルインが終了するようにするため。

                if ((nオブジェクト数値 > 0) && (nオブジェクト数値 != 2))
                {
                    chip.n発声位置 -= 32;   // 384÷32＝12 ということで、フィルインONチップは12分音符ほど前へ移動。
                }
                else if (nオブジェクト数値 == 2)
                {
                    chip.n発声位置 += 32;   // 同じく、フィルインOFFチップは12分音符ほど後ろへ移動。
                }
            }
            //-----------------
            #endregion

            // チップを配置。

            this.listChip.Add(chip);
            */

            chips.Add(1);
        }
        return true;
    }

    private bool TryParse(string s, out double result)
    {   // #23880 2010.12.30 yyagi: alternative TryParse to permit both '.' and ',' for decimal point
        // EU諸国での #BPM 123,45 のような記述に対応するため、
        // 小数点の最終位置を検出して、それをlocaleにあった
        // 文字に置き換えてからTryParse()する
        // 桁区切りの文字はスキップする

        const string DecimalSeparators = ".,";              // 小数点文字
        const string GroupSeparators = ".,' ";              // 桁区切り文字
        const string NumberSymbols = "0123456789";          // 数値文字

        int len = s.Length;                                 // 文字列長
        int decimalPosition = len;                          // 真の小数点の位置 最初は文字列終端位置に仮置きする

        for (int i = 0; i < len; i++)
        {                           // まず、真の小数点(一番最後に現れる小数点)の位置を求める
            char c = s[i];
            if (NumberSymbols.IndexOf(c) >= 0)
            {               // 数値だったらスキップ
                continue;
            }
            else if (DecimalSeparators.IndexOf(c) >= 0)
            {       // 小数点文字だったら、その都度位置を上書き記憶
                decimalPosition = i;
            }
            else if (GroupSeparators.IndexOf(c) >= 0)
            {       // 桁区切り文字の場合もスキップ
                continue;
            }
            else
            {                                           // 数値_小数点_区切り文字以外がきたらループ終了
                break;
            }
        }

        StringBuilder decimalStr = new StringBuilder(16);
        for (int i = 0; i < len; i++)
        {                           // 次に、localeにあった数値文字列を生成する
            char c = s[i];
            if (NumberSymbols.IndexOf(c) >= 0)
            {               // 数値だったら
                decimalStr.Append(c);                           // そのままコピー
            }
            else if (DecimalSeparators.IndexOf(c) >= 0)
            {       // 小数点文字だったら
                if (i == decimalPosition)
                {                       // 最後に出現した小数点文字なら、localeに合った小数点を出力する
                    decimalStr.Append(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                }
            }
            else if (GroupSeparators.IndexOf(c) >= 0)
            {       // 桁区切り文字だったら
                continue;                                       // 何もしない(スキップ)
            }
            else
            {
                break;
            }
        }
        return double.TryParse(decimalStr.ToString(), out result);  // 最後に、自分のlocale向けの文字列に対してTryParse実行
    }

    private static readonly Regex regexForPrefixingCommaStartingLinesWithZero = new Regex(@"^,", RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex regexForStrippingHeadingLines = new Regex(
        @"^(?!(TITLE|LEVEL|BPM|WAVE|OFFSET|BALLOON|EXAM1|EXAM2|EXAM3|BALLOONNOR|BALLOONEXP|BALLOONMAS|SONGVOL|SEVOL|SCOREINIT|SCOREDIFF|COURSE|STYLE|GAME|LIFE|DEMOSTART|SIDE|SUBTITLE|SCOREMODE|GENRE|MOVIEOFFSET|BGIMAGE|BGMOVIE|STAGE|HIDDENBRANCH|GAUGEINCR|#HBSCROLL|#BMSCROLL)).+\n",
        RegexOptions.Multiline | RegexOptions.Compiled);
    private void t入力_V4(string strInput)
    {
        if (!string.IsNullOrEmpty(strInput)) //空なら通さない
        {

            //2017.01.31 DD カンマのみの行を0,に置き換え
            strInput = regexForPrefixingCommaStartingLinesWithZero.Replace(strInput, "0,");

            //2017.02.03 DD ヘッダ内にある命令以外の文字列を削除
            var startIndex = strInput.IndexOf("#START");
            if (startIndex < 0)
            {
                Debug.Log($"#START命令が少なくとも1つは必要です。");
            }
            string strInputHeader = strInput.Remove(startIndex);
            strInput = strInput.Remove(0, startIndex);
            strInputHeader = regexForStrippingHeadingLines.Replace(strInputHeader, "");
            strInput = strInputHeader + "\n" + strInput;

            //どうせ使わないので先にSplitしてコメントを削除。
            var strSplitした譜面 = (string[])this.RemoveChangeLine(strInput, 1);
            for (int i = 0; strSplitした譜面.Length > i; i++)
            {
                strSplitした譜面[i] = this.RemoveComment(strSplitした譜面[i]);
            }
            //空のstring配列を詰める
            strSplitした譜面 = this.t空のstring配列を詰めたstring配列を返す(strSplitした譜面);

            #region[ヘッダ]

            //2015.05.21 kairera0467
            //ヘッダの読み込みは譜面全体から該当する命令を探す。
            //少し処理が遅くなる可能性はあるが、ここは正確性を重視する。
            //点数などの指定は後から各コースで行うので問題は無いだろう。

            //SplitしたヘッダのLengthの回数だけ、forで回して各種情報を読み取っていく。
            
            for (int i = 0; strSplitした譜面.Length > i; i++)
            {
                this.t入力_行解析ヘッダ(strSplitした譜面[i]);
            }
            #endregion

            #region[譜面]

            LoadCourse = 3;
            int n譜面数 = 0; //2017.07.22 kairera0467 tjaに含まれる譜面の数

            //まずはコースごとに譜面を分割。
            strSplitした譜面 = this.SplitByCourse(this.StringArrayToString(strSplitした譜面, "\n"));
            //存在するかのフラグ作成。
            for (int i = 0; i < strSplitした譜面.Length; i++)
            {
                if (!string.IsNullOrEmpty(strSplitした譜面[i]))
                {
                    this.MapExist[i] = true;
                    n譜面数++;
                }
                else
                    this.MapExist[i] = false;
            }
            #region[ 読み込ませるコースを決定 ]

            if (this.MapExist[(int)GameSetting.Difficulty] == false)                          //谱面难度确定
            {
                LoadCourse = (int)GameSetting.Difficulty;
                LoadCourse++;
                for (int n = 1; n < (int)Difficulty.Total; n++)
                {
                    if (this.MapExist[LoadCourse] == false)
                    {
                        LoadCourse++;
                        if (LoadCourse > (int)Difficulty.Total - 1)
                            LoadCourse = 0;
                    }
                    else
                        break;
                }
            }
            else
                LoadCourse = (int)GameSetting.Difficulty;
            #endregion

            //指定したコースの譜面の命令を消去する。
            strSplitした譜面[LoadCourse] = CDTXStyleExtractor.tセッション譜面がある(
                strSplitした譜面[LoadCourse], 0, string.Empty);

            //命令をすべて消去した譜面
            var str命令消去譜面 = strSplitした譜面[LoadCourse].Split(this.dlmtEnter, StringSplitOptions.RemoveEmptyEntries);
            str命令消去譜面 = this.tコマンド行を削除したTJAを返す(str命令消去譜面, 2);

            //ここで1行の文字数をカウント。配列にして返す。
            var strSplit読み込むコース = strSplitした譜面[LoadCourse].Split(this.dlmtEnter, StringSplitOptions.RemoveEmptyEntries);
            string str = "";
            try
            {
                if (n譜面数 > 0)
                {
                    //2017.07.22 kairera0467 譜面が2つ以上ある場合はCOURSE以下のBALLOON命令を使う
                    this.listBalloon_Normal.Clear();
                    this.listBalloon_Expert.Clear();
                    this.listBalloon_Master.Clear();
                }

                for (int i = 0; i < strSplit読み込むコース.Length; i++)
                {
                    if (!string.IsNullOrEmpty(strSplit読み込むコース[i]))
                    {
                        this.t難易度別ヘッダ(strSplit読み込むコース[i]);
                    }
                }
                for (int i = 0; i < str命令消去譜面.Length; i++)
                {
                    if (str命令消去譜面[i].IndexOf(',', 0) == -1 && !String.IsNullOrEmpty(str命令消去譜面[i]))
                    {
                        if (str命令消去譜面[i].Substring(0, 1) == "#")
                        {
                            this.t1小節の文字数をカウントしてリストに追加する(str + str命令消去譜面[i]);
                        }

                        if (this.CharConvertNote(str命令消去譜面[i].Substring(0, 1)) != -1)
                            str += str命令消去譜面[i];
                    }
                    else
                    {
                        this.t1小節の文字数をカウントしてリストに追加する(str + str命令消去譜面[i]);
                        str = "";
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.ToString());
            }

            //読み込み部分本体に渡す譜面を作成。
            //0:ヘッダー情報 1:#START以降 となる。個数の定義は後からされるため、ここでは省略。
            var strSplitした後の譜面 = strSplit読み込むコース; //strSplitした譜面[ n読み込むコース ].Split( this.dlmtEnter, StringSplitOptions.RemoveEmptyEntries );
            strSplitした後の譜面 = this.tコマンド行を削除したTJAを返す(strSplitした後の譜面, 1);
            //string str命令消去譜面temp = this.StringArrayToString( this.str命令消去譜面 );
            //string[] strDelimiter = { "," };
            //this.str命令消去譜面 = str命令消去譜面temp.Split( strDelimiter, StringSplitOptions.RemoveEmptyEntries );

            //if( bLog && stream != null )
            //{
            //    stream.WriteLine( "-------------------------------------------------" );
            //    stream.WriteLine( ">>this.str命令消去譜面 (Splitした後)" );
            //    for( int i = 0; i < this.str命令消去譜面.Length; i++ )
            //    {
            //        stream.WriteLine( this.str命令消去譜面[ i ] );
            //    }
            //    stream.WriteLine( "-------------------------------------------------" );
            //}

            this.current_chapter_index = 1;
            #region[ 最初の処理 ]
            //1小節の時間を挿入して開始時間を調節。
            this.dbNowTime += ((15000 / 120.0f * (4.0f / 4.0f)) * 16.0f);
            //this.dbNowBMScollTime += (( this.dbBarLength ) * 16.0 );
            #endregion
            //string strWrite = "";
            for (int i = 0; strSplitした後の譜面.Length > i; i++)
            {
                str = strSplitした後の譜面[i];
                //strWrite += str;
                //if( !str.StartsWith( "#" ) && !string.IsNullOrEmpty( this.strTemp ) )
                //{
                //    str = this.strTemp + str;
                //}
                MapDeCode(str);
            }
            #endregion
        }
    }

    protected string[] dlmtSpace = { " " };
    protected string[] dlmtEnter = { "\n" };
    protected string[] dlmtCOURSE = { "COURSE:" };

    protected int current_chapter_index = 1;
    protected int nNowRoll = 0;
    protected int nNowRollCount = 0;

    public string TITLE { get; protected set; }
    public float dbScrollSpeed { get; protected set; }
    public int LoadCourse { get; protected set; }

    private object RemoveChangeLine(string strInput, int nMode)
    {
        string str = "";
        str = strInput.Replace(Environment.NewLine, "\n");
        str = str.Replace(new_line, "\n");
        str = str.Replace('\t', ' ');

        if (nMode == 0)
        {
            str = str.Replace("\n", " ");
        }
        else if (nMode == 1)
        {
            str = str + "\n";

            string[] strArray;
            strArray = str.Split(this.dlmtEnter, StringSplitOptions.RemoveEmptyEntries);

            return strArray;
        }

        return str;
    }

    private string RemoveComment(string input)
    {
        string strOutput = Regex.Replace(input, @" *//.*", ""); //2017.01.28 DD コメント前のスペースも削除するように修正

        return strOutput;
    }

    private string[] t空のstring配列を詰めたstring配列を返す(string[] input)
    {
        var sb = new StringBuilder();

        for (int n = 0; n < input.Length; n++)
        {
            if (!string.IsNullOrEmpty(input[n]))
            {
                sb.Append(input[n] + "\n");
            }
        }

        string[] strOutput = sb.ToString().Split(this.dlmtEnter, StringSplitOptions.None);

        return strOutput;
    }

    private void t入力_行解析ヘッダ(string InputText)
    {
        //やべー。先頭にコメント行あったらやばいやん。
        string[] strArray = InputText.Split(new char[] { ':' });
        string strCommandName = "";
        string strCommandParam = "";

        if (InputText.StartsWith("#BRANCHSTART"))
        {
            //2015.08.18 kairera0467
            //本来はヘッダ命令ではありませんが、難易度ごとに違う項目なのでここで読み込ませます。
            //Lengthのチェックをされる前ににif文を入れています。
            this.bHasBranch[current_course_difficulty] = true;
            //Debug.Log(string.Format("difficulty {0} has branch", current_course_difficulty));
        }

        //まずは「:」でSplitして割り当てる。
        if (strArray.Length == 2)
        {
            strCommandName = strArray[0].Trim();
            strCommandParam = strArray[1].Trim();
        }
        else if (strArray.Length > 2)
        {
            //strArrayが2じゃない場合、ヘッダのSplitを通していない可能性がある。
            //この処理自体は「t入力」を改造したもの。STARTでSplitしていない等、一部の処理が異なる。

            #region[ヘッダ]
            InputText = InputText.Replace(Environment.NewLine, "\n"); //改行文字を別の文字列に差し替え。
            InputText = InputText.Replace(new_line, "\n");
            InputText = InputText.Replace('\t', ' '); //何の文字か知らないけどスペースに差し替え。
            InputText = InputText + "\n";

            string[] strDelimiter2 = { "\n" };
            strArray = InputText.Split(strDelimiter2, StringSplitOptions.RemoveEmptyEntries);


            strArray = strArray[0].Split(new char[] { ':' });
            WarnSplitLength("Header Name & Value", strArray, 2);

            strCommandName = strArray[0].Trim();
            strCommandParam = strArray[1].Trim();

            #endregion
        }

        void ParseOptionalInt16(Action<short> setValue)
        {
            this.ParseOptionalInt16(strCommandName, strCommandParam, setValue);
        }

        //パラメータを分別、そこから割り当てていきます。
        if (strCommandName.Equals("TITLE"))
        {
            //this.TITLE = strCommandParam;
            var subTitle = "";
            for (int i = 0; i < strArray.Length; i++)
            {
                subTitle += strArray[i];
            }

            this.TITLE = subTitle.Substring(5);

        }
        if (strCommandName.Equals("SUBTITLE"))
        {
            if (strCommandParam.StartsWith("--"))
            {
                //this.SUBTITLE = strCommandParam.Remove( 0, 2 );
                var subTitle = "";
                for (int i = 0; i < strArray.Length; i++)
                {
                    subTitle += strArray[i];
                }
                this.SUBTITLE = subTitle.Substring(10);
            }
            else if (strCommandParam.StartsWith("++"))
            {
                var subTitle = "";
                for (int i = 0; i < strArray.Length; i++)
                {
                    subTitle += strArray[i];
                }
                this.SUBTITLE = subTitle.Substring(10);
            }
        }
        else if (strCommandName.Equals("LEVEL"))
        {
            var level = (int)Convert.ToDouble(strCommandParam);
            this.LEVELtaiko[current_course_difficulty] = level;
        }
        else if (strCommandName.Equals("BPM"))
        {
            if (strCommandParam.IndexOf(",") != -1)
                strCommandParam = strCommandParam.Replace(',', '.');

            float dbBPM = (float)Convert.ToDouble(strCommandParam);
            this.BPM = dbBPM;
            this.BASEBPM = dbBPM;
            this.dbNowBPM = dbBPM;

            this.listBPM.Add(this.n内部番号BPM1to - 1, new CBPM() { n内部番号 = this.n内部番号BPM1to - 1, n表記上の番号 = this.n内部番号BPM1to - 1, dbBPM値 = dbBPM, });
            this.n内部番号BPM1to++;

            CChip chip = new CChip
            {
                Type = CChip.CType.BPM_CHANGE,
                JudgeTime = (int)this.dbNowTime + this.nOFFSET,
                Bpm = dbBPM,
                StartChapter = current_chapter_index
            };
            Others.Add(chip);

            chips.Add(1);
        }
        else if (strCommandName.Equals("OFFSET") && !string.IsNullOrEmpty(strCommandParam))
        {
            this.nOFFSET = (int)(Convert.ToDouble(strCommandParam) * 1000);
            this.offset_minus = this.nOFFSET < 0 ? true : false;

            this.listBPM[0].bpm_change_bmscroll_time = -2000 * this.dbNowBPM / 15000;
            this.nOFFSET = this.nOFFSET * -1;
            /*
            if (this.offset_minus == true)
                this.nOFFSET = this.nOFFSET * -1; //OFFSETは秒を加算するので、必ず正の数にすること。
                */
            //tbOFFSET.Text = strCommandParam;
        }
        #region[移動→不具合が起こるのでここも一応復活させておく]
        else if (strCommandName.Equals("BALLOON"))
        {

            ParseBalloon(strCommandParam, this.commonBalloon);
        }
        else if (strCommandName.Equals("BALLOONNOR"))
        {
            ParseBalloon(strCommandParam, this.listBalloon_Normal);
        }
        else if (strCommandName.Equals("BALLOONEXP"))
        {
            ParseBalloon(strCommandParam, this.listBalloon_Expert);
            //tbBALLOON.Text = strCommandParam;
        }
        else if (strCommandName.Equals("BALLOONMAS"))
        {
            ParseBalloon(strCommandParam, this.listBalloon_Master);
            //tbBALLOON.Text = strCommandParam;
        }
        else if (strCommandName.Equals("SCOREMODE"))
        {
            ParseOptionalInt16(value => ScoreModeTmp = value);
        }
        else if (strCommandName.Equals("SCOREINIT"))
        {
            if (!string.IsNullOrEmpty(strCommandParam))
            {
                string[] scoreinit = strCommandParam.Split(',');

                this.ParseOptionalInt16("SCOREINIT first value", scoreinit[0], value =>
                {
                    ScoreInit[0, this.current_course_difficulty] = value;
                });

                if (scoreinit.Length == 2)
                {
                    this.ParseOptionalInt16("SCOREINIT second value", scoreinit[1], value =>
                    {
                        ScoreInit[1, this.current_course_difficulty] = value;
                    });
                }
            }
        }
        else if (strCommandName.Equals("SCOREDIFF"))
        {
            ParseOptionalInt16(value => ScoreDiff[current_course_difficulty] = value);
        }
        #endregion
        else if (strCommandName.Equals("SONGVOL") && !string.IsNullOrEmpty(strCommandParam))
        {
            this.SongVol = Math.Min(100, Convert.ToInt32(strCommandParam));
        }
        else if (strCommandName.Equals("SEVOL"))
        {
            //tbSeVol.Text = strCommandParam;
        }
        else if (strCommandName.Equals("COURSE"))
        {
            if (!string.IsNullOrEmpty(strCommandParam))
            {
                this.current_course_difficulty = this.strConvertCourse(strCommandParam);
            }
        }

        else if (strCommandName.Equals("HEADSCROLL"))
        {
            //新定義:初期スクロール速度設定(というよりこのシステムに合わせるには必須。)
            //どうしても一番最初に1小節挿入されるから、こうするしかなかったんだ___

            this.dbScrollSpeed = (float)Convert.ToDouble(strCommandParam);

            this.listSCROLL.Add(this.n内部番号SCROLL1to, new CSCROLL() { n内部番号 = this.n内部番号SCROLL1to, n表記上の番号 = 0, dbSCROLL値 = this.dbScrollSpeed, });

            /*
            //チップ追加して割り込んでみる。
            var chip = new CChip();

            chip.nチャンネル番号 = 0x9D;
            chip.n発声位置 = ((this.n現在の小節数 - 2) * 384);
            chip.n整数値 = 0x00;
            chip.n整数値_内部番号 = this.n内部番号SCROLL1to;
            chip.dbSCROLL = this.dbScrollSpeed;

            // チップを配置。

            this.listChip.Add(chip);
            */
            chips.Add(1);
            this.n内部番号SCROLL1to++;

            //this.nScoreDiff = Convert.ToInt16( strCommandParam );
            //tbScoreDiff.Text = strCommandParam;
        }
        else if (strCommandName.Equals("GENRE"))
        {
            //2015.03.28 kairera0467
            //ジャンルの定義。DTXから入力もできるが、tjaからも入力できるようにする。
            //日本語名だと選曲画面でバグが出るので、そこもどうにかしていく予定。

            if (!string.IsNullOrEmpty(strCommandParam))
            {
                this.GENRE = strCommandParam;
            }
        }
        else if (strCommandName.Equals("DEMOSTART"))
        {
            //2015.04.10 kairera0467

            if (!string.IsNullOrEmpty(strCommandParam))
            {
                int nOFFSETms;
                try
                {
                    nOFFSETms = (int)(Convert.ToDouble(strCommandParam) * 1000.0);
                }
                catch
                {
                    nOFFSETms = 0;
                }


                this.DemoBgmOffset = nOFFSETms;
            }
        }
        else if (strCommandName.Equals("BGIMAGE"))
        {
            //2016.02.02 kairera0467
            if (!string.IsNullOrEmpty(strCommandParam))
            {
                this.strBGIMAGE_PATH = strCommandParam;
            }
        }
        else if (strCommandName.Equals("HIDDENBRANCH"))
        {
            //2016.04.01 kairera0467 パラメーターは
            if (!string.IsNullOrEmpty(strCommandParam))
            {
                this.bHIDDENBRANCH = true;
            }
        }
        if (ScoreModeTmp == 99)
        {
            //2017.01.28 DD 
            ScoreModeTmp = 2;
            //スコア計算方法(0:旧配点, 1:旧筐体配点, 2:新配点
        }
    }

    private void WarnSplitLength(string name, string[] strArray, int minimumLength)
    {
        if (strArray.Length < minimumLength)
        {
            Debug.Log(
                $"命令 {name} のパラメータが足りません。少なくとも {minimumLength} つのパラメータが必要です。 (現在のパラメータ数: {strArray.Length}). (filename)");
        }
    }

    private void ParseOptionalInt16(string name, string unparsedValue, Action<short> setValue)
    {
        if (string.IsNullOrEmpty(unparsedValue))
        {
            return;
        }

        if (short.TryParse(unparsedValue, out var value))
        {
            setValue(value);
        }
        else
        {
            Debug.Log($"命令名: {name} のパラメータの値が正しくないことを検知しました。値: {unparsedValue} (filename)");
        }
    }

    public static string tZZ(int n)
    {
        if (n < 0 || n >= 36 * 36)
            return "!!";    // オーバー／アンダーフロー。

        // n を36進数2桁の文字列にして返す。

        string str = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        return new string(new char[] { str[n / 36], str[n % 36] });
    }

    private int strConvertCourse(string str)
    {
        //2016.08.24 kairera0467
        //正規表現を使っているため、easyでもEASYでもOK。

        // 小文字大文字区別しない正規表現で仮対応。 (AioiLight)
        // 相変わらず原始的なやり方だが、正常に動作した。
        string[] Matchptn = new string[7] { "easy", "normal", "hard", "oni", "edit", "tower", "dan" };
        for (int i = 0; i < Matchptn.Length; i++)
        {
            if (Regex.IsMatch(str, Matchptn[i], RegexOptions.IgnoreCase))
            {
                return i;
            }
        }

        switch (str)
        {
            case "0":
                return 0;
            case "1":
                return 1;
            case "2":
                return 2;
            case "3":
                return 3;
            case "4":
                return 4;
            case "5":
                return 5;
            case "6":
                return 6;
            default:
                return 3;
        }
    }

    private void ParseBalloon(string strCommandParam, Dictionary<int, List<int>> listBalloon)
    {
        string[] strParam = strCommandParam.Split(',');
        if (!listBalloon.ContainsKey(current_course_difficulty)) listBalloon[current_course_difficulty] = new List<int>();
        for (int n = 0; n < strParam.Length; n++)
        {
            int n打数;
            try
            {
                if (strParam[n] == null || strParam[n] == "")
                    break;

                n打数 = Convert.ToInt32(strParam[n]);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.ToString());
                break;
            }

            listBalloon[current_course_difficulty].Add(n打数);
            //Debug.Log(string.Format("difficiculty  {0} add balloon {1}", current_course_difficulty, n打数));
        }
    }

    private string[] SplitByCourse(string strTJA)
    {
        string[] strCourseTJA = new string[(int)Difficulty.Total];

        if (strTJA.IndexOf("COURSE", 0) != -1)
        {
            //tja内に「COURSE」があればここを使う。
            string[] strTemp = strTJA.Split(this.dlmtCOURSE, StringSplitOptions.RemoveEmptyEntries);

            for (int n = 1; n < strTemp.Length; n++)
            {
                int nCourse = 0;
                string nNC = "";
                while (strTemp[n].Substring(0, 1) != "\n") //2017.01.29 DD COURSE単語表記に対応
                {
                    nNC += strTemp[n].Substring(0, 1);
                    strTemp[n] = strTemp[n].Remove(0, 1);
                }

                if (this.strConvertCourse(nNC) != -1)
                {
                    nCourse = this.strConvertCourse(nNC);
                    strCourseTJA[nCourse] = strTemp[n];
                }
                else
                {

                }
                //strCourseTJA[ ];

            }
        }
        else
        {
            strCourseTJA[3] = strTJA;
        }

        return strCourseTJA;
    }

    private string StringArrayToString(string[] input, string strデリミタ文字)
    {
        var sb = new StringBuilder();

        for (int n = 0; n < input.Length; n++)
        {
            sb.Append(input[n] + strデリミタ文字);
        }

        return sb.ToString();
    }

    private string[] tコマンド行を削除したTJAを返す(string[] input, int nMode)
    {
        var sb = new StringBuilder();

        // 18/11/11 AioiLight 譜面にSpace、スペース、Tab等が入っているとおかしくなるので修正。
        // 多分コマンドもスペースが抜かれちゃっているが、コマンド行を除く譜面を返すので大丈夫(たぶん)。
        for (int i = 0; i < input.Length; i++)
        {
            input[i] = input[i].Trim();
        }

        for (int n = 0; n < input.Length; n++)
        {
            if (nMode == 0)
            {
                if (!string.IsNullOrEmpty(input[n]) && this.CharConvertNote(input[n].Substring(0, 1)) != -1)
                {
                    sb.Append(input[n] + "\n");
                }
            }
            else if (nMode == 1)
            {
                if (!string.IsNullOrEmpty(input[n]) && (input[n].Substring(0, 1) == "#" || this.CharConvertNote(input[n].Substring(0, 1)) != -1))
                {
                    if (input[n].StartsWith("BALLOON") || input[n].StartsWith("BPM"))
                    {
                        //A～Fで始まる命令が削除されない不具合の対策
                    }
                    else
                    {
                        sb.Append(input[n] + "\n");
                    }
                }
            }
            else if (nMode == 2)
            {
                if (!string.IsNullOrEmpty(input[n]) && this.CharConvertNote(input[n].Substring(0, 1)) != -1)
                {
                    if (input[n].StartsWith("BALLOON") || input[n].StartsWith("BPM"))
                    {
                        //A～Fで始まる命令が削除されない不具合の対策
                    }
                    else
                    {
                        sb.Append(input[n] + "\n");
                    }
                }
                else
                {
                    if (input[n].StartsWith("#BRANCHSTART") || input[n] == "#N" || input[n] == "#E" || input[n] == "#M")
                    {
                        sb.Append(input[n] + "\n");
                    }

                }
            }
        }

        string[] strOutput = sb.ToString().Split(this.dlmtEnter, StringSplitOptions.None);

        return strOutput;
    }

    private int CharConvertNote(string str)
    {
        switch (str)
        {
            case "0":
                return 0;
            case "1":
                return 1;
            case "2":
                return 2;
            case "3":
                return 3;
            case "4":
                return 4;
            case "5":
                return 5;
            case "6":
                return 6;
            case "7":
                return 7;
            case "8":
                return 8;
            case "9":
                return 9; //2017.01.30 DD 芋連打を風船連打扱いに
            case "A": //2017.08.22 kairera0467 手つなぎ
                return 10;
            case "B":
                return 11;
            case "F":
                return 15;
            default:
                return -1;
        }
    }

    /// <summary>
    /// 難易度ごとによって変わるヘッダ値を読み込む。
    /// (BALLOONなど。)
    /// </summary>
    /// <param name="InputText"></param>
    private void t難易度別ヘッダ(string InputText)
    {
        string[] strArray = InputText.Split(new char[] { ':' });
        string strCommandName = "";
        string strCommandParam = "";

        if (strArray.Length == 2)
        {
            strCommandName = strArray[0].Trim();
            strCommandParam = strArray[1].Trim();
        }

        void ParseOptionalInt16(Action<short> setValue)
        {
            this.ParseOptionalInt16(strCommandName, strCommandParam, setValue);
        }

        if (strCommandName.Equals("BALLOON"))
        {
            ParseBalloon(strCommandParam, this.commonBalloon);
        }
        else if (strCommandName.Equals("BALLOONNOR"))
        {
            ParseBalloon(strCommandParam, this.listBalloon_Normal);
        }
        else if (strCommandName.Equals("BALLOONEXP"))
        {
            ParseBalloon(strCommandParam, this.listBalloon_Expert);
            //tbBALLOON.Text = strCommandParam;
        }
        else if (strCommandName.Equals("BALLOONMAS"))
        {
            ParseBalloon(strCommandParam, this.listBalloon_Master);
            //tbBALLOON.Text = strCommandParam;
        }
        else if (strCommandName.Equals("SCOREMODE"))
        {
            ParseOptionalInt16(value => ScoreModeTmp = value);
        }
        else if (strCommandName.Equals("SCOREINIT"))
        {
            if (!string.IsNullOrEmpty(strCommandParam))
            {
                string[] scoreinit = strCommandParam.Split(',');

                this.ParseOptionalInt16("SCOREINIT first value", scoreinit[0], value =>
                {
                    ScoreInit[0, this.current_course_difficulty] = value;
                    b配点が指定されている[0, this.current_course_difficulty] = true;
                });

                if (scoreinit.Length == 2)
                {
                    this.ParseOptionalInt16("SCOREINIT second value", scoreinit[1], value =>
                    {
                        ScoreInit[1, this.current_course_difficulty] = value;
                        b配点が指定されている[2, this.current_course_difficulty] = true;
                    });
                }
            }
        }
        else if (strCommandName.Equals("SCOREDIFF"))
        {
            ParseOptionalInt16(value =>
            {
                ScoreDiff[this.current_course_difficulty] = value;
                b配点が指定されている[1, this.current_course_difficulty] = true;
            });
        }
        else if (strCommandName.Equals("SCOREMODE"))
        {
            if (!string.IsNullOrEmpty(strCommandParam))
            {
                ScoreModeTmp = Convert.ToInt16(strCommandParam);
            }
        }
        else if (strCommandName.Equals("SCOREINIT"))
        {
            if (!string.IsNullOrEmpty(strCommandParam))
            {
                string[] scoreinit = strCommandParam.Split(',');

                ScoreInit[0, this.current_course_difficulty] = Convert.ToInt16(scoreinit[0]);
                b配点が指定されている[0, this.current_course_difficulty] = true;
                if (scoreinit.Length == 2)
                {
                    ScoreInit[1, this.current_course_difficulty] = Convert.ToInt16(scoreinit[1]);
                    b配点が指定されている[2, this.current_course_difficulty] = true;
                }
            }
        }
        else if (strCommandName.Equals("SCOREDIFF"))
        {
            if (!string.IsNullOrEmpty(strCommandParam))
            {
                ScoreDiff[this.current_course_difficulty] = Convert.ToInt16(strCommandParam);
                b配点が指定されている[1, this.current_course_difficulty] = true;
            }
        }
        
        if (ScoreModeTmp == 99) //2017.01.28 DD SCOREMODEを入力していない場合のみConfigで設定したモードにする
        {
            ScoreModeTmp = 2;
        }
    }

    private void t1小節の文字数をカウントしてリストに追加する(string InputText)
    {
        if (InputText.StartsWith("#BRANCHSTART"))
        {
            this.nLineCountTemp = this.current_chapter_index;
            return;
        }
        else if (InputText.StartsWith("#N"))
        {
            this.nLineCountCourseTemp = 0;
            this.current_chapter_index = this.nLineCountTemp;
            return;
        }
        else if (InputText.StartsWith("#E"))
        {
            this.nLineCountCourseTemp = 1;
            this.current_chapter_index = this.nLineCountTemp;
            return;
        }
        else if (InputText.StartsWith("#M"))
        {
            this.nLineCountCourseTemp = 2;
            this.current_chapter_index = this.nLineCountTemp;
            return;
        }

        var line = new CLine
        {
            nコース = this.nLineCountCourseTemp,
            n文字数 = InputText.Length - 1,
            n小節番号 = this.current_chapter_index
        };

        this.listLine.Add(line);

        this.current_chapter_index++;
    }

    private void MapDeCode(string InputText)
    {
        if (!string.IsNullOrEmpty(InputText))
        {
            int n文字数 = 16;

            //現在のコース、小節に当てはまるものをリストから探して文字数を返す。
            for (int i = 0; i < this.listLine.Count; i++)
            {
                if (this.listLine[i].n小節番号 == this.current_chapter_index && this.listLine[i].nコース == this.current_course)
                {
                    n文字数 = this.listLine[i].n文字数;
                    //Debug.Log(string.Format("current chapter {0}, count is {1}", current_chapter_index, n文字数));
                    break;
                }
            }

            if (InputText.StartsWith("#"))
            {
                this.InsertCommand(InputText);
                return;
            }
            else
            {
                if (this.b小節線を挿入している == false)
                {
                    chips.Add(1);
                    bool inserted = false;
                    foreach (ChapterLineScript script in Lines)
                    {
                        if (script.Chapter == current_chapter_index)
                        {
                            inserted = true;
                            break;
                        }
                    }
                    if (!inserted)
                    {
                        if (GameSetting.Mode != CommonClass.PlayMode.Replay)
                        {
                            ChapterLine script = new ChapterLine();
                            script.Chapter = current_chapter_index;
                            script.Bpm = dbNowBPM;

                            float speed = (int)GameSetting.Config.Speed;
                            if (speed > 10) speed /= 10;

                            script.Scroll = dbNowScroll * speed;

                            script.JudgeTime = (float)this.dbNowTime + nOFFSET;
                            script.AppearTime = (float)(this.AppearTime * 1000.0);
                            script.MoveTime = (float)(this.WaitingTime * 1000.0);
                            script.WaitingTime = (int)(this.WaitingTime * 1000.0);
                            lines.Add(script);

                            if (GameSetting.Mode == CommonClass.PlayMode.PlayWithReplay)
                            {
                                ChapterLine script2 = new ChapterLine();
                                script2.Chapter = current_chapter_index;
                                script2.Bpm = dbNowBPM;

                                float speed2 = (int)config.Speed;
                                if (speed2 > 10) speed2 /= 10;

                                script2.Scroll = dbNowScroll * speed2;
                                script2.JudgeTime = (float)this.dbNowTime + nOFFSET;
                                script2.AppearTime = (float)(this.AppearTime * 1000.0);
                                script2.MoveTime = (float)(this.WaitingTime * 1000.0);
                                script2.WaitingTime = (int)(this.WaitingTime * 1000.0);
                                lines_2p.Add(script2);
                            }
                        }
                        else
                        {
                            ChapterLine script = new ChapterLine();
                            script.Chapter = current_chapter_index;
                            script.Bpm = dbNowBPM;

                            float speed = (int)config.Speed;
                            if (speed > 10) speed /= 10;

                            script.Scroll = dbNowScroll * speed;

                            script.JudgeTime = (float)this.dbNowTime + nOFFSET;
                            script.AppearTime = (float)(this.AppearTime * 1000.0);
                            script.MoveTime = (float)(this.WaitingTime * 1000.0);
                            script.WaitingTime = (int)(this.WaitingTime * 1000.0);
                            lines.Add(script);
                        }
                    }

                    this.dbLastTime = this.dbNowTime;
                    this.b小節線を挿入している = true;

                    /*
                #region[ 拍線チップテスト ]
                //1拍の時間を計算
                double db1拍 = (60.0 / this.dbNowBPM) / 4.0;
                //forループ(拍数)
                for (int measure = 1; measure < this.fNow_Measure_s; measure++)
                {
                    CChip hakusen = new CChip();
                    hakusen.n発声位置 = ((this.n現在の小節数) * 384);
                    hakusen.n発声時刻ms = (int)(this.dbNowTime + (((db1拍 * 4.0)) * measure) * 1000.0);
                    hakusen.nチャンネル番号 = 0x51;
                    //hakusen.n発声時刻ms = (int)this.dbNowTime;
                    hakusen.fBMSCROLLTime = this.dbNowBMScollTime;
                    hakusen.n整数値_内部番号 = this.n現在の小節数;
                    hakusen.n整数値 = 0;
                    hakusen.dbBPM = this.dbNowBPM;
                    hakusen.dbSCROLL = this.dbNowScroll;
                    hakusen.dbSCROLL_Y = this.dbNowScrollY;
                    hakusen.nコース = this.current_course;

                    this.listChip.Add(hakusen);
                    //--全ての拍線の時間を出力する--
                    //Trace.WriteLine( string.Format( "|| {0,3:##0} Time:{1} Beat:{2}", this.n現在の小節数, hakusen.n発声時刻ms, measure ) );
                    //--------------------------------

                    //chips.Add(1);
                    //Debug.Log("insert make sound");
                }

                #endregion
                    */
                }

                for (int n = 0; n < InputText.Length; n++)
                {
                    if (InputText.Substring(n, 1) == ",")
                    {
                        this.current_chapter_index++;
                        this.b小節線を挿入している = false;

                        return;
                    }

                    int nObjectNum = this.CharConvertNote(InputText.Substring(n, 1));

                    if (nObjectNum != 0)
                    {
                        //if ((nObjectNum >= 5 && nObjectNum <= 7) || nObjectNum == 9)
                        if ((nObjectNum >= 5 && nObjectNum <= 7))
                        {
                            if (nNowRoll != 0)
                            {
                                this.dbNowTime += (15000 / this.dbNowBPM * (this.fNow_Measure_s / this.fNow_Measure_m) * (16 / n文字数));
                                //Debug.Log("change dbtime");
                                this.dbNowBMScollTime += ((this.dbBarLength) * (16 / n文字数));
                                continue;
                            }
                            else
                            {
                                this.nNowRollCount = note_index;
                                nNowRoll = nObjectNum;
                            }
                        }

                        for (int i = 0; i < 1; i++)
                        {
                            if (nObjectNum == 8)
                            {
                                /*
                                chip.nノーツ終了位置 = (this.n現在の小節数 * 384) + ((384 * n) / n文字数);
                                chip.nノーツ終了時刻ms = (int)this.dbNowTime;
                                chip.fBMSCROLLTime_end = (float)this.dbNowBMScollTime;

                                chip.nノーツ出現時刻ms = listChip[nNowRollCount].nノーツ出現時刻ms;
                                chip.nノーツ移動開始時刻ms = listChip[nNowRollCount].nノーツ移動開始時刻ms;

                                chip.n連打音符State = nNowRoll;
                                listChip[nNowRollCount].nノーツ終了位置 = (this.n現在の小節数 * 384) + ((384 * n) / n文字数);
                                listChip[nNowRollCount].nノーツ終了時刻ms = (int)this.dbNowTime;
                                listChip[nNowRollCount].fBMSCROLLTime_end = (int)this.dbNowBMScollTime;
                                //listChip[ nNowRollCount ].dbBPM = this.dbNowBPM;
                                //listChip[ nNowRollCount ].dbSCROLL = this.dbNowSCROLL;
                                */
                                nNowRoll = 0;
                                //continue;
                            }

                            /*
                            if (nObjectNum < 5)
                            {
                                if (this.b最初の分岐である == false)
                                    this.nノーツ数[this.current_course]++;
                                else
                                    this.nノーツ数[3]++;
                            }
                            else if (nObjectNum == 7)
                            {
                                if (this.b最初の分岐である == false)
                                    this.n風船数[this.current_course]++;
                                else
                                    this.n風船数[3]++;
                            }


                            this.listChip.Add(chip);
                            */

                            //Debug.Log(string.Format("insert note at chapter {0}, current bpm is {1}", current_chapter_index, dbNowBPM));

                            if (nObjectNum != 8)
                            {
                                NoteChip sound = new NoteChip
                                {
                                    Type = nObjectNum
                                };
                                switch (nObjectNum)
                                {
                                    case 7:
                                        switch (this.current_course)
                                        {
                                            case 0:
                                                if (this.listBalloon_Normal.Count > this.listBalloon_Normal_数値管理)
                                                {
                                                    sound.HitCount = this.listBalloon_Normal[LoadCourse][this.listBalloon_Normal_数値管理];
                                                    this.listBalloon_Normal_数値管理++;
                                                }
                                                else if (commonBalloon[LoadCourse].Count > 0)
                                                {
                                                    sound.HitCount = commonBalloon[LoadCourse][0];
                                                    commonBalloon[LoadCourse].RemoveAt(0);
                                                }
                                                else
                                                {
#if UNITY_EDITOR
                                                    Debug.Log("balloon at normal is Empty");
#endif
                                                    sound.HitCount = 5;
                                                }

                                                break;
                                            case 1:
                                                if (this.listBalloon_Expert.Count > this.listBalloon_Expert_数値管理)
                                                {
                                                    sound.HitCount = this.listBalloon_Expert[LoadCourse][this.listBalloon_Expert_数値管理];
                                                    this.listBalloon_Expert_数値管理++;
                                                }
                                                else if (commonBalloon[LoadCourse].Count > 0)
                                                {
                                                    sound.HitCount = commonBalloon[LoadCourse][0];
                                                    commonBalloon[LoadCourse].RemoveAt(0);
                                                }
                                                else
                                                {
#if UNITY_EDITOR
                                                    Debug.Log("balloon at expert is Empty");
#endif
                                                    sound.HitCount = 5;
                                                }

                                                break;
                                            case 2:
                                                if (this.listBalloon_Master.Count > this.listBalloon_Master_数値管理)
                                                {
                                                    sound.HitCount = this.listBalloon_Master[LoadCourse][this.listBalloon_Master_数値管理];
                                                    this.listBalloon_Master_数値管理++;
                                                }
                                                else if (commonBalloon[LoadCourse].Count > 0)
                                                {
                                                    sound.HitCount = commonBalloon[LoadCourse][0];
                                                    commonBalloon[LoadCourse].RemoveAt(0);
                                                }
                                                else
                                                {
#if UNITY_EDITOR
                                                    Debug.Log("balloon at master is Empty");
#endif
                                                    sound.HitCount = 5;
                                                }

                                                break;
                                        }
                                        break;
                                    case 9:
                                        {
                                            if (notes.Count > 0 && notes[notes.Count - 1].Type == 9 && notes[notes.Count - 1].EndTime == 0)
                                            {
                                                notes[notes.Count - 1].MidTime = (float)this.dbNowTime + nOFFSET;
                                                sound = null;
                                            }
                                            else
                                            {
                                                switch (this.current_course)
                                                {
                                                    case 0:
                                                        if (this.listBalloon_Normal.Count > this.listBalloon_Normal_数値管理)
                                                        {
                                                            sound.HitCount = this.listBalloon_Normal[LoadCourse][this.listBalloon_Normal_数値管理];
                                                            this.listBalloon_Normal_数値管理++;
                                                        }
                                                        else if (commonBalloon[LoadCourse].Count > 0)
                                                        {
                                                            sound.HitCount = commonBalloon[LoadCourse][0];
                                                            commonBalloon[LoadCourse].RemoveAt(0);
                                                        }
                                                        else
                                                        {
#if UNITY_EDITOR
                                                            Debug.Log("balloon at normal is Empty");
#endif
                                                            sound.HitCount = 5;
                                                        }

                                                        break;
                                                    case 1:
                                                        if (this.listBalloon_Expert.Count > this.listBalloon_Expert_数値管理)
                                                        {
                                                            sound.HitCount = this.listBalloon_Expert[LoadCourse][this.listBalloon_Expert_数値管理];
                                                            this.listBalloon_Expert_数値管理++;
                                                        }
                                                        else if (commonBalloon[LoadCourse].Count > 0)
                                                        {
                                                            sound.HitCount = commonBalloon[LoadCourse][0];
                                                            commonBalloon[LoadCourse].RemoveAt(0);
                                                        }
                                                        else
                                                        {
#if UNITY_EDITOR
                                                            Debug.Log("balloon at expert is Empty");
#endif
                                                            sound.HitCount = 5;
                                                        }

                                                        break;
                                                    case 2:
                                                        if (this.listBalloon_Master.Count > this.listBalloon_Master_数値管理)
                                                        {
                                                            sound.HitCount = this.listBalloon_Master[LoadCourse][this.listBalloon_Master_数値管理];
                                                            this.listBalloon_Master_数値管理++;
                                                        }
                                                        else if (commonBalloon[LoadCourse].Count > 0)
                                                        {
                                                            sound.HitCount = commonBalloon[LoadCourse][0];
                                                            commonBalloon[LoadCourse].RemoveAt(0);
                                                        }
                                                        else
                                                        {
#if UNITY_EDITOR
                                                            Debug.Log("balloon at master is Empty");
#endif
                                                            sound.HitCount = 5;
                                                        }

                                                        break;
                                                }
                                            }
                                        }
                                        break;
                                }

                                if (sound != null)
                                {
                                    sound.Chapter = current_chapter_index;
                                    sound.Bpm = dbNowBPM;
                                    sound.Scroll = dbNowScroll;
                                    sound.fBMSCROLLTime = (float)this.dbNowBMScollTime;
                                    sound.JudgeTime = (float)this.dbNowTime + nOFFSET;
                                    sound.AppearTime = (float)(this.AppearTime * 1000.0);
                                    sound.MoveTime = (float)(this.WaitingTime * 1000.0);
                                    sound.WaitingTime = (int)(this.WaitingTime * 1000.0);
                                    sound.Branch = (CourseBranch)current_course;

                                    for (int index = Others.Count - 1; index >= 0; index--)
                                    {
                                        CChip chip = Others[index];
                                        if (chip.Type == CChip.CType.GOGO_TIME)
                                        {
                                            if (sound.Chapter >= chip.StartChapter)
                                            {
                                                if (sound.Chapter <= chip.EndChpater || chip.EndChpater == 0)
                                                {
                                                    sound.Gogo = true;
                                                }
                                            }
                                        }

                                        if (chip.Type == CChip.CType.BRANCH_START && sound.Chapter >= chip.StartChapter && (chip.EndChpater == 0 || sound.Chapter < chip.EndChpater))
                                        {
                                            sound.BranchRelated = true;
                                        }
                                    }
                                    sound.Index = note_index;
                                    notes.Add(note_index, sound);
                                    note_index++;

                                    if (IsEnabledFixSENote)
                                    {
                                        sound.IsFixedSENote = true;
                                        sound.Senote = FixSENote - 1;
                                    }

                                    #region[ 固定される種類のsenotesはここで設定しておく。 ]
                                    switch (nObjectNum)
                                    {
                                        case 3:
                                            sound.Senote = 5;
                                            break;
                                        case 4:
                                            sound.Senote = 6;
                                            break;
                                        case 5:
                                            sound.Senote = 7;
                                            break;
                                        case 6:
                                            sound.Senote = 10;
                                            break;
                                        case 7:
                                            sound.Senote = 11;
                                            break;
                                        case 9:
                                            sound.Senote = 12;
                                            break;
                                        case 10:
                                            sound.Senote = 13;
                                            break;
                                        case 11:
                                            sound.Senote = 14;
                                            break;
                                    }
                                    #endregion
                                }
                            }
                            else
                            {
                                NoteChip script = notes[notes.Count - 1];
                                if (script.Type > 6 && script.Type < 10)
                                {
                                    script.EndTime = (float)this.dbNowTime + nOFFSET;
                                    if (script.Type == 9 && script.MidTime == 0)
                                    {
                                        float mid = (60 / (float)this.dbNowBPM) / 4 * 1000;
                                        if (script.EndTime - script.JudgeTime > mid)
                                            script.MidTime = script.EndTime - mid;
                                    }
                                }
                                else if (script.Type == 5 || script.Type == 6)
                                    script.EndTime = (float)this.dbNowTime + nOFFSET;
                            }
                        }
                    }
                    if (IsEnabledFixSENote) IsEnabledFixSENote = false;

                    this.dbLastTime = this.dbNowTime;
                    this.dbLastBMScrollTime = this.dbNowBMScollTime;
                    this.dbNowTime += (15000 / this.dbNowBPM * (this.fNow_Measure_s / this.fNow_Measure_m) * (16 / (float)n文字数));
                    /*
                    if (InputText.Length > 50)
                    {
                        Debug.Log(this.dbNowTime);
                    }
                    */
                    this.dbNowBMScollTime += (((this.fNow_Measure_s / this.fNow_Measure_m)) * (16 / (float)n文字数));
                }
            }
        }
    }
    private static readonly Regex CommandAndArgumentRegex = new Regex(@"^(#[A-Z]+)(?:\s?)(.+?)?$", RegexOptions.Compiled);
    private static readonly Regex BranchStartArgumentRegex = new Regex(@"^([^,\s]+)\s*,\s*([^,\s]+)\s*,\s*([^,\s]+)$", RegexOptions.Compiled);
    private void InsertCommand(string InputText)
    {
        /*
        string[] SplitComma(string input)
        {
            var result = new List<string>();
            var workingIndex = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == ',') // カンマにぶち当たった
                {
                    if (input[i - 1] == '\\') // 1文字前がバックスラッシュ
                    {
                        input = input.Remove(i - 1, 1);
                    }
                    else
                    {
                        // workingIndexから今の位置までをリストにブチ込む
                        result.Add(input.Substring(workingIndex, i - workingIndex));
                        // workingIndexに今の位置+1を代入
                        workingIndex = i + 1;
                    }
                }
                if (i + 1 == input.Length) // 最後に
                {
                    result.Add(input.Substring(workingIndex, input.Length - workingIndex));
                }
            }
            return result.ToArray();
        }
        */

        var match = CommandAndArgumentRegex.Match(InputText);
        if (!match.Success)
        {
            return;
        }

        var command = match.Groups[1].Value;
        var argumentMatchGroup = match.Groups[2];
        var argument = argumentMatchGroup.Success ? argumentMatchGroup.Value : null;

        char[] chDelimiter = new char[] { ' ' };
        string[] strArray = null;

        if (command == "#START")
        {
            //#STARTと同時に鳴らすのはどうかと思うけどしゃーなしだな。
            AddMusicPreTimeMs(); // 音源を鳴らす前に遅延。
                                 /*
                                 var chip = new CChip();

                                 chip.nチャンネル番号 = 0x01;
                                 chip.n発声位置 = 384;
                                 chip.n発声時刻ms = (int)this.dbNowTime;
                                 chip.fBMSCROLLTime = this.dbNowBMScollTime;
                                 chip.n整数値 = 0x01;
                                 chip.n整数値_内部番号 = 1;

                                 // チップを配置。
                                 this.listChip.Add(chip);

                                 var chip1 = new CChip();
                                 chip1.nチャンネル番号 = 0x54;
                                 //chip1.n発声位置 = 384;
                                 //chip1.n発声時刻ms = (int)this.dbNowTime;
                                 if (this.nMOVIEOFFSET == 0)
                                     chip1.n発声時刻ms = (int)this.dbNowTime;
                                 else
                                     chip1.n発声時刻ms = (int)this.nMOVIEOFFSET;
                                 chip1.dbBPM = this.dbNowBPM;
                                 chip1.dbSCROLL = this.dbNowScroll;
                                 chip1.n整数値 = 0x01;
                                 chip1.n整数値_内部番号 = 1;
                                 chip1.eAVI種別 = EAVI種別.AVI;

                                 // チップを配置。

                                 this.listChip.Add(chip1);
                                 */

            chips.Add(1);
        }
        else if (command == "#END")
        {
            //ためしに割り込む。
            var chip = new CChip();
            chip.Type = CChip.CType.END;
            chip.StartChapter = current_chapter_index;
            //chip.n発声時刻ms = (int)( this.dbNowTime + ((15000.0 / this.dbNowBPM * ( 4.0 / 4.0 )) * 16.0) * 2  );
            chip.JudgeTime = (int)(this.dbNowTime + 1000 + nOFFSET); //2016.07.16 kairera0467 終了時から2秒後に設置するよう変更。
            // チップを配置。

            Others.Add(chip);
            
            chips.Add(1);
            //Debug.Log("add end at " + this.dbNowTime.ToString());
        }

        else if (command == "#BPMCHANGE")
        {
            float dbBPM = (float)Convert.ToDouble(argument);
            this.dbNowBPM = dbBPM;

            this.listBPM.Add(this.n内部番号BPM1to - 1,
                new CBPM() {
                    n内部番号 = this.n内部番号BPM1to - 1,
                    n表記上の番号 = 0,
                    dbBPM値 = dbBPM,
                    bpm_change_time = this.dbNowTime,
                    bpm_change_bmscroll_time = this.dbNowBMScollTime,
                    bpm_change_course = this.current_course,
                });

            CChip chip = new CChip
            {
                Type = CChip.CType.BPM_CHANGE,
                JudgeTime = (int)this.dbNowTime + this.nOFFSET,
                Bpm = dbBPM,
                StartChapter = current_chapter_index
            };
            Others.Add(chip);

            chips.Add(1);
            this.n内部番号BPM1to++;
        }
        else if (command == "#SCROLL")
        {
            //2016.08.13 kairera0467 複素数スクロールもどきのテスト
            if (argument.IndexOf('i') != -1)
            {
                //iが入っていた場合、複素数スクロールとみなす。

                double[] dbComplexNum = new double[2];
                this.tParsedComplexNumber(argument, ref dbComplexNum);

                this.dbNowScroll = (float)dbComplexNum[0];
                this.dbNowScrollY = (float)dbComplexNum[1];

                this.listSCROLL.Add(this.n内部番号SCROLL1to, new CSCROLL() { n内部番号 = this.n内部番号SCROLL1to, n表記上の番号 = 0, dbSCROLL値 = (float)dbComplexNum[0], dbSCROLL値Y = (float)dbComplexNum[1] });

                switch (this.current_course)
                {
                    case 0:
                        this.dbNowSCROLL_Normal[0] = dbComplexNum[0];
                        this.dbNowSCROLL_Normal[1] = dbComplexNum[1];
                        break;
                    case 1:
                        this.dbNowSCROLL_Expert[0] = dbComplexNum[0];
                        this.dbNowSCROLL_Expert[1] = dbComplexNum[1];
                        break;
                    case 2:
                        this.dbNowSCROLL_Master[0] = dbComplexNum[0];
                        this.dbNowSCROLL_Master[1] = dbComplexNum[1];
                        break;
                }
            }
            else
            {
                float dbSCROLL = (float)Convert.ToDouble(argument);
                this.dbNowScroll = dbSCROLL;
                this.dbNowScrollY = 0.0f;

                this.listSCROLL.Add(this.n内部番号SCROLL1to, new CSCROLL() { n内部番号 = this.n内部番号SCROLL1to, n表記上の番号 = 0, dbSCROLL値 = dbSCROLL, dbSCROLL値Y = 0.0f });

                switch (this.current_course)
                {
                    case 0:
                        this.dbNowSCROLL_Normal[0] = dbSCROLL;
                        break;
                    case 1:
                        this.dbNowSCROLL_Expert[0] = dbSCROLL;
                        break;
                    case 2:
                        this.dbNowSCROLL_Master[0] = dbSCROLL;
                        break;
                }
            }

            this.n内部番号SCROLL1to++;
        }
        else if (command == "#MEASURE")
        {
            strArray = argument.Split(new char[] { '/' });
            WarnSplitLength("#MEASURE subsplit", strArray, 2);

            double[] dbLength = new double[2];
            dbLength[0] = Convert.ToDouble(strArray[0]);
            dbLength[1] = Convert.ToDouble(strArray[1]);

            float db小節長倍率 = (float)(dbLength[0] / dbLength[1]);
            this.dbBarLength = db小節長倍率;
            this.fNow_Measure_m = (float)dbLength[1];
            this.fNow_Measure_s = (float)dbLength[0];
            //Debug.Log(string.Format("分子{0} 分母{1} 长度{2}", this.fNow_Measure_s, this.fNow_Measure_m, db小節長倍率));
        }
        else if (command == "#DELAY")
        {
            float nDELAY = (float)(Convert.ToDouble(argument) * 1000.0);
            this.listDELAY.Add(this.n内部番号DELAY1to,
                new CDELAY() {
                    n内部番号 = this.n内部番号DELAY1to,
                    n表記上の番号 = 0,
                    nDELAY値 = (int)nDELAY,
                    delay_bmscroll_time = this.dbLastBMScrollTime,
                    delay_bpm = this.dbNowBPM,
                    delay_course = this.current_course,
                    delay_time = this.dbLastTime
                });

            /*
                //チップ追加して割り込んでみる。
                var chip = new CChip();

                chip.nチャンネル番号 = 0xDC;
                chip.n発声位置 = ((this.n現在の小節数) * 384);
                chip.db発声時刻ms = this.dbNowTime;
                chip.nコース = this.n現在のコース;
                chip.n整数値_内部番号 = this.n内部番号DELAY1to;
                chip.fBMSCROLLTime = this.dbNowBMScollTime;
                // チップを配置。

                */
            this.dbNowTime += nDELAY;
            this.dbNowBMScollTime += nDELAY * this.dbNowBPM / 15000;

            this.n内部番号DELAY1to++;
        }

        else if (command == "#GOGOSTART")
        {
            CChip chip = new CChip
            {
                Type = CChip.CType.GOGO_TIME,
                JudgeTime = (int)this.dbNowTime + this.nOFFSET,
                StartChapter = current_chapter_index
            };
            Others.Add(chip);
        }
        else if (command == "#GOGOEND")
        {
            for (int i = Others.Count - 1; i >= 0; i--)
            {
                if (Others[i].Type == CChip.CType.GOGO_TIME)
                {
                    Others[i].EndChpater = current_chapter_index;
                    Others[i].EndTime = (int)this.dbNowTime + this.nOFFSET;
                    //Debug.Log("set gogo end at chapter " + current_chapter_index.ToString());
                    break;
                }
            }
        }
        else if (command == "#SECTION")
        {
            /*
            //分岐:条件リセット
            var chip = new CChip();

            chip.nチャンネル番号 = 0xDD;
            chip.n発声位置 = ((this.n現在の小節数 - 1) * 384);
            chip.n発声時刻ms = (int)this.dbNowTime;
            chip.n整数値_内部番号 = 1;
            chip.db発声時刻ms = this.dbNowTime;
            // チップを配置。
            this.listChip.Add(chip);
            */

            CChip chip = new CChip
            {
                Type = CChip.CType.BRANCH_RESET,
                JudgeTime = (int)this.dbNowTime + this.nOFFSET,
                StartChapter = current_chapter_index
            };
            Others.Add(chip);

            chips.Add(1);
            //Debug.Log("insert section");
        }
        else if (command == "#BRANCHSTART")
        {
            IsEndedBranching = false;
            /*
            this.bチップがある.Branch = true;
            this.b最初の分岐である = false;
            */

            //分岐:分岐スタート
            int n条件 = 0;

            //条件数値。
            double[] nNum = new double[2];

            var branchStartArgumentMatch = BranchStartArgumentRegex.Match(argument);
            if (!branchStartArgumentMatch.Success)
            {
                Debug.Log($"正常ではない.tjaファイルを読み込みました。 #BRANCHSTART 命令が正しく記述されていません。 ()");
                return;
            }

            nNum[0] = Convert.ToDouble(branchStartArgumentMatch.Groups[2].Value);
            nNum[1] = Convert.ToDouble(branchStartArgumentMatch.Groups[3].Value);
            switch (branchStartArgumentMatch.Groups[1].Value)
            {
                case "p":
                    n条件 = 0;
                    break;
                case "r":
                    n条件 = 1;
                    break;
                case "s":
                    n条件 = 2;
                    break;
                case "d":
                    n条件 = 3;
                    break;
                default:
                    n条件 = 0;
                    break;
            }

            if (ListBRANCH.Count > 0)
            {
                int last_end = ListBRANCH[ListBRANCH.Count - 1].EndChapter;
                if (last_end == 0)
                {
                    last_end = ListBRANCH[ListBRANCH.Count - 1].EndChapter = current_chapter_index;
                    foreach (CChip _chip in Others)
                    {
                        if (_chip.Type == CChip.CType.BRANCH_NOTICE || _chip.Type == CChip.CType.BRANCH_START)
                        {
                            if (_chip.BranchIndex == this.n内部番号BRANCH1to - 1)
                                _chip.EndChpater = current_chapter_index;
                        }
                    }
                }
            }

            int from_chapter = current_chapter_index - 1;
            int last_time = 0;
            int to_chapter = 0;
            int note_count = 0 , normal_count = 0, expert_count = 0, master_count = 0;
            if (ListBRANCH.Count > 0)
            {
                to_chapter = ListBRANCH[ListBRANCH.Count - 1].n現在の小節;
                last_time = (int)ListBRANCH[ListBRANCH.Count - 1].db判定時間;
            }

            for (int i = Others.Count - 1; i >= 0; i--)
            {
                CChip old = Others[i];
                if (old.StartChapter >= from_chapter) continue;
                if (old.Type == CChip.CType.BRANCH_RESET)
                {
                    if (old.JudgeTime >= last_time)
                        to_chapter = old.StartChapter;
                    break;
                }
            }
#if UNITY_EDITOR
            Debug.Log(string.Format("branch index {0} from {1} to {2}", n内部番号BRANCH1to, from_chapter, to_chapter));
#endif

            CChip chip = new CChip
            {
                Type = CChip.CType.BRANCH_NOTICE,
                JudgeTime =  (int)(this.dbNowTime - ((15000.0 / this.dbNowBPM * (this.fNow_Measure_s / this.fNow_Measure_m)) * 16.0)) + this.nOFFSET,
                BranchIndex = this.n内部番号BRANCH1to,
                StartChapter = current_chapter_index - 1
            };

            if (from_chapter - to_chapter - (to_chapter > 0 ? 1 : 0) == 1)
            {
                int n文字数 = 16;
                for (int i = 0; i < this.listLine.Count; i++)
                {
                    if (this.listLine[i].n小節番号 == from_chapter)
                    {
                        n文字数 = this.listLine[i].n文字数;
                        break;
                    }
                }
                //Debug.Log(n文字数);
                double mid = (15000.0 / this.dbNowBPM * (this.fNow_Measure_s / this.fNow_Measure_m) * (16.0 / n文字数));
                if (note_index > 0)
                {
                    NoteChip last = notes[note_index - 1];
                    if (last.Type >= 7)
                        chip.JudgeTime = (int)(last.EndTime + mid);
                    else if (last.Type >= 5)
                        chip.JudgeTime = (int)(last.EndTime + mid);
                    else
                        chip.JudgeTime = (int)(last.JudgeTime + mid);
                }
            }
            else if (from_chapter - to_chapter - (to_chapter > 0 ? 1 : 0) < 1)
                chip.JudgeTime = (int)(this.dbNowTime) + this.nOFFSET;

            Others.Add(chip);


            int branch_time = chip.JudgeTime;
            if (note_index > 0)
            {
                int n文字数 = 16;
                for (int i = 0; i < this.listLine.Count; i++)
                {
                    if (this.listLine[i].n小節番号 == from_chapter)
                    {
                        n文字数 = this.listLine[i].n文字数;
                        break;
                    }
                }
                //Debug.Log(n文字数);
                double mid = (15000.0 / this.dbNowBPM * (this.fNow_Measure_s / this.fNow_Measure_m) * (16.0 / n文字数));

                NoteChip last = notes[note_index - 1];
                if (last.Type >= 7 && last.EndTime > branch_time)
                    branch_time = (int)(last.EndTime + mid);
                else if (last.Type >= 5 && last.EndTime > branch_time)
                    branch_time = (int)(last.EndTime + mid);
                else if (last.JudgeTime > branch_time)
                    branch_time = (int)(last.JudgeTime + mid);
            }
#if UNITY_EDITOR
            Debug.Log(string.Format("branch notice at {0}, start at {1}", chip.JudgeTime, branch_time));
#endif
            CChip chip2 = new CChip
            {
                Type = CChip.CType.BRANCH_START,
                JudgeTime = branch_time,
                BranchIndex = this.n内部番号BRANCH1to,
                StartChapter = current_chapter_index
            };
            Others.Add(chip2);

            /*
            //分岐アニメ開始時(分岐の1小節前)に設置。
            var chip = new CChip();

            chip.nチャンネル番号 = 0xDE;
            chip.n発声位置 = ((this.n現在の小節数 - 1) * 384);
            chip.n発声時刻ms = (int)(this.dbNowTime - ((15000.0 / this.dbNowBPM * (this.fNow_Measure_s / this.fNow_Measure_m)) * 16.0)); //ここの時間設定は前の小節の開始時刻である必要があるのだが...
                                                                                                                                     //chip.n発声時刻ms = (int)this.dbLastTime;
            chip.dbSCROLL = this.dbNowScroll;
            chip.dbBPM = this.dbNowBPM;
            chip.n整数値_内部番号 = this.n内部番号BRANCH1to;

            // チップを配置。
            this.listChip.Add(chip);

            //実質的な位置に配置
            var chip2 = new CChip();

            chip2.nチャンネル番号 = 0xDF;
            chip2.n発声位置 = ((this.n現在の小節数) * 384);
            chip2.n発声時刻ms = (int)this.dbNowTime;
            chip2.dbSCROLL = this.dbNowScroll;
            chip2.dbBPM = this.dbNowBPM;
            chip2.n整数値_内部番号 = this.n内部番号BRANCH1to;

            this.listChip.Add(chip2);
            */
            chips.Add(1);
            //Debug.Log(string.Format("insert branch at {0}, animation at {1}", this.dbNowTime, chip.JudgeTime));

            foreach (NoteChip note in notes.Values)
            {
                if (note.Chapter >= to_chapter && note.JudgeTime < chip.JudgeTime && note.Type < 5)
                {
                    if (!note.BranchRelated)
                        note_count++;
                    else
                    {
                        switch (note.Branch)
                        {
                            case CourseBranch.BranchNormal:
                                normal_count++;
                                break;
                            case CourseBranch.BranchExpert:
                                expert_count++;
                                break;
                            case CourseBranch.BranchMaster:
                                master_count++;
                                break;
                        }
                    }
                }
            }
            //まずはリストに現在の小節、発声位置、分岐条件を追加。
            var branch = new CBRANCH
            {
                db判定時間 = this.dbNowTime + nOFFSET,
                db分岐時間 = ((this.current_chapter_index + 1) * 384),
                db分岐時間ms = this.dbNowTime, //ここがうまく計算できてないので後からバグが出る。
                                           //branch.db分岐時間ms = this.dbNowTime + ((((60.0 / this.dbNowBPM) / 4.0 ) * 16.0) * 1000.0);
                dbBPM = this.dbNowBPM,
                dbSCROLL = this.dbNowScroll,
                dbBMScrollTime = this.dbNowBMScollTime,
                n現在の小節 = this.current_chapter_index,
                n条件数値A = (float)nNum[0],
                n条件数値B = (float)nNum[1],
                n内部番号 = this.n内部番号BRANCH1to,
                n表記上の番号 = 0,
                n分岐の種類 = n条件,
                NoteCount = note_count,
                BranchCount = new Dictionary<CourseBranch, int>
                {
                    { CourseBranch.BranchNormal, normal_count },
                    { CourseBranch.BranchExpert, expert_count },
                    { CourseBranch.BranchMaster, master_count },
                }
                //n命令時のChipList番号 = this.listChip.Count
            };
            //Debug.Log(string.Format("branch index {0} notes count {1} expert_count {2} master count {3}", n内部番号BRANCH1to, note_count, expert_count, master_count));

            ListBRANCH.Add(n内部番号BRANCH1to, branch);

            this.n内部番号BRANCH1to++;
        }
        else if (command == "#N")
        {
            //分岐:普通譜面
            this.current_course = 0;
            if (!ListBRANCH.TryGetValue(this.n内部番号BRANCH1to - 1, out var branch))
            {
                Debug.Log($"正常ではない.tjaファイルを読み込みました。 #N 命令がありません。 ()");
                return;
            }
            this.current_chapter_index = branch.n現在の小節;
            this.dbNowTime = branch.db分岐時間ms;
            this.dbNowBPM = branch.dbBPM;
            this.dbNowScroll = branch.dbSCROLL;
            this.dbNowBMScollTime = branch.dbBMScrollTime;
        }
        else if (command == "#E")
        {
            //分岐:玄人譜面
            this.current_course = 1;
            if (!ListBRANCH.TryGetValue(this.n内部番号BRANCH1to - 1, out var branch))
            {
                Debug.Log($"正常ではない.tjaファイルを読み込みました。 #E 命令がありません。 ()");
                return;
            }
            this.current_chapter_index = branch.n現在の小節;
            this.dbNowTime = branch.db分岐時間ms;
            this.dbNowBPM = branch.dbBPM;
            this.dbNowScroll = branch.dbSCROLL;
            this.dbNowBMScollTime = branch.dbBMScrollTime;
        }
        else if (command == "#M")
        {
            //分岐:達人譜面
            this.current_course = 2;
            if (!ListBRANCH.TryGetValue(this.n内部番号BRANCH1to - 1, out var branch))
            {
                Debug.Log($"正常ではない.tjaファイルを読み込みました。 #M 命令がありません。 ()");
                return;
            }
            this.current_chapter_index = branch.n現在の小節;
            this.dbNowTime = branch.db分岐時間ms;
            this.dbNowBPM = branch.dbBPM;
            this.dbNowScroll = branch.dbSCROLL;
            this.dbNowBMScollTime = branch.dbBMScrollTime;
        }
        else if (command == "#LEVELHOLD")
        {
            var chip = new CChip();
            chip.Type = CChip.CType.LEVEL_HOLD;
            chip.JudgeTime = (int)this.dbNowTime + this.nOFFSET;
            chip.Branch = (CourseBranch)this.current_course;
            chip.StartChapter = current_chapter_index;
            Others.Add(chip);

            chips.Add(1);
#if UNITY_EDITOR
            Debug.Log("insert levelhold at chapter " + current_chapter_index.ToString());
#endif
        }
        else if (command == "#BRANCHEND")
        {
            IsEndedBranching = true;
            ListBRANCH[ListBRANCH.Count - 1].EndChapter = current_chapter_index;

            foreach (CChip chip in Others)
            {
                if (chip.Type == CChip.CType.BRANCH_NOTICE || chip.Type == CChip.CType.BRANCH_START) 
                {
                    if (chip.BranchIndex == this.n内部番号BRANCH1to - 1)
                        chip.EndChpater = current_chapter_index;

                    //Debug.Log(string.Format("branch end at chapter {0}", chip.EndChpater));
                }
            }
        }
        else if (command == "#BARLINEOFF")
        {
            var chip = new CChip();
            chip.Type = CChip.CType.CHAPTERLINE_OFF;
            chip.JudgeTime = (int)this.dbNowTime + 1 + this.nOFFSET;
            chip.Branch = (CourseBranch)this.current_course;
            chip.StartChapter = current_chapter_index;
            Others.Add(chip);

            chips.Add(1);
            //Debug.Log("insert BarlineOff at chapter " + current_chapter_index.ToString());
        }
        else if (command == "#BARLINEON")
        {
            for (int i = Others.Count - 1; i > 0; i--)
            {
                CChip chip1 = Others[i];
                if (chip1.Type == CChip.CType.CHAPTERLINE_OFF)
                {
                    chip1.EndChpater = current_chapter_index;
                    break;
                }
            }

            //Debug.Log("insert BarlineOn at chapter " + current_chapter_index.ToString());
        }
        else if (command == "#LYRIC")
        {
            /*
            this.listLiryc.Add(argument);

            var chip = new CChip();

            chip.nチャンネル番号 = 0xF1;
            chip.n発声時刻ms = (int)this.dbNowTime;
            chip.n整数値_内部番号 = 0;
            chip.nコース = this.current_course;

            // チップを配置。

            this.listChip.Add(chip);
            */

            chips.Add(1);
        }
        else if (command == "#DIRECTION")
        {
            double dbSCROLL = Convert.ToDouble(argument);
            /*
            this.nスクロール方向 = (int)dbSCROLL;

            //チップ追加して割り込んでみる。
            var chip = new CChip();

            chip.nチャンネル番号 = 0xF2;
            chip.n発声位置 = ((this.current_chapter_index) * 384) - 1;
            chip.n発声時刻ms = (int)this.dbNowTime;
            chip.n整数値_内部番号 = 0;
            chip.nスクロール方向 = (int)dbSCROLL;
            chip.nコース = this.current_course;

            // チップを配置。

            this.listChip.Add(chip);
            */

            chips.Add(1);
        }
        else if (command == "#SUDDEN")
        {
            strArray = argument.Split(chDelimiter);
            WarnSplitLength("#SUDDEN", strArray, 2);
            double db出現時刻 = Convert.ToDouble(strArray[0]);
            double db移動待機時刻 = Convert.ToDouble(strArray[1]);

            this.AppearTime = db出現時刻;
            this.WaitingTime = db移動待機時刻;
            /*
            //チップ追加して割り込んでみる。
            var chip = new CChip();

            chip.nチャンネル番号 = 0xF3;
            chip.n発声位置 = ((this.n現在の小節数) * 384) - 1;
            chip.n発声時刻ms = (int)this.dbNowTime;
            chip.n整数値_内部番号 = 0;
            chip.nノーツ出現時刻ms = (int)this.db出現時刻;
            chip.nノーツ移動開始時刻ms = (int)this.db移動待機時刻;
            chip.nコース = this.current_course;

            // チップを配置。

            this.listChip.Add(chip);
            */

            chips.Add(1);
        }
        else if (command == "#JPOSSCROLL")
        {
            /*
            strArray = argument.Split(chDelimiter);
            WarnSplitLength("#JPOSSCROLL", strArray, 3);
            double db移動時刻 = Convert.ToDouble(strArray[0]);
            int n移動px = Convert.ToInt32(strArray[1]);
            int n移動方向 = Convert.ToInt32(strArray[2]);

            //チップ追加して割り込んでみる。
            var chip = new CChip();

            chip.nチャンネル番号 = 0xE2;
            chip.n発声位置 = ((this.n現在の小節数) * 384) - 1;
            chip.n発声時刻ms = (int)this.dbNowTime;
            chip.n整数値_内部番号 = 0;
            chip.nコース = this.current_course;

            // チップを配置。

            this.listJPOSSCROLL.Add(this.n内部番号JSCROLL1to, new CJPOSSCROLL() { n内部番号 = this.n内部番号JSCROLL1to, n表記上の番号 = 0, db移動時間 = db移動時刻, n移動距離px = n移動px, n移動方向 = n移動方向 });
            this.listChip.Add(chip);
            this.n内部番号JSCROLL1to++;
            */
        }
        else if (command == "#SENOTECHANGE")
        {
            FixSENote = int.Parse(argument);
            IsEnabledFixSENote = true;
        }
        else if (command == "#NEXTSONG")
        {
            /*
            var delayTime = 6200.0; // 6.2秒ディレイ
                                    //チップ追加して割り込んでみる。
            var chip = new CChip();

            chip.nチャンネル番号 = 0x9B;
            chip.n発声位置 = ((this.n現在の小節数) * 384) - 1;
            chip.n発声時刻ms = (int)this.dbNowTime;
            this.dbNowTime += delayTime;
            this.dbNowBMScollTime += delayTime * this.dbNowBPM / 15000;
            chip.n整数値_内部番号 = 0;
            chip.nコース = this.current_course;

            // チップを配置。
            this.listChip.Add(chip);

            AddMusicPreTimeMs(); // 段位の幕が開いてからの遅延。

            strArray = SplitComma(argument); // \,をエスケープ処理するメソッドだぞっ

            WarnSplitLength("#NEXTSONG", strArray, 6);
            var dansongs = new DanSongs();
            dansongs.Title = strArray[0];
            dansongs.SubTitle = strArray[1];
            dansongs.Genre = strArray[2];
            dansongs.FileName = strArray[3];
            dansongs.ScoreInit = int.Parse(strArray[4]);
            dansongs.ScoreDiff = int.Parse(strArray[5]);
            dansongs.Wave = new CWAV
            {
                n内部番号 = this.n内部番号WAV1to,
                n表記上の番号 = this.n内部番号WAV1to,
                nチップサイズ = this.n無限管理SIZE[this.n内部番号WAV1to],
                n位置 = this.n無限管理PAN[this.n内部番号WAV1to],
                SongVol = this.SongVol,
                SongLoudnessMetadata = this.SongLoudnessMetadata,
                strファイル名 = CDTXCompanionFileFinder.FindFileName(this.strフォルダ名, strファイル名, dansongs.FileName),
                strコメント文 = "TJA BGM"
            };
            dansongs.Wave.SongLoudnessMetadata = LoudnessMetadataScanner.LoadForAudioPath(dansongs.Wave.strファイル名);
            List_DanSongs.Add(dansongs);
            this.listWAV.Add(this.n内部番号WAV1to, dansongs.Wave);
            this.n内部番号WAV1to++;

            var nextSongnextSongChip = new CChip();

            nextSongnextSongChip.nチャンネル番号 = 0x01;
            nextSongnextSongChip.n発声位置 = 384;
            nextSongnextSongChip.n発声時刻ms = (int)this.dbNowTime;
            nextSongnextSongChip.n整数値 = 0x01;
            nextSongnextSongChip.n整数値_内部番号 = 1 + List_DanSongs.Count;

            this.listWAV[1].strファイル名 = "";

            // チップを配置。
            this.listChip.Add(nextSongnextSongChip);
            */
        }
    }
    
    /// <summary>
    /// 音源再生前の空白を追加するメソッド。
    /// </summary>
    private void AddMusicPreTimeMs()
    {
        this.dbNowTime += 1000;
        this.dbNowBMScollTime += 1000 * this.dbNowBPM / 15000;
    }
    //-----------------

    /// <summary>
    /// 複素数のパースもどき
    /// </summary>
    private void tParsedComplexNumber(string strScroll, ref double[] dbScroll)
    {
        bool bFirst = true; //最初の数値か
        string[] arScroll = new string[2];
        char[] c = strScroll.ToCharArray();
        //1.0-1.0i
        for (int i = 0; i < strScroll.Length; i++)
        {
            if (bFirst)
                arScroll[0] += c[i];
            else
                arScroll[1] += c[i];

            //次の文字が'i'なら脱出。
            if (c[i + 1] == 'i')
                break;
            else if (c[i + 1] == '-' || c[i + 1] == '+')
                bFirst = false;

        }

        dbScroll[0] = Convert.ToDouble(arScroll[0]);
        dbScroll[1] = Convert.ToDouble(arScroll[1]);
        return;
    }

    /// <summary>
    /// 譜面分岐がある場合はこちらを使う
    /// </summary>
    protected void tSetSenotes_branch(List<NoteChip> notes)
    {
        #region[ list作成 ]
        //ひとまずチップだけのリストを作成しておく。
        List<NoteChip> list音符のみのリスト;
        List<NoteChip> list普通譜面のみのリスト;
        List<NoteChip> list玄人譜面のみのリスト;
        List<NoteChip> list達人譜面のみのリスト;
        list音符のみのリスト = new List<NoteChip>();
        list普通譜面のみのリスト = new List<NoteChip>();
        list玄人譜面のみのリスト = new List<NoteChip>();
        list達人譜面のみのリスト = new List<NoteChip>();

        foreach (NoteChip chip in notes)
        {
            if (!chip.BranchRelated)
            {
                list音符のみのリスト.Add(chip);
            }
            else
            { 
                switch (chip.Branch)
                {
                    case CourseBranch.BranchNormal:
                        list普通譜面のみのリスト.Add(chip);
                        break;
                    case CourseBranch.BranchExpert:
                        list玄人譜面のみのリスト.Add(chip);
                        break;
                    case CourseBranch.BranchMaster:
                        list達人譜面のみのリスト.Add(chip);
                        break;
                }
            }
        }
        #endregion

        //forで処理。
        for (int n = 0; n < 3; n++)
        {
            switch (n)
            {
                case 0:
                    list音符のみのリスト = list普通譜面のみのリスト;
                    break;
                case 1:
                    list音符のみのリスト = list玄人譜面のみのリスト;
                    break;
                case 2:
                    list音符のみのリスト = list達人譜面のみのリスト;
                    break;
            }

            //this.tSenotes_Core( list音符のみのリスト );
            this.tSenotes_Core_V2(list音符のみのリスト, true);
        }
    }

    private void tSenotes_Core_V2(List<NoteChip> list音符のみのリスト, bool ignoreSENote = false)
    {
        const int DATA = 3;
        int doco_count = 0;
        int[] sort = new int[7];
        double[] time = new double[7];
        double[] scroll = new double[7];
        double time_tmp;

        for (int i = 0; i < list音符のみのリスト.Count; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                if (i + (j - 3) < 0)
                {
                    sort[j] = -1;
                    time[j] = -1000000000;
                    scroll[j] = 1.0;
                }
                else if (i + (j - 3) >= list音符のみのリスト.Count)
                {
                    sort[j] = -1;
                    time[j] = 1000000000;
                    scroll[j] = 1.0;
                }
                else
                {
                    sort[j] = list音符のみのリスト[i + (j - 3)].Type;
                    time[j] = list音符のみのリスト[i + (j - 3)].fBMSCROLLTime;
                    scroll[j] = list音符のみのリスト[i + (j - 3)].Scroll;
                }
            }
            time_tmp = time[DATA];
            for (int j = 0; j < 7; j++)
            {
                time[j] = (time[j] - time_tmp) * scroll[j];
                if (time[j] < 0) time[j] *= -1;
            }

            if (ignoreSENote && list音符のみのリスト[i].IsFixedSENote) continue;

            switch (list音符のみのリスト[i].Type)
            {
                case 1:
                    if (doco_count != 0 && time[DATA - 1] == 2 && time[DATA + 1] == 2 && (sort[DATA + 1] == 1 || sort[DATA + 1] == 1))
                    {
                        if (doco_count % 2 == 0)
                            list音符のみのリスト[i].Senote = 1;
                        else
                            list音符のみのリスト[i].Senote = 2;
                        doco_count++;
                        break;
                    }
                    else
                    {
                        doco_count = 0;
                    }

                    //8分ドコドン
                    if ((time[DATA - 2] >= 4.1 && time[DATA - 1] == 2 && time[DATA + 1] == 2 && time[DATA + 2] >= 4.1) && (sort[DATA - 1] == 1 && sort[DATA + 1] == 1))
                    {
                        if (list音符のみのリスト[i].Bpm >= 120.0)
                        {
                            list音符のみのリスト[i - 1].Senote = 1;
                            list音符のみのリスト[i].Senote = 2;
                            list音符のみのリスト[i + 1].Senote = 0;
                            break;
                        }
                        else if (list音符のみのリスト[i].Bpm < 120.0)
                        {
                            list音符のみのリスト[i - 1].Senote = 0;
                            list音符のみのリスト[i].Senote = 0;
                            list音符のみのリスト[i + 1].Senote = 0;
                            break;
                        }
                    }

                    //BPM120以下のみ
                    //8分間隔の「ドドド」→「ドンドンドン」

                    if (time[DATA - 1] >= 2 && time[DATA + 1] >= 2)
                    {
                        if (list音符のみのリスト[i].Bpm < 120.0)
                        {
                            list音符のみのリスト[i].Senote = 0;
                            break;
                        }
                    }

                    //ドコドコドン
                    if (time[DATA - 2] >= 2.4 && time[DATA - 1] == 1 && time[DATA + 1] == 1 && time[DATA + 2] >= 2.4 && sort[DATA - 1] == 1 && sort[DATA + 1] == 1)
                    {
                        list音符のみのリスト[i].Senote = 2;
                    }
                    //右の音符が2以上離れている
                    else if (time[DATA + 1] > 2)
                    {
                        list音符のみのリスト[i].Senote = 0;
                    }
                    //右の音符が1.4以上_左の音符が1.4以内
                    else if (time[DATA + 1] >= 1.4 && time[DATA - 1] <= 1.4)
                    {
                        list音符のみのリスト[i].Senote = 0;
                    }
                    //右の音符が2以上_右右の音符が3以内
                    else if (time[DATA + 1] >= 2 && time[DATA + 2] <= 3)
                    {
                        list音符のみのリスト[i].Senote = 0;
                    }
                    //右の音符が2以上_大音符
                    else if (time[DATA + 1] >= 2 && (sort[DATA + 1] == 2 || sort[DATA + 1] == 4))
                    {
                        list音符のみのリスト[i].Senote = 0;
                    }
                    else
                    {
                        list音符のみのリスト[i].Senote = 1;
                    }
                    break;
                case 2:
                    doco_count = 0;

                    //BPM120以下のみ
                    //8分間隔の「ドドド」→「ドンドンドン」
                    if (time[DATA - 1] == 2 && time[DATA + 1] == 2)
                    {
                        if (list音符のみのリスト[i - 1].Bpm < 120.0 && list音符のみのリスト[i].Bpm < 120.0 && list音符のみのリスト[i + 1].Bpm < 120.0)
                        {
                            list音符のみのリスト[i].Senote = 3;
                            break;
                        }
                    }

                    //右の音符が2以上離れている
                    if (time[DATA + 1] > 2)
                    {
                        list音符のみのリスト[i].Senote = 3;
                    }
                    //右の音符が1.4以上_左の音符が1.4以内
                    else if (time[DATA + 1] >= 1.4 && time[DATA - 1] <= 1.4)
                    {
                        list音符のみのリスト[i].Senote = 3;
                    }
                    //右の音符が2以上_右右の音符が3以内
                    else if (time[DATA + 1] >= 2 && time[DATA + 2] <= 3)
                    {
                        list音符のみのリスト[i].Senote = 3;
                    }
                    //右の音符が2以上_大音符
                    else if (time[DATA + 1] >= 2 && (sort[DATA + 1] == 2 || sort[DATA + 1] == 4))
                    {
                        list音符のみのリスト[i].Senote = 3;
                    }
                    else
                    {
                        list音符のみのリスト[i].Senote = 4;
                    }
                    break;
                default:
                    doco_count = 0;
                    break;
            }

            //Debug.Log(string.Format("type {0} senote {1}", list音符のみのリスト[i].Type, list音符のみのリスト[i].Senote));
        }
    }

    protected void tSetSenotes(List<NoteChip> notes)
    {
        //時間判定は、「次のチップの発声時刻」から「現在(過去)のチップの発声時刻」で引く必要がある。
        //逆にしてしまうと計算がとてつもないことになるので注意。
        try
        {
            this.tSenotes_Core_V2(notes, true);
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            Trace.TraceError("例外が発生しましたが処理を継続します。 (b67473e4-1930-44f1-b320-4ead5786e74c)");
        }
    }
}
