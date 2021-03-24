using UnityEngine;

public class KisikiEffectScript : MonoBehaviour
{
    public Animator Animator;
    private int shown_layer = 5;
    private int hide_layer = 12;

    public void Play()
    {
        gameObject.layer = shown_layer;
        Animator.enabled = true;
        Animator.SetTrigger("Reset");
    }

    public void OnPlayEnd()
    {
        gameObject.layer = hide_layer;
        Animator.enabled = false;
    }
}
