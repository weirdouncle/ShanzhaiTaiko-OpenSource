using CommonClass;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionMarksScript : MonoBehaviour
{
    public GameObject[] Marks;

    public Image[] SingelMarks;
    public Image[] Player1Marks;
    public Image[] Player2Marks;

    public Sprite[] RandomSprites;
    public Sprite[] SpeedSprites;
    public Sprite[] ModeSprites;
    public Sprite[] SpecialSprites;

    public void RefreshMarks()
    {
        if (GameSetting.Player2)
        {
            Marks[1].SetActive(true);
            Marks[0].SetActive(false);

            if (GameSetting.Config.Random != RandomType.None)
            {
                Player1Marks[0].gameObject.SetActive(true);
                Player1Marks[0].sprite = RandomSprites[(int)GameSetting.Config.Random - 1];
            }
            else
                Player1Marks[0].gameObject.SetActive(false);

            if (GameSetting.Config.Speed != Speed.Normal)
            {
                Player1Marks[1].gameObject.SetActive(true);
                int index = (int)GameSetting.Config.Speed - 2;
                if ((int)GameSetting.Config.Speed > 10) index -= 6;

                Player1Marks[1].sprite = SpeedSprites[index];
            }
            else
                Player1Marks[1].gameObject.SetActive(false);

            Player1Marks[2].gameObject.SetActive(GameSetting.Config.Steal);
            Player1Marks[3].gameObject.SetActive(GameSetting.Config.Reverse);
            Player1Marks[4].gameObject.SetActive(GameSetting.Config.Special == Special.AutoPlay);
            Player1Marks[5].gameObject.SetActive((Score)GameSetting.Config.ScoreMode == Score.Shin);

            if (GameSetting.Random2P != RandomType.None)
            {
                Player2Marks[0].gameObject.SetActive(true);
                Player2Marks[0].sprite = RandomSprites[(int)GameSetting.Random2P - 1];
            }
            else
                Player2Marks[0].gameObject.SetActive(false);

            if (GameSetting.Speed2P != Speed.Normal)
            {
                Player2Marks[1].gameObject.SetActive(true);
                int index = (int)GameSetting.Speed2P - 2;
                if ((int)GameSetting.Speed2P > 10) index -= 6;

                Player2Marks[1].sprite = SpeedSprites[index];
            }
            else
                Player2Marks[1].gameObject.SetActive(false);

            Player2Marks[2].gameObject.SetActive(GameSetting.Steal2P);
            Player2Marks[3].gameObject.SetActive(GameSetting.Revers2P);
            Player2Marks[4].gameObject.SetActive(GameSetting.Special2P == Special.AutoPlay);
        }
        else
        {
            Marks[0].SetActive(true);
            Marks[1].SetActive(false);

            if (GameSetting.Config.Random != RandomType.None)
            {
                SingelMarks[0].gameObject.SetActive(true);
                SingelMarks[0].sprite = RandomSprites[(int)GameSetting.Config.Random - 1];
            }
            else
                SingelMarks[0].gameObject.SetActive(false);

            if (GameSetting.Config.Speed != Speed.Normal)
            {
                SingelMarks[1].gameObject.SetActive(true);
                int index = (int)GameSetting.Config.Speed - 2;
                if ((int)GameSetting.Config.Speed > 10) index -= 6;

                SingelMarks[1].sprite = SpeedSprites[index];
            }
            else
                SingelMarks[1].gameObject.SetActive(false);

            SingelMarks[2].gameObject.SetActive(GameSetting.Config.Steal);
            SingelMarks[3].gameObject.SetActive((Score)GameSetting.Config.ScoreMode == Score.Shin);
            SingelMarks[4].gameObject.SetActive(GameSetting.Config.Reverse);

            if (GameSetting.Mode != CommonClass.PlayMode.Normal)
            {
                SingelMarks[5].gameObject.SetActive(true);
                SingelMarks[5].sprite = GameSetting.Mode == CommonClass.PlayMode.Practice ? ModeSprites[0] : ModeSprites[1];
            }
            else
                SingelMarks[5].gameObject.SetActive(false);
            if (GameSetting.Config.Special != Special.None)
            {
                SingelMarks[6].gameObject.SetActive(true);
                SingelMarks[6].sprite = SpecialSprites[(int)GameSetting.Config.Special - 1];
            }
            else
                SingelMarks[6].gameObject.SetActive(false);
        }
    }
}
