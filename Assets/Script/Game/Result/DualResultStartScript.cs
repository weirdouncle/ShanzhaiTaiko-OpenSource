using CommonClass;
using UnityEngine;

public class DualResultStartScript : GameResultStartScript
{
    public static event ShowResultDelegate ShowResult;
    public delegate void ShowResultDelegate();

    public static int Score2P;
    public static int DrumRolls2P;
    public static int MaxCombo2P;
    public static NijiiroRank Rank2P;

    public bool RecordScore;

    void Start()
    {
    }

    void OnDestroy()
    {
        if (PlayNoteScript.Result != null)
            PlayNoteScript.Result.Clear();

        if (PlayNoteScript.Result2P != null)
            PlayNoteScript.Result2P.Clear();
    }

    public override void StartAnimating()
    {
        Score = GloableAni.TotalScore;
        DrumRolls = GloableAni.DrumRolls;
        MaxCombo = GloableAni.MaxCombo;
        Rank = GloableAni.Rank;

        Score2P = GloableAni.TotalScore2P;
        DrumRolls2P = GloableAni.DrumRolls2P;
        MaxCombo2P = GloableAni.MaxCombo2P;
        Rank2P = GloableAni.Rank2P;

        if (RecordScore)
        {
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
#if UNITY_EDITOR
            if (GameSetting.Mode != CommonClass.PlayMode.Replay)
                GameSetting.SetScore(GameSetting.SelectedInfo.Title, GameSetting.Config.PlayerName,
                    GameSetting.Difficulty, clear, full, Score, perfect, good, bad, MaxCombo, DrumRolls, (int)Rank);
#else
            if (GameSetting.Config.Special != Special.AutoPlay && GameSetting.Mode != CommonClass.PlayMode.Replay)
                GameSetting.SetScore(GameSetting.SelectedInfo.Title, GameSetting.Config.PlayerName,
                    GameSetting.Difficulty, clear, full, Score, perfect, good, bad, MaxCombo, DrumRolls, (int)Rank);
#endif
            if (GameSetting.Config.Special != Special.AutoPlay && GameSetting.Mode != CommonClass.PlayMode.Replay && Score > OldScore) SaveReplay();
        }

        Animator.enabled = true;
    }

    public void StartShowResult()
    {
        ShowResult?.Invoke();
    }
}