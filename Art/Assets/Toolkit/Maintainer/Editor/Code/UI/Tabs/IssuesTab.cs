#if UNITY_EDITOR

#define UNITY_5_3_PLUS
#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
#undef UNITY_5_3_PLUS
#endif

using System.IO;
using System.Reflection;
using CodeStage.Maintainer.Issues;
using CodeStage.Maintainer.Settings;
using CodeStage.Maintainer.Tools;
using CodeStage.Maintainer.UI.Ignores;
using UnityEditor;
using UnityEngine;

namespace CodeStage.Maintainer.UI
{
	internal class IssuesTab : RecordsTab
	{
		private bool startSearch;
		
		protected override void LoadLastRecords()
		{
			// ReSharper disable CoVariantArrayConversion
			records = SearchResultsStorage.IssuesSearchResults;
			if (records == null)
			{
				records = new IssueRecord[0];
			}
			// ReSharper restore CoVariantArrayConversion
		}

		protected override void ProcessUserActions()
		{
			if (!startSearch) return;

			startSearch = false;
			window.RemoveNotification();
			IssuesFinder.StartSearch(true);
			window.Focus();
		}

		protected override void DrawSettingsBody()
		{
			// ----------------------------------------------------------------------------
			// filtering settings
			// ----------------------------------------------------------------------------

			using (UIHelpers.Vertical(UIHelpers.panelWithBackground))
			{
				GUILayout.Space(5);

				if (GUILayout.Button("Manage Ignores..."))
				{
					IssuesIgnoresWindow.Create();
				}

				GUILayout.Space(5);

				/* Game Object Issues filtering */

				GUILayout.Label("Game Object Issues filtering");
				UIHelpers.Separator();
				GUILayout.Space(5);

				using (UIHelpers.Horizontal())
				{
					MaintainerSettings.Issues.lookInScenes = EditorGUILayout.ToggleLeft(new GUIContent("Scenes", "Uncheck to exclude all scenes from search or select filtering level:\n\n" +
					                                                                                             "All Scenes: all project scenes.\n" +
					                                                                                             "Build Scenes: enabled scenes at Build Settings.\n" +
					                                                                                             "Current Scene: currently opened scene including any additional loaded scenes."), MaintainerSettings.Issues.lookInScenes, GUILayout.Width(70));
					GUI.enabled = MaintainerSettings.Issues.lookInScenes;
					MaintainerSettings.Issues.scenesSelection = (IssuesFinderSettings.ScenesSelection)EditorGUILayout.EnumPopup(MaintainerSettings.Issues.scenesSelection);
					GUI.enabled = true;
				}

				MaintainerSettings.Issues.lookInAssets = EditorGUILayout.ToggleLeft(new GUIContent("Prefab assets", "Uncheck to exclude all prefab assets files from the search. Check readme for additional details."), MaintainerSettings.Issues.lookInAssets);
				MaintainerSettings.Issues.touchInactiveGameObjects = EditorGUILayout.ToggleLeft(new GUIContent("Inactive GameObjects", "Uncheck to exclude all inactive Game Objects from the search."), MaintainerSettings.Issues.touchInactiveGameObjects);
				MaintainerSettings.Issues.touchDisabledComponents = EditorGUILayout.ToggleLeft(new GUIContent("Disabled Components", "Uncheck to exclude all disabled Components from the search."), MaintainerSettings.Issues.touchDisabledComponents);

				GUILayout.Space(2);
			}

			using (UIHelpers.Vertical(UIHelpers.panelWithBackground, GUILayout.ExpandHeight(true)))
			{
				GUILayout.Space(5);
				GUILayout.Label("<b>Search for:</b>", UIHelpers.richLabel);

				//GUILayout.Space(5);

				settingsSectionScrollPosition = GUILayout.BeginScrollView(settingsSectionScrollPosition, GUIStyle.none, GUI.skin.verticalScrollbar);

				// ----------------------------------------------------------------------------
				// Game Object Issues
				// ----------------------------------------------------------------------------

				GUI.enabled = UIHelpers.ToggleFoldout(ref MaintainerSettings.Issues.scanGameObjects, ref MaintainerSettings.Issues.gameObjectsFoldout, new GUIContent("Game Object Issues", "Group of issues related to the Game Objects."));
				if (MaintainerSettings.Issues.gameObjectsFoldout)
				{
					UIHelpers.Indent();

					if (DrawSettingsSearchSectionHeader(SettingsSearchSection.Common, ref MaintainerSettings.Issues.commonFoldout))
					{
						MaintainerSettings.Issues.missingComponents = EditorGUILayout.ToggleLeft(new GUIContent("Missing components", "Search for the missing components on the Game Objects."), MaintainerSettings.Issues.missingComponents);

						using (UIHelpers.Horizontal())
						{
							MaintainerSettings.Issues.duplicateComponents = EditorGUILayout.ToggleLeft(new GUIContent("Duplicate components", "Search for the multiple instances of the same component with same values on the same object."), MaintainerSettings.Issues.duplicateComponents, GUILayout.Width(155));
							GUI.enabled = MaintainerSettings.Issues.duplicateComponents && MaintainerSettings.Issues.scanGameObjects;
							MaintainerSettings.Issues.duplicateComponentsPrecise = EditorGUILayout.ToggleLeft(new GUIContent("Precise mode", "Uncheck to ignore component's values."), MaintainerSettings.Issues.duplicateComponentsPrecise, GUILayout.Width(100));
							GUI.enabled = MaintainerSettings.Issues.scanGameObjects;
						}

						MaintainerSettings.Issues.missingReferences = EditorGUILayout.ToggleLeft(new GUIContent("Missing references", "Search for any missing references in the serialized fields of the components."), MaintainerSettings.Issues.missingReferences);
						MaintainerSettings.Issues.undefinedTags = EditorGUILayout.ToggleLeft(new GUIContent("Objects with undefined tags", "Search for GameObjects without any tag."), MaintainerSettings.Issues.undefinedTags);
						MaintainerSettings.Issues.inconsistentTerrainData = EditorGUILayout.ToggleLeft(new GUIContent("Inconsistent Terrain Data", "Search for Game Objects where Terrain and TerrainCollider have different Terrain Data."), MaintainerSettings.Issues.inconsistentTerrainData);
					}

					if (DrawSettingsSearchSectionHeader(SettingsSearchSection.PrefabsSpecific, ref MaintainerSettings.Issues.prefabsFoldout))
					{
						MaintainerSettings.Issues.missingPrefabs = EditorGUILayout.ToggleLeft(new GUIContent("Instances of missing prefabs", "Search for instances of prefabs which were removed from project."), MaintainerSettings.Issues.missingPrefabs);
						MaintainerSettings.Issues.disconnectedPrefabs = EditorGUILayout.ToggleLeft(new GUIContent("Instances of disconnected prefabs", "Search for disconnected prefabs instances."), MaintainerSettings.Issues.disconnectedPrefabs);
					}

					if (DrawSettingsSearchSectionHeader(SettingsSearchSection.UnusedComponents, ref MaintainerSettings.Issues.unusedFoldout))
					{
						MaintainerSettings.Issues.emptyMeshColliders = EditorGUILayout.ToggleLeft("MeshColliders without meshes", MaintainerSettings.Issues.emptyMeshColliders);
						MaintainerSettings.Issues.emptyMeshFilters = EditorGUILayout.ToggleLeft("MeshFilters without meshes", MaintainerSettings.Issues.emptyMeshFilters);
						MaintainerSettings.Issues.emptyAnimations = EditorGUILayout.ToggleLeft("Animations without clips", MaintainerSettings.Issues.emptyAnimations);
						MaintainerSettings.Issues.emptyRenderers = EditorGUILayout.ToggleLeft("Renders without materials", MaintainerSettings.Issues.emptyRenderers);
						MaintainerSettings.Issues.emptySpriteRenderers = EditorGUILayout.ToggleLeft("SpriteRenders without sprites", MaintainerSettings.Issues.emptySpriteRenderers);
						MaintainerSettings.Issues.emptyTerrainCollider = EditorGUILayout.ToggleLeft("TerrainColliders without Terrain Data", MaintainerSettings.Issues.emptyTerrainCollider);
						MaintainerSettings.Issues.emptyAudioSource = EditorGUILayout.ToggleLeft("AudioSources without AudioClips", MaintainerSettings.Issues.emptyAudioSource);
					}

					if (DrawSettingsSearchSectionHeader(SettingsSearchSection.Neatness, ref MaintainerSettings.Issues.neatnessFoldout))
					{
						using (UIHelpers.Horizontal())
						{
							MaintainerSettings.Issues.emptyArrayItems = EditorGUILayout.ToggleLeft(new GUIContent("Empty array items", "Look for any unused items in arrays."), MaintainerSettings.Issues.emptyArrayItems, GUILayout.Width(145));
							GUI.enabled = MaintainerSettings.Issues.emptyArrayItems && MaintainerSettings.Issues.scanGameObjects;
							MaintainerSettings.Issues.skipEmptyArrayItemsOnPrefabs = EditorGUILayout.ToggleLeft(new GUIContent("Skip prefab files", "Prefab files can be ignored using this toggle."), MaintainerSettings.Issues.skipEmptyArrayItemsOnPrefabs, GUILayout.Width(110));
							GUI.enabled = MaintainerSettings.Issues.scanGameObjects;
						}
						MaintainerSettings.Issues.unnamedLayers = EditorGUILayout.ToggleLeft(new GUIContent("Objects with unnamed layers", "Search for GameObjects with unnamed layers."), MaintainerSettings.Issues.unnamedLayers);
						MaintainerSettings.Issues.hugePositions = EditorGUILayout.ToggleLeft(new GUIContent("Objects with huge positions", "Search for GameObjects with huge world positions (> |100 000| on any axis)."), MaintainerSettings.Issues.hugePositions);
					}

					UIHelpers.UnIndent();
				}
				GUI.enabled = true;

				// ----------------------------------------------------------------------------
				// Project Settings Issues
				// ----------------------------------------------------------------------------

				GUI.enabled = UIHelpers.ToggleFoldout(ref MaintainerSettings.Issues.scanProjectSettings, ref MaintainerSettings.Issues.projectSettingsFoldout, new GUIContent("Project Settings Issues", "Group of issues related to the settings of the current project."));
				if (MaintainerSettings.Issues.projectSettingsFoldout)
				{
					UIHelpers.Indent();

					MaintainerSettings.Issues.duplicateScenesInBuild = EditorGUILayout.ToggleLeft(new GUIContent("Duplicate scenes in build", "Search for the duplicates at the 'Scenes In Build' section of the Build Settings."), MaintainerSettings.Issues.duplicateScenesInBuild);
					MaintainerSettings.Issues.duplicateTagsAndLayers = EditorGUILayout.ToggleLeft(new GUIContent("Duplicates in Tags and Layers", "Search for the duplicate items at the 'Tags and Layers' Project Settings."), MaintainerSettings.Issues.duplicateTagsAndLayers);

					UIHelpers.UnIndent();
				}
				GUI.enabled = true;

				GUILayout.EndScrollView();
				UIHelpers.Separator();

				//GUILayout.Space(5);
				using (UIHelpers.Horizontal())
				{
					if (GUILayout.Button("Check all"))
					{
						MaintainerSettings.Issues.SwitchAll(true);
					}

					if (GUILayout.Button("Uncheck all"))
					{
						MaintainerSettings.Issues.SwitchAll(false);
					}
				}
			}

			if (GUILayout.Button(new GUIContent("Reset", "Resets settings to defaults.")))
			{
				MaintainerSettings.Issues.Reset();
			}
		}

		protected override void DrawSearchBody()
		{
			if (GUILayout.Button("Find issues!"))
			{
				startSearch = true;
			}
			GUILayout.Space(10);

			if (records == null || records.Length == 0)
			{
				GUILayout.Label("No issues");
			}
			else
			{
				ShowCollectionPages();

				GUILayout.Space(5);

				using (UIHelpers.Horizontal())
				{
					if (GUILayout.Button("Copy report to clipboard"))
					{
						EditorGUIUtility.systemCopyBuffer = ReportsBuilder.GenerateReport(IssuesFinder.MODULE_NAME, records);
						MaintainerWindow.ShowNotification("Report copied to clipboard!");
					}

					if (GUILayout.Button("Export report..."))
					{
						string filePath = EditorUtility.SaveFilePanel("Save Issues Finder report", "", "MaintainerIssuesReport.txt", "txt");
						if (!string.IsNullOrEmpty(filePath))
						{
							StreamWriter sr = File.CreateText(filePath);
							sr.Write(ReportsBuilder.GenerateReport(IssuesFinder.MODULE_NAME, records));
							sr.Close();
							MaintainerWindow.ShowNotification("Report saved!");
						}
					}

					if (GUILayout.Button("Clear results"))
					{
						records = null;
						SearchResultsStorage.IssuesSearchResults = null;
					}
				}
			}
		}

		protected override void DrawRecord(int recordIndex, out bool recordRemoved)
		{
			recordRemoved = false;
			RecordBase record = records[recordIndex];

			UIHelpers.Separator();

			using (UIHelpers.Horizontal())
			{
				DrawSeverityIcon(record);
				if (record.location == RecordLocation.Prefab)
				{
					UIHelpers.DrawPrefabIcon();
				}
				GUILayout.Label(record.GetHeader(), UIHelpers.richLabel, GUILayout.ExpandWidth(false));
			}

			GUILayout.Label(record.GetBody(), UIHelpers.richLabel);

			using (UIHelpers.Horizontal(UIHelpers.panelWithBackground))
			{
				AddShowButtonIfPossible(record);

				if (GUILayout.Button(new GUIContent("Copy", "Copies record text to the clipboard."), UIHelpers.recordButton))
				{
					EditorGUIUtility.systemCopyBuffer = record.ToString(true);
					MaintainerWindow.ShowNotification("Issue record copied to clipboard!");
				}

				if (GUILayout.Button(new GUIContent("Hide", "Hide this issue from the results list.\nUseful when you fixed issue and wish to hide it away."), UIHelpers.recordButton))
				{
					// ReSharper disable once CoVariantArrayConversion
					records = CSArrayTools.RemoveAt(records as IssueRecord[], recordIndex);
					SearchResultsStorage.IssuesSearchResults = (IssueRecord[])records;
					recordRemoved = true;
					return;
				}

				//UIHelpers.VerticalSeparator();
				GameObjectIssueRecord objectIssue = record as GameObjectIssueRecord;
				if (objectIssue != null)
				{
					if (GUILayout.Button("More ...", UIHelpers.recordButton))
					{
						GenericMenu menu = new GenericMenu();
						if (!string.IsNullOrEmpty(objectIssue.path))
						{
							menu.AddItem(new GUIContent("Ignore/Add path to ignores"), false, () =>
							{
								if (CSArrayTools.AddIfNotExists(ref MaintainerSettings.Issues.pathIgnores, objectIssue.path))
								{
									MaintainerSettings.Save();
									MaintainerWindow.ShowNotification("Ignore added: " + objectIssue.path);
									IssuesIgnoresWindow.Refresh();
								}
								else
								{
									MaintainerWindow.ShowNotification("Such item already added to the ignores!");
								}
							});

							DirectoryInfo dir = Directory.GetParent(objectIssue.path);
							if (dir.Name != "Assets")
							{
								menu.AddItem(new GUIContent("Ignore/Add parent directory to ignores"), false, () =>
								{
									if (CSArrayTools.AddIfNotExists(ref MaintainerSettings.Issues.pathIgnores, dir.ToString()))
									{
										MaintainerSettings.Save();
										MaintainerWindow.ShowNotification("Ignore added: " + dir);
										IssuesIgnoresWindow.Refresh();
									}
									else
									{
										MaintainerWindow.ShowNotification("Such item already added to the ignores!");
									}
								});
							}
						}

						if (!string.IsNullOrEmpty(objectIssue.component))
						{
							menu.AddItem(new GUIContent("Ignore/Add component to ignores"), false, () =>
							{
								if (CSArrayTools.AddIfNotExists(ref MaintainerSettings.Issues.componentIgnores, objectIssue.component))
								{
									MaintainerSettings.Save();
									MaintainerWindow.ShowNotification("Ignore added: " + objectIssue.component);
									IssuesIgnoresWindow.Refresh();
								}
								else
								{
									MaintainerWindow.ShowNotification("Such item already added to the ignores!");
								}
							});
						}
						menu.ShowAsContext();
					}
				}
			}
		}

		private bool DrawSettingsSearchSectionHeader(SettingsSearchSection section, ref bool foldout)
		{
			GUILayout.Space(5);
			using (UIHelpers.Horizontal())
			{
				foldout = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), foldout, "<b>" + ObjectNames.NicifyVariableName(section.ToString()) + "</b>", true, UIHelpers.richFoldout);
				if (GUILayout.Button("V", UIHelpers.compactButton, GUILayout.Width(20)))
				{
					typeof(IssuesFinderSettings).InvokeMember("Switch" + section, BindingFlags.InvokeMethod | BindingFlags.NonPublic, null, MaintainerSettings.Issues, new[] {(object)true});
				}
				if (GUILayout.Button("X", UIHelpers.compactButton, GUILayout.Width(20)))
				{
					typeof(IssuesFinderSettings).InvokeMember("Switch" + section, BindingFlags.InvokeMethod | BindingFlags.NonPublic, null, MaintainerSettings.Issues, new[] {(object)false});
				}
				GUILayout.Space(5);
			}
			UIHelpers.Separator();

			return foldout;
		}
	}

	internal enum SettingsSearchSection : byte
	{
		Common,
		PrefabsSpecific,
		UnusedComponents,
		Neatness,
	}
}

#endif