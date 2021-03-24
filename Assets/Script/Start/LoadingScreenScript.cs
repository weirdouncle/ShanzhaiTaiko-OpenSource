using CommonClass;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenScript : MonoBehaviour
{
    public static event PlayDelegate Play;
    public delegate void PlayDelegate();

    public Animator Animator;
    public Text Text;
    public Text Sub;

    public static GameObject Instance = null;

    private bool assets_load;
    private bool notes_load;
    void Start()
    {
        if (Instance == null)
        {
            Instance = gameObject;
            DontDestroyOnLoad(gameObject);
            LoaderScript.Play += OnStart;
            SongDiffSelectScript.Play += OnHide;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        LoaderScript.Play -= OnStart;
        SongDiffSelectScript.Play -= OnHide;
    }

    private void OnStart()
    {
        StartCoroutine(DelayOpen());
    }


    IEnumerator DelayOpen()
    {
        yield return new WaitForSeconds(0.5f);
        Animator.SetTrigger("Open");
    }

    private void OnHide(string version, string level)
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        Text.text = string.Format("{0} {1}", GameSetting.Translate(version), GameSetting.Translate(level));
        Sub.text = string.Empty;

        Animator.SetTrigger("Hide");
    }

    private void OnHide(SongInfo info)
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        Text.text = info.Title;
        Sub.text = info.SubTitle?? string.Empty;

        Animator.SetTrigger("Hide");
    }

    public void OnPlay()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Contains("Dojo")) return;

        Play?.Invoke();

        foreach (NoteSoundScript script in LoaderScript.Notes.Values)
        {
            script.StartMoving();
        }
        foreach (ChapterLineScript script in LoaderScript.Lines)
        {
            script.StartMoving();
        }
        foreach (NoteSoundScript script in LoaderScript.Notes2P.Values)
        {
            script.StartMoving();
        }
        foreach (ChapterLineScript script in LoaderScript.Lines2P)
        {
            script.StartMoving();
        }
    }

    public void OnEnd()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Contains("Dojo"))
        {
            Play?.Invoke();
            gameObject.SetActive(false);
        }
        else
            gameObject.SetActive(false);
    }
}
