using UnityEngine;

public delegate void BalloonBroken2PDelegate(bool balloon);
public delegate void BalloonBrokenDelegate(bool balloon);
public class BreakingBalloon : MonoBehaviour
{
    public static event BalloonBrokenDelegate BalloonBroken;
    public static event BalloonBroken2PDelegate BalloonBroken2P;

    public Animator Animator;
    public Animator Shake;
    public Animator NumberAnimator;
    public SpriteNumberScript Number;
    public CriAtomSource Audio;
    public UI3DdonControll Controll;
    public bool Player2;

    protected int balloon_index = -1;

    public enum Phase
    {
        Start,
        More,
        Nearly,
        Almost
    }

    void OnEnable()
    {
        Controll.ShowBalloon(true);
    }

    void OnDisable()
    {
        Controll.ShowBalloon(false);
    }

    public void ReStart(BallonScript balloon)
    {
        Number.Count = balloon.HitCount;
        balloon_index = balloon.Index;
        if (gameObject.activeSelf)
        {
            StopAllCoroutines();
            gameObject.SetActive(false);
        }

        gameObject.SetActive(true);
    }

    public void SetBpm(float bpm)
    {
        Animator.speed = (float)bpm / 60;
    }

    public void Broken()
    {
        balloon_index = -1;
        Controll.Broken();
        Animator.SetTrigger("Broken");
        Audio.cueName = "balloon";
        Audio.Play();
    }

    public void SetPhase(Phase phase, BallonScript balloon)
    {
        string p = phase.ToString();
        if (balloon_index == balloon.Index && !Animator.GetCurrentAnimatorStateInfo(0).IsName(p))
        {
            Animator.SetTrigger(phase.ToString());
        }
    }

    public void Hit(BallonScript balloon)
    {
        Number.Count--;
        if (Number.Count > 0)
        {
            Controll.Hit();
            Shake.SetTrigger("Shake");
        }
        if (NumberAnimator != null) NumberAnimator.SetTrigger("add");
    }

    public void Fail(BallonScript balloon)
    {
        if (balloon_index == balloon.Index && Animator.gameObject.activeSelf)
        {
            balloon_index = -1;
            Animator.SetTrigger("Fail");
            Controll.BalloonFail();
        }
    }

    public void End()
    {
        if (Player2)
            BalloonBroken2P?.Invoke(true);
        else
            BalloonBroken?.Invoke(true);

        gameObject.SetActive(false);
    }
}
