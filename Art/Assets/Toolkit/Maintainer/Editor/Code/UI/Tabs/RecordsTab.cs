#if UNITY_EDITOR

using System;
using CodeStage.Maintainer.Settings;
using UnityEditor;
using UnityEngine;

namespace CodeStage.Maintainer.UI
{
	internal abstract class RecordsTab
	{
		protected const int RECORDS_PER_PAGE = 100;

		protected MaintainerWindow window;
		protected Vector2 searchSectionScrollPosition;
		protected Vector2 settingsSectionScrollPosition;
		protected int recordsCurrentPage;
		protected int recordsTotalPages;

		protected RecordBase[] records;

		private IShowableRecord gotoRecord;

		internal virtual void Refresh()
		{
			records = null;
			recordsCurrentPage = 0;
			searchSectionScrollPosition = Vector2.zero;
			settingsSectionScrollPosition = Vector2.zero;
		}

		internal virtual void Draw(MaintainerWindow parentWindow)
		{
			if (records == null)
			{
				LoadLastRecords();
				if (records != null)
				{
					recordsTotalPages = (int)Math.Ceiling((double)records.Length / RECORDS_PER_PAGE);
				}
				else
				{
					Debug.Log("records null");
					recordsTotalPages = 0;
				}
			}

			window = parentWindow;

			using (UIHelpers.Horizontal())
			{
				DrawSettingsSection();
				DrawSearchSection();
			}

			if (gotoRecord != null)
			{
				gotoRecord.Show();
				gotoRecord = null;
			}

			ProcessUserActions();
		}

		protected virtual void DrawSettingsSection()
		{
			using (UIHelpers.Vertical(UIHelpers.panelWithBackground, GUILayout.ExpandHeight(true), GUILayout.Width(300)))
			{
				GUILayout.Space(10);
				GUILayout.Label("<size=14><b>Settings</b></size>", UIHelpers.centeredLabel);
				GUILayout.Space(10);

				EditorGUI.BeginChangeCheck();

				DrawSettingsBody();

				if (EditorGUI.EndChangeCheck())
				{
					MaintainerSettings.Save();
				}
			}
		}

		protected virtual void DrawSearchSection()
		{
			using (UIHelpers.Vertical(UIHelpers.panelWithBackground, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)))
			{
				GUILayout.Space(10);
				DrawSearchBody();
			}
		}

		protected virtual void ShowCollectionPages()
		{
			int fromIssue = recordsCurrentPage * RECORDS_PER_PAGE;
			int toIssue = fromIssue + Math.Min(RECORDS_PER_PAGE, records.Length - fromIssue);

			GUILayout.Label("Results " + fromIssue + " - " + toIssue + " from " + records.Length);

			searchSectionScrollPosition = GUILayout.BeginScrollView(searchSectionScrollPosition);
			for (int i = fromIssue; i < toIssue; i++)
			{
				bool recordRemoved;
				DrawRecord(i, out recordRemoved);
				if (recordRemoved)
				{
					recordsTotalPages = (int)Math.Ceiling((double)records.Length / RECORDS_PER_PAGE);
					if (recordsCurrentPage + 1 > recordsTotalPages) recordsCurrentPage = recordsTotalPages - 1;
					break;
				}
			}
			GUILayout.EndScrollView();

			if (recordsTotalPages <= 1) return;

			GUILayout.Space(5);
			using (UIHelpers.Horizontal())
			{
				GUILayout.FlexibleSpace();

				GUI.enabled = recordsCurrentPage > 0;
				if (GUILayout.Button("<<", GUILayout.Width(50)))
				{
					window.RemoveNotification();
					recordsCurrentPage = 0;
					searchSectionScrollPosition = Vector2.zero;
				}
				if (GUILayout.Button("<", GUILayout.Width(50)))
				{
					window.RemoveNotification();
					recordsCurrentPage--;
					searchSectionScrollPosition = Vector2.zero;
				}
				GUI.enabled = true;
				GUILayout.Label(recordsCurrentPage + 1 + " of " + recordsTotalPages, UIHelpers.centeredLabel, GUILayout.Width(100));
				GUI.enabled = recordsCurrentPage < recordsTotalPages - 1;
				if (GUILayout.Button(">", GUILayout.Width(50)))
				{
					window.RemoveNotification();
					recordsCurrentPage++;
					searchSectionScrollPosition = Vector2.zero;
				}
				if (GUILayout.Button(">>", GUILayout.Width(50)))
				{
					window.RemoveNotification();
					recordsCurrentPage = recordsTotalPages - 1;
					searchSectionScrollPosition = Vector2.zero;
				}
				GUI.enabled = true;

				GUILayout.FlexibleSpace();
			}
		}

		protected abstract void LoadLastRecords();
		protected abstract void ProcessUserActions();
		protected abstract void DrawSettingsBody();
		protected abstract void DrawSearchBody();
		protected abstract void DrawRecord(int recordIndex, out bool recordRemoved);

		protected void AddShowButtonIfPossible(RecordBase record)
		{
			IShowableRecord showableIssueRecord = record as IShowableRecord;
			if (showableIssueRecord != null)
			{
				string hintText;
				switch (record.location)
				{
					case RecordLocation.Unknown:
						hintText = "Oh, sorry, but looks like I have no clue about this issue.";
						break;
					case RecordLocation.Scene:
						hintText = "Selects Game Object with issue in the scene. Opens scene with target Game Object if necessary and highlights this scene in the Project Browser.";
						break;
					case RecordLocation.Asset:
						hintText = "Selects asset file in the Project Browser.";
						break;
					case RecordLocation.Prefab:
						hintText = "Selects Prefab file where sits the Game Object with issue in the Project Browser.";
						break;
					case RecordLocation.BuildSettings:
						hintText = "Opens BuildSettings window to let you check the issue.";
						break;
					case RecordLocation.TagsAndLayers:
						hintText = "Opens Tags and Layers in inspector to let you check the issue.";
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
				if (GUILayout.Button(new GUIContent("Show", hintText), UIHelpers.recordButton))
				{
					gotoRecord = showableIssueRecord;
				}
			}
		}

		protected void DrawSeverityIcon(RecordBase record)
		{
			string iconName;

			switch (record.severity)
			{
				case RecordSeverity.Error:
					iconName = "d_console.erroricon";
					break;
				case RecordSeverity.Warning:
					iconName = "d_console.warnicon";
					break;
				case RecordSeverity.Info:
					iconName = "d_console.infoicon";
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			Texture icon = EditorGUIUtility.FindTexture(iconName);
			Rect iconArea = EditorGUILayout.GetControlRect(GUILayout.Width(20), GUILayout.Height(20));
			Rect iconRect = new Rect(iconArea);
			iconRect.width = iconRect.height = 30;
			iconRect.x -= 5;
			iconRect.y -= 5;
			GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleAndCrop);
		}
	}
}

#endif