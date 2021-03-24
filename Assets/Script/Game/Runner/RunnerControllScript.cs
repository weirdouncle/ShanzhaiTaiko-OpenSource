using System;
using System.Collections.Generic;
using UnityEngine;

public class RunnerControllScript : MonoBehaviour
{
    public GameObject Bad_pre;
    public GameObject[] Goods;
    public int Pools = 20;

    protected Queue<RunnerScript> runner0 = new Queue<RunnerScript>();
    protected List<RunnerScript> all = new List<RunnerScript>();
    protected Dictionary<int, Queue<RunnerScript>> all_list = new Dictionary<int, Queue<RunnerScript>>();
    protected float z_value = 0;

    private List<int> all_index = new List<int>();
    void Awake()
    {
        for (int i = 0; i < Pools * Goods.Length; i++)
        {
            GameObject add = Instantiate(Bad_pre, transform);
            RunnerScript script = add.GetComponent<RunnerScript>();
            runner0.Enqueue(script);
            script.ShowEnd += OnPlayEnd;
            all.Add(script);
        }

        for (int index = 0; index < Goods.Length; index++)
        {
            Queue<RunnerScript> que = new Queue<RunnerScript>();
            for (int i = 0; i < Pools; i++)
            {
                GameObject add = Instantiate(Goods[index], transform);
                RunnerScript script = add.GetComponent<RunnerScript>();
                script.ShowEnd += OnPlayEnd;
                all.Add(script);
                que.Enqueue(script);
            }
            all_list.Add(index, que);
        }

        for (int i = 0; i < Goods.Length; i++)
            all_index.Add(i);
    }

    public void ResetBpm()
    {
        foreach (RunnerScript script in all)
            script.Animator.speed = 1;
    }

    public void SetBpm(double bpm)
    {
        foreach (RunnerScript script in all)
            script.Animator.speed = (float)bpm / 60;
    }

    void OnDestroy()
    {
        foreach (RunnerScript script in all)
        {
            script.ShowEnd -= OnPlayEnd;
            Destroy(script.gameObject);
        }
    }

    public void ResetTimeLine()
    {
        z_value = 0;

        runner0.Clear();
        for (int i = 0; i < all_list.Keys.Count; i++)
            all_list[i].Clear();

        foreach (RunnerScript runner in all)
        {
            runner.Hide();
            switch (runner.Type)
            {
                case 0:
                    runner0.Enqueue(runner);
                    break;
                default:
                    all_list[runner.Type - 1].Enqueue(runner);
                    break;
            }
        }
    }

    public virtual void OnPlay(bool good)
    {
        if (!good)
        {
            if (runner0.Count > 0)
            {
                RunnerScript runner = runner0.Dequeue();
                runner.transform.localPosition = new Vector3(0, 0, z_value);
                z_value -= 0.0001f;
                runner.Play();
            }
        }
        else
        {
            List<int> all = new List<int>(all_index);
            while (all.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, all.Count);
                int last_index = all[index];
                if (all_list[last_index].Count > 0)
                {
                    RunnerScript runner = all_list[last_index].Dequeue();
                    runner.transform.localPosition = new Vector3(0, 0, z_value);
                    z_value -= 0.0001f;
                    runner.Play();
                    break;
                }
                else
                {
                    all.Remove(last_index);
                }
            }
        }
    }
    private void OnPlayEnd(RunnerScript runner)
    {
        switch (runner.Type)
        {
            case 0:
                runner0.Enqueue(runner);
                break;
            default:
                all_list[runner.Type - 1].Enqueue(runner);
                break;
        }
    }
}
