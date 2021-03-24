using System.Collections;
using UnityEngine;

public class NotesAuioScript : MonoBehaviour
{
    //public static event PlayFinishDelegate PlayFinish;
    //public delegate void PlayFinishDelegate(NotesAuioScript sound);

    public AudioSource Audio;
    public int Type;
    public void PlaySound()
    {
        //StartCoroutine(AudioPlayFinished());
        Audio.Play();
    }
    /*
    private IEnumerator AudioPlayFinished()
    {
        Audio.Play();
        yield return new WaitForSeconds(Audio.clip.length);
        PlayFinish?.Invoke(this);
    }
    */
}
