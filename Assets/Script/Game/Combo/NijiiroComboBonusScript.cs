using System;
using System.Collections;
using UnityEngine;

public class NijiiroComboBonusScript : ComboBonusScript
{
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
    public override void Show(int combo)
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

        if (combo >= 50)
        {
            StopAllCoroutines();

            Number.transform.localScale = combo >= 1000 ? new Vector3(0.8f, 1) : new Vector3(1, 1);
            foreach (GameObject game in Showns)
                game.layer = shown_layer;
            Panel.enabled = true;
            Panel.SetTrigger("Show");
            Number.Count = combo;
            StartCoroutine(Hide());
        }
    }

    IEnumerator Hide()
    {
        yield return new WaitForSeconds(2.3f);
        Panel.enabled = false;
        foreach (GameObject game in Showns)
            game.layer = hide_layer;
    }
}
