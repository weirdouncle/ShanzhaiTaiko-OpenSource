using UnityEngine;

public delegate void ApearFinishDelegate(int index);
public class DancerControllScript : MonoBehaviour
{
    public static event ApearFinishDelegate ApearFinish;

    public SpriteRenderer[] Images;
    public Animator Animator;
    public int Index;

    private int shown_layer = 0;
    private int hidden_layer = 12;
    public void Show(bool inter)
    {
        Animator.enabled = true;
        foreach (SpriteRenderer sprite in Images)
            sprite.gameObject.layer = shown_layer;
        Animator.SetTrigger(inter ? "Apear" : "Disapear");
    }

    public void OnPlayEnd(int apear)
    {
        Animator.enabled = false;
        foreach (SpriteRenderer sprite in Images)
            sprite.gameObject.layer = hidden_layer;

        if (apear == 0) ApearFinish?.Invoke(Index);
    }
}
