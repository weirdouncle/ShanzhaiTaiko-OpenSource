using UnityEngine;
using UnityEngine.UI;

public class MainTraslatorScript : MonoBehaviour
{
    public Text[] Buttons;

    void Start()
    {
        Buttons[0].text = GameSetting.Translate("Game Start");
        Buttons[1].text = GameSetting.Translate("Costume");
        if (Buttons[2] != null)
            Buttons[2].text = GameSetting.Translate("Config");
        Buttons[3].text = GameSetting.Translate("Quit");

        if (Buttons.Length > 4)
            Buttons[4].text = GameSetting.Translate("RankingDojo");
    }
}
