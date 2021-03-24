/****************************************************************************
 *
 * Copyright (c) 2011 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

/*---------------------------
 * Force Load Data with Async Method Defines
 *---------------------------*/
#if UNITY_WEBGL
	#define CRIWARE_FORCE_LOAD_ASYNC
#endif

#if !(!UNITY_EDITOR && UNITY_IOS && ENABLE_MONO)
	#define CRIWARE_SUPPORT_NATIVE_CALLBACK
#endif

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
#if !UNITY_EDITOR && (UNITY_WINRT && !ENABLE_IL2CPP)
using System.Reflection;
#endif


public static class CriAtomPlugin
{
	#region Editor/Runtime共通

#if UNITY_EDITOR
	public static bool showDebugLog = false;
	public delegate void PreviewCallback();
	public static PreviewCallback previewCallback = null;
#endif

	public static void Log(string log)
	{
	#if UNITY_EDITOR
		if (CriAtomPlugin.showDebugLog) {
			Debug.Log(log);
		}
	#endif
	}

	/* 初期化カウンタ */
	private static int initializationCount = 0;

	public static bool isInitialized { get { return initializationCount > 0; } }

	private static List<IntPtr> effectInterfaceList = null;
	public static bool GetAudioEffectInterfaceList(out List<IntPtr> effect_interface_list)
	{
		if (CriAtomPlugin.IsLibraryInitialized() == true) {
			effect_interface_list = null;
			return false;
		}
		if (effectInterfaceList == null) {
			effectInterfaceList = new List<IntPtr>();
		}
		effect_interface_list = effectInterfaceList;
		return true;
	}

	private static IntPtr GetSpatializerCoreInterfaceFromAtomOculusAudioBridge()
	{
		/* Ambisonic データを再生するために、プラットフォームによってはブリッジプラグインを必要とするかもしれない
		 * 例えば PC では Oculus Audio ブリッジプラグインを使う。
		 * 例えば PS4 では ブリッジプラグインを使わない */
		/* 以下、CRI Atom Oculus Audio ブリッジプラグインがインポートされている場合の処理 */
		Type type = Type.GetType("CriAtomOculusAudio");
		if (type == null) {
			/* BridgePluginが見つからなかった */
			Debug.LogError("[CRIWARE] ERROR: Cri Atom Oculus Audio Bridge Plugin is not imported.");
		} else {
			/* 現在のプラットフォームは Oculus Audio Bridge Plugin をサポートしているか確認 */
#if !UNITY_EDITOR && (UNITY_WINRT && !ENABLE_IL2CPP)
			System.Reflection.MethodInfo method_support_current_platform = type.GetTypeInfo().GetDeclaredMethod("SupportCurrentPlatform");
#else
			System.Reflection.MethodInfo method_support_current_platform = type.GetMethod("SupportCurrentPlatform");
#endif
			if (method_support_current_platform == null) {
				Debug.LogError("[CRIWARE] ERROR: CriAtomOculusAudio.SupportCurrentPlatform method is not found.");
				return IntPtr.Zero;
			}
			bool current_platform_supports_oculus_audio = (bool)method_support_current_platform.Invoke(null, null);
			/* カレントプラットフォームをサポートしているなら準備。
				* サポートしていないならスキップ。引き続き Atom 初期化処理を行う */
			if (current_platform_supports_oculus_audio) {
				/* 必要なメソッド情報を取得 */
#if !UNITY_EDITOR && (UNITY_WINRT && !ENABLE_IL2CPP)
				System.Reflection.MethodInfo method_get_spatializer_core_interface = type.GetTypeInfo().GetDeclaredMethod("GetSpatializerCoreInterface");
#else
				System.Reflection.MethodInfo method_get_spatializer_core_interface = type.GetMethod("GetSpatializerCoreInterface");
#endif
				if (method_get_spatializer_core_interface == null) {
					Debug.LogError("[CRIWARE] ERROR: CriAtomOculusAudio.GetSpatializerCoreInterface method is not found.");
					return IntPtr.Zero;
				}
				/* Spatilalizer の初期化に必要な情報を取得 */
				return (IntPtr)method_get_spatializer_core_interface.Invoke(null, null);
			}
		}
		return IntPtr.Zero;
	}

	public static void SetConfigParameters(int max_virtual_voices,
		int max_voice_limit_groups, int max_categories,
		int max_sequence_events_per_frame, int max_beatsync_callbacks_per_frame,
		int num_standard_memory_voices, int num_standard_streaming_voices,
		int num_hca_mx_memory_voices, int num_hca_mx_streaming_voices,
		int output_sampling_rate, int num_asr_output_channels,
		bool uses_in_game_preview, float server_frequency,
		int max_parameter_blocks,  int categories_per_playback,
		int num_buses, bool vr_mode)
	{
		IntPtr spatializer_core_interface = IntPtr.Zero;
		/* Ambisonic データの再生に必要な初期化パラメータを取得する */
		if (vr_mode) {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_ANDROID
			spatializer_core_interface = CriAtomPlugin.GetSpatializerCoreInterfaceFromAtomOculusAudioBridge();
#endif
		}
#if UNITY_WEBGL
		/* WebGLでは普通のボイスは作成することができない */
		num_standard_memory_voices = 0;
		num_standard_streaming_voices = 0;
		num_hca_mx_memory_voices = 0;
		num_hca_mx_streaming_voices = 0;
#endif
		CRIWARE5C6445EA(max_virtual_voices,
			max_voice_limit_groups, max_categories,
			max_sequence_events_per_frame, max_beatsync_callbacks_per_frame,
			num_standard_memory_voices, num_standard_streaming_voices,
			num_hca_mx_memory_voices, num_hca_mx_streaming_voices,
			output_sampling_rate, num_asr_output_channels,
			uses_in_game_preview, server_frequency,
			max_parameter_blocks, categories_per_playback,
			num_buses, vr_mode,
			spatializer_core_interface);

		CriAtomPlugin.isConfigured = true;
	}

	public static void SetConfigAdditionalParameters_PC(long buffering_time_pc)
	{
		CRIWARECBE1F916(buffering_time_pc);
	}

	public static void SetConfigAdditionalParameters_LINUX(CriAtomConfig.LinuxOutput output, int pulse_latency_usec)
	{
		CRIWARE5E9729B2((int)output, pulse_latency_usec);
	}

	public static void SetConfigAdditionalParameters_IOS(uint buffering_time_ios, bool override_ipod_music_ios)
	{
		CRIWARE6E6C4A31(buffering_time_ios, override_ipod_music_ios);
	}

	public static void SetConfigAdditionalParameters_ANDROID(int num_low_delay_memory_voices, int num_low_delay_streaming_voices,
															 int sound_buffering_time,        int sound_start_buffering_time,
															 bool use_fast_mixer,             bool use_aaudio)
	{
		CRIWARE4A98FA9C(num_low_delay_memory_voices, num_low_delay_streaming_voices,
														   sound_buffering_time,        sound_start_buffering_time,
														   use_fast_mixer);
#if !UNITY_EDITOR && UNITY_ANDROID
		if (use_fast_mixer == true) {
			IntPtr android_context = IntPtr.Zero;
			using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
			using (AndroidJavaObject activity = jc.GetStatic<AndroidJavaObject>("currentActivity")) {
				android_context = activity.GetRawObject();
				CRIWARE8E759390(android_context);
			}
		}
		CRIWARE6948E155(use_aaudio);
#endif
	}

	public static void SetConfigAdditionalParameters_VITA(int num_atrac9_memory_voices, int num_atrac9_streaming_voices, int num_mana_decoders)
	{
		#if !UNITY_EDITOR && UNITY_PSP2
		CRIWARE6579138F(num_atrac9_memory_voices, num_atrac9_streaming_voices, num_mana_decoders);
		#endif
	}

	public static void SetConfigAdditionalParameters_PS4(int num_atrac9_memory_voices, int num_atrac9_streaming_voices,
														 bool use_audio3d, int num_audio3d_memory_voices, int num_audio3d_streaming_voices)
	{
		#if !UNITY_EDITOR && (UNITY_PS4 || UNITY_PS5)
		CRIWAREE8B849A6(num_atrac9_memory_voices, num_atrac9_streaming_voices,
														use_audio3d, num_audio3d_memory_voices, num_audio3d_streaming_voices);
		#endif
	}

	public static void SetConfigAdditionalParameters_SWITCH(int num_opus_memory_voices, int num_opus_streaming_voices, bool init_socket) {
		#if !UNITY_EDITOR && UNITY_SWITCH
		CRIWARECA73AE32(num_opus_memory_voices, num_opus_streaming_voices, init_socket);
		#endif
	}

	public static void SetConfigAdditionalParameters_WEBGL(int num_webaudio_voices)
	{
		#if UNITY_WEBGL
		CRIWAREE41FBC60(num_webaudio_voices);
		#endif
	}

	public static void SetMaxSamplingRateForStandardVoicePool(int sampling_rate_for_memory, int sampling_rate_for_streaming)
	{
		CRIWARE413373CC(sampling_rate_for_memory, sampling_rate_for_streaming);
	}

	public static int GetRequiredMaxVirtualVoices(CriAtomConfig atomConfig)
	{
		/* バーチャルボイスは、全ボイスプールのボイスの合計値よりも多くなくてはならない */
		int requiredVirtualVoices = 0;

		requiredVirtualVoices += atomConfig.standardVoicePoolConfig.memoryVoices;
		requiredVirtualVoices += atomConfig.standardVoicePoolConfig.streamingVoices;
		requiredVirtualVoices += atomConfig.hcaMxVoicePoolConfig.memoryVoices;
		requiredVirtualVoices += atomConfig.hcaMxVoicePoolConfig.streamingVoices;

		#if UNITY_ANDROID
		requiredVirtualVoices += atomConfig.androidLowLatencyStandardVoicePoolConfig.memoryVoices;
		requiredVirtualVoices += atomConfig.androidLowLatencyStandardVoicePoolConfig.streamingVoices;
		#elif UNITY_PSP2
		requiredVirtualVoices += atomConfig.vitaAtrac9VoicePoolConfig.memoryVoices;
		requiredVirtualVoices += atomConfig.vitaAtrac9VoicePoolConfig.streamingVoices;
		#endif

		return requiredVirtualVoices;
	}

	public static void InitializeLibrary()
	{
		/* 初期化カウンタの更新 */
		CriAtomPlugin.initializationCount++;
		if (CriAtomPlugin.initializationCount != 1) {
			return;
		}

		/* シーン実行前に初期化済みの場合は終了させる */
		if (CriAtomPlugin.IsLibraryInitialized() == true) {
			CriAtomPlugin.FinalizeLibrary();
			CriAtomPlugin.initializationCount = 1;
		}

		/* 初期化パラメータが設定済みかどうかを確認 */
		if (CriAtomPlugin.isConfigured == false) {
			Debug.Log("[CRIWARE] Atom initialization parameters are not configured. "
				+ "Initializes Atom by default parameters.");
		}

		/* 依存ライブラリの初期化 */
		CriFsPlugin.InitializeLibrary();

		/* ライブラリの初期化 */
		CriAtomPlugin.CRIWARE044F4CBD();

		/* ユーザカスタムエフェクトプラグインのインタフェースを登録 */
	#if !UNITY_EDITOR && UNITY_PSP2
		// unsupported
	#else
		if (effectInterfaceList != null)
		{
			for (int i = 0; i < effectInterfaceList.Count; i++)
			{
				CriAtomExAsr.RegisterEffectInterface(effectInterfaceList[i]);
			}
		}
	#endif

		/* CriAtomServerのインスタンスを生成 */
		#if UNITY_EDITOR
		/* ゲームプレビュー時のみ生成する */
		if (UnityEngine.Application.isPlaying) {
			CriAtomServer.CreateInstance();
		}
		#else
		CriAtomServer.CreateInstance();
		#endif

		/* CriAtomListenerが存在しない場合のためのダミーリスナーを生成 */
		CriAtomListener.CreateDummyNativeListener();
	}

	public static bool IsLibraryInitialized()
	{
		/* ライブラリが初期化済みかチェック */
		return CRIWARE74F13712();
	}

	public static void FinalizeLibrary()
	{
		/* 初期化カウンタの更新 */
		CriAtomPlugin.initializationCount--;
		if (CriAtomPlugin.initializationCount < 0) {
			CriAtomPlugin.initializationCount = 0;
			if (CriAtomPlugin.IsLibraryInitialized() == false) {
				return;
			}
		}
		if (CriAtomPlugin.initializationCount != 0) {
			return;
		}

		/* CriAtomListenerが存在しない場合のためのダミーリスナーを破棄 */
		CriAtomListener.DestroyDummyNativeListener();

		/* CriAtomServerのインスタンスを破棄 */
		CriAtomServer.DestroyInstance();

		/* 未破棄のDisposableを破棄 */
		CriDisposableObjectManager.CallOnModuleFinalization(CriDisposableObjectManager.ModuleType.Atom);

		/* ユーザエフェクトインタフェースのリストをクリア */
		if (effectInterfaceList != null) {
			effectInterfaceList.Clear();
			effectInterfaceList = null;
		}

		/* ライブラリの終了 */
		CriAtomPlugin.CRIWARE0AC46773();

		/* 依存ライブラリの終了 */
		CriFsPlugin.FinalizeLibrary();
	}

	public static void Pause(bool pause)
	{
		if (isInitialized) {
			CRIWARED0B3D8F4(pause);
		}
	}

	#if !UNITY_EDITOR && UNITY_IOS
	public static void CallOnApplicationResume_IOS()
	{
		criAtomUnity_SleepToDelay_IOS(100);
	}
	#endif

	private static bool isConfigured = false;
	private static float timeSinceStartup = Time.realtimeSinceStartup;
	private static CriWare.Common.CpuUsage cpuUsage;
	public static CriWare.Common.CpuUsage GetCpuUsage()
	{
		float currentTimeSinceStartup = Time.realtimeSinceStartup;
		if (currentTimeSinceStartup - timeSinceStartup > 1.0f) {
			CriAtomEx.PerformanceInfo info;
			CriAtomEx.GetPerformanceInfo(out info);

			cpuUsage.last = info.lastServerTime * 100.0f / info.averageServerInterval;
			cpuUsage.average = info.averageServerTime * 100.0f / info.averageServerInterval;
			cpuUsage.peak = info.maxServerTime * 100.0f / info.averageServerInterval;

			CriAtomEx.ResetPerformanceMonitor();
			timeSinceStartup = currentTimeSinceStartup;
		}
		return cpuUsage;
	}

	private static int CRIATOMUNITY_PARAMETER_ID_LOOP_COUNT = 0;
	private static ushort CRIATOMPARAMETER2_ID_INVALID = ushort.MaxValue;

	public static ushort GetLoopCountParameterId()
	{
		ushort ret = CRIWARE39B30E48(CRIATOMUNITY_PARAMETER_ID_LOOP_COUNT);
		if (ret == CRIATOMPARAMETER2_ID_INVALID) {
			throw new Exception("GetNativeParameterId failed.");
		}
		return ret;
	}

	public static void DecryptAcb(IntPtr acb_hn, ulong key, ulong nonce)
	{
		temporalStorage = key ^ 0x0017D207B5350050UL;
		CRIWARE6E8A70F0(acb_hn, CallbackFromNative, IntPtr.Zero);
		temporalStorage = 0;
	}

	/* 変数の一時的な格納場所 */
	private static ulong temporalStorage = 0;

	#region Private Methods
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate ulong CallbackFromNativeDelegate(System.IntPtr ptr1);

	[AOT.MonoPInvokeCallback(typeof(CallbackFromNativeDelegate))]
	private static ulong CallbackFromNative(System.IntPtr ptr1)
	{
		return temporalStorage;
	}
	#endregion

	#region DLL Import
	#if !CRIWARE_ENABLE_HEADLESS_MODE
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void CRIWARE5C6445EA(int max_virtual_voices,
		int max_voice_limit_groups, int max_categories,
		int max_sequence_events_per_frame, int max_beatsync_callbacks_per_frame,
		int num_standard_memory_voices, int num_standard_streaming_voices,
		int num_hca_mx_memory_voices, int num_hca_mx_streaming_voices,
		int output_sampling_rate, int num_asr_output_channels,
		bool uses_in_game_preview, float server_frequency,
		int max_parameter_blocks, int categories_per_playback,
		int num_buses, bool use_ambisonics,
		IntPtr spatializer_core_interface);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void CRIWARECBE1F916(long buffering_time_pc);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void CRIWARE5E9729B2(int output, int pulse_latency_usec);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void CRIWARE6E6C4A31(uint buffering_time_ios, bool override_ipod_music_ios);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void CRIWARE4A98FA9C(int num_low_delay_memory_voices, int num_low_delay_streaming_voices,
																				  int sound_buffering_time,        int sound_start_buffering_time,
																				  bool apply_hw_property);

	#if !UNITY_EDITOR && UNITY_ANDROID
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void CRIWARE8E759390(IntPtr android_context);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void CRIWARE6948E155(bool flag);
	#endif

	#if !UNITY_EDITOR && UNITY_PSP2
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void CRIWARE6579138F(int num_atrac9_memory_voices, int num_atrac9_streaming_voices, int num_mana_decoders);
	#endif

	#if !UNITY_EDITOR && (UNITY_PS4 || UNITY_PS5)
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void CRIWAREE8B849A6(int num_atrac9_memory_voices, int num_atrac9_streaming_voices,
																			  bool use_audio3d, int num_audio3d_memory_voices, int num_audio3d_streaming_voices);
	#endif

	#if !UNITY_EDITOR && UNITY_SWITCH
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void CRIWARECA73AE32(int num_opus_memory_voices, int num_opus_streaming_voices, bool init_socket);
	#endif

	#if UNITY_WEBGL
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void CRIWAREE41FBC60(int num_webaudio_voices);
	#endif

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void CRIWARE044F4CBD();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern bool CRIWARE74F13712();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void CRIWARE0AC46773();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void CRIWARED0B3D8F4(bool pause);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern uint CRIWAREAEE74CFB();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern void CRIWARE7A85BD74(int code);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern void CRIWARE2D38DDC8(IntPtr cbfunc, string separator_string);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern void CRIWAREDDEFE97E();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern void CRIWARE6388ABF2(IntPtr cbfunc);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern void CRIWARE34DD9B8F();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void CRIWARE413373CC(int sampling_rate_for_memory, int sampling_rate_for_streaming);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern void CRIWARE34BFBEA8();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern void CRIWAREA0ABAC50();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern void CRIWARE6E8A70F0(IntPtr acb_hn, CallbackFromNativeDelegate func, IntPtr obj);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern ushort CRIWARE39B30E48(int id);

	#if !UNITY_EDITOR && UNITY_IOS
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern void criAtomUnity_SleepToDelay_IOS(int milliseconds);
	#endif
	#else
	private static void CRIWARE5C6445EA(int max_virtual_voices,
		int max_voice_limit_groups, int max_categories,
		int max_sequence_events_per_frame, int max_beatsync_callbacks_per_frame,
		int num_standard_memory_voices, int num_standard_streaming_voices,
		int num_hca_mx_memory_voices, int num_hca_mx_streaming_voices,
		int output_sampling_rate, int num_asr_output_channels,
		bool uses_in_game_preview, float server_frequency,
		int max_parameter_blocks, int categories_per_playback,
		int num_buses, bool use_ambisonics,
		IntPtr spatializer_core_interface) { }
	private static void CRIWARECBE1F916(long buffering_time_pc) { }
	private static void CRIWARE5E9729B2(int output, int pulse_latency_usec) { }
	private static void CRIWARE6E6C4A31(uint buffering_time_ios, bool override_ipod_music_ios) { }
	private static void CRIWARE4A98FA9C(int num_low_delay_memory_voices, int num_low_delay_streaming_voices,
																			int sound_buffering_time,          int sound_start_buffering_time,
																			bool apply_hw_property) { }
	#if !UNITY_EDITOR && UNITY_ANDROID
	private static void CRIWARE8E759390(IntPtr android_context) { }
	private static void CRIWARE6948E155(bool flag) { }
	#endif

	#if !UNITY_EDITOR && UNITY_PSP2
	private static void CRIWARE6579138F(int num_atrac9_memory_voices, int num_atrac9_streaming_voices,
																		int num_mana_decoders) { }
	#endif
	#if !UNITY_EDITOR && UNITY_PS4
	private static void CRIWAREE8B849A6(int num_atrac9_memory_voices, int num_atrac9_streaming_voices,
																		bool use_audio3d, int num_audio3d_memory_voices, int num_audio3d_streaming_voices) { }
	#endif
	#if !UNITY_EDITOR && UNITY_SWITCH
	private static void CRIWARECA73AE32(int num_opus_memory_voices, int num_opus_streaming_voices, bool init_socket) { }
	#endif
	#if UNITY_WEBGL
	private static void CRIWAREE41FBC60(int num_webaudio_voices) { }
	#endif
	private static bool _dummyInitialized = false;
	private static void CRIWARE044F4CBD() { _dummyInitialized = true; }
	public static bool CRIWARE74F13712() { return _dummyInitialized; }
	private static void CRIWARE0AC46773() { _dummyInitialized = false; }
	private static void CRIWARED0B3D8F4(bool pause) { }
	public static uint CRIWAREAEE74CFB() { return 0; }
	public static void CRIWARE7A85BD74(int code) { }
	public static void CRIWARE2D38DDC8(IntPtr cbfunc, string separator_string) { }
	public static void CRIWAREDDEFE97E() { }
	public static void CRIWARE6388ABF2(IntPtr cbfunc) { }
	public static void CRIWARE34DD9B8F() { }
	private static void CRIWARE413373CC(int sampling_rate_for_memory, int sampling_rate_for_streaming) { }
	public static void CRIWARE34BFBEA8() { }
	public static void CRIWAREA0ABAC50() { }
	public static void CRIWARE6E8A70F0(IntPtr acb_hn, CallbackFromNativeDelegate func, IntPtr obj) { }
	public static ushort CRIWARE39B30E48(int id) { return 0; }
#if !UNITY_EDITOR && UNITY_IOS
	public static void criAtomUnity_SleepToDelay_IOS(int milliseconds) { }
#endif
	#endif
	#endregion

	#endregion
}

[Serializable]
public class CriAtomCueSheet
{
	public string name = "";
	public string acbFile = "";
	public string awbFile = "";
	public CriAtomExAcb acb;
	public CriAtomExAcbLoader.Status loaderStatus = CriAtomExAcbLoader.Status.Stop;
	public bool IsLoading { get { return loaderStatus == CriAtomExAcbLoader.Status.Loading; } }
	public bool IsError { get { return (loaderStatus == CriAtomExAcbLoader.Status.Error) || (!IsLoading && acb == null); } }
}


/**
 * \addtogroup CRIATOM_UNITY_COMPONENT
 * @{
 */


/**
 * <summary>A component for controlling the overall sound playback.</summary>
 * <remarks>
 * <para header='Description'>You need to prepare one for each scene.<br/>
 * If you create a CriAtomSource using the CRI Atom window on UnityEditor,
 * it will automatically be created as an object with the name "CRIWARE". Normally, you do not need to create it.</para>
 * </remarks>
 */
[AddComponentMenu("CRIWARE/CRI Atom")]
public class CriAtom : CriMonoBehaviour
{

	/* @cond DOXYGEN_IGNORE */
	public string acfFile = "";
	private bool acfIsLoading = false;
#if CRIWARE_FORCE_LOAD_ASYNC
	private byte[] acfData = null;
#endif
	public CriAtomCueSheet[] cueSheets = {};
	public string dspBusSetting = "";
	public bool dontDestroyOnLoad = false;
#if CRIWARE_SUPPORT_NATIVE_CALLBACK
	private static CriAtomExSequencer.EventCbFunc eventUserCbFunc = null;
	private static CriAtomExBeatSync.CbFunc beatsyncUserCbFunc = null;
#endif
	private static CriAtom instance {
		get; set;
	}

	/* @endcond */

	#region Functions

	/**
	 * <summary>Attaching the DSP bus settings</summary>
	 * <param name='settingName'>Name of the DSP bus setting</param>
	 * <remarks>
	 * <para header='Description'>Builds a DSP bus from the DSP bus settings and attaches it to the sound renderer.<br/>
	 * If you want to switch the DSP bus settings currently set, detach the bus and then reattach it.
	 * <br/></para>
	 * <para header='Note'>This function is a return-on-complete function.<br/>
	 * Calling this function blocks the server processing of the Atom library for a while.<br/>
	 * If this function is called during sound playback, problems such as sound interruption may occur,
	 * so call this function at a timing when load fluctuations is accepted such as when switching scenes.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtom::DetachDspBusSetting'/>
	 */
	public static void AttachDspBusSetting(string settingName)
	{
		CriAtom.instance.dspBusSetting = settingName;
		if (!String.IsNullOrEmpty(settingName)) {
			CriAtomEx.AttachDspBusSetting(settingName);
		} else {
			CriAtomEx.DetachDspBusSetting();
		}
	}

	/**
	 * <summary>Detaches the DSP bus settings</summary>
	 * <remarks>
	 * <para header='Description'>Detaches the DSP bus settings.<br/>
	 * <br/></para>
	 * <para header='Note'>This function is a return-on-complete function.<br/>
	 * Calling this function blocks the server processing of the Atom library for a while.<br/>
	 * If this function is called during sound playback, problems such as sound interruption may occur,
	 * so call this function at a timing when load fluctuations is accepted such as when switching scenes.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtom::DetachDspBusSetting'/>
	 */
	public static void DetachDspBusSetting()
	{
		CriAtom.instance.dspBusSetting = "";
		CriAtomEx.DetachDspBusSetting();
	}

	/**
	 * <summary>Gets a Cue Sheet</summary>
	 * <param name='name'>Cue Sheet name</param>
	 * <returns>Cue Sheet object</returns>
	 * <remarks>
	 * <para header='Description'>Gets the registered Cue Sheet object based on the Cue Sheet name specified in the argument.<br/></para>
	 * </remarks>
	 */
	public static CriAtomCueSheet GetCueSheet(string name)
	{
		return CriAtom.instance.GetCueSheetInternal(name);
	}

	/**
	 * <summary>Adds a Cue Sheet</summary>
	 * <param name='name'>Cue Sheet name</param>
	 * <param name='acbFile'>ACB file path</param>
	 * <param name='awbFile'>AWB file path</param>
	 * <param name='binder'>Binder object (optional)</param>
	 * <returns>Cue Sheet object</returns>
	 * <remarks>
	 * <para header='Description'>A Cue Sheet is added based on the file path information specified in the argument.<br/>
	 * It is possible to register multiple Cue Sheets at the same time.<br/>
	 * <br/>
	 * If a relative path is specified for each file path, the file is loaded relative to the StreamingAssets folder.<br/>
	 * The file is loaded using the specified path if an absolute path or URL is specified.<br/>
	 * <br/>
	 * When adding a Cue Sheet from the ACB and AWB files packed in the CPK file,
	 * specify the binder which bound the CPK in the binder argument.<br/>
	 * The binder function cannot be used in ADX2LE.<br/></para>
	 * </remarks>
	 */
	public static CriAtomCueSheet AddCueSheet(string name, string acbFile, string awbFile, CriFsBinder binder = null)
	{
	#if CRIWARE_FORCE_LOAD_ASYNC
		return CriAtom.AddCueSheetAsync(name, acbFile, awbFile, binder);
	#else
		CriAtomCueSheet cueSheet = CriAtom.instance.AddCueSheetInternal(name, acbFile, awbFile, binder);
		if (Application.isPlaying) {
			cueSheet.acb = CriAtom.instance.LoadAcbFile(binder, acbFile, awbFile);
		}
		return cueSheet;
	#endif
	}

	/**
	 * <summary>Adds a Cue Sheet asynchronously</summary>
	 * <param name='name'>Cue Sheet name</param>
	 * <param name='acbFile'>ACB file path</param>
	 * <param name='awbFile'>AWB file path</param>
	 * <param name='binder'>Binder object (optional)</param>
	 * <param name='loadAwbOnMemory'>Whether to load the AWB file on the memory (optional)</param>
	 * <returns>Cue Sheet object</returns>
	 * <remarks>
	 * <para header='Description'>A Cue Sheet is added asynchronously based on the file path information specified in the argument.<br/>
	 * It is possible to register multiple Cue Sheets at the same time.<br/>
	 * <br/>
	 * If a relative path is specified for each file path, the file is loaded relative to the StreamingAssets folder.<br/>
	 * The file is loaded using the specified path if an absolute path or URL is specified.<br/>
	 * <br/>
	 * When adding a Cue Sheet from the ACB and AWB files packed in the CPK file,
	 * specify the binder which bound the CPK in the binder argument.<br/>
	 * The binder function cannot be used in ADX2LE.<br/>
	 * <br/>
	 * Loading is in progress while the CriAtomCueSheet::isLoading member of the returned Cue Sheet object is True.<br/>
	 * Be sure to check that it returns False before playing the Cue.<br/>
	 * <br/>
	 * If loadAwbOnMemory is set to False, only the header part of the AWB file is loaded on the memory and stream playback is performed.<br/>
	 * If loadAwbOnMemory is set to True, the entire AWB file is loaded into memory, effectively resulting in memory playback.<br/>
	 * In WebGL(Editor running), loadAwbOnMemory is forced to True for internal reasons.<br/></para>
	 * </remarks>
	 */
	public static CriAtomCueSheet AddCueSheetAsync(string name, string acbFile, string awbFile, CriFsBinder binder = null, bool loadAwbOnMemory = false)
	{
	#if UNITY_EDITOR && UNITY_WEBGL
		loadAwbOnMemory = true;
	#endif
		CriAtomCueSheet cueSheet = CriAtom.instance.AddCueSheetInternal(name, acbFile, awbFile, binder);
		if (Application.isPlaying) {
			CriAtom.instance.LoadAcbFileAsync(cueSheet, binder, acbFile, awbFile, loadAwbOnMemory);
		}
		return cueSheet;
	}

	/**
	 * <summary>Adds a Cue Sheet (reading from memory)</summary>
	 * <param name='name'>Cue Sheet name</param>
	 * <param name='acbData'>ACB data</param>
	 * <param name='awbFile'>AWB file path</param>
	 * <param name='awbBinder'>Binder object for AWB (optional)</param>
	 * <returns>Cue Sheet object</returns>
	 * <remarks>
	 * <para header='Description'>Adds a Cue Sheet from the data and file path information specified in the arguments.<br/>
	 * It is possible to register multiple Cue Sheets at the same time.<br/>
	 * <br/>
	 * If a relative path is specified for the file path, the file is loaded relative to the StreamingAssets folder.<br/>
	 * The file is loaded using the specified path if an absolute path or URL is specified.<br/>
	 * <br/>
	 * When adding a Cue Sheet from the AWB file packed in the CPK file,
	 * specify the binder which bound the CPK in the awbBinder argument.<br/>
	 * The binder function cannot be used in ADX2LE.<br/></para>
	 * </remarks>
	 */
	public static CriAtomCueSheet AddCueSheet(string name, byte[] acbData, string awbFile, CriFsBinder awbBinder = null)
	{
	#if CRIWARE_FORCE_LOAD_ASYNC
		return CriAtom.AddCueSheetAsync(name, acbData, awbFile, awbBinder);
	#else
		CriAtomCueSheet cueSheet = CriAtom.instance.AddCueSheetInternal(name, "", awbFile, awbBinder);
		if (Application.isPlaying) {
			cueSheet.acb = CriAtom.instance.LoadAcbData(acbData, awbBinder, awbFile);
		}
		return cueSheet;
	#endif
	}

	/**
	 * <summary>Adds a Cue Sheet asynchronously (reading from memory)</summary>
	 * <param name='name'>Cue Sheet name</param>
	 * <param name='acbData'>ACB data</param>
	 * <param name='awbFile'>AWB file path</param>
	 * <param name='awbBinder'>Binder object for AWB (optional)</param>
	 * <param name='loadAwbOnMemory'>Whether to load the AWB file on the memory (optional)</param>
	 * <returns>Cue Sheet object</returns>
	 * <remarks>
	 * <para header='Description'>Adds a Cue Sheet from the data and file path information specified in the arguments.<br/>
	 * It is possible to register multiple Cue Sheets at the same time.<br/>
	 * <br/>
	 * If a relative path is specified for the file path, the file is loaded relative to the StreamingAssets folder.<br/>
	 * The file is loaded using the specified path if an absolute path or URL is specified.<br/>
	 * <br/>
	 * When adding a Cue Sheet from the AWB file packed in the CPK file,
	 * specify the binder which bound the CPK in the awbBinder argument.<br/>
	 * The binder function cannot be used in ADX2LE.<br/>
	 * <br/>
	 * Loading is in progress while the CriAtomCueSheet::isLoading member of the returned Cue Sheet object is True.<br/>
	 * Be sure to check that it returns False before playing the Cue.<br/>
	 * <br/>
	 * If loadAwbOnMemory is set to False, only the header part of the AWB file is loaded on the memory and stream playback is performed.<br/>
	 * If loadAwbOnMemory is set to True, the entire AWB file is loaded into memory, effectively resulting in memory playback.<br/>
	 * In WebGL(Editor running), loadAwbOnMemory is forced to True for internal reasons.<br/></para>
	 * </remarks>
	 */
	public static CriAtomCueSheet AddCueSheetAsync(string name, byte[] acbData, string awbFile, CriFsBinder awbBinder = null, bool loadAwbOnMemory = false)
	{
	#if UNITY_EDITOR && UNITY_WEBGL
		loadAwbOnMemory = true;
	#endif
		CriAtomCueSheet cueSheet = CriAtom.instance.AddCueSheetInternal(name, "", awbFile, awbBinder);
		if (Application.isPlaying) {
			CriAtom.instance.LoadAcbDataAsync(cueSheet, acbData, awbBinder, awbFile, loadAwbOnMemory);
		}
		return cueSheet;
	}

	/**
	 * <summary>Removes a Cue Sheet</summary>
	 * <param name='name'>Cue Sheet name</param>
	 * <remarks>
	 * <para header='Description'>Removes the added Cue Sheet.<br/></para>
	 * </remarks>
	 */
	public static void RemoveCueSheet(string name)
	{
		if (CriAtom.instance == null) {
			return;
		}
		//Debug.Log("remove cue " + name);
		CriAtom.instance.RemoveCueSheetInternal(name);
	}

	/**
	 * <summary>Checks the completion of loading a Cue Sheet.</summary>
	 * <remarks>
	 * <para header='Description'>Checks the completion of loading all Cue Sheets.<br/></para>
	 * </remarks>
	 */
	public static bool CueSheetsAreLoading {
		get {
			if (CriAtom.instance == null) {
				return false;
			}
			foreach (var cueSheet in CriAtom.instance.cueSheets) {
				if (cueSheet.IsLoading) {
					return true;
				}
			}
			return false;
		}
	}

	/**
	 * <summary>Gets the ACB object</summary>
	 * <param name='cueSheetName'>Cue Sheet name</param>
	 * <returns>ACB object</returns>
	 * <remarks>
	 * <para header='Description'>Gets the ACB object corresponding to the specified Cue Sheet.<br/></para>
	 * </remarks>
	 */
	public static CriAtomExAcb GetAcb(string cueSheetName)
	{
		foreach (var cueSheet in CriAtom.instance.cueSheets) {
			if (cueSheetName == cueSheet.name) {
				return cueSheet.acb;
			}
		}
		Debug.LogWarning(cueSheetName + " is not loaded.");
		return null;
	}

	/**
	 * <summary>Sets the Category volume by specifying the Category name.</summary>
	 * <param name='name'>Category name</param>
	 * <param name='volume'>Volume</param>
	 */
	public static void SetCategoryVolume(string name, float volume)
	{
		CriAtomExCategory.SetVolume(name, volume);
	}

	/**
	 * <summary>Sets the Category volume by specifying the Category ID.</summary>
	 * <param name='id'>Category ID</param>
	 * <param name='volume'>Volume</param>
	 */
	public static void SetCategoryVolume(int id, float volume)
	{
		CriAtomExCategory.SetVolume(id, volume);
	}

	/**
	 * <summary>Gets the Category volume by specifying the Category name.</summary>
	 * <param name='name'>Category name</param>
	 * <returns>Volume</returns>
	 */
	public static float GetCategoryVolume(string name)
	{
		return CriAtomExCategory.GetVolume(name);
	}
	/**
	 * <summary>Gets the Category volume by specifying the Category ID.</summary>
	 * <param name='id'>Category ID</param>
	 * <returns>Volume</returns>
	 */
	public static float GetCategoryVolume(int id)
	{
		return CriAtomExCategory.GetVolume(id);
	}

	/**
	 * <summary>Enables the acquisition of the bus information</summary>
	 * <param name='busName'>DSP bus name</param>
	 * <param name='sw'>True: Enable acquisition. False: Disable acquisition.</param>
	 */
	public static void SetBusAnalyzer(string busName, bool sw)
	{
	#if !UNITY_EDITOR && UNITY_PSP2
		// unsupported
	#else
		if (sw) {
			CriAtomExAsr.AttachBusAnalyzer(busName, 50, 1000);
		} else {
			CriAtomExAsr.DetachBusAnalyzer(busName);
		}
	#endif
	}

	/**
	 * <summary>Enables acquisition of all bus information.</summary>
	 * <param name='sw'>True: Enable acquisition. False: Disable acquisition.</param>
	 */
	public static void SetBusAnalyzer(bool sw)
	{
	#if !UNITY_EDITOR && UNITY_PSP2
		// unsupported
	#else
		if (sw) {
			CriAtomExAsr.AttachBusAnalyzer(50, 1000);
		} else {
			CriAtomExAsr.DetachBusAnalyzer();
		}
	#endif
	}

	/**
	 * <summary>Gets the bus information.</summary>
	 * <param name='busName'>DSP bus name</param>
	 * <returns>DSP bus information</returns>
	 */
	public static CriAtomExAsr.BusAnalyzerInfo GetBusAnalyzerInfo(string busName)
	{
		CriAtomExAsr.BusAnalyzerInfo info;
	#if !UNITY_EDITOR && UNITY_PSP2
		info = new CriAtomExAsr.BusAnalyzerInfo(null);
	#else
		CriAtomExAsr.GetBusAnalyzerInfo(busName, out info);
	#endif
		return info;
	}

	[System.Obsolete("Use CriAtom.GetBusAnalyzerInfo(string busName)")]
	public static CriAtomExAsr.BusAnalyzerInfo GetBusAnalyzerInfo(int busId)
	{
		CriAtomExAsr.BusAnalyzerInfo info;
	#if !UNITY_EDITOR && UNITY_PSP2
		info = new CriAtomExAsr.BusAnalyzerInfo(null);
	#else
		CriAtomExAsr.GetBusAnalyzerInfo(busId, out info);
	#endif
		return info;
	}

	#endregion // Functions

	/* @cond DOXYGEN_IGNORE */
	#region Functions for internal use

	public void Setup()
	{
		if (CriAtom.instance != null && CriAtom.instance != this) {
			var obj = CriAtom.instance.gameObject;
			CriAtom.instance.Shutdown();
			CriAtomEx.UnregisterAcf();
			GameObject.Destroy(obj);
		}

		CriAtom.instance = this;

		CriAtomPlugin.InitializeLibrary();

		if (!String.IsNullOrEmpty(this.acfFile)) {
			string acfPath = Path.Combine(CriWare.Common.streamingAssetsPath, this.acfFile);
			CriAtomEx.RegisterAcf(null, acfPath);
		}

		if (!String.IsNullOrEmpty(dspBusSetting)) {
			AttachDspBusSetting(dspBusSetting);
		}

		foreach (var cueSheet in this.cueSheets) {
			cueSheet.acb = this.LoadAcbFile(null, cueSheet.acbFile, cueSheet.awbFile);
		}

		if (this.dontDestroyOnLoad) {
			GameObject.DontDestroyOnLoad(this.gameObject);
		}
	}

	public void Shutdown()
	{
		foreach (var cueSheet in this.cueSheets) {
			if (cueSheet.acb != null) {
				cueSheet.acb.Dispose();
				cueSheet.acb = null;
			}
		}
		CriAtomPlugin.FinalizeLibrary();
		if (CriAtom.instance == this) {
			CriAtom.instance = null;
		}
	}

	void Awake()
	{
		if (CriAtom.instance != null && CriAtom.instance != this) {
			if (CriAtom.instance.acfFile != this.acfFile) {
				var obj = CriAtom.instance.gameObject;
				CriAtom.instance.Shutdown();
				CriAtomEx.UnregisterAcf();
				GameObject.Destroy(obj);
				return;
			}
			if (CriAtom.instance.dspBusSetting != this.dspBusSetting) {
				CriAtom.AttachDspBusSetting(this.dspBusSetting);
			}
			CriAtom.instance.MargeCueSheet(this.cueSheets, this.dontRemoveExistsCueSheet);
			GameObject.Destroy(this.gameObject);
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
	#if UNITY_EDITOR
		if (CriAtomPlugin.previewCallback != null) {
			CriAtomPlugin.previewCallback();
		}
	#endif
		if (CriAtom.instance != null) {
			return;
		}

	#if CRIWARE_FORCE_LOAD_ASYNC
		this.SetupAsync();
	#else
		this.Setup();
	#endif
	}

	void OnDestroy()
	{
		if (this != CriAtom.instance) {
			return;
		}
		this.Shutdown();
	}

	public override void CriInternalUpdate()
	{
		CriAtomPlugin.CRIWAREDDEFE97E();
		CriAtomPlugin.CRIWARE34DD9B8F();
	}

	public override void CriInternalLateUpdate() { }

	public CriAtomCueSheet GetCueSheetInternal(string name)
	{
		for (int i = 0; i < this.cueSheets.Length; i++) {
			CriAtomCueSheet cueSheet = this.cueSheets[i];
			if (cueSheet.name == name) {
				return cueSheet;
			}
		}
		return null;
	}

	public CriAtomCueSheet AddCueSheetInternal(string name, string acbFile, string awbFile, CriFsBinder binder)
	{
		var cueSheets = new CriAtomCueSheet[this.cueSheets.Length + 1];
		this.cueSheets.CopyTo(cueSheets, 0);
		this.cueSheets = cueSheets;

		var cueSheet = new CriAtomCueSheet();
		this.cueSheets[this.cueSheets.Length - 1] = cueSheet;
		if (String.IsNullOrEmpty(name)) {
			cueSheet.name = Path.GetFileNameWithoutExtension(acbFile);
		} else {
			cueSheet.name = name;
		}
		cueSheet.acbFile = acbFile;
		cueSheet.awbFile = awbFile;
		return cueSheet;
	}

	public void RemoveCueSheetInternal(string name)
	{
		int index = -1;
		for (int i = 0; i < this.cueSheets.Length; i++) {
			if (name == this.cueSheets[i].name) {
				index = i;
			}
		}
		if (index < 0) {
			return;
		}

		var cueSheet = this.cueSheets[index];
		if (cueSheet.acb != null) {
			cueSheet.acb.Dispose();
			cueSheet.acb = null;
		}

		var cueSheets = new CriAtomCueSheet[this.cueSheets.Length - 1];
		Array.Copy(this.cueSheets, 0, cueSheets, 0, index);
		Array.Copy(this.cueSheets, index + 1, cueSheets, index, this.cueSheets.Length - index - 1);
		this.cueSheets = cueSheets;
	}

	public bool dontRemoveExistsCueSheet = false;

	/*
	 * newDontRemoveExistsCueSheet == false の場合、古いキューシートを登録解除して、新しいキューシートの登録を行う。
	 * ただし同じキューシートの再登録は回避する
	 */
	private void MargeCueSheet(CriAtomCueSheet[] newCueSheets, bool newDontRemoveExistsCueSheet)
	{
		if (!newDontRemoveExistsCueSheet) {
			for (int i = 0; i < this.cueSheets.Length; ) {
				int index = Array.FindIndex(newCueSheets, sheet => sheet.name == this.cueSheets[i].name);
				if (index < 0) {
					CriAtom.RemoveCueSheet(this.cueSheets[i].name);
				} else {
					i++;
				}
			}
		}

		foreach (var sheet1 in newCueSheets) {
			var sheet2 = this.GetCueSheetInternal(sheet1.name);
			if (sheet2 == null) {
				CriAtom.AddCueSheet(null, sheet1.acbFile, sheet1.awbFile, null);
			}
		}
	}

	private CriAtomExAcb LoadAcbFile(CriFsBinder binder, string acbFile, string awbFile)
	{
		if (String.IsNullOrEmpty(acbFile)) {
			return null;
		}

		string acbPath = acbFile;
		if ((binder == null) && CriWare.Common.IsStreamingAssetsPath(acbPath)) {
			acbPath = Path.Combine(CriWare.Common.streamingAssetsPath, acbPath);
		}

		string awbPath = awbFile;
		if (!String.IsNullOrEmpty(awbPath)) {
			if ((binder == null) && CriWare.Common.IsStreamingAssetsPath(awbPath)) {
				awbPath = Path.Combine(CriWare.Common.streamingAssetsPath, awbPath);
			}
		}

		return CriAtomExAcb.LoadAcbFile(binder, acbPath, awbPath);
	}

	private CriAtomExAcb LoadAcbData(byte[] acbData, CriFsBinder binder, string awbFile)
	{
		if (acbData == null) {
			return null;
		}

		string awbPath = awbFile;
		if (!String.IsNullOrEmpty(awbPath)) {
			if ((binder == null) && CriWare.Common.IsStreamingAssetsPath(awbPath)) {
				awbPath = Path.Combine(CriWare.Common.streamingAssetsPath, awbPath);
			}
		}

		return CriAtomExAcb.LoadAcbData(acbData, binder, awbPath);
	}

#if CRIWARE_FORCE_LOAD_ASYNC
	private void SetupAsync()
	{
		CriAtom.instance = this;

		CriAtomPlugin.InitializeLibrary();

		if (this.dontDestroyOnLoad) {
			GameObject.DontDestroyOnLoad(this.gameObject);
		}

		if (!String.IsNullOrEmpty(this.acfFile)) {
			this.acfIsLoading = true;
			StartCoroutine(RegisterAcfAsync(this.acfFile, this.dspBusSetting));
		}

		foreach (var cueSheet in this.cueSheets) {
			#if UNITY_EDITOR
			bool loadAwbOnMemory = true;
			#else
			bool loadAwbOnMemory = false;
			#endif
			StartCoroutine(LoadAcbFileCoroutine(cueSheet, null, cueSheet.acbFile, cueSheet.awbFile, loadAwbOnMemory));
		}
	}

	private IEnumerator RegisterAcfAsync(string acfFile, string dspBusSetting)
	{
	#if UNITY_EDITOR
		string acfPath = "file://" + CriWare.Common.streamingAssetsPath + "/" + acfFile;
	#else
		string acfPath = CriWare.Common.streamingAssetsPath + "/" + acfFile;
	#endif
		using (var req = new WWW(acfPath)) {
			yield return req;
			this.acfData = req.bytes;
		}
		CriAtomEx.RegisterAcf(this.acfData);

		if (!String.IsNullOrEmpty(dspBusSetting)) {
			AttachDspBusSetting(dspBusSetting);
		}
		this.acfIsLoading = false;
	}
#endif


	private void LoadAcbFileAsync(CriAtomCueSheet cueSheet, CriFsBinder binder, string acbFile, string awbFile, bool loadAwbOnMemory)
	{
		if (String.IsNullOrEmpty(acbFile)) {
			return;
		}

		StartCoroutine(LoadAcbFileCoroutine(cueSheet, binder, acbFile, awbFile, loadAwbOnMemory));
	}

	private IEnumerator LoadAcbFileCoroutine(CriAtomCueSheet cueSheet, CriFsBinder binder, string acbPath, string awbPath, bool loadAwbOnMemory)
	{
		cueSheet.loaderStatus = CriAtomExAcbLoader.Status.Loading;

		if ((binder == null) && CriWare.Common.IsStreamingAssetsPath(acbPath)) {
			acbPath = Path.Combine(CriWare.Common.streamingAssetsPath, acbPath);
		}

		if (!String.IsNullOrEmpty(awbPath)) {
			if ((binder == null) && CriWare.Common.IsStreamingAssetsPath(awbPath)) {
				awbPath = Path.Combine(CriWare.Common.streamingAssetsPath, awbPath);
			}
		}

		while (this.acfIsLoading) {
			yield return null;
		}

		using (var asyncLoader = CriAtomExAcbLoader.LoadAcbFileAsync(binder, acbPath, awbPath, loadAwbOnMemory)) {
			if (asyncLoader == null) {
				cueSheet.loaderStatus = CriAtomExAcbLoader.Status.Error;
				yield break;
			}

			while (true) {
				var status = asyncLoader.GetStatus();
				cueSheet.loaderStatus = status;
				if (status == CriAtomExAcbLoader.Status.Complete) {
					cueSheet.acb = asyncLoader.MoveAcb();
					break;
				} else if (status == CriAtomExAcbLoader.Status.Error) {
					break;
				}
				yield return null;
			}
		}
	}

	private void LoadAcbDataAsync(CriAtomCueSheet cueSheet, byte[] acbData, CriFsBinder awbBinder, string awbFile, bool loadAwbOnMemory)
	{
		StartCoroutine(LoadAcbDataCoroutine(cueSheet, acbData, awbBinder, awbFile, loadAwbOnMemory));
	}

	private IEnumerator LoadAcbDataCoroutine(CriAtomCueSheet cueSheet, byte[] acbData, CriFsBinder awbBinder, string awbPath, bool loadAwbOnMemory)
	{
		cueSheet.loaderStatus = CriAtomExAcbLoader.Status.Loading;

		if (!String.IsNullOrEmpty(awbPath)) {
			if ((awbBinder == null) && CriWare.Common.IsStreamingAssetsPath(awbPath)) {
				awbPath = Path.Combine(CriWare.Common.streamingAssetsPath, awbPath);
			}
		}

		while (this.acfIsLoading) {
			yield return null;
		}

		using (var asyncLoader = CriAtomExAcbLoader.LoadAcbDataAsync(acbData, awbBinder, awbPath, loadAwbOnMemory)) {
			if (asyncLoader == null) {
				cueSheet.loaderStatus = CriAtomExAcbLoader.Status.Error;
				yield break;
			}

			while (true) {
				var status = asyncLoader.GetStatus();
				cueSheet.loaderStatus = status;
				if (status == CriAtomExAcbLoader.Status.Complete) {
					cueSheet.acb = asyncLoader.MoveAcb();
					break;
				} else if (status == CriAtomExAcbLoader.Status.Error) {
					break;
				}
				yield return null;
			}
		}
	}

#if CRIWARE_SUPPORT_NATIVE_CALLBACK
	[AOT.MonoPInvokeCallback(typeof(CriAtomExSequencer.EventCbFunc))]
	public static void SequenceEventCallbackFromNative(string eventString)
	{
		/* 本関数はアプリケーションメインスレッドの CriAtom.Update から呼び出される */
		if (eventUserCbFunc != null) {
			eventUserCbFunc(eventString);
		}
	}

	[AOT.MonoPInvokeCallback(typeof(CriAtomExBeatSync.CbFunc))]
	public static void BeatSyncCallbackFromNative(ref CriAtomExBeatSync.Info info)
	{
		/* 本関数はアプリケーションメインスレッドの CriAtom.Update から呼び出される */
		if (beatsyncUserCbFunc != null) {
			beatsyncUserCbFunc(ref info);
		}
	}
#endif

	/* プラグイン内部用API */
	public static void SetEventCallback(CriAtomExSequencer.EventCbFunc func, string separator)
	{
#if CRIWARE_SUPPORT_NATIVE_CALLBACK
		/* ネイティブプラグインに関数ポインタを登録 */
		IntPtr ptr = IntPtr.Zero;
		eventUserCbFunc = func;
		if (func != null) {
			CriAtomExSequencer.EventCbFunc delegateObject;
			delegateObject = new CriAtomExSequencer.EventCbFunc(CriAtom.SequenceEventCallbackFromNative);
			ptr = Marshal.GetFunctionPointerForDelegate(delegateObject);
		}
		CriAtomPlugin.CRIWARE2D38DDC8(ptr, separator);
#else
		Debug.LogError("[CRIWARE] Event callback is not supported for this scripting backend.");
#endif
	}

	public static void SetBeatSyncCallback(CriAtomExBeatSync.CbFunc func)
	{
#if CRIWARE_SUPPORT_NATIVE_CALLBACK
		/* ネイティブプラグインに関数ポインタを登録 */
		IntPtr ptr = IntPtr.Zero;
		beatsyncUserCbFunc = func;
		if (func != null) {
			CriAtomExBeatSync.CbFunc delegateObject;
			delegateObject = new CriAtomExBeatSync.CbFunc (CriAtom.BeatSyncCallbackFromNative);
			ptr = Marshal.GetFunctionPointerForDelegate(delegateObject);
		}
		CriAtomPlugin.CRIWARE6388ABF2(ptr);
#else
		Debug.LogError("[CRIWARE] Beat sync callback is not supported for this scripting backend.");
#endif
	}

	#endregion
	/* @endcond */

}

/** @} */
/* end of file */
