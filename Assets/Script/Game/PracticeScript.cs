using CommonClass;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PracticeScript : MonoBehaviour
{
    public Transform GogoPanel;
    public GameObject GogoMark_pre;

    public Transform Loop1;
    public Transform Loop2;

    public Transform TimeBar;

    public Text Total;
    public Text Current;

    public HitResultScript[] PerfectPool;
    public HitResultScript[] GoodPool;
    public HitResultScript[] BadPool;
    public HitResultScript[] OutRangePool;
    public Transform ResultPanel;

    private List<HitResultScript> results = new List<HitResultScript>();
    private Queue<HitResultScript> p_results = new Queue<HitResultScript>();
    private Queue<HitResultScript> g_results = new Queue<HitResultScript>();
    private Queue<HitResultScript> b_results = new Queue<HitResultScript>();
    private Queue<HitResultScript> o_results = new Queue<HitResultScript>();

    private const float length = 1364;
    private int current = 0;

    private int start;
    private WaitForSeconds wait = new WaitForSeconds(0.1f);
    void Start()
    {
        foreach (HitResultScript result in PerfectPool)
        {
            result.Init();
            p_results.Enqueue(result);
        }
        foreach (HitResultScript result in GoodPool)
        {
            result.Init();
            g_results.Enqueue(result);
        }
        foreach (HitResultScript result in BadPool)
        {
            result.Init();
            b_results.Enqueue(result);
        }
        foreach (HitResultScript result in OutRangePool)
        {
            result.Init();
            o_results.Enqueue(result);
        }

        ChapterLineScript.SetChapter += SetChapter;
        LoaderScript.Play += SetVariant;

    }

    void OnDestroy()
    {
        ChapterLineScript.SetChapter -= SetChapter;
        LoaderScript.Play -= SetVariant;
    }

    private void SetVariant()
    {
        Current.text = "0";
        Total.text = LoaderScript.Lines.Count.ToString();

        foreach (CChip chip in LoaderScript.Others)
        {
            if (chip.Type == CChip.CType.GOGO_TIME)
            {
                GameObject mark = Instantiate(GogoMark_pre, GogoPanel);
                mark.transform.localPosition = new Vector3(-length / 2 + length * chip.StartChapter / LoaderScript.Lines.Count, 0);
            }
        }

        StartCoroutine(Caculate());
    }

    IEnumerator Caculate()
    {
        while (true)
        {
            yield return wait;

            if (InputScript.Holding == InputScript.HoldStatus.Playing && LoaderScript.Lines.Count > current)
            {
                float last = ((Time.time - PlayNoteScript.LastStart) * 1000 * PlayNoteScript.TimeScale) - (current == 0 ? 0 : LoaderScript.Lines[current - 1].JudgeTime);
                float x = current == 0 ? 0 : (float)current / LoaderScript.Lines.Count;
                float app = last / (LoaderScript.Lines[current].JudgeTime - (current == 0 ? 0 : LoaderScript.Lines[current - 1].JudgeTime)) / LoaderScript.Lines.Count;
                //Debug.Log(app);
                x += app;

                TimeBar.localScale = new Vector3(x, 1);
            }
        }
    }

    public void SetChapter(int chapter)
    {
        current = chapter;
        Current.text = chapter.ToString();
        TimeBar.localScale = new Vector3(chapter == 0 ? 0 : (float)chapter / LoaderScript.Lines.Count, 1);
    }

    public void SetLoop(bool on, int start = 0, int end = 0)
    {
        Loop1.gameObject.SetActive(on);
        Loop2.gameObject.SetActive(on);

        if (on)
        {
            this.start = start;
            Loop1.localPosition = new Vector3(-length / 2 + (start == 0 ? 0 : length * start / LoaderScript.Lines.Count), 0);
            Loop2.localPosition = new Vector3(-length / 2 + (end == 0 ? 0 : length * end / LoaderScript.Lines.Count), 0);
        }
    }

    public void ShowResult(bool don, HitNoteResult state, string message)
    {
        if (results.Count >= 10)
        {
            HitResultScript result = results[0];
            results.RemoveAt(0);
            result.gameObject.SetActive(false);
            switch (result.State)
            {
                case HitNoteResult.Perfect:
                    p_results.Enqueue(result);
                    break;
                case HitNoteResult.Good:
                    g_results.Enqueue(result);
                    break;
                case HitNoteResult.Bad:
                    b_results.Enqueue(result);
                    break;
                case HitNoteResult.None:
                    o_results.Enqueue(result);
                    break;
            }
        }

        HitResultScript show = null;
        switch (state)
        {
            case HitNoteResult.Perfect:
                show = p_results.Dequeue();
                break;
            case HitNoteResult.Good:
                show = g_results.Dequeue();
                break;
            case HitNoteResult.Bad:
                show = b_results.Dequeue();
                break;
            case HitNoteResult.None:
                show = o_results.Dequeue();
                break;
        }
        show.SetResult(don, message);
        results.Add(show);

        ArrengeResults();
    }

    private void ArrengeResults()
    {
        for (int i = 0; i < results.Count; i++)
            results[i].transform.localPosition = new Vector3(0, 226 - 50 * i);
    }

    public void ResetResult()
    {
        foreach (HitResultScript result in results)
            result.gameObject.SetActive(false);
        results.Clear();

        p_results.Clear();
        foreach (HitResultScript result in PerfectPool)
            p_results.Enqueue(result);
        g_results.Clear();
        foreach (HitResultScript result in GoodPool)
            g_results.Enqueue(result);
        b_results.Clear();
        foreach (HitResultScript result in BadPool)
            b_results.Enqueue(result);
        o_results.Clear();
        foreach (HitResultScript result in OutRangePool)
            o_results.Enqueue(result);
    }
}
