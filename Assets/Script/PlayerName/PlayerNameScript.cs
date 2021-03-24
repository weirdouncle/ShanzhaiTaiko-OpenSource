using CommonClass;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameScript : MonoBehaviour
{
    public Text PlayerName;
    public Text Title;
    public Animation Animation;
    void Start()
    {
        if (GameSetting.Mode == CommonClass.PlayMode.Replay)
        {
            PlayerName.text = GameSetting.Replay.Config[GameSetting.Config.ScoreMode][(int)GameSetting.Difficulty].PlayerName;
            Title.text = string.Empty;
        }
        else
        {
            PlayerName.text = GameSetting.Config.PlayerName;
            KeyValuePair<ClearState, string> title = GameSetting.GetTitle();
            Title.text = title.Value;
            if (title.Key == ClearState.GreatClear)
            {
                Title.color = new Color(.89f, .69f, 0);
            }
            else if (title.Key == ClearState.NormalClear)
            {
                Title.color = new Color(1, 1, 1);
            }
            else if (title.Key == ClearState.PerfectClear)
            {
                Animation.enabled = true;
            }
        }
    }
}
