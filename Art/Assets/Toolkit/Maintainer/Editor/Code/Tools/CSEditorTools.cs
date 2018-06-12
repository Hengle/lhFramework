#if UNITY_EDITOR

using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CodeStage.Maintainer.Tools
{
	public class CSEditorTools
	{
		private static Object objectForDelayedPing;
		private static float pingDelayStartTime;

		public static int GetPropertyHash(SerializedProperty sp)
		{
			/*Debug.Log("Property: " + sp.name);
			Debug.Log("sp.propertyType = " + sp.propertyType);*/
			StringBuilder stringHash = new StringBuilder();

			stringHash.Append(sp.type);

			if (sp.isArray)
			{
				stringHash.Append(sp.arraySize);
			}
			else
				switch (sp.propertyType)
				{
					case SerializedPropertyType.AnimationCurve:
						if (sp.animationCurveValue != null)
						{
							stringHash.Append(sp.animationCurveValue.length);
							if (sp.animationCurveValue.keys != null)
							{
								foreach (Keyframe key in sp.animationCurveValue.keys)
								{
									stringHash.Append(key.value)
											  .Append(key.time)
											  .Append(key.tangentMode)
											  .Append(key.outTangent)
											  .Append(key.inTangent);
								}
							}
						}
						
						break;
					case SerializedPropertyType.ArraySize:
						stringHash.Append(sp.intValue);
						break;
					case SerializedPropertyType.Boolean:
						stringHash.Append(sp.boolValue);
						break;
					case SerializedPropertyType.Bounds:
						stringHash.Append(sp.boundsValue.center)
								  .Append(sp.boundsValue.extents);
						break;
					case SerializedPropertyType.Character:
						stringHash.Append(sp.intValue);
						break;
					case SerializedPropertyType.Generic: // looks like arrays which we already walk through
						break;
					case SerializedPropertyType.Gradient: // unsupported
						break;
					case SerializedPropertyType.ObjectReference:
						if (sp.objectReferenceValue != null)
						{
							stringHash.Append(sp.objectReferenceValue.name);
						}
						break;
					case SerializedPropertyType.Color:
						stringHash.Append(sp.colorValue);
						break;
					case SerializedPropertyType.Enum:
						stringHash.Append(sp.enumValueIndex);
						break;
					case SerializedPropertyType.Float:
						stringHash.Append(sp.floatValue);
						break;
					case SerializedPropertyType.Integer:
						stringHash.Append(sp.intValue);
						break;
					case SerializedPropertyType.LayerMask:
						stringHash.Append(sp.intValue);
						break;
					case SerializedPropertyType.Quaternion:
						stringHash.Append(sp.quaternionValue);
						break;
					case SerializedPropertyType.Rect:
						stringHash.Append(sp.rectValue);
						break;
					case SerializedPropertyType.String:
						stringHash.Append(sp.stringValue);
						break;
					case SerializedPropertyType.Vector2:
						stringHash.Append(sp.vector2Value);
						break;
					case SerializedPropertyType.Vector3:
						stringHash.Append(sp.vector3Value);
						break;
					case SerializedPropertyType.Vector4:
						stringHash.Append(sp.vector4Value);
						break;
					default:
						Debug.LogWarning(Maintainer.LOG_PREFIX + "Unknown SerializedPropertyType: " + sp.propertyType);
						break;
				}

			return stringHash.ToString().GetHashCode();
		}

		public static string GetFullTransformPath(Transform transform)
		{
			string path = transform.name;
			while (transform.parent != null)
			{
				transform = transform.parent;
				path = transform.name + "/" + path;
			}
			return path;
		}

		public static GameObject[] GetAllSuitableGameObjectsInCurrentScene()
		{
			GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
			List<GameObject> result = new List<GameObject>(allObjects);
			result.RemoveAll(o => !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(o)) || o.hideFlags != HideFlags.None);
			return result.ToArray();
		}

		public static int GetAllSuitableGameObjectsInPrefabAssets(List<GameObject> gameObjects)
		{
            return GetAllSuitableGameObjectsInPrefabAssets(gameObjects, null);
		}

		public static int GetAllSuitableGameObjectsInPrefabAssets(List<GameObject> gameObjects, List<string> paths)
		{
			string[] allAssetPaths = FindAssetsFiltered("t:Prefab", null);
			return GetSuitablePrefabsFromSelection(allAssetPaths, gameObjects, paths);
		}

		public static int GetSuitablePrefabsFromSelection(string[] selection, List<GameObject> gameObjects, List<string> paths)
		{
			int selectedCount = 0;

			foreach (string path in selection)
			{
				GameObject go = (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));

				if (go == null) continue;

				selectedCount = GetPrefabsRecursive(gameObjects, paths, path, go, selectedCount);
			}

			return selectedCount;
		}

		private static int GetPrefabsRecursive(List<GameObject> gameObjects, List<string> paths, string path, GameObject go, int selectedCount)
		{
			if (go.hideFlags == HideFlags.None || go.hideFlags == HideFlags.HideInHierarchy)
			{
				gameObjects.Add(go);
				if (paths != null) paths.Add(path);
				selectedCount++;
			}

			int childCount = go.transform.childCount;

			for (int i = 0; i < childCount; i++)
			{
				GameObject nestedObject = go.transform.GetChild(i).gameObject;
				selectedCount = GetPrefabsRecursive(gameObjects, paths, path, nestedObject, selectedCount);
			}

			return selectedCount;
		}

		public static string[] FindAssetsFiltered(string filter, string[] ignores)
		{
			string[] allAssetsGUIDs = AssetDatabase.FindAssets(filter);
			int count = allAssetsGUIDs.Length;

			List<string> paths = new List<string>(count);

			for (int i = 0; i < count; i++)
			{
				string path = AssetDatabase.GUIDToAssetPath(allAssetsGUIDs[i]);
				bool skip = false;

				if (ignores != null)
				{
					skip = CSArrayTools.IsItemContainsAnyStringFromArray(path, ignores);
				}
				
				if (!skip) paths.Add(path);
			}

			return paths.ToArray();
		}

		public static int GetDepthInHierarchy(Transform transform, Transform upToTransform)
		{
			if (transform == upToTransform || transform.parent == null) return 0;
			return 1 + GetDepthInHierarchy(transform.parent, upToTransform);
		}

		public static string NicifyAssetPath(string path)
		{
			string nicePath = path.Remove(0, 7);

			int lastSlash = nicePath.LastIndexOf('/');
			int lastDot = nicePath.LastIndexOf('.');

			// making sure we'll not trim path like Test/My.Test/linux_file
			if (lastDot > lastSlash)
			{
				nicePath = nicePath.Remove(lastDot, nicePath.Length - lastDot);
			}

			return nicePath;
		}

		public static void PingObjectDelayed(Object objectToPing)
		{
			objectForDelayedPing = objectToPing;
			pingDelayStartTime = Time.realtimeSinceStartup;
			EditorApplication.update += OnEditorPingUpdate;
		}

		private static void OnEditorPingUpdate()
		{
			if (Time.realtimeSinceStartup - pingDelayStartTime > 0.01f)
			{
				pingDelayStartTime = 0;

				// ReSharper disable once DelegateSubtraction
                EditorApplication.update -= OnEditorPingUpdate;

				if (objectForDelayedPing != null)
				{
					EditorGUIUtility.PingObject(objectForDelayedPing);
					objectForDelayedPing = null;
				}
			}
		}
	}
}

#endif