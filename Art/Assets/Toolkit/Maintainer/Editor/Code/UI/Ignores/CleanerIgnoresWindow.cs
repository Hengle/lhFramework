#if UNITY_EDITOR

using CodeStage.Maintainer.Cleaner;
using CodeStage.Maintainer.Settings;

namespace CodeStage.Maintainer.UI.Ignores
{
	internal class CleanerIgnoresWindow : IgnoresWindow
	{
		internal static CleanerIgnoresWindow instance;

		internal static CleanerIgnoresWindow Create()
		{
			CleanerIgnoresWindow window = GetWindow<CleanerIgnoresWindow>(true);
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
				new PathIgnoresTab(MaintainerSettings.Cleaner.pathIgnores, OnPathIgnoresChange),
			};

			Init(ProjectCleaner.MODULE_NAME, tabs, 0, null);

			instance = this;
		}

		protected override void UnInitOnDisable()
		{
			instance = null;
		}

		private static void OnPathIgnoresChange(string[] collection)
		{
			MaintainerSettings.Cleaner.pathIgnores = collection;
			MaintainerSettings.Save();
		}
	}
}

#endif