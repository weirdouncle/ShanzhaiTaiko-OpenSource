using System;
using UnityEngine;
using static GameSetting;
using static UnityEngine.InputSystem.InputAction;
using Keyboard = SharpDX.DirectInput.Keyboard;
using SharpDX.DirectInput;

public delegate void KeyInvokeDelegate(KeyType key, bool pressed);
public delegate void KeyInvoke2PDelegate(KeyType key, bool pressed);
public class BasicInputScript : MonoBehaviour
{
    public static event KeyInvokeDelegate KeyInvoke;
    public static event KeyInvoke2PDelegate KeyInvoke2P;

    public static WASDInput Input;
    public static Keyboard Keyboard;
    private static DirectInput directInput;

    void Awake()
    {
        if (GameSetting.Config.DirectInput && Keyboard == null)
        {
            directInput = new DirectInput();
            Keyboard = new Keyboard(directInput);
            //IntPtr hwd = GetProcessWnd();
            //Debug.Log(hwd.ToString());
            //Keyboard.SetCooperativeLevel(hwd, CooperativeLevel.Background | CooperativeLevel.NonExclusive | CooperativeLevel.NoWinKey);
            Keyboard.Properties.BufferSize = 128;
            Keyboard.Acquire();
        }

        Input = new WASDInput();
        Input.Enable();
    }
    void Start()
    {
        if (!GameSetting.Config.DirectInput)
            Input.Player.ScreenShot.performed += ScreenShot;

        InputKeyListningScript.RebindKeys += Rebingkeys;
        Rebingkeys();
    }

    private void Rebingkeys()
    {
        if (GameSetting.Config.GamePadKeys2P.TryGetValue(KeyType.LeftDon, out string left_path) && left_path.Contains("Keyboard"))
        {
            string[] str = left_path.Split('/');
            ld_2p = str[str.Length -1];
        }
        if (GameSetting.Config.GamePadKeys2P.TryGetValue(KeyType.RightDon, out string right_path) && right_path.Contains("Keyboard"))
        {
            string[] str = right_path.Split('/');
            rd_2p = str[str.Length - 1];
        }
        if (GameSetting.Config.GamePadKeys2P.TryGetValue(KeyType.LeftKa, out string left_ka) && left_ka.Contains("Keyboard"))
        {
            string[] str = left_ka.Split('/');
            lk_2p = str[str.Length - 1];
        }
        if (GameSetting.Config.GamePadKeys2P.TryGetValue(KeyType.RightKa, out string right_ka) && right_ka.Contains("Keyboard"))
        {
            string[] str = right_ka.Split('/');
            rk_2p = str[str.Length - 1];
        }
    }

    private string ld_2p;
    private string rd_2p;
    private string lk_2p;
    private string rk_2p;

    private void ScreenShot(CallbackContext obj)
    {
        ScreenShot();
    }

    private void ScreenShot()
    {
        if (!Application.isFocused) return;
        int hour = DateTime.Now.Hour;
        int minute = DateTime.Now.Minute;
        int second = DateTime.Now.Second;
        int year = DateTime.Now.Year;
        int month = DateTime.Now.Month;
        int day = DateTime.Now.Day;

        ScreenCapture.CaptureScreenshot(string.Format("{6}/{0}{1}{2}{3}{4}{5}.png", year, month, day, hour, minute, second, Environment.CurrentDirectory));
    }

    private void OnDestroy()
    {
        InputKeyListningScript.RebindKeys -= Rebingkeys;
        Input.Player.ScreenShot.performed -= ScreenShot;
        Input.Disable();
    }

    public static bool Refresh { get; set; } = true;

    private void OnApplicationFocus(bool focus)
    {
        if (Keyboard != null)
        {
            if (!focus)
                Keyboard.Unacquire();
            else
                Keyboard.Acquire();
        }
    }

    private void Update()
    {
        if (Refresh && GameSetting.Config.DirectInput && Application.isFocused)
        {
            Keyboard.Poll();
            foreach (var state in Keyboard.GetBufferedData())
            {
                if (state.Key == SharpDX.DirectInput.Key.F1 && state.IsPressed)
                {
                    KeyInvoke?.Invoke(KeyType.Config, true);
                }
                else if (state.Key == SharpDX.DirectInput.Key.F2 && state.IsPressed)
                {
                    KeyInvoke?.Invoke(KeyType.Random, true);
                }
                else if (state.Key == SharpDX.DirectInput.Key.F3 && state.IsPressed)
                {
                    KeyInvoke?.Invoke(KeyType.KeyNone, true);
                }
                else if (state.Key == SharpDX.DirectInput.Key.PrintScreen && state.IsPressed)
                {
                    ScreenShot();
                }
                else if (state.Key == SharpDX.DirectInput.Key.Up && state.IsPressed)
                {
                    KeyInvoke?.Invoke(KeyType.Up, true);
                }
                else if (state.Key == SharpDX.DirectInput.Key.Down && state.IsPressed)
                {
                    KeyInvoke?.Invoke(KeyType.Down, true);
                }
                else if (state.Key.ToString().ToLower() == GameSetting.Config.Option.Name && state.IsPressed)
                {
                    KeyInvoke?.Invoke(KeyType.Option, true);
                }
                else if ((state.Key == SharpDX.DirectInput.Key.Return || state.Key == SharpDX.DirectInput.Key.NumberPadEnter) && state.IsPressed)
                    KeyInvoke?.Invoke(KeyType.Confirm, false);
                else if (state.Key == SharpDX.DirectInput.Key.Escape && state.IsReleased)
                    KeyInvoke?.Invoke(KeyType.Escape, false);
                else if (state.Key == SharpDX.DirectInput.Key.Left)
                {
                    if (state.IsPressed)
                        KeyInvoke?.Invoke(KeyType.Left, true);
                    else if (state.IsReleased)
                        KeyInvoke?.Invoke(KeyType.Left, false);
                }
                else if (state.Key == SharpDX.DirectInput.Key.Right)
                {
                    if (state.IsPressed)
                        KeyInvoke?.Invoke(KeyType.Right, true);
                    else if (state.IsReleased)
                        KeyInvoke?.Invoke(KeyType.Right, false);
                }
                else if (state.Key.ToString().ToLower() == GameSetting.Config.DonLeft.Name && state.IsPressed)
                {
                    KeyInvoke?.Invoke(KeyType.LeftDon, true);
                    KeyInvoke?.Invoke(KeyType.Cancel, true);
                }
                else if (state.Key.ToString().ToLower() == GameSetting.Config.DonRight.Name && state.IsPressed)
                {
                    KeyInvoke?.Invoke(KeyType.RightDon, true);
                    //KeyInvoke?.Invoke(KeyType.Confirm, true);
                }
                else if (state.Key.ToString().ToLower() == GameSetting.Config.KaLeft.Name)
                {
                    if (state.IsPressed)
                    {
                        KeyInvoke?.Invoke(KeyType.LeftKa, true);
                        KeyInvoke?.Invoke(KeyType.Left, true);
                    }
                    else if (state.IsReleased)
                    {
                        KeyInvoke?.Invoke(KeyType.Left, false);
                    }
                }
                else if (state.Key.ToString().ToLower() == GameSetting.Config.KaRight.Name)
                {
                    if (state.IsPressed)
                    {
                        KeyInvoke?.Invoke(KeyType.RightKa, true);
                        KeyInvoke?.Invoke(KeyType.Right, true);
                    }
                    else if (state.IsReleased)
                    {
                        KeyInvoke?.Invoke(KeyType.Right, false);
                    }
                }
                else if (state.IsPressed && state.Key.ToString().ToLower() == ld_2p)
                {
                    KeyInvoke2P?.Invoke(KeyType.LeftDon, true);
                }
                else if (state.IsPressed && state.Key.ToString().ToLower() == rd_2p)
                {
                    KeyInvoke2P?.Invoke(KeyType.RightDon, true);
                }
                else if (state.IsPressed && state.Key.ToString().ToLower() == lk_2p)
                {
                    KeyInvoke2P?.Invoke(KeyType.LeftKa, true);
                }
                else if (state.IsPressed && state.Key.ToString().ToLower() == rk_2p)
                {
                    KeyInvoke2P?.Invoke(KeyType.RightKa, true);
                }
            }
        }
    }

    public static void ChangeKeyBoard(KeyType type)
    {
        Input.ChangeKeyBoard(type);
    }
    public static void ChangeGamepad(KeyType type)
    {
        Input.ChangeGamePad(type);
    }

    public static void ChangeKeyBoard2P(KeyType type)
    {
        Input.ChangeKeyBoard2P(type);
    }
}
