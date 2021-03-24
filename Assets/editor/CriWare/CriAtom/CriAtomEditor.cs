/****************************************************************************
 *
 * Copyright (c) 2011 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;


[CustomEditor(typeof(CriAtom))]
public class CriAtomEditor : Editor
{
	#region Variables
	private CriAtom atom = null;
	#endregion

	#region GUI Functions
	private void OnEnable()
	{
		atom = (CriAtom)base.target;
	}

	public override void OnInspectorGUI()
	{
		if (atom == null) {
			return;
		}

		Undo.RecordObject(target, null);

		GUI.changed = false;
		{
			atom.acfFile       = EditorGUILayout.TextField("ACF File", atom.acfFile);
			atom.dspBusSetting = EditorGUILayout.TextField("DSP Bus Setting", atom.dspBusSetting);

			for (int i = 0; i < atom.cueSheets.Length; i++) {
				var cueSheet = atom.cueSheets[i];
				EditorGUILayout.BeginVertical("HelpBox");
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Cue Sheet");
				if (GUILayout.Button("Remove")) {
					atom.RemoveCueSheetInternal(cueSheet.name);
					break;
				}
				EditorGUILayout.EndHorizontal();
				EditorGUI.indentLevel++;
				cueSheet.name = EditorGUILayout.TextField("Name", cueSheet.name);
				cueSheet.acbFile = EditorGUILayout.TextField("ACB File", cueSheet.acbFile);
				cueSheet.awbFile = EditorGUILayout.TextField("AWB File", cueSheet.awbFile);
				EditorGUI.indentLevel--;
				EditorGUILayout.EndVertical();
			}
			if (GUILayout.Button("Add CueSheet")) {
				atom.AddCueSheetInternal("", "", "", null);
			}

			atom.dontRemoveExistsCueSheet = EditorGUILayout.Toggle("Dont Remove Exists CueSheet", atom.dontRemoveExistsCueSheet);
			atom.dontDestroyOnLoad        = EditorGUILayout.Toggle("Dont Destroy On Load", atom.dontDestroyOnLoad);
		}
		if (GUI.changed) {
			EditorUtility.SetDirty(atom);
		}
	}
	#endregion

	#region Editor Utilities
	public static void InitializePluginForEditor() {
		if (CriAtomPlugin.IsLibraryInitialized()) {
			return;
		}

		var criInitCom = FindObjectOfType<CriWareInitializer>();
		if (criInitCom != null) {
			if (criInitCom.initializesFileSystem) {
				CriFsConfig config = criInitCom.fileSystemConfig;
				CriFsPlugin.SetConfigParameters(
					config.numberOfLoaders,
					config.numberOfBinders,
					config.numberOfInstallers,
					(config.installBufferSize * 1024),
					config.maxPath,
					config.minimizeFileDescriptorUsage,
					config.enableCrcCheck
					);
			}
			if (criInitCom.initializesAtom) {
				CriAtomConfig config = criInitCom.atomConfig;
				CriAtomPlugin.SetConfigParameters(
					Mathf.Max(config.maxVirtualVoices, CriAtomPlugin.GetRequiredMaxVirtualVoices(config)),
					config.maxVoiceLimitGroups,
					config.maxCategories,
					config.maxSequenceEventsPerFrame,
					config.maxBeatSyncCallbacksPerFrame,
					config.standardVoicePoolConfig.memoryVoices,
					config.standardVoicePoolConfig.streamingVoices,
					config.hcaMxVoicePoolConfig.memoryVoices,
					config.hcaMxVoicePoolConfig.streamingVoices,
					config.outputSamplingRate,
					config.asrOutputChannels,
					config.usesInGamePreview,
					config.serverFrequency,
					config.maxParameterBlocks,
					config.categoriesPerPlayback,
					config.maxBuses,
					config.vrMode);

				CriAtomPlugin.SetConfigAdditionalParameters_PC(
					config.pcBufferingTime
					);

				CriAtomPlugin.SetConfigAdditionalParameters_LINUX(
					config.linuxOutput,
					config.linuxPulseLatencyUsec
					);
			}
		}
		CriAtomPlugin.InitializeLibrary();
	}

	public class PreviewPlayer : CriDisposable {
		public CriAtomExPlayer player { get; private set; }
		private bool finalizeSuppressed = false;

		private bool isPlayerReady = false;

		private void Initialize() {
			CriAtomEditor.InitializePluginForEditor();

			if (CriAtomPlugin.IsLibraryInitialized() == false) {
				return;
			}

			player = new CriAtomExPlayer();
			if (player == null) {
				return;
			}

			player.SetPanType(CriAtomEx.PanType.Pan3d);

			player.UpdateAll();

			isPlayerReady = true;

			CriDisposableObjectManager.Register(this, CriDisposableObjectManager.ModuleType.Atom);

			if (finalizeSuppressed) {
				GC.ReRegisterForFinalize(this);
			}
		}

		public PreviewPlayer() {
			Initialize();
		}

		public override void Dispose() {
			this.dispose();
			GC.SuppressFinalize(this);
			finalizeSuppressed = true;
		}

		private void dispose() {
			CriDisposableObjectManager.Unregister(this);
			if (player != null) {
				player.Dispose();
				player = null;
			}
			this.isPlayerReady = false;
		}

		~PreviewPlayer() {
			this.dispose();
		}

		/* 音声データ設定・再生 */
		public void Play(CriAtomExAcb acb, string cueName) {
			if (isPlayerReady == false) {
				this.Initialize();
			}

			if (acb != null) {
				if (player != null) {
					player.SetCue(acb, cueName);
					player.Start();
				} else {
					Debug.LogWarning("[CRIWARE] Player is not ready. Please try reloading the inspector");
				}
			} else {
				Debug.LogWarning("[CRIWARE] Atom Player for editor: internal error");
			}
		}

		/* 再生停止 */
		public void Stop(bool withoutRelease = false) {
			if (player != null) {
				if (withoutRelease) {
					player.StopWithoutReleaseTime();
				} else {
					player.Stop();
				}
			}
		}

		public void ResetPlayer() {
			player.SetVolume(1f);
			player.SetPitch(0);
			player.Loop(false);
		}
	}
	#endregion
} // end of class


/* end of file */