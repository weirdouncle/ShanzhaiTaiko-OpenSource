using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;
using static GameSetting;
using static UnityEngine.InputSystem.InputAction;

public class InputKeyListningScript : MonoBehaviour
{
    public static event PlaySelectedSoundDelegate PlaySelectedSound;

    public static event RebindKeysDelegate RebindKeys;
    public delegate void RebindKeysDelegate();

    public SettingButtonScript Switch;
    public SettingButtonScript[] KeyboardButtons;
    public SettingButtonScript[] PadButtons;
    public SettingButtonScript[] Buttons2P;
    public Transform[] Devices;
    public GameObject[] ButtonImas;
    public GameConfigScript Config;
    public Text Title;
    public Text[] Options;

    private bool listning = false;
    private bool inputing = false;
    private CurrentKey current = CurrentKey.KeyKeyboard;
    private int index = -1;

    private InputAction m_buttonAction;
    private InputAction m_dPadAction;
    private InputAction m_stickMoveAction;

    private enum CurrentKey
    {
        KeyKeyboard,
        KeyPad,
        Key2P,
    }

    void Start()
    {
        Title.text = GameSetting.Translate("key config");

        Options[0].text = Translate("controller");
        Options[1].text = Options[10].text = Options[17].text = Translate("left don");
        Options[2].text = Options[11].text = Options[18].text = Translate("right don");
        Options[3].text = Options[12].text = Options[19].text = Translate("left ka");
        Options[4].text = Options[13].text = Options[20].text = Translate("right ka");
        Options[5].text = Options[14].text = Options[16].text = Translate("option");
        Options[6].text = Options[15].text = Options[21].text = Translate("back");

        Options[7].text = Translate("direction");
        Options[8].text = Translate("confirm");
        Options[9].text = Translate("cancel");

        Switch.MouseHavor += ChangeSelected;
        for (int i = 0; i < PadButtons.Length; i++)
            PadButtons[i].MouseHavor += ChangeSelected;

        for (int i = 0; i < KeyboardButtons.Length; i++)
            KeyboardButtons[i].MouseHavor += ChangeSelected;

        for (int i = 0; i < Buttons2P.Length; i++)
            Buttons2P[i].MouseHavor += ChangeSelected;

        m_buttonAction = new InputAction(name: "ButtonPressAction", InputActionType.PassThrough, binding: "*/<button>");
        m_buttonAction.performed += callbackContext => OnButtonPress(callbackContext);
        m_buttonAction.Enable();

        m_dPadAction = new InputAction(name: "Dpadpressaction", InputActionType.PassThrough, binding: "*/<dpad>");
        m_dPadAction.performed += callbackContext => OnDpadPress(callbackContext.control as DpadControl);
        m_dPadAction.Enable();

        m_stickMoveAction = new InputAction(name: "StickMoveAction", InputActionType.PassThrough, binding: "*/<stick>");
        m_stickMoveAction.performed += callbackContext => StickMove(callbackContext.control as StickControl);
        m_stickMoveAction.Enable();

        BasicInputScript.Input.Player.Option.performed += Click;
        BasicInputScript.Input.Player.Up.performed += Click;
        BasicInputScript.Input.Player.Down.performed += Click;
        BasicInputScript.Input.Player.Esc.performed += Click;
        BasicInputScript.Input.Player.Cancel.performed += Click;
        BasicInputScript.Input.Player.Enter.performed += Click;
        BasicInputScript.Input.Player.RightDon.performed += Click;
    }

    void OnDestroy()
    {
        m_buttonAction?.Disable();
        m_dPadAction?.Disable();
        m_stickMoveAction?.Disable();

        Switch.MouseHavor -= ChangeSelected;
        for (int i = 0; i < KeyboardButtons.Length; i++)
            KeyboardButtons[i].MouseHavor -= ChangeSelected;

        for (int i = 0; i < PadButtons.Length; i++)
            PadButtons[i].MouseHavor -= ChangeSelected;

        for (int i = 0; i < Buttons2P.Length; i++)
            Buttons2P[i].MouseHavor -= ChangeSelected;

        BasicInputScript.KeyInvoke -= DirectInput;
    }

    void OnEnable()
    {
        if (GameSetting.Config.DirectInput)
            BasicInputScript.KeyInvoke += DirectInput;
    }

    void OnDisable()
    {
        BasicInputScript.KeyInvoke -= DirectInput;
    }

    private void DirectInput(KeyType key, bool pressed)
    {
        if (listning)
        {
            if (key == KeyType.Confirm)
            {
                SetButton(index);
            }
            else if (key == KeyType.Up)
            {
                Move(true);
            }
            else if (key == KeyType.Option || key == KeyType.Down)
            {
                Move(false);
            }
            else if (key == KeyType.Escape || key == KeyType.Cancel)
            {
                Back2Normal();
            }
        }
        else if (inputing)
        {
            if (key == KeyType.Escape)
                CloseInputing();
        }
    }

    private void Click(CallbackContext context)
    {
        if (!Application.isFocused) return;
        if (listning)
        {
            if (context.action == BasicInputScript.Input.Player.Enter || (context.action == BasicInputScript.Input.Player.RightDon))
            {
                SetButton(index);
            }
            else if (context.action == BasicInputScript.Input.Player.Up)
            {
                Move(true);
            }
            else if (context.action == BasicInputScript.Input.Player.Down || context.action == BasicInputScript.Input.Player.Option)
            {
                Move(false);
            }
            else if (context.action == BasicInputScript.Input.Player.Esc || context.action == BasicInputScript.Input.Player.Cancel)
            {
                Back2Normal();
            }
        }
        else if (inputing)
        {
            if (context.action == BasicInputScript.Input.Player.Esc ||
                (context.action == BasicInputScript.Input.Player.Cancel && ((current == CurrentKey.KeyKeyboard && context.control.device != Keyboard.current)
                || (current == CurrentKey.KeyPad && context.control.device == Keyboard.current))))
            {
                CloseInputing();
            }
        }
    }

    private void CloseInputing()
    {
        RefreshKey();
        inputing = false;
        StartCoroutine(DelayListning());
    }

    IEnumerator DelayListning()
    {
        if (GameSetting.Config.DirectInput)
            BasicInputScript.Keyboard.Unacquire();

        yield return new WaitForSeconds(0.1f);
        listning = true;

        if (GameSetting.Config.DirectInput)
        {
            BasicInputScript.Keyboard.Acquire();
            BasicInputScript.Refresh = true;
        }
    }

    private void Move(bool up)
    {
        if (current == CurrentKey.KeyKeyboard)
        {
            if (up)
            {
                if (index > 0)
                {
                    ChangeSelected(index - 1);
                }
                else
                    ChangeSelected(KeyboardButtons.Length);
            }
            else
            {
                if (index < KeyboardButtons.Length)
                {
                    ChangeSelected(index + 1);
                }
                else
                    ChangeSelected(0);
            }
        }
        else if (current == CurrentKey.KeyPad)
        {
            if (up)
            {
                if (index > 0)
                {
                    ChangeSelected(index - 1);
                }
                else
                    ChangeSelected(PadButtons.Length);
            }
            else
            {
                if (index < PadButtons.Length)
                {
                    ChangeSelected(index + 1);
                }
                else
                    ChangeSelected(0);
            }
        }
        else
        {
            if (up)
            {
                if (index > 0)
                {
                    ChangeSelected(index - 1);
                }
                else
                    ChangeSelected(Buttons2P.Length);
            }
            else
            {
                if (index < Buttons2P.Length)
                {
                    ChangeSelected(index + 1);
                }
                else
                    ChangeSelected(0);
            }
        }
    }

    private void SetKeboard(ButtonControl control)
    {
        Key key = new Key(control.name, control.path);
        if (key.Name.Contains("f1") || key.Name.Contains("f2") || key.Name.Contains("f3") || key.Name.Contains("escape")
            || key.Name.Contains("rightArrow") || key.Name.Contains("leftArrow") || key.Name.Contains("upArrow")
            || key.Name.Contains("downArrow") || key.Name.Contains("numpadEnter") || key.Name.Contains("enter"))
            return;
        KeyType type = KeyType.KeyNone;
        switch (index)
        {
            case 1:
                if (key.Equals(GameSetting.Config.DonRight) || key.Equals(GameSetting.Config.KaRight) || key.Equals(GameSetting.Config.KaLeft)
                    || key.Equals(GameSetting.Config.Option))
                    return;
                type = KeyType.LeftDon;
                break;
            case 2:
                if (key.Equals(GameSetting.Config.DonLeft) || key.Equals(GameSetting.Config.KaRight) || key.Equals(GameSetting.Config.KaLeft)
                    || key.Equals(GameSetting.Config.Option))
                {
                    Debug.Log(key.Path);
                    return;
                }
                type = KeyType.RightDon;
                break;
            case 3:
                if (key.Equals(GameSetting.Config.DonLeft) || key.Equals(GameSetting.Config.KaRight) || key.Equals(GameSetting.Config.DonRight)
                    || key.Equals(GameSetting.Config.Option))
                    return;
                type = KeyType.LeftKa;
                break;
            case 4:
                if (key.Equals(GameSetting.Config.DonLeft) || key.Equals(GameSetting.Config.DonRight) || key.Equals(GameSetting.Config.KaLeft)
                    || key.Equals(GameSetting.Config.Option))
                    return;
                type = KeyType.RightKa;
                break;
            case 5:
                if (key.Equals(GameSetting.Config.DonLeft) || key.Equals(GameSetting.Config.KaRight) || key.Equals(GameSetting.Config.KaLeft)
                    || key.Equals(GameSetting.Config.DonRight))
                    return;
                type = KeyType.Option;
                break;
        }
        GameSetting.SetButton(type, key);
        BasicInputScript.ChangeKeyBoard(type);
        CloseInputing();
    }

    private void CheckConflict(Key key)
    {
        if (current == CurrentKey.KeyPad)
        {
            if (index > 3 && index < 12)
            {
                if (GameSetting.Config.GamePadKeys.TryGetValue(KeyType.LeftDon, out List<string> pathes) && pathes.Count > 0 && pathes.Contains(key.Path))
                {
                    int index = pathes.IndexOf(key.Path);
                    if ((this.index == 4 && index == 0) || (this.index == 5 && index != 0)) return;
                    if (index == 0)
                        pathes[0] = string.Empty;
                    else
                        pathes.RemoveAll(t => t == key.Path);
                    GameSetting.Config.GamePadKeys[KeyType.LeftDon] = pathes;
                    SetPadButton(KeyType.LeftDon);
                }
                if (GameSetting.Config.GamePadKeys.TryGetValue(KeyType.RightDon, out List<string> pathes1) && pathes1.Count > 0 && pathes1.Contains(key.Path))
                {
                    int index = pathes1.IndexOf(key.Path);
                    if ((this.index == 6 && index == 0) || (this.index == 7 && index != 0)) return;
                    if (index == 0)
                        pathes1[0] = string.Empty;
                    else
                        pathes1.RemoveAll(t => t == key.Path);
                    GameSetting.Config.GamePadKeys[KeyType.RightDon] = pathes1;
                    SetPadButton(KeyType.RightDon);
                }
                if (GameSetting.Config.GamePadKeys.TryGetValue(KeyType.LeftKa, out List<string> pathes3) && pathes3.Count > 0 && pathes3.Contains(key.Path))
                {
                    int index = pathes3.IndexOf(key.Path);
                    if ((this.index == 8 && index == 0) || (this.index == 9 && index != 0)) return;
                    if (index == 0)
                        pathes3[0] = string.Empty;
                    else
                        pathes3.RemoveAll(t => t == key.Path);
                    GameSetting.Config.GamePadKeys[KeyType.LeftKa] = pathes3;
                    SetPadButton(KeyType.LeftKa);
                }
                if (GameSetting.Config.GamePadKeys.TryGetValue(KeyType.RightKa, out List<string> pathes4) && pathes4.Count > 0 && pathes4.Contains(key.Path))
                {
                    int index = pathes4.IndexOf(key.Path);
                    if ((this.index == 10 && index == 0) || (this.index == 11 && index != 0)) return;
                    if (index == 0)
                        pathes4[0] = string.Empty;
                    else
                        pathes4.RemoveAll(t => t == key.Path);
                    GameSetting.Config.GamePadKeys[KeyType.RightKa] = pathes4;
                    SetPadButton(KeyType.RightKa);
                }
            }
            else
            {
                if (index != 12 && GameSetting.Config.GamePadKeys.TryGetValue(KeyType.Option, out List<string> pathes5) && pathes5.Count > 0 && pathes5[0] == key.Path)
                {
                    GameSetting.Config.GamePadKeys[KeyType.Option] = new List<string>();
                    BasicInputScript.ChangeGamepad(KeyType.Option);
                }
                if (index != 2 && GameSetting.Config.GamePadKeys.TryGetValue(KeyType.Confirm, out List<string> pathes7) && pathes7.Count > 0 && pathes7[0] == key.Path)
                {
                    GameSetting.Config.GamePadKeys[KeyType.Confirm] = new List<string>();
                    BasicInputScript.ChangeGamepad(KeyType.Confirm);
                }
                if (index != 3 && GameSetting.Config.GamePadKeys.TryGetValue(KeyType.Cancel, out List<string> pathes8) && pathes8.Count > 0 && pathes8[0] == key.Path)
                {
                    GameSetting.Config.GamePadKeys[KeyType.Cancel] = new List<string>();
                    BasicInputScript.ChangeGamepad(KeyType.Cancel);
                }
            }
        }
        else
        {
            if (GameSetting.Config.GamePadKeys.TryGetValue(KeyType.LeftDon, out List<string> pathes) && pathes.Count > 0 && pathes.Contains(key.Path))
            {
                int index = pathes.IndexOf(key.Path);
                if (index == 0)
                    pathes[0] = string.Empty;
                else
                    pathes.RemoveAll(t => t == key.Path);
                GameSetting.Config.GamePadKeys[KeyType.LeftDon] = pathes;
                SetPadButton(KeyType.LeftDon);
            }
            if (GameSetting.Config.GamePadKeys.TryGetValue(KeyType.RightDon, out List<string> pathes1) && pathes1.Count > 0 && pathes1.Contains(key.Path))
            {
                int index = pathes1.IndexOf(key.Path);
                if (index == 0)
                    pathes1[0] = string.Empty;
                else
                    pathes1.RemoveAll(t => t == key.Path);
                GameSetting.Config.GamePadKeys[KeyType.RightDon] = pathes1;
                SetPadButton(KeyType.RightDon);
            }
            if (GameSetting.Config.GamePadKeys.TryGetValue(KeyType.LeftKa, out List<string> pathes3) && pathes3.Count > 0 && pathes3.Contains(key.Path))
            {
                int index = pathes3.IndexOf(key.Path);
                if (index == 0)
                    pathes3[0] = string.Empty;
                else
                    pathes3.RemoveAll(t => t == key.Path);
                GameSetting.Config.GamePadKeys[KeyType.LeftKa] = pathes3;
                SetPadButton(KeyType.LeftKa);
            }
            if (GameSetting.Config.GamePadKeys.TryGetValue(KeyType.RightKa, out List<string> pathes4) && pathes4.Count > 0 && pathes4.Contains(key.Path))
            {
                int index = pathes4.IndexOf(key.Path);
                if (index == 0)
                    pathes4[0] = string.Empty;
                else
                    pathes4.RemoveAll(t => t == key.Path);
                GameSetting.Config.GamePadKeys[KeyType.RightKa] = pathes4;
                SetPadButton(KeyType.RightKa);
            }
            if (GameSetting.Config.GamePadKeys.TryGetValue(KeyType.Option, out List<string> pathes5) && pathes5.Count > 0 && pathes5[0] == key.Path)
            {
                GameSetting.Config.GamePadKeys[KeyType.Option] = new List<string>();
                BasicInputScript.ChangeGamepad(KeyType.Option);
            }
            if (GameSetting.Config.GamePadKeys.TryGetValue(KeyType.Confirm, out List<string> pathes7) && pathes7.Count > 0 && pathes7[0] == key.Path)
            {
                GameSetting.Config.GamePadKeys[KeyType.Confirm] = new List<string>();
                BasicInputScript.ChangeGamepad(KeyType.Confirm);
            }
            if (GameSetting.Config.GamePadKeys.TryGetValue(KeyType.Cancel, out List<string> pathes8) && pathes8.Count > 0 && pathes8[0] == key.Path)
            {
                GameSetting.Config.GamePadKeys[KeyType.Cancel] = new List<string>();
                BasicInputScript.ChangeGamepad(KeyType.Cancel);
            }
        }
    }

    private void SetButton(ButtonControl control)
    {
        Key key = new Key(control.name, control.path);
        SetButton(key);
    }
    private void SetButton(Key key)
    {
        KeyType type = KeyType.KeyNone;
        CheckConflict(key);
        switch (index)
        {
            case 2:
                type = KeyType.Confirm;
                GameSetting.Config.GamePadKeys[type] = new List<string> { key.Path };
                break;
            case 3:
                type = KeyType.Cancel;
                GameSetting.Config.GamePadKeys[type] = new List<string> { key.Path };
                break;
            case 4:
                {
                    type = KeyType.LeftDon;
                    if (GameSetting.Config.GamePadKeys.TryGetValue(type, out List<string> pathes))
                    {
                        if (pathes.Count == 0)
                            pathes.Add(key.Path);
                        else
                            pathes[0] = key.Path;
                        GameSetting.Config.GamePadKeys[type] = pathes;
                    }
                    else
                        GameSetting.Config.GamePadKeys[type] = new List<string> { key.Path };
                }
                break;
            case 5:
                {
                    type = KeyType.LeftDon;
                    if (GameSetting.Config.GamePadKeys.TryGetValue(type, out List<string> pathes))
                    {
                        if (pathes.Count > 1)
                            pathes[1] = key.Path;
                        else if (pathes.Count > 0)
                            pathes.Add(key.Path);
                        else
                            pathes = new List<string> { string.Empty, key.Path };
                        GameSetting.Config.GamePadKeys[type] = pathes;
                    }
                    else
                        GameSetting.Config.GamePadKeys[type] = new List<string> { string.Empty, key.Path };
                }
                break;
            case 6:
                {
                    type = KeyType.RightDon;
                    if (GameSetting.Config.GamePadKeys.TryGetValue(type, out List<string> pathes))
                    {
                        if (pathes.Count == 0)
                            pathes.Add(key.Path);
                        else
                            pathes[0] = key.Path;
                        GameSetting.Config.GamePadKeys[type] = pathes;
                    }
                    else
                        GameSetting.Config.GamePadKeys[type] = new List<string> { key.Path };
                }
                break;
            case 7:
                {
                    type = KeyType.RightDon;
                    if (GameSetting.Config.GamePadKeys.TryGetValue(type, out List<string> pathes))
                    {
                        if (pathes.Count > 1)
                            pathes[1] = key.Path;
                        else if (pathes.Count > 0)
                            pathes.Add(key.Path);
                        else
                            pathes = new List<string> { string.Empty, key.Path };
                        GameSetting.Config.GamePadKeys[type] = pathes;
                    }
                    else
                        GameSetting.Config.GamePadKeys[type] = new List<string> { string.Empty, key.Path };
                }
                break;
            case 8:
                {
                    type = KeyType.LeftKa;
                    if (GameSetting.Config.GamePadKeys.TryGetValue(type, out List<string> pathes))
                    {
                        if (pathes.Count == 0)
                            pathes.Add(key.Path);
                        else
                            pathes[0] = key.Path;
                        GameSetting.Config.GamePadKeys[type] = pathes;
                    }
                    else
                        GameSetting.Config.GamePadKeys[type] = new List<string> { key.Path };
                }
                break;
            case 9:
                {
                    type = KeyType.LeftKa;
                    if (GameSetting.Config.GamePadKeys.TryGetValue(type, out List<string> pathes))
                    {
                        if (pathes.Count > 1)
                            pathes[1] = key.Path;
                        else if (pathes.Count > 0)
                            pathes.Add(key.Path);
                        else
                            pathes = new List<string> { string.Empty, key.Path };
                        GameSetting.Config.GamePadKeys[type] = pathes;
                    }
                    else
                        GameSetting.Config.GamePadKeys[type] = new List<string> { string.Empty, key.Path };
                }
                break;
            case 10:
                {
                    type = KeyType.RightKa;
                    if (GameSetting.Config.GamePadKeys.TryGetValue(type, out List<string> pathes))
                    {
                        if (pathes.Count == 0)
                            pathes.Add(key.Path);
                        else
                            pathes[0] = key.Path;
                        GameSetting.Config.GamePadKeys[type] = pathes;
                    }
                    else
                        GameSetting.Config.GamePadKeys[type] = new List<string> { key.Path };
                }
                break;
            case 11:
                {
                    type = KeyType.RightKa;
                    if (GameSetting.Config.GamePadKeys.TryGetValue(type, out List<string> pathes))
                    {
                        if (pathes.Count > 1)
                            pathes[1] = key.Path;
                        else if (pathes.Count > 0)
                            pathes.Add(key.Path);
                        else
                            pathes = new List<string> { string.Empty, key.Path };
                        GameSetting.Config.GamePadKeys[type] = pathes;
                    }
                    else
                        GameSetting.Config.GamePadKeys[type] = new List<string> { string.Empty, key.Path };
                }
                break;
            case 12:
                type = KeyType.Option;
                GameSetting.Config.GamePadKeys[type] = new List<string> { key.Path };
                break;
        }
        SetPadButton(type);
        BasicInputScript.ChangeGamepad(type);
        CloseInputing();
    }
    private void StickMove(StickControl control)
    {
        if (Application.isFocused && inputing && !control.name.Contains("anyKey") && current != CurrentKey.KeyKeyboard && index == 1)
        {
            Key key = new Key(control.name, control.path);
            KeyType type = KeyType.Direction;

            Vector2 value = control.ReadValue();
            if (Mathf.Abs(value.x) < 0.5f && Mathf.Abs(value.y) < 0.5f) return;

            GameSetting.Config.GamePadKeys[type] = new List<string> { control.path };
            SaveConfig();
            BasicInputScript.ChangeGamepad(type);
            CloseInputing();
        }
    }

    private void OnDpadPress(DpadControl control)
    {
        if (Application.isFocused && inputing && !control.name.Contains("anyKey"))
        {
            if (current == CurrentKey.KeyPad)
            {
                if (index == 1)
                {
                    KeyType type = KeyType.Direction;

                    GameSetting.Config.GamePadKeys[type] = new List<string> { control.path };
                    SaveConfig();
                    BasicInputScript.ChangeGamepad(type);
                    CloseInputing();
                }
                else if (index > 3 && index < 12)
                {
                    Vector2 value = control.ReadValue();
                    Key key = null;
                    if (value.x > 0)
                    {
                        key = new Key(control.name, control.path + "/right");
                    }
                    else if (value.x < 0)
                    {
                        key = new Key(control.name, control.path + "/left");
                    }
                    else if (value.y > 0)
                    {
                        key = new Key(control.name, control.path + "/up");
                    }
                    else if (value.y < 0)
                    {
                        key = new Key(control.name, control.path + "/down");
                    }

                    if (key != null) SetButton(key);
                }
            }
            else if (current == CurrentKey.Key2P)
            {
                Vector2 value = control.ReadValue();
                Key key = null;
                if (value.x > 0)
                {
                    key = new Key(control.name, control.path + "/right");
                }
                else if (value.x < 0)
                {
                    key = new Key(control.name, control.path + "/left");
                }
                else if (value.y > 0)
                {
                    key = new Key(control.name, control.path + "/up");
                }
                else if (value.y < 0)
                {
                    key = new Key(control.name, control.path + "/down");
                }

                if (key != null) SetButton2P(key);
            }
        }
    }
    

    private void OnButtonPress(CallbackContext context)
    {
        if (context.control is ButtonControl control &&  Application.isFocused && inputing && !control.isPressed && !control.name.Contains("anyKey"))
        {
            string device = control.device.description.deviceClass;
            if (current == CurrentKey.KeyKeyboard && device == "Keyboard")
                SetKeboard(control);
            else if (current == CurrentKey.KeyPad && device != "Keyboard" && device != "Mouse" && index != 1)
            {
                SetButton(control);
            }
            else if (current == CurrentKey.Key2P)
            {
                if (device == "Keyboard")
                    SetKeyboard2P(control);
                else
                {
                    Key key = new Key(control.name, control.path);
                    SetButton2P(key);
                }
            }
        }
    }

    private void Update()
    {
        if (GameSetting.Config.DirectInput && inputing)
        {
            BasicInputScript.Keyboard.Poll();
            var keys = BasicInputScript.Keyboard.GetBufferedData();
            if (keys.Length > 0)
            {
                if (keys[0].Key == SharpDX.DirectInput.Key.Escape)
                {
                    if (keys[0].IsReleased) CloseInputing();
                }
                else if (keys[0].IsPressed)
                {
                    SharpDX.DirectInput.Key input = keys[0].Key;

                    Key key = new Key(input.ToString().ToLower(), string.Format("/Keyboard/{0}", input.ToString().ToLower()));
                    if (key.Name.Contains("f1") || key.Name.Contains("f2") || key.Name.Contains("f3") || key.Name.Contains("right") || key.Name.Contains("left") || key.Name.Contains("up")
                        || key.Name.Contains("down") || key.Name.Contains("return") || key.Name.Contains("enter"))
                        return;

                    if (current == CurrentKey.KeyKeyboard)
                    {
                        KeyType type = KeyType.KeyNone;
                        switch (index)
                        {
                            case 1:
                                if (key.Equals(GameSetting.Config.DonRight) || key.Equals(GameSetting.Config.KaRight) || key.Equals(GameSetting.Config.KaLeft)
                                    || key.Equals(GameSetting.Config.Option))
                                    return;
                                type = KeyType.LeftDon;
                                break;
                            case 2:
                                if (key.Equals(GameSetting.Config.DonLeft) || key.Equals(GameSetting.Config.KaRight) || key.Equals(GameSetting.Config.KaLeft)
                                    || key.Equals(GameSetting.Config.Option))
                                {
                                    Debug.Log(key.Path);
                                    return;
                                }
                                type = KeyType.RightDon;
                                break;
                            case 3:
                                if (key.Equals(GameSetting.Config.DonLeft) || key.Equals(GameSetting.Config.KaRight) || key.Equals(GameSetting.Config.DonRight)
                                    || key.Equals(GameSetting.Config.Option))
                                    return;
                                type = KeyType.LeftKa;
                                break;
                            case 4:
                                if (key.Equals(GameSetting.Config.DonLeft) || key.Equals(GameSetting.Config.DonRight) || key.Equals(GameSetting.Config.KaLeft)
                                    || key.Equals(GameSetting.Config.Option))
                                    return;
                                type = KeyType.RightKa;
                                break;
                            case 5:
                                if (key.Equals(GameSetting.Config.DonLeft) || key.Equals(GameSetting.Config.KaRight) || key.Equals(GameSetting.Config.KaLeft)
                                    || key.Equals(GameSetting.Config.DonRight))
                                    return;
                                type = KeyType.Option;
                                break;
                        }
                        GameSetting.SetButton(type, key);
                        BasicInputScript.ChangeKeyBoard(type);
                        CloseInputing();
                    }
                    else if (current == CurrentKey.Key2P)
                    {
                        KeyType type = KeyType.KeyNone;
                        switch (index)
                        {
                            case 1:
                                type = KeyType.LeftDon;
                                break;
                            case 2:
                                type = KeyType.RightDon;
                                break;
                            case 3:
                                type = KeyType.LeftKa;
                                break;
                            case 4:
                                type = KeyType.RightKa;
                                break;
                        }
                        GameSetting.Config.GamePadKeys2P[type] = key.Path;
                        ParseKey2P(type);
                        SaveConfig();
                        BasicInputScript.ChangeKeyBoard2P(type);
                        CloseInputing();
                    }
                }
            }
        }
    }

    private void SetKeyboard2P(InputControl control)
    {
        Key key = new Key(control.name, control.path);
        if (key.Name.Contains("f1") || key.Name.Contains("f2") || key.Name.Contains("f3") || key.Name.Contains("escape")
            || key.Name.Contains("rightArrow") || key.Name.Contains("leftArrow") || key.Name.Contains("upArrow")
            || key.Name.Contains("downArrow") || key.Name.Contains("numpadEnter") || key.Name.Contains("enter"))
            return;
        KeyType type = KeyType.KeyNone;
        switch (index)
        {
            case 1:
                type = KeyType.LeftDon;
                break;
            case 2:
                type = KeyType.RightDon;
                break;
            case 3:
                type = KeyType.LeftKa;
                break;
            case 4:
                type = KeyType.RightKa;
                break;
        }
        GameSetting.Config.GamePadKeys2P[type] = key.Path;
        ParseKey2P(type);
        SaveConfig();
        BasicInputScript.ChangeKeyBoard2P(type);
        CloseInputing();
    }

    private void SetButton2P(Key key)
    {
        KeyType type = KeyType.KeyNone;
        CheckConflict(key);
        switch (index)
        {
            case 1:
                {
                    type = KeyType.LeftDon;
                }
                break;
            case 2:
                {
                    type = KeyType.RightDon;
                }
                break;
            case 3:
                {
                    type = KeyType.LeftKa;
                }
                break;
            case 4:
                {
                    type = KeyType.RightKa;
                }
                break;
        }
        GameSetting.Config.GamePadKeys2P[type] = key.Path;
        ParseKey2P(type);
        SaveConfig();
        BasicInputScript.ChangeKeyBoard2P(type);
        CloseInputing();
    }

    public void ChangeSelected(int index)
    {
        if (listning && !inputing && this.index != index)
        {
            this.index = index;

            Switch.Selected = index == 0;
            if (current == CurrentKey.KeyKeyboard)
            {
                for (int i = 0; i < KeyboardButtons.Length; i++)
                    KeyboardButtons[i].Selected = (i == index - 1);

                for (int i = 0; i < ButtonImas.Length; i++)
                    ButtonImas[i].gameObject.SetActive(i == index - 1);
            }
            else if (current == CurrentKey.KeyPad)
            {
                for (int i = 0; i < PadButtons.Length; i++)
                {
                    PadButtons[i].Selected = (i == index - 1);
                }
                ButtonImas[0].gameObject.SetActive(index == 4 || index == 5);
                ButtonImas[1].gameObject.SetActive(index == 6 || index == 7);
                ButtonImas[2].gameObject.SetActive(index == 8 || index == 9);
                ButtonImas[3].gameObject.SetActive(index == 10 || index == 11);
                ButtonImas[4].gameObject.SetActive(index == 12);
            }
            else
            {
                for (int i = 0; i < Buttons2P.Length; i++)
                    Buttons2P[i].Selected = (i == index - 1);

                for (int i = 0; i < ButtonImas.Length; i++)
                    ButtonImas[i].gameObject.SetActive(i == index - 1);
            }
        }
    }

    public void SetButton(int index)
    {
        EventSystem.current.SetSelectedGameObject(null);
        if (listning)
        {
            if (index == 0)
            {
                PlaySelectedSound?.Invoke(StartSoundControllScript.SoundType.Don);
                int key = (int)current + 1;
                if (key > 2) key = 0;
                SwitchDevice((CurrentKey)key);
            }
            else
            {
                if (current == CurrentKey.KeyKeyboard)
                {
                    if (index == KeyboardButtons.Length)
                        Back2Normal();
                    else
                    {
                        listning = false;
                        ChangeSelected(index);
                        inputing = true;
                        BasicInputScript.Refresh = false;
                        KeyboardButtons[index - 1].SetValue1(string.Empty);
                    }
                }
                else if (current == CurrentKey.KeyPad)
                {
                    if (index == PadButtons.Length)
                        Back2Normal();
                    else
                    {
                        listning = false;
                        ChangeSelected(index);
                        PadButtons[index - 1].SetValue1(string.Empty);
                        StartCoroutine(DelayInput());
                    }
                }
                else
                {
                    if (index == Buttons2P.Length)
                        Back2Normal();
                    else
                    {
                        listning = false;
                        ChangeSelected(index);
                        Buttons2P[index - 1].SetValue1(string.Empty);
                        StartCoroutine(DelayInput());
                    }
                }
            }
        }
    }

    IEnumerator DelayInput()
    {
        yield return new WaitForSeconds(0.1f);
        inputing = true;
        BasicInputScript.Refresh = false;
    }

    public void Show(bool show = true)
    {
        if (show)
        {
            listning = true;
            inputing = false;

            SwitchDevice(CurrentKey.KeyKeyboard);
        }
        else
        {
            if (!inputing && listning)
                Back2Normal();
        }
    }

    private void Back2Normal()
    {
        listning = false;
        inputing = false;
        BasicInputScript.Refresh = true;
        index = -1;

        Config.QuitButton();
    }

    private void SwitchDevice(CurrentKey keyboard)
    {
        current = keyboard;
        inputing = false;
        BasicInputScript.Refresh = true;

        string translation = string.Empty;
        switch (current)
        {
            case CurrentKey.KeyKeyboard:
                translation = "keyboard";
                break;
            case CurrentKey.KeyPad:
                translation = "peripheral";
                break;
            case CurrentKey.Key2P:
                translation = "Player2";
                break;
        }

        Switch.SetValue1(Translate(translation));
        RefreshKey();
        index = -1;

        Devices[0].gameObject.SetActive(current == CurrentKey.KeyKeyboard);
        Devices[1].gameObject.SetActive(current == CurrentKey.KeyPad);
        Devices[2].gameObject.SetActive(current == CurrentKey.Key2P);
        ChangeSelected(0);
    }

    private void RefreshKey()
    {
        if (current == CurrentKey.KeyKeyboard)
        {
            KeyboardButtons[0].SetValue1(System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(GameSetting.Config.DonLeft.Name));
            KeyboardButtons[1].SetValue1(System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(GameSetting.Config.DonRight.Name));
            KeyboardButtons[2].SetValue1(System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(GameSetting.Config.KaLeft.Name));
            KeyboardButtons[3].SetValue1(System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(GameSetting.Config.KaRight.Name));
            KeyboardButtons[4].SetValue1(System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(GameSetting.Config.Option.Name));
        }
        else if (current == CurrentKey.KeyPad)
        {
            BasicInputScript.Input.Player.Option.performed -= Click;
            BasicInputScript.Input.Player.Cancel.performed -= Click;
            BasicInputScript.Input.Player.RightDon.performed -= Click;

            BasicInputScript.Input.Player.Option.performed += Click;
            BasicInputScript.Input.Player.Cancel.performed += Click;
            BasicInputScript.Input.Player.RightDon.performed += Click;
            RebindKeys?.Invoke();

            GameSetting.Config.GamePadKeys.TryGetValue(KeyType.Direction, out List<string> pathes6);
            string key = pathes6 != null && pathes6.Count > 0 ? pathes6[0] : "--";
            string[] keys = key.Split('/');
            PadButtons[0].SetValue1(string.IsNullOrEmpty(keys[keys.Length - 1]) ? "--" :
                System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(keys[keys.Length - 1]));

            GameSetting.Config.GamePadKeys.TryGetValue(KeyType.Confirm, out List<string> pathes7);
            key = pathes7 != null && pathes7.Count > 0 ? pathes7[0] : "--";
            keys = key.Split('/');
            PadButtons[1].SetValue1(string.IsNullOrEmpty(keys[keys.Length - 1]) ? "--" :
                System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(keys[keys.Length - 1]));

            GameSetting.Config.GamePadKeys.TryGetValue(KeyType.Cancel, out List<string> pathes8);
            key = pathes8 != null && pathes8.Count > 0 ? pathes8[0] : "--";
            keys = key.Split('/');
            PadButtons[2].SetValue1(string.IsNullOrEmpty(keys[keys.Length - 1]) ? "--" :
                System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(keys[keys.Length - 1]));

            GameSetting.Config.GamePadKeys.TryGetValue(KeyType.LeftDon, out List<string> pathes);
            key = pathes != null && pathes.Count > 0 ? pathes[0] : "--";
            keys = key.Split('/');
            PadButtons[3].SetValue1(string.IsNullOrEmpty(keys[keys.Length - 1]) ? "--" :
                System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(keys[keys.Length - 1]));
            key = pathes != null && pathes.Count > 1 ? pathes[1] : "--";
            keys = key.Split('/');
            PadButtons[4].SetValue1(string.IsNullOrEmpty(keys[keys.Length - 1]) ? "--" :
                System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(keys[keys.Length - 1]));

            GameSetting.Config.GamePadKeys.TryGetValue(KeyType.RightDon, out List<string> pathes1);
            key = pathes1 != null && pathes1.Count > 0 ? pathes1[0] : "--";
            keys = key.Split('/');
            PadButtons[5].SetValue1(string.IsNullOrEmpty(keys[keys.Length - 1]) ? "--" :
                System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(keys[keys.Length - 1]));
            key = pathes1 != null && pathes1.Count > 1 ? pathes1[1] : "--";
            keys = key.Split('/');
            PadButtons[6].SetValue1(string.IsNullOrEmpty(keys[keys.Length - 1]) ? "--" :
                System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(keys[keys.Length - 1]));

            GameSetting.Config.GamePadKeys.TryGetValue(KeyType.LeftKa, out List<string> pathes3);
            key = pathes3 != null && pathes3.Count > 0 ? pathes3[0] : "--";
            keys = key.Split('/');
            PadButtons[7].SetValue1(string.IsNullOrEmpty(keys[keys.Length - 1]) ? "--" :
                System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(keys[keys.Length - 1]));
            key = pathes3 != null && pathes3.Count > 1 ? pathes3[1] : "--";
            keys = key.Split('/');
            PadButtons[8].SetValue1(string.IsNullOrEmpty(keys[keys.Length - 1]) ? "--" :
                System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(keys[keys.Length - 1]));

            GameSetting.Config.GamePadKeys.TryGetValue(KeyType.RightKa, out List<string> pathes4);
            key = pathes4 != null && pathes4.Count > 0 ? pathes4[0] : "--";
            keys = key.Split('/');
            PadButtons[9].SetValue1(string.IsNullOrEmpty(keys[keys.Length - 1]) ? "--" :
                System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(keys[keys.Length - 1]));
            key = pathes4 != null && pathes4.Count > 1 ? pathes4[1] : "--";
            keys = key.Split('/');
            PadButtons[10].SetValue1(string.IsNullOrEmpty(keys[keys.Length - 1]) ? "--" :
                System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(keys[keys.Length - 1]));

            GameSetting.Config.GamePadKeys.TryGetValue(KeyType.Option, out List<string> pathes5);
            key = pathes5 != null && pathes5.Count > 0 ? pathes5[0] : "--";
            keys = key.Split('/');
            PadButtons[11].SetValue1(string.IsNullOrEmpty(keys[keys.Length - 1]) ? "--" :
                System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(keys[keys.Length - 1]));
        }
        else
        {
            RebindKeys?.Invoke();

            GameSetting.Config.GamePadKeys2P.TryGetValue(KeyType.LeftDon, out string path);
            string key = path?? "--";
            string[] keys = key.Split('/');
            Buttons2P[0].SetValue1(string.IsNullOrEmpty(keys[keys.Length - 1]) ? "--" :
                System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(keys[keys.Length - 1]));

            GameSetting.Config.GamePadKeys2P.TryGetValue(KeyType.RightDon, out string path2);
            string key2 = path2 ?? "--";
            keys = key2.Split('/');
            Buttons2P[1].SetValue1(string.IsNullOrEmpty(keys[keys.Length - 1]) ? "--" :
                System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(keys[keys.Length - 1]));

            GameSetting.Config.GamePadKeys2P.TryGetValue(KeyType.LeftKa, out string path3);
            string key3 = path3 ?? "--";
            keys = key3.Split('/');
            Buttons2P[2].SetValue1(string.IsNullOrEmpty(keys[keys.Length - 1]) ? "--" :
                System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(keys[keys.Length - 1]));

            GameSetting.Config.GamePadKeys2P.TryGetValue(KeyType.RightKa, out string path4);
            string key4 = path4 ?? "--";
            keys = key4.Split('/');
            Buttons2P[3].SetValue1(string.IsNullOrEmpty(keys[keys.Length - 1]) ? "--" :
                System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(keys[keys.Length - 1]));
        }
    }
}
