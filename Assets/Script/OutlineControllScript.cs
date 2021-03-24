using UnityEngine;
using UnityEngine.UI;
using UnityFx.Outline;

public class OutlineControllScript : MonoBehaviour
{
    public OutlineEffect[] Outlines;
    public OutlineBuilder[] Builders;
    public TooSimpleFramework.UI.OutlineEx[] OutlineSofts;

    private void Awake()
    {
#if !UNITY_ANDROID
        if (!GameSetting.Config.Outline)
        {
            foreach (OutlineEffect outline in Outlines)
                outline.enabled = false;

            foreach (OutlineBuilder builder in Builders)
                builder.enabled = false;

            foreach (TooSimpleFramework.UI.OutlineEx builder in OutlineSofts)
                builder.enabled = true;
        }
        else
        {
            foreach (TooSimpleFramework.UI.OutlineEx builder in OutlineSofts)
            {
                builder.enabled = false;
                RawImage raw = builder.GetComponent<RawImage>();
                raw.material = null;
            }
        }
#else
        foreach (OutlineEffect outline in Outlines)
            outline.enabled = false;

        foreach (OutlineBuilder builder in Builders)
            builder.enabled = false;

        foreach (TooSimpleFramework.UI.OutlineEx builder in OutlineSofts)
            builder.enabled = true;
#endif
    }
}
