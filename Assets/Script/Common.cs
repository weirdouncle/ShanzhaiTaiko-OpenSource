using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using static GameSetting;

namespace CommonClass
{
    public static class JsonUntity
    {
        public static string Object2Json<T>(T t, bool indented = true)
        {
            if (indented)
            {
                string jsonStr = JsonConvert.SerializeObject(t, Formatting.Indented);
                return jsonStr;
            }
            else
            {
                string jsonStr = JsonConvert.SerializeObject(t);
                return jsonStr;
            }
        }
        public static T Json2Object<T>(string jsonStr)
        {
            T result = JsonConvert.DeserializeObject<T>(jsonStr);
            return result;
        }

        public static string DataSet2Json(DataSet dataSet)
        {
            string json = JsonConvert.SerializeObject(dataSet, Formatting.Indented);
            return json;
        }

        public static List<T> Json2List<T>(string jsonStr)
        {
            List<T> result = JsonConvert.DeserializeObject<List<T>>(jsonStr);
            return result;
        }

        public static List<string> IntList2StringList(List<int> list)
        {
            List<string> stringList = new List<string>();
            foreach (int num in list)
                stringList.Add(num.ToString());

            return stringList;
        }

        public static List<int> StringList2IntList(List<string> list)
        {
            List<int> intList = new List<int>();
            foreach (string str in list)
            {
                if (int.TryParse(str, out int num))
                    intList.Add(num);
            }

            return intList;
        }
    }

    public enum Difficulty
    {
        Easy,
        Normal,
        Hard,
        Oni,
        Edit,
        Tower,
        Dan,
        Total
    }

    public class CDELAY
    {
        public int nDELAY値; //格納時にはmsになっているため、doubleにはしない。
        public int n内部番号;
        public int n表記上の番号;
        public float delay_time;
        public float delay_bmscroll_time;
        public float delay_bpm;
        public int delay_course;

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(0x80);
            if (this.n内部番号 != this.n表記上の番号)
            {
                builder.Append(string.Format("CDELAY{0}(内部{1})", LoaderScript.tZZ(this.n表記上の番号), this.n内部番号));
            }
            else
            {
                builder.Append(string.Format("CDELAY{0}", LoaderScript.tZZ(this.n表記上の番号)));
            }
            builder.Append(string.Format(", DELAY:{0}", this.nDELAY値));
            return builder.ToString();
        }
    }
    public class CBRANCH
    {
        public int n分岐の種類 { set; get; } //0:精度分岐 1:連打分岐 2:スコア分岐 3:大音符のみの精度分岐
        public float n条件数値A { set; get; }
        public float n条件数値B { set; get; }
        public float db分岐時間 { set; get; }
        public float db分岐時間ms { set; get; }
        public float db判定時間 { set; get; }
        public float dbBMScrollTime { set; get; }
        public float dbBPM { set; get; }
        public float dbSCROLL { set; get; }
        public int n現在の小節 { set; get; }
        public int n命令時のChipList番号 { set; get; }
        public int EndChapter { set; get; }

        public int n表記上の番号 { set; get; }
        public int n内部番号 { set; get; }
        public int NoteCount { set; get; }
        public Dictionary<CourseBranch, int> BranchCount { set; get; }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(0x80);
            if (this.n内部番号 != this.n表記上の番号)
            {
                builder.Append(string.Format("CBRANCH{0}(内部{1})", LoaderScript.tZZ(this.n表記上の番号), this.n内部番号));
            }
            else
            {
                builder.Append(string.Format("CBRANCH{0}", LoaderScript.tZZ(this.n表記上の番号)));
            }
            builder.Append(string.Format(", BRANCH:{0}", this.n分岐の種類));
            return builder.ToString();
        }
    }

    public class CLine
    {
        public int n小節番号;
        public int n文字数;
        public float db発声時刻;
        public float dbBMS時刻;
        public int nコース;
        public int nタイプ;
    }
    public class CBPM
    {
        public float dbBPM値;
        public float bpm_change_time;
        public float bpm_change_bmscroll_time;
        public int bpm_change_course;
        public int n内部番号;
        public int n表記上の番号;

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(0x80);
            if (this.n内部番号 != this.n表記上の番号)
            {
                builder.Append(string.Format("CBPM{0}(内部{1})", LoaderScript.tZZ(this.n表記上の番号), this.n内部番号));
            }
            else
            {
                builder.Append(string.Format("CBPM{0}", LoaderScript.tZZ(this.n表記上の番号)));
            }
            builder.Append(string.Format(", BPM:{0}", this.dbBPM値));
            return builder.ToString();
        }
    }
    public class CSCROLL
    {
        public float dbSCROLL値;
        public float dbSCROLL値Y;
        public int n内部番号;
        public int n表記上の番号;

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(0x80);
            if (this.n内部番号 != this.n表記上の番号)
            {
                builder.Append(string.Format("CSCROLL{0}(内部{1})", LoaderScript.tZZ(this.n表記上の番号), this.n内部番号));
            }
            else
            {
                builder.Append(string.Format("CSCROLL{0}", LoaderScript.tZZ(this.n表記上の番号)));
            }
            builder.Append(string.Format(", SCROLL:{0}", this.dbSCROLL値));
            return builder.ToString();
        }
    }

    public class CharConverter
    {
        // プロパティ

        public static readonly string str16進数文字 = "0123456789ABCDEFabcdef";
        public static readonly string str36進数文字 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";


        // メソッド

        public static bool bONorOFF(char c)
        {
            return (c != '0');
        }

        public static double DegreeToRadian(double angle)
        {
            return ((Math.PI * angle) / 180.0);
        }
        public static double RadianToDegree(double angle)
        {
            return (angle * 180.0 / Math.PI);
        }
        public static float DegreeToRadian(float angle)
        {
            return (float)DegreeToRadian((double)angle);
        }
        public static float RadianToDegree(float angle)
        {
            return (float)RadianToDegree((double)angle);
        }

        public static int n値を範囲内に丸めて返す(int n値, int n最小値, int n最大値)
        {
            if (n値 < n最小値)
                return n最小値;

            if (n値 > n最大値)
                return n最大値;

            return n値;
        }
        public static int n値を文字列から取得して範囲内に丸めて返す(string str数値文字列, int n最小値, int n最大値, int n取得失敗時のデフォルト値)
        {
            int num;
            if ((int.TryParse(str数値文字列, out num) && (num >= n最小値)) && (num <= n最大値))
                return num;

            return n取得失敗時のデフォルト値;
        }

        public static double db値を文字列から取得して範囲内に丸めて返す(string str数値文字列, double db最小値, double db最大値, double db取得失敗時のデフォルト値)
        {
            double num;
            if ((double.TryParse(str数値文字列, out num) && (num >= db最小値)) && (num <= db最大値))
                return num;

            return db取得失敗時のデフォルト値;
        }

        // #23568 2010.11.04 ikanick add
        public static int n値を文字列から取得して範囲内にちゃんと丸めて返す(string str数値文字列, int n最小値, int n最大値, int n取得失敗時のデフォルト値)
        {
            // 1 と違って範囲外の場合ちゃんと丸めて返します。
            int num;
            if (int.TryParse(str数値文字列, out num))
            {
                if ((num >= n最小値) && (num <= n最大値))
                    return num;
                if (num < n最小値)
                    return n最小値;
                if (num > n最大値)
                    return n最大値;
            }

            return n取得失敗時のデフォルト値;
        }
        // --------------------ここまで-------------------------/
        public static int n値を文字列から取得して返す(string str数値文字列, int n取得失敗時のデフォルト値)
        {
            int num;
            if (!int.TryParse(str数値文字列, out num))
                num = n取得失敗時のデフォルト値;

            return num;
        }

        public static int n16進数2桁の文字列を数値に変換して返す(string strNum)
        {
            if (strNum.Length < 2)
                return -1;

            int digit2 = str16進数文字.IndexOf(strNum[0]);
            if (digit2 < 0)
                return -1;

            if (digit2 >= 16)
                digit2 -= (16 - 10);        // A,B,C... -> 1,2,3...

            int digit1 = str16進数文字.IndexOf(strNum[1]);
            if (digit1 < 0)
                return -1;

            if (digit1 >= 16)
                digit1 -= (16 - 10);

            return digit2 * 16 + digit1;
        }
        public static int n36進数2桁の文字列を数値に変換して返す(string strNum)
        {
            if (strNum.Length < 2)
                return -1;

            int digit2 = str36進数文字.IndexOf(strNum[0]);
            if (digit2 < 0)
                return -1;

            if (digit2 >= 36)
                digit2 -= (36 - 10);        // A,B,C... -> 1,2,3...

            int digit1 = str36進数文字.IndexOf(strNum[1]);
            if (digit1 < 0)
                return -1;

            if (digit1 >= 36)
                digit1 -= (36 - 10);

            return digit2 * 36 + digit1;
        }
        public static int n小節番号の文字列3桁を数値に変換して返す(string strNum)
        {
            if (strNum.Length >= 3)
            {
                int digit3 = str36進数文字.IndexOf(strNum[0]);
                if (digit3 < 0)
                    return -1;

                if (digit3 >= 36)                                   // 3桁目は36進数
                    digit3 -= (36 - 10);

                int digit2 = str16進数文字.IndexOf(strNum[1]);  // 2桁目は10進数
                if ((digit2 < 0) || (digit2 > 9))
                    return -1;

                int digit1 = str16進数文字.IndexOf(strNum[2]);  // 1桁目も10進数
                if ((digit1 >= 0) && (digit1 <= 9))
                    return digit3 * 100 + digit2 * 10 + digit1;
            }
            return -1;
        }

        public static string str小節番号を文字列3桁に変換して返す(int num)
        {
            if ((num < 0) || (num >= 3600)) // 3600 == Z99 + 1
                return "000";

            int digit4 = num / 100;
            int digit2 = (num % 100) / 10;
            int digit1 = (num % 100) % 10;
            char ch3 = str36進数文字[digit4];
            char ch2 = str16進数文字[digit2];
            char ch1 = str16進数文字[digit1];
            return (ch3.ToString() + ch2.ToString() + ch1.ToString());
        }
        public static string str数値を16進数2桁に変換して返す(int num)
        {
            if ((num < 0) || (num >= 0x100))
                return "00";

            char ch2 = str16進数文字[num / 0x10];
            char ch1 = str16進数文字[num % 0x10];
            return (ch2.ToString() + ch1.ToString());
        }
        public static string str数値を36進数2桁に変換して返す(int num)
        {
            if ((num < 0) || (num >= 36 * 36))
                return "00";

            char ch2 = str36進数文字[num / 36];
            char ch1 = str36進数文字[num % 36];
            return (ch2.ToString() + ch1.ToString());
        }

        public static int[] ar配列形式のstringをint配列に変換して返す(string str)
        {
            //0,1,2 ...の形式で書かれたstringをint配列に変換する。
            //一応実装はしたものの、例外処理などはまだ完成していない。
            //str = "0,1,2";
            if (String.IsNullOrEmpty(str))
                return null;

            string[] strArray = str.Split(',');
            List<int> listIntArray;
            listIntArray = new List<int>();

            for (int n = 0; n < strArray.Length; n++)
            {
                int n追加する数値 = Convert.ToInt32(strArray[n]);
                listIntArray.Add(n追加する数値);
            }
            int[] nArray = new int[] { 1 };
            nArray = listIntArray.ToArray();

            return nArray;
        }


        /// <summary>
        /// 百分率数値を255段階数値に変換するメソッド。透明度用。
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static int nParsentTo255(double num)
        {
            return (int)(255.0 * num);
        }

        /// <summary>
        /// 255段階数値を百分率に変換するメソッド。
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static int n255ToParsent(int num)
        {
            return (int)(100.0 / num);
        }


        #region [ private ]
        //-----------------

        // private コンストラクタでインスタンス生成を禁止する。
        private CharConverter()
        {
        }
        //-----------------
        #endregion
    }

    public enum HitNoteResult
    {
        Bad,
        Good,
        Perfect,
        None,
    }
    public enum HitPracticeResult
    {
        None,
        Bad,
        Lost,
        Fast,
        Late,
        Perfect,
    }

    public enum PlayMode
    {
        Normal,
        Practice,
        Replay,
        PlayWithReplay,
    }

    public enum Locale
    {
        Chinese,
        English,
        Japanese,
        Korean,
        Spanish,
    }

    public enum Special
    {
        None,
        AutoPlay,
        AllPerfect,
        Training,
        DAllPerfect,
        DTraining,
    }

    public enum GameStyle
    {
        TraditionalStyle,
        Ps4Style,
        NijiiroStyle
    }

    public enum Score
    {
        Normal,
        Shin,
        Nijiiro
    }

    public enum NijiiroRank
    {
        None,
        Level1,
        Level2,
        Level3,
        Level4,
        Level5,
        Level6,
        Level7,
    }

    public enum Speed
    {
        Normal = 1,
        Double = 2,
        Triple = 3,
        Quadruple = 4,
        potion1 = 11,
        potion2 = 12,
        potion3 = 13,
        potion4 = 14,
        potion5 = 15,
    }

    public enum RandomType
    {
        None = 0,
        Normal = 1,
        More = 2,
    }

    public class SongInfo
    {
        public string Title { set; get; }
        public string SubTitle { set; get; }
        public string Path { set; get; }
        public string SoundPath { set; get; }
        public int SoundPlayOffset { set; get; }
        public Dictionary<Difficulty, bool> Branches { set; get; }
        public List<Difficulty> Difficulties { set; get; }
        public Dictionary<Difficulty, int> Levels { set; get; }
        public string GENRE { set; get; }
        public string Comment { set; get; }
        public bool HiddenLevel { set; get; }
        public int Volume { set; get; } = 100;
        public string Stage { set; get; } = string.Empty;
        public string ARTIST { set; get; }
        public bool Lyrics { set; get; }
        public bool Favor { set; get; }

        public SongInfo CopyFavor()
        {
            SongInfo favor = new SongInfo
            {
                Title = Title,
                SubTitle = SubTitle,
                Path = Path,
                SoundPath = SoundPath,
                SoundPlayOffset = SoundPlayOffset,
                Branches = Branches,
                Difficulties = Difficulties,
                Levels = Levels,
                GENRE = GENRE,
                Comment = Comment,
                HiddenLevel = HiddenLevel,
                Volume = Volume,
                Stage = Stage,
                ARTIST = ARTIST,
                Lyrics = Lyrics,
                Favor = true
            };
            return favor;
        }
    }

    public class FindFile
    {
        /// <summary>
        /// 得某文件夹下所有的文件
        /// </summary>
        /// <param name="directory">文件夹名称</param>
        /// <param name="pattern">搜寻指类型</param>
        /// <returns></returns>
        public static List<FileInfo> GetFiles(string path, string extName, List<FileInfo> lst)
        {
            try
            {

                string[] dir = Directory.GetDirectories(path); //文件夹列表  
                DirectoryInfo fdir = new DirectoryInfo(path);
                FileInfo[] file = fdir.GetFiles();
                //FileInfo[] file = Directory.GetFiles(path); //文件列表  
                if (file.Length != 0 || dir.Length != 0) //当前目录文件或文件夹不为空          
                {
                    foreach (FileInfo f in file) //显示当前目录所有文件  
                    {
                        if (!string.IsNullOrEmpty(f.Extension) && extName.ToLower().IndexOf(f.Extension.ToLower()) >= 0 && extName.Length == f.Extension.Length)
                        {
                            lst.Add(f);
                        }
                    }
                    foreach (string d in dir)
                    {
                        GetFiles(d, extName, lst);//递归  
                    }
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    public class CChip
    {
        public enum CType
        {
            UNKNOWN,
            GOGO_TIME,
            BRANCH_RESET,
            BRANCH_START,
            BRANCH_NOTICE,
            LEVEL_HOLD,
            BPM_CHANGE,
            CHAPTERLINE_OFF,
            END,
        }
        public CType Type { set; get; }
        public int JudgeTime { set; get; }
        public int EndTime { set; get; }
        public float Bpm { set; get; }
        public int BranchIndex { set; get; } = -1;
        public int StartChapter { set; get; }
        public int EndChpater { set; get; }
        public CourseBranch Branch { set; get; }
        public bool Player2 { set; get; }
        public CChip()
        {

        }
    }

    public class NoteChip
    {
        public int Index { set; get; }
        public int Type { set; get; }
        public int Chapter { set; get; }
        public float JudgeTime { set; get; }
        public float MidTime { set; get; }
        public float EndTime { set; get; }
        public int HitCount { set; get; }
        public bool BranchRelated { set; get; }
        public CourseBranch Branch { set; get; }
        public float Bpm { set; get; }
        public float AppearTime { set; get; }
        public float MoveTime { set; get; }
        public float Scroll { set; get; }
        public int WaitingTime { set; get; }
        public bool IsFixedSENote { set; get; }
        public int Senote { set; get; }
        public bool Gogo { set; get; }
        public float fBMSCROLLTime { set; get; }

        public NoteChip Copy()
        {
            NoteChip new_chip = new NoteChip();
            new_chip.Index = Index;
            new_chip.Type = Type;
            new_chip.Chapter = Chapter;
            new_chip.JudgeTime = JudgeTime;
            new_chip.MidTime = MidTime;
            new_chip.EndTime = EndTime;
            new_chip.HitCount = HitCount;
            new_chip.BranchRelated = BranchRelated;
            new_chip.Branch = Branch;
            new_chip.Bpm = Bpm;
            new_chip.AppearTime = AppearTime;
            new_chip.MoveTime = MidTime;
            new_chip.Scroll = Scroll;
            new_chip.WaitingTime = WaitingTime;
            new_chip.IsFixedSENote = IsFixedSENote;
            new_chip.Senote = Senote;
            new_chip.Gogo = Gogo;
            new_chip.fBMSCROLLTime = fBMSCROLLTime;
            return new_chip;
        }
    }

    public class ChapterLine
    {
        public double Bpm { set; get; }
        public float AppearTime { set; get; }
        public float MoveTime { set; get; }
        public int Chapter { set; get; }
        public double Scroll { set; get; }
        public float JudgeTime { set; get; }
        public int WaitingTime { set; get; } = 0;
    }

    public class Lyric
    {
        public int Index { set; get; }
        public int JudgeTime { set; get; }
        public string Content { set; get; }
    }


    public enum CourseBranch
    {
        BranchNormal,
        BranchExpert,
        BranchMaster,
    }

    public enum BalloonResult
    {
        Perfect,
        Good,
        Fail
    }

    public class Shuffle
    {
        public static void shuffle<T>(ref List<T> list)
        {
            if (list.Count <= 1) return;
            System.Random rand = new System.Random(Guid.NewGuid().GetHashCode());
            List<T> newList = new List<T>();//儲存結果的集合
            foreach (T item in list)
            {
                newList.Insert(rand.Next(0, newList.Count + 1), item);
            }
            newList.Remove(list[0]);//移除list[0]的值
            newList.Insert(rand.Next(0, newList.Count + 1), list[0]);//再重新隨機插入第一筆

            list = newList;
        }

        public static bool random(int ratenumerator, int denominator)
        {
            if (ratenumerator == 1 && denominator == 2)
            {
                ratenumerator = 2;
                denominator = 4;
            }
            System.Random rand = new System.Random(Guid.NewGuid().GetHashCode());
            int result = rand.Next(denominator);
            return ratenumerator > result ? true : false;
        }
    }

    public class Replay
    {
        public Dictionary<int, Dictionary<int, ReplayConfig>> Config;
        public Dictionary<int, Dictionary<int, List<InputReplay>>> Input;
        public Dictionary<int, Dictionary<int, List<NoteReplay>>> Show;
        public Dictionary<int, Dictionary<int, List<int>>> Notes;
        public Replay()
        {
            Config = new Dictionary<int, Dictionary<int, ReplayConfig>>();
            Input = new Dictionary<int, Dictionary<int, List<InputReplay>>>();
            Show = new Dictionary<int, Dictionary<int, List<NoteReplay>>>();
            Notes = new Dictionary<int, Dictionary<int, List<int>>>();
        }
    }

    public enum HitState
    {
        Miss,
        Bad,
        BalloonPerfect,
        BalloonGood,
        BalloonFail,
        HammerShow,
        HammerChange,
        RapidEnd,
        Perfect,
        Good,
        HitDon,
        HitKa,
    }

    public class InputReplay
    {
        public float Time;
        public KeyType Key;

        public InputReplay() { }
        public InputReplay(float time, KeyType key)
        {
            Time = time;
            Key = key;
        }
    }

    public class NoteReplay
    {
        public int Note;
        public float Time;
        public HitState Hit;

        public NoteReplay() { }
        public NoteReplay(int note, float time, HitState hit)
        {
            Note = note;
            Time = time;
            Hit = hit;
        }
    }

    public class VersionCourse
    {
        public VersionCourse() { }
        public AcVersion Version;
        public Dictionary<DanLevel, DanCourseInfo> Courses = new Dictionary<DanLevel, DanCourseInfo>();
    }

    public class DanCourseInfo
    {
        public List<string> Songs = new List<string>();
        public List<string> Subs = new List<string>();
        public List<int> Difficulties = new List<int>();
        public List<int> Stars = new List<int>();
        public Dictionary<DanCourse.Condition, int> Conditions = new Dictionary<DanCourse.Condition, int>();
        public Dictionary<DanCourse.Condition, int> Conditions2 = new Dictionary<DanCourse.Condition, int>();
        public List<int> DrumRolls = new List<int>();
        public List<int> DrumRolls2 = new List<int>();
        public List<int> Goods = new List<int>();
        public List<int> Goods2 = new List<int>();
        public AcVersion Version;
        public DanLevel Level;
        public int RewardSkin = 0;
        public List<DaniGenre> Genres;
        public int NotesCount;

        public DanCourseInfo() { }
    }

    public class DanCourse
    {
        public enum Condition
        {
            Gauge,
            Score,
            PerfectReach,
            GoodLessThan,
            BadLessThan,
            MaxCombo,
            DrumRolls,
            Completion,
        }

        public AcVersion Version;
        public DanLevel Level;
        public List<string> Songs = new List<string>();
        public List<string> Subs = new List<string>();
        public List<DaniGenre> Genres;
        public List<int> Difficulties = new List<int>();
        public List<int> Stars = new List<int>();
        public Dictionary<Condition, int> Conditions = new Dictionary<Condition, int>();
        public Dictionary<Condition, int> Conditions2 = new Dictionary<Condition, int>();
        public List<int> DrumRolls = new List<int>();
        public List<int> DrumRolls2 = new List<int>();
        public List<int> Goods = new List<int>();
        public List<int> Goods2 = new List<int>();
        public int RewardSkin = 0;
        public Dictionary<int, List<CChip>> Others = new Dictionary<int, List<CChip>>();
        public Dictionary<int, Dictionary<int, NoteChip>> Notes = new Dictionary<int, Dictionary<int, NoteChip>>();
        public Dictionary<int, List<ChapterLine>> Lines = new Dictionary<int, List<ChapterLine>>();
        public Dictionary<int, bool> Branches = new Dictionary<int, bool>();
        public List<int> Scores = new List<int>();
        public List<int> ScoresDiff = new List<int>();
        public List<int> Volumes = new List<int>();
        public int NotesCount;

        public DanCourse()
        {
        }
    }

    public enum AcVersion
    {
        MurasakiVersion = 0,
        WhiteVersion = 1,
        RedVersion = 2,
        YellowVersion = 3,
        BlueVersion = 4,
        GreenVersion = 5,
        NijiiroVersion = 6,
    }

    public enum DanLevel
    {
        //easy
        Junior = 1,

        //normal
        Class10 = 100,
        Class9 = 101,
        Class8 = 102,
        Class7 = 103,
        Class6 = 104,

        //hard
        Class5 = 1000,
        Class4 = 1001,
        Class3 = 1002,
        Class2 = 1003,
        Class1 = 1004,

        //oni
        Dan1 = 10000,
        Dan2 = 10001,
        Dan3 = 10002,
        Dan4 = 10003,
        Dan5 = 10004,
        Dan6 = 10005,
        Dan7 = 10006,
        Dan8 = 10007,
        Dan9 = 10008,
        Dan10 = 10009,

        Kurotou = 10010,
        Meijin = 10011,
        Choujin = 10012,
        Tatsujin = 10013,
        Extension = 10014,

        //
        None = 100000
    }

    public enum DaniGenre
    {
        JPop,
        Animation,
        Vocaloid,
        Childrens,
        Variety,
        Classic,
        Gamemusic,
        NamcoOriginal,
    }

    public enum ClearState
    {
        NoClear,
        NormalClear,
        GreatClear,
        PerfectClear,
    }

    public class RewardSkin
    {
        public SkinPosition Position { set; get; }
        public int SkinId { set; get; }
        public string Path { set; get; }
        public string Name { set; get; }
        public int Mask { set; get; }
        public string Remark { set; get; }

        public RewardSkin()
        {
        }
    }

    public class SkinInfo
    {
        public int Id { set; get; }
        public string Name { set; get; }
        public string DisplayName { set; get; }
        public bool Reward { set; get; }
        public GameObject Prefab { set; get; }
        public string Remark { set; get; }

        public SkinInfo(int id, string name, string display_name, GameObject pre, bool reward, string remark = "")
        {
            Id = id;
            Name = name;
            DisplayName = display_name;
            Reward = reward;
            Prefab = pre;
            Remark = remark;
        }
    }

    public class AtomSongInfo
    {
        public string Title { set; get; }
        public string Ab { set; get; }
        public string Path { set; get; }
        public DaniGenre Ganre { set; get; }
        public AtomSongInfo(string title, string ab, string path, DaniGenre genre)
        {
            Title = title;
            Ab = ab;
            Path = path;
            Ganre = genre;
        }
    }
}
