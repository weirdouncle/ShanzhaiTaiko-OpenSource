using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingBgScript : MonoBehaviour
{
    public Transform[] Images;
    public Transform MovePanel;

    protected Queue<Transform> queue = new Queue<Transform>();
    void Start()
    {
        foreach (Transform image in Images)
            queue.Enqueue(image);

        StartCoroutine(Move());
    }

    IEnumerator Move()
    {
        int count = 0;
        while (true)
        {
            MovePanel.localPosition += new Vector3(0, 0.01f);
            yield return new WaitForSeconds(1f / 80);
            count++;
            if (count >= 1792)
            {
                count = 0;
                Transform first = queue.Dequeue();
                Transform[] rest = new Transform[1];
                queue.CopyTo(rest, 0);
                queue.Enqueue(first);
                first.localPosition = new Vector3(0, rest[0].localPosition.y - 17.92f);
                first.SetAsLastSibling();
            }
        }
    }
}
