using UnityEngine;

public class NijiiroRankAnimatorScript : MonoBehaviour
{
    public Animator Animator;
    public void StartScan()
    {
        if (Animator != null) Animator.enabled = true;
    }
}
