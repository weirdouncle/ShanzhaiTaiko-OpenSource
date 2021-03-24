/****************************************************************************
 *
 * Copyright (c) 2011 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;


[Serializable]
public class CriAtomWindowPrefs : ScriptableObject
{
	public string outputAssetsRoot = String.Empty;

	static string FindInstance() {
		var guids = AssetDatabase.FindAssets("t:" + typeof(CriAtomWindowPrefs).Name);
		if (guids.Length <= 0)
			return null;
		if (guids.Length > 1) {
			Debug.LogWarning("[CRIWARE] There are multiple preferences file of CriAtomWindow.");
		}
		return guids[0];
	}

	public void Save ()
	{
		if(string.IsNullOrEmpty(FindInstance())) {
			var script = MonoScript.FromScriptableObject(this);
			var prefsFilePath = Path.Combine(Directory.GetParent(AssetDatabase.GetAssetPath(script)).ToString(), "CriWareBuildPostprocessorPrefs.asset");
			AssetDatabase.CreateAsset(this, prefsFilePath);
		} else {
			EditorUtility.SetDirty(this);
		}
		AssetDatabase.SaveAssets ();
		AssetDatabase.Refresh ();
	}

	public static CriAtomWindowPrefs Load ()
	{
		CriAtomWindowPrefs preference;
		var guid = FindInstance();
		if (string.IsNullOrEmpty(guid)) {
			preference = CreateInstance<CriAtomWindowPrefs>();
			preference.Save();
		} else {
			var path = AssetDatabase.GUIDToAssetPath(guid);
			preference = AssetDatabase.LoadAssetAtPath<CriAtomWindowPrefs>(path);
		}

		return preference;
	}
}

[Serializable]
public class CriAtomWindowInfo {
	#region Serializable Classes
	[Serializable]
	public class InfoBase {
		public string name = "";
		public int id = 0;
		public string comment = "";
	}

	[Serializable]
	public class AcfInfo : InfoBase {
		public string filePath = "";

		public AcfInfo(string name, int id, string comment, string filePath) {
			this.name = name;
			this.id = id;
			this.comment = comment;

			this.filePath = filePath;
		}
	}

	[Serializable]
	public class AcbInfo : InfoBase {
		public string acbPath = "";
		public string awbPath = "";
		public List<CueInfo> cueInfoList;
		public List<CueInfo> publicCueInfoList;

		private bool sortOrder = true; /* true = incremental */

		public AcbInfo(string name, int id, string comment, string acbPath, string awbPath) {
			this.name = name;
			this.id = id;
			this.comment = comment;

			this.acbPath = acbPath;
			this.awbPath = awbPath;
			this.cueInfoList = new List<CueInfo>();
			this.publicCueInfoList = new List<CueInfo>();
		}

		public void SortCueInfo(CueSortType type) {
			if (cueInfoList.Count <= 0) {
				return;
			}

			switch (type) {
				case CueSortType.Id:
					this.cueInfoList.Sort((CueInfo x, CueInfo y) => {
						return sortOrder ? (x.id - y.id) : (y.id - x.id);
					});
					this.publicCueInfoList.Sort((CueInfo x, CueInfo y) => {
						return sortOrder ? (x.id - y.id) : (y.id - x.id);
					});
					break;
				case CueSortType.Name:
					this.cueInfoList.Sort((CueInfo x, CueInfo y) => {
						return sortOrder ? string.Compare(x.name, y.name) : string.Compare(y.name, x.name);
					});
					this.publicCueInfoList.Sort((CueInfo x, CueInfo y) => {
						return sortOrder ? string.Compare(x.name, y.name) : string.Compare(y.name, x.name);
					});
					break;
				default:
					break;
			}

			sortOrder = !sortOrder;
		}
	}

	public void SortCueSheet() {
		switch (cuesheetSortType) {
			case CueSheetSortType.NameInc:
				this.acbInfoList.Sort((AcbInfo x, AcbInfo y) => (
					string.Compare(x.name, y.name)
				));
				cuesheetSortType = CueSheetSortType.NameDec;
				break;
			case CueSheetSortType.NameDec:
				this.acbInfoList.Sort((AcbInfo x, AcbInfo y) => (
					string.Compare(y.name, x.name)
				));
				cuesheetSortType = CueSheetSortType.Id;
				break;
			case CueSheetSortType.Id:
				this.acbInfoList.Sort((AcbInfo x, AcbInfo y) => (
					x.id - y.id
				));
				cuesheetSortType = CueSheetSortType.NameInc;
				break;
			default:
				break;
		}
	}

	private enum CueSheetSortType {
		NameInc,
		NameDec,
		Id
	}
	private CueSheetSortType cuesheetSortType = CueSheetSortType.NameInc;

	public enum CueSortType {
		Id,
		Name
	}

	[Serializable]
	public class CueInfo : InfoBase {
		public bool isPublic;
		public CueInfo(string name, int id, string comment, bool isPublic) {
			this.name = name;
			this.id = id;
			this.comment = comment;
			this.isPublic = isPublic;
		}
	} /* end of class */
	#endregion

	#region Fields
	[SerializeField] private List<AcfInfo> acfInfoList = new List<AcfInfo>();
	[SerializeField] private List<AcbInfo> acbInfoList = new List<AcbInfo>();
	[SerializeField] private bool gotAcfList = false;
	[SerializeField] private bool gotAcbList = false;
	#endregion


	public List<AcbInfo> GetAcbInfoList(bool forceReload, string searchPath) {
		if (acbInfoList == null) {
			acbInfoList = new List<AcbInfo>();
		}
		if (gotAcbList == false || forceReload) {
			if (CriAtomPlugin.IsLibraryInitialized()) {
				GetAcbInfoListCore(searchPath);
			}
			gotAcbList = true;
		}
		return acbInfoList;
	}

	private void GetAcbInfoListCore(string searchPath) {
		acbInfoList.Clear();
		string[] files = null;
		try {
			files = Directory.GetFiles(searchPath, "*.acb", SearchOption.AllDirectories);
		} catch (Exception ex) {
			if (ex is ArgumentException || ex is ArgumentNullException) {
				Debug.LogWarning("[CRIWARE] Insufficient search path. Please check the path for file searching.");
			} else if (ex is DirectoryNotFoundException) {
				Debug.LogWarning("[CRIWARE] Search path not found: " + searchPath);
			} else {
				Debug.LogError("[CRIWARE] Error getting ACB files. Message: " + ex.Message);
			}
		}
		if (files == null) {
			return;
		}

		int acbIndex = 0;
		foreach (string file in files) {
			AcbInfo acbInfo = new AcbInfo(
				Path.GetFileNameWithoutExtension(file),
				acbIndex++,
				"",
				TryGetRelFilePath(file),
				TryGetAwbFile(file));
			/* 指定したACBファイル名(キューシート名)を指定してキュー情報を取得 */
			CriAtomExAcb acb = CriAtomExAcb.LoadAcbFile(null, file.Replace("\\", "/"), "");
			if (acb != null) {
				/* キュー名リストの作成 */
				CriAtomEx.CueInfo[] cueInfoList = acb.GetCueInfoList();
				foreach (CriAtomEx.CueInfo cueInfo in cueInfoList) {
					bool found = false;
					foreach (var key in acbInfo.cueInfoList) {
						if (key.id == cueInfo.id) {
							found = true;
							break;
						}
					}
					if (found == false) {
						var newCueInfo = new CueInfo(cueInfo.name, cueInfo.id, cueInfo.userData, Convert.ToBoolean(cueInfo.headerVisibility));
						acbInfo.cueInfoList.Add(newCueInfo);
						if (newCueInfo.isPublic) {
							acbInfo.publicCueInfoList.Add(newCueInfo);
						}
					} else {
						/* inGame時のサブシーケンスの場合あり */
						Debug.Log("[CRIWARE] Duplicate cue ID " + cueInfo.id.ToString() + " in cue sheet " + acbInfo.name + ". Last cue name:" + cueInfo.name);
					}
				}
				acb.Dispose();
			} else {
				Debug.Log("[CRIWARE] Failed to load ACB file: " + file);
			}
			acbInfoList.Add(acbInfo);
		}
	}

	public List<AcfInfo> GetAcfInfoList(bool forceReload, string searchPath) {
		if (acfInfoList == null) {
			acfInfoList = new List<AcfInfo>();
		}
		if (gotAcfList == false || forceReload) {
			acfInfoList.Clear();
			string[] files = null;
			try {
				files = System.IO.Directory.GetFiles(searchPath, "*.acf", System.IO.SearchOption.AllDirectories);
			} catch (Exception ex) {
				if (ex is ArgumentException || ex is ArgumentNullException) {
					Debug.LogWarning("[CRIWARE] Insufficient search path. Please check the path for file searching.");
				} else if (ex is DirectoryNotFoundException) {
					Debug.LogWarning("[CRIWARE] Search path not found: " + searchPath);
				} else {
					Debug.LogError("[CRIWARE] Error getting ACF files. Message: " + ex.Message);
				}
			}
			if (files != null) {
				int index = 0;
				foreach (string file in files) {
					var acfInfo = new AcfInfo(
						System.IO.Path.GetFileNameWithoutExtension(file),
						index++,
						"",
						TryGetRelFilePath(file));
					acfInfoList.Add(acfInfo);
				}
			}
			gotAcfList = true;
		}
		return acfInfoList;
	}

	public void ResetInfo() {
		this.acfInfoList.Clear();
		this.acbInfoList.Clear();
		this.gotAcfList = false;
		this.gotAcbList = false;
	}

	private string TryGetRelFilePath(string path) {
		Uri streamingAssetsUri = new Uri(CriWare.Common.streamingAssetsPath);
		Uri fullPathUri = new Uri(path);
		if (fullPathUri.ToString().Contains(streamingAssetsUri.ToString())) {
			string[] splitter = { streamingAssetsUri.ToString() + "/" };
			var pathChunks = fullPathUri.ToString().Split(splitter, StringSplitOptions.RemoveEmptyEntries);
			return pathChunks[pathChunks.Length - 1];
		} else {
			return path;
		}
	}

	private string TryGetAwbFile(string acbPath) {
		string dir = Path.GetDirectoryName(acbPath);
		string filenameNoExt = Path.GetFileNameWithoutExtension(acbPath);
		string presumedAwbPath = Path.Combine(dir, filenameNoExt + ".awb");
		if (File.Exists(presumedAwbPath)) {
			return TryGetRelFilePath(presumedAwbPath);
		} else {
			return "";
		}
	}
}

