using UnityEngine;

public delegate void ShowJumpEndDelegate(JumpOutScript jump);
public class JumpOutScript : MonoBehaviour
{
    public event ShowJumpEndDelegate ShowEnd;

    public GameObject[] Objects;

    public Animator Animator;
    public int Type;
    public bool Player2;

    private int shown_layer = 5;
    private int hide_layer = 12;
    private KisekiPlayScript kiseki;
    public void Show(float z_value)
    {
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -z_value);
        foreach (GameObject game in Objects)
            game.layer = shown_layer;
        Animator.enabled = true;
    }

    public void InitInstance()
    {
        if (Player2)
            kiseki = KisikiScript.Instance2.GetInstance();
        else
            kiseki = KisikiScript.Instance1.GetInstance();
    }

    public void ShowKisekiEffect(int index)
    {
        kiseki.ShowKisekiEffect(index);
    }

    public void End()
    {
        kiseki = null;
        Animator.enabled = false;

        Animator.Play(Animator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 0);
        Animator.Update(0);

        foreach (GameObject game in Objects)
            game.layer = hide_layer;

        ShowEnd?.Invoke(this);
    }
}
