using CommonClass;
using System.Collections;
using UnityEngine;

public class RapidScript : NoteSoundScript
{
    public float EndTime { set; get; }
    public int RapidHit { set; get; }

    public SpriteRenderer Body;
    public SpriteRenderer Tail;
    public Transform Se;
    public int Width;

    private WaitForSeconds wait = new WaitForSeconds(0.06f);
    protected override void SetSeNote()
    {
        switch (Type)
        {
            case 5:
                SeNotes[0].sprite = SettingLoader.SeNotes[7];
                break;
            case 6:
                SeNotes[0].sprite = SettingLoader.SeNotes[8];
                break;
        }
        SeNotes[1].sprite = SettingLoader.SeNotes[9];
        SeNotes[2].sprite = SettingLoader.SeNotes[10];
    }

    public override void Play()
    {
        acitive = true;
        if (Show && !Replay && ((!Player2 && GameSetting.Config.Special == Special.AutoPlay) || (Player2 && GameSetting.Special2P == Special.AutoPlay)))
            StartCoroutine(HitRapid());
    }
    /*
    public override void UpdateStatus(float time, float time2)
    {
        if (!Playing) return;
        if (!acitive && time <= 0)
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

                NotesSplitScript.NormalNotes[false].Remove(this);
                NotesSplitScript.MasterNotes[false].Remove(this);
                NotesSplitScript.ExpertNotes[false].Remove(this);
            }

            if (Show && !Replay) RapidShow?.Invoke(false);
            Hitted = true;
        }

        if (Tail.transform.position.x < -7)
        {
            Playing = false;
            gameObject.SetActive(false);
        }
    }
    */
    void Update()
    {
        //main image update
        int index;
        if (Player2)
        {
            if (Type == 6)
                index = NoteTickScript.ComboTypeBig2P;
            else
                index = NoteTickScript.ComboType2P;
        }
        else
        {
            if (Type == 6)
                index = NoteTickScript.ComboTypeBig;
            else
                index = NoteTickScript.ComboType;
        }
        if (image_type != index)
        {
            image_type = index;
            ShowImages[0].sprite = ImageSprites[image_type];
        }

        if (Move_2_chpater && To_chapter_time > Time.time)
        {
            float target_time = JudgeTime + Adjust - PlayNoteScript.CurrentTime * 1000;
            //float target_x = (float)(target_time * Bpm * Scroll * (1 + 1.5f)) / 628.7f * 1.5f / PlayNoteScript.TimeScale / 100;
            float target_x = (float)(target_time * Bpm * Scroll * (1 + 1.5f)) / 628.7f * 1.5f / 100;

            transform.localPosition = new Vector3(OriginPostion + (target_x - OriginPostion) * (1 - (To_chapter_time - Time.time) / 0.3f), 0, Z_Value);
        }

        if (!Playing) return;
        float time = JudgeTime + Adjust - ((Time.time - PlayNoteScript.LastStart) * 1000 * PlayNoteScript.TimeScale);
        //transform.localPosition = new Vector3((float)(time * Bpm * Scroll * (1 + 1.5f)) / 628.7f * 1.5f / PlayNoteScript.TimeScale / 100, 0, Z_Value);
        transform.localPosition = new Vector3((float)(time * Bpm * Scroll * (1 + 1.5f)) / 628.7f * 1.5f / 100, 0, Z_Value);
        if (!acitive && time <= 0)
        {
            Play();
        }

        if (!Hitted && EndTime + Adjust - ((Time.time - PlayNoteScript.LastStart) * 1000 * PlayNoteScript.TimeScale)
            + (GameSetting.Config.Special == Special.AutoPlay ? 0 : GameSetting.Config.JudgeTimeAdjust) < 0)
        {
            if (!Player2)
            {
                NotesSplitScript.NormalNotes[true].Remove(this);
                NotesSplitScript.MasterNotes[true].Remove(this);
                NotesSplitScript.ExpertNotes[true].Remove(this);

                NotesSplitScript.NormalNotes[false].Remove(this);
                NotesSplitScript.MasterNotes[false].Remove(this);
                NotesSplitScript.ExpertNotes[false].Remove(this);
            }
            else if (!Replay)
            {
                DualNotesSplitScript.NormalNotes2P[true].Remove(this);
                DualNotesSplitScript.MasterNotes2P[true].Remove(this);
                DualNotesSplitScript.ExpertNotes2P[true].Remove(this);

                DualNotesSplitScript.NormalNotes2P[false].Remove(this);
                DualNotesSplitScript.MasterNotes2P[false].Remove(this);
                DualNotesSplitScript.ExpertNotes2P[false].Remove(this);
            }

            if (Show && !Replay)
            {
                if (!Player2)
                    GloableAniControllerScript.Instance.ShowRapid(false);
                else
                    GloableAniControllerScript.Instance.ShowRapid2P(false);
            }

            Hitted = true;
        }

        if ((Scroll >= 0 && Tail.transform.position.x < -7) || (Scroll < 0 && Tail.transform.position.x > 14))
        {
            Playing = false;
            gameObject.SetActive(false);
        }
    }

    public override void Move2Chapter(float time)
    {
#if UNITY_ANDROID
        ColorBack();
#endif
        RapidHit = 0;
        base.Move2Chapter(time);
    }
    public override void Reposition()
    {
#if UNITY_ANDROID
        ColorBack();
#endif
        base.Reposition();
    }
#if UNITY_ANDROID
    private WaitForSeconds wait2 = new WaitForSeconds(0.02f);
    private void ColorBack()
    {
        Body.color = Tail.color = new Color(1, 1, 1, 1);
    }

    IEnumerator ColorRed()
    {
        for (int i = 0; i < 10; i++)
        {
            yield return wait2;
            Body.color = Tail.color -= new Color(0, 0.1f, 0.1f, 0);
        }
    }
#endif
    public override void AdjustPostion()
    {
        base.AdjustPostion();
        RapidHit = 0;

        //float length = (float)((EndTime - JudgeTime) * Bpm * Scroll * (1 + 1.5f)) / 628.7f * 1.5f / PlayNoteScript.TimeScale / 100;
        float length = (float)((EndTime - JudgeTime) * Bpm * Scroll * (1 + 1.5f)) / 628.7f * 1.5f / 100;
        float offset = SeNotes[0].sprite.bounds.size.x / 2;
        Body.transform.localScale = new Vector3(length / Width * 100, 1);
        Se.localScale = new Vector3(Mathf.Max(0, (length - offset) / 0.2f), 1);

        if (Scroll >= 0)
        {
            Se.localPosition = new Vector3(offset, Se.localPosition.y);
            Tail.transform.localPosition = new Vector3(length, Tail.transform.localPosition.y);
        }
        else
        {
            Se.localPosition = new Vector3(-offset, Se.localPosition.y);
            Tail.transform.localPosition = new Vector3(-length, Tail.transform.localPosition.y);
        }
    }

    public override void Prepare()
    {
        base.Prepare();
        RapidHit = 0;

        //float length = (float)((EndTime - JudgeTime) * Bpm * Scroll * (1 + 1.5f)) / 628.7f * 1.5f / PlayNoteScript.TimeScale / 100;
        float length = (float)((EndTime - JudgeTime) * Bpm * Scroll * (1 + 1.5f)) / 628.7f * 1.5f / 100;
        float offset = SeNotes[0].sprite.bounds.size.x / 2;
        Body.transform.localScale = new Vector3(length / Width * 100, 1);
        Se.localScale = new Vector3(Mathf.Max(0, (length - offset) / 0.2f), 1);

        if (Scroll >= 0)
        {
            Se.localPosition = new Vector3(offset, Se.localPosition.y);
            Tail.transform.localPosition = new Vector3(length, Tail.transform.localPosition.y);
        }
        else
        {
            foreach (SpriteRenderer renderer in ShowImages)
                renderer.flipX = true;
            foreach (SpriteRenderer renderer in SeNotes)
                renderer.flipX = true;

            Se.localPosition = new Vector3(-offset, Se.localPosition.y);
            Tail.transform.localPosition = new Vector3(-length, Tail.transform.localPosition.y);
        }
    }

    IEnumerator HitRapid()
    {
        float now = Time.time;
        while (Time.time - now < (EndTime + Adjust - (now - PlayNoteScript.LastStart) * 1000 * PlayNoteScript.TimeScale) / 1000 / PlayNoteScript.TimeScale && Playing)
        {
            if (!Player2)
                InputScript.Instance.PlayDrum(Type);
            else
                InputScript.Instance.PlayDrum2P(Type);

            yield return wait;
        }
    }

    public override void Hit()
    {
#if UNITY_ANDROID
        if (RapidHit == 0)
            StartCoroutine(ColorRed());
#endif
        RapidHit++;
    }
}
