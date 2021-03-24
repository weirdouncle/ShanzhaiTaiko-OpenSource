/****************************************************************************
 *
 * Copyright (c) 2019 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/
//#define CRI_TIMELINE_ATOM_VERBOSE_DEBUG

#if UNITY_2018_1_OR_NEWER && CRIWARE_TIMELINE_1_OR_NEWER

using UnityEditor;
using UnityEngine;
using UnityEditor.Timeline;
using UnityEngine.Timeline;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace CriTimeline.Atom {

	[CustomPropertyDrawer(typeof(CriAtomBehaviour))]
	public class CriAtomTimelineDrawer : PropertyDrawer {
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			SerializedProperty volumeProp = property.FindPropertyRelative("volume");
			SerializedProperty pitchProp = property.FindPropertyRelative("pitch");
			SerializedProperty aisacProp = property.FindPropertyRelative("AISACValue");

			EditorGUILayout.PropertyField(volumeProp);
			EditorGUILayout.PropertyField(pitchProp);
			EditorGUILayout.PropertyField(aisacProp);
		}
	}

	[CustomEditor(typeof(CriAtomClip)), CanEditMultipleObjects]
	public class CriAtomClipEditor : Editor { }

	[CustomEditor(typeof(CriAtomTrack))]
	public class CriAtomTrackEditor : Editor {
		private CriAtomTrack m_object;

		public void OnEnable() {
			if (target != null) {
				m_object = target as CriAtomTrack;
			}

		}

		public override void OnInspectorGUI() {
			if (m_object == null) {
				return;
			}

			serializedObject.Update();
			EditorGUI.BeginChangeCheck();

			m_object.m_AisacControls
				= EditorGUILayout.TextField("Aisac Controls", m_object.m_AisacControls);
			m_object.m_StopOnWrapping
				= EditorGUILayout.Toggle("Stop On Wrapping", m_object.m_StopOnWrapping);

			DrawLine(Color.black);
			EditorGUILayout.LabelField("Unity Editor");

			m_object.m_IsRenderMono
				= EditorGUILayout.Toggle("Is Render to Mono", m_object.m_IsRenderMono);

			serializedObject.ApplyModifiedProperties();
			if (EditorGUI.EndChangeCheck()) {
				EditorUtility.SetDirty(m_object);
			}
		}

		private static void DrawLine(Color color, int thickness = 2, int padding = 10) {
			Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
			rect.height = thickness;
			rect.y += padding / 2;
			rect.x -= 2;
			rect.width += 6;
			EditorGUI.DrawRect(rect, color);
		}
	}

#if UNITY_2019_3_OR_NEWER

	[CustomTimelineEditor(typeof(CriAtomClip))]
	public class CriAtomClipWaveformEditor : UnityEditor.Timeline.ClipEditor {
		private Dictionary<CriAtomClip, CriAtomClipWaveformPreviewer> atomClipPreviewDirectory
			= new Dictionary<CriAtomClip, CriAtomClipWaveformPreviewer>();

		public override void OnCreate(TimelineClip clip, TrackAsset track, TimelineClip clonedFrom) {
			base.OnCreate(clip, track, clonedFrom);
#if CRI_TIMELINE_ATOM_VERBOSE_DEBUG
			Debug.Log("[CRIWARE][Timeline] OnCreate");
#endif

			atomClipPreviewDirectory.Clear();

			CriAtomClip atomClip = clip.asset as CriAtomClip;
			atomClipPreviewDirectory.Add(atomClip, new CriAtomClipWaveformPreviewer(atomClip));
		}

		public override void OnClipChanged(TimelineClip clip) {
			base.OnClipChanged(clip);
#if CRI_TIMELINE_ATOM_VERBOSE_DEBUG
			Debug.Log("[CRIWARE][Timeline] OnClipChanged");
#endif

			CriAtomClip atomClip = clip.asset as CriAtomClip;
			if (atomClipPreviewDirectory.ContainsKey(atomClip)) {
				return;
			}
			atomClipPreviewDirectory.Add(atomClip, new CriAtomClipWaveformPreviewer(atomClip));
		}

		public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region) {
			base.DrawBackground(clip, region);
#if CRI_TIMELINE_ATOM_VERBOSE_DEBUG
			Debug.Log("[CRIWARE][Timeline] DrawBackground");

			Debug.Log("region.startTime : " + region.startTime.ToString() + "region.endTime : " + region.endTime.ToString());
#endif

			CriAtomClip atomClip = clip.asset as CriAtomClip;
			CriAtomClipWaveformPreviewer atomClipPreviewer;
			if (!atomClipPreviewDirectory.TryGetValue(atomClip, out atomClipPreviewer)) {
#if CRI_TIMELINE_ATOM_VERBOSE_DEBUG
				Debug.LogError("[CRIWARE][Timeline] not contains key : " + clip.displayName);
#endif
				return;
			}
			if (!atomClipPreviewer.HasDecodeTask()) {
				atomClipPreviewDirectory.Remove(atomClip);
				atomClipPreviewDirectory.Add(atomClip, new CriAtomClipWaveformPreviewer(atomClip));
			}

			if (!atomClipPreviewer.HasPreviewData()) {
				/* While decoding or decode error. Skip renderring. */
				return;
			}

			atomClipPreviewer.IsLooping = atomClip.loopWithinClip;
			atomClipPreviewer.IsMuted = atomClip.muted;
			CriAtomTrack atomTrack = clip.parentTrack as CriAtomTrack;
			if (atomTrack.m_IsRenderMono) {
				atomClipPreviewer.ChannelMode = CriAtomClipWaveformPreviewer.RenderChannelMode.Mono;
			} else {
				atomClipPreviewer.ChannelMode = CriAtomClipWaveformPreviewer.RenderChannelMode.All;
			}

			atomClipPreviewer.RenderMaterial(region);
		}
	}

	public class CriAtomClipWaveformPreviewer {
		/* Number of drawing channels */
		public enum RenderChannelMode {
			Mono = 0, /* Mono(Left Channel) */
			All       /* All Channels */
		}

		public bool IsLooping = false;
		public bool IsMuted = true;
		public RenderChannelMode ChannelMode = CriAtomClipWaveformPreviewer.RenderChannelMode.Mono;
		public static string atomClipWaveformRenderShader = "CRIWARE/CriAtomClipWaveformRender";
		public static Color backgroundColor = new Color(0.2509804f, 0.2666667f, 0.2901961f, 0.0f);
		public static Color waveColorForAll = new Color(1.0f, 0.6352941f, 0.0f, 1.0f); /* yellow */
		public static Color waveColorForMono = new Color(0.1294118f, 0.1647059f, 0.254902f, 1.0f); /* blue */

		private struct CriAtomClipWaveformInfo {
			public CriAtomEx.WaveformInfo waveformInfo;
			public double waveDurationSecond;
			public Int16[] lpcmBufferByInterleave;
		}

		private Task decodeTask = null;
		private CriAtomClipWaveformInfo atomClipWaveformInfo;
		private Texture2D simpleTexture = null;
		private readonly int simpleTextureWidth = 4096;
		private Material material = null;

		public CriAtomClipWaveformPreviewer(CriAtomClip criAtomClip) {
			if (!CriAtomTimelinePreviewer.IsInitialized || string.IsNullOrEmpty(criAtomClip.cueSheet)) {
				return;
			}
			var acb = CriAtomTimelinePreviewer.Instance.GetAcb(criAtomClip.cueSheet);
			if (acb == null) {
				Debug.LogError("[CRIWARE][Timeline] faild to load acb object.");
				return;
			}
			this.material = new Material(Shader.Find(atomClipWaveformRenderShader));

			decodeTask = Task.Run(() =>
			{
				this.atomClipWaveformInfo = new CriAtomClipWaveformInfo();
				if (!acb.GetWaveFormInfo(criAtomClip.cueName, out this.atomClipWaveformInfo.waveformInfo)) {
					/* without waveform */
					return;
				}
				this.atomClipWaveformInfo.lpcmBufferByInterleave = new Int16[this.atomClipWaveformInfo.waveformInfo.numSamples * this.atomClipWaveformInfo.waveformInfo.numChannels];
				if (!this.LoadWaveform(acb.nativeHandle, criAtomClip.cueName, ref this.atomClipWaveformInfo.lpcmBufferByInterleave)) {
					this.atomClipWaveformInfo.lpcmBufferByInterleave = null;
					return;
				}
				this.atomClipWaveformInfo.waveDurationSecond = this.atomClipWaveformInfo.waveformInfo.numSamples / (double)this.atomClipWaveformInfo.waveformInfo.samplingRate;
			});
		}

		~CriAtomClipWaveformPreviewer() {
			if (decodeTask != null) {
				if (!decodeTask.IsCompleted) {
					decodeTask.Wait();
				}
				decodeTask.Dispose();
				decodeTask = null;
			}
			if (this.simpleTexture != null) {
				if (Application.isPlaying) {
					UnityEngine.Object.Destroy(this.simpleTexture);
				} else {
					UnityEngine.Object.DestroyImmediate(this.simpleTexture);
				}
				this.simpleTexture = null;
			}
		}

		private void CreateSimpleTexture(in CriAtomClipWaveformInfo info) {
			if (this.simpleTexture != null) {
				return;
			}
			int samplePerPixel = (int)((info.waveformInfo.numSamples) / this.simpleTextureWidth);
			if (samplePerPixel <= 0) {
				samplePerPixel = 1;
			}

			this.simpleTexture = new Texture2D((int)(info.waveformInfo.numSamples / samplePerPixel), info.waveformInfo.numChannels, TextureFormat.RG16, false);
			{
				this.simpleTexture.wrapMode = TextureWrapMode.Clamp;
				this.simpleTexture.filterMode = FilterMode.Bilinear;
			}

			for (int y = 0; y < this.simpleTexture.height; y++) {
				for (int x = 0; x < info.waveformInfo.numSamples; x += samplePerPixel) {
					long range = samplePerPixel;
					if (x + samplePerPixel > info.waveformInfo.numSamples) {
						range = info.waveformInfo.numSamples - x;
					}
					Int16 maxValue, minValue;
					GetMinMaxPcmArray(this.atomClipWaveformInfo.lpcmBufferByInterleave,
						x,
						range,
						y,
						out minValue, out maxValue);
					int pixelX = x / samplePerPixel;
					if (pixelX > this.simpleTexture.width) {
						Debug.Assert(pixelX > this.simpleTexture.width);
						break;
					}

					this.simpleTexture.SetPixel(pixelX, y, new Color(
						maxValue / (float)Int16.MaxValue,
						minValue / (float)Int16.MinValue,
						0));
				}
			}
			this.simpleTexture.Apply();
		}

		public bool HasDecodeTask() {
			return decodeTask == null ? false : true;
		}

		public bool HasPreviewData() {
			if (decodeTask == null || decodeTask.IsFaulted || decodeTask.IsCanceled || !decodeTask.IsCompleted) {
				return false;
			}
			/* decodeTask.IsCompleted == true and buffer == null is decode error. */
			return this.atomClipWaveformInfo.lpcmBufferByInterleave != null;
		}

		private void GetMinMaxPcmArray(Int16[] array, long index, long length, int channel, out Int16 min, out Int16 max) {
			Int16 maxValue = 0;
			Int16 minValue = 0;
			for (long i = 0; i < length; i++) {
				var currentIndex = index * this.atomClipWaveformInfo.waveformInfo.numChannels
					+ i * this.atomClipWaveformInfo.waveformInfo.numChannels + channel;
				var amplitude = array[currentIndex];
				if (maxValue < amplitude) {
					maxValue = amplitude;
				}
				if (minValue > amplitude) {
					minValue = amplitude;
				}
			}
			max = maxValue;
			min = minValue;
		}

		public void RenderMaterial(ClipBackgroundRegion region) {
			CreateSimpleTexture(in this.atomClipWaveformInfo);

			if (region.position.height <= 0 | region.position.width <= 0) {
				/* region.position is invalid. */
				return;
			}
			this.material.SetTexture("_MainTex", this.simpleTexture);
			this.material.SetColor("_BacCol", backgroundColor);
			var materialWaveColor = this.ChannelMode == RenderChannelMode.All ? waveColorForAll : waveColorForMono;
			this.material.SetColor("_ForCol", materialWaveColor);
			int materialWaveChannel = this.ChannelMode == RenderChannelMode.All ? this.atomClipWaveformInfo.waveformInfo.numChannels : 1;
			this.material.SetFloat("_Channel", (float)materialWaveChannel);

			float zoom = (float)((region.endTime - region.startTime) / this.atomClipWaveformInfo.waveDurationSecond);
			this.material.SetFloat("_Scale", zoom);
			this.material.SetFloat("_Offset", (float)(region.startTime / this.atomClipWaveformInfo.waveDurationSecond));

			int materialLoop = IsLooping ? 1 : 0;
			this.material.SetInt("_IsLoop", materialLoop);

			int materialMute = IsMuted ? 1 : 0;
			this.material.SetInt("_IsMute", materialMute);

			int materialForceMono = this.ChannelMode == RenderChannelMode.Mono ? 1 : 0;
			this.material.SetInt("_ForceMono", materialForceMono);

			Graphics.DrawTexture(region.position, this.simpleTexture, this.material);
		}

		private bool LoadWaveform(IntPtr acbHn, string cueName, ref System.Int16[] decodeLpcmBuffer) {
			var gcHandle = GCHandle.Alloc(decodeLpcmBuffer, GCHandleType.Pinned);
			var result = CRIWAREF030BD28(acbHn, cueName, gcHandle.AddrOfPinnedObject(), decodeLpcmBuffer.LongLength);
			gcHandle.Free();
			return result;
		}

		#region DLL Import
#if !CRIWARE_ENABLE_HEADLESS_MODE
		[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
		private static extern bool CRIWAREF030BD28(IntPtr acbHn, string cue_name, IntPtr decode_lpcm_buffer, System.Int64 decodeLpcmBufferLength);
#else
		private static bool CRIWAREF030BD28(IntPtr acbHn, string cue_name, IntPtr decode_lpcm_buffer, System.Int64 decodeLpcmBufferLength) {return false;}
#endif
		#endregion
	}

#endif //UNITY_2019_3_OR_NEWER

}


#endif //UNITY_2018_1_OR_NEWER