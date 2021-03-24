using UnityEngine;

public class EmotionScript : MonoBehaviour
{
    public Sprite[] Emotions;           //脸谱总共12个文件
    public MeshRenderer Face;

    protected bool init;
    protected Texture[] emotions;
    void Start()
    {
        emotions = new Texture[Emotions.Length];
        int index = 0;
        foreach (Sprite sprite in Emotions)
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

        init = true;
    }

    public virtual void SetEmotion0()
    {
        if (init)
        Face.material.SetTexture("_MainTex", emotions[0]);
    }

    public virtual void SetEmotion1()
    {
        if (init)
            Face.material.SetTexture("_MainTex", emotions[1]);
    }
    public virtual void SetEmotion2()
    {
        if (init)
            Face.material.SetTexture("_MainTex", emotions[2]);
    }
    public virtual void SetEmotion3()
    {
        if (init)
            Face.material.SetTexture("_MainTex", emotions[3]);
    }
    public virtual void SetEmotion4()
    {
        if (init)
            Face.material.SetTexture("_MainTex", emotions[4]);
    }
    public virtual void SetEmotion5()
    {
        if (init)
            Face.material.SetTexture("_MainTex", emotions[5]);
    }
    public virtual void SetEmotion6()
    {
        if (init)
            Face.material.SetTexture("_MainTex", emotions[6]);
    }
    public virtual void SetEmotion7()
    {
        if (init)
            Face.material.SetTexture("_MainTex", emotions[7]);
    }

    public virtual void SetEmotion8()
    {
        if (init)
            Face.material.SetTexture("_MainTex", emotions[8]);
    }
    public virtual void SetEmotion9()
    {
        if (init)
            Face.material.SetTexture("_MainTex", emotions[9]);
    }
    public virtual void SetEmotion10()
    {
        if (init)
            Face.material.SetTexture("_MainTex", emotions[10]);
    }
    public virtual void SetEmotion11()
    {
        if (init)
            Face.material.SetTexture("_MainTex", emotions[11]);
    }
}
