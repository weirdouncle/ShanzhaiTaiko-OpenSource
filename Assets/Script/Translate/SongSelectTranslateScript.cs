using UnityEngine;
using UnityEngine.UI;

public class SongSelectTranslateScript : MonoBehaviour
{
    public Text[] Titles;
    public Text[] Options;
    public Text[] Descriptions;
    void Start()
    {
        Titles[0].text = GameSetting.Translate("Record");
        Titles[1].text = GameSetting.Translate("Record");
        Titles[2].text = GameSetting.Translate("score");
        Titles[3].text = GameSetting.Translate("perfect");
        Titles[4].text = GameSetting.Translate("good");
        Titles[5].text = GameSetting.Translate("bad");
        Titles[6].text = GameSetting.Translate("max combo");
        Titles[7].text = GameSetting.Translate("hits");

        Options[0].text = GameSetting.Translate("Conig(F1)");

        //游戏模式


#if !UNITY_ANDROID
        Descriptions[0].text = GameSetting.Translate("press option to add/remove your favorite song");
        Descriptions[1].text = GameSetting.Translate("press option to change game setting");
#else
        Descriptions[0].text = GameSetting.Translate("Add or remove favorite");
        Descriptions[1].text = GameSetting.Translate("Open options menu");
#endif
        Descriptions[2].text = GameSetting.Translate("reach the maximum number of 30");
        Descriptions[3].text = GameSetting.Translate("found no replay");
    }
}
