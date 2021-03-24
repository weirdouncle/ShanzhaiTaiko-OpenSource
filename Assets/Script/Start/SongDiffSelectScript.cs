using CommonClass;
using ICSharpCode.SharpZipLib.GZip;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputAction;

public delegate void PlaySongInfoDelegate(SongInfo info);
public class SongDiffSelectScript : MonoBehaviour
{
    public static event PlaySelectedSoundDelegate PlaySelectedSound;
    public static event PlaySongInfoDelegate Play;

    public Text Title;
    public Text SubTitle;
    public DifficultyButtonScript[] Buttons;
    public Animator Turn;

    public Text[] PlayerName;
    public Text[] Score;
    public Text[] Perfect;
    public Text[] Good;
    public Text[] Bad;
    public Text[] Combo;
    public Text[] MaxRapid;

    public SongScanScript MainController;
    public SongPanelScript SongInfoPanel;
    public GameConfigScript Config;
    public CriAtomSource Audio;

    public DonSongSelect DonAvatar;
    public DonSongSelect DonAvatar2P;
    public GameObject[] GameModes;
    public OptionMarksScript OptionMark;

    public Animator ReplayMessage;
    public Animator[] Players;
    public GameObject[] Player2Hide;

    protected SongInfo Info;
    protected int index = -1;
    protected int index_2p = -1;
    private List<Difficulty> available_diff;
    protected SelectMode mode = SelectMode.None;
    protected Difficulty selected_diff;
    protected Difficulty selected_diff_2p;
    protected WASDInput BasicInput;
    protected Replay replay;
    protected Dictionary<int, bool> confirm;

    protected enum SelectMode
    {
        None,
        Diff,
        Option
    }
    void Start()
    {
        SongScanScript.SongSelected += Selected;
        SongScanScript.Random += RandomSelect;

        BasicInput = BasicInputScript.Input;
        BasicInput.Player.Esc.performed += Click;
#if !UNITY_ANDROID
        BasicInput.Player.Left.performed += Click;
        BasicInput.Player.Right.performed += Click;
        BasicInput.Player.LeftKa.performed += Click;
        BasicInput.Player.RightKa.performed += Click;
        BasicInput.Player.Cancel.performed += Click;
        BasicInput.Player.Enter.performed += Click;
        BasicInput.Player.RightDon.performed += Click;
        BasicInput.Player.F1.performed += Click;
        BasicInput.Player.Option.performed += Mode;

        if (BasicInput.Player.Left2P != null)
            BasicInput.Player.Left2P.performed += Click2P;
        if (BasicInput.Player.Right2P != null)
            BasicInput.Player.Right2P.performed += Click2P;
        if (BasicInput.Player.Confirm2P != null)
            BasicInput.Player.Confirm2P.performed += Click2P;

        InputKeyListningScript.RebindKeys += Rebingkeys;
#endif

        if (GameSetting.Config.DirectInput)
        {
            BasicInputScript.KeyInvoke += DirectKey;
            BasicInputScript.KeyInvoke2P += DirectKey2P;
        }

        RefreshModeMark();
        confirm = new Dictionary<int, bool> { { 0, false }, { 1, false } };
    }

    private void OnDisable()
    {
        InputKeyListningScript.RebindKeys -= Rebingkeys;
        BasicInputScript.KeyInvoke -= DirectKey;
        BasicInputScript.KeyInvoke2P -= DirectKey2P;
    }

    private void DirectKey(GameSetting.KeyType key, bool press)
    {
        if (!MainController.DiffSelect || mode == SelectMode.None || GameConfigScript.Setting) return;

        if (mode == SelectMode.Diff)
        {
            if (confirm[0]) return;
            if (key == GameSetting.KeyType.Escape || key == GameSetting.KeyType.Cancel)
            {
                Back2SongSelect();
            }
            else if (key == GameSetting.KeyType.Confirm || key == GameSetting.KeyType.RightDon)
            {
                if (GameSetting.Player2)
                {
                    confirm[0] = true;
                    Players[0].SetTrigger("Confirm");
                }

                PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Don);
                StartGame();
            }
            else if (key == GameSetting.KeyType.Left && press)
            {
                int index = available_diff.IndexOf((Difficulty)this.index);
                if (index - 1 >= 0)
                {
                    int old = this.index;
                    if (index == 4 && available_diff[index - 1] == Difficulty.Oni && available_diff.Count > 2)
                    {
                        this.index = (int)available_diff[index - 2];
                    }
                    else
                    {
                        if (index != 4 || !confirm[1] || selected_diff_2p != Difficulty.Edit)
                        {
                            this.index = (int)available_diff[index - 1];
                        }
                        else
                            return;
                    }

                    PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                    SetSelect(this.index, old);
                }
            }
            else if (key == GameSetting.KeyType.Right && press)
            {
                int index = available_diff.IndexOf((Difficulty)this.index);
                if (index == 4 && available_diff[index - 1] == Difficulty.Oni && (!confirm[1] || selected_diff_2p != Difficulty.Edit))
                {
                    PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                    this.index = 3;
                    SetSelect(3, 4);
                }
                else if (index + 1 < available_diff.Count)
                {
                    if (available_diff[index] == Difficulty.Oni && confirm[1] && selected_diff_2p == Difficulty.Edit) return;

                    int old = this.index;
                    if (available_diff[index + 1] == Difficulty.Oni && confirm[1] && selected_diff_2p == Difficulty.Edit)
                    {
                        this.index = (int)available_diff[index + 2];
                    }
                    else
                    {
                        if (available_diff[index + 1] == Difficulty.Edit && confirm[1] && selected_diff_2p == Difficulty.Oni) return;
                        this.index = (int)available_diff[index + 1];
                    }

                    PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                    SetSelect(this.index, old);
                }
            }
            else if (key == GameSetting.KeyType.Option)
                Mode();
            else if (key == GameSetting.KeyType.Config)
                Config.Show(true);
        }
        else if (key == GameSetting.KeyType.Cancel || key == GameSetting.KeyType.Escape)
        {
            CloseMode();
        }
    }

    private void DirectKey2P(GameSetting.KeyType key, bool press)
    {
        if (GameConfigScript.Setting || confirm[1]) return;

        if (!GameSetting.Player2)
        {
            GameSetting.Speed2P = Speed.Normal;
            GameSetting.Random2P = RandomType.None;
            GameSetting.Steal2P = false;
            GameSetting.Revers2P = false;
            GameSetting.Special2P = Special.None;

            bool enable = false;
            if (!MainController.DiffSelect)
            {
                enable = MainController.EnablePlayer2();
            }
            else if (mode == SelectMode.Diff)
            {
                enable = true;
            }
            if (enable)
            {
                GameSetting.Player2 = true;
                foreach (GameObject game in Player2Hide)
                    game.SetActive(false);
                foreach (Animator animator in Players)
                    animator.gameObject.SetActive(true);

                DonAvatar2P.gameObject.SetActive(true);
                GameSetting.Mode = CommonClass.PlayMode.Normal;
                RefreshModeMark();

                if (MainController.DiffSelect)
                {
                    SetSelect2P(index_2p);
                    DonAvatar2P.Selected();
                }
            }
        }
        else if (MainController.DiffSelect)
        {
            if (mode == SelectMode.Diff)
            {
                if (key == GameSetting.KeyType.RightDon)
                {
                    confirm[1] = true;
                    Players[1].SetTrigger("Confirm");

                    PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Don);
                    StartGame();
                }
                else if (key == GameSetting.KeyType.LeftKa)
                {
                    int index = available_diff.IndexOf((Difficulty)this.index_2p);
                    if (index - 1 >= 0)
                    {
                        if (index == 4 && available_diff[index - 1] == Difficulty.Oni && available_diff.Count > 2)
                        {
                            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                            this.index_2p = (int)available_diff[index - 2];
                            SetSelect2P(this.index_2p);
                        }
                        else if (available_diff[index - 1] != Difficulty.Oni)
                        {
                            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                            this.index_2p = (int)available_diff[index - 1];
                            SetSelect2P(this.index_2p);
                        }
                    }
                }
                else if (key == GameSetting.KeyType.RightKa)
                {
                    int index = available_diff.IndexOf((Difficulty)this.index_2p);
                    if (index + 1 < available_diff.Count && available_diff[index + 1] <= Difficulty.Oni)
                    {
                        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                        this.index_2p = available_diff[index + 1] == Difficulty.Oni && Turn.GetBool("Edit") ? (int)available_diff[index + 2] : (int)available_diff[index + 1];
                        SetSelect2P(this.index_2p);
                    }
                }
            }
        }
    }
    private void Rebingkeys()
    {
        BasicInputScript.Input.Player.Cancel.performed -= Click;
        BasicInputScript.Input.Player.Option.performed -= Mode;
        BasicInputScript.Input.Player.RightDon.performed -= Click;

        BasicInputScript.Input.Player.Cancel.performed += Click;
        BasicInputScript.Input.Player.Option.performed += Mode;
        BasicInputScript.Input.Player.RightDon.performed += Click;

        if (BasicInput.Player.Left2P != null)
            BasicInput.Player.Left2P.performed -= Click2P;
        if (BasicInput.Player.Right2P != null)
            BasicInput.Player.Right2P.performed -= Click2P;
        if (BasicInput.Player.Confirm2P != null)
            BasicInput.Player.Confirm2P.performed -= Click2P;

        if (BasicInput.Player.Left2P != null)
            BasicInput.Player.Left2P.performed += Click2P;
        if (BasicInput.Player.Right2P != null)
            BasicInput.Player.Right2P.performed += Click2P;
        if (BasicInput.Player.Confirm2P != null)
            BasicInput.Player.Confirm2P.performed += Click2P;
    }
    public void RefreshModeMark()
    {
        OptionMark.RefreshMarks();
    }

    private void OnDestroy()
    {
        SongScanScript.SongSelected -= Selected;
        SongScanScript.Random -= RandomSelect;
    }

    private void Selected(SongInfo info)
    {
        if (info == Info) return;
        Info = info;
        available_diff = new List<Difficulty>();
        if (Info.Difficulties.Contains(Difficulty.Easy))
            available_diff.Add(Difficulty.Easy);
        if (Info.Difficulties.Contains(Difficulty.Normal))
            available_diff.Add(Difficulty.Normal);
        if (Info.Difficulties.Contains(Difficulty.Hard))
            available_diff.Add(Difficulty.Hard);
        if (Info.Difficulties.Contains(Difficulty.Oni))
            available_diff.Add(Difficulty.Oni);
        if (Info.Difficulties.Contains(Difficulty.Edit))
            available_diff.Add(Difficulty.Edit);

        Title.text = Info.Title;
        SubTitle.text = Info.SubTitle;
        SubTitle.gameObject.SetActive(!string.IsNullOrEmpty(Info.SubTitle));

        foreach (DifficultyButtonScript button in Buttons)
            button.OnInit(Info);

        CheckReplay();
    }

    private void Mode(CallbackContext context)
    {
        Mode();
    }

    public virtual void Mode()
    {
        if (!Application.isFocused || !MainController.DiffSelect || mode != SelectMode.Diff || GameConfigScript.Setting) return;
        EventSystem.current.SetSelectedGameObject(null);

        mode = SelectMode.Option;
        if (GameSetting.Player2 && GameModes.Length > 1)
            GameModes[1].SetActive(true);
        else
            GameModes[0].SetActive(true);
    }

    public void OnClick(BaseEventData data)
    {
        if (!Application.isFocused || !MainController.DiffSelect || mode == SelectMode.None || GameConfigScript.Setting) return;
        if (data is PointerEventData pointer && pointer.button == PointerEventData.InputButton.Right)
            Back2SongSelect();
    }

    public void OnBackButton()
    {
        if (!Application.isFocused || !MainController.DiffSelect || mode == SelectMode.None || GameConfigScript.Setting) return;
        Back2SongSelect();
    }

    Gamepad pad = null;

    private void Click(CallbackContext context)
    {
        if (!Application.isFocused || !MainController.DiffSelect || mode == SelectMode.None || GameConfigScript.Setting) return;
        if (mode == SelectMode.Diff)
        {
            if (confirm[0]) return;
            if (context.action == BasicInput.Player.Cancel || context.action == BasicInput.Player.Esc)
            {
                Back2SongSelect();
            }
            else if (context.action == BasicInput.Player.Enter || context.action == BasicInput.Player.RightDon)
            {
                if (context.control.device is Gamepad pad)
                {
                    this.pad = pad;
                    pad.SetMotorSpeeds(1, 1);
                }

                if (GameSetting.Player2)
                {
                    confirm[0] = true;
                    Players[0].SetTrigger("Confirm");
                }

                PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Don);
                StartGame();
            }
            else if (context.action == BasicInput.Player.Left || context.action == BasicInput.Player.LeftKa)
            {
                int index = available_diff.IndexOf((Difficulty)this.index);
                if (index - 1 >= 0)
                {
                    int old = this.index;
                    if (index == 4 && available_diff[index - 1] == Difficulty.Oni && available_diff.Count > 2)
                    {
                        this.index = (int)available_diff[index - 2];
                    }
                    else
                    {
                        if (index != 4 || !confirm[1] || selected_diff_2p != Difficulty.Edit)
                        {
                            this.index = (int)available_diff[index - 1];
                        }
                        else
                            return;
                    }

                    PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                    SetSelect(this.index, old);
                }
            }
            else if (context.action == BasicInput.Player.Right || context.action == BasicInput.Player.RightKa)
            {
                int index = available_diff.IndexOf((Difficulty)this.index);
                if (index == 4 && available_diff[index - 1] == Difficulty.Oni && (!confirm[1] || selected_diff_2p != Difficulty.Edit))
                {
                    PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                    this.index = 3;
                    SetSelect(3, 4);
                }
                else if (index + 1 < available_diff.Count)
                {
                    if (available_diff[index] == Difficulty.Oni && confirm[1] && selected_diff_2p == Difficulty.Edit) return;

                    int old = this.index;
                    if (available_diff[index + 1] == Difficulty.Oni && confirm[1] && selected_diff_2p == Difficulty.Edit)
                    {
                        this.index = (int)available_diff[index + 2];
                    }
                    else
                    {
                        if (available_diff[index + 1] == Difficulty.Edit && confirm[1] && selected_diff_2p == Difficulty.Oni) return;
                        this.index = (int)available_diff[index + 1];
                    }

                    PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                    SetSelect(this.index, old);
                }
            }
            else if (context.action == BasicInput.Player.F1)
                Config.Show(true);
        }
        else if (context.action == BasicInput.Player.Cancel || context.action == BasicInput.Player.Esc)
        {
            CloseMode();
        }
    }
    private void Click2P(CallbackContext context)
    {
        if (!Application.isFocused || GameConfigScript.Setting || confirm[1]) return;

        if (!GameSetting.Player2)
        {
            GameSetting.Speed2P = Speed.Normal;
            GameSetting.Random2P = RandomType.None;
            GameSetting.Steal2P = false;
            GameSetting.Revers2P = false;
            GameSetting.Special2P = Special.None;

            bool enable = false;
            if (!MainController.DiffSelect)
            {
                enable = MainController.EnablePlayer2();
            }
            else if (mode == SelectMode.Diff)
            {
                enable = true;
            }
            if (enable)
            {
                GameSetting.Player2 = true;
                foreach (GameObject game in Player2Hide)
                    game.SetActive(false);
                foreach (Animator animator in Players)
                    animator.gameObject.SetActive(true);

                DonAvatar2P.gameObject.SetActive(true);
                GameSetting.Mode = CommonClass.PlayMode.Normal;
                RefreshModeMark();

                if (MainController.DiffSelect)
                {
                    SetSelect2P(index_2p);
                    DonAvatar2P.Selected();
                }
            }
        }
        else if (MainController.DiffSelect)
        {
            if (mode == SelectMode.Diff)
            {
                if (context.action == BasicInput.Player.Confirm2P)
                {
                    confirm[1] = true;
                    Players[1].SetTrigger("Confirm");

                    PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Don);
                    StartGame();
                }
                else if (context.action == BasicInput.Player.Left2P)
                {
                    int index = available_diff.IndexOf((Difficulty)this.index_2p);
                    if (index - 1 >= 0)
                    {
                        if (index == 4 && available_diff[index - 1] == Difficulty.Oni && available_diff.Count > 2)
                        {
                            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                            this.index_2p = (int)available_diff[index - 2];
                            SetSelect2P(this.index_2p);
                        }
                        else if (available_diff[index - 1] != Difficulty.Oni)
                        {
                            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                            this.index_2p = (int)available_diff[index - 1];
                            SetSelect2P(this.index_2p);
                        }
                    }
                }
                else if (context.action == BasicInput.Player.Right2P)
                {
                    int index = available_diff.IndexOf((Difficulty)this.index_2p);
                    if (index + 1 < available_diff.Count && available_diff[index + 1] <= Difficulty.Oni)
                    {
                        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
                        this.index_2p = available_diff[index + 1] == Difficulty.Oni && Turn.GetBool("Edit") ? (int)available_diff[index + 2] : (int)available_diff[index + 1];
                        SetSelect2P(this.index_2p);
                    }
                }
            }
        }
    }

    public void CloseMode()
    {
        EventSystem.current.SetSelectedGameObject(null);
        foreach (GameObject mode in GameModes)
            if (mode != null) mode.SetActive(false);
        StartCoroutine(DelayInput());
    }

    public virtual void Back2SongSelect()
    {
        EventSystem.current.SetSelectedGameObject(null);
        DonAvatar.UnSelect();
        if (GameSetting.Player2) DonAvatar2P.UnSelect();
        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Cancel);
        //transform.localPosition = new Vector3(0, -1080);
        //MainController.transform.localPosition = new Vector3(0, 0);
        MainController.ResetPostion();
        mode = SelectMode.None;
        StartCoroutine(DelayInput2());
    }

    IEnumerator DelayInput2()
    {
        yield return new WaitForSeconds(0.5f);
        MainController.DiffSelect = false;
    }

    public void OnClick(Difficulty difficulty)
    {
        if (!MainController.DiffSelect || mode == SelectMode.None || GameConfigScript.Setting) return;
        int old = index;
        index = (int)difficulty;

        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
        SetSelect(index, old);
    }

    private void SetSelect(int index, int old_index)
    {
        selected_diff = (Difficulty)index;
        selected_diff_2p = (Difficulty)index_2p;
#if !UNITY_ANDROID

        if (GameSetting.Player2)
        {
            bool edit = Turn.GetBool("Edit");
            if (edit && index != 4 && available_diff[available_diff.Count - 2] == Difficulty.Oni)
            {
                if (!confirm[1] || selected_diff_2p != Difficulty.Edit)
                {
                    Turn.SetBool("Edit", false);
                    if (!Info.Difficulties.Contains(Difficulty.Edit) || !Info.Difficulties.Contains(Difficulty.Oni))
                        Turn.SetTrigger("Reset");

                    if (selected_diff_2p == Difficulty.Edit)
                    {
                        index_2p = (int)available_diff[available_diff.Count - 2];
                        selected_diff_2p = (Difficulty)index_2p;
                    }
                }
            }
            else if (index == 4 && !edit)
            {
                Turn.SetBool("Edit", true);
                if (selected_diff_2p == Difficulty.Oni)
                {
                    index_2p = index;
                    selected_diff_2p = Difficulty.Edit;
                }
            }

            if (index == 4 || selected_diff_2p == Difficulty.Edit)
            {
                Buttons[3].Activate = false;
                Buttons[4].Activate = true;
            }
            else
            {
                Buttons[4].Activate = false;
                Buttons[3].Activate = true;
            }

            for (int i = 0; i < Buttons.Length; i++)
            {
                if (Buttons[i].Difficulty == selected_diff || (Buttons[i].Difficulty == Difficulty.Oni && selected_diff == Difficulty.Edit)
                        || (Buttons[i].Difficulty == Difficulty.Edit && selected_diff == Difficulty.Oni))
                    Buttons[i].OnSelected(true, false);
                else
                    Buttons[i].OnSelected(false, false);

                if (Buttons[i].Difficulty == selected_diff_2p || (Buttons[i].Difficulty == Difficulty.Oni && selected_diff_2p == Difficulty.Edit)
                        || (Buttons[i].Difficulty == Difficulty.Edit && selected_diff_2p == Difficulty.Oni))
                    Buttons[i].OnSelected(true, true);
                else
                    Buttons[i].OnSelected(false, true);

                Players[0].transform.localPosition = new Vector3((index >= 3 ? Turn.transform.localPosition.x : Buttons[index].transform.localPosition.x) - 0.5f,
                    Players[0].transform.localPosition.y, Players[0].transform.localPosition.z);
                Players[1].transform.localPosition = new Vector3((index_2p >= 3 ? Turn.transform.localPosition.x : Buttons[index_2p].transform.localPosition.x) + 0.5f,
                    Players[1].transform.localPosition.y, Players[1].transform.localPosition.z);
            }
        }
        else
        {
            for (int i = 0; i < Buttons.Length; i++)
                Buttons[i].OnSelected(selected_diff);

            //Debug.Log(string.Format("old {0} new {1}", old_index, index));

            bool edit = Turn.GetBool("Edit");
            if (edit && index != 4)
            {
                Turn.SetBool("Edit", false);
                if (!Info.Difficulties.Contains(Difficulty.Edit) || !Info.Difficulties.Contains(Difficulty.Oni))
                    Turn.SetTrigger("Reset");
            }
            else if (index == 4 && !edit)
                Turn.SetBool("Edit", true);

            if (index == 4)
            {
                Buttons[3].Activate = false;
                Buttons[4].Activate = true;
            }
            else
            {
                Buttons[4].Activate = false;
                Buttons[3].Activate = true;
            }
        }
#else
        for (int i = 0; i < Buttons.Length; i++)
            Buttons[i].OnSelected(selected_diff);
#endif
        //分数
        for (int i = 0; i < 2; i++)
        {
            Score mode = i == 0 ? CommonClass.Score.Normal : CommonClass.Score.Shin;
            PlayerName[i].text = GameSetting.Record.Record.GetBestPlayer(selected_diff, Info.Title, mode);
            Score[i].text = GameSetting.Record.Record.GetScore(Info.Title, selected_diff, (int)mode).ToString();
            Perfect[i].text = GameSetting.Record.Record.GetPerfect(Info.Title, selected_diff, mode).ToString();
            Good[i].text = GameSetting.Record.Record.GetGood(Info.Title, selected_diff, mode).ToString();
            Bad[i].text = GameSetting.Record.Record.GetBad(Info.Title, selected_diff, mode).ToString();
            Combo[i].text = GameSetting.Record.Record.GetMaxCombo(Info.Title, selected_diff, mode).ToString();
            MaxRapid[i].text = GameSetting.Record.Record.GetMaxHits(Info.Title, selected_diff, mode).ToString();
        }
    }

    private void SetSelect2P(int index)
    {
        selected_diff = (Difficulty)this.index;
        selected_diff_2p = (Difficulty)index;

        for (int i = 0; i < Buttons.Length; i++)
        {
            if (Buttons[i].Difficulty == selected_diff || (Buttons[i].Difficulty == Difficulty.Oni && selected_diff == Difficulty.Edit)
                    || (Buttons[i].Difficulty == Difficulty.Edit && selected_diff == Difficulty.Oni))
                Buttons[i].OnSelected(true, false);
            else
                Buttons[i].OnSelected(false, false);

            if (Buttons[i].Difficulty == selected_diff_2p || (Buttons[i].Difficulty == Difficulty.Oni && selected_diff_2p == Difficulty.Edit)
                    || (Buttons[i].Difficulty == Difficulty.Edit && selected_diff_2p == Difficulty.Oni))
                Buttons[i].OnSelected(true, true);
            else
                Buttons[i].OnSelected(false, true);
        }

        Players[0].transform.localPosition = new Vector3((this.index >= 3 ? Turn.transform.localPosition.x : Buttons[this.index].transform.localPosition.x) - 0.5f,
            Players[0].transform.localPosition.y, Players[0].transform.localPosition.z);
        Players[1].transform.localPosition = new Vector3((index_2p >= 3 ? Turn.transform.localPosition.x : Buttons[index_2p].transform.localPosition.x) + 0.5f,
            Players[1].transform.localPosition.y, Players[1].transform.localPosition.z);
    }

    public void PlaySelect()
    {
        confirm = new Dictionary<int, bool> { { 0, false }, { 1, false } };
        int old = index;
        if (index == -1) index = GameSetting.Config.LastDiff;
        if (!Info.Difficulties.Contains((Difficulty)index))
            index = (int)available_diff[available_diff.Count - 1];

        index_2p = (int)available_diff[0];

        SetSelect(index, old);

        mode = SelectMode.None;
        DonAvatar.Selected();
        if (GameSetting.Player2) DonAvatar2P.Selected();
        Audio.cueName = "v_diffsel";
        Audio.Play();
        StartCoroutine(DelayInput());
    }

    private void RandomSelect()
    {
        Audio.cueName = "random";
        Audio.Play();
    }

    IEnumerator DelayInput()
    {
        yield return new WaitForSeconds(0.4f);
        mode = SelectMode.Diff;
    }

    public void StartGame(int index)
    {
        if (!MainController.DiffSelect || mode == SelectMode.None || GameConfigScript.Setting) return;

        selected_diff = (Difficulty)index;

        if (GameSetting.Player2)
        {
            confirm[0] = true;
            Players[0].SetTrigger("Confirm");
        }

        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Don);
        StartGame();
    }

    protected void ShowMessage()
    {
        if (ReplayMessage.gameObject.activeSelf)
            ReplayMessage.SetTrigger("Show");
        else
            ReplayMessage.gameObject.SetActive(true);
    }

    public virtual void StartGame()
    {
        EventSystem.current.SetSelectedGameObject(null);

        if (GameSetting.Player2 && (!confirm[0] || !confirm[1])) return;
        mode = SelectMode.None;

        if (GameSetting.Mode == CommonClass.PlayMode.Replay || GameSetting.Mode == CommonClass.PlayMode.PlayWithReplay)
        {
            if (!replay.Input.ContainsKey(GameSetting.Config.ScoreMode) || !replay.Input[GameSetting.Config.ScoreMode].ContainsKey((int)selected_diff))
            {
                ShowMessage();
                mode = SelectMode.Diff;

                if (pad != null) pad.ResetHaptics();

                return;
            }
            else
            {
                GameSetting.Replay = replay;
                SettingLoader.LoadReplaySkins(replay.Config[GameSetting.Config.ScoreMode][(int)selected_diff]);
            }
        }
        else
            GameSetting.Replay = null;

        GameSetting.SelectedInfo = Info;
        SongInfoPanel.StopMusic();
        SongInfoPanel.LoadMusic();

        GameSetting.Difficulty = selected_diff;
        GameSetting.Difficulty2P = selected_diff_2p;
        GameSetting.SetLastPlay();
        StartCoroutine(DelayLoad());
    }

    protected void CheckReplay()
    {
        string filepath = Info.Path;
        filepath = filepath.Remove(filepath.Length - 4);
        filepath = string.Format("{0}.rep", filepath);
        try
        {
            if (File.Exists(filepath))
            {
                GZipInputStream zipFile = new GZipInputStream(File.OpenRead(filepath));
                using (MemoryStream re = new MemoryStream(50000))
                {
                    int count;
                    byte[] data = new byte[50000];
                    while ((count = zipFile.Read(data, 0, data.Length)) != 0)
                    {
                        re.Write(data, 0, count);
                    }
                    byte[] overarr = re.ToArray();
                    re.Close();

                    //将byte数组转为string
                    string result = System.Text.Encoding.UTF8.GetString(overarr);
                    replay = JsonUntity.Json2Object<Replay>(result);
                }
                zipFile.Close();
            }
            else
                replay = new Replay();
        }
        catch (Exception)
        {
            replay = new Replay();
        }
    }

    private AsyncOperation async = null;
    IEnumerator DelayLoad()
    {
        DonAvatar.StartPlay();
        if (GameSetting.Player2) DonAvatar2P.StartPlay();
        PlayStart();

        yield return new WaitForSeconds(0.5f);

        Audio.cueName = "v_start";
        Audio.Play();

        yield return new WaitForSeconds(2);

        if (pad != null)
            pad.ResetHaptics();

        string scene = "Game";
        string stage = string.IsNullOrEmpty(Info.Stage) ? string.Empty : Info.Stage.ToLower() ;
        if (!GameSetting.Player2)
        {
            switch (GameSetting.Mode)
            {
                case CommonClass.PlayMode.PlayWithReplay:
                    scene = "DuelGame";
                    break;
            }
        }
        else
        {
            scene = "DualGame";
        }
        async = SceneManager.LoadSceneAsync(scene);
        async.allowSceneActivation = true;
    }

    private bool audio_freeze;
    protected void PlaySound(StartSoundControllScript.SoundType type)
    {
        if (!audio_freeze)
        {
            StartCoroutine("AudioDelay");
            PlaySelectedSound?.Invoke(type);
        }
    }

    IEnumerator AudioDelay()
    {
        audio_freeze = true;
        yield return new WaitForSeconds(0.1f);
        audio_freeze = false;
    }

    protected void PlayStart()
    {
        Play?.Invoke(Info);
    }
}
