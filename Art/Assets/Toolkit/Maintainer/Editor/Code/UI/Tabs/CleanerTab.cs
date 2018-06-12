#if UNITY_EDITOR

#define UNITY_5_3_PLUS
#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
#undef UNITY_5_3_PLUS
#endif

using System.IO;
using System.Linq;
using CodeStage.Maintainer.Cleaner;
using CodeStage.Maintainer.Settings;
using CodeStage.Maintainer.Tools;
using CodeStage.Maintainer.UI.Ignores;
using UnityEditor;
using UnityEngine;

namespace CodeStage.Maintainer.UI
{
	internal class CleanerTab : RecordsTab
	{
		private bool startSearch;
		private bool startClean;

		protected override void LoadLastRecords()
		{
			// ReSharper disable CoVariantArrayConversion
			records = SearchResultsStorage.CleanerSearchResults;
			if (records == null)
			{
				records = new CleanerRecord[0];
			}
			// ReSharper restore CoVariantArrayConversion
		}

		protected override void ProcessUserActions()
		{
			if (startSearch)
			{
				startSearch = false;
				window.RemoveNotification();
				ProjectCleaner.StartSearch(true);
				window.Focus();
			}

			if (startClean)
			{
				startClean = false;
				window.RemoveNotification();
				ProjectCleaner.StartClean();
				window.Focus();
			}
		}

		protected override void DrawSettingsBody()
		{
			using (UIHelpers.Vertical(UIHelpers.panelWithBackground))
			{
				GUILayout.Space(5);

				if (GUILayout.Button("Manage Ignores..."))
				{
					CleanerIgnoresWindow.Create();
				}

				MaintainerSettings.Cleaner.useTrashBin = EditorGUILayout.ToggleLeft(new GUIContent("Use Trash Bin", "All deleted items will be moved to Trash if selected. Otherwise items will be deleted permanently."), MaintainerSettings.Cleaner.useTrashBin);

				GUILayout.Space(5);
			}

			using (UIHelpers.Vertical(UIHelpers.panelWithBackground, GUILayout.ExpandHeight(true)))
			{
				GUILayout.Space(5);
				GUILayout.Label("<b>Search for:</b>", UIHelpers.richLabel);
				GUILayout.Space(5);

				settingsSectionScrollPosition = GUILayout.BeginScrollView(settingsSectionScrollPosition, GUIStyle.none, GUI.skin.verticalScrollbar);

				using (UIHelpers.Horizontal())
				{
					MaintainerSettings.Cleaner.findEmptyFolders = EditorGUILayout.ToggleLeft(new GUIContent("Empty folders", "Search for all empty folders in project."), MaintainerSettings.Cleaner.findEmptyFolders, GUILayout.Width(100));
					GUI.enabled = MaintainerSettings.Cleaner.findEmptyFolders;

					EditorGUI.BeginChangeCheck();
					MaintainerSettings.Cleaner.findEmptyFoldersAutomatically = EditorGUILayout.ToggleLeft(new GUIContent("Autorun on script reload", "Perform empty folders clean automatically on every scripts reload."), MaintainerSettings.Cleaner.findEmptyFoldersAutomatically);
					if (EditorGUI.EndChangeCheck())
					{
						if (MaintainerSettings.Cleaner.findEmptyFoldersAutomatically)
							EditorUtility.DisplayDialog(ProjectCleaner.MODULE_NAME, "In case you're having thousands of folders in your project this may hang Unity for few additional secs on every scripts reload.\n" + ProjectCleaner.DATA_LOSS_WARNING, "Understood");
					}
					GUI.enabled = true;
				}

				MaintainerSettings.Cleaner.findEmptyScenes = EditorGUILayout.ToggleLeft(new GUIContent("Empty scenes", "Search for all empty scenes in project."), MaintainerSettings.Cleaner.findEmptyScenes);

				GUILayout.EndScrollView();
				UIHelpers.Separator();
				GUILayout.Space(5);

				using (UIHelpers.Horizontal())
				{
					if (GUILayout.Button("Check all"))
					{
						MaintainerSettings.Cleaner.SwitchAll(true);
					}

					if (GUILayout.Button("Uncheck all"))
					{
						MaintainerSettings.Cleaner.SwitchAll(false);
					}
				}
			}

			if (GUILayout.Button(new GUIContent("Reset", "Resets settings to defaults.")))
			{
				MaintainerSettings.Cleaner.Reset();
			}
		}

		protected override void DrawSearchBody()
		{
			//GUILayout.BeginHorizontal();
			if (GUILayout.Button("1. Find garbage!"))
			{
				startSearch = true;
			}

			if (GUILayout.Button("2. Clean selected items!"))
			{
				startClean = true;
			}
			//GUILayout.EndHorizontal();
			GUILayout.Space(10);

			if (records == null || records.Length == 0)
			{
				GUILayout.Label("No garbage");
			}
			else
			{
				ShowCollectionPages();

				GUILayout.Space(5);

				using (UIHelpers.Horizontal())
				{
					if (GUILayout.Button("Select all"))
					{
						foreach (CleanerRecord record in records.OfType<CleanerRecord>())
						{
							record.selected = true;
						}
						SearchResultsStorage.Save();
					}

					if (GUILayout.Button("Select none"))
					{
						foreach (CleanerRecord record in records.OfType<CleanerRecord>())
						{
							record.selected = false;
						}
						SearchResultsStorage.Save();
					}

					if (GUILayout.Button("Clear results"))
					{
						records = null;
						SearchResultsStorage.CleanerSearchResults = null;
					}
				}

				using (UIHelpers.Horizontal())
				{
					if (GUILayout.Button("Copy report to clipboard"))
					{
						EditorGUIUtility.systemCopyBuffer = ReportsBuilder.GenerateReport(ProjectCleaner.MODULE_NAME, records);
						MaintainerWindow.ShowNotification("Report copied to clipboard!");
					}
					if (GUILayout.Button("Export report..."))
					{
						string filePath = EditorUtility.SaveFilePanel("Save " + ProjectCleaner.MODULE_NAME + " report", "", "MaintainerCleanerReport.txt", "txt");
						if (!string.IsNullOrEmpty(filePath))
						{
							StreamWriter sr = File.CreateText(filePath);
							sr.Write(ReportsBuilder.GenerateReport(ProjectCleaner.MODULE_NAME, records));
							sr.Close();
							MaintainerWindow.ShowNotification("Report saved!");
						}
					}
				}
			}
		}

		protected override void DrawRecord(int recordIndex, out bool recordRemoved)
		{
			recordRemoved = false;
			RecordBase record = records[recordIndex];
			CleanerRecord cleanerRecord = record as CleanerRecord;
			if (cleanerRecord == null)
			{
				GUILayout.Label("Incorrect record! Please report this to support.");
				return;
			}

			// hide cleaned records 
			if (cleanerRecord.cleaned) return;

			UIHelpers.Separator();

			using (UIHelpers.Horizontal())
			{
				DrawRecordCheckbox(cleanerRecord);

				using (UIHelpers.Vertical())
				{
					/* header */
					using (UIHelpers.Horizontal())
					{
						DrawSeverityIcon(record);
						GUILayout.Label(record.GetHeader(), UIHelpers.richLabel, GUILayout.ExpandWidth(false));
					}

					/* body */
					GUILayout.Label(record.GetBody(), UIHelpers.richLabel);

					DrawRecordButtons(record);
				}
			}
		}

		private void DrawRecordCheckbox(CleanerRecord cleanerRecord)
		{
			EditorGUI.BeginChangeCheck();
			cleanerRecord.selected = EditorGUILayout.ToggleLeft(new GUIContent(""), cleanerRecord.selected, GUILayout.Width(20));
			if (EditorGUI.EndChangeCheck())
			{
				SearchResultsStorage.Save();
			}
		}

		private void DrawRecordButtons(RecordBase record)
		{
			using (UIHelpers.Horizontal(UIHelpers.panelWithBackground))
			{
				AddShowButtonIfPossible(record);

				AssetRecord assetRecord = record as AssetRecord;
				if (assetRecord != null)
				{
					if (GUILayout.Button(new GUIContent("Reveal", "Reveals item in system default File Manager like Explorer on Windows or Finder on Mac."), UIHelpers.recordButton))
					{
						EditorUtility.RevealInFinder(assetRecord.path);
					}

					if (GUILayout.Button("More ...", UIHelpers.recordButton))
					{
						GenericMenu menu = new GenericMenu();
						if (!string.IsNullOrEmpty(assetRecord.path))
						{
							menu.AddItem(new GUIContent("Ignore/Add path to ignores"), false, () =>
							{
								if (CSArrayTools.AddIfNotExists(ref MaintainerSettings.Cleaner.pathIgnores, assetRecord.assetDatabasePath))
								{
									MaintainerSettings.Save();
									MaintainerWindow.ShowNotification("Ignore added: " + assetRecord.assetDatabasePath);
									CleanerIgnoresWindow.Refresh();
								}
								else
								{
									MaintainerWindow.ShowNotification("Such item already added to the ignores!");
								}
							});

							DirectoryInfo dir = Directory.GetParent(assetRecord.assetDatabasePath);
							if (dir.Name != "Assets")
							{
								menu.AddItem(new GUIContent("Ignore/Add parent directory to ignores"), false, () =>
								{
									if (CSArrayTools.AddIfNotExists(ref MaintainerSettings.Cleaner.pathIgnores, dir.ToString()))
									{
										MaintainerSettings.Save();
										MaintainerWindow.ShowNotification("Ignore added: " + dir);
										CleanerIgnoresWindow.Refresh();
									}
									else
									{
										MaintainerWindow.ShowNotification("Such item already added to the ignores!");
									}
								});
							}
						}
						menu.ShowAsContext();
					}
				}
			}
		}
	}
}

#endif