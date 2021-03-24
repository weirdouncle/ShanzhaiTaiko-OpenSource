using UnityEngine;
using CommonClass;
using System;

public class GaugeScript : MonoBehaviour
{
    public GloableAniControllerScript Global;
    public UpFrameControllerScript UpFrame;
    public BottomScript BottomFrame;
    public GameObject Easy_pre;
    public GameObject Normal_pre;
    public GameObject Oni_pre;
    public DancerInitScript Dancer;
    public bool Player2;

    protected CourseGaugeScript GaugeBar;
    protected bool clear = false;
    protected bool max = false;

    void Start()
    {
#if UNITY_ANDROID
        if (!GameSetting.Dancer) Dancer = null;
#endif
        Difficulty diff = GameSetting.Difficulty;
        bool player2 = false;
        if (Player2 && GameSetting.Player2)
        {
            player2 = true;
            diff = GameSetting.Difficulty2P;
        }

        switch (diff)
        {
            case Difficulty.Easy:
                {
                    GameObject gauge = Instantiate(Easy_pre, transform);
                    GaugeBar = gauge.GetComponent<CourseGaugeScript>();
                }
                break;
            case Difficulty.Normal:
            case Difficulty.Hard:
                {
                    GameObject gauge = Instantiate(Normal_pre, transform);
                    GaugeBar = gauge.GetComponent<CourseGaugeScript>();
                }
                break;
            default:
                {
                    GameObject gauge = Instantiate(Oni_pre, transform);
                    GaugeBar = gauge.GetComponent<CourseGaugeScript>();
                }
                break;
        }

        if (player2) GaugeBar.Player2 = true;
    }

    public void ResetGauge()
    {
        bool old_clear = clear;
        bool old_max = max;
        clear = max = false;
        if (old_clear)
        {
            if (Player2)
                UpFrame.SetClear2P(false);
            else if (UpFrame != null)
                UpFrame.SetClear(false);

            if (BottomFrame != null) BottomFrame.SetClear(false);
        }
        if (old_max && BottomFrame != null) BottomFrame.SetMax(false);
        GaugeBar.ResetGauge();

        if (Player2)
        {
            Global.SetClearIn2P(false);
            Global.SetMaxIn2P(false);
        }
        else
        {
            Global.SetClearIn(false);
            Global.SetMaxIn(false);
        }
    }

    public virtual void UpdateGauge()
    {
        float gauge = Player2 ? GloableAniControllerScript.GaugePoints2P : GloableAniControllerScript.GaugePoints;

        int clear_count;
        Difficulty diff = GameSetting.Difficulty;
        if (Player2 && GameSetting.Player2)
            diff = GameSetting.Difficulty2P;

        switch (diff)
        {
            case Difficulty.Easy:
                clear_count = 6000;
                if (Dancer != null)
                {
                    if (gauge < 6000)
                        Dancer.Show(4, false);
                    else
                        Dancer.Show(4, true);
                    if (gauge < 4500)
                        Dancer.Show(3, false);
                    else
                        Dancer.Show(3, true);
                    if (gauge < 3000)
                        Dancer.Show(2, false);
                    else
                        Dancer.Show(2, true);
                    if (gauge < 1500)
                        Dancer.Show(1, false);
                    else
                        Dancer.Show(1, true);
                }
                break;
            case Difficulty.Normal:
            case Difficulty.Hard:
                clear_count = 7000;
                if (Dancer != null)
                {
                    if (gauge < 7000)
                        Dancer.Show(4, false);
                    else
                        Dancer.Show(4, true);
                    if (gauge < 5400)
                        Dancer.Show(3, false);
                    else
                        Dancer.Show(3, true);
                    if (gauge < 3600)
                        Dancer.Show(2, false);
                    else
                        Dancer.Show(2, true);
                    if (gauge < 1800)
                        Dancer.Show(1, false);
                    else
                        Dancer.Show(1, true);
                }
                break;
            default:
                clear_count = 8000;
                if (Dancer != null)
                {
                    if (gauge < 8000)
                        Dancer.Show(4, false);
                    else
                        Dancer.Show(4, true);
                    if (gauge < 6000)
                        Dancer.Show(3, false);
                    else
                        Dancer.Show(3, true);
                    if (gauge < 4000)
                        Dancer.Show(2, false);
                    else
                        Dancer.Show(2, true);
                    if (gauge < 2000)
                        Dancer.Show(1, false);
                    else
                        Dancer.Show(1, true);
                }
                break;
        }
        float clear_gauge = gauge > clear_count ? Math.Min(clear_count, gauge) : gauge;

        bool old_clear = clear;
        clear = clear_gauge == clear_count;
        if (old_clear != clear)
        {
            if (Player2)
                UpFrame.SetClear2P(clear);
            else if (UpFrame != null)
                UpFrame.SetClear(clear);

            if (BottomFrame != null)
                BottomFrame.SetClear(clear);

            if (Player2)
                Global.SetClearIn2P(gauge >= clear_count);
            else
                Global.SetClearIn(gauge >= clear_count);
        }

        bool old_max = max;
        max = gauge == 10000;
        if (old_max != max)
        {
            if (BottomFrame != null)
                BottomFrame.SetMax(max);
            if (Player2)
                Global.SetMaxIn2P(gauge == 10000);
            else
                Global.SetMaxIn(gauge == 10000);
        }

        GaugeBar.UpdateGauge(gauge);
    }
}
