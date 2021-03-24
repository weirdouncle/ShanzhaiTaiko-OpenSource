/****************************************************************************
 *
 * Copyright (c) 2014 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;


public sealed class CriWareVersionWindow : EditorWindow
{
	List<TargetInfo> searchresult = new List<TargetInfo>();
	private readonly string[][] pluginBinaryFilenames   = {
		new string[]{"PC",      "PCx64",          "CRIWARE Monitor Unity", "x86_64/cri_ware_unity.dll"},
		new string[]{"PC",      "PCx86",          "CRIWARE Monitor Unity", "x86/cri_ware_unity.dll"},
		new string[]{"MacOSX",  "Mac_X86_64",     "CRIWARE Monitor Unity", "cri_ware_unity.bundle/Contents/MacOS/cri_ware_unity"},
		new string[]{"Android", "Android",        "CRIWARE Monitor Unity", "Android/libs/arm64-v8a/libcri_ware_unity.so"},
		new string[]{"Android", "Android",        "CRIWARE Monitor Unity", "Android/libs/armeabi-v7a/libcri_ware_unity.so"},
		new string[]{"Android", "Android",        "CRIWARE Monitor Unity", "Android/libs/x86/libcri_ware_unity.so"},
		new string[]{"iOS",     "iOS_ARMv7",      "CRIWARE Monitor Unity", "iOS/libcri_ware_unity.a"},
		new string[]{"iOS",     "iOS_ARMv7s",     "CRIWARE Monitor Unity", "iOS/libcri_ware_unity.a"},
		new string[]{"iOS",     "iOS_ARMv8_A64",  "CRIWARE Monitor Unity", "iOS/libcri_ware_unity.a"},
		new string[]{"iOS",     "iOS_SIM_X86",    "CRIWARE Monitor Unity", "iOS/libcri_ware_unity.a"},
		new string[]{"iOS",     "iOS_SIM_X86_64", "CRIWARE Monitor Unity", "iOS/libcri_ware_unity.a"},
		new string[]{"tvOS",    "iOS_ARMv8_A64",  "CRIWARE Monitor Unity", "tvOS/libcri_ware_unity.a"},
		new string[]{"tvOS",    "iOS_SIM_X86_64", "CRIWARE Monitor Unity", "tvOS/libcri_ware_unity.a"},
		new string[]{"PS4",     "PS4",            "CRIWARE Monitor Unity", "PS4/cri_ware_unity.prx"},
		new string[]{"PS5",     "PS5",            "CRIWARE Monitor Unity", "PS5/cri_ware_unity.prx"},
		new string[]{"XboxOne", "XboxOne",        "CRIWARE Monitor Unity", "XboxOne/cri_ware_unity.dll"},
		new string[]{"Scarlett", "Scarlett",      "CRIWARE Monitor Unity", "Scarlett/cri_ware_unity.dll"},
		new string[]{"SWITCH",  "SWITCH",         "CRIWARE Monitor Unity", "NX64/cri_ware_unity.a"},
		new string[]{"UWP",     "WinRT ARM",      "CRIWARE Monitor Unity", "UWP/ARM/cri_ware_unity.dll"},
		new string[]{"UWP",     "WinRT x86",      "CRIWARE Monitor Unity", "UWP/x86/cri_ware_unity.dll"},
		new string[]{"UWP",     "WinRT x64",      "CRIWARE Monitor Unity", "UWP/x64/cri_ware_unity.dll"},
		new string[]{"WebGL",   "Emscripten",     "CRIWARE Unity",         "WebGL/cri_ware_unity.bc"},
        new string[]{"Linux",   "LINUX_X86_64",   "CRIWARE Unity",         "x86_64/libcri_ware_unity.so"},
		new string[]{"Stadia",  "LINUX_X86_64",   "CRIWARE Unity",         "GGP/cri_ware_unity.so"},
	};

	[Serializable]
	private class ModuleInfo
	{
		public string name;
		public string target;
		public string version;
		public string buildDate;
		public string appendix;
	}

	[Serializable]
	private class PluginInfo
	{
		public string           platform;
		public string           target;
		public string           path;
		public ModuleInfo       info;
		public List<ModuleInfo> moduleVersionInfos;
	}

	[SerializeField]
	private List<PluginInfo>    pluginInfos;
	[SerializeField]
	private int                 selectedInfoIndex = 0;
	private string              detailVersionsString  = "";
	private string[]            detailVersionsStrings = {""};
	private Vector2             scrollPosition = new Vector2(0.0f, 0.0f);

	static private Font _detailFont;
	static private GUIStyle _detailStyle;
	static private GUIStyle detailStyle {
		get {
			if (_detailFont == null) {
				string fontname = "";
#if UNITY_EDITOR_WIN
				fontname = "Consolas";
#elif UNITY_EDITOR_OSX
				fontname = "Courier";
#endif
				_detailFont = Font.CreateDynamicFontFromOSFont(fontname, 12);
				_detailStyle = null;
			}
			if (_detailStyle == null) {
				_detailStyle = new GUIStyle(EditorStyles.largeLabel);
				_detailStyle.font = _detailFont;
			}
			return _detailStyle;
		}
	}

	[MenuItem("Window/CRIWARE/Version Information", false, 200)]
	static void OpenWindow()
	{
		EditorWindow.GetWindow<CriWareVersionWindow>(false, "CRI Version");
	}

	private struct TargetInfo
	{
		public TargetInfo(string Path, string[] Info)
		{
			path = Path;
			info = Info;
		}

		public string path;
		public string[] info;
	}

	private string[] GenerateStock(string libName)
	{
		return AssetDatabase.FindAssets(libName);
	}

	private List<TargetInfo> SearchItem(string[] stock, string[][] targetList) {
		List<TargetInfo> result = new List<TargetInfo>();
		foreach (string[] target in targetList) {
			foreach (string path in stock) {
				string strPath = AssetDatabase.GUIDToAssetPath(path);
				if (strPath.Contains(target[3])) {
					if (target[0] == "PC" && strPath.Contains("UWP")) {
							continue;
					}
					result.Add(new TargetInfo(strPath, target));
					break;
				}
			}
		}
		return result;
	}

	private void OnEnable()
	{
		Reload();
	}


	private void OnGUI()
	{
		EditorGUILayout.BeginVertical();
		{
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Script Version", GUILayout.ExpandWidth(true));
				if (GUILayout.Button("Copy to Clipboard", GUILayout.Width(180))) {
					EditorGUIUtility.systemCopyBuffer = PluginVersionsString();
					GUI.FocusControl("");
				}
				if (GUILayout.Button("Reload", GUILayout.Width(80))) {
					Reload(true);
					GUI.FocusControl("");
				}
				EditorGUILayout.EndHorizontal();
			}
			/* スクリプトバージョン表示 */
			{
				EditorGUI.indentLevel++;
				EditorGUILayout.LabelField("Ver." + CriWare.Common.GetScriptVersionString());
				EditorGUI.indentLevel--;
			}
			EditorGUILayout.Space();
			/* バイナリバージョン表示 */
			{
				EditorGUILayout.LabelField("Binary Version");
			}
			/* プラットフォーム別プラグインバイナリバージョン表示 */
			{
				EditorGUILayout.BeginVertical();
				GUILayoutOption platformColumnWidth  = GUILayout.Width(80);
				GUILayoutOption targetColumnWidth    = GUILayout.Width(120);
				GUILayoutOption versionColumnWidth   = GUILayout.Width(140);
				GUILayoutOption buildDateColumnWidth = GUILayout.Width(200);
				GUILayoutOption appendixColumnWidth  = GUILayout.Width(200);
				GUILayoutOption[] pathColumnWidth    = {GUILayout.MinWidth(400), GUILayout.ExpandWidth(true)};
				if (pluginInfos != null) {
					for (int i = 0; i < pluginInfos.Count; ++i) {
						EditorGUILayout.BeginHorizontal();
						{
							EditorGUILayout.LabelField("", GUILayout.Width(15));
							if (GUILayout.Button(pluginInfos[i].platform, EditorStyles.radioButton, platformColumnWidth)) {
								/* 表示の制限のため表示可能な文字数で切り出す */
								detailVersionsString  = ModuleInfosToAlignedString(pluginInfos[i].moduleVersionInfos);
								detailVersionsStrings = SplitTextAreaMaxLength(detailVersionsString);
								selectedInfoIndex = i;
								scrollPosition = new Vector2(0.0f, 0.0f);
								GUI.FocusControl("");
							}

							if (pluginInfos[i].info != null) {
								EditorGUILayout.LabelField((pluginInfos[i].target ?? "--"), targetColumnWidth);
								EditorGUILayout.LabelField((pluginInfos[i].info.version ?? "--"), versionColumnWidth);
								EditorGUILayout.LabelField((pluginInfos[i].info.buildDate ?? "--"), buildDateColumnWidth);
								EditorGUILayout.LabelField((pluginInfos[i].info.appendix ?? "--"), appendixColumnWidth);
							} else {
								EditorGUILayout.LabelField("--", targetColumnWidth);
								EditorGUILayout.LabelField("--", versionColumnWidth);
								EditorGUILayout.LabelField("--", buildDateColumnWidth);
								EditorGUILayout.LabelField("--", appendixColumnWidth);
							}

							EditorGUILayout.LabelField(pluginInfos[i].path, pathColumnWidth);
						}
						EditorGUILayout.EndHorizontal();
					}
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.Space();
			/* 詳細バージョン情報表示 */
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Details [ " + (pluginInfos != null ? pluginInfos[selectedInfoIndex].platform + " / " + pluginInfos[selectedInfoIndex].target : "") + " ]", GUILayout.ExpandWidth(true));
				if (GUILayout.Button("Copy Details to Clipboard", GUILayout.Width(180))) {
					EditorGUIUtility.systemCopyBuffer = detailVersionsString;
					GUI.FocusControl("");
				}
				EditorGUILayout.EndHorizontal();
				scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, EditorStyles.textArea);
				foreach (var item in detailVersionsStrings) {
					EditorGUILayout.TextArea(item, detailStyle);
				}
				EditorGUILayout.EndScrollView();
			}
		}
		EditorGUILayout.EndVertical();
	}


	private void Reload(bool forceRefresh = false)
	{
		string[] searchedResultStock = GenerateStock("t:DefaultAsset cri_ware_unity");
		searchresult = SearchItem(searchedResultStock, pluginBinaryFilenames);

		if (forceRefresh == false && pluginInfos != null) { return; }

		pluginInfos = LoadPluginInfos(searchresult);
		if (pluginInfos.Count > 0) {
			/* 表示の制限のため表示可能な文字数で切り出す */
			detailVersionsString  = ModuleInfosToAlignedString(pluginInfos[0].moduleVersionInfos);
			detailVersionsStrings = SplitTextAreaMaxLength(detailVersionsString);
		} else {
			detailVersionsString  = "";
			detailVersionsStrings = new string[]{""};
		}
		scrollPosition = new Vector2(0.0f, 0.0f);
	}


	private string PluginVersionsString()
	{
		string[] moduleInfoStrings
			= ModuleInfosToAlignedString(
				(from item in pluginInfos select item.info).ToList()
				).Split(new string[]{System.Environment.NewLine}, System.StringSplitOptions.None);
		int    platformLength = pluginInfos.Max(item => (item != null) ? item.platform.Length : 0);
		string platformFormat = string.Format("{{0,-{0}}}  ", platformLength);

		string s = "";
		s +=  "CRIWARE Unity Plugin Script Version" + System.Environment.NewLine
			+ "  Ver." + CriWare.Common.GetScriptVersionString() + System.Environment.NewLine + System.Environment.NewLine
				+ "CRIWARE Unity Plugin Binary Version" + System.Environment.NewLine;
		for (int i = 0; i < pluginInfos.Count; i++) {
			s += "  " + string.Format(platformFormat, pluginInfos[i].platform) + moduleInfoStrings[i] + System.Environment.NewLine;
		}

		return s;
	}


	private static string ModuleInfosToAlignedString(List<ModuleInfo> infos)
	{
		int nameLength      = 0;
		int targetLength    = 0;
		int versionLength   = 0;
		int buildDateLength = 0;
		int appendixLength  = 0;
		foreach (var info in infos) {
			if (info != null) {
				nameLength      = System.Math.Max(nameLength,      ((info.name      != null) ? info.name.Length      : 0));
				targetLength    = System.Math.Max(targetLength  ,  ((info.target    != null) ? info.target.Length    : 0));
				versionLength   = System.Math.Max(versionLength,   ((info.version   != null) ? info.version.Length   : 0));
				buildDateLength = System.Math.Max(buildDateLength, ((info.buildDate != null) ? info.buildDate.Length : 0));
				appendixLength  = System.Math.Max(appendixLength,  ((info.appendix  != null) ? info.appendix.Length  : 0));
			}
		}
		string format = string.Format(
			"{{0,-{0}}}  {{1,-{1}}}  {{2,-{2}}}  {{3,-{3}}}  {{4,-{4}}}" + System.Environment.NewLine,
			nameLength, targetLength, versionLength, buildDateLength, appendixLength
			);
		string s = "";
		foreach (var info in infos) {
			if (info != null) {
				s += string.Format(format, info.name, info.target, info.version, info.buildDate, info.appendix);
			} else {
				s += string.Format(format, "--", "--", "--", "--", "--");
			}
		}
		return s;
	}


	private static string[] SplitTextAreaMaxLength(string s)
	{
		const int textAreaMaxLength = 16000;

		List<string> stringList = new List<string>();
		int currentPos = 0;
		while(currentPos < s.Length) {
			if (currentPos + textAreaMaxLength >= s.Length) {
				string subString = s.Substring(currentPos, s.Length - currentPos);
				stringList.Add(subString);
				break;
			} else {
				string subString = s.Substring(currentPos, textAreaMaxLength);
				int lineEnd = subString.LastIndexOf("\n", StringComparison.Ordinal);
				if (lineEnd >= 0) {
					subString = subString.Substring(0, lineEnd);
					currentPos++; // ignore last line end
				}
				stringList.Add(subString);
				currentPos += subString.Length;
			}
		}

		return stringList.ToArray();
	}


	private static List<PluginInfo> LoadPluginInfos(List<TargetInfo> targetInfos)
	{
		var pluginInfos = new List<PluginInfo>();
		int itemCnt = targetInfos.Count;
		int currentCnt = 0;
		foreach (var item in targetInfos) {
			EditorUtility.DisplayProgressBar("Gathering Information", "Getting library information for " + item.info[1], currentCnt / (float)itemCnt);
			try {
				var path = item.path;
				var moduleVersionInfos = LoadModuleInfos(path, item.info);
				if (moduleVersionInfos != null) {
					var info = new PluginInfo();
					info.moduleVersionInfos = moduleVersionInfos;
					info.info = info.moduleVersionInfos.Find((minfo) => minfo.target.Contains(item.info[1]) && (minfo.name == item.info[2]));
					info.platform = item.info[0];
					info.target = info.info.target;
					info.path = item.path;
					pluginInfos.Add(info);
				}
				currentCnt++;
			} catch {
				Debug.LogError("[CRIWARE] Internal: Exception on getting information from " + item.info[3]);
			}
		}
		EditorUtility.ClearProgressBar();
		return pluginInfos;
	}


	private static List<ModuleInfo> LoadModuleInfos(string path, string[] targetInfo)
	{
		if (!System.IO.File.Exists(path)) {
			return null;
		}

		var bytes = System.IO.File.ReadAllBytes(path);
		if (System.IO.Path.GetExtension(path) == ".bc") {
			return LoadModuleInfosWithBitShift(bytes, targetInfo);
		} else {
			return LoadModuleInfos(bytes, targetInfo);
		}
	}


	private static List<ModuleInfo> LoadModuleInfos(byte[] bytes, string[] targetInfo)
	{
		var data  = System.Text.Encoding.ASCII.GetString(bytes);
		var infos = new List<ModuleInfo>();

		var versionRegex            = new System.Text.RegularExpressions.Regex("^([^/]+)(?:/(.+))? (Ver\\..+) (Build:(?:.*))$");
		var versionAppendixRegex    = new System.Text.RegularExpressions.Regex("^Append: (.*)$");

		/* Get informations only if the target name is exactly the same. For libraries with multiple targets in a single file. */
		bool checkTargetEquality = targetInfo[0].Equals("iOS") || targetInfo[0].Equals("tvOS");

		int pos = 0;
		while (true) {
			pos = data.IndexOf(" Build:", pos, StringComparison.Ordinal);
			if (pos == -1) {
				break;
			}
			++pos;
			{
				int beginPos = FindNonPrintableCharBackward(data, pos);
				int endPos   = FindNonPrintableCharFoward(data, pos);
				if ((data[beginPos] != '\n') || (data[endPos] != '\n')) {
					continue;
				}
				++beginPos;
				var s = data.Substring(beginPos, (endPos - beginPos));
				var match = versionRegex.Match(s);
				if (!match.Success) {
					continue;
				}
				var info = new ModuleInfo();
				info.name       = match.Groups[1].Value;
				info.target     = match.Groups[2].Value;
				info.version    = match.Groups[3].Value;
				info.buildDate  = match.Groups[4].Value;
				if (checkTargetEquality
					&& !info.target.Contains(' ')
					&& !info.target.Equals(targetInfo[1])) {
					continue;
				}
				/* Appendix */
				if ((endPos + 2) < data.Length) {
					int appendisBeginPos = endPos + 1;
					int appendixEndPos   = FindNonPrintableCharFoward(data, appendisBeginPos + 1);
					if ((data[appendisBeginPos] != '\0') || (data[appendixEndPos] != '\n')) {
					} else {
						++appendisBeginPos;
						s = data.Substring(appendisBeginPos, (appendixEndPos - appendisBeginPos));
						match = versionAppendixRegex.Match(s);
						if (match.Success) {
							info.appendix = match.Groups[1].Value;
						}
					}
				}
				infos.Add(info);
			}
		}

		/* モジュール情報をモジュール名順にソート */
		infos.Sort((x, y) => x.name.CompareTo(y.name) );

		return infos;
	}


	private static List<ModuleInfo> LoadModuleInfosWithBitShift(byte[] bytes, string[] targetInfo)
	{
		var infos = LoadModuleInfos(bytes, targetInfo);

		for (int shift = 1; shift <= 7; shift++) {
			int rshift = 8 - shift;
			byte[] shifted = new byte[bytes.Length + 1];
			for (int i = 0; i < bytes.Length; i++) {
				shifted[i] |= (byte)((bytes[i] << shift));
				shifted[i + 1] |= (byte)(bytes[i] >> rshift);
			}
			infos.AddRange(LoadModuleInfos(shifted, targetInfo));
		}

		/* モジュール情報をモジュール名順にソート */
		infos.Sort((x, y) => x.name.CompareTo(y.name) );

		return infos;
	}


	private static int FindNonPrintableCharBackward(string s, int index)
	{
		for (; index >= 0; --index) {
			if ((s[index] < 32) || (s[index] > 126)) {
				break;
			}
		}
		return index;
	}


	private static int FindNonPrintableCharFoward(string s, int index)
	{
		for (; index < s.Length; index++) {
			if ((s[index] < 32) || (s[index] > 126)) {
				break;
			}
		}
		return index;
	}
}

