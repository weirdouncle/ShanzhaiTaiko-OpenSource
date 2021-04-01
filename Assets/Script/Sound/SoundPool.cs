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
        Don.cueName = Ka.cueName = GameSetting.DrumTypes[GameSetting.Config.DrumSoundType].ToString();
        if (Don2 != null) Don2.cueName = Ka2.cueName = "0";
        if (GameSetting.Mode == CommonClass.PlayMode.Replay)
        {
            Don.cueName = Ka.cueName = GameSetting.DrumTypes[GameSetting.Replay.Config[GameSetting.Config.ScoreMode][(int)GameSetting.Difficulty].DrumSoundType].ToString();
        }
        else if (GameSetting.Mode == CommonClass.PlayMode.PlayWithReplay)
        {
            Don2.cueName = Ka2.cueName = GameSetting.DrumTypes[GameSetting.Replay.Config[GameSetting.Config.ScoreMode][(int)GameSetting.Difficulty].DrumSoundType].ToString();
            Don.pan3dAngle = Ka.pan3dAngle = -90;
            Don2.pan3dAngle = Ka2.pan3dAngle = 90;
        }
        else if (SceneManager.GetActiveScene().name.Contains("DualGame"))
        {
            Don.pan3dAngle = Ka.pan3dAngle = -90;
            Don2.pan3dAngle = Ka2.pan3dAngle = 90;
        }
#else
        Don.cueName = "don";
        Ka.cueName = "ka";
#endif
        InitSound();
    }

    public void PlaySound(bool don)
    {
        if (don)
            Don.PlayDirectly();
        else
            Ka.PlayDirectly();
    }

    public void PlaySound2(bool don)
    {
        if (don)
            Don2.PlayDirectly();
        else
            Ka2.PlayDirectly();
    }

    private void InitSound()
    {
        Don.volume = Ka.volume = GameSetting.Config.EffectVolume;
        Don.Init();
        Ka.Init();
        if (Don2 != null)
        {
            Don2.Init();
            Ka2.Init();
        }
    }
}
