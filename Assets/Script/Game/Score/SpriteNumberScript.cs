using System;
using UnityEngine;
using UnityEngine.UI;

public class SpriteNumberScript : MonoBehaviour
{
    public Sprite[] Sprites;
    public SpriteRenderer[] Images;
    public Transform Panel;
    public Align Position = Align.AlignCenter;
    public float Width;

    public enum Align
    {
        AlignCenter,
        AlignLeft,
        AlignRight
    }

    public int Count
    {
        get => _count;
        set
        {
            _count = value;
            UpdateCount();
        }
    }

    protected int count;
    protected int _count;
    private bool init;

    protected virtual void UpdateCount()
    {
        int Count = _count;
        if (count == Count) return;
        string str = Reverse1(Count.ToString());

        for (int i = 0; i < Images.Length; i++)
        {
            bool activa = str.Length >= i + 1;
            Images[i].gameObject.SetActive(activa);
            if (activa)
                Images[i].sprite = Sprites[int.Parse(str[i].ToString())];
        }
        count = Count;

        switch (Position)
        {
            case Align.AlignCenter:
                {
                    int length = Math.Min(str.Length, Images.Length);
                    Panel.localPosition = new Vector3(-Width * ((float)Images.Length - length) / 2, 0);
                }
                break;
            case Align.AlignLeft:
                {
                    int length = Math.Min(str.Length, Images.Length);
                    Panel.localPosition = new Vector3(Width * (length - (float)Images.Length / 2), 0);
                }
                break;
        }
    }

    protected string Reverse1(string original)
    {
        char[] arr = original.ToCharArray();
        Array.Reverse(arr);
        return new string(arr);
    }
}
