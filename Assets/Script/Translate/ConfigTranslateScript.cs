using UnityEngine;
using UnityEngine.UI;

public class ConfigTranslateScript : MonoBehaviour
{
    public Text Option;
    void Start()
    {
        Option.text = GameSetting.Translate("Conig(F1)");
    }
}
