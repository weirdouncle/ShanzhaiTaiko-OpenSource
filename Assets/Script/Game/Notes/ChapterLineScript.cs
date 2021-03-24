using UnityEngine;

public delegate void SetChapterDelegate(int index);
public delegate void SetChapter2PDelegate(int index);

public class ChapterLineScript : MonoBehaviour
{
    public static event SetChapterDelegate SetChapter;
    public static event SetChapter2PDelegate SetChapter2P;
    public bool Show
    {
        get => _show;
        set
        {
            _show = value;
            Image.enabled = _show;
            Text.gameObject.SetActive(_show && GameSetting.Mode == CommonClass.PlayMode.Practice);
        }
    }
    public SpriteNumberScript Text;
    public SpriteRenderer Image;

    public bool Player2 { set; get; }
    public bool Replay { set; get; }
    public double Bpm { set; get; }
    public float AppearTime { set; get; }
    public float MoveTime { set; get; }
    public int Chapter
    {
        set
        {
            _chapter = value;
            Text.Count = value;
        }
        get { return _chapter; }
    }
    public double Scroll { set; get; }
    public float JudgeTime { set; get; }
    public int WaitingTime { set; get; } = 0;
    public int Adjust { set; get; }

    private bool hitted = false;
    private int _chapter;
    private bool _show = true;
    protected bool playing = false;
    protected bool stop = false;
    void Start()
    {
        if (GameSetting.Mode != CommonClass.PlayMode.Practice)
            Text.gameObject.SetActive(false);
    }
    public virtual void Prepare()
    {
        hitted = false;
        float time = JudgeTime + Adjust;
        //float x = (float)(time * Bpm * Scroll * (1 + 1.5f)) / 628.7f * 1.5f / PlayNoteScript.TimeScale / 100;
        float x = (float)(time * Bpm * Scroll * (1 + 1.5f)) / 628.7f * 1.5f / 100;
        if (Scroll >= 0)
        {
            if (x > 14)
            {
                x = 14;
                if (Chapter > 1) gameObject.SetActive(false);
            }
        }
        else
        {
            if (x < -4)
            {
                x = -4;
                if (Chapter > 1) gameObject.SetActive(false);
            }
        }
        transform.localPosition = new Vector3(x, 0);
    }

    public void StartMoving()
    {
        playing = true;
        if (Chapter == 1 && !gameObject.activeSelf && ((Scroll >= 0 && transform.localPosition.x > -4) || (Scroll < 0 && transform.localPosition.x < 14)))
            gameObject.SetActive(true);
    }
    public void Hold()
    {
        playing = false;
        float time = JudgeTime - PlayNoteScript.CurrentTime * 1000 + Adjust;
        //transform.localPosition = new Vector3((float)(time * Bpm * Scroll * (1 + 1.5f)) / 628.7f * 1.5f / PlayNoteScript.TimeScale / 100, 0);
        transform.localPosition = new Vector3((float)(time * Bpm * Scroll * (1 + 1.5f)) / 628.7f * 1.5f / 100, 0);
    }

    public void Reposition(bool restart = false)
    {
        if (playing && !restart) return;

        float time = JudgeTime - PlayNoteScript.CurrentTime * 1000 + Adjust;
        //float x = (float)(time * Bpm * Scroll * (1 + 1.5f)) / 628.7f * 1.5f / PlayNoteScript.TimeScale / 100;
        float x = (float)(time * Bpm * Scroll * (1 + 1.5f)) / 628.7f * 1.5f / 100;
        transform.localPosition = new Vector3(x, 0);
        hitted = time < -0.01f;

        if (Scroll >= 0)
        {
            if (Chapter == 1 && x > -4)
                gameObject.SetActive(true);
            else
                gameObject.SetActive(x > -4 && x <= 14);
        }
        else
        {
            if (Chapter == 1 && x < 14)
                gameObject.SetActive(true);
            else
                gameObject.SetActive(x > -4 && x <= 14);
        }

        //if (gameObject.activeSelf)
        //    Debug.Log(string.Format("chapter {2} time {0} hit {1}", time, hitted, Chapter));
    }

    private void Update()
    {
        if (!playing) return;
        float time = JudgeTime + Adjust - ((Time.time - PlayNoteScript.LastStart) * 1000 * PlayNoteScript.TimeScale);
        if (!hitted && time <= 0)
        {
            hitted = true;
            if (!Replay)
            {
                if (!Player2)
                    SetChapter?.Invoke(Chapter);
                else
                    SetChapter2P?.Invoke(Chapter);
            }
        }
        //transform.localPosition = new Vector3((float)(time * Bpm * Scroll * (1 + 1.5f)) / 628.7f * 1.5f / PlayNoteScript.TimeScale / 100, 0);
        transform.localPosition = new Vector3((float)(time * Bpm * Scroll * (1 + 1.5f)) / 628.7f * 1.5f / 100, 0);

        if (stop || (Scroll >= 0 && transform.localPosition.x < -4) || (Scroll < 0 && transform.localPosition.x > 14))
        {
            playing = false;
            gameObject.SetActive(false);
        }
    }
}
