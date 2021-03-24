using CommonClass;
using CriMana;
using System;
using UnityEngine;

public class CourseGaugeScript : MonoBehaviour
{
    public Transform ClearGuage;
    public Transform SoulGuage;
    public SpriteRenderer[] Clearchars;
    public GameObject ClearAnimation;
    public GameObject[] MaxAnimations;

    public bool Player2
    {
        get => player2;
        set
        {
            player2 = value; SetClearCount();
        }
    }

    protected float gauge;
    protected int clear_count = 0;
    protected int max_count = 0;
    protected int clear;
    private bool player2;

    void Start()
    {
        if (Clearchars != null && Clearchars.Length > 1)
        {
            Clearchars[0].sprite = SettingLoader.ClearSprites[1];
            Clearchars[1].sprite = SettingLoader.ClearSprites[0];
        }

        SetClearCount();
    }

    protected void SetClearCount()
    {
        Difficulty diff = GameSetting.Difficulty;
        if (Player2 && GameSetting.Player2) diff = GameSetting.Difficulty2P;
        switch (diff)
        {
            case Difficulty.Easy:
                clear = 6000 - 200;
                break;
            case Difficulty.Normal:
            case Difficulty.Hard:
                clear = 7000 - 200;
                break;
            default:
                clear = 8000 - 200;
                break;
        }
    }

    public virtual void ResetGauge()
    {
        gauge = 0;
        clear_count = 0;
        max_count = 0;

        ClearGuage.gameObject.SetActive(false);
        SoulGuage.gameObject.SetActive(false);
        Clearchars[1].gameObject.SetActive(false);

        ClearAnimation.SetActive(false);

        foreach (GameObject animation in MaxAnimations)
            animation.SetActive(false);

        ClearGuage.localScale = new Vector3(0, 1);
        SoulGuage.localScale = new Vector3(0, 1);
    }
    public virtual void UpdateGauge(float variation)
    {
        gauge = variation;
        gauge = Mathf.Max(0, gauge);
        gauge = Mathf.Min(gauge, 10000);

        int clear;
        switch (GameSetting.Difficulty)
        {
            case CommonClass.Difficulty.Easy:
                clear = 6000 - 200;
                break;
            case CommonClass.Difficulty.Normal:
            case CommonClass.Difficulty.Hard:
                clear = 7000 - 200;
                break;
            default:
                clear = 8000 - 200;
                break;
        }

        float clear_gauge = gauge > clear ? Math.Min(clear, gauge) : gauge;
        float soul_gauge = Math.Max(0, gauge - clear);

        soul_gauge = Math.Min(10000 - clear, soul_gauge);
        ClearGuage.gameObject.SetActive(gauge > 0);
        SoulGuage.gameObject.SetActive(gauge > clear + 200);
        Clearchars[1].gameObject.SetActive(gauge >= clear + 200);

        ClearAnimation.SetActive(gauge >= clear && gauge < 10000);
        foreach (GameObject animation in MaxAnimations)
            animation.SetActive(gauge == 10000);

        clear_count = (int)clear_gauge / 200;
        max_count = (int)soul_gauge / 200;
        //float clear_every = 1f / (clear / 200);
        //float max_every = 1f / ((10000 - clear) / 200);

        ClearGuage.localScale = new Vector3(clear_count, 1);
        SoulGuage.localScale = new Vector3(max_count, 1);
    }
}
