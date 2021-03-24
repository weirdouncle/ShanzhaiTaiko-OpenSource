using CommonClass;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameSetting;

public class SettingLoader : MonoBehaviour
{
    public StagePackScript[] Stages;
    public SkinPackScript SkinPack;

    public static Dictionary<int, RewardSkin> RewardSkins;
    public static Dictionary<AcVersion, RankingDojoCourseScript> RankingDojoData = new Dictionary<AcVersion, RankingDojoCourseScript>();
    public static Dictionary<string, StagePackScript> TranditionStageInfos = new Dictionary<string, StagePackScript>();
    public static Dictionary<string, StagePackScript> Ps4StageInfos = new Dictionary<string, StagePackScript>();
    public static Dictionary<string, StagePackScript> NijiiroStageInfos = new Dictionary<string, StagePackScript>();
    public static DataTable SongsData;

    public static GameObject Emotion;
    public static GameObject Head;
    public static GameObject Body;
    public static Material BodySkin;
    public static GameObject Cos;
    public static GameObject Sub;
    public static Material Face;
    public static Material Skin;
    public static Material Tatoo;

    public static GameObject Replay_Emotion;
    public static GameObject Replay_Head;
    public static GameObject Replay_Body;
    public static Material Replay_BodySkin;
    public static GameObject Replay_Cos;
    public static GameObject Replay_Sub;
    public static Material Replay_Face;
    public static Material Replay_Skin;
    public static Material Replay_Tatoo;

    public static GameSetting Setting = null;
    public static int Width;
    public static int Height;

    public static List<Sprite> NoteResults;
    public static List<Sprite> Combos;
    public static List<Sprite> Diffs;
    public static List<Sprite> Branches;
    public static List<Sprite> TextBranches;
    public static Sprite Roll;
    public static List<Sprite> SeNotes;
    public static List<Sprite> ClearSprites;

    public static List<int> ByteIndexes;

    public Text[] TextAmounts;
    public Text[] Text2;
    public Text[] Currents;

    private FileInfo current_file;
    private bool first;
    private SongInfo song;
    private int current_line_index;
    private int current_course_difficulty;
    private string new_line = Encoding.Default.GetString(new byte[1] { 13 });
    private static Dictionary<string, SkinPackScript> skin_pack = new Dictionary<string, SkinPackScript>();
    private List<SongInfo> all_songs = new List<SongInfo>();

    private int skin_data_pack;
    private int dojo_course;
    private int songs_data_pack;
    private int songs_count = -1;
    private int songs_loaded;
    private int skin_data_pack_count = -1;
    private int dojo_course_count = -1;
    private int songs_data_pack_count = -1;
    private int parse_failed;
    private StringBuilder failed_file = new StringBuilder();
    private List<FileInfo> file_list = new List<FileInfo>();

    private bool songs_all_loaded;
    private bool skin_prepared;
    private bool skin_loaded;
    private bool parsing_started;
    private bool loading;

    private string current_course;
    private string current_skin;
    private string current_acb;
    private string current_tja;

    void Start()
    {
        if (Setting == null)
        {
            RewardSkins = new Dictionary<int, RewardSkin>();
            Setting = new GameSetting();

#if !UNITY_ANDROID
            QualitySettings.maxQueuedFrames = 1;
            Screen.SetResolution(GameSetting.Config.ResolutionWidth, GameSetting.Config.ResolutionHeight, GameSetting.Config.FullScreen);
            QualitySettings.vSyncCount = GameSetting.Config.VSync ? 1 : 0;
#else
            Width = Screen.currentResolution.width;
            Height = Screen.currentResolution.height;

            float x = (float)Width / 16;
            float y = (float)Height / 9;
            int width = 0;
            int height = 0;
            if (GameSetting.Config.FullScreen)
            {
                if (x > y)
                {
                    y = Math.Min(120, y);
                    width = (int)Math.Ceiling(y * 16);
                    height = (int)Math.Ceiling(y * 9);
                }
                else if (y > x)
                {
                    x = Math.Min(120, x);
                    width = (int)Math.Ceiling(x * 16);
                    height = (int)Math.Ceiling(x * 9);
                }
                Screen.SetResolution(width, height, true);
            }
            else
            {
                if (x > y)
                {
                    y = Math.Min(120, y);
                    width = (int)Math.Ceiling(y * 9 * Width / Height);
                    height = (int)Math.Ceiling(y * 9);
                }
                else if (y > x)
                {
                    x = Math.Min(120, x);
                    width = (int)Math.Ceiling(x * 16);
                    height = (int)Math.Ceiling(x * 16 * Height / Width);
                }
                Screen.SetResolution(width, height, true);
            }
#endif

            //加载曲包
            Thread thread = new Thread(LoadSongs);
            thread.IsBackground = true;
            thread.Start();

            LoadPics();
            //LoadRewardSkin();

            //load dojo course
            string path;
#if UNITY_STANDALONE_WIN
            path = string.Format("{0}/AssetBundles/dojo", Environment.CurrentDirectory);
#else
            path = string.Format("{0}/AssetBundles/dojo", Application.persistentDataPath);
#endif
            if (Directory.Exists(path))
            {
                string[] dir = Directory.GetDirectories(path); //文件夹列表  
                DirectoryInfo fdir = new DirectoryInfo(path);
                FileInfo[] file = fdir.GetFiles();

                dojo_course_count = file.Length;
                if (file.Length != 0 || dir.Length != 0) //当前目录文件或文件夹不为空     
                {
                    foreach (FileInfo info in file)
                        StartCoroutine(LoadRankingDojo(info.FullName));
                }
            }
            else
                dojo_course_count = 0;

            //load songs
            SongsData = new DataTable("Song");
            SongsData.Columns.Add("title", typeof(string));
            SongsData.Columns.Add("file", typeof(string));
            SongsData.Columns.Add("name", typeof(string));
            SongsData.Columns.Add("path", typeof(string));
            SongsData.Columns.Add("genre", typeof(int));

#if UNITY_STANDALONE_WIN
            path = string.Format("{0}/AssetBundles/songs", Environment.CurrentDirectory);
#else
            path = string.Format("{0}/AssetBundles/songs", Application.persistentDataPath);
#endif
            if (Directory.Exists(path))
            {
                string[] dir = Directory.GetDirectories(path); //文件夹列表  
                DirectoryInfo fdir = new DirectoryInfo(path);
                FileInfo[] file = fdir.GetFiles();

                songs_data_pack_count = file.Length;
                if (file.Length != 0 || dir.Length != 0) //当前目录文件或文件夹不为空     
                {
                    foreach (FileInfo info in file)
                    {
                        StartCoroutine(LoadSongAb(info.FullName));
                    }
                }
            }
            else
                songs_data_pack_count = 0;

            //Debug.LogError(getSerialNumber());

            //读取场景包
            foreach (StagePackScript info in Stages)
            {
                switch (info.GameStyle)
                {
                    case GameStyle.TraditionalStyle:
                        TranditionStageInfos.Add(info.StageName.ToLower(), info);
                        break;
                    case GameStyle.NijiiroStyle:
                        NijiiroStageInfos.Add(info.StageName.ToLower(), info);
                        break;
                }
            }

            //读取存档中的皮肤设置
            LoadAllSkinPack();
        }
    }

    private void LoadSkinSaved()
    {
        Body = LoadPart(SkinPosition.TypeBody, GameSetting.Config.Body);
        Emotion = LoadPart(SkinPosition.TypeEmotion, GameSetting.Config.Emotion);
        Head = LoadPart(SkinPosition.TypeHead, GameSetting.Config.Head);
        Cos = LoadPart(SkinPosition.TypeCos, GameSetting.Config.SkinCos);
        Sub = LoadPart(SkinPosition.TypeSub, GameSetting.Config.SkinSub);

        BodySkin = LoadMaterial(SkinPosition.TypeBodySkin, GameSetting.Config.BodySkin);
        Face = LoadMaterial(SkinPosition.TypeFace, GameSetting.Config.Face);
        Skin = LoadMaterial(SkinPosition.TypeSkin, GameSetting.Config.Skin);
        Tatoo = LoadMaterial(SkinPosition.TypeTatoo, GameSetting.Config.Tatoo);

        skin_loaded = true;
    }

    private string getSerialNumber()
    {
        return SystemInfo.deviceUniqueIdentifier;
    }

    private void LoadAllSkinPack()
    {
        if (SkinPack != null) LoadDataFromSkinPack(SkinPack);

        string path = string.Empty;
#if !UNITY_ANDROID || UNITY_EDITOR
        path = string.Format("{0}/AssetBundles/skins", Environment.CurrentDirectory);
#else
        path = string.Format("{0}/AssetBundles/skins", Application.persistentDataPath);
#endif

        if (Directory.Exists(path))
        {
            string[] dir = Directory.GetDirectories(path); //文件夹列表  
            DirectoryInfo fdir = new DirectoryInfo(path);
            FileInfo[] file = fdir.GetFiles();
            //FileInfo[] file = Directory.GetFiles(path); //文件列表  
            skin_data_pack_count = file.Length;
            if (file.Length != 0 || dir.Length != 0) //当前目录文件或文件夹不为空     
            {
                foreach (FileInfo info in file)
                {
                    if (info.FullName.EndsWith(".json"))
                    {
                        skin_data_pack_count--;
                        continue;
                    }

                    StartCoroutine(LoadSkinFromABs(info.FullName));
                }
            }

            //读取翻译文件
            List<FileInfo> lst = new List<FileInfo>();
            FindFile.GetFiles(path, ".json", lst);
            foreach (FileInfo info in lst)
            {
                string str2;
                using (var streamReader = new StreamReader(info.FullName, Encoding.UTF8))
                {
                    str2 = streamReader.ReadToEnd();
                    streamReader.Close();
                }

                try
                {
                    DataSet data = JsonUntity.Json2Object<DataSet>(str2);
                    if (data.Tables.Contains("Head"))
                    {
                        foreach (DataRow row in data.Tables["Head"].Rows)
                        {
                            SkinTranslation.Tables["Head"].ImportRow(row);
                        }
                    }
                    if (data.Tables.Contains("Body"))
                    {
                        foreach (DataRow row in data.Tables["Body"].Rows)
                        {
                            SkinTranslation.Tables["Body"].ImportRow(row);
                        }
                    }
                    if (data.Tables.Contains("Emotion"))
                    {
                        foreach (DataRow row in data.Tables["Emotion"].Rows)
                        {
                            SkinTranslation.Tables["Emotion"].ImportRow(row);
                        }
                    }
                    if (data.Tables.Contains("Cos"))
                    {
                        foreach (DataRow row in data.Tables["Cos"].Rows)
                        {
                            SkinTranslation.Tables["Cos"].ImportRow(row);
                        }
                    }
                    if (data.Tables.Contains("Sub"))
                    {
                        foreach (DataRow row in data.Tables["Sub"].Rows)
                        {
                            SkinTranslation.Tables["Sub"].ImportRow(row);
                        }
                    }
                    if (data.Tables.Contains("Face"))
                    {
                        foreach (DataRow row in data.Tables["Face"].Rows)
                        {
                            SkinTranslation.Tables["Face"].ImportRow(row);
                        }
                    }
                    if (data.Tables.Contains("BodyColor"))
                    {
                        foreach (DataRow row in data.Tables["BodyColor"].Rows)
                        {
                            SkinTranslation.Tables["BodyColor"].ImportRow(row);
                        }
                    }
                    if (data.Tables.Contains("SkinColor"))
                    {
                        foreach (DataRow row in data.Tables["SkinColor"].Rows)
                        {
                            SkinTranslation.Tables["SkinColor"].ImportRow(row);
                        }
                    }
                    if (data.Tables.Contains("Tatoo"))
                    {
                        foreach (DataRow row in data.Tables["Tatoo"].Rows)
                        {
                            SkinTranslation.Tables["Tatoo"].ImportRow(row);
                        }
                    }
                }
                catch (Exception)
                {
                    Debug.Log(string.Format("file {0} parse error", info.FullName));
                }
            }
        }
        else
            skin_data_pack_count = 0;
    }

    IEnumerator LoadSkinFromABs(string path)
    {
        var bundleLoadRequest = AssetBundle.LoadFromFileAsync(path);
        yield return bundleLoadRequest;

        var _ab = bundleLoadRequest.assetBundle;
        if (_ab != null)
        {
            GameObject[] games = _ab.LoadAllAssets<GameObject>();
            foreach (GameObject game in games)
            {
                if (game != null)
                {
                    SkinPackScript pack = game.GetComponent<SkinPackScript>();
                    if (pack != null)
                        LoadDataFromSkinPack(pack);
                }
            }
        }

        skin_data_pack++;
    }

    private void LoadDataFromSkinPack(SkinPackScript pack)
    {
        string pack_name = pack.name;
        while (skin_pack.ContainsKey(pack_name))
        {
            pack_name += "1";
        }
        if (pack.Data != null)
        {
            DataSet data = JsonUntity.Json2Object<DataSet>(pack.Data.ToString());

            if (data.Tables.Contains("Body"))
            {
                foreach (DataRow row in data.Tables["Body"].Rows)
                {
                    row["Pack"] = pack_name;
                    GameSetting.Skin.Tables["Body"].ImportRow(row);
                }

                if (pack.Bodies.Length != data.Tables["Body"].Rows.Count)
                {
                    foreach (GameObject body in pack.Bodies)
                    {
                        DataRow[] rows = data.Tables["Body"].Select(string.Format("Name = '{0}'", body.name));
                        if (rows.Length == 0)
                            Debug.Log(body.name);
                    }
                }
            }

            if (data.Tables.Contains("Cos"))
            {
                foreach (DataRow row in data.Tables["Cos"].Rows)
                {
                    row["Pack"] = pack_name;
                    GameSetting.Skin.Tables["Cos"].ImportRow(row);
                }

                if (pack.Coses.Length != data.Tables["Cos"].Rows.Count)
                {
                    foreach (GameObject body in pack.Coses)
                    {
                        DataRow[] rows = data.Tables["Cos"].Select(string.Format("Name = '{0}'", body.name));
                        if (rows.Length == 0)
                            Debug.Log(body.name);
                    }
                }
            }
        }

        foreach (GameObject head in pack.Heads)
        {
            DataRow new_row = GameSetting.Skin.Tables["Head"].NewRow();
            new_row["Name"] = head.name;
            new_row["Pack"] = pack_name;
            GameSetting.Skin.Tables["Head"].Rows.Add(new_row);
        }

        foreach (GameObject head in pack.Emotions)
        {
            DataRow new_row = GameSetting.Skin.Tables["Emotion"].NewRow();
            new_row["Name"] = head.name;
            new_row["Pack"] = pack_name;
            GameSetting.Skin.Tables["Emotion"].Rows.Add(new_row);
        }

        foreach (GameObject head in pack.Subs)
        {
            DataRow new_row = GameSetting.Skin.Tables["Sub"].NewRow();
            new_row["Name"] = head.name;
            new_row["Pack"] = pack_name;
            GameSetting.Skin.Tables["Sub"].Rows.Add(new_row);
        }
        foreach (Material head in pack.FaceColors)
        {
            DataRow new_row = GameSetting.Skin.Tables["Face"].NewRow();
            new_row["Name"] = head.name;
            new_row["Pack"] = pack_name;
            GameSetting.Skin.Tables["Face"].Rows.Add(new_row);
        }
        foreach (Material head in pack.BodyColors)
        {
            DataRow new_row = GameSetting.Skin.Tables["BodyColor"].NewRow();
            new_row["Name"] = head.name;
            new_row["Pack"] = pack_name;
            GameSetting.Skin.Tables["BodyColor"].Rows.Add(new_row);
        }
        foreach (Material head in pack.SkinColors)
        {
            DataRow new_row = GameSetting.Skin.Tables["SkinColor"].NewRow();
            new_row["Name"] = head.name;
            new_row["Pack"] = pack_name;
            GameSetting.Skin.Tables["SkinColor"].Rows.Add(new_row);
        }
        foreach (Material head in pack.Tatoos)
        {
            DataRow new_row = GameSetting.Skin.Tables["Tatoo"].NewRow();
            new_row["Name"] = head.name;
            new_row["Pack"] = pack_name;
            GameSetting.Skin.Tables["Tatoo"].Rows.Add(new_row);
        }
        skin_pack[pack_name] = pack;
    }

    private void LoadSongs()
    {
        string path = string.Empty;
#if !UNITY_ANDROID || UNITY_EDITOR
        path = Environment.CurrentDirectory;
#else
            path = Application.persistentDataPath;
#endif
        FindFile.GetFiles(path, ".tja", file_list);
        songs_count = file_list.Count;
    }

    private void ParseTJA()
    {
        foreach (FileInfo info in file_list)
        {
            current_tja = info.FullName;
            current_file = info;
            first = true;
            song = new SongInfo
            {
                Path = info.FullName,
                Difficulties = new List<Difficulty>(),
                Branches = new Dictionary<Difficulty, bool>(),
                Levels = new Dictionary<Difficulty, int>()
            };

            LoadSongInfo(info.FullName, Encoding.GetEncoding("Shift_JIS"));

            if ((!string.IsNullOrEmpty(song.SoundPath) && File.Exists(song.SoundPath)) || HasAcbSong(song.Title))
            {
                all_songs.Add(song);
                string lyrics_path = song.Path.ToLower();
                lyrics_path = lyrics_path.Replace(".tja", ".lrc");
                if (File.Exists(lyrics_path))
                    song.Lyrics = true;
            }
            else
            {
                first = true;
                song = new SongInfo
                {
                    Path = info.FullName,
                    Difficulties = new List<Difficulty>(),
                    Branches = new Dictionary<Difficulty, bool>(),
                    Levels = new Dictionary<Difficulty, int>()
                };

                LoadSongInfo(info.FullName, Encoding.UTF8);

                if ((!string.IsNullOrEmpty(song.SoundPath) && File.Exists(song.SoundPath)) || HasAcbSong(song.Title))
                {
                    all_songs.Add(song);
                    string lyrics_path = song.Path.ToLower();
                    lyrics_path = lyrics_path.Replace(".tja", ".lrc");
                    if (File.Exists(lyrics_path))
                        song.Lyrics = true;
                }
                else
                {
                    parse_failed++;
                    failed_file.Append(info.FullName);
                    failed_file.Append("\n");
                }
            }

            songs_loaded++;
        }

        if (all_songs.Count > 0)
        {
            bool other = false;
            for (int i = 0; i < GameSetting.Config.SortColor.Count; i++)
            {
                SortColor sort = GameSetting.Config.SortColor[i];
                if (sort.Title == "Other") other = true;
                List<SongInfo> infos = new List<SongInfo>();
                foreach (SongInfo info in all_songs)
                {
                    if (SongClassify(info, sort))
                    {
                        info.GENRE = sort.Title;
                        infos.Add(info);
                    }
                }
                all_songs.RemoveAll(t => infos.Contains(t));
                infos.Sort((x, y) => string.Compare(x.Title, y.Title));
                Songs[sort] = infos;
            }

            if (all_songs.Count > 0 && !other)
            {
                all_songs.Sort((x, y) => string.Compare(x.Title, y.Title));
                SortColor sort = new SortColor("Other", string.Empty, "#FFFFFF");
                foreach (SongInfo info in all_songs)
                    info.GENRE = sort.Title;

                Songs[sort] = all_songs;
            }
        }
        songs_all_loaded = true;
        current_tja = string.Empty;
    }

    void Update()
    {
        if (skin_data_pack == skin_data_pack_count && !skin_prepared)
        {
            skin_prepared = true;
            LoadSkinSaved();
        }

        if (songs_count > -1 && !parsing_started && songs_data_pack == songs_data_pack_count)
        {
            parsing_started = true;
            Thread thread = new Thread(ParseTJA);
            thread.IsBackground = true;
            thread.Start();
        }

        if (!loading)
        {
            if (songs_all_loaded && dojo_course_count == dojo_course && songs_data_pack == songs_data_pack_count && skin_loaded)
            {
                loading = true;
                string scene = "MainScreen";
                AsyncOperation async = SceneManager.LoadSceneAsync(scene);
                async.allowSceneActivation = true;
            }
            else
            {
                Text2[0].text = dojo_course.ToString();
                TextAmounts[0].text = dojo_course_count >= 0 ? dojo_course_count.ToString() : "Scanning";

                Text2[1].text = skin_data_pack.ToString();
                TextAmounts[1].text = skin_data_pack_count >= 0 ? skin_data_pack_count.ToString() : "Scanning";

                Text2[2].text = songs_data_pack.ToString();
                TextAmounts[2].text = songs_data_pack_count >= 0 ? songs_data_pack_count.ToString() : "Scanning";

                Text2[3].text = songs_loaded.ToString();
                TextAmounts[3].text = songs_count >= 0 ? songs_count.ToString() : "Scanning";

                Text2[4].text = parse_failed.ToString();
                if (failed_file != null)
                    Text2[5].text = failed_file.ToString();

                Currents[3].text = current_tja;
                //Debug.Log(string.Format("songs {0}/{1} course {2}/{3} skins {4}/{5} acb songs {6}/{7}", songs_loaded, songs_count,
                //    dojo_course, dojo_course_count, skin_data, skin_data_count, songs_data, songs_data_count));
            }
        }
    }

    IEnumerator LoadRankingDojo(string path)
    {
        var bundleLoadRequest = AssetBundle.LoadFromFileAsync(path);
        yield return bundleLoadRequest;

        var myLoadedAssetBundle = bundleLoadRequest.assetBundle;
        if (myLoadedAssetBundle == null)
        {
            Debug.Log("Failed to load AssetBundle " + path);
        }
        else
        {
            GameObject[] games = myLoadedAssetBundle.LoadAllAssets<GameObject>();
            foreach (GameObject game in games)
            {
                RankingDojoCourseScript script = game.GetComponent<RankingDojoCourseScript>();
                if (script != null && !RankingDojoData.ContainsKey(script.Version) && script.Courses.Length > 0)
                {
                    RankingDojoData[script.Version] = script;
                }
            }
            //myLoadedAssetBundle.Unload(true);
        }
        dojo_course++;
    }

    IEnumerator LoadSongAb(string path)
    {
        var bundleLoadRequest = AssetBundle.LoadFromFileAsync(path);
        yield return bundleLoadRequest;

        var myLoadedAssetBundle = bundleLoadRequest.assetBundle;
        if (myLoadedAssetBundle == null)
        {
            Debug.Log("Failed to load AssetBundle " + path);
        }
        else
        {
            GameObject[] games = myLoadedAssetBundle.LoadAllAssets<GameObject>();
            foreach (GameObject game in games)
            {
                SongPackScript script = game.GetComponent<SongPackScript>();
                if (script != null)
                {
                    Dictionary<string, AtomSongInfo> song = JsonUntity.Json2Object<Dictionary<string, AtomSongInfo>>(script.SongList.ToString());
                    List<TextAsset> datas = new List<TextAsset>(script.Songs);
                    foreach (string title in song.Keys)
                    {
                        if (!HasAcbSong(title))
                        {
                            TextAsset data = datas.Find(t => t.name == song[title].Path);
                            if (data != null)
                            {
                                DataRow new_row = SongsData.NewRow();
                                new_row["title"] = title;
                                new_row["file"] = path;
                                new_row["name"] = game.name;
                                new_row["path"] = song[title].Path;
                                new_row["genre"] = (int)song[title].Ganre;

                                SongsData.Rows.Add(new_row);
                            }
                        }
                    }
                }
            }

            myLoadedAssetBundle.Unload(true);
        }
        songs_data_pack++;
    }

    public static bool HasAcbSong(string title)
    {
        title = title.Replace("'", "''");
        DataRow[] rows = SongsData.Select(string.Format("title = '{0}'", title));
        return rows.Length > 0;
    }

    public static KeyValuePair<string, AtomSongInfo> GetSongPath(string title)
    {
        title = title.Replace("'", "''");
        DataRow[] rows = SongsData.Select(string.Format("title = '{0}'", title));
        if (rows.Length > 0)
        {
            return new KeyValuePair<string, AtomSongInfo>(rows[0]["file"].ToString(),
                new AtomSongInfo(title, rows[0]["name"].ToString(), rows[0]["path"].ToString(), (DaniGenre)int.Parse(rows[0]["genre"].ToString())));
        }
        return new KeyValuePair<string, AtomSongInfo>(string.Empty, null);
    }

    public static void LoadReplaySkins(ReplayConfig config)
    {
        Replay_Body = LoadPart(SkinPosition.TypeBody, config.Body, true);
        if (Replay_Body == null && !string.IsNullOrEmpty(config.Body))
            Replay_Body = LoadPart(SkinPosition.TypeBody, GetSkinName(SkinPosition.TypeBody, config.Body, false), true);

        Replay_Emotion = LoadPart(SkinPosition.TypeEmotion, config.Emotion, true);
        if (Replay_Emotion == null && !string.IsNullOrEmpty(config.Emotion))
            Replay_Emotion = LoadPart(SkinPosition.TypeEmotion, GetSkinName(SkinPosition.TypeEmotion, config.Emotion, false), true);

        Replay_Head = LoadPart(SkinPosition.TypeHead, config.Head, true);
        if (Replay_Head == null && !string.IsNullOrEmpty(config.Head))
            Replay_Head = LoadPart(SkinPosition.TypeHead, GetSkinName(SkinPosition.TypeHead, config.Head, false), true);

        Replay_Cos = LoadPart(SkinPosition.TypeCos, config.SkinCos, true);
        if (Replay_Cos == null && !string.IsNullOrEmpty(config.SkinCos))
            Replay_Cos = LoadPart(SkinPosition.TypeCos, GetSkinName(SkinPosition.TypeCos, config.SkinCos, false), true);

        Replay_Sub = LoadPart(SkinPosition.TypeSub, config.SkinSub, true);
        if (Replay_Sub == null && !string.IsNullOrEmpty(config.SkinSub))
            Replay_Sub = LoadPart(SkinPosition.TypeSub, GetSkinName(SkinPosition.TypeSub, config.SkinSub, false), true);

        Replay_BodySkin = LoadMaterial(SkinPosition.TypeBodySkin, config.BodySkin);
        if (Replay_BodySkin == null && !string.IsNullOrEmpty(config.BodySkin))
            Replay_BodySkin = LoadMaterial(SkinPosition.TypeBodySkin, GetSkinName(SkinPosition.TypeBodySkin, config.BodySkin, false));

        Replay_Face = LoadMaterial(SkinPosition.TypeFace, config.Face);
        if (Replay_Face == null && !string.IsNullOrEmpty(config.Face))
            Replay_Face = LoadMaterial(SkinPosition.TypeFace, GetSkinName(SkinPosition.TypeFace, config.Face, false));

        Replay_Skin = LoadMaterial(SkinPosition.TypeSkin, config.Skin);
        if (Replay_Skin == null && !string.IsNullOrEmpty(config.Skin))
            Replay_Skin = LoadMaterial(SkinPosition.TypeSkin, GetSkinName(SkinPosition.TypeSkin, config.Skin, false));

        Replay_Tatoo = LoadMaterial(SkinPosition.TypeTatoo, config.Tatoo);
        if (Replay_Tatoo == null && !string.IsNullOrEmpty(config.Tatoo))
            Replay_Tatoo = LoadMaterial(SkinPosition.TypeTatoo, GetSkinName(SkinPosition.TypeTatoo, config.Tatoo, false));
    }

    public static GameObject LoadPart(SkinPosition postion, string part_name, bool replay = false)
    {
        if (!string.IsNullOrEmpty(part_name))
        {
            if (int.TryParse(part_name, out int skin_id) && RewardSkins.TryGetValue(skin_id, out RewardSkin skin) && skin.Position == postion
                && (replay || GameSetting.Record.Rewards.Contains(skin_id)))
            {
                return Resources.Load<GameObject>(RewardSkins[skin_id].Path);
            }
            else
            {
                string skin_name = part_name;
                string pack_name = GetSkinPackName(postion, skin_name);

                if (!string.IsNullOrEmpty(skin_name) && skin_pack.TryGetValue(pack_name, out SkinPackScript pack))
                {
                    switch (postion)
                    {
                        case SkinPosition.TypeHead:
                            {
                                List<GameObject> games = new List<GameObject>(pack.Heads);
                                return games.Find(t => t.name == skin_name);
                            }
                        case SkinPosition.TypeBody:
                            {
                                List<GameObject> games = new List<GameObject>(pack.Bodies);
                                return games.Find(t => t.name == skin_name);
                            }
                        case SkinPosition.TypeEmotion:
                            {
                                List<GameObject> games = new List<GameObject>(pack.Emotions);
                                return games.Find(t => t.name == skin_name);
                            }
                        case SkinPosition.TypeCos:
                            {
                                List<GameObject> games = new List<GameObject>(pack.Coses);
                                return games.Find(t => t.name == skin_name);
                            }
                        case SkinPosition.TypeSub:
                            {
                                List<GameObject> games = new List<GameObject>(pack.Subs);
                                return games.Find(t => t.name == skin_name);
                            }
                    }
                }
            }
        }

        return null;
    }

    public static Material LoadMaterial(SkinPosition postion, string part_name)
    {
        if (!string.IsNullOrEmpty(part_name))
        {
            string skin_name = part_name;
            string pack_name = GetSkinPackName(postion, skin_name);
            if (!string.IsNullOrEmpty(skin_name) && skin_pack.TryGetValue(pack_name, out SkinPackScript pack))
            {
                switch (postion)
                {
                    case SkinPosition.TypeFace:
                        {
                            List<Material> games = new List<Material>(pack.FaceColors);
                            return games.Find(t => t.name == skin_name);
                        }
                    case SkinPosition.TypeBodySkin:
                        {
                            List<Material> games = new List<Material>(pack.BodyColors);
                            return games.Find(t => t.name == skin_name);
                        }
                    case SkinPosition.TypeSkin:
                        {
                            List<Material> games = new List<Material>(pack.SkinColors);
                            return games.Find(t => t.name == skin_name);
                        }
                    case SkinPosition.TypeTatoo:
                        {
                            List<Material> games = new List<Material>(pack.Tatoos);
                            return games.Find(t => t.name == skin_name);
                        }
                }
            }
        }

        return null;
    }

    public static void LoadPics()
    {
        NoteResults = new List<Sprite>();
        Combos = new List<Sprite>();
        Diffs = new List<Sprite>();
        Branches = new List<Sprite>();
        TextBranches = new List<Sprite>();
        SeNotes = new List<Sprite>();
        ClearSprites = new List<Sprite>();

        List<Sprite> sprites = new List<Sprite>(Resources.LoadAll<Sprite>("Texture/localization"));

        string suffix = "_cn";
        switch (GameSetting.Config.Locale)
        {
            case Locale.English:
                suffix = "_en";
                break;
            case Locale.Japanese:
                suffix = "_jp";
                break;
            case Locale.Korean:
                suffix = "_kr";
                break;
            case Locale.Spanish:
                suffix = "_es";
                break;
        }

        //result
        Sprite perfect = sprites.Find(t => t.name == "perfect" + suffix);
        if (perfect == null) perfect = sprites.Find(t => t.name == "perfect_cn");
        NoteResults.Add(perfect);

        Sprite good = sprites.Find(t => t.name == "good" + suffix);
        if (good == null) good = sprites.Find(t => t.name == "good_cn");
        NoteResults.Add(good);

        Sprite bad = sprites.Find(t => t.name == "bad" + suffix);
        if (bad == null) bad = sprites.Find(t => t.name == "bad_cn");
        NoteResults.Add(bad);

        //Combos
        Sprite combo = sprites.Find(t => t.name == "combo" + suffix);
        if (combo == null) combo = sprites.Find(t => t.name == "combo_cn");
        Combos.Add(combo);

        Sprite combo1 = sprites.Find(t => t.name == "combo1" + suffix);
        if (combo1 == null) combo1 = sprites.Find(t => t.name == "combo1_cn");
        Combos.Add(combo1);

        Sprite combo2 = sprites.Find(t => t.name == "combo2" + suffix);
        if (combo2 == null) combo2 = sprites.Find(t => t.name == "combo2_cn");
        Combos.Add(combo2);

        //Diff
        Sprite easy = sprites.Find(t => t.name == "easy" + suffix);
        if (easy == null) easy = sprites.Find(t => t.name == "easy_cn");
        Diffs.Add(easy);

        Sprite normal = sprites.Find(t => t.name == "normal" + suffix);
        if (normal == null) normal = sprites.Find(t => t.name == "normal_cn");
        Diffs.Add(normal);

        Sprite hard = sprites.Find(t => t.name == "hard" + suffix);
        if (hard == null) hard = sprites.Find(t => t.name == "hard_cn");
        Diffs.Add(hard);

        Sprite oni = sprites.Find(t => t.name == "oni" + suffix);
        if (oni == null) oni = sprites.Find(t => t.name == "oni_cn");
        Diffs.Add(oni);

        Sprite edit = sprites.Find(t => t.name == "edit" + suffix);
        if (edit == null) edit = sprites.Find(t => t.name == "edit_cn");
        Diffs.Add(edit);

        //branch
        Sprite lv_up = sprites.Find(t => t.name == "lv_up" + suffix);
        if (lv_up == null) lv_up = sprites.Find(t => t.name == "lv_up_cn");
        Branches.Add(lv_up);

        Sprite lv_down = sprites.Find(t => t.name == "lv_down" + suffix);
        if (lv_down == null) lv_down = sprites.Find(t => t.name == "lv_down_cn");
        Branches.Add(lv_down);

        Sprite text_branch1 = sprites.Find(t => t.name == "Text_Normal" + suffix);
        if (text_branch1 == null) text_branch1 = sprites.Find(t => t.name == "Text_Normal_cn");
        TextBranches.Add(text_branch1);

        Sprite text_branch2 = sprites.Find(t => t.name == "Text_Expert" + suffix);
        if (text_branch2 == null) text_branch2 = sprites.Find(t => t.name == "Text_Expert_cn");
        TextBranches.Add(text_branch2);

        Sprite text_branch3 = sprites.Find(t => t.name == "Text_Master" + suffix);
        if (text_branch3 == null) text_branch3 = sprites.Find(t => t.name == "Text_Master_cn");
        TextBranches.Add(text_branch3);

        Roll = sprites.Find(t => t.name == "drumroll" + suffix);
        if (Roll == null) Roll = sprites.Find(t => t.name == "drumroll_en");

        Sprite clear = sprites.Find(t => t.name == "clear" + suffix);
        if (clear == null) clear = sprites.Find(t => t.name == "clear_en");
        ClearSprites.Add(clear);
        Sprite unclear = sprites.Find(t => t.name == "unclear" + suffix);
        if (unclear == null) unclear = sprites.Find(t => t.name == "unclear_en");
        ClearSprites.Add(unclear);

        List<Sprite> se_sprites = new List<Sprite>(Resources.LoadAll<Sprite>("Texture/senotes"));
        //don
        for (int i = 1; i <= 4; i++)
        {
            Sprite don = se_sprites.Find(t => t.name == string.Format("don{0}{1}", i, suffix));
            if (don == null) don = se_sprites.Find(t => t.name == string.Format("don{0}_en", i));
            SeNotes.Add(don);
        }
        //ka
        for (int i = 1; i <= 3; i++)
        {
            Sprite ka = se_sprites.Find(t => t.name == string.Format("ka{0}{1}", i, suffix));
            if (ka == null) ka = se_sprites.Find(t => t.name == string.Format("ka{0}_en", i));
            SeNotes.Add(ka);
        }
        //rapid
        for (int i = 1; i <= 2; i++)
        {
            Sprite ka = se_sprites.Find(t => t.name == string.Format("roll{0}{1}", i, suffix));
            if (ka == null) ka = se_sprites.Find(t => t.name == string.Format("roll{0}_en", i));
            SeNotes.Add(ka);

        }
        //9
        Sprite body = se_sprites.Find(t => t.name == string.Format("roll_body{0}", suffix));
        if (body == null) body = se_sprites.Find(t => t.name == "roll_body_en");
        SeNotes.Add(body);
        //10
        Sprite tail = se_sprites.Find(t => t.name == string.Format("roll_tail{0}", suffix));
        if (tail == null) tail = se_sprites.Find(t => t.name == "roll_tail_en");
        SeNotes.Add(tail);
        //11
        Sprite balloon = se_sprites.Find(t => t.name == string.Format("balloon{0}", suffix));
        if (balloon == null) balloon = se_sprites.Find(t => t.name == "balloon_en");
        SeNotes.Add(balloon);
        //12
        Sprite hammer = se_sprites.Find(t => t.name == string.Format("hammer{0}", suffix));
        if (hammer == null) hammer = se_sprites.Find(t => t.name == "hammer_en");
        SeNotes.Add(hammer);
    }


    public void LoadSongInfo(string path, Encoding code)
    {
        string str2;
        using (var streamReader = new StreamReader(path, code))
        {
            str2 = streamReader.ReadToEnd();
            streamReader.Close();
        }
        Decode(str2);
    }

    private void Decode(string str)
    {
        if (!string.IsNullOrEmpty(str))
        {
            str = str.Replace(Environment.NewLine, "\n");
            str = str.Replace(new_line, "\n");
            str = str.Replace('\t', ' ');
            str += "\n";

            CharEnumerator ce = str.GetEnumerator();
            if (ce.MoveNext())
            {
                int current_line_index = 1;
                do
                {
                    if (!this.SkipBlankorChangeLine(ref ce))
                    {
                        break;
                    }
                    if (first)
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

            }
        }
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
#region [ TITLE ]
        //-----------------
        if (command_str.StartsWith("TITLE", StringComparison.OrdinalIgnoreCase))
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
            song.ARTIST = parameter_str;
        }
        //-----------------
#endregion
#region [ COMMENT ]
        //-----------------
        else if (command_str.StartsWith("COMMENT", StringComparison.OrdinalIgnoreCase))
        {
            this.t入力_パラメータ食い込みチェック("COMMENT", ref command_str, ref parameter_str);
            song.Comment = parameter_str;
        }
        //-----------------
#endregion
#region [ GENRE ]
        //-----------------
        else if (command_str.StartsWith("GENRE", StringComparison.OrdinalIgnoreCase))
        {
            this.t入力_パラメータ食い込みチェック("GENRE", ref command_str, ref parameter_str);
            song.GENRE = parameter_str;
        }
        //-----------------
#endregion
#region [ HIDDENLEVEL ]
        //-----------------
        else if (command_str.StartsWith("HIDDENLEVEL", StringComparison.OrdinalIgnoreCase))
        {
            this.t入力_パラメータ食い込みチェック("HIDDENLEVEL", ref command_str, ref parameter_str);
            song.HiddenLevel = parameter_str.ToLower().Equals("on");
        }
        //-----------------
#endregion
#region [ PREIMAGE ]
        //-----------------
        else if (command_str.StartsWith("PREIMAGE", StringComparison.OrdinalIgnoreCase))
        {
            this.t入力_パラメータ食い込みチェック("PREIMAGE", ref command_str, ref parameter_str);
            //this.PREIMAGE = parameter_str;
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
    }

    private void t入力_パラメータ食い込みチェック(string command_str, ref string command, ref string parameter)
    {
        Debug.Log(string.Format("command before: {0}", command));
        Debug.Log(string.Format("para before: {0}", parameter));
        if ((command.Length > command_str.Length) && command.StartsWith(command_str, StringComparison.OrdinalIgnoreCase))
        {
            parameter = command.Substring(command_str.Length).Trim();
            command = command.Substring(0, command_str.Length);

            Debug.Log(string.Format("command after: {0}", command));
            Debug.Log(string.Format("para after: {0}", parameter));
        }
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
        @"^(?!(TITLE|LEVEL|BPM|WAVE|OFFSET|BALLOON|EXAM1|EXAM2|EXAM3|BALLOONNOR|BALLOONEXP|BALLOONMAS|SONGVOL|SEVOL|SCOREINIT|SCOREDIFF|COURSE|STYLE|GAME|LIFE|DEMOSTART|SIDE|SUBTITLE|SCOREMODE|GENRE|MOVIEOFFSET|STAGE|BGIMAGE|BGMOVIE|HIDDENBRANCH|GAUGEINCR|#HBSCROLL|#BMSCROLL)).+\n",
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
                Debug.Log(string.Format("#START命令が少なくとも1つは必要です。 {0}", current_file.FullName));
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
            strSplitした譜面 = this.SplitByCourse(this.StringArrayToString(strSplitした譜面, "\n"));
            //存在するかのフラグ作成。
            for (int i = 0; i < strSplitした譜面.Length; i++)
            {
                if (!string.IsNullOrEmpty(strSplitした譜面[i]))
                {
                    song.Difficulties.Add((Difficulty)i);
                }
            }
#endregion
            first = false;
        }
    }

    private string[] dlmtSpace = { " " };
    private string[] dlmtEnter = { "\n" };
    private string[] dlmtCOURSE = { "COURSE:" };

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
            song.Branches[(Difficulty)current_course_difficulty] = true;
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

        //パラメータを分別、そこから割り当てていきます。
        if (strCommandName.Equals("TITLE"))
        {
            var subTitle = "";
            for (int i = 0; i < strArray.Length; i++)
            {
                subTitle += strArray[i];
            }

            song.Title = subTitle.Substring(5);

            if (song.Title[song.Title.Length - 1].ToString() == new_line)
            {
                song.Title = song.Title.Substring(0, song.Title.Length - 1);
            }
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
                song.SubTitle = subTitle.Substring(10);
            }
            else if (strCommandParam.StartsWith("++"))
            {
                var subTitle = "";
                for (int i = 0; i < strArray.Length; i++)
                {
                    subTitle += strArray[i];
                }
                song.SubTitle = subTitle.Substring(10);
            }
        }
        else if (strCommandName.Equals("LEVEL"))
        {
            var level = (int)Convert.ToDouble(strCommandParam);
            song.Levels[(Difficulty)current_course_difficulty] = level;
        }
        else if (strCommandName.Equals("WAVE"))
        {
            //Debug.Log(strCommandParam);
            if (!string.IsNullOrEmpty(strCommandParam))
                song.SoundPath = string.Format("{0}//{1}", current_file.DirectoryName, strCommandParam);
        }
        else if (strCommandName.Equals("SONGVOL") && !string.IsNullOrEmpty(strCommandParam))
        {
            song.Volume = Math.Min(100, Convert.ToInt32(strCommandParam));
        }
        else if (strCommandName.Equals("STAGE") && !string.IsNullOrEmpty(strCommandParam))
        {
            song.Stage = strCommandParam;
        }
        else if (strCommandName.Equals("COURSE"))
        {
            if (!string.IsNullOrEmpty(strCommandParam))
            {
                this.current_course_difficulty = this.strConvertCourse(strCommandParam);
            }
        }
        else if (strCommandName.Equals("GENRE"))
        {
            //2015.03.28 kairera0467
            //ジャンルの定義。DTXから入力もできるが、tjaからも入力できるようにする。
            //日本語名だと選曲画面でバグが出るので、そこもどうにかしていく予定。

            if (!string.IsNullOrEmpty(strCommandParam))
            {
                song.GENRE = strCommandParam;
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


                song.SoundPlayOffset = nOFFSETms;
            }
        }
        else if (strCommandName.Equals("BGIMAGE"))
        {
            //2016.02.02 kairera0467
            if (!string.IsNullOrEmpty(strCommandParam))
            {
                //this.strBGIMAGE_PATH = strCommandParam;
            }
        }
        else if (strCommandName.Equals("HIDDENBRANCH"))
        {
            //2016.04.01 kairera0467 パラメーターは
            if (!string.IsNullOrEmpty(strCommandParam))
            {
                //this.bHIDDENBRANCH = true;
            }
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


    private string[] SplitByCourse(string strTJA)
    {
        string[] strCourseTJA = new string[(int)CommonClass.Difficulty.Total];

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
        var sb = new System.Text.StringBuilder();

        for (int n = 0; n < input.Length; n++)
        {
            sb.Append(input[n] + strデリミタ文字);
        }

        return sb.ToString();
    }

    private static readonly Regex CommandAndArgumentRegex = new Regex(@"^(#[A-Z]+)(?:\s?)(.+?)?$", RegexOptions.Compiled);
    private static readonly Regex BranchStartArgumentRegex = new Regex(@"^([^,\s]+)\s*,\s*([^,\s]+)\s*,\s*([^,\s]+)$", RegexOptions.Compiled);

    private bool SongClassify(SongInfo song, SortColor sort)
    {
        if (song.GENRE == sort.Title || (!string.IsNullOrEmpty(sort.SubTitle) && song.GENRE == sort.SubTitle))
            return true;

        string path = song.Path;
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        path = path.Remove(0, Environment.CurrentDirectory.Length);
#elif UNITY_ANDROID
        path = path.Remove(0, Application.persistentDataPath.Length);
#endif

        char split = '\\';
#if UNITY_ANDROID && !UNITY_EDITOR
        split = '/';
#endif

        List<string> strs = new List<string>(path.Split(split));
        strs.RemoveAt(strs.Count - 1);

        for (int i = strs.Count - 1; i >= 0; i--)
        {
            //Debug.Log(strs[i]);
            if (strs[i] == sort.Title || (!string.IsNullOrEmpty(sort.SubTitle) && strs[i] == sort.SubTitle))
                return true;
        }

        return false;
    }
}