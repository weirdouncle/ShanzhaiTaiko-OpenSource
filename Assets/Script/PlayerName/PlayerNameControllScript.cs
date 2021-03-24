using CommonClass;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerNameControllScript : MonoBehaviour
{
    public GameObject Normal_pre;
    public GameObject Clear_pre;
    public GameObject GoldClear_pre;
    public GameObject Perfect_pre;
    public bool Player2;

    private PlayerNameNormalScript script;
    void Start()
    {
        if (!Player2)
        {
            if (SceneManager.GetActiveScene().name.Contains("Game") && GameSetting.Mode == CommonClass.PlayMode.Replay)
            {
                GameObject game = Instantiate(Normal_pre, transform);
                PlayerNameNormalScript script = game.GetComponent<PlayerNameNormalScript>();
                script.SetName(GameSetting.Replay.Config[GameSetting.Config.ScoreMode][(int)GameSetting.Difficulty].PlayerName, string.Empty);
            }
            else
            {
                KeyValuePair<ClearState, string> title = GameSetting.GetTitle();
                GameObject game = null;
                switch (title.Key)
                {
                    case ClearState.NoClear:
                        game = Instantiate(Normal_pre, transform);
                        break;
                    case ClearState.NormalClear:
                        game = Instantiate(Clear_pre, transform);
                        break;
                    case ClearState.GreatClear:
                        game = Instantiate(GoldClear_pre, transform);
                        break;
                    case ClearState.PerfectClear:
                        game = Instantiate(Perfect_pre, transform);
                        break;
                }

                script = game.GetComponent<PlayerNameNormalScript>();
                if (title.Key != ClearState.NoClear && script.Animation != null)
                {
                    if (title.Key == ClearState.PerfectClear)
                        script.Animation.enabled = true;
                    else if (title.Key == ClearState.GreatClear)
                        script.Title.color = new Color(.89f, .69f, 0);
                    else
                        script.Title.color = new Color(1, 1, 1);
                }
                script.SetName(GameSetting.Config.PlayerName, title.Value);
            }
        }
        else
        {
            GameObject game = Instantiate(Normal_pre, transform);
            script = game.GetComponent<PlayerNameNormalScript>();

            if (SceneManager.GetActiveScene().name.Contains("DuelGame"))
                script.SetName(GameSetting.Replay.Config[GameSetting.Config.ScoreMode][(int)GameSetting.Difficulty].PlayerName, string.Empty);
            else
                script.SetName("GUEST", string.Empty);
        }
    }

    public void ChangeName()
    {
        StartCoroutine(DelayName());
    }

    IEnumerator DelayName()
    {
        yield return new WaitForEndOfFrame();
        script.ResetName(GameSetting.Config.PlayerName);
    }
}
