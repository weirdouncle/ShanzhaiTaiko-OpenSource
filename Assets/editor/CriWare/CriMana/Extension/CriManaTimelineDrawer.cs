/****************************************************************************
 *
 * Copyright (c) 2019 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

#if UNITY_2018_1_OR_NEWER && CRIWARE_TIMELINE_1_OR_NEWER

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;


namespace CriTimeline.Mana
{
	[CustomEditor(typeof(CriManaClip))]
	public class CriManaTimelineComponentEditor : Editor
	{
		private SerializedObject m_object;
		private SerializedProperty m_filePath;
		private SerializedProperty m_loopWithinClip;
		private SerializedProperty m_movieFrameRate;
		private SerializedProperty m_clipDuration;
		private SerializedProperty m_movieData;
		private SerializedProperty m_useOnMemoryPlayback;

		public void OnEnable()
		{
			if (target != null) {
				m_object = new SerializedObject(target);
				m_filePath = m_object.FindProperty("m_moviePath");
				m_loopWithinClip = m_object.FindProperty("m_loopWithinClip");
				m_movieFrameRate = m_object.FindProperty("m_movieFrameRate");
				m_clipDuration = m_object.FindProperty("m_clipDuration");
				m_movieData = m_object.FindProperty("m_movieData");
				m_useOnMemoryPlayback = m_object.FindProperty("m_useOnMemoryPlayback");
			}
		}

		public override void OnInspectorGUI()
		{
			if (m_object == null ||
				m_filePath == null ||
				m_loopWithinClip == null) {
				return;
			}

			m_object.Update();
			EditorGUILayout.PropertyField(m_useOnMemoryPlayback);
			EditorGUILayout.PropertyField(m_movieData);
			if (m_useOnMemoryPlayback.boolValue) {
				GUI.enabled = false;
			}

			EditorGUILayout.PropertyField(m_filePath);

			Event evt = Event.current;
			Rect moviePathFieldRect = GUILayoutUtility.GetLastRect();
			int id = GUIUtility.GetControlID(FocusType.Passive);
			switch (evt.type) {
				case EventType.DragUpdated:
				case EventType.DragPerform:
					if (!moviePathFieldRect.Contains(evt.mousePosition)) {
						break;
					}
					DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
					DragAndDrop.activeControlID = id;
					if (evt.type == EventType.DragPerform) {
						DragAndDrop.AcceptDrag();
						foreach (var path in DragAndDrop.paths) {
							if (System.IO.Path.GetExtension(path).Equals(".usm")) {
								string[] splitPath = Regex.Split(path, "Assets/StreamingAssets/");
								if(splitPath.Length < 2) {
									Debug.LogWarning("[Warning] Not in StreamingAssets Folder [" + System.IO.Path.GetFileName(path) + "].");
								} else {
									m_filePath.stringValue = splitPath[1];
								}

							} else {
								Debug.LogWarning("[Warning] Not usm file [" + System.IO.Path.GetFileName(path) + "].");
							}
						}
						DragAndDrop.activeControlID = 0;
					}
					Event.current.Use();
					break;
			}

			EditorGUI.indentLevel++;
			GUI.enabled = false;
			EditorGUILayout.PropertyField(m_movieFrameRate);
			EditorGUILayout.PropertyField(m_clipDuration);
			GUI.enabled = true;
			EditorGUI.indentLevel--;
			EditorGUILayout.PropertyField(m_loopWithinClip);

			m_object.ApplyModifiedProperties();
		}
	}
}


#endif