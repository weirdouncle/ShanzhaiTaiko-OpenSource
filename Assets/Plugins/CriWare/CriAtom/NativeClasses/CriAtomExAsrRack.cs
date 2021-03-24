/****************************************************************************
 *
 * Copyright (c) 2016 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

/*---------------------------
 * Asr Support Defines
 *---------------------------*/
#if !UNITY_PSP2
#define CRIWARE_SUPPORT_ASR
#endif

using System;
using System.Runtime.InteropServices;

/*==========================================================================
 *      CRI Atom Native Wrapper
 *=========================================================================*/
/**
 * \addtogroup CRIATOM_NATIVE_WRAPPER
 * @{
 */


/**
 * <summary>ASR Rack</summary>
 */
public partial class CriAtomExAsrRack : CriDisposable
{
	#region Data Types
	/**
	 * <summary>A config structure for creating an ASR Rack</summary>
	 * <remarks>
	 * <para header='Description'>A structure for specifying the behavior of CriAtomExAsrRack.<br/>
	 * You pass this structure as an argument when creating a module (the ::CriAtomExAsrRack::CriAtomExAsrRack function).<br/></para>
	 * <para header='Note'>Change the default configuration obtained using ::CriAtomExAsrRack::defaultConfig
	 * if required.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExAsrRack::CriAtomExAsrRack'/>
	 * <seealso cref='CriAtomExAsrRack::defaultConfig'/>
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct Config
	{
		/**
		 * <summary>Frequency of server process</summary>
		 * <remarks>
		 * <para header='Description'>Specifies the frequency of running the server process</para>
		 * <para header='Note'>Set the same value as the one set to CriAtomConfig::serverFrequency for CriWareInitializer.</para>
		 * </remarks>
		 */
		public float serverFrequency;

		/**
		 * <summary>The number of buses</summary>
		 * <remarks>
		 * <para header='Description'>Specifies the number of buses that ASR creates.<br/>
		 * The bus is responsible for mixing sounds and managing the DSPeffects.</para>
		 * </remarks>
		 */
		public int numBuses;

		/**
		 * <summary>The number of output channels</summary>
		 * <remarks>
		 * <para header='Description'>Specifies the number of output channels of the ASR Rack.<br/>
		 * Specify 6ch or more channels when using pan 3D or 3D Positioning features.</para>
		 * </remarks>
		 */
		public int outputChannels;

		/**
		 * <summary>Output sampling rate</summary>
		 * <remarks>
		 * <para header='Description'>Specifies the output and processing sampling rate of the ASR Rack.<br/>
		 * Normally, specify the sampling rate of the sound device on the target machine.</para>
		 * <para header='Note'>Lowering it lowers the processing load but lowers the sound quality.</para>
		 * </remarks>
		 */
		public int outputSamplingRate;

		/**
		 * <summary>Sound renderer type</summary>
		 * <remarks>
		 * <para header='Description'>Specifies the type of the output destination sound renderer of the ASR Rack.<br/>
		 * If you specify CriAtomEx.SoundRendererType.Native for soundRendererType,
		 * the sound data is transferred to each platform's default output.</para>
		 * </remarks>
		 */
		public CriAtomEx.SoundRendererType soundRendererType;

		/**
		 * <summary>Destination ASR Rack ID</summary>
		 * <remarks>
		 * <para header='Description'>Specifies the ASR Rack ID of the ASR Rack's output destination.<br/>
		 * Valid only when you specify CriAtomEx.SoundRendererType.Asr for soundRendererType.</para>
		 * </remarks>
		 */
		public int outputRackId;

		/**
		 * <summary>Pointer to platform-specific parameters</summary>
		 * <remarks>
		 * <para header='Description'>Specifies a pointer to platform-specific parameters.<br/>
		 * When using it as the argument to the CriAtomExAsrRack::CriAtomExAsrRack function,
		 * specify IntPtr.Zero because it is overwritten by the second argument PlatformContext.</para>
		 * </remarks>
		 */
		public IntPtr context;
	}

	/**
	 * <summary>A platform-specific config structure for creating an ASR Rack</summary>
	 * <remarks>
	 * <para header='Description'>A structure for specifying the behavior of CriAtomExAsrRack.<br/>
	 * You pass this structure as an argument when creating a module (the ::CriAtomExAsrRack::CriAtomExAsrRack function).<br/>
	 * For details, refer to the platform-specific manual.</para>
	 * </remarks>
	 * <seealso cref='CriAtomExAsrRack::CriAtomExAsrRack'/>
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct PlatformConfig
	{
	#if !UNITY_EDITOR && UNITY_PS4
		public int userId;
		public CriWarePS4.AudioPortType portType;
		public CriWarePS4.AudioPortAttribute portAttr;
	#else
		public byte reserved;
	#endif
	}
	#endregion

	/**
	 * <summary>Creating an ASR Rack</summary>
	 * <param name='config'>Config structure</param>
	 * <param name='platformConfig'>Platform-specific parameter structure</param>
	 * <returns>ASR Rack</returns>
	 * <remarks>
	 * <para header='Description'>Creates an ASR Rack.<br/>
	 * Be sure to discard the ASR Rack created by this function using the Dispose function.</para>
	 * </remarks>
	 */
	public CriAtomExAsrRack(Config config, PlatformConfig platformConfig)
	{
	#if CRIWARE_SUPPORT_ASR
		this._rackId = CRIWAREFB1E1E0A(ref config, ref platformConfig);
		if (config.context != IntPtr.Zero) {
			Marshal.FreeHGlobal(config.context);
		}
		if (this._rackId == -1) {
			throw new Exception("CriAtomExAsrRack() failed.");
		}

		CriDisposableObjectManager.Register(this, CriDisposableObjectManager.ModuleType.Atom);
	#else
		this._rackId = -1;
	#endif
	}

	/**
	 * <summary>Discards an ASR Rack</summary>
	 * <remarks>
	 * <para header='Description'>Discards an ASR Rack.</para>
	 * </remarks>
	 */
	public override void Dispose()
	{
	#if CRIWARE_SUPPORT_ASR
		CriDisposableObjectManager.Unregister(this);

		if (this._rackId != -1) {
			criAtomExAsrRack_Destroy(this._rackId);
			this._rackId = -1;
		}
	#endif
		GC.SuppressFinalize(this);
	}

	public int rackId {
		get { return this._rackId; }
	}

	#region Static Properties
	/**
	 * <summary>Default configuration</summary>
	 * <remarks>
	 * <para header='Description'>Default config.</para>
	 * <para header='Note'>Change the default configuration obtained using this property
	 * and specify it in the ::CriAtomExAsrRack::CriAtomExAsrRack function.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExAsrRack::CriAtomExAsrRack'/>
	 */
	public static Config defaultConfig {
		get {
			Config config;
			config.serverFrequency = 60.0f;
			config.numBuses = 8;
			config.soundRendererType = CriAtomEx.SoundRendererType.Native;
			config.outputRackId = 0;
			config.context = System.IntPtr.Zero;
	#if !UNITY_EDITOR && UNITY_PS4
			config.outputChannels = 8;
			config.outputSamplingRate = 48000;
	#elif !UNITY_EDITOR && UNITY_IOS || UNITY_ANDROID
			config.outputChannels = 2;
			config.outputSamplingRate = 44100;
	#elif !UNITY_EDITOR && UNITY_PSP2
			config.outputChannels = 2;
			config.outputSamplingRate = 48000;
	#else
			config.outputChannels = 6;
			config.outputSamplingRate = 48000;
	#endif
			return config;
		}
	}

	/**
	 * <summary>Default ASR Rack ID</summary>
	 * <remarks>
	 * <para header='Description'>Default ASR Rack ID.
	 * When returning to normal output or discarding the generated ASR Rack, use this constant
	 * for each player to specify the ASR Rack ID.</para>
	 * </remarks>
	 * <seealso cref='CriAtomExPlayer::SetAsrRackId'/>
	 * <seealso cref='CriMana::Player::SetAsrRackId'/>
	 */
	public static int defaultRackId = 0;

	#endregion


	#region internal members
	~CriAtomExAsrRack()
	{
		this.Dispose();
	}

	private int _rackId = -1;
	#endregion

	#region DLL Import
	#if CRIWARE_SUPPORT_ASR
	#if !CRIWARE_ENABLE_HEADLESS_MODE
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int CRIWAREFB1E1E0A([In] ref Config config, [In] ref PlatformConfig platformConfig);
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAsrRack_Destroy(Int32 rackId);
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAsrRack_AttachDspBusSetting(Int32 rackId, string setting, IntPtr work, Int32 workSize);
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAsrRack_DetachDspBusSetting(Int32 rackId);
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExAsrRack_ApplyDspBusSnapshot(Int32 rackId, string snapshotName, Int32 timeMs);
	#else
	private static int CRIWAREFB1E1E0A([In] ref Config config, [In] ref PlatformConfig platformConfig) { return 0; }
	private static void criAtomExAsrRack_Destroy(Int32 rackId) { }
	private static void criAtomExAsrRack_AttachDspBusSetting(Int32 rackId, string setting, IntPtr work, Int32 workSize) { }
	private static void criAtomExAsrRack_DetachDspBusSetting(Int32 rackId) { }
	private static void criAtomExAsrRack_ApplyDspBusSnapshot(Int32 rackId, string snapshotName, Int32 timeMs) { }
	#endif
	#endif
	#endregion
}

/**
 * @}
 */

/* --- end of file --- */
