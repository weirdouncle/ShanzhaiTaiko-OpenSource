using CommonClass;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NijiiroGameReuslt2PScript : MonoBehaviour
{
    public SpriteRenderer BackGround;
    public SpriteRenderer[] DiffChar;
    public GameObject Panel;
    public NumberAnimatorScript[] Numbers;
    public UI3DdonControll DonAvatar;
    public Text[] Texts;

    public GameObject[] Gauges;
    public Transform GaugeParent;
    public Transform CrownParent;
    public Transform RankParent;
    public GameObject PlayerName;

    public Image[] HitResult;
    public Image AutoMark;
    public Image SpeedMark;
    public Image RandomMark;
    public Image StealMark;
    public Image ShinMark;
    public Image ReverseMark;
    public Image ModeMark;
    public Sprite[] SpeedSprites;
    public Sprite[] RandomSprites;

    public int clear_gauge_2p { set; get; }
    public CourseGaugeScript gauge_bar_2p { set; get; }

    public void SetPlayer2(NijiiroResultScript result)
    {
        BackGround.gameObject.SetActive(true);
        DiffChar[0].gameObject.SetActive(true);
        Panel.SetActive(true);

        DonAvatar.gameObject.SetActive(true);
        DonAvatar.ResultStart();

        PlayerName.SetActive(true);

        foreach (Image image in HitResult)
            image.gameObject.SetActive(true);
        foreach (Text text in Texts)
            text.gameObject.SetActive(true);

        List<Transform> marks = new List<Transform>();
        if (GameSetting.Special2P == Special.AutoPlay)
        {
            AutoMark.gameObject.SetActive(true);
            marks.Add(AutoMark.transform);
        }
        if (GameSetting.Steal2P)
        {
            StealMark.gameObject.SetActive(true);
            marks.Add(StealMark.transform);
        }
        if (GameSetting.Speed2P != Speed.Normal)
        {
            SpeedMark.gameObject.SetActive(true);
            int index = (int)GameSetting.Speed2P - 2;
            if ((int)GameSetting.Speed2P > 10) index -= 6;
            SpeedMark.sprite = SpeedSprites[index];
            marks.Add(SpeedMark.transform);
        }
        if (GameSetting.Random2P != RandomType.None)
        {
            RandomMark.gameObject.SetActive(true);
            RandomMark.sprite = RandomSprites[(int)GameSetting.Random2P - 1];
            marks.Add(RandomMark.transform);
        }
        if (GameSetting.Revers2P)
        {
            ReverseMark.gameObject.SetActive(true);
            marks.Add(ReverseMark.transform);
        }

        if ((Score)GameSetting.Config.ScoreMode == Score.Shin)
        {
            ShinMark.gameObject.SetActive(true);
            marks.Add(ShinMark.transform);
        }

        for (int i = 0; i < marks.Count; i++)
            marks[i].localPosition = new Vector3(-880 + i * 24, marks[i].localPosition.y);

        switch (GameSetting.Difficulty2P)
        {
            case Difficulty.Easy:
                {
                    GameObject gauge = Instantiate(Gauges[0], GaugeParent);
                    gauge_bar_2p = gauge.GetComponent<CourseGaugeScript>();
                    clear_gauge_2p = 6000;
                }
                break;
            case Difficulty.Normal:
            case Difficulty.Hard:
                {
                    GameObject gauge = Instantiate(Gauges[1], GaugeParent);
                    gauge_bar_2p = gauge.GetComponent<CourseGaugeScript>();
                    clear_gauge_2p = 7000;
                }
                break;
            default:
                {
                    GameObject gauge = Instantiate(Gauges[2], GaugeParent);
                    gauge_bar_2p = gauge.GetComponent<CourseGaugeScript>();
                    clear_gauge_2p = 8000;
                }
                break;
        }
    }
}
