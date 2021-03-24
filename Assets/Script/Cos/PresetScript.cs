using UnityEngine;
using UnityEngine.UI;

public delegate void PresetClickedDelegate(int index);
public class PresetScript : MonoBehaviour
{
    public static event PresetClickedDelegate PresetClicked;

    public int Index;
    public Text Text;
    public Animator Animator;

    private bool selected;

    public bool Selected {
        get => selected;
        set
        {
            selected = value;
            Animator.SetBool("Selected", value);
        }
    }

    void Start()
    {
        Text.text = GameSetting.Translate("Preset") + (Index + 1).ToString();
    }

    public void OnClick()
    {
        PresetClicked?.Invoke(Index);
    }
}
