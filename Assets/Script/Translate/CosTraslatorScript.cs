using UnityEngine;
using UnityEngine.UI;

public class CosTraslatorScript : MonoBehaviour
{
    public Text Title;

    void Start()
    {
        Title.text = GameSetting.Translate("Don Chan's room");
    }
}
