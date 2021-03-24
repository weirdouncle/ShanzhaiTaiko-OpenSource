using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BottomScript : MonoBehaviour
{
    public GameObject[] Clear_pres;
    public Transform ClearParent;
    public GameObject[] Max_pres;
    public Transform MaxParent;
    public GameObject[] Bottom_pres;
    public Transform BottomParent;
    public GameObject[] Foot_pres;
    public Transform FootParent;

    private bool switch_clear;
    private ClearScript clear;
    private MaxScript max;
    private GameObject foot;

    void Start()
    {
        LoadAsset();
    }
    public virtual void LoadAsset()
    {
        Instantiate(Bottom_pres[0], BottomParent);

        GameObject game = Instantiate(Clear_pres[0], ClearParent);
        clear = game.GetComponent<ClearScript>();


        foot = Instantiate(Foot_pres[0], FootParent);

        GameObject max_object = Instantiate(Max_pres[0], MaxParent);
        max = max_object.GetComponent<MaxScript>();
    }

    public void SetHalf(bool half)
    {
    }

    public void SetClear(bool clear)
    {
        if (switch_clear) foot.SetActive(!clear);
        if (this.clear != null) this.clear.SetClear(clear);
    }
    

    public void SetMax(bool max)
    {
        if (this.max != null) this.max.SetMax(max);
    }

    public void SetBPM(double bpm)
    {
        if (max != null) max.SetBPM(bpm);
    }
}
