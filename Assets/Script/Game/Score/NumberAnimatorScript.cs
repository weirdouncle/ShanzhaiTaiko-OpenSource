using System;
using UnityEngine;
using UnityEngine.UI;

public class NumberAnimatorScript : MonoBehaviour
{
    public Sprite[] Sprites;
    public Image[] Images;
    public Transform Panel;
    public float Scale = 1.5f;
    public bool AlignCenter = true;

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
        if (AlignCenter)
        {
            int length = Math.Min(str.Length, Images.Length);
            RectTransform panel_rect = Panel.GetComponent<RectTransform>();
            Panel.localPosition = new Vector3(-(panel_rect.sizeDelta.x / Images.Length) * ((float)Images.Length - length) / 2, 0);
        }
    }

    private string Reverse1(string original)
    {
        char[] arr = original.ToCharArray();
        Array.Reverse(arr);
        return new string(arr);
    }
}
