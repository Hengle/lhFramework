#if UNITY_EDITOR

using System;

namespace CodeStage.Maintainer.Settings
{
	[Serializable]
	public class IssuesFinderSettings
	{
		[Serializable]
		public enum ScenesSelection
		{
			AllScenes,
			BuildScenesOnly,
			CurrentSceneOnly
		}

		// ----------------------------------------------------------------------------
		// filtering
		// ----------------------------------------------------------------------------

		public string[] pathIgnores = new string[0];
		public string[] componentIgnores = new string[0];

		public int ignoresTabIndex = 0;

		public bool lookInScenes;

		public ScenesSelection scenesSelection;
		public bool lookInAssets;
		public bool touchInactiveGameObjects;
		public bool touchDisabledComponents;

		public bool gameObjectsFoldout;
		public bool commonFoldout;
		public bool prefabsFoldout;
		public bool unusedFoldout;
		public bool neatnessFoldout;
		public bool projectSettingsFoldout;

		// ----------------------------------------------------------------------------
		// search settings
		// ----------------------------------------------------------------------------

		public bool scanGameObjects;
		public bool scanProjectSettings;

		/* common  */

		public bool missingComponents;
		public bool duplicateComponents;
		public bool duplicateComponentsPrecise;
		public bool missingReferences;
		public bool undefinedTags;
		public bool inconsistentTerrainData;

		/* prefabs specific */

		public bool missingPrefabs;
		public bool disconnectedPrefabs;

		/* unused components */

		public bool emptyMeshColliders;
		public bool emptyMeshFilters;
		public bool emptyAnimations;
		public bool emptyRenderers;
		public bool emptySpriteRenderers;
		public bool emptyTerrainCollider;
		public bool emptyAudioSource;

		/* neatness */

		public bool emptyArrayItems;
		public bool skipEmptyArrayItemsOnPrefabs;
		public bool unnamedLayers;
		public bool hugePositions;

		/* project settings */

		public bool duplicateScenesInBuild;
		public bool duplicateTagsAndLayers;

		public IssuesFinderSettings()
		{
            Reset();
		}

		internal void SwitchAll(bool enable)
		{
			scanGameObjects = enable;
			scanProjectSettings = enable;

			SwitchCommon(enable);
			SwitchPrefabsSpecific(enable);
			SwitchUnusedComponents(enable);
			SwitchNeatness(enable);
			SwitchProjectSettings(enable);
		}

		internal void SwitchCommon(bool enable)
		{
			missingComponents = enable;
			duplicateComponents = enable;
			missingReferences = enable;
			undefinedTags = enable;
			inconsistentTerrainData = enable;
		}

		internal void SwitchPrefabsSpecific(bool enable)
		{
			missingPrefabs = enable;
			disconnectedPrefabs = enable;
		}

		internal void SwitchUnusedComponents(bool enable)
		{
			emptyMeshColliders = enable;
			emptyMeshFilters = enable;
			emptyAnimations = enable;
			emptyRenderers = enable;
			emptySpriteRenderers = enable;
			emptyTerrainCollider = enable;
			emptyAudioSource = enable;
		}

		internal void SwitchNeatness(bool enable)
		{
			emptyArrayItems = enable;
			unnamedLayers = enable;
			hugePositions = enable;
		}

		internal void SwitchProjectSettings(bool enable)
		{
			duplicateScenesInBuild = enable;
			duplicateTagsAndLayers = enable;
		}

		internal void Reset()
		{
			scanGameObjects = true;
			gameObjectsFoldout = true;
			commonFoldout = false;
			prefabsFoldout = false;
			unusedFoldout = false;
			neatnessFoldout = false;
			scanProjectSettings = true;
			projectSettingsFoldout = true;
			lookInScenes = true;
			scenesSelection = ScenesSelection.AllScenes;
			lookInAssets = true;
			touchInactiveGameObjects = true;
			touchDisabledComponents = true;
			missingComponents = true;
			duplicateComponents = true;
			duplicateComponentsPrecise = true;
			missingReferences = true;
			undefinedTags = true;
			inconsistentTerrainData = true;
			emptyArrayItems = true;
			skipEmptyArrayItemsOnPrefabs = true;
			missingPrefabs = true;
			disconnectedPrefabs = true;
			emptyMeshColliders = true;
			emptyMeshFilters = true;
			emptyAnimations = true;
			emptyRenderers = true;
			emptySpriteRenderers = true;
			emptyTerrainCollider = true;
			emptyAudioSource = true;
			unnamedLayers = true;
			hugePositions = true;
			duplicateScenesInBuild = true;
			duplicateTagsAndLayers = true;
		}
	}
}

#endif