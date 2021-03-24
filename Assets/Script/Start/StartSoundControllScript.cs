using System.Collections;
using UnityEngine;

public class StartSoundControllScript : MonoBehaviour
{
    public enum SoundType
    {
        Don,
        Ka,
        Switch,
        Cancel,
        Option,
        PopUp,
        Branch,
        Miss1p,
        Miss2p,
        Retry
    }
    public CriAtomSource Source;

    private WaitForSecondsRealtime wait = new WaitForSecondsRealtime(0.08f);
    private SoundType last = SoundType.Retry;
    private bool sound = true;

    private static GameObject Instance = null;
    void Start()
    {
        if (Instance == null)
        {
            //InputSystem.pollingFrequency = 120;

            Instance = gameObject;
            DontDestroyOnLoad(gameObject);
            SongDiffSelectScript.PlaySelectedSound += PlaySound;
            SongScanScript.PlaySelectedSound += PlaySound;
            GameConfigScript.PlaySelectedSound += PlaySound;
            InputKeyListningScript.PlaySelectedSound += PlaySound;
            OptionScript.PlaySelectedSound += PlaySound;
            PlayOptionScript.PlaySelectedSound += PlaySound;
            GameResultScript.PlaySelectedSound += PlaySound;
            MainSceenScript.PlaySelectedSound += PlaySound;
            CosChangeScript.PlaySelectedSound += PlaySound;
            GloableAniControllerScript.PlaySelectedSound += PlaySound;
            GameModeScript.PlaySelectedSound += PlaySound;
            ResolutionScript.PlaySelectedSound += PlaySound;
            AndroidConfigScript.PlaySelectedSound += PlaySound;
        }
        else
            Destroy(gameObject);
    }

    private void OnDestroy()
    {
        SongDiffSelectScript.PlaySelectedSound -= PlaySound;
        SongScanScript.PlaySelectedSound -= PlaySound;
        GameConfigScript.PlaySelectedSound -= PlaySound;
        InputKeyListningScript.PlaySelectedSound -= PlaySound;
        OptionScript.PlaySelectedSound -= PlaySound;
        PlayOptionScript.PlaySelectedSound -= PlaySound;
        GameResultScript.PlaySelectedSound -= PlaySound;
        MainSceenScript.PlaySelectedSound -= PlaySound;
        CosChangeScript.PlaySelectedSound -= PlaySound;
        GloableAniControllerScript.PlaySelectedSound -= PlaySound;
        GameModeScript.PlaySelectedSound -= PlaySound;
        ResolutionScript.PlaySelectedSound -= PlaySound;
        AndroidConfigScript.PlaySelectedSound -= PlaySound;
    }

    private void PlaySound(SoundType type)
    {
        if (!sound && type == last) return;
        last = type;
        switch (type)
        {
            case SoundType.Don:
                //Instantiate(Don_pre);
                Source.cueName = "Decide";
                Source.Play();
                break;
            case SoundType.Ka:
                //Instantiate(Ka_pre);
                Source.cueName = "hit";
                Source.Play();
                break;
            case SoundType.Switch:
                //Instantiate(Switch_pre);
                Source.cueName = "switch";
                Source.Play();
                break;
            case SoundType.Cancel:
                //Instantiate(Cancel_pre);
                Source.cueName = "cancel";
                Source.Play();
                break;
            case SoundType.Option:
                //Instantiate(Option_pre);
                Source.cueName = "option";
                Source.Play();
                break;
            case SoundType.PopUp:
                //Instantiate(PopUp_pre);
                Source.cueName = "popup";
                Source.Play();
                break;
            case SoundType.Branch:
                //Instantiate(Branch_pre);
                Source.cueName = "branch";
                Source.Play();
                break;
            case SoundType.Retry:
                Source.cueName = "retry";
                Source.Play();
                break;
            case SoundType.Miss1p:
                Source.cueName = "miss1p";
                Source.Play();
                break;
            case SoundType.Miss2p:
                Source.cueName = "miss2p";
                Source.Play();
                break;
        }

        StartCoroutine(DelayAvailable());
    }

    IEnumerator DelayAvailable()
    {
        sound = false;
        yield return wait;
        sound = true;
    }
}
