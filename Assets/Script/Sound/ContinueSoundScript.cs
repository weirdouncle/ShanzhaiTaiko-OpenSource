using System.Collections;
using UnityEngine;

public class ContinueSoundScript : MonoBehaviour
{
    public AudioSource Audio;
    public AudioClip[] Clips;
    void Start()
    {
        StartCoroutine(Play());
    }

    IEnumerator Play()
    {
        foreach (AudioClip clip in Clips)
        {
            Audio.clip = clip;
            Audio.Play();
            float time = clip.length;
            yield return new WaitForSeconds(time);
        }

        Destroy(gameObject);
    }
}
