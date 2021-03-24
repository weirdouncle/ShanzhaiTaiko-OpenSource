using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonClass;

public class DancerScript : MonoBehaviour
{

    public GameObject[] Dancer_pres;
    public GameObject[] Dancer_a_pres;
    public Transform ApearParent;
    public Vector3[] Vectors;
    public bool Suit4Nijiiro;
    public bool Nijiiro { set; get; } = false;

    protected DancingObjectScript[] dancers;
    protected DancerControllScript[] apears;

    protected Dictionary<int, bool> shows = new Dictionary<int, bool> { { 0, false }, { 1, false }, { 2, false }, { 3, false }, { 4, false } };
    void Start()
    {
        DancerControllScript.ApearFinish += OnApear;

        Dictionary<int, Vector3> index_position = new Dictionary<int, Vector3>();
        for (int i = 0; i < Vectors.Length; i++)
            index_position[i] = Vectors[i];

        List<int> indexs = new List<int> { 0, 1, 2, 3, 4 };
        Shuffle.shuffle(ref indexs);

        dancers = new DancingObjectScript[indexs.Count];
        apears = new DancerControllScript[indexs.Count];
        for (int index = 0; index < indexs.Count; index++)
        {
            int i = indexs[index];
            if (i >= Dancer_pres.Length)
            {
                i = Random.Range(0, Dancer_pres.Length);
            }

            GameObject game = Instantiate(Dancer_pres[i], transform);
            game.transform.SetAsFirstSibling();
            game.transform.localPosition = index_position[index];
            DancingObjectScript game_ob = game.GetComponent<DancingObjectScript>();
            dancers[index] = game_ob;

            game = Instantiate(Dancer_a_pres[i], ApearParent);
            game.transform.localPosition = index_position[index];
            DancerControllScript script = game.GetComponent<DancerControllScript>();
            script.Index = index;
            apears[index] = script;
        }
    }

    private void OnDestroy()
    {
        DancerControllScript.ApearFinish -= OnApear;
    }

    public virtual void ChangeSpeed(double bpm)
    {
        foreach (DancingObjectScript animator in dancers)
            animator.SetBpm(bpm);
    }

    public virtual void Init()
    {
        for (int i = 0; i < dancers.Length; i++)
        {
            if (shows[i])
            {
                shows[i] = false;
                dancers[i].Act(false);
                apears[i].Show(false);
            }
        }
        foreach (DancingObjectScript animator in dancers)
            animator.Act(false);
    }

    public void StartPlay()
    {
        StartCoroutine(DelayPlay());
    }

    IEnumerator DelayPlay()
    {
        yield return new WaitForEndOfFrame();
        Apear(0);
    }

    public virtual void Apear(int index)
    {
        if (!shows[index] && apears.Length > index)
        {
            if (index == 0 && Suit4Nijiiro && Nijiiro)
            {
                for (int i = 0; i < 3; i++)
                {
                    shows[i] = true;
                    apears[i].Show(true);
                }
            }
            else if (index >= 3 || !Suit4Nijiiro || !Nijiiro)
            {
                shows[index] = true;
                apears[index].Show(true);
            }
        }
    }

    public virtual void OnApear(int index)
    {
        if (index == 0)
        {
            foreach (DancingObjectScript animator in dancers)
                animator.Act(true);
        }
        dancers[index].Show(true);
    }

    public virtual void Disapear(int index)
    {
        if (shows[index])
        {
            if (index >= 3 || !Suit4Nijiiro || !Nijiiro)
            {
                shows[index] = false;
                dancers[index].Show(false);
                apears[index].Show(false);
            }
        }
    }
}
