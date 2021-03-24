/****************************************************************************
 *
 * Copyright (c) 2011 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

/*==========================================================================
 *      CRI Atom Native Wrapper
 *=========================================================================*/
/**
 * \addtogroup CRIATOM_NATIVE_WRAPPER
 * @{
 */


/**
 * <summary>Playback sound object</summary>
 * <remarks>
 * <para header='Description'>On object returned when the function ::CriAtomExPlayer::Start is called.<br/>
 * If you want to change parameters or get the status for each sound played back using the function ::CriAtomExPlayer::Start ,
 * not for each player, you need to use this object for controlling playback.<br/></para>
 * </remarks>
 * <seealso cref='CriAtomExPlayer::Start'/>
 */
public struct CriAtomExPlayback
{
	/**
	 * <summary>Playback status</summary>
	 * <remarks>
	 * <para header='Description'>Status of the sound that has been played on the AtomExPlayer.<br/>
	 * It can be obtained by using the ::CriAtomExPlayback::GetStatus function.<br/>
	 * <br/>
	 * The playback status usually changes in the following order.<br/>
	 * -# Prep
	 * -# Playing
	 * -# Removed
	 * .</para>
	 * <para header='Note'>Status indicates the status of the sound that was played
	 * ( ::CriAtomExPlayer::Start function was called) by the AtomExPlayer
	 * instead of the status of the player.<br/>
	 * <br/>
	 * The sound resource being played is discarded when the playback is stopped.<br/>
	 * Therefore, the status of the playback sound changes to Removed in the following cases.<br/>
	 * - When playback is complete.
	 * - When the sound being played is stopped using the Stop function.
	 * - When a Voice being played is stolen by a high priority playback request.
	 * - When an error occurred during playback.
	 * .</para>
	 * </remarks>
	 * <seealso cref='CriAtomExPlayer::Start'/>
	 * <seealso cref='CriAtomExPlayback::GetStatus'/>
	 * <seealso cref='CriAtomExPlayback::Stop'/>
	 */
	public enum Status {
		Prep = 1,   /**< Preparing for playback */
		Playing,    /**< Playing */
		Removed     /**< Removed */
	}

	/**
	 * <summary>Playback Track information</summary>
	 * <remarks>
	 * <para header='Description'>A structure to get the Track information of the Cue being played.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExPlayback::GetTrackInfo'/>
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct TrackInfo {
		public uint id;                         /**< Playback ID */
		public CriAtomEx.CueType sequenceType;  /**< Parent Sequence type */
		public IntPtr playerHn;                 /**< Player handle */
		public ushort trackNo;                  /**< Track number */
		public ushort reserved;                 /**< Reserved area */
	}

	public CriAtomExPlayback(uint id)
		: this()
	{
		this.id = id;
	#if CRIWARE_ENABLE_HEADLESS_MODE
		this._dummyStatus = Status.Prep;
	#endif
	}

	/**
	 * <summary>Stops the playback sound</summary>
	 * <param name='ignoresReleaseTime'>Whether to ignore release time
	 * (False = perform release process, True = ignore release time and stop immediately)</param>
	 * <remarks>
	 * <para header='Description'>Stops for each played sound. <br/>
	 * By using this function, it is possible to pause the sound played
	 * for each individual sound on the player.<br/></para>
	 * <para header='Note'>If you want to stop all the sounds played back by the AtomExPlayer,
	 * use the ::CriAtomExPlayer::Stop function instead of this function.<br/>
	 * (The ::CriAtomExPlayer::Stop function stops all the sounds being played by the player.)<br/></para>
	 * <para header='Note'>When the playback sound is stopped using this function, the status of the sound being played changes to Removed.<br/>
	 * Since the Voice resource is also discarded when stopped, it will not be possible to acquire information
	 * from the playback object that changed to Removed state.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExPlayer::Stop'/>
	 * <seealso cref='CriAtomExPlayback::GetStatus'/>
	 */
	public void Stop(bool ignoresReleaseTime)
	{
		if (CriAtomPlugin.IsLibraryInitialized() == false) { return; }

		if (ignoresReleaseTime == false) {
			criAtomExPlayback_Stop(this.id);
		} else {
			criAtomExPlayback_StopWithoutReleaseTime(this.id);
		}
	}

	/**
	 * <summary>Pauses playback sound</summary>
	 * <remarks>
	 * <para header='Description'>Pauses for each played sound. <br/>
	 * <br/>
	 * By using this function, it is possible to pause the sound played
	 * for each individual sound on the player.<br/></para>
	 * <para header='Note'>If you want to pause all the sounds played back by the player,
	 * use the ::CriAtomExPlayer::Pause function instead of this function.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExPlayback::IsPaused'/>
	 * <seealso cref='CriAtomExPlayer::Pause'/>
	 * <seealso cref='CriAtomExPlayback::Resume'/>
	 */
	public void Pause()
	{
		criAtomExPlayback_Pause(this.id, true);
	}

	/**
	 * <summary>Unpauses the playback sound</summary>
	 * <param name='mode'>Unpausing target</param>
	 * <remarks>
	 * <para header='Description'>Unpauses for each played sound. <br/>
	 * When this function is called by specifying PausedPlayback in the argument (mode),
	 * the playback of the sound paused by the user using
	 * the ::CriAtomExPlayer::Pause function (or the ::CriAtomExPlayback::Pause function) is resumed.<br/>
	 * When this function is called by specifying PreparedPlayback in the argument (mode),
	 * the playback of the sound prepared by the user using the::CriAtomExPlayer::Prepare starts.<br/></para>
	 * <para header='Note'>If you prepare a playback using the ::CriAtomExPlayer::Prepare function for the player paused using the ::CriAtomExPlayer::Pause function,
	 * the sound is not played back until it is unpaused using PausedPlayback
	 * as well as unpaused with PreparedPlayback.<br/>
	 * <br/>
	 * If you want to always start playback regardless of whether
	 * the sound is paused using the ::CriAtomExPlayer::Pause or ::CriAtomExPlayer::Prepare function, call this function by specifying AllPlayback in the argument (mode).<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExPlayback::IsPaused'/>
	 * <seealso cref='CriAtomExPlayer::Resume'/>
	 * <seealso cref='CriAtomExPlayer::Pause'/>
	 */
	public void Resume(CriAtomEx.ResumeMode mode)
	{
		criAtomExPlayback_Resume(this.id, mode);
	}

	/**
	 * <summary>Gets pausing status of the playback sound</summary>
	 * <returns>Whether the playback is paused (False = not paused, True = paused)</returns>
	 * <remarks>
	 * <para header='Description'>Returns whether or not the sound being played is paused.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExPlayback::Pause'/>
	 */
	public bool IsPaused()
	{
		return criAtomExPlayback_IsPaused(this.id);
	}

	/**
	 * <summary>Gets the playback sound format information</summary>
	 * <param name='info'>Format information</param>
	 * <returns>Whether the information can be acquired (True = could be acquired, False = could not be acquired)</returns>
	 * <remarks>
	 * <para header='Description'>Gets the format information of the sound played back using the ::CriAtomExPlayer::Start function.<br/>
	 * <br/>
	 * This function returns True if the format information can be obtained.<br/>
	 * This function returns False if the specified Voice was already deleted.<br/></para>
	 * <para header='Note'>When playing a Cue that contains multiple sound data, the information on
	 * the first sound data found is returned.<br/></para>
	 * <para header='Note'>This function can get the format information only during sound playback.<br/>
	 * If the Voice is deleted by Voice control during playback preparation or after the playback,
	 * acquisition of format information fails.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExPlayer::Start'/>
	 * <seealso cref='CriAtomExPlayer::GetStatus'/>
	 */
	public bool GetFormatInfo(out CriAtomEx.FormatInfo info)
	{
		return criAtomExPlayback_GetFormatInfo(this.id, out info);
	}

	/**
	 * <summary>Gets the playback status</summary>
	 * <returns>Playback status</returns>
	 * <remarks>
	 * <para header='Description'>Gets the status of the sound played back using the ::CriAtomExPlayer::Start function.<br/></para>
	 * <para header='Note'>This function gets the status of each sound already played while the
	 * ::CriAtomExPlayer::GetStatus function returns the status of the AtomExPlayer.<br/>
	 * <br/>
	 * The Voice resource of the sound being played is released in the following cases.<br/>
	 * - When playback is complete.
	 * - When the sound being played is stopped using the Stop function.
	 * - When a Voice being played is stolen by a high priority playback request.
	 * - When an error occurred during playback.
	 * .
	 * Therefore, regardless of whether you explicitly stopped playback using the ::CriAtomExPlayback::Stop function
	 * or the playback is stopped due to some other factor,
	 * the status of the sound changes to Removed in all cases.<br/>
	 * (If you need to detect the occurrence of an error, it is necessary to check the status of the AtomExPlayer
	 * using the ::CriAtomExPlayer::GetStatus function instead of this function.)<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExPlayer::Start'/>
	 * <seealso cref='CriAtomExPlayer::GetStatus'/>
	 * <seealso cref='CriAtomExPlayback::Stop'/>
	 */
	public Status GetStatus()
	{
		return criAtomExPlayback_GetStatus(this.id);
	}

	/**
	 * <summary>Gets the playback time</summary>
	 * <returns>Playback time (in milliseconds)</returns>
	 * <remarks>
	 * <para header='Description'>Gets the playback time of the sound played back using the ::CriAtomExPlayer::Start function.<br/>
	 * <br/>
	 * This function returns a value of 0 or greater if the playback time can be obtained.<br/>
	 * This function returns a negative value if the specified Voice was already deleted.<br/></para>
	 * <para header='Note'>The playback time returned by this function is "the elapsed time from the start of playback".<br/>
	 * The time does not rewind depending on the playback position,
	 * even during loop playback or seamless linked playback.<br/>
	 * <br/>
	 * When the playback is paused using the ::CriAtomExPlayer::Pause function,
	 * the playback time count-up also stops.<br/>
	 * (If you unpause the playback, the count-up resumes.)
	 * <br/>
	 * The accuracy of the time that can be obtained by this function depends on the frequency of the server processing.<br/>
	 * (The time is updated for each server process.)<br/>
	 * If you need to get more accurate time, use the
	 * ::CriAtomExPlayback::GetNumPlayedSamples function instead of this function
	 * to get the number of samples played.<br/></para>
	 * <para header='Note'>The return type is long, but currently there is no precision over 32bit.<br/>
	 * When performing control based on the playback time, it should be noted that the playback time becomes incorrect in about 24 days.<br/>
	 * (The playback time overflows and becomes a negative value when it exceeds 2147483647 milliseconds.)<br/>
	 * <br/>
	 * This function can get the time only during sound playback.<br/>
	 * (Unlike the ::CriAtomExPlayer::GetTime function, this function can get the time
	 * for each sound being played, but it cannot get the playback end time.)<br/>
	 * Getting the playback time fails after the playback ends
	 * or when the Voice is erased by the Voice control.<br/>
	 * (Negative value is returned.)<br/>
	 * <br/>
	 * If the sound data supply is temporarily interrupted due to device read retry processing, etc.,
	 * the count-up of the playback time is not interrupted.<br/>
	 * (The time progresses even if the playback is stopped due to the stop of data supply.)<br/>
	 * Therefore, when synchronizing sound with the source video based on the time acquired by this function,
	 * the synchronization may be greatly deviated each time a read retry occurs.<br/>
	 * If it is necessary to strictly synchronize the waveform data and video,
	 * use the ::CriAtomExPlayback::GetNumPlayedSamples function instead of this function
	 * to synchronize with the number of played samples.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExPlayer::Start'/>
	 * <seealso cref='CriAtomExPlayer::GetTime'/>
	 * <seealso cref='CriAtomExPlayback::GetNumPlayedSamples'/>
	 */
	public long GetTime()
	{
		return criAtomExPlayback_GetTime(this.id);
	}

	/**
	 * <summary>Gets the playback time synchronized with sound</summary>
	 * <returns>Playback time (in milliseconds)</returns>
	 * <remarks>
	 * <para header='Description'>Gets the playback time of the sound played back using the ::CriAtomExPlayer::Start function.<br/>
	 * <br/>
	 * This function returns a value of 0 or greater if the playback time can be obtained.<br/>
	 * This function returns a negative value if the specified Voice was already deleted.<br/></para>
	 * <para header='Note'>Unlike the "elapsed time from the start of playback" returned by the ::CriAtomExPlayback::GetTime function,
	 * it is possible to obtain the playback time synchronized with the sound being played
	 * using this function.<br/>
	 * If the sound data supply is interrupted due to device read retry, etc.,
	 * and the playback is stopped, the count-up of the playback time is temporarily stopped.<br/>
	 * If you want to strictly synchronize with the sound played,
	 * use the playback time acquired using this function.<br/>
	 * However, the time does not rewind depending on the playback position,
	 * even during loop playback or seamless linked playback.<br/>
	 * In addition, the playback time cannot be acquired successfully
	 * for the Sequence Cue not filled with waveforms.<br/>
	 * <br/>
	 * When the playback is paused using the ::CriAtomExPlayer::Pause function,
	 * the playback time count-up also stops.<br/>
	 * (If you unpause the playback, the count-up resumes.)<br/>
	 * <br/>
	 * To get the playback time using this function, use the ::CriAtomExPlayer::CriAtomExPlayer(bool) function
	 * to create a player specifying a flag that enables the sound synchronization timer.
	 * <br/></para>
	 * <para header='Note'>The return type is long, but currently there is no precision over 32bit.<br/>
	 * When performing control based on the playback time, it should be noted that the playback time becomes incorrect in about 24 days.<br/>
	 * (The playback time overflows and becomes a negative value when it exceeds 2147483647 milliseconds.)<br/>
	 * <br/>
	 * This function can get the time only during sound playback.<br/>
	 * (Unlike the ::CriAtomExPlayer::GetTime function, this function can get the time
	 * for each sound being played, but it cannot get the playback end time.)<br/>
	 * Getting the playback time fails after the playback ends
	 * or when the Voice is erased by the Voice control.<br/>
	 * (Negative value is returned.)<br/>
	 * <br/>
	 * This function calculates the time internally, so the processing load
	 * may be a problem depending on the platform. In addition, it returns the updated time with each call,
	 * even within the same frame of the application.<br/>
	 * Although it depends on how the playback time is used by the application,
	 * basically use this function to get the time only once per frame.<br/>
	 * <br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExPlayer::Start'/>
	 * <seealso cref='CriAtomExPlayback::GetTime'/>
	 * <seealso cref='CriAtomExPlayback::GetNumPlayedSamples'/>
	 * <seealso cref='CriAtomExPlayer::CriAtomExPlayer(bool)'/>
	 */
	public long GetTimeSyncedWithAudio()
	{
		return criAtomExPlayback_GetTimeSyncedWithAudio(this.id);
	}

	/**
	 * <summary>Gets the number of playback samples</summary>
	 * <param name='numSamples'>The number of played samples</param>
	 * <param name='samplingRate'>Sampling rate</param>
	 * <returns>Whether the sample count can be acquired (True = could be acquired, False = could not be acquired)</returns>
	 * <remarks>
	 * <para header='Description'>Returns the number of playback samples and the sampling rate of the
	 * sound played back using the ::CriAtomExPlayer::Start function.<br/>
	 * <br/>
	 * This function returns True if the number of playback samples can be obtained.<br/>
	 * This function returns False if the specified Voice was already deleted.<br/>
	 * (When an error occurs, the values of numSamples and samplingRate are also negative.)<br/></para>
	 * <para header='Note'>The accuracy of the number of samples played depends on the
	 * sound library in the platform SDK.<br/>
	 * (The accuracy of the number of samples played depends on the platform.)<br/>
	 * <br/>
	 * When playing a Cue that contains multiple sound data, the information
	 * on the first sound data found is returned.<br/></para>
	 * <para header='Note'>If the sound data supply is interrupted due to device read retry processing, etc.,
	 * the count-up of the number of playback samples stops.<br/>
	 * (The count-up restarts when the data supply is resumed.)<br/>
	 * <br/>
	 * This function can get the number of playback samples only while the sound is being played.<br/>
	 * Getting the number of playback samples fails after the playback ends
	 * or when the Voice is erased by the Voice control.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExPlayer::Start'/>
	 */
	public bool GetNumPlayedSamples(out long numSamples, out int samplingRate)
	{
		return criAtomExPlayback_GetNumPlayedSamples(this.id, out numSamples, out samplingRate);
	}

	/**
	 * <summary>Gets the Sequence playback position</summary>
	 * <returns>Sequence playback position (in milliseconds)</returns>
	 * <remarks>
	 * <para header='Description'>Gets the Sequence playback position of the sound played back using the ::CriAtomExPlayer::Start function.<br/>
	 * <br/>
	 * This function returns a value of 0 or greater if the playback position can be obtained.<br/>
	 * This function returns a negative value if, for example, the specified Sequence was already deleted.<br/></para>
	 * <para header='Note'>The playback time returned by this function is the "playback position on the Sequence data".<br/>
	 * When a Sequence loop or Block transition was performed, the rewound value is returned.<br/>
	 * <br/>
	 * The Sequencer does not run if you performed non-Cue-specified playback. For playback other than Cue playback,
	 * this function returns a negative value.<br/>
	 * <br/>
	 * When the playback is paused using the ::CriAtomExPlayer::Pause function, the playback position also stops being updated.<br/>
	 * (If you unpause, the update resumes.)
	 * <br/>
	 * The accuracy of the time that can be obtained by this function depends on the frequency of the server processing.<br/>
	 * (The time is updated for each server process.)<br/></para>
	 * <para header='Note'>The return type is long, but currently there is no precision over 32bit.<br/>
	 * When performing control based on the playback position, it should be noted that
	 * the playback position becomes incorrect in about 24 days for the data that has no settings such as Sequence loops.<br/>
	 * (The playback position overflows and becomes a negative value when it exceeds 2147483647 milliseconds.)<br/>
	 * <br/>
	 * This function can get the position only during sound playback.<br/>
	 * After the end of playback, or when the Sequence was erased by Voice control,
	 * acquisition of the playback position fails.<br/>
	 * (Negative value is returned.)<br/></para>
	 * </remarks>
	 *
	 */
	public long GetSequencePosition()
	{
		return criAtomExPlayback_GetSequencePosition(this.id);
	}

	/**
	 * <summary>Gets the current Block index of the playback sound</summary>
	 * <returns>Current Block index</returns>
	 * <remarks>
	 * <para header='Description'>Gets the current Block index of the Block sequence
	 * played back using the ::CriAtomExPlayer::Start function.<br/></para>
	 * <para header='Note'>Returns 0xFFFFFFFF if the data being played by the playback ID is not a Block Sequence.<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExPlayer::Start'/>
	 * <seealso cref='CriAtomExPlayer::SetFirstBlockIndex'/>
	 * <seealso cref='CriAtomExPlayback::SetNextBlockIndex'/>
	 */
	public int GetCurrentBlockIndex()
	{
		return criAtomExPlayback_GetCurrentBlockIndex(this.id);
	}

	/**
	 * <summary>Gets the playback Track information</summary>
	 * <param name='info'>Playback Track information</param>
	 * <returns>Whether the acquisition succeeded</returns>
	 * <remarks>
	 * <para header='Description'>Gets the Track information of the Cue played back using the ::CriAtomExPlayer::Start function.<br/>
	 * The Track information that can be obtained is only the information directly under the Cue; the information for sub-Sequence or Cue link cannot be obtained.<br/></para>
	 * <para header='Note'>An attempt to get the Track information fails if the following data is being played.<br/>
	 * - Data other than Cue is being played. (Since there is no Track information)<br/>
	 * - The Cue being played is a Polyphonic type or a selector-referenced switch type.
	 *   (Since there may be multiple Track information)<br/>
	 * - The Cue being played is a Track Transition type. (Since the playback Track changes by transition)<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExPlayer::Start'/>
	 */
	public bool GetTrackInfo(out TrackInfo info)
	{
		return criAtomExPlayback_GetPlaybackTrackInfo(this.id, out info);
	}

	/**
	 * <summary>Gets beat synchronization information</summary>
	 * <param name='info'>Beat synchronization information</param>
	 * <returns>Whether the acquisition succeeded</returns>
	 * <remarks>
	 * <para header='Description'>Gets the beat synchronization information of the Cue played back using the ::CriAtomExPlayer::Start function.<br/>
	 * You can get the current BPM, bar count, beat count, and beat progress rate (0.0 to 1.0).<br/>
	 * Beat synchronization information must be set for the Cue.<br/>
	 * Information for the Cues that are being played using Cue links or start actions cannot be acquired.<br/></para>
	 * <para header='Note'>An attempt to get the beat synchronization information fails if the following data is being played.<br/>
	 * - Data other than Cue is being played. (Since there is not beat synchronization information)<br/>
	 * - A Cue without beat synchronization information is being played.<br/>
	 * - A Cue with beat synchronization information is being played "indirectly".
	 *   (Being played with Cue link or start action)<br/></para>
	 * </remarks>
	 * <seealso cref='CriAtomExPlayer::Start'/>
	 */
	public bool GetBeatSyncInfo(out CriAtomExBeatSync.Info info)
	{
		return criAtomExPlayback_GetBeatSyncInfo(this.id, out info);
	}

	/**
	 * <summary>Block transition of playback sound</summary>
	 * <param name='index'>Block index</param>
	 * <remarks>
	 * <para header='Description'>Block transition is performed for each played sound. <br/>
	 * When this function is executed, if the Voice with the specified ID is a block sequence,
	 * It transitions the block according to the specified block sequence setting. <br/></para>
	 * <para header='Note'>Use the ::CriAtomExPlayer::SetFirstBlockIndex function to specify the playback start Block,
	 * and use the ::CriAtomExPlayback::GetCurrentBlockIndex function to get the Block index during playback.</para>
	 * </remarks>
	 * <seealso cref='CriAtomExPlayer::SetFirstBlockIndex'/>
	 * <seealso cref='CriAtomExPlayback::GetCurrentBlockIndex'/>
	 */
	public void SetNextBlockIndex(int index)
	{
		criAtomExPlayback_SetNextBlockIndex(this.id, index);
	}

	public uint id
	{
		get;
		private set;
	}

	public Status status
	{
		get {
			return this.GetStatus();
		}
	}

	public long time
	{
		get {
			return this.GetTime();
		}
	}

	public long timeSyncedWithAudio
	{
		get {
			return this.GetTimeSyncedWithAudio();
		}
	}

	public const uint invalidId = 0xFFFFFFFF;

	/* Old APIs */
	public void Stop() {
		if (CriAtomPlugin.IsLibraryInitialized() == false) { return; }
		criAtomExPlayback_Stop(this.id);
	}
	public void StopWithoutReleaseTime() {
		if (CriAtomPlugin.IsLibraryInitialized() == false) { return; }
		criAtomExPlayback_StopWithoutReleaseTime(this.id);
	}
	public void Pause(bool sw) { criAtomExPlayback_Pause(this.id, sw); }


	#region DLL Import
	#if !CRIWARE_ENABLE_HEADLESS_MODE
	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExPlayback_Stop(uint id);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExPlayback_StopWithoutReleaseTime(uint id);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExPlayback_Pause(uint id, bool sw);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExPlayback_Resume(uint id, CriAtomEx.ResumeMode mode);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomExPlayback_IsPaused(uint id);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern Status criAtomExPlayback_GetStatus(uint id);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomExPlayback_GetFormatInfo(
		uint id, out CriAtomEx.FormatInfo info);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern long criAtomExPlayback_GetTime(uint id);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern long criAtomExPlayback_GetTimeSyncedWithAudio(uint id);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomExPlayback_GetNumPlayedSamples(
		uint id, out long num_samples, out int sampling_rate);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern long criAtomExPlayback_GetSequencePosition(uint id);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern void criAtomExPlayback_SetNextBlockIndex(uint id, int index);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern int criAtomExPlayback_GetCurrentBlockIndex(uint id);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomExPlayback_GetPlaybackTrackInfo(uint id, out TrackInfo info);

	[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
	private static extern bool criAtomExPlayback_GetBeatSyncInfo(uint id, out CriAtomExBeatSync.Info info);
	#else
	private Status _dummyStatus;
	private bool _dummyPaused;
	private void criAtomExPlayback_Stop(uint id) { _dummyStatus = Status.Removed; }
	private void criAtomExPlayback_StopWithoutReleaseTime(uint id) { _dummyStatus = Status.Removed; }
	private void criAtomExPlayback_Pause(uint id, bool sw) { _dummyPaused = sw; }
	private static void criAtomExPlayback_Resume(uint id, CriAtomEx.ResumeMode mode) { }
	private bool criAtomExPlayback_IsPaused(uint id) { return _dummyPaused; }
	private Status criAtomExPlayback_GetStatus(uint id)
	{
		if (_dummyStatus != Status.Removed) {
			_dummyStatus = _dummyStatus + 1;
		}
		return _dummyStatus;
	}
	private static bool criAtomExPlayback_GetFormatInfo(
		uint id, out CriAtomEx.FormatInfo info) { info = new CriAtomEx.FormatInfo(); return false; }
	private static long criAtomExPlayback_GetTime(uint id) { return 0; }
	private static long criAtomExPlayback_GetTimeSyncedWithAudio(uint id) { return 0; }
	private static bool criAtomExPlayback_GetNumPlayedSamples(
		uint id, out long num_samples, out int sampling_rate) { num_samples = sampling_rate = 0; return false; }
	private static long criAtomExPlayback_GetSequencePosition(uint id) { return 0; }
	private static void criAtomExPlayback_SetNextBlockIndex(uint id, int index) { }
	private static int criAtomExPlayback_GetCurrentBlockIndex(uint id) { return -1; }
	private static bool criAtomExPlayback_GetPlaybackTrackInfo(uint id, out TrackInfo info) { info = new TrackInfo(); return false; }
	private static bool criAtomExPlayback_GetBeatSyncInfo(uint id, out CriAtomExBeatSync.Info info) { info = new CriAtomExBeatSync.Info(); return false; }
	#endif
	#endregion
}

/**
 * @}
 */

/* --- end of file --- */
