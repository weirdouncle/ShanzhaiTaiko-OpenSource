using System.Collections.Generic;
using UnityEngine;

public class DancerInitScript : MonoBehaviour
{
    public GameObject[] Dancers_pre;
    public bool Nijiiro { set; get; }

    private DancerScript dancer;

    void Start()
    {
        GameObject game = Instantiate(Dancers_pre[0], transform);
        dancer = game.GetComponent<DancerScript>();
    }

    public void SetBpm(double bpm)
    {
        if (dancer != null) dancer.ChangeSpeed(bpm);
    }

    public void StartPlay()
    {
        if (dancer != null) dancer.StartPlay();
    }

    public void Show(int index, bool show)
    {
        if (dancer == null) return;
        if (show)
            dancer.Apear(index);
        else
            dancer.Disapear(index);
    }

    public void ResetDancer()
    {
        if (dancer != null) dancer.Init();
    }
}
