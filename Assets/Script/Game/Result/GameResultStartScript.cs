using CommonClass;
using ICSharpCode.SharpZipLib.GZip;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameResultStartScript : MonoBehaviour
{
    public string LoadScene;
    public Animator Animator;
    public GameObject AndroidDrum;
    public Transform MovePanel;
    public Transform[] Images;
    public GloableAniControllerScript GloableAni;
    public Camera Camera;

    public static string Scene;
    public static int Score;
    public static int OldScore;
    public static int DrumRolls;
    public static int MaxCombo;
    public static NijiiroRank Rank;

    private Queue<Transform> queue = new Queue<Transform>();
    private WaitForSeconds wait = new WaitForSeconds(0.01f);
    protected static GameObject instance;

    void Start()
    {
        //if (Camera != null) Debug.Log(Camera.cullingMask);

        foreach (Transform image in Images)
            queue.Enqueue(image);
    }

    void OnDestroy()
    {
        if (PlayNoteScript.Result != null)
            PlayNoteScript.Result.Clear();

        if (PlayNoteScript.Result2P != null)
            PlayNoteScript.Result2P.Clear();
    }

    public static void DestroyInstance()
    {
        Destroy(instance);
        instance = null;
    }

    public virtual void StartAnimating()
    {
        if (Camera != null) Camera.cullingMask = 1079;

        Score = GloableAni.TotalScore;
        DrumRolls = GloableAni.DrumRolls;
        MaxCombo = GloableAni.MaxCombo;
        Rank = GloableAni.Rank;

        int bad = PlayNoteScript.Result[HitNoteResult.Bad].Count;
        int good = PlayNoteScript.Result[HitNoteResult.Good].Count;
        int perfect = PlayNoteScript.Result[HitNoteResult.Perfect].Count;

        int clear_gauge;
        switch (GameSetting.Difficulty)
        {
            case Difficulty.Easy:
                clear_gauge = 6000;
                break;
            case Difficulty.Normal:
            case Difficulty.Hard:
                clear_gauge = 7000;
                break;
            default:
                clear_gauge = 8000;
                break;
        }

        bool clear = false;
        bool full = false;

        float gauge = GloableAniControllerScript.GaugePoints;
        if (gauge >= clear_gauge)
        {
            clear = true;
            if (bad == 0) full = true;
        }

        OldScore = GameSetting.Record.Record.GetScore(GameSetting.SelectedInfo.Title, GameSetting.Difficulty, GameSetting.Config.ScoreMode);
        if (GameSetting.Config.Special != Special.AutoPlay && GameSetting.Mode != CommonClass.PlayMode.Replay)
            GameSetting.SetScore(GameSetting.SelectedInfo.Title, GameSetting.Config.PlayerName,
                GameSetting.Difficulty, clear, full, Score, perfect, good, bad, MaxCombo, DrumRolls, (int)Rank);

        if (GameSetting.Config.Special != Special.AutoPlay && GameSetting.Mode != CommonClass.PlayMode.Replay && Score > OldScore) SaveReplay();

        if (AndroidDrum != null) AndroidDrum.SetActive(false);

        Animator.enabled = true;
    }

    public virtual void InterScene()
    {
        Scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        instance = gameObject;
        DontDestroyOnLoad(gameObject);
        UnityEngine.SceneManagement.SceneManager.LoadScene(LoadScene);
    }

    public virtual void BgMoving()
    {
        StartCoroutine(Move());
    }

    IEnumerator Move()
    {
        int count = 0;
        int round = 0;
        while (true)
        {
            MovePanel.localPosition += new Vector3(0, 0.02f);
            yield return wait;
            count++;
            if (count >= 579)
            {
                count = 0;
                round++;
                Transform first = queue.Dequeue();
                Transform[] rest = new Transform[2];
                queue.CopyTo(rest, 0);
                queue.Enqueue(first);
                first.localPosition = new Vector3(0, rest[1].localPosition.y - 11.58f);
                first.SetAsLastSibling();
            }
        }
    }

    protected void SaveReplay()
    {
#if !UNITY_ANDROID
        if (GameSetting.Mode == CommonClass.PlayMode.Normal || GameSetting.Mode == CommonClass.PlayMode.PlayWithReplay)
        {
            string filepath = GameSetting.SelectedInfo.Path;
            filepath = filepath.Remove(filepath.Length - 4);
            filepath = string.Format("{0}.rep", filepath);

            Replay replay = null;
            if (File.Exists(filepath))
            {
                GZipInputStream zipFile = new GZipInputStream(File.OpenRead(filepath));
                using (MemoryStream re = new MemoryStream(50000))
                {
                    int count;
                    byte[] data = new byte[50000];
                    while ((count = zipFile.Read(data, 0, data.Length)) != 0)
                    {
                        re.Write(data, 0, count);
                    }
                    byte[] overarr = re.ToArray();
                    re.Close();
                    //将byte数组转为string
                    string result = System.Text.Encoding.UTF8.GetString(overarr);
                    try
                    {
                        replay = JsonUntity.Json2Object<Replay>(result);
                    }
                    catch (System.Exception)
                    {
                        replay = new Replay();
                    }
                }
                zipFile.Close();
                File.Delete(filepath);
            }
            else
            {
                replay = new Replay();
            }
            if (!replay.Config.ContainsKey(GameSetting.Config.ScoreMode)) replay.Config.Add(GameSetting.Config.ScoreMode, new Dictionary<int, ReplayConfig>());
            replay.Config[GameSetting.Config.ScoreMode][(int)GameSetting.Difficulty] = GameSetting.Config.Copy2Replay();

            if (!replay.Input.ContainsKey(GameSetting.Config.ScoreMode)) replay.Input.Add(GameSetting.Config.ScoreMode, new Dictionary<int, List<InputReplay>>());
            replay.Input[GameSetting.Config.ScoreMode][(int)GameSetting.Difficulty] = InputScript.InputRecord;

            if (!replay.Show.ContainsKey(GameSetting.Config.ScoreMode)) replay.Show.Add(GameSetting.Config.ScoreMode, new Dictionary<int, List<NoteReplay>>());
            replay.Show[GameSetting.Config.ScoreMode][(int)GameSetting.Difficulty] = GloableAniControllerScript.NotesRecord;

            List<NoteSoundScript> notes = new List<NoteSoundScript>(LoaderScript.Notes.Values);
            notes.Sort((x, y) => { return x.Index < y.Index ? -1 : 1; });
            List<int> notes_index = new List<int>();
            foreach (NoteSoundScript note in notes)
                notes_index.Add(note.Type);

            if (!replay.Notes.ContainsKey(GameSetting.Config.ScoreMode)) replay.Notes.Add(GameSetting.Config.ScoreMode, new Dictionary<int, List<int>>());
            replay.Notes[GameSetting.Config.ScoreMode][(int)GameSetting.Difficulty] = notes_index;

            string str = JsonUntity.Object2Json(replay, false);
            byte[] rawData = System.Text.Encoding.UTF8.GetBytes(str);
            using (MemoryStream ms = new MemoryStream())
            {
                GZipOutputStream compressedzipStream = new GZipOutputStream(ms);
                compressedzipStream.Write(rawData, 0, rawData.Length);
                compressedzipStream.Close();
                FileStream fs = new FileStream(filepath, FileMode.Create);
                fs.Write(ms.ToArray(), 0, ms.ToArray().Length);
                fs.Close();
                ms.Close();
            }
#if UNITY_EDITOR
            Debug.Log("replay save");
#endif
        }
#endif
    }
}
