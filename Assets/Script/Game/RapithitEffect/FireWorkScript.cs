using UnityEngine;

public class FireWorkScript : MonoBehaviour
{
    public Animator Animator;
    public event ShowEndDelegate ShowEnd;
    public delegate void ShowEndDelegate(FireWorkScript fire);

    private int shown_layer = 5;
    private int hide_layer = 12;

    public void OnPlay()
    {
        transform.localPosition += new Vector3(Random.Range(-5, 6), Random.Range(-5, 6));
        float z_r = Random.Range(-45f, 46f);
        transform.localRotation = Quaternion.AngleAxis(z_r, Vector3.forward);

        gameObject.layer = shown_layer;
        Animator.enabled = true;
        Animator.SetTrigger("Show");
    }

    public void End()
    {
        ShowEnd?.Invoke(this);
        transform.localPosition = new Vector3(0, 0);
        transform.localRotation = Quaternion.AngleAxis(0, Vector3.forward);
        Animator.enabled = false;
        gameObject.layer = hide_layer;
    }
}
