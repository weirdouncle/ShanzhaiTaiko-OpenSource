using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SongGenreScript : MonoBehaviour
{
    public Image Image;
    public Text Text;
    void Start()
    {
        List<SortColor> colors = new List<SortColor>(GameSetting.Songs.Keys);
        string genre = GameSetting.SelectedInfo.GENRE;
        SortColor color = colors.Find(t => t.Title == genre);
        ColorUtility.TryParseHtmlString(color.Color, out Color new_color);
        Image.color = new_color;
        Text.text = genre;
    }
}
