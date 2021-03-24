using UnityEngine;

public class MaxScript : MonoBehaviour
{
    public Animator Animator;
    public GameObject[] Showings;

    public void SetBPM(double bpm)
    {
        Animator.speed = (float)bpm / 60;
    }

    public virtual void SetMax(bool max)
    {
        if (max)
        {
            Animator.SetTrigger("Reset");
        }

        foreach (GameObject show in Showings)
            show.SetActive(max);
    }
}
