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
using System.Globalization;
using System.Text;
using CodeStage.Maintainer.Tools;
using CodeStage.Maintainer.UI;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CodeStage.Maintainer.Issues
{
	[Serializable]
	public class GameObjectIssueRecord : IssueRecord, IShowableRecord
	{
		public string path;
		public string gameObjectPath;
		public long objectId;
		public string component;
		public string property;

		public void Show()
		{
			GameObject[] allObjects;

			if (location == RecordLocation.Scene)
			{
				if (CSSceneTools.GetCurrentScenePath() != path)
				{
					if (!CSSceneTools.SaveCurrentSceneIfUserWantsTo())
					{
						return;
					}
					CSSceneTools.OpenScene(path);
				}

				allObjects = CSEditorTools.GetAllSuitableGameObjectsInCurrentScene();
				CSEditorTools.PingObjectDelayed(AssetDatabase.LoadAssetAtPath(path, typeof(Object)));
			}
			else
			{
				List<GameObject> prefabs = new List<GameObject>();
				CSEditorTools.GetAllSuitableGameObjectsInPrefabAssets(prefabs);
				allObjects = prefabs.ToArray();
			}

			GameObject go = FindObjectInCollection(allObjects);
			if (go != null)
			{
				if (location == RecordLocation.Scene)
				{
					Selection.activeTransform = go.transform;
				}
				else
				{
					Selection.activeGameObject = go;

					if (gameObjectPath.Split('/').Length > 2)
					{
						CSEditorTools.PingObjectDelayed(AssetDatabase.LoadAssetAtPath(path, typeof(Object)));
					}
				}
			}
			else
			{
				MaintainerWindow.ShowNotification("Can't find object " + gameObjectPath);
			}
		}

		internal static GameObjectIssueRecord Create(RecordType type, RecordLocation location, string path, GameObject gameObject)
		{
			return new GameObjectIssueRecord(type, location, path, gameObject);
		}

		internal static GameObjectIssueRecord Create(RecordType type, RecordLocation location, string path, GameObject gameObject, Component component, Type componentType, string componentName)
		{
			return new GameObjectIssueRecord(type, location, path, gameObject, component, componentType, componentName);
		}

		internal static GameObjectIssueRecord Create(RecordType type, RecordLocation location, string path, GameObject gameObject, Component component, Type componentType, string componentName, string property)
		{
			return new GameObjectIssueRecord(type, location, path, gameObject, component, componentType, componentName, property);
		}

		protected GameObjectIssueRecord(RecordType type, RecordLocation location, string path, GameObject gameObject):base(type,location)
		{
			this.path = path;
			gameObjectPath = CSEditorTools.GetFullTransformPath(gameObject.transform);
			objectId = CSObjectTools.GetLocalIdentifierInFileForObject(gameObject);

#if UNITY_5_3_PLUS
			if (location == RecordLocation.Scene)
			{
				this.path = gameObject.scene.path;
			}
#endif
		}

		protected GameObjectIssueRecord(RecordType type, RecordLocation location, string path, GameObject gameObject, Component component, Type componentType, string componentName) : this(type, location, path, gameObject)
		{
			if (component == null) return;

			this.component = componentName;
			if (gameObject.GetComponents(componentType).Length > 1)
			{
				long id = CSObjectTools.GetLocalIdentifierInFileForObject(component);
				if (id != 0)
				{
					this.component += " (ID: " + id + ")";
				}
			}
		}

		protected GameObjectIssueRecord(RecordType type, RecordLocation location, string path, GameObject gameObject, Component component, Type componentType, string componentName, string property):this(type, location, path, gameObject, component, componentType, componentName)
		{
			if (!string.IsNullOrEmpty(property))
			{
				string nicePropertyName = ObjectNames.NicifyVariableName(property);
				TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
				this.property = textInfo.ToTitleCase(nicePropertyName);
			}
		}

		protected override void ConstructBody(StringBuilder text)
		{
			text.Append(location == RecordLocation.Scene ? "<b>Scene:</b> " : "<b>Prefab:</b> ");

			string nicePath = path == "" ? "Untitled (current scene)" : CSEditorTools.NicifyAssetPath(path);

			text.Append(nicePath);

			if (!string.IsNullOrEmpty(gameObjectPath)) text.Append("\n<b>Game Object:</b> ").Append(gameObjectPath);
			if (!string.IsNullOrEmpty(component)) text.Append("\n<b>Component:</b> ").Append(component);
			if (!string.IsNullOrEmpty(property)) text.Append("\n<b>Property:</b> ").Append(property);
		}

		private GameObject FindObjectInCollection(IEnumerable<GameObject> allObjects)
		{
			GameObject candidate = null;

			foreach (GameObject gameObject in allObjects)
			{
				if (CSEditorTools.GetFullTransformPath(gameObject.transform) != gameObjectPath) continue;

				candidate = gameObject;
				if (objectId == CSObjectTools.GetLocalIdentifierInFileForObject(candidate))
				{
					break;
				}
			}
			return candidate;
		}
	}
}

#endif