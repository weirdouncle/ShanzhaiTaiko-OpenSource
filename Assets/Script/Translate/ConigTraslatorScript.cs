using UnityEngine;
using UnityEngine.UI;

public class ConigTraslatorScript : MonoBehaviour
{
    public Text Title;
    public Text[] Options;
    public Text[] Descriptions;
    void Start()
    {
        Translate();
        ResolutionScript.ResetLanguage += Translate;
    }

    private void Translate()
    {
        Title.text = GameSetting.Translate("Config");

        Options[0].text = GameSetting.Translate("FullScreen");
        Options[1].text = GameSetting.Translate("Resolution");
        Options[2].text = GameSetting.Translate("Music Auto-rectify");
        Options[3].text = GameSetting.Translate("Gamepad Polling Frequency");
        Options[4].text = GameSetting.Translate("VSync");
        Options[5].text = GameSetting.Translate("Input Mode");
        Options[6].text = GameSetting.Translate("Play style");
        Options[7].text = GameSetting.Translate("Language");
        Options[8].text = GameSetting.Translate("Hardware Outline");

        Descriptions[0].text = GameSetting.Translate("disable this option to avoid CPU spike");
        Descriptions[1].text = GameSetting.Translate("duel mode or special stage will force tranditional style");
    }

    void OnDestroy()
    {
        ResolutionScript.ResetLanguage -= Translate;
    }
}
