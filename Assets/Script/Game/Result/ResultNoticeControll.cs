using CommonClass;
using System.Collections.Generic;
using UnityEngine;

public class ResultNoticeControll : MonoBehaviour
{
    public GameObject[] Results;
    public Transform Player2;
    public void ShowResult(GloableAniControllerScript gloableAni)
    {
       
            int bad = PlayNoteScript.Result[HitNoteResult.Bad].Count;
            int good = PlayNoteScript.Result[HitNoteResult.Good].Count;

            int clear_gauge;
            switch (GameSetting.Difficulty)
            {
                case Difficulty.Easy:
                    clear_gauge = 6000;
                    break;
                case Difficulty.Normal:
                case Difficulty.Hard:
                    clear_gauge = 7000;
                    break;
                default:
                    clear_gauge = 8000;
                    break;
            }

            bool clear = false;
            bool full = false;

            float gauge = GloableAniControllerScript.GaugePoints;
            if (gauge >= clear_gauge)
            {
                clear = true;
                if (bad == 0) full = true;
            }

            GameObject p1;
            if (!clear)
            {
                p1 = Instantiate(Results[0], transform);
            }
            else if (full)
            {
                if (good == 0)
                    p1 = Instantiate(Results[3], transform);
                else
                    p1 = Instantiate(Results[1], transform);
            }
            else
            {
                p1 = Instantiate(Results[2], transform);
            }

            if (Player2 != null)
            {
                bool clear2 = false;
                bool full2 = false;
                int bad2 = PlayNoteScript.Result2P[HitNoteResult.Bad].Count;
                int good2 = PlayNoteScript.Result2P[HitNoteResult.Good].Count;

                float gauge2 = GloableAniControllerScript.GaugePoints2P;
                int clear_gauge2;
                switch (GameSetting.Difficulty)
                {
                    case Difficulty.Easy:
                        clear_gauge2 = 6000;
                        break;
                    case Difficulty.Normal:
                    case Difficulty.Hard:
                        clear_gauge2 = 7000;
                        break;
                    default:
                        clear_gauge2 = 8000;
                        break;
                }
                if (gauge2 >= clear_gauge2)
                {
                    clear2 = true;
                    if (bad2 == 0) full2 = true;
                }


                GameObject p2;
                if (!clear2)
                {
                    p2 = Instantiate(Results[0], Player2);
                }
                else if (full2)
                {
                    if (good2 == 0)
                        p2 = Instantiate(Results[3], Player2);
                    else
                        p2 = Instantiate(Results[1], Player2);
                }
                else
                {
                    p2 = Instantiate(Results[2], Player2);
                }

                ResultNoticeScript script = p1.GetComponent<ResultNoticeScript>();
                script.SetAudioAngle(true);
                ResultNoticeScript script2 = p2.GetComponent<ResultNoticeScript>();
                script2.SetAudioAngle(false);
            }
    }
}
