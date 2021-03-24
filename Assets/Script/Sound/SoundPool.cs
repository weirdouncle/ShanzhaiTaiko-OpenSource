using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundPool : MonoBehaviour
{
    public CriAtomSource Don;
    public CriAtomSource Ka;
    public CriAtomSource Don2;
    public CriAtomSource Ka2;
    public float AndriodAudioVolume = 0.7f;

    private int don;
    private int ka;
    private int sound_type;
    private int sound_type_2p;
    void Start()
    {
        /*
        MonoBehaviour[] scripts = Object.FindObjectsOfType<CriAtom>();

        foreach (MonoBehaviour item in scripts)
        {
            //因为直接输出item.ToString()的值为  游戏物体名称(脚本名称)
            Debug.Log(item.gameObject.name + ": " + item.ToString().Split(new char[] { '(', ')' })[1]);
        }
        */
#if !UNITY_ANDROID
        sound_type = GameSetting.DrumTypes[GameSetting.Config.DrumSoundType];
        if (GameSetting.Mode == CommonClass.PlayMode.Replay)
        {
            sound_type = GameSetting.DrumTypes[GameSetting.Replay.Config[GameSetting.Config.ScoreMode][(int)GameSetting.Difficulty].DrumSoundType];
        }
        else if (GameSetting.Mode == CommonClass.PlayMode.PlayWithReplay)
        {
            sound_type_2p = GameSetting.DrumTypes[GameSetting.Replay.Config[GameSetting.Config.ScoreMode][(int)GameSetting.Difficulty].DrumSoundType];
            Don.pan3dAngle = Ka.pan3dAngle = -90;
            Don2.pan3dAngle = Ka2.pan3dAngle = 90;
        }
        else if (SceneManager.GetActiveScene().name.Contains("DualGame"))
        {
            Don.pan3dAngle = Ka.pan3dAngle = -90;
            Don2.pan3dAngle = Ka2.pan3dAngle = 90;
        }
#endif
        SetVolume();
    }

    public void PlaySound(bool don)
    {
#if !UNITY_ANDROID
        if (don)
            Don.Play(sound_type);
        else
            Ka.Play(sound_type);
#else
        if (don)
            Don.Play();
        else
            Ka.Play();
#endif
    }

    public void PlaySound2(bool don)
    {
        if (don)
            Don2.Play(sound_type_2p);
        else
            Ka2.Play(sound_type_2p);
    }

    private void SetVolume()
    {
        float volum = GameSetting.Config.EffectVolume;
        Don.volume = volum;
        Ka.volume = volum;
    }
}
