using UnityEngine;
using UnityEngine.UI;

public class PlayerNameNormalScript : MonoBehaviour
{
    public Text PlayerName;
    public Text Title;
    public Animation Animation;

    public void SetName(string player_name, string title)
    {
        PlayerName.text = player_name;
        Title.text = title;
    }

    public void ResetName(string player_name)
    {
        PlayerName.text = player_name;
    }
}
