using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DonCosScript : MonoBehaviour
{
    public Transform AddonParent;
    public Transform SubParent;

    public SkinnedMeshRenderer Emotion;
    public SkinnedMeshRenderer Body;
    public SkinnedMeshRenderer Skin;
    public SkinnedMeshRenderer FrontBack;
    public Texture Transparent;

    public Animator Animator;

    public Sprite[] DefaultEmotions;           //脸谱总共12个文件

    public CosChangeScript Cos;
    public UnityFx.Outline.OutlineEffect OutlineEffect;

    private Texture[] default_emotions;
    private bool custom_emotion;
    private AvatarBoneScript bone;
    private Dictionary<GameSetting.SkinPosition, GameObject> parts = new Dictionary<GameSetting.SkinPosition, GameObject>();
    private HeadColorScript head;
    private EmotionScript emotion;
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

    private Color origin_face = new Color(0.973f, 0.282f, 0.157f);
    private Color origin_skin = new Color(0.976f, 0.937f, 0.875f);
    private Color origin_bodyskin = new Color(0.4078f, 0.753f, 0.753f);

    private Color origin_face_2p = new Color(0.973f, 0.282f, 0.157f);
    private Color origin_skin_2p = new Color(0.976f, 0.937f, 0.875f);

    private Color skin_color;
    private Color face_color;
    private Color body_skin_color;

    void Awake()
    {
        skin_color = origin_skin;
        face_color = origin_face;

        default_emotions = new Texture[DefaultEmotions.Length];
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
            default_emotions[index] = targetTex;
            index++;
        }

        Cos.SetCos += CosChange;
        Cos.ResetCos += ResetCos;
    }

    private void OnDestroy()
    {
        Cos.SetCos -= CosChange;
        Cos.ResetCos -= ResetCos;
    }

    #region cos
    private void ResetCos()
    {
        ReLoadCos(true);
    }
    private void CosChange(GameSetting.SkinPosition position)
    {
        bool change_color = false;
        switch (position)
        {
            case GameSetting.SkinPosition.TypeBody:
            case GameSetting.SkinPosition.TypeCos:
            case GameSetting.SkinPosition.TypeHead:
                ReLoadCos(false);
                break;
            case GameSetting.SkinPosition.TypeBodySkin:
            case GameSetting.SkinPosition.TypeTatoo:
                change_color = true;
                if (!keeps[GameSetting.SkinPosition.TypeBodySkin] && !mask[GameSetting.SkinPosition.TypeBodySkin])
                {
                    bool change = false;
                    body_skin_color = Cos.BodySkin != null ? Cos.BodySkin.GetColor("_Color") : origin_bodyskin;
                    if (bone != null) change = bone.SetBodySkin(body_skin_color, Cos.Tatoo);
                    if (!change)
                    {
                        if (Cos.Tatoo != null)
                        {
                            Skin.gameObject.SetActive(true);
                            Skin.material = Cos.Tatoo;
                        }
                        else
                            Skin.gameObject.SetActive(false);
                    }
                    else
                        Skin.gameObject.SetActive(false);

                    if (head != null) head.SetBodySkin(body_skin_color);
                }
                else
                {
                    body_skin_color = !keeps[GameSetting.SkinPosition.TypeBodySkin] && Cos.BodySkin != null ? Cos.BodySkin.GetColor("_Color") : origin_bodyskin;
                    Skin.gameObject.SetActive(false);
                    if (head != null) head.SetBodySkin(body_skin_color);
                    if (bone != null) bone.SetBodySkin(body_skin_color, Cos.Tatoo);
                }
                break;
            case GameSetting.SkinPosition.TypeFace:
                change_color = true;
                if (Cos.Face != null)
                {
                    face_color = Cos.Face.GetColor("_Color");
                    //FrontBack.materials = new Material[2] { FrontBack.materials[0], Cos.Face };
                    //if (bone != null) bone.SetFace(Cos.Face);
                    //if (head != null) head.SetFace(Cos.Face);
                }
                else
                {
                    face_color = origin_face;
                    //FrontBack.materials = new Material[2] { FrontBack.materials[0], origin_face };
                    //if (bone != null) bone.SetFace(origin_face);
                    //if (head != null) head.SetFace(origin_face);
                }
                break;
            case GameSetting.SkinPosition.TypeEmotion:
                if (Cos.Cos != null && keeps[GameSetting.SkinPosition.TypeEmotion]) break;
                if (emotion != null)
                {
                    Destroy(emotion.gameObject);
                    parts.Remove(GameSetting.SkinPosition.TypeEmotion);
                }

                bool big = false;
                if (Cos.Emotion != null)
                {
                    GameObject game = Instantiate(Cos.Emotion, AddonParent);
                    parts.Add(GameSetting.SkinPosition.TypeEmotion, game);
                    emotion = game.GetComponent<EmotionScript>();
                    mask[GameSetting.SkinPosition.TypeEmotion] = true;

                    MeshFilter mesh = emotion.GetComponent<MeshFilter>();
                    big = mesh.mesh.name.Contains("BIG");
                    if (head != null) head.InitEmotion(DefaultEmotions, big);
                }
                else
                {
                    mask[GameSetting.SkinPosition.TypeEmotion] = false;
                    if (head != null) head.InitEmotion(DefaultEmotions, false);
                }

                if (Cos.Cos == null && Cos.Body != null && bone != null
                    && GameSetting.MaskPosition(Cos.BodyName, GameSetting.SkinPosition.TypeEmotion, false))
                {
                    mask[GameSetting.SkinPosition.TypeEmotion] = true;
                    if (emotion != null)
                    {
                        bone.InitEmotion(emotion.Emotions, big);

                        //bone.InitEmotion(emotion.Emotions, GameSetting.GetEmotionBig(GameSetting.Config.Emotion));
                        emotion.gameObject.SetActive(false);
                    }
                    else
                        bone.InitEmotion(DefaultEmotions, false);
                }

                if (mask[GameSetting.SkinPosition.TypeEmotion])
                    Emotion.material.SetTexture("_MainTex", Transparent);
                break;
            case GameSetting.SkinPosition.TypeSub:
                if (parts.TryGetValue(GameSetting.SkinPosition.TypeSub, out GameObject sub))
                {
                    Destroy(sub);
                    parts.Remove(GameSetting.SkinPosition.TypeSub);
                }
                if (Cos.Sub != null)
                {
                    GameObject game = Instantiate(Cos.Sub, SubParent);
                    parts.Add(GameSetting.SkinPosition.TypeSub, game);

                    foreach (Transform transform in SubParent.transform)
                        transform.gameObject.layer = 0;
                }
                break;
            case GameSetting.SkinPosition.TypeSkin:
                change_color = true;
                if (Cos.Skin != null)
                {
                    skin_color = Cos.Skin.GetColor("_Color");
                    //FrontBack.material = Body.material = Cos.Skin;
                    //if (bone != null) bone.SetSkin(Cos.Skin);
                    //if (head != null) head.SetSkin(Cos.Skin);
                }
                else
                {
                    skin_color = origin_skin;
                    //FrontBack.material = Body.material = origin_skin;
                    //if (bone != null) bone.SetSkin(origin_skin);
                    //if (head != null) head.SetSkin(origin_skin);
                }
                break;
        }

        if (change_color)
        {
            //修改身体、面部贴图
            UpdateBody();
            UpdateFrontBack();

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
        }

        OutlineEffect.UpdateOutlineObject();
    }

    private void ReLoadCos(bool sub_reload)
    {
        List<GameSetting.SkinPosition> positions = new List<GameSetting.SkinPosition>(parts.Keys);
        foreach (GameSetting.SkinPosition position in positions)
        {
            if (sub_reload || position != GameSetting.SkinPosition.TypeSub)
            {
                Destroy(parts[position]);
                parts.Remove(position);
            }
        }

        emotion = null;
        head = null;
        bone = null;
        if (Cos.Cos != null)
        {
            GameObject game = Instantiate(Cos.Cos, AddonParent);
            parts.Add(GameSetting.SkinPosition.TypeCos, game);
            bone = game.GetComponent<AvatarBoneScript>();
            if (bone != null && bone.News.Length > 0)
            {
                //game.transform.SetParent(transform);
                foreach (SkinnedMeshRenderer skin in bone.News)
                    skin.bones = Body.bones;
            }
            emotion = game.GetComponentInChildren<EmotionScript>();
        }
        ReAssignMaskKeep(Cos.CosName);

        if (!keeps[GameSetting.SkinPosition.TypeEmotion] && !mask[GameSetting.SkinPosition.TypeEmotion] && Cos.Emotion != null)
        {
            GameObject game = Instantiate(Cos.Emotion, AddonParent);
            parts.Add(GameSetting.SkinPosition.TypeEmotion, game);
            emotion = game.GetComponent<EmotionScript>();
            mask[GameSetting.SkinPosition.TypeEmotion] = true;
        }

        if (!keeps[GameSetting.SkinPosition.TypeHead] && !mask[GameSetting.SkinPosition.TypeHead] && Cos.Head != null)
        {
            GameObject game = Instantiate(Cos.Head, AddonParent);
            head = game.GetComponent<HeadColorScript>();
            parts.Add(GameSetting.SkinPosition.TypeHead, game);
        }

        if (Cos.Cos == null && Cos.Body != null)
        {
            GameObject game = Instantiate(Cos.Body, AddonParent);
            parts.Add(GameSetting.SkinPosition.TypeBody, game);
            mask[GameSetting.SkinPosition.TypeBodySkin] = GameSetting.MaskPosition(Cos.BodyName, GameSetting.SkinPosition.TypeBodySkin, false);
            mask[GameSetting.SkinPosition.TypeFrontBack] = GameSetting.MaskPosition(Cos.BodyName, GameSetting.SkinPosition.TypeFrontBack, false);
            mask[GameSetting.SkinPosition.TypeBody] = GameSetting.MaskPosition(Cos.BodyName, GameSetting.SkinPosition.TypeBody, false);

            bone = game.GetComponent<AvatarBoneScript>();
            if (bone != null)
            {
                if (bone.News.Length > 0)
                {
                    //game.transform.SetParent(transform);
                    foreach (SkinnedMeshRenderer skin in bone.News)
                        skin.bones = FrontBack.bones;
                }

                if (GameSetting.MaskPosition(Cos.BodyName, GameSetting.SkinPosition.TypeEmotion, false))
                {
                    mask[GameSetting.SkinPosition.TypeEmotion] = true;
                    if (emotion != null)
                    {
                        MeshFilter mesh = emotion.GetComponent<MeshFilter>();
                        bool big = mesh.mesh.name.Contains("BIG");
                        bone.InitEmotion(emotion.Emotions, big);

                        //bone.InitEmotion(emotion.Emotions, GameSetting.GetEmotionBig(GameSetting.Config.Emotion));
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

        bool change = false;
        if (!keeps[GameSetting.SkinPosition.TypeBodySkin] && !mask[GameSetting.SkinPosition.TypeBodySkin])
        {
            body_skin_color = Cos.BodySkin != null ? Cos.BodySkin.GetColor("_Color") : origin_bodyskin;
            if (head != null) head.SetBodySkin(body_skin_color);
            if (bone != null) change = bone.SetBodySkin(body_skin_color, Cos.Tatoo);

            if (!change)
            {
                if (Cos.Tatoo != null)
                {
                    Skin.gameObject.SetActive(true);
                    Skin.material = Cos.Tatoo;
                }
                else
                {
                    Skin.gameObject.SetActive(false);
                }
            }
            else
                Skin.gameObject.SetActive(false);
        }
        else
        {
            body_skin_color = !keeps[GameSetting.SkinPosition.TypeBodySkin] && Cos.BodySkin != null ? Cos.BodySkin.GetColor("_Color") : origin_bodyskin;
            if (head != null) head.SetBodySkin(body_skin_color);
            if (bone != null) bone.SetBodySkin(body_skin_color, Cos.Tatoo);
            Skin.gameObject.SetActive(false);
        }

        //Skin.gameObject.SetActive(!mask[GameSetting.SkinPosition.TypeBodySkin] && !change);

        if (Cos.Face != null)
        {
            face_color = Cos.Face.GetColor("_Color");
            /*
            FrontBack.materials = new Material[2] { FrontBack.materials[0], Cos.Face };
            if (bone != null) bone.SetFace(Cos.Face);
            if (head != null) head.SetFace(Cos.Face);
            */
        }
        else
        {
            face_color = origin_face;
            /*
            FrontBack.materials = new Material[2] { FrontBack.materials[0], origin_face };
            if (head != null) head.SetFace(origin_face);
            if (bone != null) bone.SetFace(origin_face);
            */
        }

        if (Cos.Skin != null)
        {
            skin_color = Cos.Skin.GetColor("_Color");
            /*
            FrontBack.material = Body.material = Cos.Skin;
            if (bone != null)
                bone.SetSkin(Cos.Skin);

            if (head != null) head.SetSkin(Cos.Skin);
            */
        }
        else
        {
            skin_color = origin_skin;
            /*
            FrontBack.material = Body.material = origin_skin;
            if (head != null) head.SetSkin(origin_skin);
            if (bone != null) bone.SetSkin(origin_skin);
            */
        }

        if (sub_reload && Cos.Sub != null)
        {
            GameObject game = Instantiate(Cos.Sub, SubParent);
            parts.Add(GameSetting.SkinPosition.TypeSub, game);

            foreach (Transform transform in SubParent.transform)
                transform.gameObject.layer = 0;
        }

        //修改身体、面部贴图
        UpdateBody();
        UpdateFrontBack();

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

        OutlineEffect.UpdateOutlineObject();
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

    private void UpdateBody()
    {
        Body.material.SetColor("_Red", body_skin_color);
        Body.material.SetColor("_Blue", skin_color);
    }

    private void UpdateFrontBack()
    {
        FrontBack.material.SetColor("_Green", face_color);
        FrontBack.material.SetColor("_Blue", skin_color);
    }

    #endregion


    #region Game Play

    public void OnReset()
    {
        ResetCombo();
        Animator.SetBool("Max", false);
        Animator.SetBool("Clear", false);
        Animator.SetBool("Miss", false);
    }

    public void Combo()
    {
        Animator.SetTrigger("Combo");
    }

    public void SetClear(bool clear)
    {
        if (Animator.GetBool("Clear") != clear)
        {
            if (clear)
                Animator.SetTrigger("ClearIn");

            Animator.SetBool("Clear", clear);
        }
    }

    public void ResetCombo()
    {
        Animator.ResetTrigger("Combo");
    }

    public void Gogo(bool go)
    {
        if (Animator.GetBool("Gogo") != go)
        {
            if (!go)
                ResetCombo();
            else
                Animator.SetTrigger("GogoStart");

            Animator.SetBool("Gogo", go);
        }
    }

    public void SetDefaultEmotion()
    {
        if (!custom_emotion)
        {
            SetEmotion4();
            if (emotion != null) emotion.SetEmotion4();
        }
    }

    public void Miss()
    {
        if (!custom_emotion && Animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            custom_emotion = true;
            if (!mask[GameSetting.SkinPosition.TypeEmotion])
                Emotion.material.SetTexture("_MainTex", default_emotions[10]);
            StartCoroutine(MissEmotion());
        }
    }

    IEnumerator MissEmotion()
    {
        yield return new WaitForSeconds(0.1f);
        if (custom_emotion)
        {
            if (!mask[GameSetting.SkinPosition.TypeEmotion])
                Emotion.material.SetTexture("_MainTex", default_emotions[10]);

            yield return new WaitForSeconds(0.05f);
            if (custom_emotion) SetEmotion4();
            if (emotion != null) emotion.SetEmotion4();
        }
    }

    public void SetMiss(bool miss)
    {
        if (Animator.GetBool("Miss") != miss)
            Animator.SetBool("Miss", miss);
    }

    #endregion
    public void HideOnPlayEnd()
    {
        gameObject.SetActive(false);
    }


    public void SetEmotion0()
    {
        custom_emotion = false;
        if (!mask[GameSetting.SkinPosition.TypeEmotion])
            Emotion.material.SetTexture("_MainTex", default_emotions[0]);
        if (emotion != null) emotion.SetEmotion0();
        if (bone != null) bone.SetEmotion0();
        if (head != null) head.SetEmotion0();
    }

    public void SetEmotion1()
    {
        custom_emotion = false;
        if (!mask[GameSetting.SkinPosition.TypeEmotion])
            Emotion.material.SetTexture("_MainTex", default_emotions[1]);
        if (emotion != null) emotion.SetEmotion1();
        if (bone != null) bone.SetEmotion1();
        if (head != null) head.SetEmotion1();
    }
    public void SetEmotion2()
    {
        custom_emotion = false;
        if (!mask[GameSetting.SkinPosition.TypeEmotion])
            Emotion.material.SetTexture("_MainTex", default_emotions[2]);
        if (emotion != null) emotion.SetEmotion2();
        if (bone != null) bone.SetEmotion2();
        if (head != null) head.SetEmotion2();
    }
    public void SetEmotion3()
    {
        custom_emotion = false;
        if (!mask[GameSetting.SkinPosition.TypeEmotion])
            Emotion.material.SetTexture("_MainTex", default_emotions[3]);
        if (emotion != null) emotion.SetEmotion3();
        if (bone != null) bone.SetEmotion3();
        if (head != null) head.SetEmotion3();
    }
    public void SetEmotion4()
    {
        custom_emotion = false;
        if (!mask[GameSetting.SkinPosition.TypeEmotion])
            Emotion.material.SetTexture("_MainTex", default_emotions[4]);
        if (emotion != null) emotion.SetEmotion4();
        if (bone != null) bone.SetEmotion4();
        if (head != null) head.SetEmotion4();
    }
    public void SetEmotion5()
    {
        custom_emotion = false;
        if (!mask[GameSetting.SkinPosition.TypeEmotion])
            Emotion.material.SetTexture("_MainTex", default_emotions[5]);
        if (emotion != null) emotion.SetEmotion5();
        if (bone != null) bone.SetEmotion5();
        if (head != null) head.SetEmotion5();
    }
    public void SetEmotion6()
    {
        custom_emotion = false;
        if (!mask[GameSetting.SkinPosition.TypeEmotion])
            Emotion.material.SetTexture("_MainTex", default_emotions[6]);
        if (emotion != null) emotion.SetEmotion6();
        if (bone != null) bone.SetEmotion6();
        if (head != null) head.SetEmotion6();
    }
    public void SetEmotion7()
    {
        custom_emotion = false;
        if (!mask[GameSetting.SkinPosition.TypeEmotion])
            Emotion.material.SetTexture("_MainTex", default_emotions[7]);
        if (emotion != null) emotion.SetEmotion7();
        if (bone != null) bone.SetEmotion7();
        if (head != null) head.SetEmotion7();
    }

    public void SetEmotion8()
    {
        custom_emotion = false;
        if (!mask[GameSetting.SkinPosition.TypeEmotion])
            Emotion.material.SetTexture("_MainTex", default_emotions[8]);
        if (emotion != null) emotion.SetEmotion8();
        if (bone != null) bone.SetEmotion8();
        if (head != null) head.SetEmotion8();
    }
    public void SetEmotion9()
    {
        custom_emotion = false;
        if (!mask[GameSetting.SkinPosition.TypeEmotion])
            Emotion.material.SetTexture("_MainTex", default_emotions[9]);
        if (emotion != null) emotion.SetEmotion9();
        if (bone != null) bone.SetEmotion9();
        if (head != null) head.SetEmotion9();
    }
    public void SetEmotion10()
    {
        custom_emotion = false;
        if (!mask[GameSetting.SkinPosition.TypeEmotion])
            Emotion.material.SetTexture("_MainTex", default_emotions[10]);
        if (emotion != null) emotion.SetEmotion10();
        if (bone != null) bone.SetEmotion10();
        if (head != null) head.SetEmotion10();
    }
    public void SetEmotion11()
    {
        custom_emotion = false;
        if (!mask[GameSetting.SkinPosition.TypeEmotion])
            Emotion.material.SetTexture("_MainTex", default_emotions[11]);
        if (emotion != null) emotion.SetEmotion11();
        if (bone != null) bone.SetEmotion11();
        if (head != null) head.SetEmotion11();
    }
}
