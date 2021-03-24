using CommonClass;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SongPackScript : MonoBehaviour
{
    public TextAsset SongList;
    public TextAsset[] Songs;

    private void Start()
    {
        Dictionary<string, AtomSongInfo> songs = new Dictionary<string, AtomSongInfo>();
        foreach (TextAsset text in Songs)
        {
            AtomSongInfo info = new AtomSongInfo(text.name, string.Empty, text.name, DaniGenre.Animation);
            songs[text.name] = info;
        }

        string path = string.Format("{0}/songs.txt", Environment.CurrentDirectory);
        File.Delete(path);
        File.AppendAllText(path, JsonUntity.Object2Json(songs));
    }
}
