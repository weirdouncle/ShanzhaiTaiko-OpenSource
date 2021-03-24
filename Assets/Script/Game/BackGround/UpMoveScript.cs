using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpMoveScript : MonoBehaviour
{
    public Sprite[] Sprites;
    public SpriteRenderer[] Images;
    public Transform MovePanel;
    public float Width;

    protected Queue<Transform> queue = new Queue<Transform>();
    private WaitForSeconds wait = new WaitForSeconds(1f / 80);
    void Start()
    {
        foreach (SpriteRenderer image in Images)
            queue.Enqueue(image.transform);

        StartCoroutine(Move());
    }

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
                Transform[] rest = new Transform[Images.Length - 1];
                queue.CopyTo(rest, 0);
                queue.Enqueue(first);
                first.localPosition = new Vector3(rest[rest.Length - 1].localPosition.x + Width / 100, 0);
                first.SetAsLastSibling();
            }
        }
    }

    public virtual void Clear(bool clear)
    {
        foreach (SpriteRenderer image in Images)
            image.sprite = clear ? Sprites[1] : Sprites[0];
    }
}
