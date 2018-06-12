#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace CodeStage.Maintainer.UI.Ignores
{
	internal class PathIgnoresTab : StringCollectionTab
	{
		internal PathIgnoresTab(string[] ignoresList, SaveCollectionCallback saveCallback) : base(ignoresList, saveCallback)
		{
			name = "Path Ignores";
		}

		internal override void DrawTabHeader()
		{
			EditorGUILayout.LabelField("Here you may specify full or partial paths to ignore. Ignores are case-sensitive.\n" +
										"You may drag & drop files and folders to this window directly from the Project View.",
										EditorStyles.wordWrappedLabel);
			EditorGUILayout.LabelField("Examples:\n" +
									   "folder name/ - excludes all assets having such folder in the path\n" +
									   ".unity - excludes all scenes from the search\n" +
									   "SomeFile.ext - excludes specified file from any folder", EditorStyles.wordWrappedMiniLabel);
		}

		internal override void CheckNewItem()
		{
			newItemText = newItemText.Replace('\\', '/');
		}

		internal override void ProcessDrags()
		{
			if (currentEventType != EventType.DragUpdated && currentEventType != EventType.DragPerform) return;

			string[] paths = DragAndDrop.paths;

			if (paths != null && paths.Length > 0)
			{
				DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

				if (currentEventType == EventType.DragPerform)
				{
					bool needToSave = false;
					bool needToShowWarning = false;

					foreach (string path in paths)
					{
						bool added = TryAddNewItemToIgnores(path);
						needToSave |= added;
						needToShowWarning |= !added;
					}

					if (needToSave)
					{
						SaveChanges();
					}

					if (needToShowWarning)
					{
						window.ShowNotification(new GUIContent("One or more of the dragged items already present in the list!"));
					}

					DragAndDrop.AcceptDrag();
				}
			}
			Event.current.Use();
		}
	}
}

#endif