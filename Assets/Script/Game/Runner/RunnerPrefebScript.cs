using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RunnerPrefebScript : MonoBehaviour
{
    public GameObject[] Runner_pres;

    public Transform Player2Panel;

    private RunnerControllScript runner;
    private RunnerControllScript runner2;

    void Start()
    {
        LoadAsset();
    }

    private void LoadAsset()
    {
        GameObject game = Instantiate(Runner_pres[0], transform);
        runner = game.GetComponent<RunnerControllScript>();
    }

    public void SetBpm(double bpm, bool player2)
    {
        if (!player2)
            runner.SetBpm(bpm);
        else if (runner2 != null)
            runner2.SetBpm(bpm);
    }
    public void ResetBpm()
    {
        runner.ResetBpm();

        if (runner2 != null)
            runner2.ResetBpm();
    }

    public void OnPlay(bool good, bool p1 = true)
    {
        if (p1)
            runner.OnPlay(good);
        else
            runner2.OnPlay(good);
    }

    public void ResetTimeLine()
    {
        runner.ResetTimeLine();
        if (runner2 != null)
            runner2.ResetTimeLine();
    }
}
