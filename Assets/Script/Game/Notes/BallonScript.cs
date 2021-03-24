using CommonClass;
using System.Collections;
using UnityEngine;

public class BallonScript : NoteSoundScript
{
    public float EndTime { set; get; }
    public int HitCount { set; get; }
    public int RapidHit { set; get; }
    protected float rate;
    protected int Phase;

    public override void Prepare()
    {
        rate = (float)HitCount / 5;
        if (Scroll < 0)
        {
            ShowImages[0].flipX = true;
            ShowImages[1].flipX = true;
            ShowImages[1].transform.localPosition = new Vector3(Mathf.Abs(ShowImages[1].transform.localPosition.x), ShowImages[1].transform.localPosition.y);
        }

        base.Prepare();
    }

    public override void Play()
    {
        Hitted = false;
        acitive = true;
        if (Show && !Replay)
        {
            if ((!Player2 && GameSetting.Config.Special == Special.AutoPlay) || Player2 && GameSetting.Special2P == Special.AutoPlay)
                StartCoroutine(HitBalloon());
        }
    }

    public override void AdjustPostion()
    {
        if (Playing) return;
        Phase = 0;
        RapidHit = 0;
        Hitted = false;
        if (Show)
        {
            foreach (SpriteRenderer image in ShowImages)
                image.enabled = !Steal;
            foreach (SpriteRenderer image in SeNotes)
                image.enabled = true;
        }
        float time = JudgeTime + Adjust - PlayNoteScript.CurrentTime * 1000;
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
    /*
    public override void UpdateStatus(float time, float time2)
    {
        if (!Playing) return;
        if (!acitive && time <= -(GameSetting.Config.Special == Special.AutoPlay ? 0 : GameSetting.Config.JudgeTimeAdjust))
        {
            Play();
        }

        if (!Hitted && time2 < 0)
        {
            if (!Player2)
            {
                NotesSplitScript.NormalNotes[true].Remove(this);
                NotesSplitScript.MasterNotes[true].Remove(this);
                NotesSplitScript.ExpertNotes[true].Remove(this);
            }
            if (Show && !Replay)
            {
                StopAllCoroutines();
                BalloonBroken?.Invoke(this, BalloonResult.Fail);
            }
            Hitted = true;
        }

        if (transform.localPosition.x < -7)
        {
            Playing = false;
            gameObject.SetActive(false);
        }
    }
    */
    private void Update()
    {
        //main image update
        int index = Player2 ? NoteTickScript.ComboType2P : NoteTickScript.ComboType;
        if (image_type != index)
        {
            image_type = index;
            ShowImages[0].sprite = ImageSprites[image_type];
        }

        if (Move_2_chpater && To_chapter_time > Time.time)
        {
            float target_time = JudgeTime + Adjust - PlayNoteScript.CurrentTime * 1000;
            float target_x;
            if (target_time >= 0)
                target_x = (float)(target_time * Bpm * Scroll * (1 + 1.5f)) / 628.7f * 1.5f / 100;
            else
                    {
                target_time = EndTime + Adjust - PlayNoteScript.CurrentTime * 1000;
                if (target_time >= 0)
                    target_x = 0;
                else
                    target_x = (float)(target_time * Bpm * Scroll * (1 + 1.5f)) / 628.7f * 1.5f / 100;
            }

            transform.localPosition = new Vector3(OriginPostion + (target_x - OriginPostion) * (1 - (To_chapter_time - Time.time) / 0.3f), 0, Z_Value);
        }

        if (!Playing) return;
        float time = JudgeTime + Adjust - ((Time.time - PlayNoteScript.LastStart) * 1000 * PlayNoteScript.TimeScale);

        if (!acitive && time <= - (GameSetting.Config.Special == Special.AutoPlay ? 0 : GameSetting.Config.JudgeTimeAdjust))
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
                transform.localPosition = new Vector3(0, 0, Z_Value);
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
            else if (!Replay)
            {
                DualNotesSplitScript.NormalNotes2P[true].Remove(this);
                DualNotesSplitScript.MasterNotes2P[true].Remove(this);
                DualNotesSplitScript.ExpertNotes2P[true].Remove(this);
            }

            if (Show && !Replay)
            {
                StopAllCoroutines();

                if (!Player2)
                    GloableAniControllerScript.Instance.BrokenBalloon(this, BalloonResult.Fail);
                else
                    GloableAniControllerScript.Instance.BrokenBalloon2P(this, BalloonResult.Fail);

                //foreach (SpriteRenderer image in ShowImages)
                //image.enabled = !GameSetting.Steal;
                //foreach (SpriteRenderer image in SeNotes)
                //    image.enabled = true;
            }
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
        base.Move2Chapter(time);
    }

    IEnumerator HitBalloon()
    {
        if (HitCount > 0)
        {
            float start_time = Time.time;
            float wait = (EndTime + Adjust - (start_time - PlayNoteScript.LastStart) * 1000 * PlayNoteScript.TimeScale) / 1000 / (HitCount + 1 + HitCount / 3) / PlayNoteScript.TimeScale;

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
    
    public override void Hit()
    {
        if (RapidHit == 0)
        {
            if (Show)
            {
                foreach (SpriteRenderer image in ShowImages)
                    image.enabled = false;
                foreach (SpriteRenderer image in SeNotes)
                    image.enabled = false;
            }
            if (Player2)
                GloableAniControllerScript.Instance.ShowBalloon2P(this, 0);
            else
                GloableAniControllerScript.Instance.ShowBalloon(this, 0);
        }

        RapidHit++;
        if (Phase < 5 && RapidHit >= HitCount)
        {
            Phase = 5;
            if (!Replay)
            {
                if (!Player2)
                    GloableAniControllerScript.Instance.BrokenBalloon(this, BalloonResult.Good);
                else
                    GloableAniControllerScript.Instance.BrokenBalloon2P(this, BalloonResult.Good);
            }
            Hitted = true;

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
        else if (Phase < 4 && RapidHit >= rate * 4)
        {
            Phase = 4;
            if (Player2)
                GloableAniControllerScript.Instance.ShowBalloon2P(this, 4);
            else
                GloableAniControllerScript.Instance.ShowBalloon(this, 4);
        }
        else if (Phase < 3 && RapidHit >= rate * 3)
        {
            Phase = 3;
            if (Player2)
                GloableAniControllerScript.Instance.ShowBalloon2P(this, 3);
            else
                GloableAniControllerScript.Instance.ShowBalloon(this, 3);
        }
        else if (Phase < 2 && RapidHit >= rate * 2)
        {
            Phase = 2;
            if (Player2)
                GloableAniControllerScript.Instance.ShowBalloon2P(this, 2);
            else
                GloableAniControllerScript.Instance.ShowBalloon(this, 2);
        }
        else if (Phase == 0 && RapidHit >= rate)
        {
            Phase = 1;
            if (Player2)
                GloableAniControllerScript.Instance.ShowBalloon2P(this, 1);
            else
                GloableAniControllerScript.Instance.ShowBalloon(this, 1);
        }
    }
}
