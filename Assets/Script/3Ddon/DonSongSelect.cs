using UnityEngine;

public class DonSongSelect : MonoBehaviour
{
    public Animator Animator;

    public void Selected()
    {
        Animator.SetBool("Selected", true);
    }

    public void UnSelect()
    {
        Animator.SetBool("Selected", false);
    }

    public void Confirm()
    {
        Animator.SetTrigger("Confirm");
    }

    public void StartPlay()
    {
        Animator.SetTrigger("Start");
    }
}
