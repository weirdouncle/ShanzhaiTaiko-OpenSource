using System.Collections.Generic;
using UnityEngine;

public class MikuRunerScript : RunnerControllScript
{
    private Queue<RunnerScript> runner1 = new Queue<RunnerScript>();
    void Start()
    {
        for (int i = 0; i < Pools; i++)
        {
            GameObject add = Instantiate(Bad_pre, transform);
            RunnerScript script = add.GetComponent<RunnerScript>();
            runner0.Enqueue(script);
            script.ShowEnd += OnPlayEnd;
            all.Add(script);
        }

        for (int i = 0; i < Pools; i++)
        {
            GameObject add = Instantiate(Goods[0], transform);
            RunnerScript script = add.GetComponent<RunnerScript>();
            script.ShowEnd += OnPlayEnd;
            all.Add(script);
            runner1.Enqueue(script);
        }
    }

    void OnDestroy()
    {
        foreach (RunnerScript script in all)
        {
            script.ShowEnd -= OnPlayEnd;
            Destroy(script.gameObject);
        }
    }

    public override void OnPlay(bool good)
    {
        if (!good)
        {
            if (runner0.Count > 0)
            {
                RunnerScript runner = runner0.Dequeue();
                //runner.transform.SetAsFirstSibling();
                runner.transform.localPosition = new Vector3(0, 0, z_value);
                z_value -= 0.0001f;
                runner.Play();
            }
        }
        else
        {
            if (runner1.Count > 0)
            {
                RunnerScript runner = runner1.Dequeue();
                //runner.transform.SetAsFirstSibling();
                runner.transform.localPosition = new Vector3(0, 0, z_value);
                z_value -= 0.0001f;
                runner.Play();
            }
        }
    }
    //private float last_time;

    private void OnPlayEnd(RunnerScript runner)
    {
        switch (runner.Type)
        {
            case 0:
                runner0.Enqueue(runner);
                break;
            case 1:
                runner1.Enqueue(runner);
                break;
        }
    }
}
