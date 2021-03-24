using UnityEngine;

public class UI3DdonControll : MonoBehaviour
{
    public Animator ResultPostion;
    public DonScript Don;

    protected WaitForEndOfFrame wait = new WaitForEndOfFrame();
    void Start()
    {
        Don.RandomPlay += RandomPlay;
    }

    void OnDestroy()
    {
        Don.RandomPlay -= RandomPlay;
    }

    #region Balloon
    public virtual void ShowBalloon(bool show)
    {
        //if (Don != null) Don.gameObject.SetActive(show);
        //if (show) Animator.SetTrigger("Beat");
        if (show) Don.ShowBalloon(show);
    }
    public virtual void ReStart()
    {
        if (gameObject.activeSelf)
            gameObject.SetActive(false);

        gameObject.SetActive(true);
    }
    public void Hit()
    {
        Don.Animator.SetTrigger("Beat");
    }

    public void BalloonFail()
    {
        Don.BalloonFail();
    }
    public void Broken()
    {
        Don.BalloonBroken();
    }
    #endregion

    #region Game Result
    public void ResultStart()
    {
        if (ResultPostion != null)
            ResultPostion.SetTrigger("Move");
        if (Don.Sub != null)
        {
            foreach (SpriteRenderer sprite in Don.Sub.GetComponentsInChildren<SpriteRenderer>(false))
            {
                sprite.sortingLayerName = "AboveUI";
                sprite.sortingOrder = 1;
            }
        }
    }

    private void RandomPlay()
    {
        int index = Random.Range(0, 10);         //随机播放舞蹈动画
        if (index == 0)
            Don.Animator.SetTrigger("Dance");
    }

    public void Clear()
    {
        Don.Animator.SetTrigger("Clear");
    }

    public void Full()
    {
        if (ResultPostion != null)
            Don.Animator.SetTrigger("Full");
        else
            Don.Animator.SetTrigger("Clear");
    }
    public void Percious()
    {
        Don.Animator.SetTrigger("Percious");
    }
    public void ResultFail()
    {
        Don.Animator.SetTrigger("Fail");
    }
    #endregion
}
