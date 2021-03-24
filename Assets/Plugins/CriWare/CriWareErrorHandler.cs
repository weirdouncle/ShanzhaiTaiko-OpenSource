/****************************************************************************
 *
 * Copyright (c) 2012 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

#if !(!UNITY_EDITOR && UNITY_IOS && ENABLE_MONO)
#define CRIWAREERRORHANDLER_SUPPORT_NATIVE_CALLBACK
#endif

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

/**
 * \addtogroup CRIWARE_UNITY_COMPONENT
 * @{
 */

/**
 * <summary>CRIWARE error handling object</summary>
 * <remarks>
 * <para header='Description'>This component is used to initialize the CRIWARE library.<br/></para>
 * </remarks>
 */
[AddComponentMenu("CRIWARE/Error Handler")]
public class CriWareErrorHandler : CriMonoBehaviour{
	/**
	 * <summary>Whether to enable Coroutine debug output</summary>
	 * <remarks>
	 * <para header='Note'>Whether to enable console debug output along with Unity debug window output [deprecated]
	 * It will only output to the Unity debug window on PC.</para>
	 * </remarks>
	 */
	public bool enableDebugPrintOnTerminal = false;

	/** Whether to force crashing on error (for debug only) */
	public bool enableForceCrashOnError = false;

	/** Whether to remove the error handler at scene change */
	public bool dontDestroyOnLoad = true;

	/** Error message */
	public static string errorMessage { get; set; }

	/**
	 * <summary>Error callback delegate</summary>
	 * <remarks>
	 * <para header='Description'>A callback delegate that will be called when an error occurs
	 * in the CRIWARE native library. <br/>
	 * The argument string contains the message in the format "Error ID: Error details".</para>
	 * </remarks>
	 */
	public delegate void Callback(string message);

	/**
	 * <summary>Error callback</summary>
	 * <remarks>
	 * <para header='Description'>A callback that will be called when an error occurs in the CRIWARE native library. <br/>
	 * If not set, the default log output function defined in this class will be called. <br/>
	 * If a user defined process based on the error message is preferred,
	 * register a delegate and perform the process inside the callback function. <br/>
	 * Set the callback to null to unregister.</para>
	 * <para header='Note'>The registered callback may be called from a thread other than the main thread. <br/>
	 * <br/>
	 * The registered callback may be called at any time while CriWareErrorHandler is active. <br/>
	 * Be careful not to release the instance of the called function before CriWareErrorHandler .</para>
	 * </remarks>
	 */
	public static Callback callback = null;

	/* オブジェクト作成時の処理 */
	void Awake() {
		/* 初期化カウンタの更新 */
		initializationCount++;
		if (initializationCount != 1) {
			/* 多重初期化は許可しない */
			GameObject.Destroy(this);
			return;
		}

		/* エラー処理の初期化 */
		CRIWAREE5005D92();
		CRIWARE96CD2992(enableForceCrashOnError);

		/* ターミナルへのログ出力表示切り替え */
		CRIWARE7511F117(enableDebugPrintOnTerminal);

#if CRIWAREERRORHANDLER_SUPPORT_NATIVE_CALLBACK
		CRIWAREA10D44BF(ErrorCallbackFromNative);
#endif

		/* シーンチェンジ後もオブジェクトを維持するかどうかの設定 */
		if (dontDestroyOnLoad) {
			DontDestroyOnLoad(transform.gameObject);
		}
	}

	/* Execution Order の設定を確実に有効にするために OnEnable をオーバーライド */
	protected override void OnEnable() {
		base.OnEnable();
#if CRIWAREERRORHANDLER_SUPPORT_NATIVE_CALLBACK
		CRIWAREA10D44BF(ErrorCallbackFromNative);
#endif
	}

	protected override void OnDisable() {
		base.OnDisable();
#if CRIWAREERRORHANDLER_SUPPORT_NATIVE_CALLBACK
		CRIWAREA10D44BF(null);
#endif
	}

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	public override void CriInternalUpdate() {
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS || UNITY_TVOS)
		if (enableDebugPrintOnTerminal == false){
			/* iOS/Androidの場合、エラーメッセージの出力先が重複してしまうため、*/
			/* ターミナル出力が無効になっている場合のみ、Unity出力を有効にする。*/
			OutputErrorMessage();
		}
#else
		OutputErrorMessage();
#endif
	}

	public override void CriInternalLateUpdate() { }

	void OnDestroy() {
		/* 初期化カウンタの更新 */
		initializationCount--;
		if (initializationCount != 0) {
			return;
		}

#if CRIWAREERRORHANDLER_SUPPORT_NATIVE_CALLBACK
		CRIWAREA10D44BF(null);
#endif

		/* エラー処理の終了処理 */
		CRIWARE6FF199CE();
	}

	/* エラーメッセージのポーリングと出力 */
	private static void OutputErrorMessage()
	{
		/* エラーメッセージのポーリング */
		System.IntPtr ptr = CRIWARE94B4687C();
		if (ptr == IntPtr.Zero) {
			return;
		}

		/* Unity上でログ出力 */
		string message = Marshal.PtrToStringAnsi(ptr);
		if (message != string.Empty) {
			HandleMessage(message);
			CRIWARE921AC20B();
		}

		if (CriWareErrorHandler.errorMessage == null) {
			CriWareErrorHandler.errorMessage = message.Substring(0);
		}
	}

	private static void HandleMessage(string errmsg)
	{
		if (errmsg == null) {
			return;
		}

		if (callback == null) {
			OutputDefaultLog(errmsg);
		} else {
			callback(errmsg);
		}
	}

	/** Default log output */
	private static void OutputDefaultLog(string errmsg)
	{
		if (errmsg.StartsWith("E")) {
			Debug.LogError("[CRIWARE] Error:" + errmsg);
		} else if (errmsg.StartsWith("W")) {
			Debug.LogWarning("[CRIWARE] Warning:" + errmsg);
		} else {
			Debug.Log("[CRIWARE]" + errmsg);
		}
	}

#if CRIWAREERRORHANDLER_SUPPORT_NATIVE_CALLBACK
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void ErrorCallbackFunc(string errmsg);

	[AOT.MonoPInvokeCallback(typeof(ErrorCallbackFunc))]
	private static void ErrorCallbackFromNative(string errmsg)
	{
		HandleMessage(errmsg);
	}
#endif

	/** Initialization counter */
	private static int initializationCount = 0;

	#region DLL Import
	#if !CRIWARE_ENABLE_HEADLESS_MODE
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void CRIWAREE5005D92();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void CRIWARE6FF199CE();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern System.IntPtr CRIWARE94B4687C();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void CRIWARE921AC20B();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void CRIWARE7511F117(bool sw);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void CRIWARE96CD2992(bool sw);

#if CRIWAREERRORHANDLER_SUPPORT_NATIVE_CALLBACK
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void CRIWAREA10D44BF(ErrorCallbackFunc callback);
#endif
	#else
	private static void CRIWAREE5005D92() { }
	private static void CRIWARE6FF199CE() { }
	private static System.IntPtr CRIWARE94B4687C() { return System.IntPtr.Zero; }
	private static void CRIWARE921AC20B() { }
	private static void CRIWARE7511F117(bool sw) { }
	private static void CRIWARE96CD2992(bool sw) { }
	private static void CRIWAREA10D44BF(ErrorCallbackFunc callback) { }
	#endif
	#endregion
} // end of class

/** @} */

/* --- end of file --- */
