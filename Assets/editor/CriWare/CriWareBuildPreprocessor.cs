/****************************************************************************
 *
 * Copyright (c) 2017 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

#if UNITY_EDITOR && UNITY_5_6_OR_NEWER

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;


public class CriWareBuildPreprocessor : ScriptableObject,
#if UNITY_2018_1_OR_NEWER
	IPreprocessBuildWithReport
#else
	IPreprocessBuild
#endif
{
	private static string prefsFilePath;
	public bool muteOtherAudio  = true;

	public int callbackOrder { get { return 0; } }

	[MenuItem("GameObject/CRIWARE/Create CriWareBuildPreprocessorPrefs.asset")]
	public static void Create()
	{
		CriWareBuildPreprocessor instance = LoadExistingAsset();
		if (instance) {
			Debug.LogError("[CRIWARE] Preferences file of CriWareBuildPreprocessor already exists.");
			Selection.activeObject = instance;
			return;
		}

		var scobj = ScriptableObject.CreateInstance<CriWareBuildPreprocessor>();
		if (scobj == null) {
			Debug.Log("[CRIWARE] Failed to create CriWareBuildPreprocessor");
			return;
		}

		var script = MonoScript.FromScriptableObject(scobj);
		prefsFilePath = Path.Combine(Directory.GetParent(AssetDatabase.GetAssetPath(script)).ToString(), "CriWareBuildPreprocessorPrefs.asset");

		Directory.CreateDirectory(Path.GetDirectoryName(prefsFilePath));
		AssetDatabase.CreateAsset(scobj, prefsFilePath);

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		Debug.Log("[CRIWARE] Created the preferences file of CriWareBuildPreprocessor. (" + prefsFilePath + ")");

		Selection.activeObject = scobj;
	}

	private static CriWareBuildPreprocessor LoadExistingAsset()
	{
		string preProcessorPath = "";
		string[] searchResult = AssetDatabase.FindAssets("t:CriWareBuildPreprocessor");
		if (searchResult.Length == 0) {
			return null;
		}
		preProcessorPath = AssetDatabase.GUIDToAssetPath(searchResult[0]);
		var instance = (CriWareBuildPreprocessor)AssetDatabase.LoadAssetAtPath(preProcessorPath, typeof(CriWareBuildPreprocessor));
		return instance;
	}

#if UNITY_2018_1_OR_NEWER
	public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
	{
		OnPreprocessBuild(report.summary.platform, report.summary.outputPath);
	}
#endif

	public void OnPreprocessBuild(BuildTarget build_target, string path)
	{
		CriWareBuildPreprocessor instance = LoadExistingAsset();
		if (instance == null) {
			instance = ScriptableObject.CreateInstance<CriWareBuildPreprocessor>();
			Debug.Log(
				"[CRIWARE] Run CriWareBuildPreprocessor.OnPreprocessBuild with default preferences.\n"
				+ "If you want to change the preferences, please create the preferences file by 'GameObject/CRIWARE/Create CriWareBuildPreprocessorPrefs.asset' menu."
			);
		} else {
			Debug.Log(
				"[CRIWARE] Run CriWareBuildPreprocessor.OnPreprocessBuild with default preferences.\n"
				+ "If you want to change the preferences, please edit the preferences file (" + prefsFilePath + ")"
			);
		}

		if (instance.muteOtherAudio == true) {
			instance.ModifyAudioManager(build_target, path);
		}
	}

	private void ModifyAudioManager(BuildTarget build_target, string path)
	{
		string audioManagerPath="ProjectSettings/AudioManager.asset";
		SerializedObject audioManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath(audioManagerPath)[0]);
		SerializedProperty propertyDisableUnityAudio = audioManager.FindProperty("m_DisableAudio");

		if(build_target == BuildTarget.Android)
		{
			if(PlayerSettings.muteOtherAudioSources == true)
			{
				propertyDisableUnityAudio.boolValue = false;
			}
		} else {
			propertyDisableUnityAudio.boolValue = true;
		}

		audioManager.ApplyModifiedProperties();
	}
}



#endif

/* end of file */