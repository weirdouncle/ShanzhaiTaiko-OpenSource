using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionSoundScript : MonoBehaviour
{
    public AudioSource Audio;
    void Start()
    {
        StartCoroutine(Destroy(Audio.clip.length));
    }

    IEnumerator Destroy(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}
