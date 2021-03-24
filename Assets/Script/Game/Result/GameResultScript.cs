using CommonClass;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputAction;

public class GameResultScript : MonoBehaviour
{
    public static event PlaySelectedSoundDelegate PlaySelectedSound;

    public Animator Animator;
    public Text SongName;
    public GameObject NormalName_pre;
    public GameObject ClearName_pre;
    public GameObject GoldClearName_pre;
    public GameObject PerfectName_pre;
    public Transform PlayerNamePanel;
    public Transform GaugeParent;
    public Image Diff;
    public Animator Flower;
    public Image[] HitResult;
    public Image DiffChar;
    
    public UI3DdonControll Avatar;
    public NumberAnimatorScript Score;
    public NumberAnimatorScript Perfect;
    public NumberAnimatorScript Good;
    public NumberAnimatorScript Bad;
    public NumberAnimatorScript Combo;
    public NumberAnimatorScript RapidHit;

    public GameObject[] Gauges;
    public CriAtomSource ScoreIncreasing;

    public Transform CrownParent;
    public GameObject ClearClip_pre;
    public GameObject FullClip_pre;
    public GameObject[] Fail_pre;
    public GameObject RobotFullClip_pre;
    public CriAtomSource Best;

    public GameObject Mask;

    public Image AutoMark;
    public Image SpeedMark;
    public Image RandomMark;
    public Image StealMark;
    public Image ShinMark;
    public Image ReverseMark;
    public Image ModeMark;
    public Sprite[] SpeedSprites;
    public Sprite[] RandomSprites;

    public Text[] Texts;

    protected CourseGaugeScript GaugeBar;
    protected bool received;
    protected bool gauge_phase;
    protected bool all_phase;
    protected bool quiting;
    protected Config config;

    void Start()
    {
        Texts[0].text = GameSetting.Translate("game result");
        Texts[1].text = GameSetting.Translate("score");
        Texts[2].text = GameSetting.Translate("record update");
        Texts[3].text = GameSetting.Translate("max combo");
        Texts[4].text = GameSetting.Translate("hits");

        config = GameSetting.Config.Copy();
        if (GameSetting.Mode == CommonClass.PlayMode.Replay)
            config.CopyReplayConfig(GameSetting.Replay.Config[GameSetting.Config.ScoreMode][(int)GameSetting.Difficulty]);


        SongName.text = GameSetting.SelectedInfo.Title;

        if (GameSetting.Mode == CommonClass.PlayMode.Replay)
        {
            GameObject game = Instantiate(NormalName_pre, PlayerNamePanel);
            PlayerNameNormalScript script = game.GetComponent<PlayerNameNormalScript>();
            script.SetName(GameSetting.Replay.Config[GameSetting.Config.ScoreMode][(int)GameSetting.Difficulty].PlayerName, string.Empty);
        }
        else
        {
            KeyValuePair<ClearState, string> title = GameSetting.GetTitle();
            GameObject game = null;

            switch (title.Key)
            {
                case ClearState.NoClear:
                    game = Instantiate(NormalName_pre, PlayerNamePanel);
                    break;
                case ClearState.NormalClear:
                    game = Instantiate(ClearName_pre, PlayerNamePanel);
                    break;
                case ClearState.GreatClear:
                    game = Instantiate(GoldClearName_pre, PlayerNamePanel);
                    break;
                case ClearState.PerfectClear:
                    game = Instantiate(PerfectName_pre, PlayerNamePanel);
                    break;
            }

            PlayerNameNormalScript script = game.GetComponent<PlayerNameNormalScript>();
            if (title.Key != ClearState.NoClear && script.Animation != null)
            {
                if (title.Key == ClearState.PerfectClear)
                    script.Animation.enabled = true;
                else if (title.Key == ClearState.GreatClear)
                    script.Title.color = new Color(.89f, .69f, 0);
                else
                    script.Title.color = new Color(1, 1, 1);
            }
            script.SetName(GameSetting.Config.PlayerName, title.Value);
        }

        string path = "Texture/Difficulties";
        if (GameResultStartScript.Scene.Contains("PS4"))
            path = "Texture/DifficultiesPs4";

        foreach (Sprite sprite in Resources.LoadAll<Sprite>(path))
        {
            if (sprite.name == string.Format("lv{0}", (int)GameSetting.Difficulty + 1))
            {
                Diff.sprite = sprite;
                break;
            }
        }

        switch (GameSetting.Difficulty)
        {
            case Difficulty.Easy:
                {
                    GameObject gauge = Instantiate(Gauges[0], GaugeParent);
                    GaugeBar = gauge.GetComponent<CourseGaugeScript>();
                }
                break;
            case Difficulty.Normal:
            case Difficulty.Hard:
                {
                    GameObject gauge = Instantiate(Gauges[1], GaugeParent);
                    GaugeBar = gauge.GetComponent<CourseGaugeScript>();
                }
                break;
            default:
                {
                    GameObject gauge = Instantiate(Gauges[2], GaugeParent);
                    GaugeBar = gauge.GetComponent<CourseGaugeScript>();
                }
                break;
        }

        DiffChar.sprite = SettingLoader.Diffs[(int)GameSetting.Difficulty];

        HitResult[0].sprite = SettingLoader.NoteResults[0];
        RectTransform p_rect = HitResult[0].GetComponent<RectTransform>();
        p_rect.sizeDelta = new Vector2(HitResult[0].sprite.bounds.size.x * 100, HitResult[0].sprite.bounds.size.y * 100);
        HitResult[0].transform.localPosition += new Vector3(HitResult[0].sprite.bounds.size.x / 2 * 90, 0); 

        HitResult[1].sprite = SettingLoader.NoteResults[1];
        RectTransform g_rect = HitResult[1].GetComponent<RectTransform>();
        g_rect.sizeDelta = new Vector2(HitResult[1].sprite.bounds.size.x * 100, HitResult[1].sprite.bounds.size.y * 100);
        HitResult[1].transform.localPosition += new Vector3(HitResult[1].sprite.bounds.size.x / 2 * 90, 0);

        HitResult[2].sprite = SettingLoader.NoteResults[2];
        RectTransform b_rect = HitResult[2].GetComponent<RectTransform>();
        b_rect.sizeDelta = new Vector2(HitResult[2].sprite.bounds.size.x * 100, HitResult[2].sprite.bounds.size.y * 100);
        HitResult[2].transform.localPosition += new Vector3(HitResult[2].sprite.bounds.size.x / 2 * 90, 0);


#if !UNITY_ANDROID
        BasicInputScript.Input.Player.RightDon.performed += Click;
        BasicInputScript.Input.Player.Cancel.performed += Click;
        BasicInputScript.Input.Player.Enter.performed += Click;
#endif

        BasicInputScript.Input.Player.Esc.performed += Click;

        if (GameSetting.Config.DirectInput)
            BasicInputScript.KeyInvoke += DirectKey;

        AutoMark.gameObject.SetActive(GameSetting.Config.Special == Special.AutoPlay);
        StealMark.gameObject.SetActive(config.Steal);
        if (config.Speed != Speed.Normal)
        {
            SpeedMark.gameObject.SetActive(true);
            int index = (int)config.Speed - 2;
            if ((int)config.Speed > 10) index -= 6;
            SpeedMark.sprite = SpeedSprites[index];
        }
        if (config.Random != RandomType.None)
        {
            RandomMark.gameObject.SetActive(true);
            RandomMark.sprite = RandomSprites[(int)config.Random - 1];
        }
        if ((Score)GameSetting.Config.ScoreMode == CommonClass.Score.Shin)
            ShinMark.gameObject.SetActive(true);
        if (config.Reverse)
            ReverseMark.gameObject.SetActive(true);
        if (GameSetting.Mode == CommonClass.PlayMode.Replay)
            ModeMark.gameObject.SetActive(true);

        StartCoroutine(DelayStart());
    }

    private void OnDestroy()
    {
        BasicInputScript.KeyInvoke -= DirectKey;
    }

    IEnumerator DelayStart()
    {
        yield return new WaitForEndOfFrame();
        Animator.enabled = true;
    }

    public virtual void StartShow()
    {
        received = true;
        StartCoroutine("ShowResult");
    }
    public virtual void AvatarShow()
    {
        Avatar.ResultStart();
    }
    IEnumerator ShowResult()
    {
        //计算魂槽
        float gauge = GloableAniControllerScript.GaugePoints;
        int gauge_count = (int)gauge / 200;
        if (gauge_count > 0) ScoreIncreasing.Play();
        int gauge_grow = 0;
        for (int i = 1; i <= gauge_count; i++)
        {
            gauge_grow += 200;
            yield return new WaitForSeconds(0.05f);
            GaugeBar.UpdateGauge(gauge_grow);
        }
        if (gauge_count > 0) ScoreIncreasing.Stop();

        bool full = false;
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

        if (gauge >= clear_gauge)
        {
            if (GameSetting.Config.Special == Special.AutoPlay)
                Instantiate(RobotFullClip_pre, CrownParent);
            else
            {
                if (bad == 0)
                {
                    full = true;
                    Instantiate(FullClip_pre, CrownParent);
                }
                else
                {
                    Instantiate(ClearClip_pre, CrownParent);
                }
            }

            if (full)
                Avatar.Full();
            else
                Avatar.Clear();
            Flower.SetTrigger("Bloom");
        }
        else
        {
            Avatar.ResultFail();
            int index = UnityEngine.Random.Range(0, Fail_pre.Length);
            Instantiate(Fail_pre[index], CrownParent);
        }

        gauge_phase = true;

        yield return new WaitForSeconds(1);

        //score
        int score = GameResultStartScript.Score;
        char[] arr = score.ToString().ToCharArray();
        Array.Reverse(arr);

        int show_score = 0;
        int mult = 1;
        if (score > 0)
            ScoreIncreasing.Play();
        for (int i = 1; i < arr.Length; i++)
        {
            mult *= 10;
            string str = arr[i].ToString();
            int count = int.Parse(str);
            for (int y = 1; y <= count; y++)
            {
                yield return new WaitForSeconds(0.06f);
                Score.Count = show_score + y * mult;
            }
            show_score += count * mult;
        }
        if (score > 0)
            ScoreIncreasing.Stop();

        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Don);
        yield return new WaitForSeconds(1);

        if (GameSetting.Config.Special != Special.AutoPlay && GameSetting.Mode != CommonClass.PlayMode.Replay && GameResultStartScript.Score > GameResultStartScript.OldScore)
        {
            Best.gameObject.SetActive(true);
            yield return new WaitForSeconds(1);
        }
#if UNITY_EDITOR
        Debug.Log(string.Format("perfect {0} good {1} bad {2} combo {3} hits {4}", perfect, good, bad, GameResultStartScript.MaxCombo, GameResultStartScript.DrumRolls));
#endif
        //perfect
        score = perfect;
        char[] p_arr = score.ToString().ToCharArray();
        Array.Reverse(p_arr);

        show_score = 0;
        mult = 1;

        if (score > 0)
            ScoreIncreasing.Play();
        for (int i = 0; i < p_arr.Length; i++)
        {
            mult = (int)Math.Pow(10, i);
            string str = p_arr[i].ToString();

            //Debug.Log(string.Format("str {0} mult {1}", str, mult));

            int count = int.Parse(str);
            for (int y = 1; y <= count; y++)
            {
                yield return new WaitForSeconds(0.06f);
                Perfect.Count = show_score + y * mult;
            }
            show_score += count * mult;
        }
        if (score > 0)
            ScoreIncreasing.Stop();

        if (perfect > 0)
        {
            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Don);
            yield return new WaitForSeconds(1);
        }

        //good
        score = good;
        char[] g_arr = score.ToString().ToCharArray();
        Array.Reverse(g_arr);

        show_score = 0;
        mult = 1;
        if (good > 0)
            ScoreIncreasing.Play();
        for (int i = 0; i < g_arr.Length; i++)
        {
            mult = (int)Math.Pow(10, i);
            string str = g_arr[i].ToString();
            int count = int.Parse(str);
            for (int y = 1; y <= count; y++)
            {
                yield return new WaitForSeconds(0.06f);
                Good.Count = show_score + y * mult;
            }
            show_score += count * mult;
        }
        if (good > 0)
        {
            ScoreIncreasing.Stop();
            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Don);
            yield return new WaitForSeconds(1);
        }

        //bad
        score = bad;
        char[] b_arr = score.ToString().ToCharArray();
        Array.Reverse(b_arr);

        show_score = 0;
        mult = 1;
        if (bad > 0)
            ScoreIncreasing.Play();
        for (int i = 0; i < b_arr.Length; i++)
        {
            mult = (int)Math.Pow(10, i);
            string str = b_arr[i].ToString();
            int count = int.Parse(str);
            for (int y = 1; y <= count; y++)
            {
                yield return new WaitForSeconds(0.06f);
                Bad.Count = show_score + y * mult;
            }
            show_score += count * mult;
        }
        if (bad > 0)
        {
            ScoreIncreasing.Stop();
            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Don);
            yield return new WaitForSeconds(1);
        }

        //combo
        score = GameResultStartScript.MaxCombo;
        char[] c_arr = score.ToString().ToCharArray();
        Array.Reverse(c_arr);

        show_score = 0;
        mult = 1;
        if (score > 0)
            ScoreIncreasing.Play();
        for (int i = 0; i < c_arr.Length; i++)
        {
            mult = (int)Math.Pow(10, i);
            string str = c_arr[i].ToString();
            int count = int.Parse(str);
            for (int y = 1; y <= count; y++)
            {
                yield return new WaitForSeconds(0.06f);
                Combo.Count = show_score + y * mult;
            }
            show_score += count * mult;
        }
        if (score > 0)
        {
            ScoreIncreasing.Stop();
            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Don);
            yield return new WaitForSeconds(1);
        }
        //hits
        score = GameResultStartScript.DrumRolls;
        char[] h_arr = score.ToString().ToCharArray();
        Array.Reverse(h_arr);

        show_score = 0;
        mult = 1;
        if (score > 0)
            ScoreIncreasing.Play();
        for (int i = 0; i < h_arr.Length; i++)
        {
            mult = (int)Math.Pow(10, i);
            string str = h_arr[i].ToString();
            int count = int.Parse(str);
            for (int y = 1; y <= count; y++)
            {
                yield return new WaitForSeconds(0.06f);
                RapidHit.Count = show_score + y * mult;
            }
            show_score += count * mult;
        }
        if (score > 0)
        {
            ScoreIncreasing.Stop();
            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Don);
        }

        all_phase = true;
    }

    private void Click(CallbackContext context)
    {
        Skip();
    }

    protected virtual void DirectKey(GameSetting.KeyType key, bool press)
    {
        if (key == GameSetting.KeyType.Confirm || key == GameSetting.KeyType.Cancel || key == GameSetting.KeyType.Escape || key == GameSetting.KeyType.RightDon)
            Skip();
    }

    protected void PlaySound(StartSoundControllScript.SoundType type)
    {
        PlaySelectedSound?.Invoke(type);
    }

    public virtual void Skip()
    {
        if (!Application.isFocused || !received || quiting) return;
        ScoreIncreasing.Stop();
        if (all_phase)
        {
            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Cancel);
            quiting = true;
            Mask.SetActive(true);
            StartCoroutine(Quit());
        }
        else
        {
            int bad = PlayNoteScript.Result[HitNoteResult.Bad].Count;
            int good = PlayNoteScript.Result[HitNoteResult.Good].Count;
            int perfect = PlayNoteScript.Result[HitNoteResult.Perfect].Count;

            received = false;
            StopCoroutine("ShowResult");
            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Don);

            if (!gauge_phase)
            {
                //计算魂槽
                float gauge = GloableAniControllerScript.GaugePoints;
                GaugeBar.UpdateGauge(gauge);

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

                bool full = false;
                if (gauge >= clear_gauge)
                {
                    if (bad == 0)
                    {
                        full = true;
                        if (GameSetting.Config.Special == Special.AutoPlay)
                            Instantiate(RobotFullClip_pre, CrownParent);
                        else
                            Instantiate(FullClip_pre, CrownParent);
                    }
                    else
                    {
                        Instantiate(ClearClip_pre, CrownParent);
                    }

                    if (full)
                        Avatar.Full();
                    else
                        Avatar.Clear();
                    Flower.SetTrigger("Bloom");
                }
                else
                {
                    Avatar.ResultFail();
                    int index = UnityEngine.Random.Range(0, Fail_pre.Length);
                    Instantiate(Fail_pre[index], CrownParent);
                }
            }

            //score
            Score.Count = GameResultStartScript.Score;
            if (GameSetting.Config.Special != Special.AutoPlay && GameSetting.Mode != CommonClass.PlayMode.Replay && GameResultStartScript.Score > GameResultStartScript.OldScore)
            {
                Best.gameObject.SetActive(true);
            }
            //perfect
            Perfect.Count = perfect;
            //good
            Good.Count = good;
            //bad
            Bad.Count = bad;
            //combo
            Combo.Count = GameResultStartScript.MaxCombo;

            //hits
            RapidHit.Count = GameResultStartScript.DrumRolls;
            all_phase = true;
            StartCoroutine(Freeze());
        }
    }

    IEnumerator Freeze()
    {
        yield return new WaitForSeconds(0.5f);
        received = true;
    }

    IEnumerator Quit()
    {
        yield return new WaitForSeconds(0.5f);
        GameResultStartScript.DestroyInstance();
        yield return new WaitForSeconds(1f);
        string scene = "SongSelect";
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
    }
}
