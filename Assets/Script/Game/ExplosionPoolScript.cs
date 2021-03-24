using CommonClass;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionPoolScript : MonoBehaviour
{
    public int Pool = 30;
    public GameObject ExpGood_pre;
    public GameObject ExpGreat_pre;
    public GameObject ExpBigGood_pre;
    public GameObject ExpBigGreat_pre;
    public GameObject Bad_pre;

    public NoteExplosionScript NoteExp;

    private Queue<JudgeExplosionScript> bad = new Queue<JudgeExplosionScript>();
    private Queue<JudgeExplosionScript> good = new Queue<JudgeExplosionScript>();
    private Queue<JudgeExplosionScript> perfect = new Queue<JudgeExplosionScript>();
    private Queue<JudgeExplosionScript> big_good = new Queue<JudgeExplosionScript>();
    private Queue<JudgeExplosionScript> big_perfect = new Queue<JudgeExplosionScript>();
    private List<JudgeExplosionScript> all = new List<JudgeExplosionScript>();

    private WaitForEndOfFrame wait = new WaitForEndOfFrame();
    private float index = 0;
    void Start()
    {
        StartCoroutine(Instantiate());
    }
    IEnumerator Instantiate()
    {
        int count = 0;
        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        yield return wait;

        float volum = GameSetting.Config.EffectVolume;
        for (int i = 0; i < Pool; i++)
        {
            if (count >= 5)
            {
                count = 0;
                yield return wait;
            }
            GameObject game = Instantiate(Bad_pre, transform);
            JudgeExplosionScript script = game.GetComponent<JudgeExplosionScript>();
            script.ShowEnd += OnPlayEnd;
            bad.Enqueue(script);
            script.Init();
            all.Add(script);

            game = Instantiate(ExpGood_pre, transform);
            script = game.GetComponent<JudgeExplosionScript>();
            script.ShowEnd += OnPlayEnd;
            good.Enqueue(script);
            script.Init();
            all.Add(script);

            game = Instantiate(ExpGreat_pre, transform);
            script = game.GetComponent<JudgeExplosionScript>();
            script.ShowEnd += OnPlayEnd;
            perfect.Enqueue(script);
            script.Init();
            all.Add(script);

            game = Instantiate(ExpBigGood_pre, transform);
            script = game.GetComponent<JudgeExplosionScript>();
            script.ShowEnd += OnPlayEnd;
            big_good.Enqueue(script);
            script.Init();
            all.Add(script);

            game = Instantiate(ExpBigGreat_pre, transform);
            script = game.GetComponent<JudgeExplosionScript>();
            script.ShowEnd += OnPlayEnd;
            big_perfect.Enqueue(script);
            script.Init();
            all.Add(script);
            count++;
        }
    }

    void OnDestroy()
    {
        foreach (JudgeExplosionScript judge in all)
        {
            judge.ShowEnd -= OnPlayEnd;
            Destroy(judge.gameObject);
        }
    }

    public void ResetTimeLine()
    {
        index = 0;
    }

    public void Show(HitNoteResult state, NoteSoundScript note)
    {
        StartCoroutine(ShowDelay(state, note));
    }

    IEnumerator ShowDelay(HitNoteResult state, NoteSoundScript note)
    {
        yield return wait;
        NoteExp.Show(state, note);
        bool big = note.Type == 3 || note.Type == 4 || note.Type == 6;

        switch (state)
        {
            case HitNoteResult.Bad:
                if (bad.Count > 0)
                {
                    JudgeExplosionScript script = bad.Dequeue();
                    script.transform.localPosition = new Vector3(0, 0, index);
                    script.Show();
                }
                break;
            case HitNoteResult.Good:
                if ((big && big_good.Count > 0) || (!big && good.Count > 0))
                {
                    JudgeExplosionScript script = big ? big_good.Dequeue() : good.Dequeue();
                    script.transform.localPosition = new Vector3(0, 0, index);
                    script.Show();
                }
                break;
            case HitNoteResult.Perfect:
                if ((big && big_perfect.Count > 0) || (!big && perfect.Count > 0))
                {
                    JudgeExplosionScript script = big ? big_perfect.Dequeue() : perfect.Dequeue();
                    script.transform.localPosition = new Vector3(0, 0, index);
                    script.Show();
                }
                break;
        }
        index -= 0.001f;
    }

    public void OnPlayEnd(JudgeExplosionScript exp)
    {
        switch (exp.Type)
        {
            case 0:
                bad.Enqueue(exp);
                break;
            case 1:
                good.Enqueue(exp);
                break;
            case 2:
                perfect.Enqueue(exp);
                break;
            case 3:
                big_good.Enqueue(exp);
                break;
            case 4:
                big_perfect.Enqueue(exp);
                break;
        }
    }
}
