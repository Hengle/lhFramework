#if UNITY_EDITOR

using System;

namespace CodeStage.Maintainer.Settings
{
	[Serializable]
	public class ProjectCleanerSettings
	{
		public string[] pathIgnores = new string[0];

		public bool useTrashBin;

		public bool findEmptyFolders;
		public bool findEmptyFoldersAutomatically;
		public bool findEmptyScenes;

		public bool firstTime;

		public ProjectCleanerSettings()
		{
			Reset();
		}

		internal void Reset()
		{
			useTrashBin = true;

			findEmptyFolders = true;
			findEmptyFoldersAutomatically = false;
			findEmptyScenes = true;

			firstTime = true;
		}

		internal void SwitchAll(bool enable)
		{
			findEmptyFolders = enable;
			findEmptyScenes = enable;
		}
	}
}

#endif