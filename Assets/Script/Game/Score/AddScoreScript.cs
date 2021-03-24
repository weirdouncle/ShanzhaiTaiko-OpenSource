using UnityEngine;

public class AddScoreScript : SpriteNumberScript
{
    public event AddScoreEndDelegate AddScoreEnd;
    public delegate void AddScoreEndDelegate(AddScoreScript number, int score);

    //public event AddScoreEnd2Delegate AddScoreEnd2;
    //public delegate void AddScoreEnd2Delegate(AddScoreScript number, int score);

    public Animator Animator;
    public bool Bonus;

    public void AddNumber(int number)
    {
        Count = number;
        Animator.enabled = true;
    }

    public virtual void End()
    {
        Animator.enabled = false;
        foreach (SpriteRenderer image in Images)
            image.gameObject.SetActive(false);

        //if (Player2)
         //   AddScoreEnd2?.Invoke(this, Count);
        //else
            AddScoreEnd?.Invoke(this, Count);

        Count = count = 0;
    }
}
