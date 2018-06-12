#if UNITY_EDITOR
using CodeStage.Maintainer.Cleaner;
using CodeStage.Maintainer.Issues;
using CodeStage.Maintainer.UI;
using UnityEditor;
using UnityEngine;

namespace CodeStage.Maintainer
{
	public class Maintainer : ScriptableObject
	{
		public const string LOG_PREFIX = "<b>[Maintainer]</b> ";
		public const string VERSION = "1.2.0.3";
		public const string SUPPORT_EMAIL = "focus@codestage.ru";

		private static string directory;

		public static string Directory
		{
			get
			{
				if (!string.IsNullOrEmpty(directory)) return directory;

				Maintainer tempInstance = CreateInstance<Maintainer>();
				MonoScript script = MonoScript.FromScriptableObject(tempInstance);
				directory = AssetDatabase.GetAssetPath(script);
				DestroyImmediate(tempInstance);

				if (!string.IsNullOrEmpty(directory))
				{
					if (directory.IndexOf("Editor/Code/Maintainer.cs") >= 0)
					{
						directory = directory.Replace("/Code/Maintainer.cs", "");
					}
					else
					{
						directory = null;
						Debug.LogError(LOG_PREFIX + "Looks like Maintainer is placed in project incorrectly!\nPlease, contact me for support: " + SUPPORT_EMAIL);
					}
				}
				else
				{
					directory = null;
					Debug.LogError(LOG_PREFIX + "Can't locate the Maintainer directory!\nPlease, report to " + SUPPORT_EMAIL);
				}
				return directory;
			}
		}

		public static string ConstructError(string errorText)
		{
			return LOG_PREFIX + errorText + " Please report to " + SUPPORT_EMAIL;
		}

		/*[MenuItem("Assets/Code Stage/Maintainer/Find Issues %#&f", false, 100)]
		private static void FindAllIssues()
		{
			IssuesFinder.StartSearch(true);
		}*/

		[MenuItem("Tools/Maintainer/Show %#&`", false, 900)]
		private static void ShowWindow()
		{
			MaintainerWindow.Create();
		}

		[MenuItem("Tools/Maintainer/About", false, 901)]
		private static void ShowAbout()
		{
			MaintainerWindow.ShowAbout();
		}

		[MenuItem("Tools/Maintainer/Find Issues %#&f", false, 1000)]
		private static void FindAllIssues()
		{
			IssuesFinder.StartSearch(true);
		}

		[MenuItem("Tools/Maintainer/Find Garbage %#&g", false, 1001)]
		private static void FindAllGarbage()
		{
			ProjectCleaner.StartSearch(true);
		}
	}
}
#endif