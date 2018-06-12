#define UNITY_5_PLUS
#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9
#undef UNITY_5_PLUS
#endif

#if UNITY_EDITOR

using UnityEngine;
using System.Reflection;
using UnityEditor;

namespace CodeStage.Maintainer.Tools
{
	public class CSObjectTools
	{
		private static PropertyInfo cachedInspectorModeInfo;

		internal static long GetLocalIdentifierInFileForObject(Object unityObject)
		{
			long id = -1;

			if (unityObject == null) return id;

			if (cachedInspectorModeInfo == null)
			{
				cachedInspectorModeInfo = typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
			}

			SerializedObject serializedObject = new SerializedObject(unityObject);
			cachedInspectorModeInfo.SetValue(serializedObject, InspectorMode.Debug, null);
			SerializedProperty serializedProperty = serializedObject.FindProperty("m_LocalIdentfierInFile");
#if UNITY_5_PLUS
			id = serializedProperty.longValue;
#else
			id = serializedProperty.intValue;
#endif
			if (id <= 0)
			{
				PrefabType prefabType = PrefabUtility.GetPrefabType(unityObject);
				if (prefabType != PrefabType.None)
				{
					id = GetLocalIdentifierInFileForObject(PrefabUtility.GetPrefabObject(unityObject));
				}
				else
				{
					// this will work for the new objects in scene which weren't saved yet
					id = unityObject.GetInstanceID();
				}
			}

			return id;
		}
	}
}

#endif