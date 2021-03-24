using UnityEngine;
using UnityEngine.UI;

public class GameTranslateScript : MonoBehaviour
{
    public Text[] Options;

    void Start()
    {
        Options[0].text = GameSetting.Translate("chapter");
        Options[1].text = GameSetting.Translate("play speed");
        Options[2].text = GameSetting.Translate("restart");
        Options[3].text = GameSetting.Translate("repeat");
        Options[4].text = GameSetting.Translate("auto play");
        Options[5].text = GameSetting.Translate("current branch");
        Options[6].text = GameSetting.Translate("show judge result");

        //loop
        Options[7].text = GameSetting.Translate("repeat");
        Options[8].text = GameSetting.Translate("Function");
        Options[9].text = GameSetting.Translate("start chapter");
        Options[10].text = GameSetting.Translate("end chapter");
        Options[11].text = GameSetting.Translate("back");
    }
}
