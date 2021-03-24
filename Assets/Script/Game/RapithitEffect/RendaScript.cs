using CommonClass;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RendaScript : MonoBehaviour
{
    public int Pools;
    public GameObject[] Fireball_pres;
    public Vector3[] Positions;
    
    protected List<FireballScript> all = new List<FireballScript>();
    protected Queue<FireballScript> queue = new Queue<FireballScript>();
    private List<int> all_index = new List<int>();
    void Start()
    {
        for (int index = 0; index < Fireball_pres.Length; index++)
        {
            for (int i = 0; i < Pools; i++)
            {
                GameObject add = Instantiate(Fireball_pres[index], transform);
                FireballScript script = add.GetComponent<FireballScript>();
                script.ShowEnd += OnPlayEnd;
                all.Add(script);
            }
        }

        Shuffle.shuffle(ref all);
        int x = 0;
        foreach (FireballScript runner in all)
        {
            runner.transform.localPosition = Positions[x];
            x++;
            if (x >= Positions.Length) x = 0;
            queue.Enqueue(runner);
        }
    }
    

    void OnDestroy()
    {
        foreach (FireballScript script in all)
        {
            script.ShowEnd -= OnPlayEnd;
            Destroy(script.gameObject);
        }
    }

    public virtual void OnPlay()
    {
        if (queue.Count > 0)
        {
            FireballScript runner = queue.Dequeue();
            runner.Play();
        }
    }

    private void OnPlayEnd(FireballScript runner)
    {
        queue.Enqueue(runner);
    }
}
