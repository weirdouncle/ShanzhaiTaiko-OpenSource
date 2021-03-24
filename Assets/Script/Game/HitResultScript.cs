using CommonClass;
using UnityEngine;
using UnityEngine.UI;

public class HitResultScript : MonoBehaviour
{
    public Text Time;
    public GameObject NoteDon;
    public GameObject NoteKa;
    public HitNoteResult State;
    public Image Image;

    public void SetResult(bool don, string message)
    {
        NoteDon.SetActive(don);
        NoteKa.SetActive(!don);
        Time.text = message;
        gameObject.SetActive(true);
    }

    public void Init()
    {
        switch (State)
        {
            case HitNoteResult.Perfect:
                Image.sprite = SettingLoader.NoteResults[0];
                break;
            case HitNoteResult.Good:
                Image.sprite = SettingLoader.NoteResults[1];
                break;
            case HitNoteResult.Bad:
                Image.sprite = SettingLoader.NoteResults[2];
                break;
        }


        RectTransform rect = Image.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(Image.sprite.bounds.size.x * 100, Image.sprite.bounds.size.y * 100);
        Image.transform.localPosition -= new Vector3(Image.sprite.bounds.size.x / 2 * 50, 0);
    }
}
