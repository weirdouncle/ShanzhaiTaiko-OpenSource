using System.Collections;
using UnityEngine;
using CommonClass;
using System;

public class NoteSoundScript : MonoBehaviour
{
    //多线程使用
    //public static event NoteShowDelegate NoteShow;
    //public delegate void NoteShowDelegate(NoteSoundScript note, bool show);

    public enum ComboType
    {
        Combo_None,
        Combo_50,
        Combo_150,
        Combo_300,
    }

    public SpriteRenderer[] ShowImages;
    public SpriteRenderer[] SeNotes;
    public SpriteRenderer Result;
    public Sprite[] ImageSprites;
    public Sprite[] ResultSprites;
    public bool Player2
    {
        get => player2;
        set
        {
            player2 = value;
            //if (player2) ReBind();
        }
    }

    public int Index;
    public int Chapter;
    public float Z_Value { set; get; }
    public float Adjust { set; get; }
    public bool Steal { set; get; }
    public bool Replay { set; get; }
    public int Type { set; get; }
    public bool Gogo { set; get; }
    public double Bpm { set; get; }
    public float AppearTime { set; get; }
    public float MoveTime { set; get; }
    public int ShowChapter { set; get; }
    public double Scroll { set; get; }
    public float fBMSCROLLTime { set; get; }
    public float JudgeTime { set; get; }
    public int WaitingTime { set; get; } = 0;
    public bool Hitted { set; get; } = false;
    public bool Playing { set; get; } = false;
    public float To_chapter_time { set; get; }
    public bool Move_2_chpater { set; get; } = false;
    public float OriginPostion { set; get; }
    public HitPracticeResult State
    {
        get => state;
        set
        {
            state = value;
            SetResult();
        }
    }
    public bool BranchRelated;
    public CourseBranch Branch;
    public bool IsFixedSENote { set; get; }
    public int Senote
    {
        get => senote;
        set
        {
            senote = value;
            SetSeNote();
        }
    }
    public bool Show
    {
        get => _show;
        set
        {
            _show = value;
            foreach (SpriteRenderer image in ShowImages)
                image.enabled = !Steal && value;

            foreach (SpriteRenderer image in SeNotes)
                image.enabled = value;
        }
    }

    protected bool acitive = false;
    protected bool _show = true;
    protected int image_type;
    private bool player2;
    private HitPracticeResult state;
    private int senote;

    public virtual void Play()
    {
        acitive = true;
        if (Show && !Replay)
        {
            if (!Player2 && GameSetting.Config.Special == Special.AutoPlay)
                InputScript.Instance.PlayDrum(Type);
            else if (Player2 && GameSetting.Special2P == Special.AutoPlay)
                InputScript.Instance.PlayDrum2P(Type);
        }
    }

    protected virtual void SetSeNote()
    {
        switch (Type)
        {
            case 1:
                SeNotes[0].sprite = SettingLoader.SeNotes[senote <= 3 ? senote : 0];
                break;
            case 2:
                int index = Math.Max(senote + 1, 4);
                SeNotes[0].sprite = SettingLoader.SeNotes[index <= 6 ? index : 4];
                break;
            case 3:
                SeNotes[0].sprite = SettingLoader.SeNotes[3];
                break;
            case 4:
                SeNotes[0].sprite = SettingLoader.SeNotes[6];
                break;
            case 7:
                SeNotes[0].sprite = SettingLoader.SeNotes[11];
                break;
            case 9:
                SeNotes[0].sprite = SettingLoader.SeNotes[12];
                break;
        }
    }

    private void SetResult()
    {
        if (GameSetting.Mode == CommonClass.PlayMode.Practice && Result != null && state != HitPracticeResult.None)
            Result.sprite = ResultSprites[(int)state - 1];
    }

    public virtual void Prepare()
    {
        foreach (SpriteRenderer image in ShowImages)
            image.enabled = !Steal && _show;

        //State = HitNoteResult.None;
        //gameObject.SetActive(true);
        float time = JudgeTime + Adjust;
        Hitted = false;
        acitive = false;
        //transform.localPosition = new Vector3((float)(time * Bpm * Scroll * (1 + 1.5f)) / 628.7f * 1.5f / PlayNoteScript.TimeScale / 100, 0, Z_Value);
        transform.localPosition = new Vector3((float)(time * Bpm * Scroll * (1 + 1.5f)) / 628.7f * 1.5f / 100, 0, Z_Value);
    }

    public void StartMoving()
    {
        Playing = true;
    }

    public void Hold()
    {
        Playing = false;
    }

    public virtual void AdjustPostion()
    {
        if (Playing) return;
        float time = JudgeTime + Adjust - PlayNoteScript.CurrentTime * 1000;
        //transform.localPosition = new Vector3((float)(time * Bpm * Scroll * (1 + 1.5f)) / 628.7f * 1.5f / PlayNoteScript.TimeScale / 100, 0, Z_Value);
        transform.localPosition = new Vector3((float)(time * Bpm * Scroll * (1 + 1.5f)) / 628.7f * 1.5f / 100, 0, Z_Value);
    }

    public virtual void Reposition()
    {
        acitive = false;
        Hitted = false;
        //if (Animator != null) Animator.SetTrigger("lost");
        AdjustPostion();
    }

    /*
    public virtual void UpdateStatus(float time, float time2)
    {
        if (!Playing) return;
        if (!acitive && time <= 0)
        {
            Play();
        }
        if (!Hitted && time < -GameSetting.Config.JudgeRange[HitNoteResult.Bad] - (GameSetting.Config.Special == Special.AutoPlay ? 0 : GameSetting.Config.JudgeTimeAdjust))
        {
            if (!Player2)
            {
                bool don = Type == 1 || Type == 3;
                NotesSplitScript.NormalNotes[don].Remove(this);
                NotesSplitScript.MasterNotes[don].Remove(this);
                NotesSplitScript.ExpertNotes[don].Remove(this);
            }

            Hitted = true;
            if (Show && !Replay)
            {
                BadNote?.Invoke(this);
                State = HitPracticeResult.Lost;
            }
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
        int index;
        if (Player2)
        {
            if (Type > 2)
                index = NoteTickScript.ComboTypeBig2P;
            else
                index = NoteTickScript.ComboType2P;
        }
        else
        {
            if (Type > 2)
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
            float target_x = (float)(target_time * Bpm * Scroll * (1 + 1.5f)) / 628.7f * 1.5f / 100;

            transform.localPosition = new Vector3(OriginPostion + (target_x - OriginPostion) * (1 - (To_chapter_time - Time.time) / 0.3f), 0, Z_Value);
        }

        if (!Playing) return;
        float time = JudgeTime + Adjust - ((Time.time - PlayNoteScript.LastStart) * 1000 * PlayNoteScript.TimeScale);
        transform.localPosition = new Vector3((float)(time * Bpm * Scroll * (1 + 1.5f)) / 628.7f * 1.5f / 100, 0, Z_Value);
        if (!acitive && time <= 0)
        {
            Play();
        }
        if (!Hitted && time < -GameSetting.Config.JudgeRange[HitNoteResult.Bad] - (GameSetting.Config.Special == Special.AutoPlay ? 0 : GameSetting.Config.JudgeTimeAdjust))
        {
            bool don = Type == 1 || Type == 3;
            if (!Player2)
            {
                NotesSplitScript.NormalNotes[don].Remove(this);
                NotesSplitScript.MasterNotes[don].Remove(this);
                NotesSplitScript.ExpertNotes[don].Remove(this);
            }
            else if (!Replay)
            {
                DualNotesSplitScript.NormalNotes2P[don].Remove(this);
                DualNotesSplitScript.MasterNotes2P[don].Remove(this);
                DualNotesSplitScript.ExpertNotes2P[don].Remove(this);
            }

            Hitted = true;
            if (Show && !Replay)
            {
                if (!Player2)
                    GloableAniControllerScript.Instance.CountResult(this);
                else
                    GloableAniControllerScript.Instance.CountResult2P(this);

                State = HitPracticeResult.Lost;

                //Debug.Log(PlayNoteScript.LastStart);
                //if (DanDojoLoaderScript.DojoNotes[0].ContainsValue(this))
                //    Debug.Log("song 0");
                //if (DanDojoLoaderScript.DojoNotes[1].ContainsValue(this))
                //    Debug.Log("song 1");
                //if (DanDojoLoaderScript.DojoNotes[2].ContainsValue(this))
                //    Debug.Log("song 2");
            }
        }

        if ((Scroll >= 0 && transform.localPosition.x < -7) || (Scroll < 0 && transform.localPosition.x > 14))
        {
            Playing = false;
            gameObject.SetActive(false);
        }
    }
    public virtual void Move2Chapter(float time)
    {
        acitive = false;
        Hitted = false;
        //Animator.SetTrigger("lost");
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

    public virtual void Hit()
    {
        Hitted = true;
    }
    /*
    private void OnEnable()
    {
        //if (ShowChapter < PlayNoteScript.Chapter)
        //    Debug.Log("enable");

        if (Player2)
            NoteTickScript.Note2PTick += StartAnimation;
        else
            NoteTickScript.NoteTick += StartAnimation;

        //NoteShow?.Invoke(this, true);
    }
    private void OnDestroy()
    {
        NoteTickScript.NoteTick -= StartAnimation;
        NoteTickScript.Note2PTick -= StartAnimation;
    }
    private void OnDisable()
    {
        NoteTickScript.NoteTick -= StartAnimation;
        NoteTickScript.Note2PTick -= StartAnimation;

        //NoteShow?.Invoke(this, false);
    }

    private void ReBind()
    {
        if (gameObject.activeSelf)
        {
            NoteTickScript.NoteTick -= StartAnimation;
            NoteTickScript.Note2PTick -= StartAnimation;
            NoteTickScript.Note2PTick += StartAnimation;
        }
    }
    public virtual void StartAnimation(ComboType type)
    {
        if (Animator != null && gameObject.activeSelf)
        {
            switch (type)
            {
                case ComboType.Combo_None:
                    Animator.SetTrigger("lost");
                    break;
                case ComboType.Combo_50:
                    Animator.SetTrigger("50");
                    break;
                case ComboType.Combo_150:
                    Animator.SetTrigger("150");
                    break;
                case ComboType.Combo_300:
                    if (Type == 3 || Type == 4 || Type == 6)
                        Animator.SetTrigger("150");
                    else
                        Animator.SetTrigger("300");
                    break;
            }
        }
    }
    */
}
