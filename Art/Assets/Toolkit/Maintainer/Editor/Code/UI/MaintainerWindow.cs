#if UNITY_EDITOR

using CodeStage.Maintainer.Cleaner;
using CodeStage.Maintainer.Issues;
using CodeStage.Maintainer.Settings;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace CodeStage.Maintainer.UI
{
	public class MaintainerWindow : EditorWindow
	{
		private static MaintainerWindow windowInstance;

		private static bool needToRepaint;

		private int currentTab;
		private readonly string[] tabsNames = { IssuesFinder.MODULE_NAME, ProjectCleaner.MODULE_NAME, "About" };

		private IssuesTab issuesTab;
		private CleanerTab cleanerTab;
		private AboutTab aboutTab;

		public static MaintainerWindow Create()
		{
			MaintainerWindow window = GetWindow<MaintainerWindow>("Maintainer");
			window.Init();

			return window;
		}

		public static void ShowIssues()
		{
			Create().currentTab = 0;
			MaintainerSettings.Instance.selectedTabIndex = 0;
			MaintainerSettings.Save();
		}

		public static void ShowCleaner()
		{
			ShowProjectCleanerWarning();

			Create().currentTab = 1;
			MaintainerSettings.Instance.selectedTabIndex = 1;
			MaintainerSettings.Save();
		}

		public static void ShowAbout()
		{
			Create().currentTab = 2;
			MaintainerSettings.Instance.selectedTabIndex = 2;
			MaintainerSettings.Save();
		}

		public static void ShowNotification(string text)
		{
			if (windowInstance)
			{
				windowInstance.ShowNotification(new GUIContent(text));
			}
		}

		private static void ShowProjectCleanerWarning()
		{
			if (MaintainerSettings.Cleaner.firstTime)
			{
				EditorUtility.DisplayDialog(ProjectCleaner.MODULE_NAME + " PREVIEW", "Please note, this module currently has very basic features and will be greatly improved in future, stay tuned!\nThis message shows only once.", "Dismiss");
				MaintainerSettings.Cleaner.firstTime = false;
			}
		}

		[DidReloadScripts]
		private static void OnScriptsRecompiled()
		{
			needToRepaint = true;
		}

		private void Init()
		{
			minSize = new Vector2(700f, 500f);
			Focus();
			currentTab = MaintainerSettings.Instance.selectedTabIndex;

			CreateTabs();
			Refresh();
		}

		private void CreateTabs()
		{
			if (issuesTab == null)
				issuesTab = new IssuesTab();

			if (cleanerTab == null)
				cleanerTab = new CleanerTab();

			if (aboutTab == null)
				aboutTab = new AboutTab();
		}

		private void Refresh()
		{
			issuesTab.Refresh();
			cleanerTab.Refresh();
		}

		private void OnEnable()
		{
			hideFlags = HideFlags.HideAndDontSave;
			windowInstance = this;
		}

		private void OnInspectorUpdate()
		{
			if (needToRepaint)
			{
				needToRepaint = false;
				Repaint();

				currentTab = MaintainerSettings.Instance.selectedTabIndex;
			}
		}

		private void OnGUI()
		{
			CreateTabs();

			UIHelpers.SetupStyles();

			EditorGUI.BeginChangeCheck();
			currentTab = GUILayout.Toolbar(currentTab, tabsNames, GUILayout.ExpandWidth(false));
			if (EditorGUI.EndChangeCheck())
			{
				if (currentTab == 1) ShowProjectCleanerWarning();

				MaintainerSettings.Instance.selectedTabIndex = currentTab;
				MaintainerSettings.Save();
			}

			if (currentTab == 0)
			{
				issuesTab.Draw(this);
			}
			else if (currentTab == 1)
			{
				cleanerTab.Draw(this); 
			}
			else if (currentTab == 2)
			{
				aboutTab.Draw(this);
			}
		}
	}
}

#endif