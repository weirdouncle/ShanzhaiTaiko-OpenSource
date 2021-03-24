using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearScript : MonoBehaviour
{
    public Transform MovePanel;
    public Transform ClearBg;
    public Transform[] Images;
    public Animator ClearAni;
    public Animator Bg2Move;

    protected Queue<Transform> queue = new Queue<Transform>();
    private WaitForSeconds wait = new WaitForSeconds(0.01f);
    void Start()
    {
        foreach (Transform image in Images)
        {
            queue.Enqueue(image);
        }
        gameObject.SetActive(false);
    }

    public virtual void SetClear(bool clear)
    {
        if (clear)
        {
            if (ClearBg != null)
            {
                ClearBg.localPosition = new Vector3(0, 4.82f);
                Bg2Move.transform.localPosition = new Vector3(0, 0);
                MovePanel.localPosition = new Vector3(0, 0);
                for (int i = 0; i < Images.Length; i++)
                    Images[i].localPosition = new Vector3(i * 19.2f, 0);
            }

            gameObject.SetActive(true);
            ClearAni.SetTrigger("Play");
        }
        else
        {
            if (ClearBg != null)
            {
                StopAllCoroutines();
                Bg2Move.enabled = false;
                queue.Clear();
                foreach (Transform image in Images)
                    queue.Enqueue(image);
            }

            if (gameObject.activeSelf)
                ClearAni.SetTrigger("Quit");
        }
    }

    public virtual void OnDisapearFinish()
    {
        gameObject.SetActive(false);
    }

    public virtual void OnApearFinish()
    {
        //ClearAni.enabled = false;
        Bg2Move.enabled = true;
        if (MovePanel != null)
            StartCoroutine(Move());
    }

    IEnumerator Move()
    {
        int count = 0;
        int round = 0;
        while (true)
        {
            MovePanel.localPosition -= new Vector3(0.04f, 0);
            yield return wait;
            count++;
            if (count >= 480)
            {
                count = 0;
                round++;
                Transform first = queue.Dequeue();
                Transform[] rest = new Transform[2];
                queue.CopyTo(rest, 0);
                queue.Enqueue(first);
                first.localPosition = new Vector3(rest[1].localPosition.x + 19.2f, 0);
                first.SetAsLastSibling();
            }
        }
    }
}
