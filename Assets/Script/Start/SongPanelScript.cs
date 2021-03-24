using CommonClass;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SongPanelScript : MonoBehaviour
{
    //public static event LoadAndroidMusicDelegate LoadAndroidMusic;
    //public delegate void LoadAndroidMusicDelegate(string path);

    public Text Title;
    public Text SubTitle;
    public GameObject Lyrics;
    public Image StageIcon;
    public GameObject[] StarsEasy;
    public GameObject[] StarsNormal;
    public GameObject[] StarsHard;
    public GameObject[] StarsOni;
    public GameObject[] StarsEdit;

    public GameObject Easy;
    public GameObject Hard;
    public GameObject Normal;
    public GameObject Oni;
    public GameObject Edit;

    public Image[] Crowns;
    public Image[] BranchMarks;
    public Text[] LVTexts;

    public Animator ScorePanel;
    public Animator DiffChange;

    public GameObject[] DiffMarks;
    public Text[] ScoreNormal;
    public Text[] PlayerNormal;
    public Text[] ScoreShin;
    public Text[] PlayerShin;

    public AudioSource Audio;
    public CriAtomSource AtomAudio;
    public Sprite[] Sprites;

    private SongInfo Info = null;
    private Dictionary<Difficulty, List<GameObject>> stars = new Dictionary<Difficulty, List<GameObject>>();
    private bool loaded;
    //private string atom_playing;
    void Start()
    {
        SongScanScript.SongSelected += Selected;
        SongScanScript.Random += RandomSelect;

        stars[Difficulty.Easy] = new List<GameObject>(StarsEasy);
        stars[Difficulty.Normal] = new List<GameObject>(StarsNormal);
        stars[Difficulty.Hard] = new List<GameObject>(StarsHard);
        stars[Difficulty.Oni] = new List<GameObject>(StarsOni);
        stars[Difficulty.Edit] = new List<GameObject>(StarsEdit);
    }

    private void OnDestroy()
    {
        SongScanScript.SongSelected -= Selected;
        SongScanScript.Random -= RandomSelect;
    }

    private readonly Dictionary<Difficulty, string> difficulties = new Dictionary<Difficulty, string>
    {
        { Difficulty.Easy, "简单" },
        { Difficulty.Normal, "通常" },
        { Difficulty.Hard, "困难" },
        { Difficulty.Oni, "鬼" },
        { Difficulty.Edit, "里" },
    };


    private void RandomSelect()
    {
        Title.text = "??????";
        SubTitle.text = string.Empty;
        StopByChangeMusic();
    }

    private void Selected(SongInfo song)
    {
        bool same = Info != null && song.Title == Info.Title;
        Info = song;

#if UNITY_ANDROID
        /*
        if (!same)
        {
            StopAllCoroutines();
            StopMusic();
            StartCoroutine(PlayAudio(Info.SoundPath));
        }
        */
        if (!same || (!Audio.isPlaying && AtomAudio.status != CriAtomSource.Status.Playing))
        {
            loaded = false;
            StopAllCoroutines();
            StopMusic();
            StartCoroutine(PlayAudio(Info.SoundPath));
        }
#else
        if (!same || (!Audio.isPlaying && AtomAudio.status != CriAtomSource.Status.Playing))
        {
            loaded = false;
            StopAllCoroutines();
            StopByChangeMusic();
            StartCoroutine(PlayAudio(Info.SoundPath));
        }
#endif

        if (same) return;

        Lyrics.SetActive(song.Lyrics);
        Info.Difficulties.RemoveAll(t => !difficulties.ContainsKey(t));

        StageIcon.gameObject.SetActive(false);
        if (!string.IsNullOrEmpty(song.Stage))
        {
            string stage = song.Stage.ToLower();
            if (SettingLoader.TranditionStageInfos.TryGetValue(stage, out StagePackScript info))
            {
                StageIcon.sprite = info.Icon;
                StageIcon.gameObject.SetActive(true);
            }
        }

        Title.text = Info.Title;
        SubTitle.text = Info.SubTitle;
        SubTitle.gameObject.SetActive(!string.IsNullOrEmpty(Info.SubTitle));

        Easy.SetActive(Info.Difficulties.Contains(Difficulty.Easy));
        Normal.SetActive(Info.Difficulties.Contains(Difficulty.Normal));
        Hard.SetActive(Info.Difficulties.Contains(Difficulty.Hard));
        Oni.SetActive(Info.Difficulties.Contains(Difficulty.Oni));

        if (Info.Difficulties.Contains(Difficulty.Edit))
        {
            DiffChange.SetTrigger("Change");
            Edit.SetActive(true);
        }
        else
        {
            Edit.SetActive(false);
            DiffChange.SetTrigger("Cancel");
        }

        //过关 or full combo
        List<Difficulty> diffs = new List<Difficulty>(difficulties.Keys);

        for (int i = 0; i < diffs.Count; i++)
        {
            Difficulty diff = diffs[i];
            if (GameSetting.Record.Record.IsFullCombo(Info.Title, diff))
            {
                Crowns[i].gameObject.SetActive(true);
                Crowns[i].sprite = Sprites[0];
            }
            else if (GameSetting.Record.Record.IsClear(Info.Title, diff))
            {
                Crowns[i].gameObject.SetActive(true);
                Crowns[i].sprite = Sprites[1];
            }
            else
                Crowns[i].gameObject.SetActive(false);

            BranchMarks[i].gameObject.SetActive(Info.Branches.ContainsKey(diff));

            if (Info.Levels.ContainsKey(diff))
            {
                LVTexts[i].text = string.Format("{0}", Info.Levels[diff]);
                for (int x = 0; x < stars[diff].Count; x++)
                    stars[diff][x].SetActive(x < Info.Levels[diff]);
            }

            DiffMarks[i].SetActive(Info.Difficulties.Contains(diff));
            ScoreNormal[i].gameObject.SetActive(Info.Difficulties.Contains(diff));
            ScoreShin[i].gameObject.SetActive(Info.Difficulties.Contains(diff));
            ScoreNormal[i].text = GameSetting.Record.Record.GetScore(Info.Title, diff, (int)Score.Normal).ToString();
            PlayerNormal[i].text = GameSetting.Record.Record.GetBestPlayer(diff, Info.Title, Score.Normal);
            ScoreShin[i].text = GameSetting.Record.Record.GetScore(Info.Title, diff, (int)Score.Shin).ToString();
            PlayerShin[i].text = GameSetting.Record.Record.GetBestPlayer(diff, Info.Title, Score.Shin);
        }
        ScorePanel.SetTrigger("Refresh");
    }
    /*
    IEnumerator LoadSongAb(StagePackInfo info)
    {
#if UNITY_STANDALONE_WIN
        string path = System.Environment.CurrentDirectory + "/AssetBundles";
#else
        string path = Application.persistentDataPath + "/AssetBundles";
#endif

        var bundleLoadRequest = AssetBundle.LoadFromFileAsync(string.Format("{0}/{1}", path, info.ABName));
        yield return bundleLoadRequest;

        var myLoadedAssetBundle = bundleLoadRequest.assetBundle;
        if (myLoadedAssetBundle == null)
        {
            Debug.Log("Failed to load stage AssetBundle " + name);
        }
        else
        {
            GameObject game = myLoadedAssetBundle.LoadAsset<GameObject>(info.Path);
            if (game != null)
            {
                StagePack = game.GetComponent<StagePackScript>();
                if (StagePack != null && StagePack.Icon != null)
                {
                    StageIcon.sprite = StagePack.Icon;
                    StageIcon.gameObject.SetActive(true);
                }
            }

            myLoadedAssetBundle.Unload(false);
        }
    }
    */
    IEnumerator PlayAudio(string path)
    {
        yield return new WaitForSeconds(0.75f);

        if (!File.Exists(Info.SoundPath) && SettingLoader.HasAcbSong(Info.Title))
        {
            StartCoroutine(LoadAtomAcb());
            yield break;
        }
        else if (File.Exists(Info.SoundPath))
        {

#if UNITY_ANDROID
        //PlayMusic?.Invoke(Info.SoundPath, Info.SoundPlayOffset, true);

        string file = path.ToLower();
        AudioType Type = AudioType.WAV;
        if (file.EndsWith(".mp3"))
            Type = AudioType.MPEG;
        else if (file.EndsWith(".ogg"))
            Type = AudioType.OGGVORBIS;

        using (var uwr = UnityWebRequestMultimedia.GetAudioClip(string.Format("file://{0}", path), Type))
        {
            DownloadHandlerAudioClip dl = (DownloadHandlerAudioClip)uwr.downloadHandler;
            dl.compressed = false;
            dl.streamAudio = true;
            yield return uwr.SendWebRequest();
            if (uwr.result != UnityWebRequest.Result.ConnectionError)
            {
                loaded = true;

                AudioClip clip = DownloadHandlerAudioClip.GetContent(uwr);
                if (GameSetting.Temp != null) Destroy(GameSetting.Temp);

                Audio.clip = GameSetting.Temp = clip;
                float value = (float)Info.SoundPlayOffset / 1000;
                Audio.time = value;
                Audio.Play();
                float replay_time = clip.length - value;
                StartCoroutine(Replay(replay_time, value));
            }
        }
#else
            string file = path.ToLower();
            AudioType Type = AudioType.WAV;
            if (file.EndsWith(".mp3"))
                Type = AudioType.MPEG;
            else if (file.EndsWith(".ogg"))
                Type = AudioType.OGGVORBIS;

            using (var uwr = UnityWebRequestMultimedia.GetAudioClip(string.Format("file://{0}", path), Type))
            {
                DownloadHandlerAudioClip dl = (DownloadHandlerAudioClip)uwr.downloadHandler;
                dl.compressed = false;
                dl.streamAudio = true;
                yield return uwr.SendWebRequest();
                if (uwr.result != UnityWebRequest.Result.ConnectionError)
                {
                    loaded = true;

                    AudioClip clip = DownloadHandlerAudioClip.GetContent(uwr);
                    if (GameSetting.Temp != null) Destroy(GameSetting.Temp);

                    Audio.clip = GameSetting.Temp = clip;
                    float value = (float)Info.SoundPlayOffset / 1000;
                    Audio.time = value;
                    Audio.Play();
                    float replay_time = clip.length - value;
                    StartCoroutine(Replay(replay_time, value));
                }
            }
#endif
        }
    }
    IEnumerator Replay(float time, float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(time);
            {
                Audio.Stop();
                Audio.time = delay;
                Audio.Play();
            }
        }
    }

    IEnumerator LoadAtomAcb()
    {
        loaded = true;
        if (SettingLoader.HasAcbSong(Info.Title))
        {
            KeyValuePair<string, AtomSongInfo> path = SettingLoader.GetSongPath(Info.Title);

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
                            CriAtomCueSheet sheet = CriAtom.AddCueSheet(Info.Title, tex.bytes, string.Empty);
                            sheet.acb.GetCueInfoByIndex(0, out CriAtomEx.CueInfo song_info);
                            sheet.acb.GetWaveFormInfo(song_info.name, out CriAtomEx.WaveformInfo wave_info);
                            float second = wave_info.numSamples / (float)wave_info.samplingRate;
                            AtomAudio.cueSheet = sheet.name;
                            AtomAudio.cueName = song_info.name;
                            //Debug.Log(atom_playing);
                            AtomAudio.startTime = Info.SoundPlayOffset;
                            AtomAudio.Play();

                            float replay_time = second - (float)Info.SoundPlayOffset / 1000;
                            StartCoroutine(AtomReplay(replay_time, Info.SoundPlayOffset));
                        }
                    }
                }

                myLoadedAssetBundle.Unload(true);
                AssetBundle.UnloadAllAssetBundles(false);
            }
        }
    }

    IEnumerator AtomReplay(float waiting_time, int start_time)
    {
        while (true)
        {
            yield return new WaitForSeconds(waiting_time);
            {
                AtomAudio.Stop();
                AtomAudio.startTime = start_time;
                AtomAudio.Play();
            }
        }
    }


    private void StopByChangeMusic()
    {
        StopAllCoroutines();
        Audio.Stop();
        CriAtom.RemoveCueSheet(AtomAudio.cueSheet);

        if (GameSetting.Selected != null)
        {
            Destroy(GameSetting.Selected);
            GameSetting.Selected = null;
        }
    }

    public void StopMusic()
    {
        StopAllCoroutines();
#if UNITY_ANDROID
        //StopPlaying?.Invoke();
        Audio.Stop();
#else
        Audio.Stop();
#endif

        CriAtom.RemoveCueSheet(AtomAudio.cueSheet);
    }

    public void LoadMusic()
    {
        if (!File.Exists(Info.SoundPath) && SettingLoader.HasAcbSong(Info.Title)) return;

        if (!loaded || GameSetting.Temp == null)
            StartCoroutine(LoadAudio());
        else
            GameSetting.Selected = GameSetting.Temp;
    }

    IEnumerator LoadAudio()
    {
        string path = Info.SoundPath;
        string file = path.ToLower();
        AudioType Type = AudioType.WAV;
        if (file.EndsWith(".mp3"))
            Type = AudioType.MPEG;
        else if (file.EndsWith(".ogg"))
            Type = AudioType.OGGVORBIS;

        using (var uwr = UnityWebRequestMultimedia.GetAudioClip(string.Format("file://{0}", path), Type))
        {
            DownloadHandlerAudioClip dl = (DownloadHandlerAudioClip)uwr.downloadHandler;
            dl.compressed = false;
            yield return uwr.SendWebRequest();
            if (uwr.result != UnityWebRequest.Result.ConnectionError)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(uwr);
                if (GameSetting.Temp != null) Destroy(GameSetting.Temp);
                GameSetting.Selected = GameSetting.Temp = clip;
            }
        }
    }
}
