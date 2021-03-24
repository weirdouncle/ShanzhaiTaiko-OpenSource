using CommonClass;
using UnityEngine;

public delegate void ShowEndDelegate(JudgeExplosionScript note);
public class JudgeExplosionScript : MonoBehaviour
{
    public event ShowEndDelegate ShowEnd;

    public int Type;
    public HitNoteResult Result;
    public SpriteRenderer Chara;
    public GameObject[] Sprites;
    public Animator Animator;

    protected int shown_layer = 5;
    protected int hide_layer = 12;
    public void Init()
    {
        int index = 0;
        switch (Result)
        {
            case HitNoteResult.Good:
                index = 1;
                break;
            case HitNoteResult.Bad:
                index = 2;
                break;
        }

        Chara.sprite = SettingLoader.NoteResults[index];
    }

    public void Show()
    {
        foreach (GameObject game in Sprites)
            game.layer = shown_layer;
        Animator.enabled = true;
        Animator.SetTrigger("Show");
    }

    public void End()
    {
        ShowEnd?.Invoke(this);
        Animator.enabled = false;
        foreach (GameObject game in Sprites)
            game.layer = hide_layer;
    }
}
