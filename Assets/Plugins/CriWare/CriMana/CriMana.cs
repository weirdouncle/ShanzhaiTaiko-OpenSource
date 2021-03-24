/****************************************************************************
 *
 * Copyright (c) 2011 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.IO;
#if !UNITY_EDITOR && (UNITY_WINRT && !ENABLE_IL2CPP)
using System.Reflection;
#endif


namespace CriMana
{
	/**
	 * <summary>A value indicating the type of movie codec.</summary>
	 */
	public enum CodecType
	{
		Unknown         = 0,
		SofdecPrime     = 1,
		H264            = 5,
		VP9             = 9,
	}


	/**
	 * <summary>A value indicating the movie composition mode.</summary>
	 */
	public enum AlphaType
	{
		CompoOpaq       = 0,    /**< Opaque, no alpha information */
		CompoAlphaFull  = 1,    /**< Full Alpha synthesis (alpha data is 8 bits) */
		CompoAlpha3Step = 2,    /**< Ternary alpha */
		CompoAlpha32Bit = 3,    /**< Full Alpha, (32 bits with color and alpha data) */
	}


	/**
	 * <summary>The audio analysis information in the movie file.</summary>
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct AudioInfo
	{
		public UInt32   samplingRate;   /**< Sampling frequency */
		public UInt32   numChannels;    /**< The number of audio channels */
		public UInt32   totalSamples;   /**< Total number of samples */
	}


	/**
	 * <summary>The header analysis information for movie files.</summary>
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public class MovieInfo
	{
		private UInt32  _reserved1;
		private UInt32  _hasAlpha;
		public UInt32   width;              /**< Maximum movie width (a multiple of 8) */
		public UInt32   height;             /**< Maximum movie height (a multiple of 8) */
		public UInt32   dispWidth;          /**< The number of horizontal pixels of the image to be displayed (from the left edge) */
		public UInt32   dispHeight;         /**< The number of vertical pixels of the image to be displayed (from the top edge) */
		public UInt32   framerateN;         /**< Rational format frame rate (numerator) framerate [x1000] = framerateN / framerateD */
		public UInt32   framerateD;         /**< Rational format frame rate (denominator) framerate [x1000] = framerateN / framerateD */
		public UInt32   totalFrames;        /**< Total number of frames */
		private UInt32  _codecType;
		private UInt32  _alphaCodecType;
		public UInt32   numAudioStreams;            /**< The number of audio streams */
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
		public AudioInfo[]  audioPrm;               /**< Audio parameters */
		public UInt32       numSubtitleChannels;    /**< The number of subtitle channels */
		public UInt32       maxSubtitleSize;        /**< Maximum size of the subtitle data */
		public UInt32       maxChunkSize;           /**< Maximum USF chunk size */

		public bool         hasAlpha { get { return _hasAlpha == 1; } set { _hasAlpha = value ? 1u : 0u; } }                    /**< Whether it is an alpha movie (1: with alpha, 0: without alpha) */
		public CodecType    codecType { get { return (CodecType)_codecType; } set { _codecType = (UInt32)value; } }             /**< Codec type (information to be used inside the plug-in) */
		public CodecType    alphaCodecType { get { return (CodecType)_alphaCodecType; } set { _alphaCodecType = (UInt32)value; } }  /**< Alpha codec type (information to be used inside the plug-in) */
	}


	/**
	 * <summary>Video frame information.</summary>
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public class FrameInfo
	{
		public Int32    frameNo;                /**< Frame identification number (serial number from 0) */
		public Int32    frameNoPerFile;         /**< Frame identification number (identification number specific to movie files) */
		public UInt32   width;                  /**< Movie width [pixel] (a multiple of 8) */
		public UInt32   height;                 /**< Movie height [pixel] (a multiple of 8) */
		public UInt32   dispWidth;              /**< The number of horizontal pixels of the image to be displayed (from the left edge) */
		public UInt32   dispHeight;             /**< The number of vertical pixels of the image to be displayed (from the top edge) */
		public UInt32   framerateN;             /**< Rational format frame rate (numerator) framerate [x1000] = framerateN / framerateD */
		public UInt32   framerateD;             /**< Rational format frame rate (numerator) framerate [x1000] = framerateN / framerateD */
		public UInt64   time;                   /**< Time. Seconds are represented by time/tunit. Continuously added during loop playback or linked playback. */
		public UInt64   tunit;                  /**< Time unit */
		public UInt32   cntConcatenatedMovie;   /**< The number of movie concatenation */
		AlphaType       alphaType;              /**< Alpha composition mode */
		public UInt32   cntSkippedFrames;       /**< The number of frames skipped in decoding */
		public UInt32   totalFramesPerFile;     /**< Total number of frames (specific to movie files) */
	}


	/**
	 * <summary>Event point information.</summary>
	 * <remarks>
	 * <para header='Description'>Individual event point information embedded in the movie data using the Cue point feature.<br/></para>
	 * </remarks>
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct EventPoint
	{
		public IntPtr   cueName;            /**< A pointer to the event point name. The character code follows the Cue point information text. Use it after conversion using System.Runtime.InteropServices.Marshal.PtrToStringAuto etc. */
		public UInt32   cueNameSize;        /**< Data size of the event point name */
		public UInt64   time;               /**< Timer count */
		public UInt64   tunit;              /**< Timer count value per second. count ÷ unit gives the time in seconds. */
		public Int32    type;               /**< Event point type */
		public IntPtr   paramString;        /**< A pointer to the user parameter string. The character code follows the Cue point information text. Use it after conversion using System.Runtime.InteropServices.Marshal.PtrToStringAuto etc. */
		public UInt32   paramStringSize;    /**< Data size of the user parameter string */
		public UInt32   cntCallback;        /**< Cue point callback call counter */
	}

	public static class Settings
	{

		public static void SetDecodeSkippingEnabled(bool enabled)
		{
			CriManaPlugin.CRIWAREE0F2DCC8(enabled);
		}

	}
}


public class CriManaPlugin
{
#if (UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	private enum GraphicsApi
	{
		Unknown         = -1, // Unknown (from unity cpp)
		OpenGLES_2_0    = 8,  // UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2;
		OpenGLES_3_0    = 11, // UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3;
		Metal           = 16, // UnityEngine.Rendering.GraphicsDeviceType.Metal;
	};
#endif


	/* 初期化カウンタ */
	private static int initializationCount = 0;
	public static bool isInitialized { get { return initializationCount > 0; } }

	private static bool isConfigured = false;

	private static bool enabledMultithreadedRendering = false;
	public static bool isMultithreadedRenderingEnabled { get { return enabledMultithreadedRendering; } }

	public static int renderingEventOffset = 0x43570000; // => 'CW\0\0'

	public static void SetConfigParameters(bool graphicsMultiThreaded, int num_decoders, int max_num_of_entries)
	{
#if (UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
		int graphicsApi = (int)GraphicsApi.Unknown;
	#if !UNITY_EDITOR && (UNITY_IOS || UNITY_TVOS)
		if (SystemInfo.graphicsDeviceVersion.IndexOf("OpenGL ES 2.") > -1) {
			graphicsApi = (int)GraphicsApi.OpenGLES_2_0;
		} else if (SystemInfo.graphicsDeviceVersion.IndexOf("OpenGL ES 3.") > -1) {
			graphicsApi = (int)GraphicsApi.OpenGLES_3_0;
		} else if (SystemInfo.graphicsDeviceVersion.IndexOf("Metal") > -1) {
			graphicsApi = (int)GraphicsApi.Metal;
		}
	#endif
#else
		int graphicsApi = (int)SystemInfo.graphicsDeviceType;
#endif

		enabledMultithreadedRendering = graphicsMultiThreaded;
		CriWare.Common.criWareUnity_SetRenderingEventOffsetForMana(renderingEventOffset);

		CriManaPlugin.CRIWARE4167F112((int)graphicsApi, enabledMultithreadedRendering, num_decoders, max_num_of_entries);

		CriManaPlugin.isConfigured = true;
	}

	/* VP9の設定 */
	private static void SetupVp9() {
#if !UNITY_EDITOR && UNITY_SWITCH
		return;
#endif
		Type type = Type.GetType("CriManaVp9");
		if (type == null) {
			/* Pluginが見つからなかった */
			return;
		}

		/* 現在のプラットフォームは VP9 をサポートしているか確認 */
#if !UNITY_EDITOR && (UNITY_WINRT && !ENABLE_IL2CPP)
		System.Reflection.MethodInfo method_support_current_platform = type.GetTypeInfo().GetDeclaredMethod("SupportCurrentPlatform");
#else
		System.Reflection.MethodInfo method_support_current_platform = type.GetMethod("SupportCurrentPlatform");
#endif
		if (method_support_current_platform == null)
		{
			Debug.LogError("[CRIWARE] ERROR: CriManaVp9.SupportCurrentPlatform method is not found.");
			return;
		}

		bool current_platform_supports_vp9 = (bool)method_support_current_platform.Invoke(null, null);
		if (current_platform_supports_vp9 == false)
		{
			Debug.LogError("[CRIWARE] ERROR: current platform is not supported VP9.");
			return;
		}

		/* VP9 設定関数の呼び出し */
#if !UNITY_EDITOR && (UNITY_WINRT && !ENABLE_IL2CPP)
		System.Reflection.MethodInfo method_setup_vp9_decoder = type.GetTypeInfo().GetDeclaredMethod("SetupVp9Decoder");
#else
		System.Reflection.MethodInfo method_setup_vp9_decoder = type.GetMethod("SetupVp9Decoder");
#endif
		if (method_setup_vp9_decoder == null)
		{
			Debug.LogError("[CRIWARE] ERROR: CriManaVp9.SetupVp9Decoder method is not found.");
			return;
		}

		method_setup_vp9_decoder.Invoke(null, null);
	}

	[System.Obsolete("Use CriWareVITA.EnableH264Playback and CriWareVITA.SetH264DecoderMaxSize instead.")]
	public static void SetConfigAdditonalParameters_VITA(bool use_h264_playback, int width, int height)
	{
#if !UNITY_EDITOR && UNITY_PSP2
		CriManaPlugin.CRIWAREB5A9A7A7(use_h264_playback, (UInt32)width, (UInt32)height);
#endif
	}

	public static void SetConfigAdditonalParameters_PC(bool use_h264_playback)
	{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		CriManaPlugin.CRIWARE75DBF0DC(use_h264_playback);
#endif
	}

	public static void SetConfigAdditonalParameters_ANDROID(bool use_h264_playback)
	{
#if !UNITY_EDITOR && UNITY_ANDROID
		using (var version = new AndroidJavaClass("android.os.Build$VERSION")) {
			if (version.GetStatic<int>("SDK_INT") >= 16) {
				// CRI H264 Decoder exists over API-16.
				CriManaPlugin.criManaUnity_SetConfigAdditionalParameters_ANDROID(use_h264_playback);
			}
		}
#endif
	}


	public static void SetConfigAdditonalParameters_WEBGL(string webworkerPath, uint heapSize)
	{
#if !UNITY_EDITOR && UNITY_WEBGL
		CriManaPlugin.CRIWARE8896B1AA(webworkerPath,heapSize);
#endif
	}

	public static void InitializeLibrary()
	{
		/* 初期化カウンタの更新 */
		CriManaPlugin.initializationCount++;
		if (CriManaPlugin.initializationCount != 1) {
			return;
		}

		/* シーン実行前に初期化済みの場合は終了させる */
		if (CriManaPlugin.IsLibraryInitialized() == true) {
			CriManaPlugin.FinalizeLibrary();
			CriManaPlugin.initializationCount = 1;
		}

		/* 初期化パラメータが設定済みかどうかを確認 */
		if (CriManaPlugin.isConfigured == false) {
			Debug.Log("[CRIWARE] Mana initialization parameters are not configured. "
				+ "Initializes Mana by default parameters.");
		}

		/* 依存ライブラリの初期化 */
		CriFsPlugin.InitializeLibrary();
		CriAtomPlugin.InitializeLibrary();

		/* VP9の設定 */
		CriManaPlugin.SetupVp9();

		/* ライブラリの初期化 */
		CriManaPlugin.CRIWARE293FA022();

		/* RendererResource の自動登録を実行 */
		CriMana.Detail.AutoResisterRendererResourceFactories.InvokeAutoRegister();
	}

	public static bool IsLibraryInitialized()
	{
		/* ライブラリが初期化済みかチェック */
		return CRIWARE1DC439B8();
	}

	public static void FinalizeLibrary()
	{
		/* 初期化カウンタの更新 */
		CriManaPlugin.initializationCount--;
		if (CriManaPlugin.initializationCount < 0) {
			CriManaPlugin.initializationCount = 0;
			if (CriManaPlugin.IsLibraryInitialized() == false) {
				return;
			}
		}
		if (CriManaPlugin.initializationCount != 0) {
			return;
		}

		/* 未破棄のDisposableを破棄 */
		CriDisposableObjectManager.CallOnModuleFinalization(CriDisposableObjectManager.ModuleType.Mana);

		/* ライブラリの終了 */
		CriManaPlugin.CRIWAREA256AD28();

		/* RendererResourceFactoryのインスタンスを破棄 */
		CriMana.Detail.RendererResourceFactory.DisposeAllFactories();

		/* 依存ライブラリの終了 */
		CriAtomPlugin.FinalizeLibrary();
		CriFsPlugin.FinalizeLibrary();
	}


	public static void SetDecodeThreadPriorityAndroidExperimental(int prio)
	{
		/*
		 * <summary>
		 * デコードマスタースレッドのプライオリティを変更します。
		 * </summary>
		 * <param name="prio">スレッドプライオリティ</param>
		 * <remarks name="呼び出し条件:">
		 * Manaライブラリが初期化された後に本メソッドを呼び出してください。
		 * </remarks>
		 * <remarks name="説明:">
		 * デコード処理を行うスレッドのプライオリティを変更します。
		 * デフォルトでは、デコードスレッドのプライオリティはメインスレッドよりも低く設定されています。
		 * </remarks>
		 * <attention>
		 * 本メソッドはExperimentalです。今後のアップデートで削除、または仕様変更される可能性があります。
		 * </attention>
		 */
#if !UNITY_EDITOR && UNITY_ANDROID
		criManaUnity_SetDecodeThreadPriority_ANDROID(prio);
#endif
	}

#if UNITY_2017_1_OR_NEWER
	public static bool ShouldSampleRed(UnityEngine.Rendering.GraphicsDeviceType type, IntPtr tex_ptr)
	{
#if !UNITY_EDITOR && UNITY_ANDROID
		if (type == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3) {
			return criManaUnity_ShouldSwitchTextureSampleColorToRedGLES30_ANDROID(tex_ptr);
		}
#endif
		return false;
	}
#endif

	public static void Lock()
	{
		CriManaPlugin.CRIWARE34184E44();
	}

	public static void Unlock()
	{
		CriManaPlugin.CRIWARE23309A02();
	}

	#region DLL Import
	#if !CRIWARE_ENABLE_HEADLESS_MODE
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void CRIWARE4167F112(int graphics_api, bool graphics_multi_threaded, int num_decoders, int num_of_max_entries);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void CRIWARE293FA022();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern bool CRIWARE1DC439B8();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void CRIWAREA256AD28();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern void CRIWAREE0F2DCC8(bool flag);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern void CRIWARE34184E44();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern void CRIWARE23309A02();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern void criMana_UseStreamerManager(bool flag);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern bool criMana_IsStreamerManagerUsed();

#if !UNITY_EDITOR && UNITY_WEBGL
#else
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern uint CRIWAREDCC73711();
#endif

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern void CRIWARE75DBF0DC(bool enable);
#endif

#if !UNITY_EDITOR && UNITY_ANDROID
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern void criManaUnity_SetDecodeThreadPriority_ANDROID(int prio);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criManaUnity_SetConfigAdditionalParameters_ANDROID(bool enable_h264_playback);

#if UNITY_2017_1_OR_NEWER
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern void criManaUnity_EnableSwitchTextureSampleColorGLES30_ANDROID();

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criManaUnity_ShouldSwitchTextureSampleColorToRedGLES30_ANDROID(System.IntPtr tex_ptr);
#endif
#elif !UNITY_EDITOR && UNITY_PSP2
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern void CRIWAREB5A9A7A7(bool use_h264_playback, UInt32 width, UInt32 height);
#elif !UNITY_EDITOR && UNITY_WEBGL
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	public static extern void CRIWARE8896B1AA(string webworkerPath, uint heapSize);
#endif
	#else
	private static bool _dummyInitialized = false;
	private static void CRIWARE4167F112(int graphics_api, bool graphics_multi_threaded, int num_decoders, int num_of_max_entries) { }
	private static void CRIWARE293FA022() { _dummyInitialized = true; }
	public static bool CRIWARE1DC439B8() { return _dummyInitialized; }
	private static void CRIWAREA256AD28() { _dummyInitialized = false; }
	public static void CRIWAREE0F2DCC8(bool flag) { }
	public static void CRIWARE34184E44() { }
	public static void CRIWARE23309A02() { }
	public static void criMana_UseStreamerManager(bool flag) { }
	public static bool criMana_IsStreamerManagerUsed() { return false; }
#if !UNITY_EDITOR && UNITY_WEBGL
#else
	public static uint CRIWAREDCC73711() { return 0u; }
#endif
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
	public static void CRIWARE75DBF0DC(bool enable) { }
#endif
#if !UNITY_EDITOR && UNITY_ANDROID
	public static extern criManaUnity_SetDecodeThreadPriority_ANDROID(int prio) { }
	private static void criManaUnity_SetConfigAdditionalParameters_ANDROID(bool enable_h264_playback) { }
#if UNITY_2017_1_OR_NEWER
	public static void criManaUnity_EnableSwitchTextureSampleColorGLES30_ANDROID() { }
	private static bool criManaUnity_ShouldSwitchTextureSampleColorToRedGLES30_ANDROID(System.IntPtr tex_ptr) { return false; }
#endif
#elif !UNITY_EDITOR && UNITY_PSP2
	public static void CRIWAREB5A9A7A7(bool use_h264_playback, UInt32 width, UInt32 height) { }
#elif !UNITY_EDITOR && UNITY_WEBGL
	public static void CRIWARE8896B1AA(string webworkerPath, uint heapSize) { }
#endif
	#endif
	#endregion
}

/* end of file */
