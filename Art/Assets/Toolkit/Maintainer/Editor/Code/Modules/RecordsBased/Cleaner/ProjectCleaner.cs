#define UNITY_5_3_PLUS
#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
#undef UNITY_5_3_PLUS
#endif

#if UNITY_5_3_PLUS
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

#if UNITY_EDITOR

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CodeStage.Maintainer.Settings;
using CodeStage.Maintainer.Tools;
using CodeStage.Maintainer.UI;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CodeStage.Maintainer.Cleaner
{
	/// <summary>
	/// Allows to find and clean garbage in your Unity project. See readme for details.
	/// </summary>
	public class ProjectCleaner
	{
		internal const string MODULE_NAME = "Project Cleaner";
		internal const string DATA_LOSS_WARNING = "Make sure you've made a backup of your project before proceeding.\nAuthor is not responsible for any data loss due to use of the " + MODULE_NAME + "!";

		private const string PROGRESS_CAPTION = MODULE_NAME + ": phase {0} of {1}, item {2} of {3}";

		private static int phasesCount;
		private static int currentPhase;

		private static int folderIndex;
		private static int foldersCount;

		private static int itemsToClean;

		private static string searchStartScene;

		/// <summary>
		/// Starts garbage search and generates report.
		/// </summary>
		/// <returns>Project Cleaner report, similar to the exported report from the %Maintainer window.</returns>
		/// %Maintainer window is not shown.
		/// <br/>Useful when you wish to integrate %Maintainer in your build pipeline.
		public static string SearchAndReport()
		{
			CleanerRecord[] foundGarbage = StartSearch(false);

			// ReSharper disable once CoVariantArrayConversion
			return ReportsBuilder.GenerateReport(MODULE_NAME, foundGarbage);
		}

		/// <summary>
		/// Starts garbage search, cleans what was found with optional confirmation and 
		/// generates report to let you know what were cleaned up.
		/// </summary>
		/// <param name="showConfirmation">Enables or disables confirmation dialog about cleaning up found stuff.</param>
		/// <returns>Project Cleaner report about removed items.</returns>
		/// %Maintainer window is not shown.
		/// <br/>Useful when you wish to integrate %Maintainer in your build pipeline.
		public static string SearchAndCleanAndReport(bool showConfirmation = true)
		{
			CleanerRecord[] foundGarbage = StartSearch(false);
			CleanerRecord[] cleanedGarbage = StartClean(foundGarbage, false, showConfirmation);

			// ReSharper disable once CoVariantArrayConversion
			return ReportsBuilder.GenerateReport(MODULE_NAME, cleanedGarbage, "Following items were cleaned up:");
		}

		/// <summary>
		/// Starts garbage search with current settings.
		/// </summary>
		/// <param name="showResults">Shows results in %Maintainer window if true.</param>
		/// <returns>Array of CleanerRecords in case you wish to manually iterate over them and make custom report.</returns>
		public static CleanerRecord[] StartSearch(bool showResults)
		{
			List<CleanerRecord> results = new List<CleanerRecord>();

			phasesCount = 0;
			currentPhase = 0;

			if (MaintainerSettings.Cleaner.findEmptyFolders) phasesCount++;
			if (MaintainerSettings.Cleaner.findEmptyScenes)
			{
				if (!CSSceneTools.SaveCurrentSceneIfUserWantsTo())
				{
					Debug.Log(Maintainer.LOG_PREFIX + "Search canceled by user!");
					return null;
				}
				phasesCount++;
				searchStartScene = CSSceneTools.GetCurrentScenePath(true);
			}

			Stopwatch sw = Stopwatch.StartNew();

			bool searchCanceled = false;

			if (MaintainerSettings.Cleaner.findEmptyFolders)
			{
				searchCanceled = !ScanFolders(results);
			}

			if (MaintainerSettings.Cleaner.findEmptyScenes)
			{
				searchCanceled = !ScanSceneFiles(results);
			}

			sw.Stop();

			// opening scene where we started scan
			if (MaintainerSettings.Cleaner.findEmptyScenes)
			{
				if (string.IsNullOrEmpty(searchStartScene))
				{
					CSSceneTools.NewScene();
				}
				else if (CSSceneTools.GetCurrentScenePath() != searchStartScene)
				{
					CSSceneTools.OpenScene(searchStartScene);
				}
			}
			EditorUtility.ClearProgressBar();

			if (!searchCanceled)
			{
				Debug.Log(Maintainer.LOG_PREFIX + MODULE_NAME + " results: " + results.Count +
						" items found in " + sw.Elapsed.TotalSeconds.ToString("0.000") +
						" seconds.");
			}
			else 
			{
				Debug.Log(Maintainer.LOG_PREFIX + "Search canceled by user!");
			}

			SearchResultsStorage.CleanerSearchResults = results.ToArray();
			if (showResults) MaintainerWindow.ShowCleaner();

			return results.ToArray();
		}

		/// <summary>
		/// Starts clean of the garbage found with StartSearch() method.
		/// </summary>
		/// <param name="recordsToClean">Pass records you wish to clean here or leave null to let it load last search results.</param>
		/// <param name="showResults">Shows results in the %Maintainer window if true.</param>
		/// <param name="showConfirmation">Shows confirmation dialog before performing cleanup if true.</param>
		/// <returns>Array of CleanRecords which were cleaned up.</returns>
		public static CleanerRecord[] StartClean(CleanerRecord[] recordsToClean = null, bool showResults = true, bool showConfirmation = true)
		{
			CleanerRecord[] records = recordsToClean;
			if (records == null)
			{
				records = SearchResultsStorage.CleanerSearchResults;
			}
			
			if (records.Length == 0)
			{
				return null;
			}

			itemsToClean = 0;

			foreach (CleanerRecord record in records)
			{
				if (record.selected) itemsToClean++;
			}

			if (itemsToClean == 0)
			{
				EditorUtility.DisplayDialog(MODULE_NAME, "Please select items to clean up!", "Ok");
				return null;
			}

			if (!showConfirmation || EditorUtility.DisplayDialog("Confirmation", "Do you really wish to delete " + itemsToClean + " items?\n" + DATA_LOSS_WARNING, "Go for it!", "Cancel"))
			{
				Stopwatch sw = Stopwatch.StartNew();

				bool cleanCanceled = CleanRecords(records);

				List<CleanerRecord> cleanedRecords = new List<CleanerRecord>(records.Length);
				List<CleanerRecord> notCleanedRecords = new List<CleanerRecord>(records.Length);

				foreach (CleanerRecord record in records)
				{
					if (record.cleaned)
					{
						cleanedRecords.Add(record);
					}
					else
					{
						notCleanedRecords.Add(record);
					}
				}

				records = notCleanedRecords.ToArray();

				sw.Stop();

				EditorUtility.ClearProgressBar();

				if (!cleanCanceled)
				{
					Debug.Log(Maintainer.LOG_PREFIX + MODULE_NAME + " results: " + itemsToClean +
						" items cleaned in " + sw.Elapsed.TotalSeconds.ToString("0.000") +
						" seconds.");
				}
				else
				{
					Debug.Log(Maintainer.LOG_PREFIX + "Clean canceled by user!");
				}

				SearchResultsStorage.CleanerSearchResults = records;
				if (showResults) MaintainerWindow.ShowCleaner();

				return cleanedRecords.ToArray();
			}

			return null;
		}

		[DidReloadScripts]
		private static void AutoCleanFolders()
		{
			if (!MaintainerSettings.Cleaner.findEmptyFolders || !MaintainerSettings.Cleaner.findEmptyFoldersAutomatically) return;

			List<CleanerRecord> results = new List<CleanerRecord>();
			ScanFolders(results, false);

			if (results.Count > 0)
			{
				int result = EditorUtility.DisplayDialogComplex("Maintainer", MODULE_NAME + " found " + results.Count + " empty folders. Do you wish to remove them?\n" + DATA_LOSS_WARNING, "Yes", "No", "Show in Maintainer");
				if (result == 0)
				{
					CleanerRecord[] records = results.ToArray();
					CleanRecords(records, false);
					Debug.Log(Maintainer.LOG_PREFIX + results.Count + " empty folders cleaned.");
				}
				else if (result == 2)
				{
					SearchResultsStorage.CleanerSearchResults = results.ToArray();
					MaintainerWindow.ShowCleaner(); 
				}
			}
		}

		private static bool ScanFolders(ICollection<CleanerRecord> results, bool showProgress = true)
		{
			bool canceled;
			currentPhase++;

			folderIndex = 0;

			List<string> emptyFolders = new List<string>();
			string root = Application.dataPath;

			foldersCount = Directory.GetDirectories(root, "*", SearchOption.AllDirectories).Length;
			FindEmptyFoldersRecursive(emptyFolders, root, showProgress, out canceled);

			ExcludeSubFoldersOfEmptyFolders(ref emptyFolders);

			foreach (string folder in emptyFolders)
			{
				results.Add(AssetRecord.Create(RecordType.EmptyFolder, folder));
			}

			return !canceled;
		}

		private static bool ScanSceneFiles(List<CleanerRecord> results, bool showProgress = true)
		{
			bool canceled = false;
			currentPhase++;

			string[] scenesPaths = CSEditorTools.FindAssetsFiltered("t:Scene", MaintainerSettings.Cleaner.pathIgnores);

			int scenesCount = scenesPaths.Length;

			for (int i = 0; i < scenesCount; i++)
			{
				if (showProgress && EditorUtility.DisplayCancelableProgressBar(string.Format(PROGRESS_CAPTION, currentPhase, phasesCount, i+1, scenesCount), "Scanning scene files...", (float)i / scenesCount))
				{
					canceled = true;
					break;
				}

				string scenePath = scenesPaths[i];

				if (CSSceneTools.GetCurrentScenePath() != scenePath)
				{
					CSSceneTools.OpenScene(scenePath);
				}

#if UNITY_5_3_PLUS
				// if we're scanning currently opened scene and going to scan more scenes,
				// we need to close all additional scenes to avoid duplicates
				else if (EditorSceneManager.loadedSceneCount > 1 && scenesCount > 1)
				{
					CSSceneTools.CloseAllScenesButActive();
				}
#endif
				int objectsInScene = 0;

				GameObject[] gameObjects = CSEditorTools.GetAllSuitableGameObjectsInCurrentScene();
				objectsInScene = gameObjects.Length;

				if (objectsInScene == 0)
				{
					results.Add(AssetRecord.Create(RecordType.EmptyScene, scenePath));
				}
			}

			return !canceled;
		}

		private static void ExcludeSubFoldersOfEmptyFolders(ref List<string> emptyFolders)
		{
			List<string> emptyFoldersFiltered = new List<string>(emptyFolders.Count);
			for (int i = emptyFolders.Count-1; i >= 0; i--)
			{
				string folder = emptyFolders[i];
				if (!CSArrayTools.IsItemContainsAnyStringFromArray(folder, emptyFoldersFiltered))
				{
					emptyFoldersFiltered.Add(folder);
				}
			}
			emptyFolders = emptyFoldersFiltered;
		}

		private static bool FindEmptyFoldersRecursive(List<string> foundEmptyFolders, string root, bool showProgress, out bool canceledByUser)
		{
			string[] rootSubFolders = Directory.GetDirectories(root);

			bool canceled = false;
			bool emptySubFolders = true;
			foreach (string folder in rootSubFolders)
			{
				folderIndex++;

				if (showProgress && EditorUtility.DisplayCancelableProgressBar(string.Format(PROGRESS_CAPTION, currentPhase, phasesCount, folderIndex, foldersCount), "Scanning folders...", (float)folderIndex / foldersCount))
				{
					canceled = true;
					break;
				}

				if (CSArrayTools.IsItemContainsAnyStringFromArray(folder.Replace('\\', '/'), MaintainerSettings.Cleaner.pathIgnores))
				{
					emptySubFolders = false;
					continue;
				}

				if (Path.GetFileName(folder).StartsWith("."))
				{
					continue;
				}

				emptySubFolders &= FindEmptyFoldersRecursive(foundEmptyFolders, folder, showProgress, out canceled);
				if (canceled) break;
			}

			if (canceled)
			{
				canceledByUser = true;
				return false;
			}

			bool rootFolderHasFiles = true;
			string[] filesInRootFolder = Directory.GetFiles(root);

			foreach (string file in filesInRootFolder)
			{
				if (file.EndsWith(".meta")) continue;

				rootFolderHasFiles = false;
				break;
			}

			bool rootFolderEmpty = emptySubFolders && rootFolderHasFiles;
			if (rootFolderEmpty)
			{
				foundEmptyFolders.Add(root);
			}

			canceledByUser = false;
			return rootFolderEmpty;
		}

		private static bool CleanRecords(IEnumerable<CleanerRecord> results, bool showProgress = true)
		{
			bool canceled = false;
			int itemsCleaned = 0;

			AssetDatabase.StartAssetEditing();

			foreach (CleanerRecord item in results)
			{
				if (showProgress && EditorUtility.DisplayCancelableProgressBar(string.Format(PROGRESS_CAPTION, currentPhase, phasesCount, itemsCleaned + 1, itemsToClean), "Cleaning selected items...", (float)itemsCleaned/itemsToClean))
				{
					canceled = true;
					break;
				}

				if (item.selected)
				{
					itemsCleaned++;
					item.Clean(); 
				}
			}

			AssetDatabase.StopAssetEditing();

			AssetDatabase.Refresh();

			return canceled;
		}
	}
}

#endif