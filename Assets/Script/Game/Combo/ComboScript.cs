using UnityEngine;
using UnityEngine.SceneManagement;

public class ComboScript : MonoBehaviour
{
    public SpriteRenderer[] Characters;
    public SpriteNumberScript Number;
    public Animator Animator;

    void Start()
    {
        Characters[0].sprite = SettingLoader.Combos[1];
        if (Characters.Length > 1)
            Characters[1].sprite = SettingLoader.Combos[2];
    }

    public void SetCombo(int combo)
    {
        if (combo != 0)
            Animator.SetTrigger("add");

            Number.Count = combo;

        if (combo < 10)
        {
            foreach (SpriteRenderer chara in Characters)
                chara.gameObject.SetActive(false);

            Number.gameObject.SetActive(false);
        }
        else if (combo == 10)
        {
            Characters[0].gameObject.SetActive(true);
            Number.gameObject.SetActive(true);
        }
        else if (combo == 100)
        {
            if (Characters.Length > 1)
            {
                Characters[0].gameObject.SetActive(false);
                Characters[1].gameObject.SetActive(true);
            }
        }

        string scene = SceneManager.GetActiveScene().name;
#if UNITY_ANDROID
        Number.transform.localScale = combo >= 1000 ? new Vector3(0.85f, 1, 1) : new Vector3(1, 1, 1);
#else
        if (scene.Contains("Nijiiro") || scene.Contains("Dojo"))
            Number.transform.localScale = combo >= 1000 ? new Vector3(0.85f, 1, 1) : new Vector3(1, 1, 1);
#endif
    }
}
