using CommonClass;
using System.Collections;
using UnityEngine;

public class KusudamaNoteScript : BallonScript
{
    public float MidTime { set;get; }

    public override void Play()
    {
        Hitted = false;
        acitive = true;
        if (Show)
        {
            foreach (SpriteRenderer image in ShowImages)
                image.enabled = false;
            foreach (SpriteRenderer image in SeNotes)
                image.enabled = false;

            float time = !Player2 && GameSetting.Config.Special == Special.AutoPlay ? 0 : Player2 && GameSetting.Special2P == Special.AutoPlay ? 0 : GameSetting.Config.JudgeTimeAdjust;
            if (!Replay && EndTime + Adjust - 50
                - ((Time.time - PlayNoteScript.LastStart) * 1000 * PlayNoteScript.TimeScale) > -(time))
            {
                if (!Player2)
                    GloableAniControllerScript.Instance.ShowBalloon(this, 0);

                if ((!Player2 && GameSetting.Config.Special == Special.AutoPlay) || Player2 && GameSetting.Special2P == Special.AutoPlay)
                    StartCoroutine(HitBalloon());
            }
        }
    }

    public override void AdjustPostion()
    {
        RapidHit = 0;
        Hitted = false;

        if (Show)
        {
            foreach (SpriteRenderer image in ShowImages)
                image.enabled = !Steal;
            foreach (SpriteRenderer image in SeNotes)
                image.enabled = true;
        }

        float time = JudgeTime - PlayNoteScript.CurrentTime * 1000;
        if (time >= 0)
            //transform.localPosition = new Vector3((float)(time * Bpm * Scroll * (1 + 1.5f)) / 628.7f * 1.5f / PlayNoteScript.TimeScale / 100, 0, Z_Value);
            transform.localPosition = new Vector3((float)(time * Bpm * Scroll * (1 + 1.5f)) / 628.7f * 1.5f / 100, 0, Z_Value);
        else
        {
            time = EndTime + Adjust - PlayNoteScript.CurrentTime * 1000;
            if (time >= 0)
                transform.localPosition = new Vector3(0, 0, Z_Value);
            else
                //transform.localPosition = new Vector3((float)(time * Bpm * Scroll * (1 + 1.5f)) / 628.7f * 1.5f / PlayNoteScript.TimeScale / 100, 0, Z_Value);
                transform.localPosition = new Vector3((float)(time * Bpm * Scroll * (1 + 1.5f)) / 628.7f * 1.5f / 100, 0, Z_Value);
        }
    }

    void Update()
    {
        if (Move_2_chpater && To_chapter_time > Time.time)
        {
            float target_time = JudgeTime + Adjust - PlayNoteScript.CurrentTime * 1000;
            float target_x;
            if (target_time >= 0)
                //target_x = (float)(target_time * Bpm * Scroll * (1 + 1.5f)) / 628.7f * 1.5f / PlayNoteScript.TimeScale / 100;
                target_x = (float)(target_time * Bpm * Scroll * (1 + 1.5f)) / 628.7f * 1.5f / 100;
            else
            {
                target_time = EndTime + Adjust - PlayNoteScript.CurrentTime * 1000;
                if (target_time >= 0)
                    target_x = 0;
                else
                    //target_x = (float)(target_time * Bpm * Scroll * (1 + 1.5f)) / 628.7f * 1.5f / PlayNoteScript.TimeScale / 100;
                    target_x = (float)(target_time * Bpm * Scroll * (1 + 1.5f)) / 628.7f * 1.5f / 100;
            }

            transform.localPosition = new Vector3(OriginPostion + (target_x - OriginPostion) * (1 - (To_chapter_time - Time.time) / 0.3f), 0, Z_Value);
        }

        if (!Playing) return;
        float time = JudgeTime + Adjust - ((Time.time - PlayNoteScript.LastStart) * 1000 * PlayNoteScript.TimeScale);
        
        if (!acitive && time <= -(GameSetting.Config.Special == Special.AutoPlay ? 0 : GameSetting.Config.JudgeTimeAdjust))
        {
            Play();
        }

        if (time >= 0)
            //transform.localPosition = new Vector3((float)(time * Bpm * Scroll * (1 + 1.5f)) / 628.7f * 1.5f / PlayNoteScript.TimeScale / 100, 0, Z_Value);
            transform.localPosition = new Vector3((float)(time * Bpm * Scroll * (1 + 1.5f)) / 628.7f * 1.5f / 100, 0, Z_Value);
        else
        {
            time = EndTime + Adjust - ((Time.time - PlayNoteScript.LastStart) * 1000 * PlayNoteScript.TimeScale);
            if (time >= 0)
                transform.localPosition = new Vector3(0, 0);
            else
                //transform.localPosition = new Vector3((float)(time * Bpm * Scroll * (1 + 1.5f)) / 628.7f * 1.5f / PlayNoteScript.TimeScale / 100, 0, Z_Value);
                transform.localPosition = new Vector3((float)(time * Bpm * Scroll * (1 + 1.5f)) / 628.7f * 1.5f / 100, 0, Z_Value);
        }

        if (!Hitted && EndTime + Adjust - ((Time.time - PlayNoteScript.LastStart) * 1000 * PlayNoteScript.TimeScale)
            + (GameSetting.Config.Special == Special.AutoPlay ? 0 : GameSetting.Config.JudgeTimeAdjust) < 0)
        {
            if (!Player2)
            {
                NotesSplitScript.NormalNotes[true].Remove(this);
                NotesSplitScript.MasterNotes[true].Remove(this);
                NotesSplitScript.ExpertNotes[true].Remove(this);
            }
            else
            {
                DualNotesSplitScript.NormalNotes2P[true].Remove(this);
                DualNotesSplitScript.MasterNotes2P[true].Remove(this);
                DualNotesSplitScript.ExpertNotes2P[true].Remove(this);
            }

            if (Show && !Replay && !Player2) GloableAniControllerScript.Instance.BrokenBalloon(this, BalloonResult.Fail);
            Hitted = true;
        }

        if ((Scroll >= 0 && transform.localPosition.x < -7) || (Scroll < 0 && transform.localPosition.x > 14))
        {
            Playing = false;
            gameObject.SetActive(false);
        }
    }

    public override void Move2Chapter(float time)
    {
        if (Show)
        {
            foreach (SpriteRenderer image in ShowImages)
                image.enabled = !Steal;
            foreach (SpriteRenderer image in SeNotes)
                image.enabled = true;
        }
        RapidHit = 0;
        acitive = false;
        Hitted = false;
        To_chapter_time = Time.time + time;
        OriginPostion = transform.localPosition.x;

        StopAllCoroutines();
        StartCoroutine(ToChpater(time));
    }

    IEnumerator ToChpater(float time)
    {
        Move_2_chpater = true;
        yield return new WaitForSeconds(time);
        Move_2_chpater = false;
        AdjustPostion();
    }
    IEnumerator HitBalloon()
    {
        if (HitCount > 0)
        {
            float start_time = Time.time;
            float last = 0;
            if (MidTime + Adjust > 0) last = MidTime + Adjust - (start_time - PlayNoteScript.LastStart) * 1000 * PlayNoteScript.TimeScale;
            if (last <= 0)
                last = EndTime + Adjust - (start_time - PlayNoteScript.LastStart) * 1000 * PlayNoteScript.TimeScale;

            if (last > 0)
            {
                float wait = last / 1000 / (HitCount + 1 + HitCount / 3) / PlayNoteScript.TimeScale;
                yield return new WaitForSeconds(0.12f);
                for (int i = 0; i < HitCount; i++)
                {
                    if (!Playing) break;

                    if (!Player2)
                        InputScript.Instance.PlayDrum(Type);
                    else
                        InputScript.Instance.PlayDrum2P(Type);

                    yield return new WaitForSeconds(wait);
                }
            }
        }
    }

    public override void Hit()
    {
        RapidHit++;
        /*
        if (RapidHit >= HitCount)
        {
            if (!Replay)
            {
                float time = MidTime - ((Time.time - PlayNoteScript.LastStart) * 1000 * PlayNoteScript.TimeScale)
                    + (GameSetting.Config.Special == Special.AutoPlay ? 0 : GameSetting.Config.JudgeTimeAdjust);
                bool good = MidTime == 0 || time >= 0;

                GloableAniControllerScript.Instance.BrokenBalloon(this, good ? BalloonResult.Perfect : BalloonResult.Good);
            }

            Hitted = true;

            if (!Player2)
            {
                NotesSplitScript.NormalNotes[true].Remove(this);
                NotesSplitScript.MasterNotes[true].Remove(this);
                NotesSplitScript.ExpertNotes[true].Remove(this);
            }

            //State = CommonClass.HitNoteResult.Perfect;
            gameObject.SetActive(false);
        }
        else if (RapidHit > (HitCount * 3 / 4))
        {
            GloableAniControllerScript.Instance.ShowBalloon(this, 4);
        }
        else if (RapidHit > HitCount / 2)
        {
            GloableAniControllerScript.Instance.ShowBalloon(this, 3);
        }
        else if (RapidHit > HitCount / 4)
        {
            GloableAniControllerScript.Instance.ShowBalloon(this, 2);
        }
        else if (RapidHit > 0)
        {
            GloableAniControllerScript.Instance.ShowBalloon(this, 1);
        }
        */
    }

    private void HammerBroken(BalloonResult result)
    {
        if (gameObject.activeSelf && !Hitted && acitive && Show)
        {
            if (!Player2)
            {
                NotesSplitScript.NormalNotes[true].Remove(this);
                NotesSplitScript.MasterNotes[true].Remove(this);
                NotesSplitScript.ExpertNotes[true].Remove(this);
            }
            else if (!Replay)
            {
                DualNotesSplitScript.NormalNotes2P[true].Remove(this);
                DualNotesSplitScript.MasterNotes2P[true].Remove(this);
                DualNotesSplitScript.ExpertNotes2P[true].Remove(this);
            }

            //State = CommonClass.HitNoteResult.Perfect;
            gameObject.SetActive(false);
        }
    }

    public override void Prepare()
    {
        KusudamaShowScript.BalloonSucceed += HammerBroken;
        rate = (float)HitCount / 5;
        base.Prepare();
    }
    void OnEnable()
    {
    }
    void OnDestroy()
    {
        KusudamaShowScript.BalloonSucceed -= HammerBroken;
    }
}
