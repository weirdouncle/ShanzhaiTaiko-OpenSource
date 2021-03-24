using UnityEngine;

public class RunnerScript : MonoBehaviour
{
    public int Type;
    public Animator Animator;
    public SpriteRenderer[] ShownImages;

    public event ShowEndDelegate ShowEnd;
    public delegate void ShowEndDelegate(RunnerScript runner);

    private int shown_layer = 5;
    private int hidden_layer = 12;
    public void End()
    {
        Animator.enabled = false;
        Animator.Play(Animator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 0);
        Animator.Update(0);
        foreach (SpriteRenderer sprite in ShownImages)
            sprite.gameObject.layer = hidden_layer;

        ShowEnd?.Invoke(this);
        transform.localPosition = new Vector3(0, 0);
    }

    public void Hide()
    {
        Animator.enabled = false;
        Animator.Play(Animator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 0);
        Animator.Update(0);
        foreach (SpriteRenderer sprite in ShownImages)
            sprite.gameObject.layer = hidden_layer;
        transform.localPosition = new Vector3(0, 0);
    }

    public void Play()
    {
        Animator.enabled = true;
        foreach (SpriteRenderer sprite in ShownImages)
            sprite.gameObject.layer = shown_layer;
    }
}
