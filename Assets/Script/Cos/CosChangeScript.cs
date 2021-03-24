using CommonClass;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputAction;

public delegate void ResetCosDelegate();
public delegate void SetCosDelegate(GameSetting.SkinPosition position);
public class CosChangeScript : MonoBehaviour
{
    public static event PlaySelectedSoundDelegate PlaySelectedSound;

    public event SetCosDelegate SetCos;

    public event ResetCosDelegate ResetCos;

    public LabelButtonScript[] Buttons;
    public SkinScrollScript[] Scrolls;
    public Text Title;
    public Text Numbers;
    public GameObject Arrows;
    public UnityEngine.UI.Button QuitButton;
    public PresetManageScript Preset;
    public Animator Animator;

    public GameObject Emotion { set; get; }
    public GameObject Head { set; get; }
    public GameObject Body { set; get; }
    public Material BodySkin { set; get; }
    public GameObject Cos { set; get; }
    public GameObject Sub { set; get; }
    public Material Face { set; get; }
    public Material Skin { set; get; }
    public Material Tatoo { set; get; }
    public string EmotionName { set; get; }
    public string HeadName { set; get; }
    public string BodyName { set; get; }
    public string BodySkinName { set; get; }
    public string CosName { set; get; }
    public string SubName { set; get; }
    public string FaceName { set; get; }
    public string SkinName { set; get; }
    public string TatooName { set; get; }
    public bool Preset_open { set; get; }

    private Dictionary<int, GameSetting.SkinPosition> keys = new Dictionary<int, GameSetting.SkinPosition>();
    private int inits = 0;
    private int index = 0;

    private Dictionary<int, SkinInfo> emotion_name = new Dictionary<int, SkinInfo> { { 0, new SkinInfo(0, string.Empty, GameSetting.Translate("default"), null, false) } };
    private int emotion_index = 0;
    
    private Dictionary<int, SkinInfo> head_name = new Dictionary<int, SkinInfo> { { 0, new SkinInfo(0, string.Empty, GameSetting.Translate("none"), null, false) } };
    private int head_index = 0;

    private Dictionary<int, SkinInfo> bodies_name = new Dictionary<int, SkinInfo> { { 0, new SkinInfo(0, string.Empty, GameSetting.Translate("default"), null, false) } };
    private int body_index = 0;

    private Dictionary<int, string> body_skins_name = new Dictionary<int, string> { { 0, GameSetting.Translate("default") } };
    private Dictionary<int, Material> body_skins = new Dictionary<int, Material> { { 0, null } };
    private int body_skin_index = 0;

    private Dictionary<int, string> body_tatoo_name = new Dictionary<int, string> { { 0, GameSetting.Translate("none") } };
    private Dictionary<int, Material> body_tatoos = new Dictionary<int, Material> { { 0, null } };
    private int body_tatoo_index = 0;

    private Dictionary<int, SkinInfo> cos_name = new Dictionary<int, SkinInfo> { { 0, new SkinInfo(0, string.Empty, GameSetting.Translate("none"), null, false) } };
    private int cos_index = 0;

    private Dictionary<int, SkinInfo> subs_name = new Dictionary<int, SkinInfo> { { 0, new SkinInfo(0, string.Empty, GameSetting.Translate("none"), null, false) } };
    private int sub_index = 0;

    private Dictionary<int, string> face_name = new Dictionary<int, string> { { 0, GameSetting.Translate("default") } };
    private Dictionary<int, Material> faces = new Dictionary<int, Material> { { 0, null } };
    private int face_index = 0;

    private Dictionary<int, string> skins_name = new Dictionary<int, string> { { 0, GameSetting.Translate("default") } };
    private Dictionary<int, Material> skins = new Dictionary<int, Material> { { 0, null } };
    private int skin_index = 0;

    private bool quit;
    private WASDInput BasicInput;
    void Start()
    {
#if !UNITY_ANDROID
        QuitButton.gameObject.SetActive(false);
#endif

        for (int x = 0; x < Buttons.Length; x++)
        {
            keys.Add(x, Buttons[x].Position);
            Buttons[x].LabelClick += OnSelect;
        }

        Emotion = SettingLoader.Emotion;
        Head = SettingLoader.Head;
        Body = SettingLoader.Body;
        BodySkin = SettingLoader.BodySkin;
        Cos = SettingLoader.Cos;
        Sub = SettingLoader.Sub;
        Face = SettingLoader.Face;
        Skin = SettingLoader.Skin;
        Tatoo = SettingLoader.Tatoo;

        EmotionName = GameSetting.Config.Emotion;
        HeadName = GameSetting.Config.Head;
        BodyName = GameSetting.Config.Body;
        BodySkinName = GameSetting.Config.BodySkin;
        CosName = GameSetting.Config.SkinCos;
        SubName = GameSetting.Config.SkinSub;
        FaceName = GameSetting.Config.Face;
        SkinName = GameSetting.Config.Skin;
        TatooName = GameSetting.Config.Tatoo;

        List<RewardSkin> heads = new List<RewardSkin>();
        List<RewardSkin> bodies = new List<RewardSkin>();
        List<RewardSkin> coses = new List<RewardSkin>();
        List<RewardSkin> subs = new List<RewardSkin>();

        //读取系统奖励皮肤
        foreach (int id in GameSetting.Record.Rewards)
        {
            if (SettingLoader.RewardSkins.TryGetValue(id, out RewardSkin skin_data))
            {
                switch (skin_data.Position)
                {
                    case GameSetting.SkinPosition.TypeHead:
                        heads.Add(skin_data);
                        break;
                    case GameSetting.SkinPosition.TypeBody:
                        bodies.Add(skin_data);
                        break;
                    case GameSetting.SkinPosition.TypeCos:
                        coses.Add(skin_data);
                        break;
                    case GameSetting.SkinPosition.TypeSub:
                        subs.Add(skin_data);
                        break;
                }
            }
        }

        //读取头部
        int i = 1;
        foreach (DataRow row in GameSetting.Skin.Tables["Head"].Rows)
        {
            string part_name = row["Name"].ToString();
            GameObject part = LoadPart(GameSetting.SkinPosition.TypeHead, part_name);
            if (part != null)
            {
                head_name[i] = new SkinInfo(i, part_name, GameSetting.GetSkinName(GameSetting.SkinPosition.TypeHead, part_name), part, false);
                i++;
            }
        }

        foreach (RewardSkin skin in heads)
        {
            head_name.Add(i, new SkinInfo(skin.SkinId, skin.SkinId.ToString(), GameSetting.GetSkinName(GameSetting.SkinPosition.TypeHead, skin.Name), LoadReward(skin.Path), true, skin.Remark));
            i++;
        }

        for (int n = 0; n < head_name.Keys.Count; n++)
        {
            if (GameSetting.Config.Head == (head_name[n].Reward ? head_name[n].Id.ToString() : head_name[n].Name))
            {
                head_index = n;
                break;
            }
        }

        Scrolls[0].SetOptions(new List<SkinInfo>(head_name.Values), head_index);

        //读取身体
        i = 1;
        foreach (DataRow row in GameSetting.Skin.Tables["Body"].Rows)
        {
            string part_name = row["Name"].ToString();
            GameObject part = LoadPart(GameSetting.SkinPosition.TypeBody, part_name);
            if (part != null)
            {
                bodies_name[i] = new SkinInfo(i, part_name, GameSetting.GetSkinName(GameSetting.SkinPosition.TypeBody, part_name), part, false);
                i++;
            }
        }

        foreach (RewardSkin skin in bodies)
        {
            head_name.Add(i, new SkinInfo(skin.SkinId, skin.SkinId.ToString(), GameSetting.GetSkinName(GameSetting.SkinPosition.TypeBody, skin.Name), LoadReward(skin.Path), true, skin.Remark));
            i++;
        }

        for (int n = 0; n < bodies_name.Keys.Count; n++)
        {
            if (GameSetting.Config.Body == (bodies_name[n].Reward ? bodies_name[n].Id.ToString() : bodies_name[n].Name))
            {
                body_index = n;
                break;
            }
        }

        Scrolls[1].SetOptions(new List<SkinInfo>(bodies_name.Values), body_index);

        //读取表情
        i = 1;
        foreach (DataRow row in GameSetting.Skin.Tables["Emotion"].Rows)
        {
            string part_name = row["Name"].ToString();
            GameObject part = LoadPart(GameSetting.SkinPosition.TypeEmotion, part_name);
            if (part != null)
            {
                if (GameSetting.Config.Emotion == part_name)
                    emotion_index = i;
                emotion_name[i] = new SkinInfo(i, part_name, GameSetting.GetSkinName(GameSetting.SkinPosition.TypeEmotion, part_name), part, false);
                i++;
            }
        }

        Scrolls[2].SetOptions(new List<SkinInfo>(emotion_name.Values), emotion_index);

        //读取全身
        i = 1;
        foreach (DataRow row in GameSetting.Skin.Tables["Cos"].Rows)
        {
            string part_name = row["Name"].ToString();
            GameObject part = LoadPart(GameSetting.SkinPosition.TypeCos, part_name);
            if (part != null)
            {
                cos_name[i] = new SkinInfo(i, part_name, GameSetting.GetSkinName(GameSetting.SkinPosition.TypeCos, part_name), part, false);
                i++;
            }
        }

        foreach (RewardSkin skin in coses)
        {
            cos_name.Add(i, new SkinInfo(skin.SkinId, skin.SkinId.ToString(), GameSetting.GetSkinName(GameSetting.SkinPosition.TypeCos, skin.Name), LoadReward(skin.Path), true, skin.Remark));
            i++;
        }

        for (int n = 0; n < cos_name.Keys.Count; n++)
        {
            if (GameSetting.Config.SkinCos == (cos_name[n].Reward ? cos_name[n].Id.ToString() : cos_name[n].Name))
            {
                cos_index = n;
                break;
            }
        }

        Scrolls[3].SetOptions(new List<SkinInfo>(cos_name.Values), cos_index);

        //读取挂件
        i = 1;
        foreach (DataRow row in GameSetting.Skin.Tables["Sub"].Rows)
        {
            string part_name = row["Name"].ToString();
            GameObject part = LoadPart(GameSetting.SkinPosition.TypeSub, part_name);
            if (part != null)
            {
                subs_name[i] = new SkinInfo(i, part_name, GameSetting.GetSkinName(GameSetting.SkinPosition.TypeSub, part_name), part, false);
                i++;
            }
        }

        foreach (RewardSkin skin in subs)
        {
            head_name.Add(i, new SkinInfo(skin.SkinId, skin.SkinId.ToString(), GameSetting.GetSkinName(GameSetting.SkinPosition.TypeSub, skin.Name), LoadReward(skin.Path), true, skin.Remark));
            i++;
        }

        for (int n = 0; n < subs_name.Keys.Count; n++)
        {
            if (GameSetting.Config.SkinSub == (subs_name[n].Reward ? subs_name[n].Id.ToString() : subs_name[n].Name))
            {
                sub_index = n;
                break;
            }
        }

        Scrolls[4].SetOptions(new List<SkinInfo>(subs_name.Values), sub_index);

        //读取面部
        i = 1;
        foreach (DataRow row in GameSetting.Skin.Tables["Face"].Rows)
        {
            string part_name = row["Name"].ToString();
            Material part = LoadMaterial(GameSetting.SkinPosition.TypeFace, part_name);
            if (part != null)
            {
                if (GameSetting.Config.Face == part_name)
                    face_index = i;
                face_name[i] = part_name;
                faces[i] = part;
                i++;
            }
        }
        List<string> face_tanslation = new List<string>();
        foreach (string part_name in face_name.Values)
            face_tanslation.Add(GameSetting.GetSkinName(GameSetting.SkinPosition.TypeFace, part_name));
        Scrolls[5].SetOptions(face_tanslation, face_index);

        //读取皮肤
        i = 1;
        foreach (DataRow row in GameSetting.Skin.Tables["SkinColor"].Rows)
        {
            string part_name = row["Name"].ToString();
            Material part = LoadMaterial(GameSetting.SkinPosition.TypeSkin, part_name);
            if (part != null)
            {
                if (GameSetting.Config.Skin == part_name)
                    skin_index = i;
                skins_name[i] = part_name;
                skins[i] = part;
                i++;
            }
        }
        List<string> skin_tanslation = new List<string>();
        foreach (string part_name in skins_name.Values)
            skin_tanslation.Add(GameSetting.GetSkinName(GameSetting.SkinPosition.TypeSkin, part_name));
        Scrolls[6].SetOptions(skin_tanslation, skin_index);

        //读取鼓皮
        i = 1;
        foreach (DataRow row in GameSetting.Skin.Tables["BodyColor"].Rows)
        {
            string part_name = row["Name"].ToString();
            Material part = LoadMaterial(GameSetting.SkinPosition.TypeBodySkin, part_name);
            if (part != null)
            {
                if (GameSetting.Config.BodySkin == part_name)
                    body_skin_index = i;
                body_skins_name[i] = part_name;
                body_skins[i] = part;
                i++;

            }
        }
        List<string> body_tanslation = new List<string>();
        foreach (string part_name in body_skins_name.Values)
            body_tanslation.Add(GameSetting.GetSkinName(GameSetting.SkinPosition.TypeBodySkin, part_name));
        Scrolls[7].SetOptions(body_tanslation, body_skin_index);

        //读取鼓皮纹身
        i = 1;
        foreach (DataRow row in GameSetting.Skin.Tables["Tatoo"].Rows)
        {
            string part_name = row["Name"].ToString();
            Material part = LoadMaterial(GameSetting.SkinPosition.TypeTatoo, part_name);
            if (part != null)
            {
                if (GameSetting.Config.Tatoo == part_name)
                    body_tatoo_index = i;
                body_tatoo_name[i] = part_name;
                body_tatoos[i] = part;
                i++;

            }
        }
        List<string> tatoo_tanslation = new List<string>();
        foreach (string part_name in body_tatoo_name.Values)
            tatoo_tanslation.Add(GameSetting.GetSkinName(GameSetting.SkinPosition.TypeTatoo, part_name));
        Scrolls[8].SetOptions(tatoo_tanslation, body_tatoo_index);

        ResetCos?.Invoke();
        foreach (LabelButtonScript label in Buttons)
        {
            bool selected = label.Position == keys[index];
            label.Selected = selected;
        }
        Title.text = GameSetting.Translate("Head");
        Numbers.text = (head_name.Keys.Count - 1).ToString();
        Arrows.SetActive(head_name.Count > 8);

        BasicInput = BasicInputScript.Input;
        BasicInput.Player.Up.performed += ChangeBack;
        BasicInput.Player.Down.performed += ChangeForward;
        BasicInput.Player.Left.performed += MoveDown;
        BasicInput.Player.Right.performed += MoveDown;

        BasicInput.Player.LeftKa.performed += MoveDown;
        BasicInput.Player.RightKa.performed += MoveDown;

        BasicInput.Player.Esc.performed += QuitCos;
        BasicInput.Player.Cancel.performed += QuitCos;

        BasicInput.Player.Option.performed += OptionMove;

        BasicInput.Player.Enter.performed += Confirm;
        BasicInput.Player.RightDon.performed += Confirm;

        BasicInput.Player.F1.performed += OpenPreset;

        if (GameSetting.Config.DirectInput)
            BasicInputScript.KeyInvoke += DirectInput;

        SkinButtonScript.SkinConfirm += Confirm;
        SkinButtonScript.SkinClick += OnChange;
    }

    private void OnDestroy()
    {
        for (int x = 0; x < Buttons.Length; x++)
        {
            Buttons[x].LabelClick -= OnSelect;
        }

        BasicInputScript.KeyInvoke -= DirectInput;

        SkinButtonScript.SkinConfirm -= Confirm;
        SkinButtonScript.SkinClick -= OnChange;
    }

    private GameObject LoadPart(GameSetting.SkinPosition position, string part_name)
    {
        return SettingLoader.LoadPart(position, part_name);
    }

    private GameObject LoadReward(string path)
    {
        return Resources.Load<GameObject>(path);
    }

    private Material LoadMaterial(GameSetting.SkinPosition position, string part_name)
    {
        return SettingLoader.LoadMaterial(position, part_name);
    }

    public void InitEnd()
    {
        inits++;
    }

    private void Arrenge()
    {
        foreach (LabelButtonScript label in Buttons)
        {
            bool selected = label.Position == keys[index];
            label.Selected = selected;

            if (selected)
            {
                switch (label.Position)
                {
                    case GameSetting.SkinPosition.TypeHead:
                        Title.text = GameSetting.Translate("Head");
                        Numbers.text = (head_name.Keys.Count - 1).ToString();
                        head_index = Scrolls[index].selected_index;
                        Arrows.SetActive(head_name.Keys.Count > 8);
                        break;
                    case GameSetting.SkinPosition.TypeBody:
                        Title.text = GameSetting.Translate("Body");
                        Numbers.text = (bodies_name.Keys.Count - 1).ToString();
                        body_index = Scrolls[index].selected_index;
                        Arrows.SetActive(bodies_name.Keys.Count > 8);
                        break;
                    case GameSetting.SkinPosition.TypeEmotion:
                        Title.text = GameSetting.Translate("Emotion");
                        Numbers.text = (emotion_name.Keys.Count - 1).ToString();
                        emotion_index = Scrolls[index].selected_index;
                        Arrows.SetActive(emotion_name.Keys.Count > 8);
                        break;
                    case GameSetting.SkinPosition.TypeCos:
                        Title.text = GameSetting.Translate("Cosplay");
                        Numbers.text = (cos_name.Keys.Count - 1).ToString();
                        cos_index = Scrolls[index].selected_index;
                        Arrows.SetActive(cos_name.Keys.Count > 8);
                        break;
                    case GameSetting.SkinPosition.TypeSub:
                        Title.text = GameSetting.Translate("Sub");
                        Numbers.text = (subs_name.Keys.Count - 1).ToString();
                        sub_index = Scrolls[index].selected_index;
                        Arrows.SetActive(subs_name.Keys.Count > 8);
                        break;
                    case GameSetting.SkinPosition.TypeFace:
                        Title.text = GameSetting.Translate("FaceColor");
                        Numbers.text = (faces.Keys.Count - 1).ToString();
                        face_index = Scrolls[index].selected_index;
                        Arrows.SetActive(faces.Keys.Count > 8);
                        break;
                    case GameSetting.SkinPosition.TypeSkin:
                        Title.text = GameSetting.Translate("BodyColor");
                        Numbers.text = (skins.Keys.Count - 1).ToString();
                        skin_index = Scrolls[index].selected_index;
                        Arrows.SetActive(skins.Keys.Count > 8);
                        break;
                    case GameSetting.SkinPosition.TypeBodySkin:
                        Title.text = GameSetting.Translate("SkinColor");
                        Numbers.text = (body_skins.Keys.Count - 1).ToString();
                        body_skin_index = Scrolls[index].selected_index;
                        Arrows.SetActive(body_skins.Keys.Count > 8);
                        break;
                    case GameSetting.SkinPosition.TypeTatoo:
                        Title.text = GameSetting.Translate("Tatoo");
                        Numbers.text = (body_tatoos.Keys.Count - 1).ToString();
                        body_tatoo_index = Scrolls[index].selected_index;
                        Arrows.SetActive(body_tatoos.Keys.Count > 8);
                        break;
                }
            }
        }

        foreach (SkinScrollScript scroll in Scrolls)
        {
            bool enable = scroll.Position == keys[index];
            scroll.gameObject.SetActive(enable);
            if (enable) scroll.ResetValue();
        }
    }

    private void ChangeForward(CallbackContext context)
    {
        if (!Application.isFocused || quit) return;
        EventSystem.current.SetSelectedGameObject(null);
        if (!Preset_open)
            SettingChange(true);
        else
            Preset.Move(false);
    }
    private void ChangeBack(CallbackContext context)
    {
        if (!Application.isFocused || quit) return;
        EventSystem.current.SetSelectedGameObject(null);

        if (!Preset_open)
            SettingChange(false);
        else
            Preset.Move(true);
    }

    private void Confirm(CallbackContext context)
    {
        if (!Preset_open)
            Confirm();
        else
            Preset.LoadSet();
    }

    private void Confirm()
    {
        switch (keys[index])
        {
            case GameSetting.SkinPosition.TypeHead:
                {
                    if (SettingLoader.Head != Head)
                    {
                        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Don);
                        string part_name = head_index == 0 ? string.Empty : head_name[head_index].Name;
                        if (head_name[head_index].Reward) part_name = head_name[head_index].Id.ToString();
                        SettingLoader.Head = head_name[head_index].Prefab;
                        GameSetting.SetPart(GameSetting.SkinPosition.TypeHead, part_name);
                        Scrolls[index].SetSelected(head_index);

                        if (head_index > 0 && SettingLoader.Cos != null && GameSetting.MaskPosition(GameSetting.Config.SkinCos, GameSetting.SkinPosition.TypeHead, true))
                        {
                            SettingLoader.Cos = Cos = null;
                            GameSetting.SetPart(GameSetting.SkinPosition.TypeCos, string.Empty);

                            cos_index = 0;
                            Scrolls[3].SetFocus(0);
                            Scrolls[3].SetSelected(0);
                        }
                    }
                }
                break;
            case GameSetting.SkinPosition.TypeBody:
                {
                    if (SettingLoader.Body != Body)
                    {
                        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Don);
                        SettingLoader.Body = bodies_name[body_index].Prefab;
                        string part_name = body_index == 0 ? string.Empty : bodies_name[body_index].Name;
                        if (bodies_name[body_index].Reward) part_name = bodies_name[body_index].Id.ToString();
                        GameSetting.SetPart(GameSetting.SkinPosition.TypeBody, part_name);
                        Scrolls[index].SetSelected(body_index);

                        if (body_index > 0 && SettingLoader.Cos != null)
                        {
                            SettingLoader.Cos = Cos = null;
                            GameSetting.SetPart(GameSetting.SkinPosition.TypeCos, string.Empty);

                            cos_index = 0;
                            Scrolls[3].SetFocus(0);
                            Scrolls[3].SetSelected(0);
                        }
                    }
                }
                break;
            case GameSetting.SkinPosition.TypeEmotion:
                {
                    if (SettingLoader.Emotion != Emotion)
                    {
                        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Don);
                        SettingLoader.Emotion = emotion_name[emotion_index].Prefab;
                        string part_name = emotion_index == 0 ? string.Empty : emotion_name[emotion_index].Name;
                        if (emotion_name[emotion_index].Reward) part_name = emotion_name[emotion_index].Id.ToString();
                        GameSetting.SetPart(GameSetting.SkinPosition.TypeEmotion, part_name);
                        Scrolls[index].SetSelected(emotion_index);
                    }
                }
                break;
            case GameSetting.SkinPosition.TypeCos:
                {
                    if (SettingLoader.Cos != Cos)
                    {
                        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Don);
                        SettingLoader.Cos = cos_name[cos_index].Prefab;
                        string part_name = cos_index == 0 ? string.Empty : cos_name[cos_index].Name;
                        if (cos_name[cos_index].Reward) part_name = cos_name[cos_index].Id.ToString();
                        GameSetting.SetPart(GameSetting.SkinPosition.TypeCos, part_name);
                        Scrolls[index].SetSelected(cos_index);

                        if (cos_index > 0 && SettingLoader.Body != null)
                        {
                            SettingLoader.Body = Body = null;
                            GameSetting.SetPart(GameSetting.SkinPosition.TypeBody, string.Empty);

                            body_index = 0;
                            Scrolls[1].SetFocus(0);
                            Scrolls[1].SetSelected(0);
                        }
                    }
                }
                break;
            case GameSetting.SkinPosition.TypeSub:
                {
                    if (SettingLoader.Sub != Sub)
                    {
                        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Don);
                        SettingLoader.Sub = subs_name[sub_index].Prefab;
                        string part_name = sub_index == 0 ? string.Empty : subs_name[sub_index].Name;
                        if (subs_name[sub_index].Reward) part_name = subs_name[sub_index].Id.ToString();
                        GameSetting.SetPart(GameSetting.SkinPosition.TypeSub, part_name);
                        Scrolls[index].SetSelected(sub_index);
                    }
                }
                break;
            case GameSetting.SkinPosition.TypeFace:
                {
                    if (SettingLoader.Face != Face)
                    {
                        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Don);
                        SettingLoader.Face = faces[face_index];
                        string part_name = face_index == 0 ? string.Empty : face_name[face_index];
                        GameSetting.SetPart(GameSetting.SkinPosition.TypeFace, part_name);
                        Scrolls[index].SetSelected(face_index);
                    }
                }
                break;
            case GameSetting.SkinPosition.TypeSkin:
                {
                    if (SettingLoader.Skin != Skin)
                    {
                        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Don);
                        SettingLoader.Skin = skins[skin_index];
                        string part_name = skin_index == 0 ? string.Empty : skins_name[skin_index];
                        GameSetting.SetPart(GameSetting.SkinPosition.TypeSkin, part_name);
                        Scrolls[index].SetSelected(skin_index);
                    }
                }
                break;
            case GameSetting.SkinPosition.TypeBodySkin:
                {
                    if (SettingLoader.BodySkin != BodySkin)
                    {
                        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Don);
                        SettingLoader.BodySkin = body_skins[body_skin_index];
                        string part_name = body_skin_index == 0 ? string.Empty : body_skins_name[body_skin_index];
                        GameSetting.SetPart(GameSetting.SkinPosition.TypeBodySkin, part_name);
                        Scrolls[index].SetSelected(body_skin_index);
                    }
                }

                break;
            case GameSetting.SkinPosition.TypeTatoo:
                {
                    if (SettingLoader.Tatoo != Tatoo)
                    {
                        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Don);
                        SettingLoader.Tatoo = body_tatoos[body_tatoo_index];
                        string part_name = body_tatoo_index == 0 ? string.Empty : body_tatoo_name[body_tatoo_index];
                        GameSetting.SetPart(GameSetting.SkinPosition.TypeTatoo, part_name);
                        Scrolls[index].SetSelected(body_tatoo_index);
                    }
                }
                break;
        }
    }

    private void MoveDown(CallbackContext context)
    {
        if (inits < Buttons.Length || !Application.isFocused || quit || Preset_open) return;
        if (context.action == BasicInput.Player.Left || context.action == BasicInput.Player.LeftKa)
            Move(true);
        else
            Move(false);
    }

    private void OptionMove(CallbackContext context)
    {
        if (inits < Buttons.Length || !Application.isFocused || quit) return;

        if (!Preset_open)
        {
            Move(false);
        }
        else
        {
            Preset.SaveSet();
        }
    }

    private void Move(bool left)
    {
        if (quit || Preset_open) return;
        if (left)
        {
            int index = this.index - 1;
            if (index < 0) index = Buttons.Length - 1;
            OnSelect(keys[index]);
        }
        else
        {
            int index = this.index + 1;
            if (index >= Buttons.Length) index = 0;
            OnSelect(keys[index]);
        }
    }

    void QuitCos(CallbackContext context)
    {
        if (!Preset_open)
            QuitCos();
        else
            Preset.Quit();
    }

    public void QuitCos()
    {
        if (!Application.isFocused || quit) return;
        StartCoroutine(LoadMain());
    }

    IEnumerator LoadMain()
    {
        quit = true;
        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Cancel);
        yield return new WaitForSeconds(0.3f);

        string scene = "MainScreen";
#if UNITY_ANDROID
        scene = "AndroidMainScreen";
#endif

        AsyncOperation async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scene);
        async.allowSceneActivation = true;
        yield return async;
    }

    private void DirectInput(GameSetting.KeyType key, bool pressed)
    {
        switch (key)
        {
            case GameSetting.KeyType.Option when inits >= Buttons.Length && !quit:
                {
                    if (!Preset_open)
                    {
                        Move(false);
                    }
                    else
                    {
                        Preset.SaveSet();
                    }
                }
                break;
            case GameSetting.KeyType.Confirm:
                {
                    if (!Preset_open)
                        Confirm();
                    else
                        Preset.LoadSet();
                }
                break;
            case GameSetting.KeyType.Escape:
                {
                    if (!Preset_open)
                        QuitCos();
                    else
                        Preset.Quit();
                }
                break;
            case GameSetting.KeyType.Up when !quit:
                {
                    EventSystem.current.SetSelectedGameObject(null);

                    if (!Preset_open)
                        SettingChange(false);
                    else
                        Preset.Move(true);
                }
                break;
            case GameSetting.KeyType.Down when !quit:
                {
                    EventSystem.current.SetSelectedGameObject(null);
                    if (!Preset_open)
                        SettingChange(true);
                    else
                        Preset.Move(false);
                }
                break;
            case GameSetting.KeyType.Left when !quit && pressed && inits >= Buttons.Length && !Preset_open:
                {
                    Move(true);
                }
                break;
            case GameSetting.KeyType.Right when !quit && pressed && inits >= Buttons.Length && !Preset_open:
                {
                    Move(false);
                }
                break;
            case GameSetting.KeyType.Config:
                Preset.OnShow();
                break;
        }
    }

    private void OnSelect(GameSetting.SkinPosition position)
    {
        if (inits < Buttons.Length) return;

        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
        for (int i = 0; i < Buttons.Length; i++)
        {
            bool selected = Buttons[i].Position == position;
            Buttons[i].Selected = selected;
            if (selected)
                index = i;
        }

        if (SettingLoader.Head != Head || SettingLoader.Body != Body || SettingLoader.Emotion != Emotion || SettingLoader.Cos != Cos || SettingLoader.Sub != Sub
            || SettingLoader.Face != Face || SettingLoader.Skin != Skin || SettingLoader.BodySkin != BodySkin || SettingLoader.Tatoo != Tatoo)
        {
            Emotion = SettingLoader.Emotion;
            Head = SettingLoader.Head;
            Body = SettingLoader.Body;
            BodySkin = SettingLoader.BodySkin;
            Cos = SettingLoader.Cos;
            Sub = SettingLoader.Sub;
            Face = SettingLoader.Face;
            Skin = SettingLoader.Skin;
            Tatoo = SettingLoader.Tatoo;

            EmotionName = GameSetting.Config.Emotion;
            HeadName = GameSetting.Config.Head;
            BodyName = GameSetting.Config.Body;
            BodySkinName = GameSetting.Config.BodySkin;
            CosName = GameSetting.Config.SkinCos;
            SubName = GameSetting.Config.SkinSub;
            FaceName = GameSetting.Config.Face;
            SkinName = GameSetting.Config.Skin;
            TatooName = GameSetting.Config.Tatoo;

            ResetCos?.Invoke();
        }

        Arrenge();
    }

    public void SettingChange(bool forward)
    {
        GameSetting.SkinPosition position = keys[index];

        switch (index)
        {
            case 0:
                {
                    if (head_name.Count == 1) return;
                    if (forward)
                    {
                        head_index++;
                        if (head_index >= head_name.Count)
                            head_index = 0;
                    }
                    else
                    {
                        head_index--;
                        if (head_index < 0)
                            head_index = head_name.Count - 1;
                    }
                    Head = head_name[head_index].Prefab;
                    HeadName = head_name[head_index].Name;
                    Scrolls[index].SetFocus(head_index);

                    if (head_index > 0 && GameSetting.MaskPosition(GameSetting.Config.SkinCos, GameSetting.SkinPosition.TypeHead, true))
                    {
                        Cos = null;
                        CosName = string.Empty;
                    }
                    else if (Cos != SettingLoader.Cos)
                    {
                        Cos = SettingLoader.Cos;
                        CosName = GameSetting.Config.SkinCos;
                    }
                }
                break;
            case 1:
                {
                    if (bodies_name.Count == 1) return;
                    if (forward)
                    {
                        body_index++;
                        if (body_index >= bodies_name.Count)
                            body_index = 0;
                    }
                    else
                    {
                        body_index--;
                        if (body_index < 0)
                            body_index = bodies_name.Count - 1;
                    }
                    Body = bodies_name[body_index].Prefab;
                    BodyName = bodies_name[body_index].Reward ? bodies_name[body_index].Id.ToString() : bodies_name[body_index].Name;
                    Scrolls[index].SetFocus(body_index);

                    if (body_index > 0 && Cos != null)
                    {
                        Cos = null;
                        CosName = string.Empty;
                    }
                }
                break;
            case 2:
                {
                    if (emotion_name.Count == 1) return;
                    if (forward)
                    {
                        emotion_index++;
                        if (emotion_index >= emotion_name.Count)
                            emotion_index = 0;
                    }
                    else
                    {
                        emotion_index--;
                        if (emotion_index < 0)
                            emotion_index = emotion_name.Count - 1;
                    }
                    Emotion = emotion_name[emotion_index].Prefab;
                    EmotionName = emotion_name[emotion_index].Reward ? emotion_name[emotion_index].Id.ToString() : emotion_name[emotion_index].Name;
                    Scrolls[index].SetFocus(emotion_index);
                }
                break;
            case 3:
                {
                    if (cos_name.Count == 1) return;
                    if (forward)
                    {
                        cos_index++;
                        if (cos_index >= cos_name.Count)
                            cos_index = 0;
                    }
                    else
                    {
                        cos_index--;
                        if (cos_index < 0)
                            cos_index = cos_name.Count - 1;
                    }
                    Cos = cos_name[cos_index].Prefab;
                    CosName = cos_name[cos_index].Reward ? cos_name[cos_index].Id.ToString() : cos_name[cos_index].Name;
                    Scrolls[index].SetFocus(cos_index);
                }
                break;
            case 4:
                {
                    if (subs_name.Count == 1) return;
                    if (forward)
                    {
                        sub_index++;
                        if (sub_index >= subs_name.Count)
                            sub_index = 0;
                    }
                    else
                    {
                        sub_index--;
                        if (sub_index < 0)
                            sub_index = subs_name.Count - 1;
                    }
                    Sub = subs_name[sub_index].Prefab;
                    SubName = subs_name[sub_index].Reward ? subs_name[sub_index].Id.ToString() : subs_name[sub_index].Name;
                    Scrolls[index].SetFocus(sub_index);
                }
                break;
            case 5:
                {
                    if (face_name.Count == 1) return;
                    if (forward)
                    {
                        face_index++;
                        if (face_index >= face_name.Count)
                            face_index = 0;
                    }
                    else
                    {
                        face_index--;
                        if (face_index < 0)
                            face_index = face_name.Count - 1;
                    }
                    Face = faces[face_index];
                    FaceName = face_name[face_index];
                    Scrolls[index].SetFocus(face_index);
                }
                break;
            case 6:
                {
                    if (skins_name.Count == 1) return;
                    if (forward)
                    {
                        skin_index++;
                        if (skin_index >= skins_name.Count)
                            skin_index = 0;
                    }
                    else
                    {
                        skin_index--;
                        if (skin_index < 0)
                            skin_index = skins_name.Count - 1;
                    }
                    Skin = skins[skin_index];
                    SkinName = skins_name[skin_index];
                    Scrolls[index].SetFocus(skin_index);
                }
                break;
            case 7:
                {
                    if (body_skins_name.Count == 1) return;
                    if (forward)
                    {
                        body_skin_index++;
                        if (body_skin_index >= body_skins_name.Count)
                            body_skin_index = 0;
                    }
                    else
                    {
                        body_skin_index--;
                        if (body_skin_index < 0)
                            body_skin_index = body_skins_name.Count - 1;
                    }
                    BodySkin = body_skins[body_skin_index];
                    BodySkinName = body_skins_name[body_skin_index];
                    Scrolls[index].SetFocus(body_skin_index);
                }
                break;
            case 8:
                {
                    if (body_tatoo_name.Count == 1) return;
                    if (forward)
                    {
                        body_tatoo_index++;
                        if (body_tatoo_index >= body_tatoo_name.Count)
                            body_tatoo_index = 0;
                    }
                    else
                    {
                        body_tatoo_index--;
                        if (body_tatoo_index < 0)
                            body_tatoo_index = body_tatoo_name.Count - 1;
                    }
                    Tatoo = body_tatoos[body_tatoo_index];
                    TatooName = body_tatoo_name[body_tatoo_index];
                    Scrolls[index].SetFocus(body_tatoo_index);
                }
                break;
        }

        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
        SetCos?.Invoke(position);
    }

    private void OnChange(GameSetting.SkinPosition position, int index)
    {
        switch (position)
        {
            case GameSetting.SkinPosition.TypeHead:
                {
                    head_index = index;
                    Head = head_name[head_index].Prefab;
                    HeadName = head_name[head_index].Reward ? head_name[head_index].Id.ToString() : head_name[head_index].Name;
                    Scrolls[this.index].SetFocus(head_index, false);

                    if (head_index > 0 && GameSetting.MaskPosition(GameSetting.Config.SkinCos, GameSetting.SkinPosition.TypeHead, true))
                    {
                        Cos = null;
                        CosName = string.Empty;
                    }
                    else if (Cos != SettingLoader.Cos)
                    {
                        Cos = SettingLoader.Cos;
                        CosName = GameSetting.Config.SkinCos;
                    }
                }
                break;
            case GameSetting.SkinPosition.TypeBody:
                {
                    body_index = index;
                    Body = bodies_name[body_index].Prefab;
                    BodyName = bodies_name[body_index].Reward ? bodies_name[body_index].Id.ToString() : bodies_name[body_index].Name;
                    Scrolls[this.index].SetFocus(body_index, false);

                    if (body_index > 0 && Cos != null)
                    {
                        Cos = null;
                        CosName = string.Empty;
                    }
                }
                break;
            case  GameSetting.SkinPosition.TypeEmotion:
                {
                    emotion_index = index;
                    Emotion = emotion_name[emotion_index].Prefab;
                    EmotionName = emotion_name[emotion_index].Reward ? emotion_name[emotion_index].Id.ToString() : emotion_name[emotion_index].Name;
                    Scrolls[this.index].SetFocus(emotion_index, false);
                }
                break;
            case GameSetting.SkinPosition.TypeCos:
                {
                        cos_index = index;
                    Cos = cos_name[cos_index].Prefab;
                    CosName = cos_name[cos_index].Reward ? cos_name[cos_index].Id.ToString() : cos_name[cos_index].Name;
                    Scrolls[this.index].SetFocus(cos_index, false);
                }
                break;
            case GameSetting.SkinPosition.TypeSub:
                {
                    sub_index = index;
                    Sub = subs_name[sub_index].Prefab;
                    SubName = subs_name[sub_index].Reward ? subs_name[sub_index].Id.ToString() : subs_name[sub_index].Name;
                    Scrolls[this.index].SetFocus(sub_index, false);
                }
                break;
            case GameSetting.SkinPosition.TypeFace:
                {
                        face_index = index;
                    Face = faces[face_index];
                    FaceName = face_name[face_index];
                    Scrolls[this.index].SetFocus(face_index, false);
                }
                break;
            case  GameSetting.SkinPosition.TypeSkin:
                {
                        skin_index = index;
                    Skin = skins[skin_index];
                    SkinName = skins_name[skin_index];
                    Scrolls[this.index].SetFocus(skin_index, false);
                }
                break;
            case GameSetting.SkinPosition.TypeBodySkin:
                {
                        body_skin_index = index;
                    BodySkin = body_skins[body_skin_index];
                    BodySkinName = body_skins_name[body_skin_index];
                    Scrolls[this.index].SetFocus(body_skin_index, false);
                }
                break;
            case  GameSetting.SkinPosition.TypeTatoo:
                {
                        body_tatoo_index = index;
                    Tatoo = body_tatoos[body_tatoo_index];
                    TatooName = body_tatoo_name[body_tatoo_index];
                    Scrolls[this.index].SetFocus(body_tatoo_index, false);
                }
                break;
        }

        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
        SetCos?.Invoke(position);
    }

    private void OpenPreset(CallbackContext context)
    {
        Preset.OnShow();
    }
    public void OpenPreset()
    {
        EventSystem.current.SetSelectedGameObject(null);
        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Option);
        Preset_open = true;
        Animator.SetBool("Open", true);
        foreach (SkinScrollScript scroll in Scrolls)
            scroll.gameObject.SetActive(false);

        Refresh();
    }

    public void ClosePreset()
    {
        PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Option);
        Animator.SetBool("Open", false);
    }

    public void OnPresetClose()
    {
        Preset_open = false;

        for (int i = 0; i < Scrolls.Length; i++)
            Scrolls[i].gameObject.SetActive(i == index);
    }

    public void OnPresetOn()
    {
        Preset.SetInput(true);
    }

    public void Refresh()
    {
        head_index = 0;
        for (int n = 1; n < head_name.Keys.Count; n++)
        {
            if (GameSetting.Config.Head == (head_name[n].Reward ? head_name[n].Id.ToString() : head_name[n].Name))
            {
                head_index = n;
                break;
            }
        }
        HeadName = GameSetting.Config.Head;
        SettingLoader.Head = Head = head_name[head_index].Prefab;
        Scrolls[0].SetSelected(head_index);

        body_index = 0;
        for (int n = 1; n < bodies_name.Keys.Count; n++)
        {
            if (GameSetting.Config.Body == (bodies_name[n].Reward ? bodies_name[n].Id.ToString() : bodies_name[n].Name))
            {
                body_index = n;
                break;
            }
        }
        BodyName = GameSetting.Config.Body;
        SettingLoader.Body = Body = bodies_name[body_index].Prefab;
        Scrolls[1].SetSelected(body_index);

        //读取表情
        emotion_index = 0;
        for (int n = 1; n < emotion_name.Keys.Count; n++)
        {
            if (GameSetting.Config.Emotion == emotion_name[n].Name)
            {
                emotion_index = n;
                break;
            }
        }
        EmotionName = GameSetting.Config.Emotion;
        SettingLoader.Emotion = Emotion = emotion_name[emotion_index].Prefab;
        Scrolls[2].SetSelected(emotion_index);

        //读取全身
        cos_index = 0;
        for (int n = 1; n < cos_name.Keys.Count; n++)
        {
            if (GameSetting.Config.SkinCos == (cos_name[n].Reward ? cos_name[n].Id.ToString() : cos_name[n].Name))
            {
                cos_index = n;
                break;
            }
        }
        CosName = GameSetting.Config.SkinCos;
        SettingLoader.Cos = Cos = cos_name[cos_index].Prefab;
        Scrolls[3].SetSelected(cos_index);

        //读取挂件
        sub_index = 0;
        for (int n = 1; n < subs_name.Keys.Count; n++)
        {
            if (GameSetting.Config.SkinSub == (subs_name[n].Reward ? subs_name[n].Id.ToString() : subs_name[n].Name))
            {
                sub_index = n;
                break;
            }
        }
        SubName = GameSetting.Config.SkinSub;
        SettingLoader.Sub = Sub = subs_name[sub_index].Prefab;
        Scrolls[4].SetSelected(sub_index);

        //读取面部
        face_index = 0;
        for (int n = 1; n < face_name.Keys.Count; n++)
        {
            if (GameSetting.Config.Face == face_name[n])
            {
                face_index = n;
                break;
            }
        }
        FaceName = GameSetting.Config.Face;
        SettingLoader.Face = Face = faces[face_index];
        Scrolls[5].SetSelected(face_index);

        //读取皮肤
        skin_index = 0;
        for (int n = 1; n < skins_name.Keys.Count; n++)
        {
            if (GameSetting.Config.Skin == skins_name[n])
            {
                skin_index = n;
                break;
            }
        }
        SkinName = GameSetting.Config.Skin;
        SettingLoader.Skin = Skin = skins[skin_index];
        Scrolls[6].SetSelected(skin_index);

        //读取鼓皮
        body_skin_index = 0;
        for (int n = 1; n < body_skins_name.Keys.Count; n++)
        {
            if (GameSetting.Config.BodySkin == body_skins_name[n])
            {
                body_skin_index = n;
                break;
            }
        }
        BodySkinName = GameSetting.Config.BodySkin;
        SettingLoader.BodySkin = BodySkin = body_skins[body_skin_index];
        Scrolls[7].SetSelected(body_skin_index);

        //读取鼓皮纹身
        body_tatoo_index = 0;
        for (int n = 1; n < body_tatoo_name.Keys.Count; n++)
        {
            if (GameSetting.Config.Tatoo == body_tatoo_name[n])
            {
                body_tatoo_index = n;
                break;
            }
        }
        TatooName = GameSetting.Config.Tatoo;
        SettingLoader.Tatoo = Tatoo = body_tatoos[body_tatoo_index];
        Scrolls[8].SetSelected(body_tatoo_index);

        ResetCos?.Invoke();
    }

    public void PlaySound(bool don)
    {
        if (don)
            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Don);
        else
            PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Ka);
    }
}
