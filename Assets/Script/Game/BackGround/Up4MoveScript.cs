using UnityEngine;

public class Up4MoveScript : UpMoveScript
{
    public SpriteRenderer[] Images2;
    void Start()
    {
    }
    /*
    protected Queue<Transform> queue2 = new Queue<Transform>();
    private WaitForSeconds wait = new WaitForSeconds(1f / 80);
    IEnumerator Move()
    {
        int count = 0;
        while (true)
        {
            MovePanel.localPosition -= new Vector3(0.01f, 0);
            yield return wait;
            count++;
            if (count >= Width)
            {
                count = 0;
                Transform first = queue.Dequeue();
                Transform[] rest = new Transform[4];
                queue.CopyTo(rest, 0);
                queue.Enqueue(first);
                first.localPosition = new Vector3(rest[3].localPosition.x + Width / 100, 0);
                first.SetAsLastSibling();
            }
        }
    }

    IEnumerator Move2()
    {
        int count = 0;
        while (true)
        {
            MovePanel2.localPosition -= new Vector3(0.01f, 0);
            yield return wait;
            count++;
            if (count >= 493)
            {
                count = 0;
                Transform first = queue2.Dequeue();
                Transform[] rest = new Transform[4];
                queue2.CopyTo(rest, 0);
                queue2.Enqueue(first);
                first.localPosition = new Vector3(rest[3].localPosition.x + 4.93f, 0);
                first.SetAsLastSibling();
            }
        }
    }
    */
    public override void Clear(bool clear)
    {
        foreach (SpriteRenderer image in Images)
            image.sprite = clear ? Sprites[1] : Sprites[0];

        foreach (SpriteRenderer image in Images2)
            image.sprite = clear ? Sprites[3] : Sprites[2];
    }
}
