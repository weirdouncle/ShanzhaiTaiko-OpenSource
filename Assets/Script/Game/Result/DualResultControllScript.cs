using UnityEngine;

public class DualResultControllScript : ResultControllScript
{
    public UI3DdonControll Don2P;
    void Start()
    {
        GameObject game;
#if UNITY_ANDROID
        game = Instantiate(SettingLoader.Nijiiro.Result, transform);
        NijiiroResultScript script = game.GetComponent<NijiiroResultScript>();
        script.Mask = Mask;
        script.Avatar = Don;
        //script.Avatar2P = Don2P;
        this.script = script;
#else
        if (Result_pres.Length > 0)
        {
            if (GameResultStartScript.Scene.Contains("Nijiiro") || GameResultStartScript.Scene.Contains("Android"))
            {
                game = Instantiate(Result_pres[1], transform);
                script = game.GetComponent<GameResultScript>();
                script.Mask = Mask;
            }
            else
            {
                game = Instantiate(Result_pres[0], transform);
                DualResultScript script = game.GetComponent<DualResultScript>();
                script.Mask = Mask;
                script.Avatar = Don;
                script.Avatar2P = Don2P;
                this.script = script;
            }
        }
        else
        {

        }
#endif
    }
}
