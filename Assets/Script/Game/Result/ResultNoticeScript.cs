using UnityEngine;

public class ResultNoticeScript : MonoBehaviour
{
    public CriAtomSource Audio;
    public CriAtomSource Audio2;

    private bool send;

    public void SetAudioAngle(bool left)
    {
        if (left)
        {
            Audio.pan3dAngle = -90;
            if (Audio2 != null)
                Audio2.pan3dAngle = -90;
        }
        else
        {
            Audio.pan3dAngle = 90;
            if (Audio2 != null)
                Audio2.pan3dAngle = 90;
        }
    }
    public void PlayResultAudio()
    {
        Audio.Play();
    }
    public void PlayResultAudio2()
    {
        Audio2.Play();
    }

    public void Send()
    {
        if (!send)
        {
            send = true;
            GloableAniControllerScript.Instance.EndPlayByNotice();
        }
    }
}
