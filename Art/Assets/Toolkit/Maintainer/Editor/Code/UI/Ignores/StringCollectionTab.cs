#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace CodeStage.Maintainer.UI.Ignores
{
	internal abstract class StringCollectionTab : TabBase
	{
		internal delegate void SaveCollectionCallback(string[] collection);

		protected string[] ignores;
		protected event SaveCollectionCallback SaveChangesCallback;

		protected bool didFocus;
		protected string newItemText = "";

		protected StringCollectionTab(string[] ignoresList, SaveCollectionCallback saveCallback)
		{
			ignores = ignoresList;
			SaveChangesCallback = saveCallback;
		}

		internal override void Show(IgnoresWindow hostingWindow)
		{
			base.Show(hostingWindow);

			newItemText = "";
			didFocus = false;
		}

		internal override void DrawTabContents()
		{
			DrawTabHeader();

			GUILayout.Space(5);
			UIHelpers.Separator();
			GUILayout.Space(5);

			DrawAddItemSection();

			GUILayout.Space(5);
			UIHelpers.Separator();
			GUILayout.Space(5);

			DrawIgnoresList();
		}

		internal bool TryAddNewItemToIgnores(string newItem)
		{
			if (Array.IndexOf(ignores, newItem) != -1) return false;
			ArrayUtility.Add(ref ignores, newItem);
			return true;
		}

		internal abstract void DrawTabHeader();

		internal virtual void DrawAddItemSection()
		{
			EditorGUILayout.LabelField("Add new item to the list");
			using (UIHelpers.Horizontal())
			{
				GUI.SetNextControlName("ignoresTxt");
				newItemText = EditorGUILayout.TextField(newItemText);
				if (!didFocus)
				{
					didFocus = true;
					EditorGUI.FocusTextInControl("ignoresTxt");
				}

				GUILayout.Space(5);
				GUI.SetNextControlName("AddButton");

				bool flag = currentEvent.isKey && (currentEvent.keyCode == KeyCode.Return || currentEvent.keyCode == KeyCode.KeypadEnter);
				if (flag) currentEvent.Use();
				if (GUILayout.Button("<b><color=#02C85F>ADD</color></b>", UIHelpers.compactButton, GUILayout.ExpandWidth(false)) || flag)
				{
					if (string.IsNullOrEmpty(newItemText))
					{
						window.ShowNotification(new GUIContent("You can't add empty item!"));
					}
					else if (newItemText.IndexOf('*') != -1)
					{
						window.ShowNotification(new GUIContent("Masks are not supported!"));
					}
					else
					{
						CheckNewItem();

						if (TryAddNewItemToIgnores(newItemText))
						{
							SaveChanges();
							newItemText = "";
							GUI.FocusControl("AddButton");
							didFocus = false;
						}
						else
						{
							window.ShowNotification(new GUIContent("This item already exists in the list!"));
						}
					}
				}
				GUILayout.Space(5);
			}
		}
		
		internal virtual void DrawIgnoresList()
		{
			if (ignores == null) return;

			ignoresScrollPosition = GUILayout.BeginScrollView(ignoresScrollPosition);

			foreach (string ignoredItem in ignores)
			{
				using (UIHelpers.Horizontal(UIHelpers.panelWithBackground))
				{

					if (GUILayout.Button("<b><color=#FF4040>X</color></b>", UIHelpers.compactButton, GUILayout.ExpandWidth(false)))
					{
						ArrayUtility.Remove(ref ignores, ignoredItem);
						SaveChanges();
					}
					GUILayout.Space(5);
					if (GUILayout.Button(ignoredItem, UIHelpers.richLabel))
					{
						newItemText = ignoredItem;
						GUI.FocusControl("AddButton");
						didFocus = false;
					}
				}
			}
			GUILayout.EndScrollView();

			if (ignores.Length > 0)
			{
				if (GUILayout.Button("Clear All " + name))
				{
					if (EditorUtility.DisplayDialog("Clearing the " + name + " list",
						"Are you sure you wish to clear all the ignores in the " + name + " list?",
						"Yes", "No"))
					{
						Array.Resize(ref ignores, 0);
						SaveChanges();
					}
				}
			}
		}

		internal abstract void CheckNewItem();

		protected void SaveChanges()
		{
			if (SaveChangesCallback != null)
			{
				SaveChangesCallback(ignores);
			}
		}
	}
}

#endif