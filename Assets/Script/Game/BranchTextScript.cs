using UnityEngine;

public class BranchTextScript : MonoBehaviour
{
    public SpriteRenderer[] Images;

    void Start()
    {
        for (int i = 0; i < Images.Length; i++)
            Images[i].sprite = SettingLoader.TextBranches[i];
    }
}
