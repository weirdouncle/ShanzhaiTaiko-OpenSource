using UnityEngine;

public class Emotion2Script : EmotionScript
{
    public SkinnedMeshRenderer Face2;
    public override void SetEmotion0()
    {
        if (init)
            Face2.material.SetTexture("_MainTex", emotions[0]);
    }

    public override void SetEmotion1()
    {
        if (init)
            Face2.material.SetTexture("_MainTex", emotions[1]);
    }
    public override void SetEmotion2()
    {
        if (init)
            Face2.material.SetTexture("_MainTex", emotions[2]);
    }
    public override void SetEmotion3()
    {
        if (init)
            Face2.material.SetTexture("_MainTex", emotions[3]);
    }
    public override void SetEmotion4()
    {
        if (init)
            Face2.material.SetTexture("_MainTex", emotions[4]);
    }
    public override void SetEmotion5()
    {
        if (init)
            Face2.material.SetTexture("_MainTex", emotions[5]);
    }
    public override void SetEmotion6()
    {
        if (init)
            Face2.material.SetTexture("_MainTex", emotions[6]);
    }
    public override void SetEmotion7()
    {
        if (init)
            Face2.material.SetTexture("_MainTex", emotions[7]);
    }

    public override void SetEmotion8()
    {
        if (init)
            Face2.material.SetTexture("_MainTex", emotions[8]);
    }
    public override void SetEmotion9()
    {
        if (init)
            Face2.material.SetTexture("_MainTex", emotions[9]);
    }
    public override void SetEmotion10()
    {
        if (init)
            Face2.material.SetTexture("_MainTex", emotions[10]);
    }
    public override void SetEmotion11()
    {
        if (init)
            Face2.material.SetTexture("_MainTex", emotions[11]);
    }
}
