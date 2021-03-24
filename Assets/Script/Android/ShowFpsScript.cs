using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ShowFpsScript : MonoBehaviour
{
    public Text Text;

    private float updateInterval = 1.0f;
    private float lastInterval;
    private int frames = 0;
    private bool counting = false;

    private float lowest = 900;
    private int counter = 0;
    private float amount = 0;
    void Start()
    {
#if (!UNITY_ANDROID && !UNITY_EDITOR)
        BasicInputScript.Input.Player.F3.performed += Activate;
        gameObject.SetActive(false);
        
        if (GameSetting.Config.DirectInput)
            BasicInputScript.KeyInvoke += DirectKey;
        counting = true;
#else
        LoaderScript.Play += OnStart;
        GloableAniControllerScript.EndPlay += EndPlay;
#endif
        lastInterval = Time.realtimeSinceStartup;
    }

    private void OnDestroy()
    {
        LoaderScript.Play -= OnStart;
        GloableAniControllerScript.EndPlay -= EndPlay;
        BasicInputScript.KeyInvoke -= DirectKey;
    }

    private void OnStart()
    {
        StartCoroutine(DelayOpen());
    }
    IEnumerator DelayOpen()
    {
        yield return new WaitForSeconds(1);
        counting = true;
    }

    private void EndPlay()
    {
        counting = false;
        Debug.Log(string.Format("lowest {0} average {1}", lowest, amount / counter));
    }

    private void DirectKey(GameSetting.KeyType key, bool press)
    {
        if (key == GameSetting.KeyType.KeyNone)
            gameObject.SetActive(!gameObject.activeSelf);
    }
    private void Activate(InputAction.CallbackContext obj)
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    // Update is called once per frame
    void Update()
    {
        if (counting && GameSetting.Config != null && GameSetting.Config.DirectInput || InputSystem.settings.updateMode == InputSettings.UpdateMode.ProcessEventsInDynamicUpdate)
        {
            frames++;
            var timeNow = Time.realtimeSinceStartup;
            if (timeNow > lastInterval + updateInterval)
            {
                float fps = frames / (timeNow - lastInterval);
                Text.text = fps.ToString("#0.0");
                frames = 0;
                lastInterval = timeNow;
#if UNITY_EDITOR
                if (fps < lowest)
                {
                    lowest = fps;
                    if (fps < 100) UnityEditor.EditorApplication.isPaused = true;
                }
                counter++;
                amount += fps;
#endif
            }
        }
    }

#if !UNITY_ANDROID
    private void FixedUpdate()
    {
        if (counting && GameSetting.Config != null && !GameSetting.Config.DirectInput && InputSystem.settings.updateMode == InputSettings.UpdateMode.ProcessEventsInFixedUpdate)
        {
            frames++;
            var timeNow = Time.realtimeSinceStartup;
            if (timeNow > lastInterval + updateInterval)
            {
                float fps = frames / (timeNow - lastInterval);
                Text.text = fps.ToString("#0.0");
                frames = 0;
                lastInterval = timeNow;
#if UNITY_EDITOR
                if (fps < lowest)
                {
                    lowest = fps;
                    if (fps < 90) UnityEditor.EditorApplication.isPaused = true;
                }
                counter++;
                amount += fps;
#endif
            }
        }
    }
#endif
}
