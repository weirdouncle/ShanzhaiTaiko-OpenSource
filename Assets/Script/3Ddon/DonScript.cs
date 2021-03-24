using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityFx.Outline;

public delegate void RandomPlayDelegate();
public class DonScript : MonoBehaviour
{
    public event RandomPlayDelegate RandomPlay;

    public Transform AddonParent;
    public Transform SubParent;

    public SkinnedMeshRenderer Emotion;
    public SkinnedMeshRenderer Body;
    public SkinnedMeshRenderer Skin;
    public SkinnedMeshRenderer FrontBack;
    public Texture Transparent;

    public MetaDonScript Robot;
    public Animator Animator;
    public AvatarMaxScript MaxMask;
    public bool StartOnHide;

    public Sprite[] DefaultEmotions;           //脸谱总共12个文件
    public bool Player2;
    //public bool Balloon;
    public Vector3 BalloonPosition;
    public DonImageScript Image;

    public OutlineEffect OutlineEffect;
    private Vector3 _position;
    private Quaternion _ratation;
    private Vector3 _scale;

    public GameObject Sub { set; get; } = null;

    private Texture[] emotions;
    private bool auto;
    private bool hide;
    private bool custom_emotion;

    private AvatarBoneScript bone;
    private List<GameObject> parts = new List<GameObject>();
    private EmotionScript emotion;
    private HeadColorScript head;
    private Dictionary<GameSetting.SkinPosition, bool> mask = new Dictionary<GameSetting.SkinPosition, bool>
    {
        { GameSetting.SkinPosition.TypeEmotion, false },
        { GameSetting.SkinPosition.TypeHead, false },
        { GameSetting.SkinPosition.TypeBody, false },
        { GameSetting.SkinPosition.TypeBodySkin, false },
        { GameSetting.SkinPosition.TypeFrontBack, false },
    };
    private Dictionary<GameSetting.SkinPosition, bool> keeps = new Dictionary<GameSetting.SkinPosition, bool>
    {
        { GameSetting.SkinPosition.TypeEmotion, false },
        { GameSetting.SkinPosition.TypeHead, false },
        { GameSetting.SkinPosition.TypeBody, false },
        { GameSetting.SkinPosition.TypeBodySkin, false },
    }; 
    private Dictionary<GameSetting.SkinPosition, bool> change = new Dictionary<GameSetting.SkinPosition, bool>
    {
        { GameSetting.SkinPosition.TypeBody, false },
        { GameSetting.SkinPosition.TypeBodySkin, false },
    };
    private Color origin_face = new Color(0.973f, 0.282f, 0.157f);
    private Color origin_skin = new Color(0.976f, 0.937f, 0.875f);
    private Color origin_bodyskin = new Color(0.4078f, 0.753f, 0.753f);

    private Color origin_face_2p = new Color(0.157f, 0.754f, 0.765f);
    private Color origin_skin_2p = new Color(0.976f, 0.937f, 0.875f);
    private Color origin_bodyskin_2p = new Color(0.98f, 0.286f, 0.161f);
    private Color skin_color;
    private Color face_color;
    private Color bodyskin_color;
    private bool katsu;
    void Start()
    {
        _position = transform.localPosition;
        _ratation = transform.localRotation;
        _scale = transform.localScale;

        //if (Balloon) Debug.Log(_ratation);

        if (Player2 && GameSetting.Mode != CommonClass.PlayMode.PlayWithReplay)
            katsu = true;

        skin_color = katsu ? origin_skin_2p : origin_skin;
        face_color = katsu ? origin_face_2p : origin_face;

        OptionScript.SetAuto += AutoChange;
        emotions = new Texture[DefaultEmotions.Length];
        int index = 0;
        foreach (Sprite sprite in DefaultEmotions)
        {
            var targetTex = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            var pixels = sprite.texture.GetPixels(
                (int)sprite.textureRect.x,
                (int)sprite.textureRect.y,
                (int)sprite.textureRect.width,
                (int)sprite.textureRect.height);
            targetTex.SetPixels(pixels);

            targetTex.Apply();
            emotions[index] = targetTex;
            index++;
        }

        ReLoadCos();

        if (StartOnHide) gameObject.SetActive(false);
        string scene = SceneManager.GetActiveScene().name;
        if ((scene.Contains("Game") || scene.Contains("Result")) && (!Player2 && GameSetting.Mode != CommonClass.PlayMode.Replay || Player2 && GameSetting.Mode == CommonClass.PlayMode.Normal))
        {
            Robot.Init(Body, Player2);
            //初始化机器咚的贴图
            AutoChange();
        }
        else if (Robot != null)
            Robot.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        OptionScript.SetAuto -= AutoChange;
    }

    #region cos
    private void ReLoadCos()
    {
        bone = null;
        head = null;
        string scene = SceneManager.GetActiveScene().name;
        if (Player2 && (scene.Contains("Select") || scene.Contains("DualGame") || scene == "ResultDual" || scene == "ResultNijiiro"))
        {
            UpdateColor(true);
            if (OutlineEffect != null) OutlineEffect.UpdateOutlineObject();
            return;
        }

        if (Player2 || ((scene.Contains("Game") || scene.Contains("Result")) && GameSetting.Mode == CommonClass.PlayMode.Replay))
        {
            Config config = new Config();
            config.CopyReplayConfig(GameSetting.Replay.Config[GameSetting.Config.ScoreMode][(int)GameSetting.Difficulty]);

            if (SettingLoader.Replay_Cos != null)
            {
                GameObject game = Instantiate(SettingLoader.Replay_Cos, AddonParent);
                parts.Add(game);
                bone = game.GetComponent<AvatarBoneScript>();
                if (bone != null && bone.News.Length > 0)
                {
                    //game.transform.SetParent(transform);
                    foreach (SkinnedMeshRenderer skin in bone.News)
                        skin.bones = Body.bones;
                }
                ReAssignMaskKeep(config.SkinCos);
                if (mask[GameSetting.SkinPosition.TypeEmotion])
                    emotion = game.GetComponentInChildren<EmotionScript>();
            }

            if (!keeps[GameSetting.SkinPosition.TypeEmotion] && !mask[GameSetting.SkinPosition.TypeEmotion] && SettingLoader.Replay_Emotion != null)
            {
                GameObject game = Instantiate(SettingLoader.Replay_Emotion, AddonParent);
                parts.Add(game);
                emotion = game.GetComponent<EmotionScript>();
                mask[GameSetting.SkinPosition.TypeEmotion] = true;
            }

            if (!keeps[GameSetting.SkinPosition.TypeHead] && !mask[GameSetting.SkinPosition.TypeHead] && SettingLoader.Replay_Head != null)
            {
                GameObject game = Instantiate(SettingLoader.Replay_Head, AddonParent);
                head = game.GetComponent<HeadColorScript>();
                parts.Add(game);
            }

            if (SettingLoader.Replay_Cos == null && SettingLoader.Replay_Body != null)
            {
                GameObject game = Instantiate(SettingLoader.Replay_Body, AddonParent);
                parts.Add(game);
                change[GameSetting.SkinPosition.TypeBody] = true;
                mask[GameSetting.SkinPosition.TypeBodySkin] = GameSetting.MaskPosition(config.Body, GameSetting.SkinPosition.TypeBodySkin, false);
                mask[GameSetting.SkinPosition.TypeFrontBack] = GameSetting.MaskPosition(config.Body, GameSetting.SkinPosition.TypeFrontBack, false);
                mask[GameSetting.SkinPosition.TypeBody] = GameSetting.MaskPosition(config.Body, GameSetting.SkinPosition.TypeBody, false);

                bone = game.GetComponent<AvatarBoneScript>();

                if (bone != null)
                {
                    if (bone.News.Length > 0)
                    {
                        //game.transform.SetParent(transform);
                        foreach (SkinnedMeshRenderer skin in bone.News)
                            skin.bones = Body.bones;
                    }

                    if (GameSetting.MaskPosition(config.Body, GameSetting.SkinPosition.TypeEmotion, false))
                    {
                        mask[GameSetting.SkinPosition.TypeEmotion] = true;
                        if (emotion != null)
                        {
                            MeshFilter mesh = emotion.GetComponent<MeshFilter>();
                            bool big = mesh.mesh.name.Contains("BIG");
                            bone.InitEmotion(emotion.Emotions, big);
                            emotion.gameObject.SetActive(false);
                        }
                        else
                            bone.InitEmotion(DefaultEmotions, false);
                    }
                }
            }

            if (head != null)
            {
                if (emotion != null)
                {
                    MeshFilter mesh = emotion.GetComponent<MeshFilter>();
                    bool big = mesh.mesh.name.Contains("BIG");
                    head.InitEmotion(emotion.Emotions, big);
                }
                else
                    head.InitEmotion(DefaultEmotions, false);
            }

            if (mask[GameSetting.SkinPosition.TypeEmotion])
                Emotion.material.SetTexture("_MainTex", Transparent);

            Body.gameObject.SetActive(!mask[GameSetting.SkinPosition.TypeBody]);

            FrontBack.gameObject.SetActive(!mask[GameSetting.SkinPosition.TypeFrontBack]);

            if (!keeps[GameSetting.SkinPosition.TypeBodySkin] && !mask[GameSetting.SkinPosition.TypeBodySkin])
            {
                bodyskin_color = SettingLoader.Replay_BodySkin != null ? SettingLoader.Replay_BodySkin.GetColor("_Color") : origin_bodyskin;
                if (head != null) head.SetBodySkin(bodyskin_color);
                if (bone != null) change[GameSetting.SkinPosition.TypeBodySkin] = bone.SetBodySkin(bodyskin_color, SettingLoader.Replay_Tatoo);
                if (!change[GameSetting.SkinPosition.TypeBodySkin])
                {
                    if (SettingLoader.Replay_Tatoo != null)
                    {
                        Skin.gameObject.SetActive(true);
                        Skin.material = SettingLoader.Replay_Tatoo;
                    }
                    else
                        Skin.gameObject.SetActive(false);
                }
                else
                    Skin.gameObject.SetActive(false);
            }
            else
            {
                bodyskin_color = !keeps[GameSetting.SkinPosition.TypeBodySkin] && SettingLoader.Replay_BodySkin != null ? SettingLoader.Replay_BodySkin.GetColor("_Color") : origin_bodyskin;
                if (head != null) head.SetBodySkin(bodyskin_color);
                if (bone != null) change[GameSetting.SkinPosition.TypeBodySkin] = bone.SetBodySkin(bodyskin_color, SettingLoader.Replay_Tatoo);
                Skin.gameObject.SetActive(false);
            }

            //Skin.gameObject.SetActive(!mask[GameSetting.SkinPosition.TypeBodySkin] && !change[GameSetting.SkinPosition.TypeBodySkin]);

            if (SettingLoader.Replay_Face != null)
                face_color = SettingLoader.Replay_Face.GetColor("_Color");

            if (SettingLoader.Replay_Skin != null)
                skin_color = SettingLoader.Replay_Skin.GetColor("_Color");

            if (SubParent != null && SettingLoader.Replay_Sub != null)
            {
                Sub = Instantiate(SettingLoader.Replay_Sub, SubParent);
                parts.Add(Sub);
            }
        }
        else
        {
            if (SettingLoader.Cos != null)
            {
                GameObject game = Instantiate(SettingLoader.Cos, AddonParent);
                parts.Add(game);
                bone = game.GetComponent<AvatarBoneScript>();
                if (bone != null && bone.News.Length > 0)
                {
                    //game.transform.SetParent(transform);
                    foreach (SkinnedMeshRenderer skin in bone.News)
                        skin.bones = Body.bones;
                }
                ReAssignMaskKeep(GameSetting.Config.SkinCos);
                if (mask[GameSetting.SkinPosition.TypeEmotion])
                    emotion = game.GetComponentInChildren<EmotionScript>();
            }

            if (!keeps[GameSetting.SkinPosition.TypeEmotion] && !mask[GameSetting.SkinPosition.TypeEmotion] && SettingLoader.Emotion != null)
            {
                GameObject game = Instantiate(SettingLoader.Emotion, AddonParent);
                parts.Add(game);
                emotion = game.GetComponent<EmotionScript>();
                mask[GameSetting.SkinPosition.TypeEmotion] = true;
            }

            if (!keeps[GameSetting.SkinPosition.TypeHead] && !mask[GameSetting.SkinPosition.TypeHead] && SettingLoader.Head != null)
            {
                GameObject game = Instantiate(SettingLoader.Head, AddonParent);
                head = game.GetComponent<HeadColorScript>();
                parts.Add(game);
            }

            if (SettingLoader.Cos == null && SettingLoader.Body != null)
            {
                GameObject game = Instantiate(SettingLoader.Body, AddonParent);
                parts.Add(game);
                change[GameSetting.SkinPosition.TypeBody] = true;
                mask[GameSetting.SkinPosition.TypeBodySkin] = GameSetting.MaskPosition(GameSetting.Config.Body, GameSetting.SkinPosition.TypeBodySkin, false);
                mask[GameSetting.SkinPosition.TypeFrontBack] = GameSetting.MaskPosition(GameSetting.Config.Body, GameSetting.SkinPosition.TypeFrontBack, false);
                mask[GameSetting.SkinPosition.TypeBody] = GameSetting.MaskPosition(GameSetting.Config.Body, GameSetting.SkinPosition.TypeBody, false);

                bone = game.GetComponent<AvatarBoneScript>();
                if (bone != null)
                {
                    if (bone.News.Length > 0)
                    {
                        //game.transform.SetParent(transform);
                        foreach (SkinnedMeshRenderer skin in bone.News)
                            skin.bones = Body.bones;
                    }

                    if (GameSetting.MaskPosition(GameSetting.Config.Body, GameSetting.SkinPosition.TypeEmotion, false))
                    {
                        mask[GameSetting.SkinPosition.TypeEmotion] = true;
                        if (emotion != null)
                        {
                            MeshFilter mesh = emotion.GetComponent<MeshFilter>();
                            bool big = mesh.mesh.name.Contains("BIG");
                            bone.InitEmotion(emotion.Emotions, big);
                            emotion.gameObject.SetActive(false);
                        }
                        else
                            bone.InitEmotion(DefaultEmotions, false);
                    }
                }
            }
            if (head != null)
            {
                if (emotion != null)
                {
                    MeshFilter mesh = emotion.GetComponent<MeshFilter>();
                    bool big = mesh.mesh.name.Contains("BIG");
                    head.InitEmotion(emotion.Emotions, big);
                }
                else
                    head.InitEmotion(DefaultEmotions, false);
            }
            if (mask[GameSetting.SkinPosition.TypeEmotion])
                Emotion.material.SetTexture("_MainTex", Transparent);
            Body.gameObject.SetActive(!mask[GameSetting.SkinPosition.TypeBody]);

            FrontBack.gameObject.SetActive(!mask[GameSetting.SkinPosition.TypeFrontBack]);

            if (!keeps[GameSetting.SkinPosition.TypeBodySkin] && !mask[GameSetting.SkinPosition.TypeBodySkin])
            {
                bodyskin_color = SettingLoader.BodySkin != null ? SettingLoader.BodySkin.GetColor("_Color") : origin_bodyskin;
                if (head != null) head.SetBodySkin(bodyskin_color);
                if (bone != null)
                {
                    change[GameSetting.SkinPosition.TypeBodySkin] = bone.SetBodySkin(bodyskin_color, SettingLoader.Tatoo);
                }
                if (!change[GameSetting.SkinPosition.TypeBodySkin])
                {
                    if (SettingLoader.Tatoo != null)
                    {
                        Skin.gameObject.SetActive(true);
                        Skin.material = SettingLoader.Tatoo;
                    }
                    else
                        Skin.gameObject.SetActive(false);
                }
                else
                    Skin.gameObject.SetActive(false);
            }
            else
            {
                bodyskin_color = !keeps[GameSetting.SkinPosition.TypeBodySkin] && SettingLoader.BodySkin != null ? SettingLoader.BodySkin.GetColor("_Color") : origin_bodyskin;
                if (head != null) head.SetBodySkin(bodyskin_color);
                if (bone != null) change[GameSetting.SkinPosition.TypeBodySkin] = bone.SetBodySkin(bodyskin_color, SettingLoader.Tatoo);
                Skin.gameObject.SetActive(false);
            }

            //Skin.gameObject.SetActive(!mask[GameSetting.SkinPosition.TypeBodySkin] && !change[GameSetting.SkinPosition.TypeBodySkin]);

            if (SettingLoader.Face != null)
                face_color = SettingLoader.Face.GetColor("_Color");

            if (SettingLoader.Skin != null)
                skin_color = SettingLoader.Skin.GetColor("_Color");

            if (SubParent != null && SettingLoader.Sub != null)
            {
                Sub = Instantiate(SettingLoader.Sub, SubParent);
                parts.Add(Sub);
            }
        }

        foreach (GameObject part in parts)
        {
            if (part == Sub) continue;
            part.layer = gameObject.layer;
            foreach (Transform tran in part.transform)
                tran.gameObject.layer = gameObject.layer;
        }

        if (SubParent != null)
            foreach (Transform trans in SubParent.transform)
                trans.gameObject.layer = 0;
        /*
        if (Balloon && Sub != null)
            foreach (SpriteRenderer trans in SubParent.GetComponentsInChildren<SpriteRenderer>())
                trans.sortingLayerName = "JumpOut";
        */

        //修改身体、面部贴图
        UpdateColor(false);

        if (bone != null)
        {
            bone.SetSkin(skin_color);
            bone.SetFace(face_color);
        }
        if (head != null)
        {
            head.SetSkin(skin_color);
            head.SetFace(face_color);
        }

        if (OutlineEffect != null) OutlineEffect.UpdateOutlineObject();
    }

    private void ReAssignMaskKeep(string cos_name)
    {
        mask[GameSetting.SkinPosition.TypeEmotion] = GameSetting.MaskPosition(cos_name, GameSetting.SkinPosition.TypeEmotion, true);
        mask[GameSetting.SkinPosition.TypeHead] = GameSetting.MaskPosition(cos_name, GameSetting.SkinPosition.TypeHead, true);
        mask[GameSetting.SkinPosition.TypeBody] = GameSetting.MaskPosition(cos_name, GameSetting.SkinPosition.TypeBody, true);
        mask[GameSetting.SkinPosition.TypeBodySkin] = GameSetting.MaskPosition(cos_name, GameSetting.SkinPosition.TypeBodySkin, true);
        mask[GameSetting.SkinPosition.TypeFrontBack] = GameSetting.MaskPosition(cos_name, GameSetting.SkinPosition.TypeFrontBack, true);

        keeps[GameSetting.SkinPosition.TypeEmotion] = GameSetting.KeepPostionDefault(cos_name, GameSetting.SkinPosition.TypeEmotion);
        keeps[GameSetting.SkinPosition.TypeHead] = GameSetting.KeepPostionDefault(cos_name, GameSetting.SkinPosition.TypeHead);
        keeps[GameSetting.SkinPosition.TypeBody] = GameSetting.KeepPostionDefault(cos_name, GameSetting.SkinPosition.TypeBody);
        keeps[GameSetting.SkinPosition.TypeBodySkin] = GameSetting.KeepPostionDefault(cos_name, GameSetting.SkinPosition.TypeBodySkin);
    }
    private void UpdateColor(bool origin)
    {
        if (origin)
        {
            Color body_skin_color = Player2 ? origin_bodyskin_2p : origin_bodyskin;
            Body.material.SetColor("_Red", body_skin_color);
            Body.material.SetColor("_Blue", !katsu ? origin_skin : origin_skin_2p);

            FrontBack.material.SetColor("_Green", !katsu ? origin_face : origin_face_2p);
            FrontBack.material.SetColor("_Blue", !katsu ? origin_skin : origin_skin_2p);
        }
        else
        {
            Body.material.SetColor("_Red", bodyskin_color);
            Body.material.SetColor("_Blue", skin_color);

            FrontBack.material.SetColor("_Green", face_color);
            FrontBack.material.SetColor("_Blue", skin_color);
        }
    }
    #endregion
    private void AutoChange()
    {
        string scene = SceneManager.GetActiveScene().name;
        auto = GameSetting.Player2 && Player2 ? GameSetting.Special2P == CommonClass.Special.AutoPlay : GameSetting.Config.Special == CommonClass.Special.AutoPlay;
        if ((Player2 && !GameSetting.Player2) || (scene.Contains("Game") && GameSetting.Mode == CommonClass.PlayMode.Replay))
            auto = false;

        if (auto)
        {
            foreach (GameObject part in parts)
                part.SetActive(false);

            UpdateColor(true);

            Emotion.material.SetTexture("_MainTex", Transparent);

            Skin.gameObject.SetActive(false);
            Body.gameObject.SetActive(false);
            FrontBack.gameObject.SetActive(false);
            Robot.gameObject.SetActive(!hide);
        }
        else
        {
            Robot.gameObject.SetActive(false);

            if (!Player2)
            {
                foreach (GameObject part in parts)
                    part.SetActive(!hide);

                if (bone != null && !keeps[GameSetting.SkinPosition.TypeBody] && !mask[GameSetting.SkinPosition.TypeBody] && SettingLoader.Body != null
                    && GameSetting.MaskPosition(GameSetting.Config.Body, GameSetting.SkinPosition.TypeEmotion, false))
                {
                    if (emotion != null) emotion.gameObject.SetActive(false);
                }
            }

            Body.gameObject.SetActive(!hide && !mask[GameSetting.SkinPosition.TypeBody]);
            Skin.gameObject.SetActive(!hide && !change[GameSetting.SkinPosition.TypeBodySkin] && !mask[GameSetting.SkinPosition.TypeBodySkin]);
            FrontBack.gameObject.SetActive(!hide && !mask[GameSetting.SkinPosition.TypeFrontBack]);

            if (!Player2) UpdateColor(false);
        }
    }

    public void Hide(bool hide, bool balloon)
    {
        if (!balloon)
        {
            this.hide = hide;
            Emotion.gameObject.SetActive(!hide);
            if (auto)
            {
                Skin.gameObject.SetActive(false);

                FrontBack.gameObject.SetActive(false);
                Body.gameObject.SetActive(false);
                Robot.gameObject.SetActive(!hide);
            }
            else
            {
                if (Robot != null) Robot.gameObject.SetActive(false);
                foreach (GameObject part in parts)
                    part.SetActive(!hide);

                if (bone != null && !keeps[GameSetting.SkinPosition.TypeBody] && !mask[GameSetting.SkinPosition.TypeBody] && SettingLoader.Body != null
                    && GameSetting.MaskPosition(GameSetting.Config.Body, GameSetting.SkinPosition.TypeEmotion, false))
                {
                    if (emotion != null) emotion.gameObject.SetActive(false);
                }

                Body.gameObject.SetActive(!hide && !mask[GameSetting.SkinPosition.TypeBody]);
                Skin.gameObject.SetActive(!hide && !change[GameSetting.SkinPosition.TypeBodySkin] && !mask[GameSetting.SkinPosition.TypeBodySkin]);
                FrontBack.gameObject.SetActive(!hide && !mask[GameSetting.SkinPosition.TypeFrontBack]);
            }
        }

        if (MaxMask != null)
            MaxMask.SetMax(!hide && Animator.GetBool("Max"));
    }

    public void SetBpm(float bpm)
    {
        Animator.speed = (float)bpm / 60;
    }


    #region Game Play

    public void OnReset()
    {
        ResetCombo();
        Animator.SetBool("Max", false);
        Animator.SetBool("Clear", false);
        Animator.SetBool("Miss", false);

        ballooning = false;
        if (Animator.GetBool("Balloon"))  ShowBalloon(false);
    }

    public void Combo()
    {
        Animator.SetTrigger("Combo");
    }

    public void SetClear(bool clear)
    {
        if (Animator.GetBool("Clear") != clear)
        {
            if (clear && !Animator.GetBool("Balloon"))
                Animator.SetTrigger("ClearIn");

            Animator.SetBool("Clear", clear);
        }
    }

    public void ResetCombo()
    {
        Animator.ResetTrigger("Combo");
    }

    public void Max(bool max)
    {
        MaxMask.SetMax(max);

        if (Animator.GetBool("Max") != max)
        {
            if (max && !Animator.GetBool("Balloon"))
                Animator.SetTrigger("MaxIn");
            Animator.SetBool("Max", max);
        }
    }

    public void Gogo(bool go)
    {
        if (Animator.GetBool("Gogo") != go)
        {
            if (!go)
                ResetCombo();
            else if (!Animator.GetBool("Balloon"))
            {
                Animator.SetTrigger("GogoStart");
            }

            Animator.SetBool("Gogo", go);
        }
    }

    public void SetDefaultEmotion()
    {
        if (!custom_emotion) SetEmotion4();
    }

    public void Miss()
    {
        if (!custom_emotion && Animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            custom_emotion = true;
            if (!auto && !mask[GameSetting.SkinPosition.TypeEmotion])
                Emotion.material.SetTexture("_MainTex", emotions[10]);
            if (Robot != null) Robot.SetEmotion10();
            if (emotion != null) emotion.SetEmotion10();
            StartCoroutine(MissEmotion());
        }
    }

    IEnumerator MissEmotion()
    {
        yield return new WaitForSeconds(0.1f);
        if (custom_emotion)
        {
            if (!auto && !mask[GameSetting.SkinPosition.TypeEmotion])
                Emotion.material.SetTexture("_MainTex", emotions[10]);
            if (Robot != null) Robot.SetEmotion10();
            if (emotion != null) emotion.SetEmotion10();

            yield return new WaitForSeconds(0.05f);
            if (custom_emotion) SetEmotion4();
        }
    }

    public void SetMiss(bool miss)
    {
        if (Animator.GetBool("Miss") != miss)
        {
            Animator.SetBool("Miss", miss);
        }
    }

    #endregion
    public void HideOnPlayEnd()
    {
        gameObject.SetActive(false);
    }

    public void OnPlayRandom()
    {
        RandomPlay?.Invoke();
    }

    public void SetEmotion0()
    {
        custom_emotion = false;
        if (!auto && !mask[GameSetting.SkinPosition.TypeEmotion])
            Emotion.material.SetTexture("_MainTex", emotions[0]);
        if (Robot != null) Robot.SetEmotion0();
        if (emotion != null) emotion.SetEmotion0();
        if (bone != null) bone.SetEmotion0();
    }

    public void SetEmotion1()
    {
        custom_emotion = false;
        if (!auto && !mask[GameSetting.SkinPosition.TypeEmotion])
            Emotion.material.SetTexture("_MainTex", emotions[1]);
        if (Robot != null) Robot.SetEmotion1();
        if (emotion != null) emotion.SetEmotion1();
        if (bone != null) bone.SetEmotion1();
    }
    public void SetEmotion2()
    {
        custom_emotion = false;
        if (!auto && !mask[GameSetting.SkinPosition.TypeEmotion])
            Emotion.material.SetTexture("_MainTex", emotions[2]);
        if (Robot != null) Robot.SetEmotion2();
        if (emotion != null) emotion.SetEmotion2();
        if (bone != null) bone.SetEmotion2();
    }
    public void SetEmotion3()
    {
        custom_emotion = false;
        if (!auto && !mask[GameSetting.SkinPosition.TypeEmotion])
            Emotion.material.SetTexture("_MainTex", emotions[3]);
        if (Robot != null) Robot.SetEmotion3();
        if (emotion != null) emotion.SetEmotion3();
        if (bone != null) bone.SetEmotion3();
    }
    public void SetEmotion4()
    {
        custom_emotion = false;
        if (!auto && !mask[GameSetting.SkinPosition.TypeEmotion])
            Emotion.material.SetTexture("_MainTex", emotions[4]);
        if (Robot != null) Robot.SetEmotion4();
        if (emotion != null) emotion.SetEmotion4();
        if (bone != null) bone.SetEmotion4();
    }
    public void SetEmotion5()
    {
        custom_emotion = false;
        if (!auto && !mask[GameSetting.SkinPosition.TypeEmotion])
            Emotion.material.SetTexture("_MainTex", emotions[5]);
        if (Robot != null) Robot.SetEmotion5();
        if (emotion != null) emotion.SetEmotion5();
        if (bone != null) bone.SetEmotion5();
    }
    public void SetEmotion6()
    {
        custom_emotion = false;
        if (!auto && !mask[GameSetting.SkinPosition.TypeEmotion])
            Emotion.material.SetTexture("_MainTex", emotions[6]);
        if (Robot != null) Robot.SetEmotion6();
        if (emotion != null) emotion.SetEmotion6();
        if (bone != null) bone.SetEmotion6();
    }
    public void SetEmotion7()
    {
        custom_emotion = false;
        if (!auto && !mask[GameSetting.SkinPosition.TypeEmotion])
            Emotion.material.SetTexture("_MainTex", emotions[7]);
        if (Robot != null) Robot.SetEmotion7();
        if (emotion != null) emotion.SetEmotion7();
        if (bone != null) bone.SetEmotion7();
    }

    public void SetEmotion8()
    {
        custom_emotion = false;
        if (!auto && !mask[GameSetting.SkinPosition.TypeEmotion])
            Emotion.material.SetTexture("_MainTex", emotions[8]);
        if (Robot != null) Robot.SetEmotion8();
        if (emotion != null) emotion.SetEmotion8();
        if (bone != null) bone.SetEmotion8();
    }
    public void SetEmotion9()
    {
        custom_emotion = false;
        if (!auto && !mask[GameSetting.SkinPosition.TypeEmotion])
            Emotion.material.SetTexture("_MainTex", emotions[9]);
        if (Robot != null) Robot.SetEmotion9();
        if (emotion != null) emotion.SetEmotion9();
        if (bone != null) bone.SetEmotion9();
    }
    public void SetEmotion10()
    {
        custom_emotion = false;
        if (!auto && !mask[GameSetting.SkinPosition.TypeEmotion])
            Emotion.material.SetTexture("_MainTex", emotions[10]);
        if (Robot != null) Robot.SetEmotion10();
        if (emotion != null) emotion.SetEmotion10();
        if (bone != null) bone.SetEmotion10();
    }
    public void SetEmotion11()
    {
        custom_emotion = false;
        if (!auto && !mask[GameSetting.SkinPosition.TypeEmotion])
            Emotion.material.SetTexture("_MainTex", emotions[11]);
        if (Robot != null) Robot.SetEmotion11();
        if (emotion != null) emotion.SetEmotion11();
        if (bone != null) bone.SetEmotion11();
    }


    #region Balloon
    private bool ballooning;
    public void ShowBalloon(bool show)
    {
        if (show)
        {
            //Debug.Log("balloon start");
            ballooning = true;

            Animator.SetTrigger("BalloonStart");
            Animator.SetBool("Balloon", true);
            transform.localPosition = BalloonPosition;
            transform.localRotation = new Quaternion(0, 1, 0, 0.2f);
            transform.localScale = new Vector3(11, 11, 11);
            foreach (SpriteRenderer trans in SubParent.GetComponentsInChildren<SpriteRenderer>())
                trans.sortingLayerName = "JumpOut";

            Hide(true, true);
            Image.SetBalloon(true);
        }
        else if (!ballooning)
        {
            //Debug.Log("position reset");
            transform.localPosition = _position;
            transform.localRotation = _ratation;
            transform.localScale = _scale;
            foreach (SpriteRenderer trans in SubParent.GetComponentsInChildren<SpriteRenderer>())
                trans.sortingLayerName = "Default";

            Hide(false, true);
            Image.SetBalloon(false);

            Animator.SetBool("Balloon", false);
        }
    }

    public void BalloonFail()
    {
        ballooning = false;
        Animator.SetTrigger("Fail");
    }

    public void BalloonBroken()
    {
        //Debug.Log("broken");
        ballooning = false;
        Animator.SetTrigger("Broken");
    }

    public void ResetPostion()
    {
        ShowBalloon(false);
    }
    #endregion
}