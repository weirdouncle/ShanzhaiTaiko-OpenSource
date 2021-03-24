using System;
using System.Collections.Generic;
using UnityEngine;

public class ComboNumberScript : SpriteNumberScript
{
    public Transform[] Effects;
    public Sprite[] Sprites2;
    public Sprite[] Sprites3;

    protected override void UpdateCount()
    {
        if (count == Count || Count < 0) return;

        string str = Reverse1(Count.ToString());

        if (Sprites3.Length == 0)
        {
            bool second = Count >= 100;
            List<Sprite> pics = second ? new List<Sprite>(Sprites2) : new List<Sprite>(Sprites);
            for (int i = 0; i < Images.Length; i++)
            {
                bool activa = str.Length >= i + 1;
                Images[i].gameObject.SetActive(activa);
                Effects[i].gameObject.SetActive(activa && second);
                if (activa)
                    Images[i].sprite = pics[int.Parse(str[i].ToString())];
            }
        }
        else
        {
            bool second = Count >= 50;
            List<Sprite> pics = second ? Count >= 100 ? new List<Sprite>(Sprites3) : new List<Sprite>(Sprites2) : new List<Sprite>(Sprites);
            for (int i = 0; i < Images.Length; i++)
            {
                bool activa = str.Length >= i + 1;
                Images[i].gameObject.SetActive(activa);
                if (Effects.Length > 0)
                    Effects[i].gameObject.SetActive(activa && second);
                if (activa)
                    Images[i].sprite = pics[int.Parse(str[i].ToString())];
            }
        }

        count = Count;
        int length = Math.Min(str.Length, Images.Length);
        Panel.localPosition = new Vector3(-Width * ((float)Images.Length - length) / 2, 0);
    }
}
