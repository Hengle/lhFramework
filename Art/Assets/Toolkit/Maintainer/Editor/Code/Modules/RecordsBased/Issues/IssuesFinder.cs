#define UNITY_5_3_PLUS
#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
#undef UNITY_5_3_PLUS
#endif

#if UNITY_5_3_PLUS
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using CodeStage.Maintainer.Settings;
using CodeStage.Maintainer.Tools;
using CodeStage.Maintainer.UI;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace CodeStage.Maintainer.Issues
{
	/// <summary>
	/// Allows to find issues in your Unity project. See readme for details.
	/// </summary>
	public class IssuesFinder
	{
		internal const string MODULE_NAME = "Issues Finder";
		private const string PROGRESS_CAPTION = MODULE_NAME + ": phase {0} of {1}, item {2} of {3}";

		private static string[] scenesPaths;
		private static List<GameObject> prefabs;
		private static List<string> prefabsPaths;

		private static int phasesCount;
		private static int currentPhase;

		private static int scenesCount;
		private static int prefabsCount;

		private static string searchStartScene;

		/// <summary>
		/// Starts issues search and generates report. %Maintainer window is not shown.
		/// Useful when you wish to integrate %Maintainer in your build pipeline.
		/// </summary>
		/// <returns>%Issues report, similar to the exported report from the %Maintainer window.</returns>
		public static string SearchAndReport()
		{
			IssueRecord[] foundIssues = StartSearch(false);

			// ReSharper disable once CoVariantArrayConversion
			return ReportsBuilder.GenerateReport(MODULE_NAME, foundIssues);
		}

		/// <summary>
		/// Starts search with current settings.
		/// </summary>
		/// <param name="showResults">Shows results in the %Maintainer window if true.</param>
		/// <returns>Array of IssueRecords in case you wish to manually iterate over them and make custom report.</returns>
		public static IssueRecord[] StartSearch(bool showResults)
		{
			phasesCount = 0;

            if (MaintainerSettings.Issues.scanGameObjects && MaintainerSettings.Issues.lookInScenes)
			{
				if (MaintainerSettings.Issues.scenesSelection != IssuesFinderSettings.ScenesSelection.CurrentSceneOnly)
				{
					if (!CSSceneTools.SaveCurrentSceneIfUserWantsTo())
					{
						Debug.Log(Maintainer.LOG_PREFIX + "Issues search canceled by user!");
						return null;
					}
					searchStartScene = CSSceneTools.GetCurrentScenePath(true);
				}
				else
				{
					searchStartScene = CSSceneTools.GetCurrentScenePath();
				}
			}

			List<IssueRecord> issues = new List<IssueRecord>();
			Stopwatch sw = Stopwatch.StartNew();

			try
			{
				CollectInput();

				bool searchCanceled = false;

				if (MaintainerSettings.Issues.scanGameObjects)
				{
					if (MaintainerSettings.Issues.lookInScenes)
					{
						searchCanceled = !ProcessSelectedScenes(issues);
					}

					if (!searchCanceled && MaintainerSettings.Issues.lookInAssets)
					{
						searchCanceled = !ProcessPrefabFiles(issues);
					}
				}

				if (MaintainerSettings.Issues.scanProjectSettings)
				{
					if (!searchCanceled)
					{
						searchCanceled = !ProcessSettings(issues);
					}
				}
				sw.Stop();

				if (!searchCanceled)
				{
					Debug.Log(Maintainer.LOG_PREFIX + MODULE_NAME + " results: " + issues.Count + 
						" issues in " + sw.Elapsed.TotalSeconds.ToString("0.000") + 
						" seconds, " + scenesCount + " scenes and " + prefabsCount + " prefabs scanned.");
				}
				else
				{
					Debug.Log(Maintainer.LOG_PREFIX + "Search canceled by user!");
				}

				SearchResultsStorage.IssuesSearchResults = issues.ToArray();
				if (showResults) MaintainerWindow.ShowIssues();
			}
			catch (Exception e)
			{
				Debug.LogError(Maintainer.LOG_PREFIX + MODULE_NAME + ": something went wrong :(\n" + e);
			}

			FinishSearch();
			return issues.ToArray();
		}

		private static void CollectInput()
		{
			phasesCount = 0;
			currentPhase = 0;

			scenesCount = 0;
			prefabsCount = 0;

			if (MaintainerSettings.Issues.scanGameObjects)
			{
				if (MaintainerSettings.Issues.lookInScenes)
				{
					EditorUtility.DisplayProgressBar(MODULE_NAME, "Collecting input data: Scenes...", 0);

					if (MaintainerSettings.Issues.scenesSelection == IssuesFinderSettings.ScenesSelection.AllScenes)
					{
						scenesPaths = CSEditorTools.FindAssetsFiltered("t:Scene", MaintainerSettings.Issues.pathIgnores); //Directory.GetFiles("Assets", "*.unity", SearchOption.AllDirectories);
					}
					else if (MaintainerSettings.Issues.scenesSelection == IssuesFinderSettings.ScenesSelection.BuildScenesOnly)
					{
						scenesPaths = GetEnabledScenesInBuild();
					}
					else
					{
						scenesPaths = new[] {CSSceneTools.GetCurrentScenePath()};
					}

					scenesCount = scenesPaths.Length;

					/*for (int i = 0; i < scenesCount; i++)
					{
						scenesPaths[i] = scenesPaths[i].Replace('\\', '/');
					}*/

					phasesCount++;
				}

				if (MaintainerSettings.Issues.lookInAssets)
				{
					if (prefabs == null)
						prefabs = new List<GameObject>();
					else
						prefabs.Clear();

					if (prefabsPaths == null)
						prefabsPaths = new List<string>();
					else
						prefabsPaths.Clear();

					EditorUtility.DisplayProgressBar(MODULE_NAME, "Collecting input data: Prefabs...", 0);

					string[] filteredPaths = CSEditorTools.FindAssetsFiltered("t:Prefab", MaintainerSettings.Issues.pathIgnores);
					prefabsCount = CSEditorTools.GetSuitablePrefabsFromSelection(filteredPaths, prefabs, prefabsPaths);

					phasesCount++;
				}
			}

			if (MaintainerSettings.Issues.scanProjectSettings)
			{
				phasesCount++;
			}
		}

		private static bool ProcessSelectedScenes(List<IssueRecord> issues)
		{
			bool result = true;
			currentPhase ++;

			for (int i = 0; i < scenesCount; i++)
			{
				string scenePath = scenesPaths[i];
				string sceneName = Path.GetFileNameWithoutExtension(scenePath);

				if (EditorUtility.DisplayCancelableProgressBar(string.Format(PROGRESS_CAPTION, currentPhase, phasesCount, i+1, scenesCount), string.Format("Opening scene: " + Path.GetFileNameWithoutExtension(scenePath)), (float)i / scenesCount))
				{
					result = false;
					break;
				}

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

				GameObject[] gameObjects = CSEditorTools.GetAllSuitableGameObjectsInCurrentScene();
				int objectsCount = gameObjects.Length;

				for (int j = 0; j < objectsCount; j++)
				{
					if (EditorUtility.DisplayCancelableProgressBar(string.Format(PROGRESS_CAPTION, currentPhase, phasesCount, i+1, scenesCount), string.Format("Processing scene: {0} ... {1}%", sceneName, j * 100 / objectsCount), (float)i / scenesCount))
					{
						result = false;
						break;
					}

					CheckObjectForIssues(issues, scenePath, gameObjects[j], true);
				}

				if (!result) break;
			}

			return result;
		}

		private static bool ProcessPrefabFiles(List<IssueRecord> issues)
		{
			bool result = true;
			currentPhase++;

			for (int i = 0; i < prefabsCount; i++)
			{
				if (EditorUtility.DisplayCancelableProgressBar(string.Format(PROGRESS_CAPTION, currentPhase, phasesCount, i+1, prefabsCount), "Processing prefabs files...", (float)i / prefabsCount))
				{
					result = false;
					break;
				}

				CheckObjectForIssues(issues, prefabsPaths[i], prefabs[i], false);
			}

			return result;
		}

		private static void CheckObjectForIssues(List<IssueRecord> issues, string path, GameObject go, bool checkingScene)
		{
			RecordLocation location = checkingScene ? RecordLocation.Scene : RecordLocation.Prefab;

			// ----------------------------------------------------------------------------
			// looking for object-level issues
			// ----------------------------------------------------------------------------

			if (!MaintainerSettings.Issues.touchInactiveGameObjects)
			{
				if (checkingScene)
				{
					if (!go.activeInHierarchy) return;
				}
				else
				{
					if (!go.activeSelf) return;
				}
			}

			// ----------------------------------------------------------------------------
			// checking all components for ignores
			// ----------------------------------------------------------------------------

			bool checkForIgnores = (MaintainerSettings.Issues.componentIgnores != null && MaintainerSettings.Issues.componentIgnores.Length > 0);
			bool skipEmptyMeshFilter = false;
			bool skipEmptyAudioSource = false;

			Component[] allComponents = go.GetComponents<Component>();
			int allComponentsCount = allComponents.Length;

			List<Component> components = new List<Component>(allComponentsCount);
			List<Type> componentsTypes = new List<Type>(allComponentsCount);
			List<string> componentsNames = new List<string>(allComponentsCount);
			List<string> componentsFullNames = new List<string>(allComponentsCount);
			List<string> componentsNamespaces = new List<string>(allComponentsCount);

			int componentsCount = 0;

			for (int i = 0; i < allComponentsCount; i++)
			{
				Component component = allComponents[i];

				if (component == null)
				{
					if (MaintainerSettings.Issues.missingComponents)
					{
						issues.Add(GameObjectIssueRecord.Create(RecordType.MissingComponent, location, path, go));
					}
					continue;
				}

				Type componentType = component.GetType();
				string componentName = componentType.Name;
				string componentFullName = componentType.FullName;
				string componentNamespace = componentType.Namespace;

				/* 
				   checking object for the components which may affect 
				   other components and produce false positives 
				*/

				// allowing empty mesh filters for the objects with attached TextMeshPro and 2D Toolkit components.
				if (!skipEmptyMeshFilter)
				{
					skipEmptyMeshFilter = (componentFullName == "TMPro.TextMeshPro") || componentName.StartsWith("tk2d");
				}

				// allowing empty AudioSources for the objects with attached standard FirstPersonController.
				if (!skipEmptyAudioSource)
				{
					skipEmptyAudioSource = componentFullName == "UnityStandardAssets.Characters.FirstPerson.FirstPersonController";
				}

				// skipping disabled components
				if (!MaintainerSettings.Issues.touchDisabledComponents)
				{
					if (EditorUtility.GetObjectEnabled(component) == 0) continue;
				}

				// skipping ignored components
				if (checkForIgnores)
				{
					if (Array.IndexOf(MaintainerSettings.Issues.componentIgnores, componentName) != -1) continue;
				}

				components.Add(component);
				componentsTypes.Add(componentType);
				componentsNames.Add(componentName);
				componentsFullNames.Add(componentFullName);
				componentsNamespaces.Add(componentNamespace);
				componentsCount++;
			}

			// ----------------------------------------------------------------------------
			// checking stuff related to the prefabs in scenes
			// ----------------------------------------------------------------------------

			if (checkingScene)
			{
				PrefabType prefabType = PrefabUtility.GetPrefabType(go);

				if (prefabType != PrefabType.None)
				{
					/* checking if we're inside of nested prefab with same type as root,
					   allows to skip detections of missed and disconnected prefabs children */

					GameObject rootPrefab = PrefabUtility.FindRootGameObjectWithSameParentPrefab(go);
					bool rootPrefabHasSameType = false;
					if (rootPrefab != go)
					{
						PrefabType rootPrefabType = PrefabUtility.GetPrefabType(rootPrefab);
						if (rootPrefabType == prefabType)
						{
							rootPrefabHasSameType = true;
						}
					}

					if (prefabType == PrefabType.MissingPrefabInstance)
					{
						if (MaintainerSettings.Issues.missingPrefabs && !rootPrefabHasSameType)
						{
							issues.Add(GameObjectIssueRecord.Create(RecordType.MissingPrefab, location, path, go));
						}
					}
					else if (prefabType == PrefabType.DisconnectedPrefabInstance ||
							 prefabType == PrefabType.DisconnectedModelPrefabInstance)
					{
						if (MaintainerSettings.Issues.disconnectedPrefabs && !rootPrefabHasSameType)
						{
							issues.Add(GameObjectIssueRecord.Create(RecordType.DisconnectedPrefab, location, path, go));
						}
					}

					/* checking if this game object is actually prefab instance
					   without any changes, so we can skip it if we have assets search enabled */

					if (prefabType != PrefabType.DisconnectedPrefabInstance &&
						prefabType != PrefabType.DisconnectedModelPrefabInstance &&
						prefabType != PrefabType.MissingPrefabInstance && MaintainerSettings.Issues.lookInAssets)
					{
						bool skipThisPrefabInstance = true;
						
						// we shouldn't skip object if it's nested deeper 2nd level
						if (CSEditorTools.GetDepthInHierarchy(go.transform, rootPrefab.transform) >= 2)
						{
							skipThisPrefabInstance = false;
						}
						else
						{
							PropertyModification[] modifications = PrefabUtility.GetPropertyModifications(go);
							foreach (PropertyModification modification in modifications)
							{
								Object target = modification.target;

								if (target is Transform) continue;

								skipThisPrefabInstance = false;
								break;
							}
						}
						if (skipThisPrefabInstance) return;
					}
				}
			}

			if (MaintainerSettings.Issues.undefinedTags)
			{
				bool undefinedTag = false;
				try
				{
					if (string.IsNullOrEmpty(go.tag))
					{
						undefinedTag = true;
					}
				}
				catch (UnityException e)
				{
					if (e.Message.Contains("undefined tag"))
					{
						undefinedTag = true;
					}
					else
					{
						Debug.LogError(Maintainer.LOG_PREFIX + "Unknown error while checking tag of the " + go.name + "\n" + e);
					}
				}

				if (undefinedTag)
				{
					issues.Add(GameObjectIssueRecord.Create(RecordType.UndefinedTag, location, path, go));
				}
			}

			if (MaintainerSettings.Issues.unnamedLayers)
			{
				int layerIndex = go.layer;
				if (string.IsNullOrEmpty(LayerMask.LayerToName(layerIndex)))
				{
					GameObjectIssueRecord issue = GameObjectIssueRecord.Create(RecordType.UnnamedLayer, location, path, go);
					issue.headerExtra = "(index: " + layerIndex + ")";
					issues.Add(issue);
				}
			}

			Dictionary<Type, int> uniqueTypes = null;
			List<int> similarComponentsIndexes = null;

			TerrainData terrainTerrainData = null;
			TerrainData terrainColliderTerrainData = null;
			bool terrainChecked = false;
			bool terrainColliderChecked = false;

			// ----------------------------------------------------------------------------
			// looking for component-level issues
			// ----------------------------------------------------------------------------

			for (int i = 0; i < componentsCount; i++)
			{
				Component component = components[i];
				Type componentType = componentsTypes[i];
				string componentName = componentsNames[i];
				//string componentFullName = componentsFullNames[i];
				string componentNamespace = componentsNamespaces[i];

				if (component is Transform)
				{
					if (MaintainerSettings.Issues.hugePositions)
					{
						Vector3 position = (component as Transform).position;

						if (Math.Abs(position.x) > 100000f || Math.Abs(position.y) > 100000f || Math.Abs(position.z) > 100000f)
						{
							issues.Add(GameObjectIssueRecord.Create(RecordType.HugePosition, location, path, go, component, componentType, componentName, "Position"));
						}
					}
					continue;
				}

				if (MaintainerSettings.Issues.duplicateComponents &&
					(componentNamespace != "Fabric"))
				{
					// initializing dictionary and list on first usage
					if (uniqueTypes == null) uniqueTypes = new Dictionary<Type, int>(componentsCount);
					if (similarComponentsIndexes == null) similarComponentsIndexes = new List<int>(componentsCount);

					// checking if current component type already met before
					if (uniqueTypes.ContainsKey(componentType))
					{
						int uniqueTypeIndex = uniqueTypes[componentType];

						// checking if initially met component index already in indexes list
						// since we need to compare all duplicate candidates against initial component
						if (!similarComponentsIndexes.Contains(uniqueTypeIndex)) similarComponentsIndexes.Add(uniqueTypeIndex);

						// adding current component index to the indexes list
						similarComponentsIndexes.Add(i);
					}
					else
					{
						uniqueTypes.Add(componentType, i);
					}
				}
	
				if (component is MeshCollider)
				{
					if (MaintainerSettings.Issues.emptyMeshColliders)
					{
						if ((component as MeshCollider).sharedMesh == null)
						{
							issues.Add(GameObjectIssueRecord.Create(RecordType.EmptyMeshCollider, location, path, go, component, componentType, componentName));
						}
					}
				}
				else if (component is MeshFilter)
				{
					if (MaintainerSettings.Issues.emptyMeshFilters && !skipEmptyMeshFilter)
					{
						if ((component as MeshFilter).sharedMesh == null)
						{
							issues.Add(GameObjectIssueRecord.Create(RecordType.EmptyMeshFilter, location, path, go, component, componentType, componentName));
						}
					}
				}
				else if (component is Renderer)
				{
					if (MaintainerSettings.Issues.emptyRenderers)
					{
						if ((component as Renderer).sharedMaterial == null)
						{
							issues.Add(GameObjectIssueRecord.Create(RecordType.EmptyRenderer, location, path, go, component, componentType, componentName));
						}
					}

					if (component is SpriteRenderer)
					{
						if (MaintainerSettings.Issues.emptySpriteRenderers)
						{
							if ((component as SpriteRenderer).sprite == null)
							{
								issues.Add(GameObjectIssueRecord.Create(RecordType.EmptySpriteRenderer, location, path, go, component, componentType, componentName));
							}
						}
					}
				}
				else if (component is Animation)
				{
					if (MaintainerSettings.Issues.emptyAnimations)
					{
						if ((component as Animation).GetClipCount() <= 0 && (component as Animation).clip == null)
						{
							issues.Add(GameObjectIssueRecord.Create(RecordType.EmptyAnimation, location, path, go, component, componentType, componentName));
						}
					}
				}
				else if (component is Terrain)
				{
					if (MaintainerSettings.Issues.inconsistentTerrainData)
					{
						terrainTerrainData = (component as Terrain).terrainData;
						terrainChecked = true;
					}
				}
				else if (component is TerrainCollider)
				{
					if (MaintainerSettings.Issues.inconsistentTerrainData)
					{
						terrainColliderTerrainData = (component as TerrainCollider).terrainData;
						terrainColliderChecked = true;
					}

					if (MaintainerSettings.Issues.emptyTerrainCollider)
					{
						if ((component as TerrainCollider).terrainData == null)
						{
							issues.Add(GameObjectIssueRecord.Create(RecordType.EmptyTerrainCollider, location, path, go, component, componentType, componentName));
						}
					}
				}
				else if (component is AudioSource)
				{
					if (MaintainerSettings.Issues.emptyAudioSource && !skipEmptyAudioSource)
					{
						if ((component as AudioSource).clip == null)
						{
							issues.Add(GameObjectIssueRecord.Create(RecordType.EmptyAudioSource, location, path, go, component, componentType, componentName));
						}
					}
				}

				// ----------------------------------------------------------------------------
				// looping through the component's SerializedProperties via SerializedObject
				// ----------------------------------------------------------------------------

				Dictionary<string, int> emptyArrayItems = new Dictionary<string, int>();
				SerializedObject so = new SerializedObject(component);
				SerializedProperty sp = so.GetIterator();

				while (sp.NextVisible(true))
				{
					string fullPropertyPath = sp.propertyPath;

					bool isArrayItem = fullPropertyPath.EndsWith("]", StringComparison.Ordinal);

					if (MaintainerSettings.Issues.missingReferences)
					{
						if (sp.propertyType == SerializedPropertyType.ObjectReference)
						{
							if (sp.objectReferenceValue == null && sp.objectReferenceInstanceIDValue != 0)
							{
								string propertyName = isArrayItem ? GetArrayItemNameAndIndex(fullPropertyPath) : sp.name;
								issues.Add(GameObjectIssueRecord.Create(RecordType.MissingReference, location, path, go, component, componentType, componentName, propertyName));
							}
						}
					}

					if (checkingScene || !MaintainerSettings.Issues.skipEmptyArrayItemsOnPrefabs)
					{
						// skipping SpriteRenderer as it has hidden array with materials of size 1
						if (MaintainerSettings.Issues.emptyArrayItems && isArrayItem)
						{
							// ignoring components where empty array items is a normal behavior
							if (!(component is SpriteRenderer) && !componentName.StartsWith("TextMeshPro"))
							{
								if (sp.propertyType == SerializedPropertyType.ObjectReference &&
								    sp.objectReferenceValue == null &&
								    sp.objectReferenceInstanceIDValue == 0)
								{
									string arrayName = GetArrayItemName(fullPropertyPath);

									// ignoring TextMeshPro's FontAssetArrays with 16 empty items inside
									if (!emptyArrayItems.ContainsKey(arrayName))
									{
										emptyArrayItems.Add(arrayName, 0);
									}
									emptyArrayItems[arrayName]++;
								}
							}
						}
					}
					/*else
					{
						continue;
					}*/
				}

				if (MaintainerSettings.Issues.emptyArrayItems)
				{
					foreach (var item in emptyArrayItems.Keys)
					{
						GameObjectIssueRecord issueRecord = GameObjectIssueRecord.Create(RecordType.EmptyArrayItem, location, path, go, component, componentType, componentName, item);
						issueRecord.headerFormatArgument = emptyArrayItems[item].ToString();
						issues.Add(issueRecord);
					}
				}
			}

			if (MaintainerSettings.Issues.inconsistentTerrainData && 
				terrainColliderTerrainData != terrainTerrainData &&
				terrainChecked && terrainColliderChecked)
			{
				issues.Add(GameObjectIssueRecord.Create(RecordType.InconsistentTerrainData, location, path, go));
			}

			if (MaintainerSettings.Issues.duplicateComponents)
			{
				if (similarComponentsIndexes != null && similarComponentsIndexes.Count > 0)
				{
					int similarComponentsCount = similarComponentsIndexes.Count;
					List<long> similarComponentsHashes = new List<long>(similarComponentsCount);

					for (int i = 0; i < similarComponentsCount; i++)
					{
						int componentIndex = similarComponentsIndexes[i];
						Component component = components[componentIndex];

						long componentHash = 0;

						if (MaintainerSettings.Issues.duplicateComponentsPrecise)
						{
							SerializedObject so = new SerializedObject(component);
							SerializedProperty sp = so.GetIterator();
							while (sp.NextVisible(true))
							{
								componentHash += CSEditorTools.GetPropertyHash(sp);
							}
						}

						similarComponentsHashes.Add(componentHash);
					}

					List<long> distinctItems = new List<long>(similarComponentsCount);

					for (int i = 0; i < similarComponentsCount; i++)
					{
						int componentIndex = similarComponentsIndexes[i];

						if (distinctItems.Contains(similarComponentsHashes[i]))
						{
							issues.Add(GameObjectIssueRecord.Create(RecordType.DuplicateComponent, location, path, go, components[componentIndex], componentsTypes[componentIndex], componentsNames[componentIndex]));
						}
						else
						{
							distinctItems.Add(similarComponentsHashes[i]);
						}
					}
				}
			}
		}

		private static bool ProcessSettings(List<IssueRecord> issues)
		{
			bool result = true;
			currentPhase++;

			if (MaintainerSettings.Issues.duplicateScenesInBuild)
			{
				if (EditorUtility.DisplayCancelableProgressBar(string.Format(PROGRESS_CAPTION, currentPhase, phasesCount, 1, 1), "Checking settings: Build Settings", (float)0/1))
				{
					result = false;
				}
			}

			CheckBuildSettings(issues);

			if (MaintainerSettings.Issues.duplicateTagsAndLayers)
			{
				if (EditorUtility.DisplayCancelableProgressBar(string.Format(PROGRESS_CAPTION, currentPhase, phasesCount, 1, 1), "Checking settings: Tags and Layers", (float)0/1))
				{
					result = false;
				}
			}

			CheckTagsAndLayers(issues);

			return result;
		}

		private static void CheckBuildSettings(List<IssueRecord> issues)
		{
			if (MaintainerSettings.Issues.duplicateScenesInBuild)
			{
				string[] scenesForBuild = GetEnabledScenesInBuild();
				string[] duplicates = CSArrayTools.FindDuplicatesInArray(scenesForBuild);

				foreach (var duplicate in duplicates)
				{
					issues.Add(BuildSettingsIssueRecord.Create(RecordType.DuplicateScenesInBuild, 
						"<b>Duplicate scene:</b> " + CSEditorTools.NicifyAssetPath(duplicate)));
				}
			}
		}
		 
		private static void CheckTagsAndLayers(List<IssueRecord> issues)
		{
			if (MaintainerSettings.Issues.duplicateTagsAndLayers)
			{
				StringBuilder issueBody = new StringBuilder();

				/* looking for duplicates in tags*/

				List<string> tags = new List<string>(InternalEditorUtility.tags);
				tags.RemoveAll(string.IsNullOrEmpty);
				List<string> duplicateTags = CSArrayTools.FindDuplicatesInArray(tags);

				if (duplicateTags.Count > 0)
				{
					issueBody.Append("Duplicate <b>tag(s)</b>: ");

					foreach (var duplicate in duplicateTags)
					{
						issueBody.Append('"').Append(duplicate).Append("\", ");
					}
					issueBody.Length -= 2;
				}

				/* looking for duplicates in layers*/

				List<string> layers = new List<string>(InternalEditorUtility.layers);
				layers.RemoveAll(string.IsNullOrEmpty);
				List<string> duplicateLayers = CSArrayTools.FindDuplicatesInArray(layers);

				if (duplicateLayers.Count > 0)
				{
					if (issueBody.Length > 0) issueBody.AppendLine();
					issueBody.Append("Duplicate <b>layer(s)</b>: ");

					foreach (var duplicate in duplicateLayers)
					{
						issueBody.Append('"').Append(duplicate).Append("\", ");
					}
					issueBody.Length -= 2;
				}

				/* looking for duplicates in sorting layers*/

				Type internalEditorUtilityType = typeof(InternalEditorUtility);
				PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);

				List<string> sortingLayers = new List<string>((string[])sortingLayersProperty.GetValue(null, new object[0]));
				sortingLayers.RemoveAll(string.IsNullOrEmpty);
				List<string> duplicateSortingLayers = CSArrayTools.FindDuplicatesInArray(sortingLayers);

				if (duplicateSortingLayers.Count > 0)
				{
					if (issueBody.Length > 0) issueBody.AppendLine();
					issueBody.Append("Duplicate <b>sorting layer(s)</b>: ");

					foreach (var duplicate in duplicateSortingLayers)
					{
						issueBody.Append('"').Append(duplicate).Append("\", ");
					}
					issueBody.Length -= 2;
				}

				if (issueBody.Length > 0)
				{
					issues.Add(TagsAndLayersIssueRecord.Create(RecordType.DuplicateTagsAndLayers, issueBody.ToString()));
				}

				issueBody.Length = 0;
			}
		}

		private static void FinishSearch()
		{
			if (MaintainerSettings.Issues.lookInScenes)
			{
				if (string.IsNullOrEmpty(searchStartScene))
				{
					if (MaintainerSettings.Issues.scenesSelection != IssuesFinderSettings.ScenesSelection.CurrentSceneOnly)
						CSSceneTools.NewScene();
				}
				else if (CSSceneTools.GetCurrentScenePath() != searchStartScene)
				{
					EditorUtility.DisplayProgressBar("Opening initial scene", "Opening scene: " + Path.GetFileNameWithoutExtension(searchStartScene), 0);
					CSSceneTools.OpenScene(searchStartScene);
				}
			}
			EditorUtility.ClearProgressBar();
		}

		private static string GetArrayItemNameAndIndex(string fullPropertyPath)
		{
			string propertyPath = fullPropertyPath.Replace(".Array.data", "").Replace("].", "] / ");
			return propertyPath;
		}

		private static string GetArrayItemName(string fullPropertyPath)
		{
			string name = GetArrayItemNameAndIndex(fullPropertyPath);
			int lastOpeningBracketIndex = name.LastIndexOf('[');
			return name.Substring(0, lastOpeningBracketIndex);
		}

		private static string[] GetEnabledScenesInBuild()
		{
			EditorBuildSettingsScene[] scenesForBuild = EditorBuildSettings.scenes;
			List<string> scenesInBuild = new List<string>(scenesForBuild.Length);

			foreach (EditorBuildSettingsScene sceneInBuild in scenesForBuild)
			{
				if (sceneInBuild.enabled)
				{
					scenesInBuild.Add(sceneInBuild.path);
				}
			}
			return scenesInBuild.ToArray();
		}
	}
}

#endif