#if UNITY_EDITOR

using CodeStage.Maintainer.Issues;
using CodeStage.Maintainer.Settings;

namespace CodeStage.Maintainer.UI.Ignores
{
	internal class IssuesIgnoresWindow : IgnoresWindow
	{
		internal static IssuesIgnoresWindow instance;

		internal static IssuesIgnoresWindow Create()
		{
			IssuesIgnoresWindow window = GetWindow<IssuesIgnoresWindow>(true);
			window.Focus();

			return window;
		}

		internal static void Refresh()
		{
			if (instance == null) return;

			instance.InitOnEnable();
			instance.Focus();
		}

		protected override void InitOnEnable()
		{
			TabBase[] tabs =
			{
				new PathIgnoresTab(MaintainerSettings.Issues.pathIgnores, OnPathIgnoresChange),
				new ComponentIgnoresTab(MaintainerSettings.Issues.componentIgnores, OnComponentIgnoresChange)
			};

			Init(IssuesFinder.MODULE_NAME, tabs, MaintainerSettings.Issues.ignoresTabIndex, OnTabChange);

			instance = this;
		}

		protected override void UnInitOnDisable()
		{
			instance = null;
		}

		private void OnPathIgnoresChange(string[] collection)
		{
			MaintainerSettings.Issues.pathIgnores = collection;
			MaintainerSettings.Save();
		}

		private void OnComponentIgnoresChange(string[] collection)
		{
			MaintainerSettings.Issues.componentIgnores = collection;
			MaintainerSettings.Save();
		}

		private void OnTabChange(int newTab)
		{
			MaintainerSettings.Issues.ignoresTabIndex = newTab;
			MaintainerSettings.Save();
		}
	}
}

#endif