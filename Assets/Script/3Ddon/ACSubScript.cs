using UnityEngine;

public class ACSubScript : MonoBehaviour
{
    public SpriteRenderer Renderer;
    public Sprite[] Sprites;
    protected bool tick;
    public virtual void OnTick()
    {
        if (Sprites.Length == 2)
        {
            Renderer.sprite = tick ? Sprites[0] : Sprites[1];
            tick = !tick;
        }
    }
}
