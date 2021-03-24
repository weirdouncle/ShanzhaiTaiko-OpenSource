using CommonClass;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DifficultyButtonScript : MonoBehaviour
{
    public Sprite[] Numbers;
    public Sprite[] Crowns;

    public SpriteRenderer Diff;
    public SpriteRenderer DiffChar;
    public SpriteRenderer Crown;
    public SpriteRenderer LvNumber;
    public GameObject Ten;
    public GameObject Branch;
    public GameObject Turn;
    public SongDiffSelectScript Song;
    public Difficulty Difficulty;
    public GameObject SelectedFrame;
    public bool Activate = true;
    private bool selected;

    public bool Selected {
        get => selected;
        set {
            selected = value;
#if !UNITY_ANDROID
            SelectedFrame.SetActive(value);
#endif
        }
    }

    void Start()
    {
        DiffChar.sprite = SettingLoader.Diffs[(int)Difficulty];
    }

    public void OnInit(SongInfo info)
    {
        if (!info.Difficulties.Contains(Difficulty))
        {
            gameObject.SetActive(false);
            return;
        }
        else
            gameObject.SetActive(true);

        if (GameSetting.Record.Record.IsFullCombo(info.Title, Difficulty))
        {
            Crown.gameObject.SetActive(true);
            Crown.sprite = Crowns[1];
        }
        else if (GameSetting.Record.Record.IsClear(info.Title, Difficulty))
        {
            Crown.gameObject.SetActive(true);
            Crown.sprite = Crowns[0];
        }
        else
            Crown.gameObject.SetActive(false);

        Branch.gameObject.SetActive(info.Branches.ContainsKey(Difficulty));
        if (info.Levels.TryGetValue(Difficulty, out int lv) && lv > 0)
        {
            if (lv == 10)
            {
                Ten.SetActive(true);
                LvNumber.gameObject.SetActive(false);
            }
            else
            {
                Ten.SetActive(false);
                LvNumber.gameObject.SetActive(true);
                LvNumber.sprite = Numbers[lv];
            }
        }
        else
        {
            Ten.SetActive(false);
            LvNumber.gameObject.SetActive(true);
            LvNumber.sprite = Numbers[0];
        }

#if !UNITY_ANDROID
        if (Difficulty == Difficulty.Oni)
            Turn.SetActive(info.Difficulties.Contains(Difficulty.Edit));
#else
        Turn.SetActive(false);
        SelectedFrame.SetActive(true);
#endif
    }

    public void OnSelected(Difficulty difficulty)
    {
#if !UNITY_ANDROID
        if (Difficulty == difficulty || (Difficulty == Difficulty.Oni && difficulty == Difficulty.Edit) || (Difficulty == Difficulty.Edit && difficulty == Difficulty.Oni))
            transform.localScale = new Vector3(1.2f, 1.2f);
        else
            transform.localScale = new Vector3(1f, 1f);
#else
        if (Difficulty == difficulty)
            transform.localScale = new Vector3(1.2f, 1.2f);
        else
            transform.localScale = new Vector3(1, 01);
#endif
        Selected = Difficulty == difficulty;
    }

    private bool player1_selected;
    private bool player2_selected;
    public void OnSelected(bool selected, bool player2)
    {
        if (selected)
        {
            transform.localScale = new Vector3(1.2f, 1.2f);
            Selected = true;

            if (player2)
                player2_selected = true;
            else
                player1_selected = true;
        }
        else
        {
            if (player2)
                player2_selected = false;
            else
                player1_selected = false;

            if (!player1_selected && !player2_selected)
            {
                transform.localScale = new Vector3(1f, 1f);
                Selected = false;
            }
        }
    }

    public void OnHavor()
    {
        if (Activate)
            Song.OnClick(Difficulty);
    }

    public void OnClick(BaseEventData data)
    {
        if (data is PointerEventData pointer)
        {
            if (Activate && pointer.button == PointerEventData.InputButton.Left)
            {
#if !UNITY_ANDROID
                Song.StartGame((int)Difficulty);
#else
                if (selected)
                    Song.StartGame((int)Difficulty);
                else
                    Song.OnClick(Difficulty);
#endif
            }
            else if (pointer.button == PointerEventData.InputButton.Right)
                Song.Back2SongSelect();
        }
    }
}
