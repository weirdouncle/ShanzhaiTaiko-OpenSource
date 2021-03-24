using UnityEngine;

public class HitBgPoolScript : MonoBehaviour
{
    public Animator Don;
    public Animator Ka;
    public bool Player2;

    public static HitBgPoolScript Instance { set; get; }
    public static HitBgPoolScript Instance2P { set; get; }
    void Start()
    {
        if (Player2)
            Instance2P = this;
        else
            Instance = this;
    }

    public void Play(bool don)
    {
        if (don)
        {
            Don.SetTrigger("Hit");
            Don.transform.SetAsLastSibling();
        }
        else
        {
            Ka.SetTrigger("Hit");
            Ka.transform.SetAsLastSibling();
        }
    }
}
