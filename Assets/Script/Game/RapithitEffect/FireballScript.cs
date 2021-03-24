using UnityEngine;

public class FireballScript : MonoBehaviour
{
    public event ShowEndDelegate ShowEnd;
    public delegate void ShowEndDelegate(FireballScript runner);

    public virtual void End()
    {
        ShowEnd?.Invoke(this);
        gameObject.SetActive(false);
    }

    public virtual void Play()
    {
        gameObject.SetActive(true);
    }
}
