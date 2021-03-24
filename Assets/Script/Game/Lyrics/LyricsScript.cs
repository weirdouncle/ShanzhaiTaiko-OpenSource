using CommonClass;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class LyricsScript : MonoBehaviour
{
    public Text Text;

    private Queue<int> times = new Queue<int>();
    private Dictionary<int, Lyric> chips = new Dictionary<int, Lyric>();
    private List<Lyric> lyrics = new List<Lyric>();
    private Dictionary<int, Lyric> index_lyrics = new Dictionary<int, Lyric>();
    private float offset;

    private readonly double time_before_start = 1f + (15000f / 120f * (4f / 4f)) * 16f / 1000;
    void Start()
    {
        string path = GameSetting.SelectedInfo.Path.ToLower();
        path = path.Replace(".tja", ".lrc");
        if (File.Exists(path)) Decode(path);
    }

    void Update()
    {
        if (InputScript.Holding != InputScript.HoldStatus.Playing) return;
        if (times.Count > 0 && times.Peek() - (Time.time - PlayNoteScript.LastStart) * 1000 * PlayNoteScript.TimeScale <= 0)
        {
            int time = times.Dequeue();
            Lyric chip = chips[time];
            Text.text = chip.Content;
        }
    }

    private void Decode(string LrcPath)
    {
        using (FileStream fs = new FileStream(LrcPath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            string line;
            using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
            {
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("[ti:"))
                    {
                    }
                    else if (line.StartsWith("[ar:"))
                    {
                    }
                    else if (line.StartsWith("[al:"))
                    {
                    }
                    else if (line.StartsWith("[by:"))
                    {
                    }
                    else if (line.StartsWith("[offset:"))
                    {
                        offset = float.Parse(SplitInfo(line)) / 1000;
                    }
                    else
                    {
                        const string rx_pattern = @"\[([0-9.:]*)\]+(.*)";
                        Match result = Regex.Match(line, rx_pattern);
                        if (result.Success && result.Length > 0)
                        {
                            double time = TimeSpan.Parse("00:" + result.Groups[1].ToString()).TotalSeconds;
                            time += (time_before_start + offset);
                            string word = result.Groups[2].Value;
                            word = word.Replace("\\n", "\n");
                            Lyric lyric = new Lyric { JudgeTime = (int)(time * 1000), Content = word };
                            lyrics.Add(lyric);
                        }
                    }
                }
            }
        }

        lyrics.Sort((x, y) => { return x.JudgeTime < y.JudgeTime ? -1 : 1; });
        int index = 0;
        foreach (Lyric lyric in lyrics)
        {
            if (chips.ContainsKey(lyric.JudgeTime))
            {
                Debug.Log(lyric.JudgeTime);
                Debug.Log(lyric.Content);
            }

            chips.Add(lyric.JudgeTime, lyric);
            times.Enqueue(lyric.JudgeTime);
            lyric.Index = index;
            index_lyrics.Add(index, lyric);
            index++;
        }
    }

    public void ResetTimeLine()
    {
        if (lyrics.Count == 0) return;

        times.Clear();
        foreach (Lyric lyric in lyrics)
        {
            if ((float)lyric.JudgeTime / 1000 >= PlayNoteScript.CurrentTime)
                times.Enqueue(lyric.JudgeTime);
        }

        Text.text = string.Empty;
        if (times.Count > 0)
        {
            if ((float)times.Peek() / 1000 - PlayNoteScript.CurrentTime == 0)
            {
                int time = times.Dequeue();
                Lyric chip = chips[time];
                Text.text = chip.Content;
            }
            else
            {
                int index = chips[times.Peek()].Index;
                if (index > 0)
                {
                    Lyric chip = index_lyrics[index - 1];
                    Text.text = chip.Content;
                }
            }
        }
    }

    /// <summary>
    /// 处理信息(私有方法)
    /// </summary>
    /// <param name="line"></param>
    /// <returns>返回基础信息</returns>
    static string SplitInfo(string line)
    {
        return line.Substring(line.IndexOf(":") + 1).TrimEnd(']');
    }
}
