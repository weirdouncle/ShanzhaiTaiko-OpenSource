using UnityEngine;

public class AnimatorFinishScript : MonoBehaviour
{
    public event FinishDelegate Finish;
    public delegate void FinishDelegate(int type);

    public void OnFinishe1()
    {
        Finish?.Invoke(0);
    }
    public void OnFinishe2()
    {
        Finish?.Invoke(1);
    }
}
