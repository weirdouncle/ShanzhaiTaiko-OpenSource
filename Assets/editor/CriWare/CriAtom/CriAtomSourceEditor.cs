/****************************************************************************
 *
 * Copyright (c) 2014 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

#define CRI_UNITY_EDITOR_PREVIEW

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;


[CustomEditor(typeof(CriAtomSource))]
public class CriAtomSourceEditor : Editor
{
	#region Variables
	private CriAtomSource source = null;
	private bool showAndroidConfig;
	private GUIStyle style;

#if CRI_UNITY_EDITOR_PREVIEW
	public CriAtom atomComponent;
	private CriAtomEditor.PreviewPlayer previewPlayer;
	private string strPreviewAcb = null;
	private string strPreviewAwb = null;
	private CriAtomExAcb previewAcb = null;
	private string lastCuesheet = "";
#endif
	#endregion

	#region Functions
	private void OnEnable() {
		this.source = (CriAtomSource)base.target;
		this.style = new GUIStyle();

#if CRI_UNITY_EDITOR_PREVIEW
		/* シーンからCriAtomコンポーネントを見つけ出す */
		atomComponent = (CriAtom)FindObjectOfType(typeof(CriAtom));
#endif
		previewPlayer = new CriAtomEditor.PreviewPlayer();
	}

	private void OnDisable() {
		if (previewAcb != null) {
			previewAcb.Dispose();
			previewAcb = null;
		}
		lastCuesheet = "";
		if (previewPlayer != null) {
			previewPlayer.Dispose();
			previewPlayer = null;
		}
	}

#if CRI_UNITY_EDITOR_PREVIEW
	/* プレビュ用：音声データ設定・再生関数 */
	private void StartPreviewPlayer() {
		if (previewPlayer == null) {
			return;
		}

		if (lastCuesheet != this.source.cueSheet) {
			if (previewAcb != null) {
				previewAcb.Dispose();
				previewAcb = null;
			}
			foreach (var cuesheet in atomComponent.cueSheets) {
				if (cuesheet.name == this.source.cueSheet) {
					strPreviewAcb = Path.Combine(CriWare.Common.streamingAssetsPath, cuesheet.acbFile);
					strPreviewAwb = (cuesheet.awbFile == null) ? null : Path.Combine(CriWare.Common.streamingAssetsPath, cuesheet.awbFile);
					previewAcb = CriAtomExAcb.LoadAcbFile(null, strPreviewAcb, strPreviewAwb);
					lastCuesheet = cuesheet.name;
				}
			}
		}
		if (previewAcb != null) {
			previewPlayer.player.SetVolume(this.source.volume);
			previewPlayer.player.SetPitch(this.source.pitch);
			previewPlayer.player.Loop(this.source.loop);
			previewPlayer.Play(previewAcb, this.source.cueName);
		} else {
			Debug.LogWarning("[CRIWARE] Specified cue sheet could not be found");
		}
	}
#endif

	public override void OnInspectorGUI() {
		if (this.source == null) {
			return;
		}

		Undo.RecordObject(target, null);

		GUI.changed = false;
		{
			EditorGUI.indentLevel++;
			this.source.cueSheet = EditorGUILayout.TextField("Cue Sheet", this.source.cueSheet);
			this.source.cueName = EditorGUILayout.TextField("Cue Name", this.source.cueName);
#if CRI_UNITY_EDITOR_PREVIEW
			GUI.enabled = false;
			atomComponent = (CriAtom)EditorGUILayout.ObjectField("CriAtom Object", atomComponent, typeof(CriAtom), true);
			GUI.enabled = (atomComponent != null);
			GUILayout.BeginHorizontal();
			{
				EditorGUILayout.LabelField("Preview", GUILayout.MaxWidth(EditorGUIUtility.labelWidth - 5));
				if (GUILayout.Button("Play", GUILayout.MaxWidth(60))) {
					this.StartPreviewPlayer();
				}
				if (GUILayout.Button("Stop", GUILayout.MaxWidth(60))) {
					this.previewPlayer.Stop();
				}
			}
			GUILayout.EndHorizontal();
			GUI.enabled = true;
#endif
			this.source.playOnStart = EditorGUILayout.Toggle("Play On Start", this.source.playOnStart);
			EditorGUILayout.Space();
			this.source.volume = EditorGUILayout.Slider("Volume", this.source.volume, 0.0f, 1.0f);
			this.source.pitch = EditorGUILayout.Slider("Pitch", this.source.pitch, -1200f, 1200);
			this.source.loop = EditorGUILayout.Toggle("Loop", this.source.loop);
			if (this.source.use3dPositioning = EditorGUILayout.Toggle("3D Positioning", this.source.use3dPositioning)) {
				EditorGUI.indentLevel++;
				this.source.regionOnStart = EditorGUILayout.ObjectField("Region On Start", this.source.regionOnStart, typeof(CriAtomRegion), true) as CriAtomRegion;
				this.source.listenerOnStart = EditorGUILayout.ObjectField("Listener On Start", this.source.listenerOnStart, typeof(CriAtomListener), true) as CriAtomListener;
				EditorGUI.indentLevel--;
			}

			this.showAndroidConfig = EditorGUILayout.Foldout(this.showAndroidConfig, "Android Config");
			if (this.showAndroidConfig) {
				EditorGUI.indentLevel++;
				EditorGUILayout.BeginHorizontal();
				style.stretchWidth = true;
				this.source.androidUseLowLatencyVoicePool = EditorGUILayout.Toggle("Low Latency Playback", this.source.androidUseLowLatencyVoicePool);
				EditorGUILayout.EndHorizontal();
				EditorGUI.indentLevel--;
			}
		}
		if (GUI.changed) {
			EditorUtility.SetDirty(this.source);
		}
	}
	#endregion
} // end of class


/* end of file */
