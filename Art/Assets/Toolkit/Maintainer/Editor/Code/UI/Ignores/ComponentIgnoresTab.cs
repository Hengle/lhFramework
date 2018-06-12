#if UNITY_EDITOR

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CodeStage.Maintainer.UI.Ignores
{
	internal class ComponentIgnoresTab : StringCollectionTab
	{
		internal ComponentIgnoresTab(string[] ignoresList, SaveCollectionCallback saveCallback) : base(ignoresList, saveCallback)
		{
			name = "Component Ignores";
		}

		internal override void DrawTabHeader()
		{
			EditorGUILayout.LabelField("Here you may specify names of Components you'd like to ignore during the Issues Search.\n"+
									   "You may drag & drop components to this window from the Inspector.", EditorStyles.wordWrappedLabel);
			EditorGUILayout.LabelField("Examples:\n" +
									   "MeshFilter\n" +
									   "DOTweenAnimation",
									   EditorStyles.wordWrappedMiniLabel);
		}

		internal override void CheckNewItem()
		{
			bool found = false;

			foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (Type t in a.GetTypes())
				{
					if (t.Name == newItemText)
					{
						found = true;
						break;
					}
				}

				if (found) break;
			}

			if (!found)
			{
				EditorUtility.DisplayDialog("Can't find specified Component", "Specified component " + newItemText + " wasn't found in usual places. Make sure you've entered valid name.", "Cool, thanks!");
			}
		}

		internal override void ProcessDrags()
		{
			if (currentEventType == EventType.DragUpdated || currentEventType == EventType.DragPerform)
			{
				Object[] objects = DragAndDrop.objectReferences;

				if (objects != null && objects.Length > 0)
				{
					bool canDrop = false;

					for (int i = 0; i < objects.Length; i++)
					{
						if (objects[i] is Component)
						{
							canDrop = true;
							break;
						}
					}

					if (canDrop)
					{
						DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

						if (currentEventType == EventType.DragPerform)
						{
							bool needToSave = false;
							bool needToShowWarning = false;
							bool noComponent = false;
							bool invalidComponent = false;

							for (int i = 0; i < objects.Length; i++)
							{
								Component component = objects[i] as Component;

								if (component != null)
								{
									string componentName = component.GetType().Name;
									if (componentName != "Object" && componentName != "Component" && componentName != "Behaviour")
									{
										bool added = TryAddNewItemToIgnores(componentName);
										needToSave |= added;
										needToShowWarning |= !added;
									}
									else
									{
										invalidComponent = true;
									}
								}
								else
								{
									noComponent = true;
								}
							}

							if (needToSave)
							{
								SaveChanges();
							}

							string warningText = "";

							if (needToShowWarning)
							{
								warningText = "One or more of the dragged items already present in the list!";
							}

							if (noComponent)
							{
								if (!string.IsNullOrEmpty(warningText))
								{
									warningText += "\n";
								}
								warningText += "One or more of the dragged items are not the Components!";
							}

							if (invalidComponent)
							{
								if (!string.IsNullOrEmpty(warningText))
								{
									warningText += "\n";
								}
								warningText += "Can't detect valid name for one or more of the dragged items!";
							}

							if (!string.IsNullOrEmpty(warningText)) window.ShowNotification(new GUIContent(warningText));

							DragAndDrop.AcceptDrag();
						}
					}
				}
				Event.current.Use();
			}
		}
	}
}

#endif