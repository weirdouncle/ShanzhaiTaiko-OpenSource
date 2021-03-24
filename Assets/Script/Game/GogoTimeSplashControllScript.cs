using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GogoTimeSplashControllScript : MonoBehaviour
{
    public Animator[] Games;
    public GameObject[] fires1;
    public GameObject[] fires2;

    private WaitForSeconds wait1 = new WaitForSeconds(0.15f);
    private WaitForSeconds wait2 = new WaitForSeconds(0.5f);

    private int shown_layer = 5;
    private int hide_layer = 12;
    public void ShowSplash()
    {
        StartCoroutine(Play());
    }

    IEnumerator Play()
    {
        for (int i = 0; i < Games.Length; i++)
        {
            if (i == 0)
            {
                foreach (GameObject game in fires1)
                    game.layer = shown_layer;
            }
            else
            {
                foreach (GameObject game in fires2)
                    game.layer = shown_layer;
            }
            Games[i].enabled = true;
            Games[i].SetTrigger("Show");
            StartCoroutine("Hide", i);
            yield return wait1;
        }
    }

    IEnumerator Hide(int index)
    {
        yield return wait2;
        if (index == 0)
        {
            foreach (GameObject game in fires1)
                game.layer = hide_layer;
        }
        else
        {
            foreach (GameObject game in fires2)
                game.layer = hide_layer;
        }
        Games[index].enabled = false;
    }
}
