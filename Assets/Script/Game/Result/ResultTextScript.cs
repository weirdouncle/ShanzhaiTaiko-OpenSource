using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultTextScript : MonoBehaviour
{
    public Text Text1;
    public Text Text2;

    public string Speech1;
    public string Speech2;

    public bool Player2;
    public CriAtomSource Audio;

    void Start()
    {
        Text1.text = GameSetting.Translate(Speech1);
        if (Text2 != null)
            Text2.text = GameSetting.Translate(Speech2);

        if (Player2)
            Audio.pan3dAngle = 90;
        else if (SceneManager.GetActiveScene().name == "ResultDual")
            Audio.pan3dAngle = -90;
    }
}
