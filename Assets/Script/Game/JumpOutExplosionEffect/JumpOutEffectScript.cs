using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpOutEffectScript : MonoBehaviour
{
    public GameObject JumpOutDon_pre;
    public GameObject JumpOutKa_pre;
    public GameObject JumpOutBigDon_pre;
    public GameObject JumpOutBigKa_pre;
    public GameObject JumpOutRainbow_pre;
    public int Pool = 30;
    public bool Player2;
    public Transform RainbowParent;

    private Queue<JumpOutScript> don = new Queue<JumpOutScript>();
    private Queue<JumpOutScript> ka = new Queue<JumpOutScript>();
    private Queue<JumpOutScript> big_don = new Queue<JumpOutScript>();
    private Queue<JumpOutScript> big_ka = new Queue<JumpOutScript>();
    private Queue<JumpOutScript> rainbow = new Queue<JumpOutScript>();
    private List<JumpOutScript> all = new List<JumpOutScript>();
    private float z_value = 0;
    private float z_value_rainbow = 0;
    private WaitForEndOfFrame wait = new WaitForEndOfFrame();

    void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            GameObject game = Instantiate(JumpOutRainbow_pre, RainbowParent);
            JumpOutScript script = game.GetComponent<JumpOutScript>();
            script.ShowEnd += OnPlayEnd;
            rainbow.Enqueue(script);
            all.Add(script);
        }

        StartCoroutine(Instantiate());
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
                yield return wait;
            }
            GameObject game = Instantiate(JumpOutDon_pre, transform);
            JumpOutScript script = game.GetComponent<JumpOutScript>();
            script.ShowEnd += OnPlayEnd;
            don.Enqueue(script);
            all.Add(script);

            game = Instantiate(JumpOutKa_pre, transform);
            script = game.GetComponent<JumpOutScript>();
            script.ShowEnd += OnPlayEnd;
            ka.Enqueue(script);
            all.Add(script);

            game = Instantiate(JumpOutBigDon_pre, transform);
            script = game.GetComponent<JumpOutScript>();
            script.ShowEnd += OnPlayEnd;
            big_don.Enqueue(script);
            all.Add(script);

            game = Instantiate(JumpOutBigKa_pre, transform);
            script = game.GetComponent<JumpOutScript>();
            script.ShowEnd += OnPlayEnd;
            big_ka.Enqueue(script);
            all.Add(script);
            count++;
        }
    }

    void OnDestroy()
    {
        foreach (JumpOutScript jump in all)
        {
            jump.ShowEnd -= OnPlayEnd;
            Destroy(jump.gameObject);
        }
    }

    public void Play(NoteSoundScript note, bool don)
    {
        StartCoroutine(PlayDelay(note, don));
    }

    IEnumerator PlayDelay(NoteSoundScript note, bool don)
    {
        yield return wait;
        switch (note.Type)
        {
            case 1:
                if (this.don.Count > 0)
                {
                    JumpOutScript jump = this.don.Dequeue();
                    jump.Show(z_value);
                }
                break;
            case 2:
                if (ka.Count > 0)
                {
                    JumpOutScript jump = ka.Dequeue();
                    jump.Show(z_value);
                }
                break;
            case 3:
                if (big_don.Count > 0)
                {
                    JumpOutScript jump = big_don.Dequeue();
                    jump.Show(z_value);
                }
                break;
            case 4:
                if (big_ka.Count > 0)
                {
                    JumpOutScript jump = big_ka.Dequeue();
                    jump.Show(z_value);
                }
                break;
            case 5:
                if (don && this.don.Count > 0)
                {
                    JumpOutScript jump = this.don.Dequeue();
                    jump.Show(z_value);
                }
                else if (!don && ka.Count > 0)
                {
                    JumpOutScript jump = ka.Dequeue();
                    jump.Show(z_value);
                }
                break;
            case 6:
                if (don && big_don.Count > 0)
                {
                    JumpOutScript jump = big_don.Dequeue();
                    jump.Show(z_value);
                }
                else if (!don && big_ka.Count > 0)
                {
                    JumpOutScript jump = big_ka.Dequeue();
                    jump.Show(z_value);
                }
                break;
            case 7:
                if (big_don.Count > 0)
                {
                    JumpOutScript jump = big_don.Dequeue();
                    jump.Show(z_value);
                }

                if (rainbow.Count > 0)
                {
                    JumpOutScript jump = rainbow.Dequeue();
                    jump.Show(z_value_rainbow);

                    if (Player2)
                        z_value_rainbow -= 0.0001f;
                    else
                        z_value_rainbow += 0.0001f;
                }
                break;
        }

        if (Player2)
            z_value -= 0.0001f;
        else
            z_value += 0.0001f;
    }

    public void OnPlayEnd(JumpOutScript exp)
    {
        switch (exp.Type)
        {
            case 0:
                don.Enqueue(exp);
                break;
            case 1:
                ka.Enqueue(exp);
                break;
            case 2:
                big_don.Enqueue(exp);
                break;
            case 3:
                big_ka.Enqueue(exp);
                break;
            case 4:
                rainbow.Enqueue(exp);
                break;
        }
    }
}
