using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonClass;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public delegate void RandomDelegate();
public delegate void SongSelectedDelegate(SongInfo info);
public class SongScanScript : MonoBehaviour
{
    public static event PlaySelectedSoundDelegate PlaySelectedSound;
    public static event SongSelectedDelegate SongSelected;
    public static event RandomDelegate Random;

    public Animator Animator;
    //public GameObject Song_pre;
    public Transform MainParent;
    public Transform LeftParent;
    public Transform RightParent;
    public Text GenTex;
    public Text GenNum;
    public Image GenPic;
    public GameConfigScript Config;
    public Animator FavoritesMessage;
    public SongDiffSelectScript DiffSelectPanel;
    public InputField Search;
    public Text SearchHolder;
    public SongInfoScript Center;
    public SongInfoScript[] Lefts;
    public SongInfoScript[] Rights;

    public bool DiffSelect { set; get; }
    public bool ScanFinished { set; get; }
    public SongInfo Selected { set; get; }

    private List<SongInfo> infos = new List<SongInfo>();
    private List<SongInfo> favorites = new List<SongInfo>();
    private string key_words = string.Empty;
    private string arrenge_key = string.Empty;
    private List<SongInfo> alls = new List<SongInfo>();
    private List<SongInfo> all_favorites = new List<SongInfo>();
    private Dictionary<SortColor, List<SongInfo>> Songs = new Dictionary<SortColor, List<SongInfo>>();
    private Dictionary<SongInfo, SortColor> song_color = new Dictionary<SongInfo, SortColor>();

    void Start()
    {
        GameSetting.Player2 = false;

        SongInfoScript.SongSelected += ReArrenge;

        SearchHolder.text = GameSetting.Translate("Search");
        random_freeze = false;

        if (GameSetting.Songs.Count > 0)
        {
            foreach (SortColor color in GameSetting.Songs.Keys)
            {
                List<SongInfo> songs = GameSetting.Songs[color];
                foreach (SongInfo info in songs)
                {
                    /*
                    GameObject song = Instantiate(Song_pre, transform);

                    song.SetActive(false);
                    SongInfoScript script = song.GetComponent<SongInfoScript>();
                    ColorUtility.TryParseHtmlString(color.Color, out Color new_color);
                    script.SetInfo(info, new_color);
                    */
                    alls.Add(info);
                    song_color[info] = color;

                    if (GameSetting.Config.Favorites.Contains(info.Title))
                    {
                        /*
                        GameObject favor = Instantiate(Song_pre, transform);
                        favor.SetActive(false);
                        SongInfoScript _script = favor.GetComponent<SongInfoScript>();
                        _script.SetInfo(info, new_color);
                        script.SetFavor(true);
                        _script.SetFavor(true);
                        */
                        SongInfo favor = info.CopyFavor();
                        song_color[favor] = color;
                        all_favorites.Add(favor);
                    }
                }
            }

            alls.AddRange(all_favorites);
            FilterSongs(false);
            ScanFinished = true;
        }

        StartCoroutine(DelayInput());
    }

    private void OnDestroy()
    {
        SongInfoScript.SongSelected -= ReArrenge;
        InputKeyListningScript.RebindKeys -= Rebingkeys;

        BasicInputScript.KeyInvoke -= DirectKey;
    }

    IEnumerator DelayInput()
    {
        yield return new WaitForSeconds(0.1f);

        BasicInputScript.Input.Player.Esc.performed += Click;
#if !UNITY_ANDROID
        BasicInputScript.Input.Player.Up.performed += Click;
        BasicInputScript.Input.Player.Down.performed += Click;
        BasicInputScript.Input.Player.Left.performed += Click;
        BasicInputScript.Input.Player.Right.performed += Click;
        BasicInputScript.Input.Player.LeftKa.performed += Click;
        BasicInputScript.Input.Player.RightKa.performed += Click;
        BasicInputScript.Input.Player.Cancel.performed += Click;
        BasicInputScript.Input.Player.Enter.performed += Click;
        BasicInputScript.Input.Player.RightDon.performed += Click;
        BasicInputScript.Input.Player.F1.performed += Click;
        BasicInputScript.Input.Player.Option.performed += Favor;
        BasicInputScript.Input.Player.Random.performed += RandomSelect;

        InputKeyListningScript.RebindKeys += Rebingkeys;
#endif

        if (GameSetting.Config.DirectInput)
            BasicInputScript.KeyInvoke += DirectKey;
    }
    private void DirectKey(GameSetting.KeyType key, bool press)
    {
        if (!ScanFinished || DiffSelect || GameConfigScript.Setting || random_freeze) return;

        if (key == GameSetting.KeyType.Confirm || key == GameSetting.KeyType.RightDon)
        {
            if (EventSystem.current.currentSelectedGameObject == Search.gameObject)
            {
                if (key != GameSetting.KeyType.RightDon)
                    ReFilter();
            }
            else
            {
                Confirm();
            }
            return;
        }

        if (EventSystem.current.currentSelectedGameObject == Search.gameObject) return;
        if (key == GameSetting.KeyType.Left && press)
        {
            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
            if (Lefts[0].gameObject.activeSelf)
                Lefts[0].OnClick(SongInfoScript.ClickType.Left, true);
            else if (Rights[0].gameObject.activeSelf)
                Rights[0].OnClick(SongInfoScript.ClickType.Left, true);
        }
        else if (key == GameSetting.KeyType.Right && press)
        {
            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
            if (Rights[0].gameObject.activeSelf)
                Rights[0].OnClick(SongInfoScript.ClickType.Right, true);
            else if (Lefts[0].gameObject.activeSelf)
                Lefts[0].OnClick(SongInfoScript.ClickType.Right, true);
        }
        else if (key == GameSetting.KeyType.Up)
        {
            Switch(true);
        }
        else if (key == GameSetting.KeyType.Down)
        {
            Switch(false);
        }
        else if (key == GameSetting.KeyType.Escape)
        {
            ScanFinished = false;
            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Cancel);
            StartCoroutine(LoadMain());
        }
        else if (key == GameSetting.KeyType.Config)
            Config.Show(true);
        else if (key == GameSetting.KeyType.Random)
            RandomSelect();
        else if (key == GameSetting.KeyType.Option)
            Favor();
    }
    private void Rebingkeys()
    {
        BasicInputScript.Input.Player.Cancel.performed -= Click;
        BasicInputScript.Input.Player.RightDon.performed -= Click;
        BasicInputScript.Input.Player.Option.performed -= Favor;

        BasicInputScript.Input.Player.Cancel.performed += Click;
        BasicInputScript.Input.Player.RightDon.performed += Click;
        BasicInputScript.Input.Player.Option.performed += Favor;
    }

    public bool EnablePlayer2()
    {
        if (ScanFinished && !random_freeze && EventSystem.current.currentSelectedGameObject != Search.gameObject)
            return true;

        return false;
    }

    private void FilterSongs(bool re_arrenge)
    {
        arrenge_key = key_words;

        infos.Clear();
        favorites.Clear();
        Songs.Clear();

        foreach (SortColor color in GameSetting.Songs.Keys)
        {
            List<SongInfo> color_infos = GameSetting.Songs[color];
            List<SongInfo> adds = new List<SongInfo>();
            foreach (SongInfo info in color_infos)
            {
                string title = info.Title.ToLower();
                if (string.IsNullOrEmpty(key_words) || title.Contains(key_words))
                {
                    infos.Add(info);
                    adds.Add(info);
                }
            }
            Songs.Add(color, adds);
        }

        foreach (SongInfo info in all_favorites)
        {
            string title = info.Title.ToLower();
            if (string.IsNullOrEmpty(key_words) || title.Contains(key_words))
                favorites.Add(info);
        }

        infos.AddRange(favorites);

        if (infos.Count > 0)
        {
            if (!re_arrenge)
                StartCoroutine(Arrenge());
            else
            {
                ColorUtility.TryParseHtmlString(song_color[infos[0]].Color, out Color new_color);
                Center.SetInfo(infos[0], new_color);
                Center.OnClick(SongInfoScript.ClickType.Mouse, true);
            }
        }
    }

    public void ReFilter()
    {
        if (!Application.isFocused || !ScanFinished || DiffSelect || GameConfigScript.Setting || random_freeze) return;

        EventSystem.current.SetSelectedGameObject(null);

        if (key_words != arrenge_key)
            FilterSongs(true);
    }

    public void SearchReset()
    {
        if (!Application.isFocused || !ScanFinished || DiffSelect || GameConfigScript.Setting || random_freeze) return;
        EventSystem.current.SetSelectedGameObject(null);

        key_words = Search.text = string.Empty;
        SearchHolder.text = GameSetting.Translate("Search");
        if (!string.IsNullOrEmpty(arrenge_key))
        {
            FilterSongs(true);
        }
    }

    public void OnSearchValueChange()
    {
        key_words = Search.text.ToLower();
    }

    private void RandomSelect(CallbackContext obj)
    {
        RandomSelect();
    }

    public void RandomSelect()
    {
        if (!Application.isFocused || !ScanFinished || DiffSelect || GameConfigScript.Setting) return;

        EventSystem.current.SetSelectedGameObject(null);
        if (random_freeze) return;
        random_freeze = true;
        StartCoroutine(DelayRandom());
    }

    public static bool random_freeze { set; get; }
    IEnumerator DelayRandom()
    {
        if (infos.Count > 0)
        {
            GenTex.text = "????";
            GenNum.text = string.Empty;

            Random?.Invoke();

            for (int i = 0; i < 15; i++)
            {
                yield return new WaitForSeconds(0.1f);

                int index = UnityEngine.Random.Range(0, infos.Count);
                ColorUtility.TryParseHtmlString(song_color[infos[index]].Color, out Color new_color);
                Center.SetInfo(infos[index], new_color);
            }

            yield return new WaitForSeconds(0.5f);
            random_freeze = false;
            Center.OnClick(SongInfoScript.ClickType.Mouse, true);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            random_freeze = false;
        }
    }

    private void Favor(CallbackContext context)
    {
        Favor();
    }

    public void Favor()
    {
        if (!Application.isFocused || !ScanFinished || DiffSelect || GameConfigScript.Setting || random_freeze) return;
        EventSystem.current.SetSelectedGameObject(null);

        if (Selected != null)
        {
            if (!GameSetting.Config.Favorites.Contains(Selected.Title))
            {
                if (GameSetting.Config.Favorites.Count >= 30)
                {
                    if (FavoritesMessage.gameObject.activeSelf)
                        FavoritesMessage.SetTrigger("Show");
                    else
                        FavoritesMessage.gameObject.SetActive(true);

                    return;
                }

                Center.SetFavor(true);
                GameSetting.SetFavorite(Selected.Title, true);
                SongInfo _script = Selected.CopyFavor();
                favorites.Add(_script);
                all_favorites.Add(_script);
                infos.Add(_script);
                alls.Add(_script);
                song_color[_script] = song_color[Selected];

                Center.OnClick(SongInfoScript.ClickType.Mouse, true);
            }
            else
            {
                Center.SetFavor(false);
                GameSetting.SetFavorite(Selected.Title, false);
                if (favorites.Contains(Selected))
                {
                    foreach (SongInfoScript info in Lefts)
                    {
                        if (info.Info.Title == Selected.Title && info.Favor.activeSelf)
                        {
                            info.SetFavor(false);
                            break;
                        }
                    }
                    foreach (SongInfoScript info in Rights)
                    {
                        if (info.Info.Title == Selected.Title && info.Favor.activeSelf)
                        {
                            info.SetFavor(false);
                            break;
                        }
                    }

                    infos.Remove(Selected);
                    alls.Remove(Selected);
                    favorites.Remove(Selected);
                    all_favorites.Remove(Selected);
                    Selected = null;

                    if (Rights[0].gameObject.activeSelf)
                        Rights[0].OnClick(SongInfoScript.ClickType.Left, true);
                    else
                        Lefts[0].OnClick(SongInfoScript.ClickType.Left, true);
                }
                else
                {
                    List<SongInfo> removes = favorites.FindAll(t => t.Title == Selected.Title);
                    infos.RemoveAll(t => removes.Contains(t));
                    alls.RemoveAll(t => removes.Contains(t));
                    favorites.RemoveAll(t => removes.Contains(t));
                    all_favorites.RemoveAll(t => removes.Contains(t));
                    Center.OnClick(SongInfoScript.ClickType.Mouse, true);
                }
            }
        }
    }
    IEnumerator Vibration(Gamepad pad)
    {
        pad.SetMotorSpeeds(0.8f, 0.8f);
        yield return new WaitForSecondsRealtime(0.5f);
        pad.ResetHaptics();
    }

    private void Click(CallbackContext context)
    {
        if (!Application.isFocused || !ScanFinished || DiffSelect || GameConfigScript.Setting || random_freeze) return;

        if (context.action == BasicInputScript.Input.Player.Enter || context.action == BasicInputScript.Input.Player.RightDon)
        {
            if (EventSystem.current.currentSelectedGameObject == Search.gameObject)
            {
                if (context.action != BasicInputScript.Input.Player.RightDon || context.control.device != Keyboard.current)
                    ReFilter();
            }
            else
            {
                if (context.control.device is Gamepad pad)
                    StartCoroutine(Vibration(pad));

                Confirm();
            }
            return;
        }

        if (EventSystem.current.currentSelectedGameObject == Search.gameObject) return;
        if (context.action == BasicInputScript.Input.Player.Left || context.action == BasicInputScript.Input.Player.LeftKa)
        {
            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
            if (Lefts[0].gameObject.activeSelf)
                Lefts[0].OnClick(SongInfoScript.ClickType.Left, true);
            else if (Rights[0].gameObject.activeSelf)
                Rights[0].OnClick(SongInfoScript.ClickType.Left, true);
        }
        else if (context.action == BasicInputScript.Input.Player.Right || context.action == BasicInputScript.Input.Player.RightKa)
        {
            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
            if (Rights[0].gameObject.activeSelf)
                Rights[0].OnClick(SongInfoScript.ClickType.Right, true);
            else if (Lefts[0].gameObject.activeSelf)
                Lefts[0].OnClick(SongInfoScript.ClickType.Right, true);
        }
        else if (context.action == BasicInputScript.Input.Player.Up)
        {
            Switch(true);
        }
        else if (context.action == BasicInputScript.Input.Player.Down)
        {
            Switch(false);
        }
        else if (context.action == BasicInputScript.Input.Player.Esc
            || (context.action == BasicInputScript.Input.Player.Cancel && context.control.device != Keyboard.current))
        {
            ScanFinished = false;
            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Cancel);
            StartCoroutine(LoadMain());
        }
        else if (context.action == BasicInputScript.Input.Player.F1)
            Config.Show(true);
    }

    private void Confirm()
    {
        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Don);
        Center.OnClick(SongInfoScript.ClickType.Mouse);
    }

    IEnumerator LoadMain()
    {
        string scene = "MainScreen";

        AsyncOperation async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scene);
        async.allowSceneActivation = true;
        yield return async;
    }

    public void Switch(bool left)
    {
        if (!ScanFinished || DiffSelect || GameConfigScript.Setting) return;

        int index = 0;
        List<SortColor> sorts = new List<SortColor>(Songs.Keys);

        if (left)
        {
            if (favorites.Contains(Selected))
            {
                index = sorts.Count - 1;
            }
            else
            {
                for (int i = 0; i < sorts.Count; i++)
                {
                    if (Songs[sorts[i]].Contains(Selected))
                    {
                        if (i > 0)
                            index = i - 1;
                        else
                            index = (favorites.Count == 0 ? sorts.Count - 1 : -1);

                        break;
                    }
                }
            }

            SongInfo select = null;
            if (index >= 0)
            {
                for (int i = 0; i < sorts.Count; i++)
                {
                    if (Songs[sorts[index]].Count > 0)
                    {
                        select = Songs[sorts[index]][Songs[sorts[index]].Count - 1];
                        break;
                    }
                    index--;
                    if (index < 0)
                    {
                        if (favorites.Count == 0)
                            index = sorts.Count - 1;
                        else
                            break;
                    }
                }
            }

            if (index < 0)
            {
                select = favorites[favorites.Count - 1];
            }

            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Switch);

            ColorUtility.TryParseHtmlString(song_color[select].Color, out Color new_color);
            Center.SetInfo(select, new_color);

            Center.OnClick(SongInfoScript.ClickType.Left, true);
        }
        else
        {
            if (favorites.Contains(Selected))
            {
                index = 0;
            }
            else
            {
                for (int i = 0; i < sorts.Count; i++)
                {
                    if (Songs[sorts[i]].Contains(Selected))
                    {
                        if (i < sorts.Count - 1)
                            index = i + 1;
                        else
                            index = favorites.Count == 0 ? 0 : -1;

                        break;
                    }
                }
            }

            SongInfo select = null;
            if (index >= 0)
            {
                for (int i = 0; i < sorts.Count; i++)
                {
                    if (Songs[sorts[index]].Count > 0)
                    {
                        select = Songs[sorts[index]][0];
                        break;
                    }
                    index++;
                    if (index >= sorts.Count)
                    {
                        if (favorites.Count == 0)
                            index = 0;
                        else
                        {
                            index = -1;
                            break;
                        }
                    }
                }
            }

            if (index < 0)
                select = favorites[0];

            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Switch);

            ColorUtility.TryParseHtmlString(song_color[select].Color, out Color new_color);
            Center.SetInfo(select, new_color);

            Center.OnClick(SongInfoScript.ClickType.Right, true);
        }
    }

    public void ResetPostion()
    {
        Animator.SetTrigger("Off");
    }

    private void ReArrenge(SongInfoScript info, SongInfoScript.ClickType type, bool only_arrenge)
    {
        if (!only_arrenge && Selected == info.Info)
        {
            DiffSelect = true;
            Animator.SetTrigger("On");
            //transform.localPosition = new Vector3(0, 1080);
            DiffSelectPanel.PlaySelect();
        }
        else
        {
            Selected = info.Info;
            Color color;
            if (favorites.Contains(Selected))
            {
                GameSetting.FavorSelected = true;
                GenTex.text = GameSetting.Config.FavorColor.Title;
                GenNum.text = string.IsNullOrEmpty(arrenge_key) ? favorites.Count.ToString() : string.Format("{0}({1})", favorites.Count.ToString(), all_favorites.Count.ToString());
                ColorUtility.TryParseHtmlString(GameSetting.Config.FavorColor.Color, out Color new_color);
                GenPic.color = new_color;

                ColorUtility.TryParseHtmlString(song_color[Selected].Color, out color);
                Center.SetFavor(true);
            }
            else
            {
                Center.SetFavor(GameSetting.Config.Favorites.Contains(Selected.Title));
                GameSetting.FavorSelected = false;

                GenTex.text = song_color[Selected].Title;
                GenNum.text = string.IsNullOrEmpty(arrenge_key) ? Songs[song_color[Selected]].Count.ToString() :
                    string.Format("{0}({1})", Songs[song_color[Selected]].Count.ToString(), GameSetting.Songs[song_color[Selected]].Count.ToString());
                ColorUtility.TryParseHtmlString(song_color[Selected].Color, out Color new_color);
                GenPic.color = color = new_color;
            }
            Center.SetInfo(Selected, color);

            int index = infos.IndexOf(Selected);

            List<SongInfo> arrenges = new List<SongInfo> { Selected };
            int count = infos.Count - 1;

            int right = type != SongInfoScript.ClickType.Left ? Math.Min((int)Math.Ceiling((float)count / 2), 4) : Math.Min(count / 2, 4);

            for (int i = 0; i < Rights.Length; i++)
            {
                //Debug.Log(string.Format("count {0} index {1} i {2} ", infos.Count, index, i));
                SongInfoScript script = Rights[i];
                if (i < right)
                {
                    script.gameObject.SetActive(true);
                    SongInfo info1 = infos.Count > index + 1 + i ? infos[index + i + 1] : infos[index + 1 + i - infos.Count];
                    arrenges.Add(info1);
                    ColorUtility.TryParseHtmlString(song_color[info1].Color, out Color new_color);
                    script.SetInfo(info1, new_color);
                    script.SetFavor(GameSetting.Config.Favorites.Contains(info1.Title));
                }
                else
                    script.gameObject.SetActive(false);
            }

            int left = Math.Min(4, count - right);

            //Debug.Log(string.Format("right {0} left {1}", right, left));

            for (int i = 0; i < Lefts.Length; i++)
            {
                SongInfoScript script = Lefts[i];
                if (i < left)
                {
                    script.gameObject.SetActive(true);
                    //Debug.Log(string.Format("count {0} index {1} i {2} ", infos.Count, index, i));
                    SongInfo info1 = index - 1 - i >= 0 ? infos[index - 1 - i] : infos[infos.Count + index - 1 - i];
                    arrenges.Add(info1);
                    ColorUtility.TryParseHtmlString(song_color[info1].Color, out Color new_color);
                    script.SetInfo(info1, new_color);
                    script.SetFavor(GameSetting.Config.Favorites.Contains(info1.Title));
                }
                else
                    script.gameObject.SetActive(false);

            }
            if (type != SongInfoScript.ClickType.Mouse)
            {
                Center.Forward(type == SongInfoScript.ClickType.Right);
                foreach (SongInfoScript script in Lefts)
                {
                    if (!script.gameObject.activeSelf) continue;
                    if (type == SongInfoScript.ClickType.Left)
                        script.Forward(false);
                    else if (type == SongInfoScript.ClickType.Right)
                        script.Forward(true);
                }
                foreach (SongInfoScript script in Rights)
                {
                    if (!script.gameObject.activeSelf) continue;
                    if (type == SongInfoScript.ClickType.Left)
                        script.Forward(false);
                    else if (type == SongInfoScript.ClickType.Right)
                        script.Forward(true);
                }
            }

            SongSelected?.Invoke(Selected);
        }
    }

    IEnumerator Arrenge()
    {
        yield return new WaitForFixedUpdate();

        string last_title = GameSetting.Config.LastPlay != null ? GameSetting.Config.LastPlay : string.Empty;
        SongInfo pre_select = null;
        if (Selected != null)
        {
            pre_select = Selected;
            Selected = null;
        }
        else if (!string.IsNullOrEmpty(last_title))
        {
            if (GameSetting.FavorSelected)
                pre_select = favorites.Find(t => t.Title == last_title);

            if (pre_select == null)
                pre_select = infos.Find(t => t.Title == last_title);
        }

        if (pre_select == null) pre_select = infos[0];

        ColorUtility.TryParseHtmlString(song_color[pre_select].Color, out Color new_color);
        Center.SetInfo(pre_select, new_color);

        Center.OnClick(SongInfoScript.ClickType.Mouse, true);
    }
}
