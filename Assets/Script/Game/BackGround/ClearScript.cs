using System.Collections.Generic;
using UnityEngine;

public class ClearScript : MonoBehaviour
{
    public Transform MovePanel;
    public Transform ClearBg;
    public Transform[] Images;
    public Animator ClearAni;
    public Animator Bg2Move;
    public Animator[] UVAnimations;

    protected Queue<Transform> queue = new Queue<Transform>();
    void Start()
    {
    }

    public virtual void SetClear(bool clear)
    {
        if (clear)
        {
            ClearAni.SetTrigger("Play");
        }
        else
        {
            foreach (Animator animation in UVAnimations)
                animation.enabled = false;
            Bg2Move.enabled = false;

            ClearAni.SetTrigger("Quit");
        }
    }

    public virtual void OnDisapearFinish()
    {
    }

    public virtual void OnApearFinish()
    {
        //ClearAni.enabled = false;
        Bg2Move.enabled = true;
        foreach (Animator animation in UVAnimations)
            animation.enabled = true;
    }

    /*
    private WaitForSeconds wait = new WaitForSeconds(0.01f);

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
    */
}
