using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PresetManageScript : MonoBehaviour
{
    public PresetScript[] Buttons;
    public Text Title;
    public Text[] Contents;
    public Text[] FunButtons;
    public Button LoadButton;

    public CosChangeScript Cos;

    private int index;
    private bool input;
    void Start()
    {
        PresetScript.PresetClicked += Switch;
        string title = GameSetting.Translate("Preset Manage");
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < title.Length; i++)
        {
            builder.Append(title[i].ToString());
            if (i != title.Length - 1)
                builder.Append("\n");
        }
        Title.text = builder.ToString();

        FunButtons[0].text = GameSetting.Translate("OpenMenu");
        FunButtons[1].text = GameSetting.Translate("Load");
        FunButtons[2].text = GameSetting.Translate("Save");
        FunButtons[3].text = GameSetting.Translate("press option button to save current set, and press enter to load preset");
    }


    private void OnDestroy()
    {
        PresetScript.PresetClicked -= Switch;
    }

    public void Quit()
    {
        if (input && Cos.Preset_open)
        {
            OnShow();
        }
    }

    public void OnShow()
    {
        if (!Cos.Preset_open)
        {
            FunButtons[0].text = GameSetting.Translate("Fold");

            index = 0;
            List<string> sets = new List<string>();
            if (GameSetting.Config.Presets != null && GameSetting.Config.Presets.ContainsKey(index))
                sets = GameSetting.Config.Presets[index];

            for (int i = 0; i < Buttons.Length; i++)
                Buttons[i].Selected = Buttons[i].Index == index;

            for (int i = 0; i < Contents.Length; i++)
            {
                if (sets.Count > i)
                {
                    if (string.IsNullOrEmpty(sets[i]))
                    {
                        Contents[i].text = "——";
                    }
                    else
                    {
                        switch (i)
                        {
                            case 0:
                                Contents[i].text = GameSetting.GetSkinName(GameSetting.SkinPosition.TypeHead, sets[i]);
                                break;
                            case 1:
                                Contents[i].text = GameSetting.GetSkinName(GameSetting.SkinPosition.TypeBody, sets[i]);
                                break;
                            case 2:
                                Contents[i].text = GameSetting.GetSkinName(GameSetting.SkinPosition.TypeEmotion, sets[i]);
                                break;
                            case 3:
                                Contents[i].text = GameSetting.GetSkinName(GameSetting.SkinPosition.TypeCos, sets[i]);
                                break;
                            case 4:
                                Contents[i].text = GameSetting.GetSkinName(GameSetting.SkinPosition.TypeSub, sets[i]);
                                break;
                            case 5:
                                Contents[i].text = GameSetting.GetSkinName(GameSetting.SkinPosition.TypeFace, sets[i]);
                                break;
                            case 6:
                                Contents[i].text = GameSetting.GetSkinName(GameSetting.SkinPosition.TypeBodySkin, sets[i]);
                                break;
                            case 7:
                                Contents[i].text = GameSetting.GetSkinName(GameSetting.SkinPosition.TypeSkin, sets[i]);
                                break;
                            case 8:
                                Contents[i].text = GameSetting.GetSkinName(GameSetting.SkinPosition.TypeTatoo, sets[i]);
                                break;
                        }
                    }
                }
                else
                    Contents[i].text = "——";
            }

            Cos.OpenPreset();
        }
        else if (input)
        {
            input = false;
            Cos.ClosePreset();
            FunButtons[0].text = GameSetting.Translate("OpenMenu");
        }
    }

    public void SetInput(bool input)
    {
        this.input = input;
    }

    public void Move(bool up)
    {
        int i = index;
        if (up)
        {
            i--;
            if (i < 0) i = Buttons.Length - 1;
        }
        else
        {
            i++;
            if (i >= Buttons.Length) i = 0;
        }
        Switch(i);
    }

    private void Switch(int index)
    {
        if (input)
        {
            EventSystem.current.SetSelectedGameObject(null);
            if (this.index != index)
            {
                Cos.PlaySound(false);
                this.index = index;
                List<string> sets = new List<string>();
                if (GameSetting.Config.Presets != null && GameSetting.Config.Presets.ContainsKey(index))
                    sets = GameSetting.Config.Presets[index];

                for (int i = 0; i < Buttons.Length; i++)
                    Buttons[i].Selected = Buttons[i].Index == index;

                for (int i = 0; i < Contents.Length; i++)
                {
                    if (sets.Count > i)
                    {
                        if (string.IsNullOrEmpty(sets[i]))
                        {
                            Contents[i].text = "——";
                        }
                        else
                        {
                            switch (i)
                            {
                                case 0:
                                    Contents[i].text = GameSetting.GetSkinName(GameSetting.SkinPosition.TypeHead, sets[i]);
                                    break;
                                case 1:
                                    Contents[i].text = GameSetting.GetSkinName(GameSetting.SkinPosition.TypeBody, sets[i]);
                                    break;
                                case 2:
                                    Contents[i].text = GameSetting.GetSkinName(GameSetting.SkinPosition.TypeEmotion, sets[i]);
                                    break;
                                case 3:
                                    Contents[i].text = GameSetting.GetSkinName(GameSetting.SkinPosition.TypeCos, sets[i]);
                                    break;
                                case 4:
                                    Contents[i].text = GameSetting.GetSkinName(GameSetting.SkinPosition.TypeSub, sets[i]);
                                    break;
                                case 5:
                                    Contents[i].text = GameSetting.GetSkinName(GameSetting.SkinPosition.TypeFace, sets[i]);
                                    break;
                                case 6:
                                    Contents[i].text = GameSetting.GetSkinName(GameSetting.SkinPosition.TypeBodySkin, sets[i]);
                                    break;
                                case 7:
                                    Contents[i].text = GameSetting.GetSkinName(GameSetting.SkinPosition.TypeSkin, sets[i]);
                                    break;
                                case 8:
                                    Contents[i].text = GameSetting.GetSkinName(GameSetting.SkinPosition.TypeTatoo, sets[i]);
                                    break;
                            }
                        }
                    }
                    else
                        Contents[i].text = "——";
                }

                LoadButton.interactable = GameSetting.Config.Presets.ContainsKey(index);
            }
        }
    }

    public void SaveSet()
    {
        if (!input) return;

        EventSystem.current.SetSelectedGameObject(null);

        List<string> sets = new List<string>();
        sets.Add(GameSetting.Config.Head);
        sets.Add(GameSetting.Config.Body);
        sets.Add(GameSetting.Config.Emotion);
        sets.Add(GameSetting.Config.SkinCos);
        sets.Add(GameSetting.Config.SkinSub);
        sets.Add(GameSetting.Config.Face);
        sets.Add(GameSetting.Config.BodySkin);
        sets.Add(GameSetting.Config.Skin);
        sets.Add(GameSetting.Config.Tatoo);

        for (int i = 0; i < Contents.Length; i++)
        {
            if (sets.Count > i)
            {
                if (string.IsNullOrEmpty(sets[i]))
                {
                    Contents[i].text = "——";
                }
                else
                {
                    switch (i)
                    {
                        case 0:
                            Contents[i].text = GameSetting.GetSkinName(GameSetting.SkinPosition.TypeHead, sets[i]);
                            break;
                        case 1:
                            Contents[i].text = GameSetting.GetSkinName(GameSetting.SkinPosition.TypeBody, sets[i]);
                            break;
                        case 2:
                            Contents[i].text = GameSetting.GetSkinName(GameSetting.SkinPosition.TypeEmotion, sets[i]);
                            break;
                        case 3:
                            Contents[i].text = GameSetting.GetSkinName(GameSetting.SkinPosition.TypeCos, sets[i]);
                            break;
                        case 4:
                            Contents[i].text = GameSetting.GetSkinName(GameSetting.SkinPosition.TypeSub, sets[i]);
                            break;
                        case 5:
                            Contents[i].text = GameSetting.GetSkinName(GameSetting.SkinPosition.TypeFace, sets[i]);
                            break;
                        case 6:
                            Contents[i].text = GameSetting.GetSkinName(GameSetting.SkinPosition.TypeBodySkin, sets[i]);
                            break;
                        case 7:
                            Contents[i].text = GameSetting.GetSkinName(GameSetting.SkinPosition.TypeSkin, sets[i]);
                            break;
                        case 8:
                            Contents[i].text = GameSetting.GetSkinName(GameSetting.SkinPosition.TypeTatoo, sets[i]);
                            break;
                    }
                }
            }
            else
                Contents[i].text = "——";
        }

        Dictionary<int, List<string>> preset = GameSetting.Config.Presets;
        preset[index] = sets;
        GameSetting.SaveConfig();
        Cos.PlaySound(true);
    }

    public void LoadSet()
    {
        if (!input) return;

        EventSystem.current.SetSelectedGameObject(null);
        if (GameSetting.Config.Presets != null && GameSetting.Config.Presets.ContainsKey(index))
        {
            List<string> sets = GameSetting.Config.Presets[index];

            GameSetting.Config.Head = sets.Count > 0 ? sets[0] : string.Empty;
            GameSetting.Config.Body = sets.Count > 1 ? sets[1] : string.Empty;
            GameSetting.Config.Emotion = sets.Count > 2 ? sets[2] : string.Empty;
            GameSetting.Config.SkinCos = sets.Count > 4 ? sets[3] : string.Empty;
            GameSetting.Config.SkinSub = sets.Count > 3 ? sets[4] : string.Empty;
            GameSetting.Config.Face = sets.Count > 5 ? sets[5] : string.Empty;
            GameSetting.Config.BodySkin = sets.Count > 6 ? sets[6] : string.Empty;
            GameSetting.Config.Skin = sets.Count > 7 ? sets[7] : string.Empty;
            GameSetting.Config.Tatoo = sets.Count > 8 ? sets[8] : string.Empty;
            GameSetting.SaveConfig();

            //cos refresh
            Cos.Refresh();
        }
        Cos.PlaySound(true);
    }
}
