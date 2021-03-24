using UnityEngine;

public class LoopScript : MonoBehaviour
{
    public OptionScript Option;
    public PlayNoteScript Play;
    public GloableAniControllerScript GloableAni;

    //private readonly float time_before_start = 1f + (15000f / 120f * (4f / 4f)) * 16f / 1000;
    void Update()
    {
        if (Option.Loop && InputScript.Holding == InputScript.HoldStatus.Playing && !InputScript.Quiting)
        {
            float time;
            if (Option.EndChap < LoaderScript.Lines.Count)
                time = (LoaderScript.Lines[Option.EndChap].JudgeTime + GameSetting.Config.NoteAdjust) / 1000 / PlayNoteScript.TimeScale;
            else
            {
                time = (LoaderScript.Others[LoaderScript.Others.Count - 1].JudgeTime + GameSetting.Config.NoteAdjust - 1000) / 1000 / PlayNoteScript.TimeScale;
            }
            
            if (Time.time - PlayNoteScript.LastStart >= time)
            {
                Play.Hold(true);
                InputScript.Holding = InputScript.HoldStatus.None;
                GloableAni.Restart(Option.StartChap);
            }
        }
    }
}
