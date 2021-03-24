using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DonImageScript : MonoBehaviour
{
    public RawImage Image;
    public Canvas Canvas;
    public cakeslice.Outline Outline;
    public Animation ReplayAnimation;
    public TooSimpleFramework.UI.OutlineEx OutlineEx;
    public Material Material;

    private bool miss;
    private bool balloon;
    private bool max;
    private Vector3 init_positon;

    private void Start()
    {
        if (ReplayAnimation != null && SceneManager.GetActiveScene().name.Contains("DualGame"))
            ReplayAnimation.enabled = false;

        if (GameSetting.Config.Outline)
            Image.material = null;

        init_positon = transform.localPosition;
    }

    public void SetMiss(bool miss)
    {
        this.miss = miss;
        if (!balloon)
            Image.color = miss ? new Color(.36f, 0.36f, 0.36f) : new Color(1, 1, 1);

        if (GameSetting.Config.Outline)
            Image.material = null;
        else
            Image.material.SetFloat("Light", 1);
    }

    public void SetBalloon(bool balloon)
    {
        this.balloon = balloon;
        Image.color = balloon ? new Color(1, 1, 1) : miss ? new Color(.36f, 0.36f, 0.36f) : new Color(1, 1, 1);
        if (GameSetting.Config.Outline)
        {
            Image.material = max ? Material : null;
        }
        else
        {
            Image.material.SetFloat("Light", !balloon && max ? 1.2f : 1);
            OutlineEx.SetColor(!balloon && max ? new Color(0.23f, 0.23f, 0.2f) : new Color(0, 0, 0));
        }

        Canvas.sortingLayerName = balloon ? "JumpOut" : "Default";
        transform.localPosition = balloon ? new Vector3(0, 0, -2) : init_positon;
    }

    public void SetMax(bool max)
    {
        this.max = max;
        if (!balloon)
        {
            if (GameSetting.Config.Outline)
            {
                Image.material = max ? Material : null;
            }
            else
            {
                Image.material.SetFloat("Light", max ? 1.2f : 1);
                OutlineEx.SetColor(max ? new Color(0.23f, 0.23f, 0.2f) : new Color(0, 0, 0));
            }
        }
    }
}
