using System.Collections.Generic;
using UnityEngine;

public class MetaDonScript : MonoBehaviour
{
    public SkinnedMeshRenderer[] Meshes;
    public SkinnedMeshRenderer RobotFace;
    public SkinnedMeshRenderer Body;
    public SkinnedMeshRenderer FrontBack;
    public Sprite[] RobotEmotions;           //脸谱总共12个文件

    private Color origin_face = new Color(0.973f, 0.282f, 0.157f);
    private Color origin_skin = new Color(0.976f, 0.937f, 0.875f);
    private Color origin_face_2p = new Color(0.157f, 0.754f, 0.765f);
    private Color origin_skin_2p = new Color(0.976f, 0.937f, 0.875f);

    private Texture[] robot_emotions;
    private bool init;
    public void Init(SkinnedMeshRenderer bone, bool player2)
    {
        robot_emotions = new Texture[RobotEmotions.Length];
        int index = 0;
        foreach (Sprite sprite in RobotEmotions)
        {
            var targetTex = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            var pixels = sprite.texture.GetPixels(
                (int)sprite.textureRect.x,
                (int)sprite.textureRect.y,
                (int)sprite.textureRect.width,
                (int)sprite.textureRect.height);
            targetTex.SetPixels(pixels);

            targetTex.Apply();
            robot_emotions[index] = targetTex;
            index++;
        }

        //重绑骨骼
        foreach (SkinnedMeshRenderer skin in Meshes)
            skin.bones = bone.bones;

        //身体颜色
        Body.material.SetColor("_Blue", !player2 ? origin_skin : origin_skin_2p);
        FrontBack.material.SetColor("_Green", !player2 ? origin_face : origin_face_2p);
        FrontBack.material.SetColor("_Blue", !player2 ? origin_skin : origin_skin_2p);

        gameObject.SetActive(false);
        init = true;
    }


    public void SetEmotion0()
    {
        if (init)
            RobotFace.material.SetTexture("_MainTex", robot_emotions[0]);
    }

    public void SetEmotion1()
    {
        if (init)
            RobotFace.material.SetTexture("_MainTex", robot_emotions[1]);
    }
    public void SetEmotion2()
    {
        if (init)
            RobotFace.material.SetTexture("_MainTex", robot_emotions[2]);
    }
    public void SetEmotion3()
    {
        if (init)
            RobotFace.material.SetTexture("_MainTex", robot_emotions[3]);
    }
    public void SetEmotion4()
    {
        if (init)
            RobotFace.material.SetTexture("_MainTex", robot_emotions[4]);
    }
    public void SetEmotion5()
    {
        if (init)
            RobotFace.material.SetTexture("_MainTex", robot_emotions[5]);
    }
    public void SetEmotion6()
    {
        if (init)
            RobotFace.material.SetTexture("_MainTex", robot_emotions[6]);
    }
    public void SetEmotion7()
    {
        if (init)
            RobotFace.material.SetTexture("_MainTex", robot_emotions[7]);
    }

    public void SetEmotion8()
    {
        if (init)
            RobotFace.material.SetTexture("_MainTex", robot_emotions[8]);
    }
    public void SetEmotion9()
    {
        if (init)
            RobotFace.material.SetTexture("_MainTex", robot_emotions[9]);
    }
    public void SetEmotion10()
    {
        if (init)
            RobotFace.material.SetTexture("_MainTex", robot_emotions[10]);
    }
    public void SetEmotion11()
    {
        if (init)
            RobotFace.material.SetTexture("_MainTex", robot_emotions[11]);
    }
}
