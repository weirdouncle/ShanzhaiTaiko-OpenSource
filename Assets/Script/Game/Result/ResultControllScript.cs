using UnityEngine;

public class ResultControllScript : MonoBehaviour
{
    public GameObject[] Result_pres;
    public UI3DdonControll Don;
    public GameObject Mask;

    protected GameResultScript script;
    void Start()
    {
        if (GameResultStartScript.Scene.Contains("PS4"))
        {
            GameObject game = Instantiate(Result_pres[0], transform);
            script = game.GetComponent<GameResultScript>();
        }
        else
        {
            GameObject game = Instantiate(Result_pres[1], transform);
            script = game.GetComponent<GameResultScript>();
        }
        script.Avatar = Don;
        script.Mask = Mask;
    }

    public void Skip()
    {
        if (script != null)
            script.Skip();
    }
}
