using CommonClass;
using System.Collections;
using TooSimpleFramework.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public delegate void SongInfoSelectedDelegate(SongInfoScript info, SongInfoScript.ClickType type, bool only_arrenge);
public class SongInfoScript : MonoBehaviour
{
    public static event SongInfoSelectedDelegate SongSelected;

    public enum ClickType
    {
        Mouse,
        Left,
        Right
    }

    public Text Text1;
    public Image Image;
    public SpriteRenderer Pic;
    public Animator Animator;
    public GameObject SelectedFrame;
    public Sprite[] Sprites;
    public GameObject Full;
    public GameObject Clear;
    public GameObject Favor;
    public Text FullText;
    public Text ClearText;
    public OutlineEx Outline;

    private bool _selected;
    //private RectTransform rect;

    public SongInfo Info { private set; get; }
    public bool Selected {
        get => _selected;
        set {
            _selected = value;
            //SetSelected();
        }
    }

    private void SetSelected()
    {
        /*
        SelectedFrame.SetActive(_selected);
        Image.sprite = _selected ? Sprites[0] : Sprites[1];
        transform.localScale = _selected ? new Vector3(2, 2, 1) : new Vector3(1, 1, 1);
        Pic.gameObject.layer = _selected ? 2 : 1;
        Text1.transform.localScale = _selected ? new Vector3(0.5f, 0.5f) : new Vector3(1, 1);
        Text1.fontSize = _selected ? 50 : 25;
        if (rect == null)
            rect = Text1.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(_selected ? 400 : 200, 0);
        */
    }

    void Start()
    {
        Image.alphaHitTestMinimumThreshold = 0.1f;
    }

    public void SetInfo(SongInfo info, Color color)
    {
        Info = info;
        Text1.text = Info.Title;
        //color.a = 0.8f;
        Pic.color = color;

        int clear = 0;
        int full = 0;
        foreach (Difficulty difficulty in info.Difficulties)
        {
            bool all = GameSetting.Record.Record.IsFullCombo(info.Title, difficulty);
            if (all)
                full++;
            else if (GameSetting.Record.Record.IsClear(info.Title, difficulty))
                clear++;
        }

        Full.SetActive(full > 0);
        FullText.text = full.ToString();

        Clear.SetActive(clear > 0);
        ClearText.text = clear.ToString();

        StartCoroutine(DelaySetTitle());
    }

    IEnumerator DelaySetTitle()
    {
        yield return new WaitForEndOfFrame();
        Outline.SetColor(Outline.OutlineColor);
    }

    public void OnClick(ClickType click, bool only_arrenge = false)
    {
        EventSystem.current.SetSelectedGameObject(null);
        SongSelected?.Invoke(this, click, only_arrenge);
    }

    public void OnClick(BaseEventData data)
    {
        if (Info != null && !SongScanScript.random_freeze && data is PointerEventData pointer && pointer.button == PointerEventData.InputButton.Left)
        {
            EventSystem.current.SetSelectedGameObject(null);
            SongSelected?.Invoke(this, ClickType.Mouse, false);
        }
    }

    public void Forward(bool right)
    {
        Animator.SetTrigger(right ? "Right" : "Left");
    }

    public void SetFavor(bool favor)
    {
        Favor.SetActive(favor);
    }
}
