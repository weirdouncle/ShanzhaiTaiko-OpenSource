using System.Collections;
using UnityEngine;

public class KusudamaShowScript : MonoBehaviour
{
    public static event BalloonSucceedDelegate BalloonSucceed;

    public GameObject Ball;
    public GameObject points_1k;
    public GameObject points_5k;
    public SpriteNumberScript Number;
    public SpriteRenderer Mask;
    public GameObject BGEffect;

    public KusudamaDonControll DonAnimator;
    public CriAtomSource Source;

    private float count = 0;
    void OnDisable()
    {
        if (DonAnimator != null) DonAnimator.OnPlayEnd();
    }
    public virtual void Play(int count)
    {
        StopAllCoroutines();
        this.count = count;
        Number.Count = count;
        Ball.SetActive(false);
        points_5k.SetActive(false);
        points_1k.SetActive(false);
        gameObject.SetActive(true);
        BGEffect.SetActive(true);
        Ball.SetActive(true);
        DonAnimator.OnStart();
        Mask.color = new Color(1, 1, 1, 0);
    }

    public virtual void Hit(bool player2 = false)
    {
        if (Number.Count > 0)
        {
            Number.Count--;
            if (!player2)
                DonAnimator.Hit();
            else
                DonAnimator.Hit2P();

            Updatephase();
            if (Number.Count == 0)
                BalloonSucceed?.Invoke(CommonClass.BalloonResult.Perfect);
        }
    }

    public virtual void Hide()
    {
        StopAllCoroutines();
        Number.Count = 0;
        points_5k.SetActive(false);
        points_1k.SetActive(false);
        Ball.SetActive(false);
        BGEffect.SetActive(false);
        DonAnimator.Fail();
        if (gameObject.activeSelf)
            StartCoroutine(DelayHide());
    }
    public virtual void ShowResult(bool good)
    {
        Number.Count = 0;
        Ball.SetActive(false);
        BGEffect.SetActive(false);
        /*
        if (good)
        {
            points_5k.SetActive(true);
            StartCoroutine(Delay(points_5k));
        }
        else
        {
            points_1k.SetActive(true);
            StartCoroutine(Delay(points_1k));
        }
        */

        DonAnimator.Success();

        Source.cueName = "balloon";
        Source.Play();
        points_5k.SetActive(true);

        StartCoroutine(Delay(points_5k));
    }
    IEnumerator DelayHide()
    {
        yield return new WaitForSeconds(0.5f);
        DonAnimator.OnPlayEnd();
        gameObject.SetActive(false);
    }

    IEnumerator Delay(GameObject game)
    {
        yield return new WaitForSeconds(1.5f);
        gameObject.SetActive(false);
        DonAnimator.OnPlayEnd();
    }

    public virtual void Updatephase()
    {
        float alpha = (count - Number.Count) / count / 2;
        Mask.color = new Color(1, 1, 1, alpha);
    }
}
