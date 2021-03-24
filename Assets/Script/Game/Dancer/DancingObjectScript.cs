using UnityEngine;

public class DancingObjectScript : MonoBehaviour
{
    public Animator Animator;
    public SpriteRenderer[] Images;

    private int shown_layer = 0;
    private int hidden_layer = 12;

    public void Act(bool act)
    {
        Animator.enabled = act;
        if (!act)
        {
            Animator.speed = 1;
            Animator.Play(Animator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 0);
            Animator.Update(0);
            Show(false);
        }
    }
    public void Show(bool show)
    {
        foreach (SpriteRenderer sprite in Images)
            sprite.gameObject.layer = show ? shown_layer : hidden_layer;
    }

    public void SetBpm(double bpm)
    {
        Animator.speed = (float)bpm / 60;
    }
}
