using CommonClass;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkinScrollScript : MonoBehaviour
{
    public GameSetting.SkinPosition Position;
    public Scrollbar Bar;
    public Transform Panel;
    public GameObject Button_pre;
    public int selected_index;
    public CosChangeScript Cos;

    private List<SkinButtonScript> buttons = new List<SkinButtonScript>();
    private const float height = 80;
    private WaitForEndOfFrame wait = new WaitForEndOfFrame();

    public void SetOptions(List<SkinInfo> options, int index)
    {
        RectTransform rect = Panel.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, height * options.Count);

        StartCoroutine(Arrenge(options, index));
    }

    IEnumerator Arrenge(List<SkinInfo> options, int index)
    {
        yield return wait;

        int x = 0;
        for (int i = 0; i < options.Count; i++)
        {
            GameObject button = Instantiate(Button_pre, Panel);
            SkinButtonScript script = button.GetComponent<SkinButtonScript>();
            script.SetText(i, options[i].DisplayName, Position);
            button.transform.localPosition = new Vector3(0, options.Count * height / 2 - height / 2 - i * height);
            buttons.Add(script);
            x++;

            if (Position != GameSetting.SkinPosition.TypeHead && x == 5)
            {
                yield return wait;
                x = 0;
            }
        }

        SetFocus(index);
        SetSelected(index);

        if (Position != GameSetting.SkinPosition.TypeHead)
        {
            gameObject.SetActive(false);
            transform.localPosition = new Vector3(0, 0);
        }
        Cos.InitEnd();
    }

    public void SetOptions(List<string> options, int index)
    {
        RectTransform rect = Panel.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, height * options.Count);

        StartCoroutine(Arrenge(options, index));
    }

    IEnumerator Arrenge(List<string> options, int index)
    {
        yield return wait;

        int x = 0;
        for (int i = 0; i < options.Count; i++)
        {
            GameObject button = Instantiate(Button_pre, Panel);
            SkinButtonScript script = button.GetComponent<SkinButtonScript>();
            script.SetText(i, options[i], Position);
            button.transform.localPosition = new Vector3(0, options.Count * height / 2 - height / 2 - i * height);
            buttons.Add(script);
            x++;

            if (Position != GameSetting.SkinPosition.TypeHead && x == 5)
            {
                yield return wait;
                x = 0;
            }
        }

        SetFocus(index);
        SetSelected(index);

        if (Position != GameSetting.SkinPosition.TypeHead)
        {
            gameObject.SetActive(false);
            transform.localPosition = new Vector3(0, 0);
        }
        Cos.InitEnd();
    }

    public void ResetValue()
    {
        SetFocus(selected_index);
    }

    public void SetSelected(int index)
    {
        selected_index = index;
        foreach (SkinButtonScript button in buttons)
            button.Selected = button.Index == index;
    }

    public void SetFocus(int index, bool assign = true)
    {
        foreach (SkinButtonScript button in buttons)
            button.Focus = button.Index == index;

        if (assign)
        {
            float half = (float)buttons.Count / 2;
            int position = index < half ? index : System.Math.Abs(buttons.Count - 1 - index);

            float value = position < 6 ? 0 : (float)(position + 1) / buttons.Count;
            float pow = Mathf.Max(1, 1.4f - ((position + 1) / half));
            if (value != 0) value = Mathf.Pow(value, pow);

            Bar.value = index < half ? 1 - value : value;
        }
    }
}
