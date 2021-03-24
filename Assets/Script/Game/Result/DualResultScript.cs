using CommonClass;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.UI;

public class DualResultScript : GameResultScript
{
    public Transform GaugeParent2P;
    public Image Diff2P;
    public Image[] HitResult2P;
    public Image DiffChar2P;

    public UI3DdonControll Avatar2P;
    public NumberAnimatorScript Score2P;
    public NumberAnimatorScript Perfect2P;
    public NumberAnimatorScript Good2P;
    public NumberAnimatorScript Bad2P;
    public NumberAnimatorScript Combo2P;
    public NumberAnimatorScript RapidHit2P;

    public Transform CrownParent2P;
    public PlayerNameNormalScript PlayerName2P;

    public Image AutoMark2P;
    public Image SpeedMark2P;
    public Image RandomMark2P;
    public Image StealMark2P;
    public Image ShinMark2P;
    public Image ReverseMark2P;
    public Image ModeMark2P;

    public GameObject[] Vol2P;
    public GameObject[] Gauges2P;

    private CourseGaugeScript GaugeBar2P;
    void Start()
    {
        Texts[0].text = GameSetting.Translate("game result");
        Texts[1].text = Texts[4].text = GameSetting.Translate("score");
        Texts[2].text = Texts[5].text = GameSetting.Translate("max combo");
        Texts[3].text = Texts[6].text = GameSetting.Translate("hits");

        config = GameSetting.Config;
        SongName.text = GameSetting.SelectedInfo.Title;

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
        PlayerName2P.SetName("GUEST", string.Empty);

        string path = "Texture/Difficulties";
        foreach (Sprite sprite in Resources.LoadAll<Sprite>(path))
        {
            if (sprite.name == string.Format("lv{0}", (int)GameSetting.Difficulty + 1))
            {
                Diff.sprite = sprite;
            }
            if (sprite.name == string.Format("lv{0}", (int)GameSetting.Difficulty2P + 1))
            {
                Diff2P.sprite = sprite;
            }
        }

        switch (GameSetting.Difficulty)
        {
            case Difficulty.Easy:
                {
                    GameObject gauge = Instantiate(Gauges[0], GaugeParent);
                    GaugeBar = gauge.GetComponent<CourseGaugeScript>();

                    GameObject gauge2p = Instantiate(Gauges2P[0], GaugeParent2P);
                    GaugeBar2P = gauge2p.GetComponent<CourseGaugeScript>();
                }
                break;
            case Difficulty.Normal:
            case Difficulty.Hard:
                {
                    GameObject gauge = Instantiate(Gauges[1], GaugeParent);
                    GaugeBar = gauge.GetComponent<CourseGaugeScript>();

                    GameObject gauge2p = Instantiate(Gauges2P[1], GaugeParent2P);
                    GaugeBar2P = gauge2p.GetComponent<CourseGaugeScript>();
                }
                break;
            default:
                {
                    GameObject gauge = Instantiate(Gauges[2], GaugeParent);
                    GaugeBar = gauge.GetComponent<CourseGaugeScript>();

                    GameObject gauge2p = Instantiate(Gauges2P[2], GaugeParent2P);
                    GaugeBar2P = gauge2p.GetComponent<CourseGaugeScript>();
                }
                break;
        }

        DiffChar.sprite = SettingLoader.Diffs[(int)GameSetting.Difficulty];
        DiffChar2P.sprite = SettingLoader.Diffs[(int)GameSetting.Difficulty2P];

        HitResult[0].sprite = HitResult2P[0].sprite = SettingLoader.NoteResults[0];
        RectTransform p_rect = HitResult[0].GetComponent<RectTransform>();
        RectTransform p_rect_2p = HitResult2P[0].GetComponent<RectTransform>();
        p_rect.sizeDelta = p_rect_2p.sizeDelta = new Vector2(HitResult[0].sprite.bounds.size.x * 100, HitResult[0].sprite.bounds.size.y * 100);
        HitResult[0].transform.localPosition += new Vector3(HitResult[0].sprite.bounds.size.x / 2 * 90, 0);
        HitResult2P[0].transform.localPosition += new Vector3(HitResult[0].sprite.bounds.size.x / 2 * 90, 0);

        HitResult[1].sprite = HitResult2P[1].sprite = SettingLoader.NoteResults[1];
        RectTransform g_rect = HitResult[1].GetComponent<RectTransform>();
        RectTransform g_rect_2p = HitResult2P[1].GetComponent<RectTransform>();
        g_rect.sizeDelta = g_rect_2p.sizeDelta = new Vector2(HitResult[1].sprite.bounds.size.x * 100, HitResult[1].sprite.bounds.size.y * 100);
        HitResult[1].transform.localPosition += new Vector3(HitResult[1].sprite.bounds.size.x / 2 * 90, 0);
        HitResult2P[1].transform.localPosition += new Vector3(HitResult[1].sprite.bounds.size.x / 2 * 90, 0);

        HitResult[2].sprite = HitResult2P[2].sprite = SettingLoader.NoteResults[2];
        RectTransform b_rect = HitResult[2].GetComponent<RectTransform>();
        RectTransform b_rect_2p = HitResult2P[2].GetComponent<RectTransform>();
        b_rect.sizeDelta = b_rect_2p.sizeDelta = new Vector2(HitResult[2].sprite.bounds.size.x * 100, HitResult[2].sprite.bounds.size.y * 100);
        HitResult[2].transform.localPosition += new Vector3(HitResult[2].sprite.bounds.size.x / 2 * 90, 0);
        HitResult2P[2].transform.localPosition += new Vector3(HitResult[2].sprite.bounds.size.x / 2 * 90, 0);

        BasicInputScript.Input.Player.RightDon.performed += Click;
        BasicInputScript.Input.Player.Cancel.performed += Click;
        BasicInputScript.Input.Player.Enter.performed += Click;

        BasicInputScript.Input.Player.Esc.performed += Click;

        AutoMark.gameObject.SetActive(GameSetting.Config.Special == Special.AutoPlay);
        StealMark.gameObject.SetActive(config.Steal);
        AutoMark2P.gameObject.SetActive(GameSetting.Special2P == Special.AutoPlay);
        StealMark2P.gameObject.SetActive(GameSetting.Steal2P);
        if (config.Speed != Speed.Normal)
        {
            SpeedMark.gameObject.SetActive(true);
            int index = (int)config.Speed - 2;
            if ((int)config.Speed > 10) index -= 6;
            SpeedMark.sprite = SpeedSprites[index];
        }
        if (GameSetting.Speed2P != Speed.Normal)
        {
            SpeedMark2P.gameObject.SetActive(true);
            int index = (int)GameSetting.Speed2P - 2;
            if ((int)GameSetting.Speed2P > 10) index -= 6;
            SpeedMark2P.sprite = SpeedSprites[index];
        }
        if (config.Random != RandomType.None)
        {
            RandomMark.gameObject.SetActive(true);
            RandomMark.sprite = RandomSprites[(int)config.Random - 1];
        }
        if (GameSetting.Random2P != RandomType.None)
        {
            RandomMark2P.gameObject.SetActive(true);
            RandomMark2P.sprite = RandomSprites[(int)GameSetting.Random2P - 1];
        }
        if ((Score)GameSetting.Config.ScoreMode == CommonClass.Score.Shin)
        {
            ShinMark.gameObject.SetActive(true);
            ShinMark2P.gameObject.SetActive(true);
        }
        if (config.Reverse)
        {
            ReverseMark.gameObject.SetActive(true);
        }
        if (GameSetting.Revers2P)
        {
            ReverseMark2P.gameObject.SetActive(true);
        }

        DualResultStartScript.ShowResult += DelayStart;
        if (GameSetting.Config.DirectInput)
            BasicInputScript.KeyInvoke += DirectKey;
    }

    void OnDestroy()
    {
        DualResultStartScript.ShowResult -= DelayStart;
        BasicInputScript.KeyInvoke -= DirectKey;
    }
    private void DelayStart()
    {
        Animator.enabled = true;
    }

    public override void StartShow()
    {
        received = true;
        StartCoroutine("ShowResult");
    }
    public override void AvatarShow()
    {
        Avatar.ResultStart();
        Avatar2P.ResultStart();
    }
    IEnumerator ShowResult()
    {
        //计算魂槽
        GaugeBar.UpdateGauge(GloableAniControllerScript.GaugePoints);
        GaugeBar2P.UpdateGauge(GloableAniControllerScript.GaugePoints2P);

        yield return new WaitForSeconds(0.1f);
        ScoreIncreasing.Play();

        //score

        Score.Count = GameResultStartScript.Score;
        Score2P.Count = DualResultStartScript.Score2P;
        Score.gameObject.SetActive(true);
        Score2P.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        ScoreIncreasing.Play();

        int bad = PlayNoteScript.Result[HitNoteResult.Bad].Count;
        int good = PlayNoteScript.Result[HitNoteResult.Good].Count;
        int perfect = PlayNoteScript.Result[HitNoteResult.Perfect].Count;

        int bad2 = PlayNoteScript.Result2P[HitNoteResult.Bad].Count;
        int good2 = PlayNoteScript.Result2P[HitNoteResult.Good].Count;
        int perfect2 = PlayNoteScript.Result2P[HitNoteResult.Perfect].Count;

#if UNITY_EDITOR
        Debug.Log(string.Format("player1 perfect {0} good {1} bad {2} combo {3} hits {4}", perfect, good, bad, GameResultStartScript.MaxCombo, GameResultStartScript.DrumRolls));
        Debug.Log(string.Format("player2 perfect {0} good {1} bad {2} combo {3} hits {4}", perfect2, good2, bad2, DualResultStartScript.MaxCombo2P, DualResultStartScript.DrumRolls2P));
#endif

        //perfect
        Perfect.Count = perfect;
        Perfect2P.Count = perfect2;
        Good.Count = good;
        Good2P.Count = good2;
        Bad.Count = bad;
        Bad2P.Count = bad2;

        Perfect.gameObject.SetActive(true);
        Perfect2P.gameObject.SetActive(true);
        Good.gameObject.SetActive(true);
        Good2P.gameObject.SetActive(true);
        Bad.gameObject.SetActive(true);
        Bad2P.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.1f);
        ScoreIncreasing.Play();

        //combo
        Combo.Count = GameResultStartScript.MaxCombo;
        Combo2P.Count = DualResultStartScript.MaxCombo2P;
        Combo.gameObject.SetActive(true);
        Combo2P.gameObject.SetActive(true);

        //hits
        RapidHit.Count = GameResultStartScript.DrumRolls;
        RapidHit2P.Count = DualResultStartScript.DrumRolls2P;
        RapidHit.gameObject.SetActive(true);
        RapidHit2P.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.1f);
        ScoreIncreasing.Play();

        yield return new WaitForSeconds(0.4f);

        int clear_gauge, clear_gauge_2p;
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
        switch (GameSetting.Difficulty2P)
        {
            case Difficulty.Easy:
                clear_gauge_2p = 6000;
                break;
            case Difficulty.Normal:
            case Difficulty.Hard:
                clear_gauge_2p = 7000;
                break;
            default:
                clear_gauge_2p = 8000;
                break;
        }

        bool full = false, full2 = false;
        if (GloableAniControllerScript.GaugePoints >= clear_gauge)
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
        }
        else
        {
            Avatar.ResultFail();
            int index = Random.Range(0, Fail_pre.Length);
            Instantiate(Fail_pre[index], CrownParent);
        }

        if (GloableAniControllerScript.GaugePoints2P >= clear_gauge_2p)
        {
            if (GameSetting.Special2P == Special.AutoPlay)
                Instantiate(Vol2P[0], CrownParent2P);
            else
            {
                if (bad2 == 0)
                {
                    full2 = true;
                    Instantiate(Vol2P[1], CrownParent2P);
                }
                else
                {
                    Instantiate(Vol2P[2], CrownParent2P);
                }
            }

            if (full2)
                Avatar2P.Full();
            else
                Avatar2P.Clear();
        }
        else
        {
            Avatar2P.ResultFail();
            int index = Random.Range(3, 5);
            Instantiate(Vol2P[index], CrownParent2P);
        }

        all_phase = true;
    }

    private void Click(CallbackContext context)
    {
        Skip();
    }

    public override void Skip()
    {
        if (!Application.isFocused || !received || quiting) return;
        ScoreIncreasing.Stop();
        if (all_phase)
        {
            PlaySound(StartSoundControllScript.SoundType.Cancel);
            quiting = true;
            Mask.SetActive(true);
            StartCoroutine(Quit());
        }
        else
        {
            StopCoroutine("ShowResult");
            PlaySound(StartSoundControllScript.SoundType.Don);
            received = false;
            all_phase = true;
            //计算魂槽
            GaugeBar.UpdateGauge(GloableAniControllerScript.GaugePoints);
            GaugeBar2P.UpdateGauge(GloableAniControllerScript.GaugePoints2P);

            //score
            Score.Count = GameResultStartScript.Score;
            Score2P.Count = DualResultStartScript.Score2P;
            Score.gameObject.SetActive(true);
            Score2P.gameObject.SetActive(true);

            int bad = PlayNoteScript.Result[HitNoteResult.Bad].Count;
            int good = PlayNoteScript.Result[HitNoteResult.Good].Count;
            int perfect = PlayNoteScript.Result[HitNoteResult.Perfect].Count;

            int bad2 = PlayNoteScript.Result2P[HitNoteResult.Bad].Count;
            int good2 = PlayNoteScript.Result2P[HitNoteResult.Good].Count;
            int perfect2 = PlayNoteScript.Result2P[HitNoteResult.Perfect].Count;

            //perfect
            Perfect.Count = perfect;
            Perfect2P.Count = perfect2;
            Good.Count = good;
            Good2P.Count = good2;
            Bad.Count = bad;
            Bad2P.Count = bad2;

            Perfect.gameObject.SetActive(true);
            Perfect2P.gameObject.SetActive(true);
            Good.gameObject.SetActive(true);
            Good2P.gameObject.SetActive(true);
            Bad.gameObject.SetActive(true);
            Bad2P.gameObject.SetActive(true);

            //combo
            Combo.Count = GameResultStartScript.MaxCombo;
            Combo2P.Count = DualResultStartScript.MaxCombo2P;
            Combo.gameObject.SetActive(true);
            Combo2P.gameObject.SetActive(true);

            //hits
            RapidHit.Count = GameResultStartScript.DrumRolls;
            RapidHit2P.Count = DualResultStartScript.DrumRolls2P;
            RapidHit.gameObject.SetActive(true);
            RapidHit2P.gameObject.SetActive(true);

            int clear_gauge, clear_gauge_2p;
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
            switch (GameSetting.Difficulty2P)
            {
                case Difficulty.Easy:
                    clear_gauge_2p = 6000;
                    break;
                case Difficulty.Normal:
                case Difficulty.Hard:
                    clear_gauge_2p = 7000;
                    break;
                default:
                    clear_gauge_2p = 8000;
                    break;
            }

            bool full = false, full2 = false;
            if (GloableAniControllerScript.GaugePoints >= clear_gauge)
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
            }
            else
            {
                Avatar.ResultFail();
                int index = Random.Range(0, Fail_pre.Length);
                Instantiate(Fail_pre[index], CrownParent);
            }

            if (GloableAniControllerScript.GaugePoints2P >= clear_gauge_2p)
            {
                if (GameSetting.Special2P == Special.AutoPlay)
                    Instantiate(Vol2P[0], CrownParent2P);
                else
                {
                    if (bad2 == 0)
                    {
                        full2 = true;
                        Instantiate(Vol2P[1], CrownParent2P);
                    }
                    else
                    {
                        Instantiate(Vol2P[2], CrownParent2P);
                    }
                }

                if (full2)
                    Avatar2P.Full();
                else
                    Avatar2P.Clear();
            }
            else
            {
                Avatar2P.ResultFail();
                int index = Random.Range(3, 5);
                Instantiate(Vol2P[index], CrownParent2P);
            }

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
        string scene = GameSetting.Style == GameStyle.NijiiroStyle ? "SongSelectVertical" : "SongSelect";

        UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
    }
}
