/****************************************************************************
 *
 * Copyright (c) 2011 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

#if UNITY_2017 && UNITY_EDITOR_WIN
#define OPENFOLDERPANEL_IS_BROKEN
#endif

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


public sealed class CriAtomWindow : EditorWindow
{
	#region Variables
	private int selectedAcfId = 0;
	private int selectedCueInfoIndex = 0;
	private int selectedCueSheetId = 0;
	private bool isCueSheetListInitiated = false;
	private int lastPreviewCueSheetId = -1;
	private Vector2 scrollPosCueSheetList;
	private Vector2 scrollPosCueList;
	private Vector2 scrollPosWindow;
	private bool useCopyAssetsFromCriAtomCraft = false;
	private string[] popupAcfList = null;
	private Color currentGuiColor;

	private GameObject targetObject = null;
	private CriAtomSource targetAtomSource = null;
	private CriAtomEditor.PreviewPlayer previewPlayer = null;
	private CriAtomExAcb previewAcb = null;
	private string lastAcbName = null;

	[SerializeField] private string searchPath = "";
	[SerializeField] private CriAtomWindowInfo projInfo = new CriAtomWindowInfo();
	[SerializeField] private CriAtomWindowPrefs criAtomWindowPrefs = null;
	[SerializeField] private GUIStyle toolBarButtonStyle = null;

	[SerializeField] private bool showPrivateCue = false;
	#endregion

	#region Functions

	[MenuItem("Window/CRIWARE/Atom Browser", false, 100)]
	static void OpenWindow()
	{
		EditorWindow.GetWindow<CriAtomWindow>(false, "Atom Browser");
	}

	private void OnEnable() {
		searchPath = Application.streamingAssetsPath;
		criAtomWindowPrefs = CriAtomWindowPrefs.Load();

		if (Selection.gameObjects.Length > 0) {
			targetObject = Selection.gameObjects[0];
			targetAtomSource = targetObject.GetComponent<CriAtomSource>();
		}

		ReloadAcbInfo();
	}

	private void OnDisable() {
		if (previewAcb != null) {
			previewAcb.Dispose();
			previewAcb = null;
		}
		lastAcbName = "";
		if (previewPlayer != null) {
			previewPlayer.Dispose();
			previewPlayer = null;
		}
	}

	private void OnSelectionChange() {
		if (Selection.gameObjects.Length > 0) {
			targetObject = Selection.gameObjects[0];
			targetAtomSource = targetObject.GetComponent<CriAtomSource>();
		}

		Repaint();
	}

	private void OnLostFocus() {
		StopPreview();
		if (previewAcb != null) {
			previewAcb.Dispose();
			previewAcb = null;
		}
		lastAcbName = "";
	}

	private void OnGUI() {
		if (toolBarButtonStyle == null) {
			toolBarButtonStyle = new GUIStyle(EditorStyles.toolbarButton);
			toolBarButtonStyle.alignment = TextAnchor.MiddleLeft;
		}

		currentGuiColor = GUI.color;
		this.scrollPosWindow = GUILayout.BeginScrollView(this.scrollPosWindow);
		{
			GUISearchAndReload();
			GUIACFSettings();
			GUICueList();
			EditorGUILayout.Space();
			GUIImportAssetsFromAtomCraft();
			EditorGUILayout.Space();
		}
		GUILayout.EndScrollView();
		GUI.color = currentGuiColor;
	}

	private void ReloadAcbInfo() {
		TryInitializePlugin();
		projInfo.GetAcfInfoList(true, searchPath);
		projInfo.GetAcbInfoList(true, searchPath);
	}

	private void PlayPreview(CriAtomExAcb acb, string cuename) {
		if (previewPlayer == null) {
			previewPlayer = new CriAtomEditor.PreviewPlayer();
		}
		previewPlayer.Play(acb, cuename);
	}

	private void StopPreview(bool withoutRelease = true) {
		if (previewPlayer != null) {
			previewPlayer.Stop(withoutRelease);
		}
	}

	private void TryInitializePlugin() {
		if (CriAtomPlugin.IsLibraryInitialized() == false) {
			if (previewAcb != null) {
				previewAcb = null;
				lastAcbName = "";
			}
		}
		CriAtomEditor.InitializePluginForEditor();
	}

	private void GUISearchAndReload()
	{
		EditorGUILayout.BeginHorizontal();
		{
			this.searchPath = EditorGUILayout.TextField("Path to Search", this.searchPath);
#if !OPENFOLDERPANEL_IS_BROKEN
			if (GUILayout.Button("...", EditorStyles.miniButton, GUILayout.Width(50)))
			{
				string pathTemp = EditorUtility.OpenFolderPanel("Select folder to search", this.searchPath, "");
				if (string.IsNullOrEmpty(pathTemp) == false) {
					if (pathTemp.Contains(Application.dataPath)) {
						this.searchPath = pathTemp;
						ReloadAcbInfo();
					} else {
						Debug.LogWarning("[CRIWARE] Selected folder should be inside this project. Search path will not be changed.");
					}
				}
			}
#endif
			if (GUILayout.Button("Reload Info", EditorStyles.miniButton, GUILayout.Width(100))) {
				ReloadAcbInfo();
			}
		}
		EditorGUILayout.EndHorizontal();
	}

	private void GUIACFSettings()
	{
		var acfInfo = projInfo.GetAcfInfoList(false, this.searchPath);
		popupAcfList = new string[acfInfo.Count];
		for (int i = 0; i < acfInfo.Count; i++) {
			popupAcfList[i] = acfInfo[i].filePath;
		}
		selectedAcfId = EditorGUILayout.Popup("ACF Files", selectedAcfId, popupAcfList);
	}

	private void GUICueList()
	{
		const int cCueListItemHeight = 18;
		var acbInfoList = projInfo.GetAcbInfoList(false, searchPath);

		if (isCueSheetListInitiated == true) {
			lastPreviewCueSheetId = this.selectedCueSheetId;
		} else {
			isCueSheetListInitiated = true;
		}

		EditorGUILayout.BeginHorizontal();

		/* cuesheet list */
		using (var cuesheetListScope = new EditorGUILayout.VerticalScope("CN Box", GUILayout.Width(200))) {
			using (var horizontalScope = new EditorGUILayout.HorizontalScope()) {
				if (GUILayout.Button("Cue Sheet", toolBarButtonStyle)) {
					projInfo.SortCueSheet();
					this.selectedCueSheetId = 0;
				}
			}
			using (var cuesheetScrollViewScope = new EditorGUILayout.ScrollViewScope(scrollPosCueSheetList)) {
				scrollPosCueSheetList = cuesheetScrollViewScope.scrollPosition;
				int listLengthInView = (int)position.height / cCueListItemHeight;
				int idFirstItemInView = Mathf.Clamp((int)scrollPosCueSheetList.y / cCueListItemHeight, 0, Mathf.Max(0, acbInfoList.Count - listLengthInView));

				GUILayout.Space(idFirstItemInView * cCueListItemHeight);
				for (int i = idFirstItemInView; i < Mathf.Min(idFirstItemInView + listLengthInView, acbInfoList.Count); i++) {
					GUI.color = (this.lastPreviewCueSheetId == i) ? Color.yellow : currentGuiColor;
					EditorGUILayout.BeginHorizontal();
					if (GUILayout.Button(acbInfoList[i].name, EditorStyles.label)) {
						this.selectedCueSheetId = i;
					}
					EditorGUILayout.EndHorizontal();
				}
				GUILayout.Space(Mathf.Max(0, acbInfoList.Count - idFirstItemInView - listLengthInView) * cCueListItemHeight);
				GUI.color = currentGuiColor;
			}
		}

		if (this.selectedCueSheetId != lastPreviewCueSheetId) {
			this.selectedCueInfoIndex = 0;
		}

		bool isCueSheetAvailable = (acbInfoList.Count > this.selectedCueSheetId);

		using (var cueListAndInfoScope = new EditorGUILayout.VerticalScope("CN Box")) {
			/* list title */
			using (var cueListTitleScope = new EditorGUILayout.HorizontalScope()) {
				if (GUILayout.Button("Cue Name", toolBarButtonStyle)) {
					if (isCueSheetAvailable) {
						acbInfoList[selectedCueSheetId].SortCueInfo(CriAtomWindowInfo.CueSortType.Name);
						this.selectedCueInfoIndex = 0;
					}
				}
				if (GUILayout.Button("Cue ID", toolBarButtonStyle, GUILayout.Width(70))) {
					if (isCueSheetAvailable) {
						acbInfoList[selectedCueSheetId].SortCueInfo(CriAtomWindowInfo.CueSortType.Id);
						this.selectedCueInfoIndex = 0;
					}
				}
			}

			/* cue list */
			bool playButtonPushed = false;
			bool isCueAvailable = false;
			CriAtomWindowInfo.CueInfo selectedCueInfo = null;

			using (var cueListScope = new EditorGUILayout.ScrollViewScope(scrollPosCueList, GUILayout.ExpandHeight(true))) {
				scrollPosCueList = cueListScope.scrollPosition;
				if (isCueSheetAvailable) {
					var acbInfo = acbInfoList[this.selectedCueSheetId];
					List<CriAtomWindowInfo.CueInfo> currentList;
					if (this.showPrivateCue) {
						currentList = acbInfo.cueInfoList;
					} else {
						currentList = acbInfo.publicCueInfoList;
					}

					if (currentList.Count <= 0) {
						EditorGUILayout.HelpBox("Nothing to be shown here.\nTry push the \"Reload Info\" button to refresh the list.", MessageType.Error);
					} else {
						int listLengthInView = (int)position.height / cCueListItemHeight;
						int idFirstCueInView = Mathf.Clamp((int)scrollPosCueList.y / cCueListItemHeight, 0, Mathf.Max(0, currentList.Count - listLengthInView));

						GUILayout.Space(idFirstCueInView * cCueListItemHeight);
						for (int i = idFirstCueInView; i < Mathf.Min(idFirstCueInView + listLengthInView, currentList.Count); ++i) {
							GUI.color = (this.selectedCueInfoIndex == i) ? Color.yellow : currentGuiColor;
							using (var cueItemScope = new EditorGUILayout.HorizontalScope(GUILayout.Height(cCueListItemHeight))) {
								if (GUILayout.Button(EditorGUIUtility.IconContent("Animation.Play"), EditorStyles.miniButton, GUILayout.Width(30))) {
									this.selectedCueInfoIndex = i;
									playButtonPushed = true;
								}
								if (GUILayout.Button(currentList[i].name, EditorStyles.label)) {
									if (selectedCueInfoIndex != i) {
										StopPreview();
									}
									this.selectedCueInfoIndex = i;
								}
								GUILayout.Label(currentList[i].id.ToString(), GUILayout.Width(40));
							}
						}
						GUILayout.Space(Mathf.Max(0, currentList.Count - idFirstCueInView - listLengthInView) * cCueListItemHeight);
						GUI.color = currentGuiColor;
					}

					isCueAvailable = currentList.Count > this.selectedCueInfoIndex;
					if (isCueAvailable) {
						selectedCueInfo = currentList[selectedCueInfoIndex];
					}
				} else {
					EditorGUILayout.HelpBox("No cue sheet is found.\nPlease check the search path. Press \"Reload Info\" button to refresh the list.", MessageType.Info);
				}
			}

			if (playButtonPushed) {
				if (isCueSheetAvailable && isCueAvailable) {
					StopPreview();

					var acbInfo = acbInfoList[selectedCueSheetId];
					TryInitializePlugin();
					if (lastAcbName != acbInfo.name) {
						if (previewAcb != null) {
							previewAcb.Dispose();
						}
						previewAcb = CriAtomExAcb.LoadAcbFile(
							null,
							Path.Combine(CriWare.Common.streamingAssetsPath, acbInfo.acbPath),
							string.IsNullOrEmpty(acbInfo.awbPath) ? null : Path.Combine(CriWare.Common.streamingAssetsPath, acbInfo.awbPath)
						);
						lastAcbName = acbInfo.name;
					}
					if (previewAcb != null) {
						PlayPreview(previewAcb, selectedCueInfo.name);
					}
				}
			}

			EditorGUILayout.BeginHorizontal("AppToolbar");
			{
				EditorGUILayout.LabelField("Cue Information");
				EditorGUILayout.Space();
				{
					var tempToggleVal = GUILayout.Toggle(this.showPrivateCue, "Show Private Cue", EditorStyles.toolbarButton, GUILayout.Width(150));
					if (tempToggleVal != this.showPrivateCue) {
						/* Clear cue selection when change setting */
						this.selectedCueInfoIndex = 0;
					}
					this.showPrivateCue = tempToggleVal;
				}
				if (GUILayout.Button("Stop All", EditorStyles.toolbarButton, GUILayout.Width(100))) {
					StopPreview();
				}
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.LabelField("Cue ID", (isCueSheetAvailable && isCueAvailable) ? selectedCueInfo.id.ToString() : "N/A");
			string cueName = "N/A";
			string userData = "";
			if (isCueSheetAvailable && isCueAvailable) {
				cueName = selectedCueInfo.name;
				userData = selectedCueInfo.comment;
			}
			EditorGUILayout.LabelField("Cue Name", cueName);
			EditorGUILayout.LabelField("User Data", userData, EditorStyles.wordWrappedLabel, GUILayout.Height(28));

			/* edit buttons */
			GUIEdit();
		} /* cueListAndInfoScope */

		EditorGUILayout.EndHorizontal();
	}

	private void GUIEdit()
	{
		EditorGUILayout.BeginHorizontal();
		{
			if (GUILayout.Button("Create GameObject", GUILayout.Height(22))) {
				this.CreateAtomSourceGameObject(true);
			}

			if (this.targetObject == null) {
				GUI.enabled = false;
			}
			if (GUILayout.Button("Add Component", GUILayout.Height(22))) {
				if (targetObject != null) {
					if (targetAtomSource != null) {
						if (EditorUtility.DisplayDialog("Duplicate Component", "CriAtomSource Component already exists. \nDo you really want to add more?", "Yes", "Cancel")) {
							this.CreateAtomSourceGameObject(false);
						}
					} else {
						this.CreateAtomSourceGameObject(false);
					}
				}
			}

			if (this.targetAtomSource == null) {
				GUI.enabled = false;
			}
			if (GUILayout.Button("Update Component", GUILayout.Height(22))) {
				if (targetAtomSource != null) {
					var acbInfoList = projInfo.GetAcbInfoList(false, searchPath);
					if (acbInfoList.Count > this.selectedCueSheetId) {
						var acbInfo = acbInfoList[this.selectedCueSheetId];
						var cueInfo = acbInfo.cueInfoList[this.selectedCueInfoIndex];
						Undo.RegisterCompleteObjectUndo(targetAtomSource, "AtomSource Update");
						targetAtomSource.cueSheet = acbInfo.name;
						targetAtomSource.cueName = cueInfo.name;
						Selection.activeGameObject = targetObject;
					}
				}
			}
			GUI.enabled = true;
		}
		EditorGUILayout.EndHorizontal();
	}

	private void GUIImportAssetsFromAtomCraft()
	{
		if (criAtomWindowPrefs == null) {
			criAtomWindowPrefs = CriAtomWindowPrefs.Load();
		}

		useCopyAssetsFromCriAtomCraft = CriFoldout(useCopyAssetsFromCriAtomCraft, "Import Assets from Atom Craft Project");
		if (useCopyAssetsFromCriAtomCraft) {
			EditorGUI.indentLevel++;

			GUILayout.BeginHorizontal();
			{
				if (criAtomWindowPrefs != null) {
					criAtomWindowPrefs.outputAssetsRoot = EditorGUILayout.TextField("Import From:", criAtomWindowPrefs.outputAssetsRoot);
				}

#if !OPENFOLDERPANEL_IS_BROKEN
				if (GUILayout.Button("...", EditorStyles.miniButton, GUILayout.Width(50))) {
					string tmpPath = "";
					string errorMsg;
					tmpPath = EditorUtility.OpenFolderPanel("Select CRI Atom Craft output Assets Folder", criAtomWindowPrefs.outputAssetsRoot, "");
					if (CheckPathIsAtomCraftAssetRoot(tmpPath, out errorMsg)) {
						criAtomWindowPrefs.outputAssetsRoot = tmpPath;
						criAtomWindowPrefs.Save();
					} else {
						Debug.LogWarning(errorMsg);
					}
				}
#endif
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			{
				GUI.color = Color.yellow;
				EditorGUILayout.PrefixLabel(" ");
				if (GUILayout.Button("Import Assets", GUILayout.Width(160), GUILayout.Height(40))) {
					string errorMsg;
					if (CheckPathIsAtomCraftAssetRoot(criAtomWindowPrefs.outputAssetsRoot, out errorMsg) == false) {
						Debug.LogWarning(errorMsg);
					} else {
						try {
							CopyDirectory(criAtomWindowPrefs.outputAssetsRoot, Application.dataPath);
							Debug.Log("[CRIWARE] Assets successfully imported from " + criAtomWindowPrefs.outputAssetsRoot);
						} catch (Exception ex) {
							Debug.LogError(ex.Message);
							Debug.LogError("[CRIWARE] Failed to import assets from " + criAtomWindowPrefs.outputAssetsRoot);
						}
						AssetDatabase.Refresh();

						ReloadAcbInfo();
					}
				}
				GUI.color = Color.white;
				EditorGUILayout.HelpBox("Copy \"Assets\" folder (created in CRI Atom Craft) to the \"StreamingAssets\" folder.\nThe folder can be created by building with the \"With Unity Assets\" flag in CRI Atom Craft.", MessageType.Info);
			}
			GUILayout.EndHorizontal();

			EditorGUI.indentLevel--;
		}
	}

	private bool CheckPathIsAtomCraftAssetRoot(string outString, out string errorMsg)
	{
		errorMsg = "";
		try {
			if (string.IsNullOrEmpty(outString) == false && Path.GetFileName(outString) == "Assets") {
				if (Directory.Exists(outString) == false) {
					errorMsg = "[CRIWARE] Path to the assets does not exist: " + outString;
					return false;
				}
				foreach (string dir in Directory.GetDirectories(outString)) {
					if (Path.GetFileName(dir) == "Editor") {
						errorMsg = "[CRIWARE] Please choose the Assets folder in your <color=yellow>Atom Craft project</color> instead of the one in your Unity project. Import path will not be changed.";
						return false;
					}
				}
				return true;
			} else {
				errorMsg = "[CRIWARE] Unmatched folder name. Please set the right import path (\"Assets\" folder).";
				return false;
			}
		} catch (Exception ex) {
			errorMsg = "[CRIWARE] I/O Error: " + ex.Message;
			return false;
		}
	}

	private static void CopyDirectory(string sourceDirName, string destDirName)
	{
		try {
			if (Directory.Exists(destDirName) == false) {
				Directory.CreateDirectory(destDirName);
				File.SetAttributes(destDirName, File.GetAttributes(sourceDirName));
			}

			foreach (var file in Directory.GetFiles(sourceDirName)) {
				string ext = Path.GetExtension(file.Replace("\\", "/"));
				if (ext == ".acf" || ext == ".acb" || ext == ".awb") {
					string targetPath = Path.Combine(destDirName, Path.GetFileName(file));
					Debug.Log(String.Format("[CRIWARE] Copying \"{0}\" to \"{1}\"", file, targetPath));
					File.Copy(file, targetPath, true);
				}
			}

			foreach (var dir in Directory.GetDirectories(sourceDirName)) {
				/* sub directories */
				CopyDirectory(dir, Path.Combine(destDirName, Path.GetFileName(dir)));
			}
		} catch {
			throw;
		}
	}

	private void CreateAtomSourceGameObject(bool createGameObjectFlag)
	{
		if (Selection.gameObjects.Length == 0) {
			createGameObjectFlag = true;
		}
		var acbInfoList = projInfo.GetAcbInfoList(false, searchPath);
		if (acbInfoList.Count > this.selectedCueSheetId) {
			GameObject go = null;
			if (createGameObjectFlag) {
				go = new GameObject(acbInfoList[this.selectedCueSheetId].cueInfoList[this.selectedCueInfoIndex].name + "(CriAtomSource)");
				if (Selection.gameObjects.Length > 0) {
					go.transform.parent = Selection.gameObjects[0].transform;
				}
				Undo.RegisterCreatedObjectUndo(go, "Create AtomSource GameObject");
			} else {
				go = Selection.gameObjects[0];
			}
			var acbInfo = acbInfoList[this.selectedCueSheetId];
			CriAtom atom = GameObject.FindObjectOfType(typeof(CriAtom)) as CriAtom;
			if (atom == null) {
				atom = CriWare.Common.managerObject.AddComponent<CriAtom>();
				var acfList = projInfo.GetAcfInfoList(false, searchPath);
				if (acfList.Count > selectedAcfId) {
					atom.acfFile = acfList[selectedAcfId].filePath;
				}
			}
			CriAtomCueSheet cueSheet = atom.GetCueSheetInternal(acbInfo.name);
			if (cueSheet == null) {
				cueSheet = atom.AddCueSheetInternal(null, acbInfo.acbPath, acbInfo.awbPath, null);
			}
			CriAtomSource newCriAtomSource = go.AddComponent<CriAtomSource>();
			if (createGameObjectFlag == false) {
				Undo.RegisterCreatedObjectUndo(newCriAtomSource, "Add AtomSource Component");
			}
			newCriAtomSource.cueSheet = cueSheet.name;
			newCriAtomSource.cueName = acbInfo.cueInfoList[this.selectedCueInfoIndex].name;
			Selection.activeObject = go;
		}
	}

	private bool CriFoldout(bool foldout, string content) {
#if UNITY_5_5_OR_NEWER
		return EditorGUILayout.Foldout(foldout, content, true);
#else
        return EditorGUILayout.Foldout(foldout, content);
#endif
	}
#endregion
}

/* end of file */
