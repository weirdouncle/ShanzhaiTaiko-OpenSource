using CommonClass;
using System.Collections;
using UnityEngine;

public delegate void BalloonSucceedDelegate(BalloonResult result);
public class HammerShowScript : KusudamaShowScript
{
    public new static event BalloonSucceedDelegate BalloonSucceed;

    public Animator Hammer;
    public GameObject Gold;
    public GameObject Silver;
    public GameObject Fail;

    private bool gold = true;
    public override void Play(int count)
    {
        gold = true;
        StopAllCoroutines();
        Number.Count = count;
        Ball.SetActive(false);
        Fail.SetActive(false);
        points_5k.SetActive(false);
        points_1k.SetActive(false);
        gameObject.SetActive(true);
        Ball.SetActive(true);
        BGEffect.SetActive(true);
        DonAnimator.OnStart();
    }
    void OnDisable()
    {
        if (DonAnimator != null) DonAnimator.OnPlayEnd();
    }

    public override void Hit(bool player2 = false)
    {
        if (Number.Count > 0)
        {
            Number.Count--;
            Hammer.SetTrigger("Hit");

            if (!player2)
                DonAnimator.Hit();
            else
                DonAnimator.Hit2P();

            if (Number.Count == 0)
                BalloonSucceed?.Invoke(gold ? BalloonResult.Perfect : BalloonResult.Good);
        }
    }

    public override void Hide()
    {
        StopAllCoroutines();
        Number.Count = 0;
        points_5k.SetActive(false);
        points_1k.SetActive(false);
        Ball.SetActive(false);
        Fail.SetActive(true);
        //Avatar.SetTrigger("Fail");
        BGEffect.SetActive(false);
        DonAnimator.Fail();
        if (gameObject.activeSelf)
        {
            StartCoroutine(DelayHide());
            Source.cueName = "hammerfail";
            Source.Play();
        }
    }

    IEnumerator DelayHide()
    {
        yield return new WaitForSeconds(0.5f);
        DonAnimator.OnPlayEnd();
        gameObject.SetActive(false);
    }

    public override void ShowResult(bool good)
    {
        Number.Count = 0;
        Ball.SetActive(false);
        BGEffect.SetActive(false);
        //Avatar.SetTrigger("Quit");
        DonAnimator.Success();
        if (good)
        {
            points_5k.SetActive(true);
            StartCoroutine(Delay(points_5k));
            Source.cueName = "hammer_gold";
            Source.Play();
        }
        else
        {
            points_1k.SetActive(true);
            StartCoroutine(Delay(points_1k));
            Source.cueName = "hammer_silver";
            Source.Play();
        }
    }

    IEnumerator Delay(GameObject game)
    {
        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);
        DonAnimator.OnPlayEnd();
    }

    public void ChangeSilver()
    {
        Silver.SetActive(true);
        Gold.SetActive(false);
        gold = false;
    }
}
