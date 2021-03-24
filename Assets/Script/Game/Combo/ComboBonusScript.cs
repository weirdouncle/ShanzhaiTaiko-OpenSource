using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ComboBonusScript : MonoBehaviour
{
    public Animator Panel;
    public SpriteNumberScript Number;
    public Transform ComboPanel;
    public SpriteRenderer ComboChar;
    public GameObject Bonus;
    public CriAtomSource Audio;
    public GameObject[] Showns;
    public GameObject NumberPics;
    public bool Player2;

    protected bool auto;
    protected int shown_layer = 5;
    protected int hide_layer = 12;

    void Start()
    {
        if (GameSetting.Mode == CommonClass.PlayMode.PlayWithReplay || GameSetting.Player2)
            Audio.pan3dAngle = Player2 ? 90 : -90;

        SetAuto();

        if (!Player2)
            OptionScript.SetAuto += SetAuto;

        ComboChar.sprite = SettingLoader.Combos[0];
    }

    void OnDestroy()
    {
        OptionScript.SetAuto -= SetAuto;
    }

    protected void SetAuto()
    {
        if (!Player2)
        {
            auto = GameSetting.Config.Special == CommonClass.Special.AutoPlay && GameSetting.Mode != CommonClass.PlayMode.Replay;
        }
        else
        {
            auto = GameSetting.Mode == CommonClass.PlayMode.PlayWithReplay ? false : GameSetting.Special2P == CommonClass.Special.AutoPlay;
        }

        if (auto)
        {
            Audio.cueSheet = Player2 ? "ComboMeta2" : "ComboMeta";
        }
        else
        {
            string acb_name = Player2 ? "Combo2P" : "Combo1P";
            Audio.cueSheet = acb_name;
        }

    }

    public virtual void Show(int combo)
    {
        if (!auto)
        {
            Audio.cueName = string.Format("{0}combo", combo);
            Audio.Play();
        }
        else
        {
            Audio.cueName = Math.Min(1500, combo).ToString();
            Audio.Play();
        }

        if (combo > 50)
        {
            StopAllCoroutines();
            if (!SceneManager.GetActiveScene().name.Contains("PS4"))
                ComboPanel.localPosition = (CommonClass.Score)GameSetting.Config.ScoreMode == CommonClass.Score.Normal && LoaderScript.ScoreModeTmp == 2 ?
                    new Vector3(0, 0.42f, -0.002f) : new Vector3(0, 0, -0.002f);

            Bonus.SetActive((CommonClass.Score)GameSetting.Config.ScoreMode == CommonClass.Score.Normal && LoaderScript.ScoreModeTmp == 2);
            Number.Count = combo;
            foreach (GameObject game in Showns)
                game.layer = shown_layer;
            Panel.enabled = true;
            Panel.SetTrigger("Show");
            StartCoroutine(Hide());
        }
    }

    IEnumerator Hide()
    {
        if (NumberPics != null) NumberPics.SetActive(true);
        yield return new WaitForSeconds(2.3f);
        if (NumberPics != null) NumberPics.SetActive(false);
        Panel.enabled = false;
        foreach (GameObject game in Showns)
            game.layer = hide_layer;
    }
}
