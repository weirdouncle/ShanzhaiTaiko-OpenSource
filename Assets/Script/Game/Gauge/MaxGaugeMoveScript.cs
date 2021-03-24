using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaxGaugeMoveScript : MonoBehaviour
{
    public Transform MovePanel;
    public Transform[] Images;

    protected Queue<Transform> queue = new Queue<Transform>();
    private WaitForSeconds wait = new WaitForSeconds(0.01f);
    void Start()
    {
        foreach (Transform image in Images)
            queue.Enqueue(image);
    }

    private void OnEnable()
    {
        StartCoroutine(Move());
    }

    private void OnDisable()
    {
        MovePanel.localPosition = new Vector3(0, 1.20f, -0.0002f);
        Images[0].localPosition = new Vector3(-16, 0);
        Images[1].localPosition = new Vector3(-32, 0);

        queue.Clear();
        foreach (Transform image in Images)
            queue.Enqueue(image);
    }


    IEnumerator Move()
    {
        int count = 0;
        bool first_time = true;
        while (true)
        {
            MovePanel.localPosition += new Vector3(0.16f, 0);
            yield return wait;
            count++;
            if (count >= (first_time ? 200 : 100))
            {
                if (first_time) first_time = false;

                count = 0;
                Transform first = queue.Dequeue();
                Transform[] rest = new Transform[1];
                queue.CopyTo(rest, 0);
                queue.Enqueue(first);
                first.localPosition = new Vector3(rest[0].localPosition.x - 16, rest[0].localPosition.y);
                first.SetAsLastSibling();
            }
        }
    }
}
