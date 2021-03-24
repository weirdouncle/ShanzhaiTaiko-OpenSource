using UnityEngine;
using UnityEngine.UI;

public class RandomSelectScript : MonoBehaviour
{
    public Image Image;
    public Text Text;

    void Start()
    {
        Image.alphaHitTestMinimumThreshold = 0.1f;
        Text.text = GameSetting.Translate("Pick a song randomly (F2)");
    }

}
