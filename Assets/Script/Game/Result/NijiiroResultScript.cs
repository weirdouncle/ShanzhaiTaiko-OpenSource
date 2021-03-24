using CommonClass;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class NijiiroResultScript : GameResultScript
{
    public Sprite[] DiffMarks;
    public Transform RankParent;
    public NijiiroResultPaopaoScript Paopao;
    public Sprite ClearBg;
    public SpriteRenderer BackGround;
    public GameObject Clear;
    public SpriteRenderer[] DiffPic1P;
    public SpriteRenderer[] DiffPic2P;
    public GameObject[] BestUpdates;
    public SpriteNumberScript[] BestUpdateCount;
    public GameObject[] Flowers;
    public NijiiroGameReuslt2PScript Player2;

    private bool score_phase;
    private bool rank_phase;
    private bool crown_phase;
    private int clear_gauge;

    /*
    private Dictionary<int, Vector2> mask_positon = new Dictionary<int, Vector2>
    {
        { 1, new Vector2(409, -871.3f) },
        { 2, new Vector2(464.7f, -847.3f) },
        { 3, new Vector2(520.4f, -823.3f) },
        { 4, new Vector2(576.1f, -799.3f) },
        { 5, new Vector2(631.8f, -775.3f) },
        { 6, new Vector2(687.5f, -751.3f) },
        { 7, new Vector2(743.2f, -727.3f) },
    };
    */
    void OnDestroy()
    {
        BasicInputScript.KeyInvoke -= DirectKey;
        DualResultStartScript.ShowResult -= DelayStart;
    }

    void Start()
    {
        Avatar.ResultStart();
        DualResultStartScript.ShowResult += DelayStart;

        Texts[0].text = GameSetting.Translate("hits");
        Player2.Texts[0].text = GameSetting.Translate("hits");
        Texts[1].text = GameSetting.Translate("max combo");
        Player2.Texts[1].text = GameSetting.Translate("hits");
        Texts[2].text = GameSetting.Translate("score");
        Player2.Texts[2].text = GameSetting.Translate("hits");
        Texts[3].text = GameSetting.Translate("record update");

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

        DiffPic1P[0].sprite = DiffMarks[(int)GameSetting.Difficulty];
        DiffPic1P[1].sprite = SettingLoader.Diffs[(int)GameSetting.Difficulty];

        switch (GameSetting.Difficulty)
        {
            case Difficulty.Easy:
                {
                    GameObject gauge = Instantiate(Gauges[0], GaugeParent);
                    GaugeBar = gauge.GetComponent<CourseGaugeScript>();
                    clear_gauge = 6000;
                }
                break;
            case Difficulty.Normal:
            case Difficulty.Hard:
                {
                    GameObject gauge = Instantiate(Gauges[1], GaugeParent);
                    GaugeBar = gauge.GetComponent<CourseGaugeScript>();
                    clear_gauge = 7000;
                }
                break;
            default:
                {
                    GameObject gauge = Instantiate(Gauges[2], GaugeParent);
                    GaugeBar = gauge.GetComponent<CourseGaugeScript>();
                    clear_gauge = 8000;
                }
                break;
        }

        HitResult[0].sprite = Player2.HitResult[0].sprite = SettingLoader.NoteResults[0];
        RectTransform p_rect = HitResult[0].GetComponent<RectTransform>();
        RectTransform p2_rect = Player2.HitResult[0].GetComponent<RectTransform>();
        p_rect.sizeDelta = p2_rect.sizeDelta = new Vector2(HitResult[0].sprite.bounds.size.x * 100, HitResult[0].sprite.bounds.size.y * 100);
        HitResult[0].transform.localPosition += new Vector3(HitResult[0].sprite.bounds.size.x / 2 * 70, 0);
        Player2.HitResult[0].transform.localPosition += new Vector3(Player2.HitResult[0].sprite.bounds.size.x / 2 * 70, 0);

        HitResult[1].sprite = Player2.HitResult[1].sprite = SettingLoader.NoteResults[1];
        RectTransform g_rect = HitResult[1].GetComponent<RectTransform>();
        RectTransform g2_rect = Player2.HitResult[1].GetComponent<RectTransform>();
        g_rect.sizeDelta = g2_rect.sizeDelta = new Vector2(HitResult[1].sprite.bounds.size.x * 100, HitResult[1].sprite.bounds.size.y * 100);
        HitResult[1].transform.localPosition += new Vector3(HitResult[1].sprite.bounds.size.x / 2 * 70, 0);
        Player2.HitResult[1].transform.localPosition += new Vector3(Player2.HitResult[1].sprite.bounds.size.x / 2 * 70, 0);

        HitResult[2].sprite = Player2.HitResult[2].sprite = SettingLoader.NoteResults[2];
        RectTransform b_rect = HitResult[2].GetComponent<RectTransform>();
        RectTransform b2_rect = Player2.HitResult[2].GetComponent<RectTransform>();
        b_rect.sizeDelta = b2_rect.sizeDelta = new Vector2(HitResult[2].sprite.bounds.size.x * 100, HitResult[2].sprite.bounds.size.y * 100);
        HitResult[2].transform.localPosition += new Vector3(HitResult[2].sprite.bounds.size.x / 2 * 70, 0);
        Player2.HitResult[2].transform.localPosition += new Vector3(Player2.HitResult[2].sprite.bounds.size.x / 2 * 70, 0);


#if !UNITY_ANDROID
        BasicInputScript.Input.Player.RightDon.performed += Click;
        BasicInputScript.Input.Player.Cancel.performed += Click;
        BasicInputScript.Input.Player.Enter.performed += Click;
#endif

        BasicInputScript.Input.Player.Esc.performed += Click;

        if (GameSetting.Config.DirectInput)
            BasicInputScript.KeyInvoke += DirectKey;

        List<Transform> marks = new List<Transform>();
        if (GameSetting.Config.Special == Special.AutoPlay)
        {
            AutoMark.gameObject.SetActive(true);
            marks.Add(AutoMark.transform);
        }
        if (config.Steal)
        {
            StealMark.gameObject.SetActive(true);
            marks.Add(StealMark.transform);
        }
        if (config.Speed != Speed.Normal)
        {
            SpeedMark.gameObject.SetActive(true);
            int index = (int)config.Speed - 2;
            if ((int)config.Speed > 10) index -= 6;
            SpeedMark.sprite = SpeedSprites[index];
            marks.Add(SpeedMark.transform);
        }
        if (config.Random != RandomType.None)
        {
            RandomMark.gameObject.SetActive(true);
            RandomMark.sprite = RandomSprites[(int)config.Random - 1];
            marks.Add(RandomMark.transform);
        }
        if (config.Reverse)
        {
            ReverseMark.gameObject.SetActive(true);
            marks.Add(ReverseMark.transform);
        }

        if ((Score)GameSetting.Config.ScoreMode == CommonClass.Score.Shin)
        {
            ShinMark.gameObject.SetActive(true);
            marks.Add(ShinMark.transform);
        }
        if (GameSetting.Mode == CommonClass.PlayMode.Replay)
        {
            ModeMark.gameObject.SetActive(true);
            marks.Add(ModeMark.transform);
        }

        for (int i = 0; i < marks.Count; i++)
            marks[i].localPosition = new Vector3(-880 + i * 24, marks[i].localPosition.y);

        /*
        if (marks.Count > 0)
        {
            foreach (Transform mask in MarkMasks)
                mask.gameObject.SetActive(true);
            MarkMasks[0].localScale = new Vector3(mask_positon[marks.Count].x, MarkMasks[0].localScale.y);
            MarkMasks[1].localPosition = new Vector3(mask_positon[marks.Count].y, MarkMasks[1].localPosition.y);
        }
        */

        if (GameSetting.Player2)
        {
            Animator.gameObject.SetActive(false);
            Player2.SetPlayer2(this);
            Player2.DiffChar[0].sprite = DiffMarks[(int)GameSetting.Difficulty2P];
            Player2.DiffChar[1].sprite = SettingLoader.Diffs[(int)GameSetting.Difficulty2P];
        }
    }

    private void DelayStart()
    {
        received = true;
        StartCoroutine("ShowResult");
    }

    IEnumerator ShowResult()
    {
        //计算魂槽
        float gauge = GloableAniControllerScript.GaugePoints;
        float gauge_2p = GloableAniControllerScript.GaugePoints2P;
        int gauge_count = (int)gauge / 200;
        int gauge_count_2p = 0;
        if (GameSetting.Player2) gauge_count_2p = (int)gauge_2p / 200;

        if (gauge_count > 0 || gauge_count_2p > 0)
        {
            ScoreIncreasing.cueName = "GaugeIncreasing";
            ScoreIncreasing.Play();
        }
        int gauge_grow = 0;
        int gauge_add_count = Mathf.Max(gauge_count, gauge_count_2p);
        for (int i = 1; i <= gauge_add_count; i++)
        {
            gauge_grow += 200;
            yield return new WaitForSeconds(0.045f);
            if (gauge_count >= gauge_add_count)
                GaugeBar.UpdateGauge(gauge_grow);
            if (gauge_count_2p >= gauge_add_count)
                Player2.gauge_bar_2p.UpdateGauge(gauge_grow);
        }
        if (gauge_add_count > 0)
        {
            ScoreIncreasing.Stop();
            if (gauge == 10000 || gauge_2p == 10000)
            {
                ScoreIncreasing.cueName = "gauge_fire";
                ScoreIncreasing.Play();
            }
        }
        yield return new WaitForSeconds(0.8f);

        int bad = PlayNoteScript.Result[HitNoteResult.Bad].Count;
        int good = PlayNoteScript.Result[HitNoteResult.Good].Count;
        int perfect = PlayNoteScript.Result[HitNoteResult.Perfect].Count;

        int bad_2p = PlayNoteScript.Result2P[HitNoteResult.Bad].Count;
        int good_2p = PlayNoteScript.Result2P[HitNoteResult.Good].Count;
        int perfect_2p = PlayNoteScript.Result2P[HitNoteResult.Perfect].Count;

        //perfect
        ScoreIncreasing.cueName = "nijiiro_score";
        ScoreIncreasing.Play();
        Perfect.Count = perfect;
        Perfect.gameObject.SetActive(true);

        if (GameSetting.Player2)
        {
            Player2.Numbers[0].Count = perfect_2p;
            Player2.Numbers[0].gameObject.SetActive(true);
        }
        yield return new WaitForSeconds(0.35f);

        //good
        ScoreIncreasing.Play();
        Good.Count = good;
        Good.gameObject.SetActive(true);
        if (GameSetting.Player2)
        {
            Player2.Numbers[1].Count = good_2p;
            Player2.Numbers[1].gameObject.SetActive(true);
        }
        yield return new WaitForSeconds(0.35f);

        //bad
        ScoreIncreasing.Play();
        Bad.Count = bad;
        Bad.gameObject.SetActive(true);
        if (GameSetting.Player2)
        {
            Player2.Numbers[2].Count = bad_2p;
            Player2.Numbers[2].gameObject.SetActive(true);
        }
        yield return new WaitForSeconds(0.35f);

        //hits
        RapidHit.Count = GameResultStartScript.DrumRolls;
        ScoreIncreasing.Play();
        RapidHit.gameObject.SetActive(true);
        if (GameSetting.Player2)
        {
            Player2.Numbers[3].Count = DualResultStartScript.DrumRolls2P;
            Player2.Numbers[3].gameObject.SetActive(true);
        }
        yield return new WaitForSeconds(0.35f);

        //combo
        Combo.Count = GameResultStartScript.MaxCombo;
        ScoreIncreasing.Play();
        Combo.gameObject.SetActive(true);
        if (GameSetting.Player2)
        {
            Player2.Numbers[4].Count = DualResultStartScript.MaxCombo2P;
            Player2.Numbers[4].gameObject.SetActive(true);
        }
        yield return new WaitForSeconds(1);

        //score
#if UNITY_EDITOR
        if (!GameSetting.Player2 && GameSetting.Mode != CommonClass.PlayMode.Replay && GameResultStartScript.Score > GameResultStartScript.OldScore)
        {
            BestUpdateCount[0].Count = GameResultStartScript.Score - GameResultStartScript.OldScore;
            BestUpdates[0].SetActive(true);
        }
#else
        if (!GameSetting.Player2 && GameSetting.Config.Special != Special.AutoPlay && GameSetting.Mode != CommonClass.PlayMode.Replay
             && GameResultStartScript.OldScore > 0 && GameResultStartScript.Score > GameResultStartScript.OldScore)
        {
            BestUpdateCount[0].Count = GameResultStartScript.Score - GameResultStartScript.OldScore;
            BestUpdates[0].SetActive(true);
        }
#endif
        Score.Count = GameResultStartScript.Score;
        Score.gameObject.SetActive(true);
        if (GameSetting.Player2)
        {
            Player2.Numbers[5].Count = DualResultStartScript.Score2P;
            Player2.Numbers[5].gameObject.SetActive(true);
        }
        PlaySound(StartSoundControllScript.SoundType.Don);
        score_phase = true;

        yield return new WaitForSeconds(1);
        /*
        if (GameSetting.Config.Special != Special.AutoPlay && GameSetting.Mode != CommonClass.PlayMode.Replay && GameResultStartScript.Score > GameResultStartScript.OldScore)
        {
            Best.gameObject.SetActive(true);
            yield return new WaitForSeconds(1);
        }
        */
#if UNITY_EDITOR
        Debug.Log(string.Format("perfect {0} good {1} bad {2} combo {3} hits {4}", perfect, good, bad, GameResultStartScript.MaxCombo, GameResultStartScript.DrumRolls));
#endif
        bool wait = false;
        if (GameResultStartScript.Rank > NijiiroRank.None)
        {
            Instantiate(Fail_pre[(int)GameResultStartScript.Rank - 1], RankParent);
            wait = true;
        }
        if (GameSetting.Player2 && DualResultStartScript.Rank2P > NijiiroRank.None)
        {
            Instantiate(Fail_pre[(int)DualResultStartScript.Rank2P - 1], Player2.RankParent);
            wait = true;
        }
        rank_phase = true;
        if (wait)
            yield return new WaitForSeconds(1.5f);

        int result_index = 3;
        if (gauge >= clear_gauge)
        {
            if (bad == 0 && good == 0)
            {
                result_index = 0;
                Instantiate(RobotFullClip_pre, CrownParent);
            }
            else if (bad == 0)
            {
                result_index = 1;
                Instantiate(FullClip_pre, CrownParent);
            }
            else
            {
                result_index = 2;
                Instantiate(ClearClip_pre, CrownParent);
            }
        }

        int result_index_2p = 3;
        if (GameSetting.Player2)
        {
            if (gauge_2p >= Player2.clear_gauge_2p)
            {
                if (bad_2p == 0 && good_2p == 0)
                {
                    result_index_2p = 0;
                    Instantiate(RobotFullClip_pre, Player2.CrownParent);
                }
                else if (bad == 0)
                {
                    result_index_2p = 1;
                    Instantiate(FullClip_pre, Player2.CrownParent);
                }
                else
                {
                    result_index_2p = 2;
                    Instantiate(ClearClip_pre, Player2.CrownParent);
                }
            }
        }

        crown_phase = true;
        yield return new WaitForSeconds(1.5f);

        Paopao.SetPaopao(result_index, false);
        if (gauge >= clear_gauge)
        {
            Avatar.Clear();
            BackGround.sprite = ClearBg;
            Animator.enabled = true;
            Clear.SetActive(!GameSetting.Player2 && true);
            Flowers[0].SetActive(true);
        }
        else
            Avatar.ResultFail();

        if (GameSetting.Player2)
        {
            Paopao.SetPaopao(result_index_2p, true);
            if (gauge_2p >= Player2.clear_gauge_2p)
            {
                Player2.DonAvatar.Clear();
                Player2.BackGround.sprite = ClearBg;
                Flowers[1].SetActive(true);
            }
            else
                Player2.DonAvatar.ResultFail();
        }

        all_phase = true;
    }

    private void Click(CallbackContext context)
    {
        Skip();
    }

    protected override void DirectKey(GameSetting.KeyType key, bool press)
    {
        Skip();
    }

    private void SkipResult()
    {
        received = false;
        ScoreIncreasing.Stop();
        StopAllCoroutines();

        float gauge = GloableAniControllerScript.GaugePoints;
        int bad = PlayNoteScript.Result[HitNoteResult.Bad].Count;
        int good = PlayNoteScript.Result[HitNoteResult.Good].Count;
        int perfect = PlayNoteScript.Result[HitNoteResult.Perfect].Count;

        float gauge_2p = GloableAniControllerScript.GaugePoints2P;
        int bad_2p = PlayNoteScript.Result2P[HitNoteResult.Bad].Count;
        int good_2p = PlayNoteScript.Result2P[HitNoteResult.Good].Count;
        int perfect_2p = PlayNoteScript.Result2P[HitNoteResult.Perfect].Count;

        if (!score_phase)
        {
            GaugeBar.UpdateGauge(gauge);
            if (GameSetting.Player2)
                Player2.gauge_bar_2p.UpdateGauge(gauge_2p);


            //perfect
            Perfect.Count = perfect;
            Perfect.gameObject.SetActive(true);

            //good
            Good.Count = good;
            Good.gameObject.SetActive(true);

            //bad
            Bad.Count = bad;
            Bad.gameObject.SetActive(true);

            //hits
            RapidHit.Count = GameResultStartScript.DrumRolls;
            RapidHit.gameObject.SetActive(true);


            //combo
            Combo.Count = GameResultStartScript.MaxCombo;
            Combo.gameObject.SetActive(true);

            //score
            Score.Count = GameResultStartScript.Score;
            Score.gameObject.SetActive(true);

            if (GameSetting.Player2)
            {
                Player2.Numbers[0].Count = perfect_2p;
                Player2.Numbers[0].gameObject.SetActive(true);

                Player2.Numbers[1].Count = good_2p;
                Player2.Numbers[1].gameObject.SetActive(true);

                Player2.Numbers[2].Count = bad_2p;
                Player2.Numbers[2].gameObject.SetActive(true);

                Player2.Numbers[3].Count = DualResultStartScript.DrumRolls2P;
                Player2.Numbers[3].gameObject.SetActive(true);

                Player2.Numbers[4].Count = DualResultStartScript.MaxCombo2P;
                Player2.Numbers[4].gameObject.SetActive(true);

                Player2.Numbers[5].Count = DualResultStartScript.Score2P;
                Player2.Numbers[5].gameObject.SetActive(true);
            }
            score_phase = true;
        }

        if (!rank_phase)
        {
            if (GameResultStartScript.Rank > NijiiroRank.None)
                Instantiate(Fail_pre[(int)GameResultStartScript.Rank - 1], RankParent);

            if (GameSetting.Player2 && DualResultStartScript.Rank2P > NijiiroRank.None)
                Instantiate(Fail_pre[(int)DualResultStartScript.Rank2P - 1], Player2.RankParent);
        }
        rank_phase = true;

        int result_index = 3;
        if (gauge >= clear_gauge)
        {
            GameObject crown;
            if (bad == 0 && good == 0)
            {
                result_index = 0;
                crown = RobotFullClip_pre;
            }
            else if (bad == 0)
            {
                result_index = 1;
                crown = FullClip_pre;
            }
            else
            {
                result_index = 2;
                crown = ClearClip_pre;
            }

            if (!crown_phase) Instantiate(crown, CrownParent);
        }

        int result_index_2p = 3;
        if (GameSetting.Player2)
        {
            if (gauge_2p >= Player2.clear_gauge_2p)
            {
                if (bad_2p == 0 && good_2p == 0)
                {
                    result_index_2p = 0;
                    Instantiate(RobotFullClip_pre, Player2.CrownParent);
                }
                else if (bad == 0)
                {
                    result_index_2p = 1;
                    Instantiate(FullClip_pre, Player2.CrownParent);
                }
                else
                {
                    result_index_2p = 2;
                    Instantiate(ClearClip_pre, Player2.CrownParent);
                }
            }
        }
        crown_phase = true;

        Paopao.SetPaopao(result_index, false);
        if (gauge >= clear_gauge)
        {
            Avatar.Clear();
            BackGround.sprite = ClearBg;
            Animator.enabled = true;
            Clear.SetActive(true);
            Flowers[0].SetActive(true);
        }
        else
            Avatar.ResultFail();

        if (GameSetting.Player2)
        {
            Paopao.SetPaopao(result_index_2p, true);
            if (gauge_2p >= Player2.clear_gauge_2p)
            {
                Player2.DonAvatar.Clear();
                Player2.BackGround.sprite = ClearBg;
                Flowers[1].SetActive(true);
            }
            else
                Player2.DonAvatar.ResultFail();
        }

        PlaySound(StartSoundControllScript.SoundType.Don);
        all_phase = true;

        StartCoroutine(DelayReceiv());
    }

    IEnumerator DelayReceiv()
    {
        yield return new WaitForSeconds(0.5f);
        received = true;
    }

    public override void Skip()
    {
        if (!Application.isFocused || !received || quiting) return;

        if (!all_phase)
            SkipResult();
        else
            Skip2Quit();
    }

    private void Skip2Quit()
    {
        PlaySound(StartSoundControllScript.SoundType.Cancel);
        quiting = true;
        Mask.SetActive(true);
        StartCoroutine(Quit());
    }

    IEnumerator Quit()
    {
        yield return new WaitForSeconds(0.5f);
        GameResultStartScript.DestroyInstance();
        yield return new WaitForSeconds(1f);
        string scene = GameSetting.Style == CommonClass.GameStyle.NijiiroStyle ? "SongSelectVertical" : "SongSelect";
#if UNITY_ANDROID
        scene = "AndroidSelectVertical";
        //scene = "AndroidSongSelect";
#endif
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
    }
}
