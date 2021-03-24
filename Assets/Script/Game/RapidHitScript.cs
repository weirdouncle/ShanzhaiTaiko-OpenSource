using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RapidHitScript : MonoBehaviour
{
    public Animator Animator;
    public Animator NumberAnimator;
    public SpriteNumberScript Number;
    public Transform FireWorkParent;
    public GameObject FireWork_pre;
    public SpriteRenderer Char;
    public int Pool = 30;

    public bool Activated { private set; get; }

    private List<FireWorkScript> all = new List<FireWorkScript>();
    private Queue<FireWorkScript> fires = new Queue<FireWorkScript>();
    private float index = 0;
    void Start()
    {
        Char.sprite = SettingLoader.Roll;
#if !UNITY_ANDROID
        StartCoroutine(Instantiate());
#else
        gameObject.SetActive(false);
#endif
    }

    IEnumerator Instantiate()
    {
        int count = 0;
        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        yield return wait;

        for (int i = 0; i < Pool; i++)
        {
            if (count >= 20)
            {
                count = 0;
            }
            GameObject fire = Instantiate(FireWork_pre, FireWorkParent);
            FireWorkScript script = fire.GetComponent<FireWorkScript>();
            script.ShowEnd += OnPlayEnd;
            all.Add(script);
            fires.Enqueue(script);
            count++;
        }

        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        foreach (FireWorkScript script in all)
            script.ShowEnd -= OnPlayEnd;
    }

    private void OnPlayEnd(FireWorkScript fire)
    {
        fires.Enqueue(fire);
    }

    public void Show()
    {
        Activated = true;
        Number.Count = 0;

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        else
        {
            Animator.SetTrigger("Show");
        }
    }

    public void Close(bool immediately)
    {
        Activated = false;
        if (!immediately)
        {
            Animator.SetTrigger("Hide");
        }
        else
        {
            Number.Count = 0;
            gameObject.SetActive(false);
        }
    }

    public void ResetRapid()
    {
        Number.Count = 0;
        gameObject.SetActive(false);
    }

    public void Hit()
    {
        Number.Count++;
        if (NumberAnimator != null)
            NumberAnimator.SetTrigger("add");
        if (fires.Count > 0)
        {
            FireWorkScript fire = fires.Dequeue();
            fire.transform.localPosition = new Vector3(0, 0, index);
            fire.OnPlay();
            index -= 0.0001f;
        }
    }
}
