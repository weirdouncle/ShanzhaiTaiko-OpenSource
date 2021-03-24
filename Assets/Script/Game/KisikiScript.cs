using System.Collections.Generic;
using UnityEngine;

public class KisikiScript : MonoBehaviour
{
    public Transform Parent;
    public GameObject Kiseki_pre;
    public int Pool = 5;
    public bool Player2;

    public static KisikiScript Instance1;
    public static KisikiScript Instance2;

    private Queue<KisekiPlayScript> queue = new Queue<KisekiPlayScript>();
    void Start()
    {
        for (int i = 0; i < Pool; i++)
        {
            GameObject game = Instantiate(Kiseki_pre, Parent);
            KisekiPlayScript script = game.GetComponent<KisekiPlayScript>();
            queue.Enqueue(script);
        }
        if (Player2)
        {
            Instance2 = this;
        }
        else
        {
            Instance1 = this;
        }
    }

    void OnDestroy()
    {
        if (Player2)
        {
            Instance2 = null;
        }
        else
        {
            Instance1 = null;
        }
    }

    public KisekiPlayScript GetInstance()
    {
        KisekiPlayScript first = queue.Dequeue();
        queue.Enqueue(first);
        return first;
    }
}
