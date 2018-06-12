#if UNITY_EDITOR

using System;
using System.IO;
using System.Xml.Serialization;
using Debug = UnityEngine.Debug;

namespace CodeStage.Maintainer.Settings
{
	[Serializable]
	public class MaintainerSettings
	{
		private const string DIRECTORY = "ProjectSettings";
		private const string PATH = DIRECTORY + "/MaintainerSettings.asset";
		private static MaintainerSettings instance;

		public IssuesFinderSettings issuesFinderSettings;
		public ProjectCleanerSettings projectCleanerSettings;
		public int selectedTabIndex = 0;

		public static MaintainerSettings Instance
		{
			get
			{
				if (instance != null) return instance;
				instance = File.Exists(PATH) ? Load() : CreateInstance();
				return instance;
			}
		}

		public static void Save()
		{
			SaveInstance(Instance);
		}

		public static IssuesFinderSettings Issues
		{
			get
			{
				if (Instance.issuesFinderSettings == null)
				{
					Instance.issuesFinderSettings = new IssuesFinderSettings();
				}
				return Instance.issuesFinderSettings;
			}
		}

		public static ProjectCleanerSettings Cleaner
		{
			get
			{
				if (Instance.projectCleanerSettings == null)
				{
					Instance.projectCleanerSettings = new ProjectCleanerSettings();
				}
				return Instance.projectCleanerSettings;
			}
		}

		private static MaintainerSettings Load()
		{
			MaintainerSettings settings;

			if (!File.Exists(PATH))
			{
				settings = CreateNewSettingsFile();
			}
			else
			{
				settings = LoadInstance();

				if (settings == null)
				{
					File.Delete(PATH);
					settings = CreateNewSettingsFile();
				}
			}

			return settings;
		}

		private static MaintainerSettings CreateNewSettingsFile()
		{
			MaintainerSettings settingsInstance = CreateInstance();

			SaveInstance(settingsInstance);

			return settingsInstance;
		}

		private static void SaveInstance(MaintainerSettings settingsInstance)
		{
			if (!Directory.Exists(DIRECTORY)) Directory.CreateDirectory(DIRECTORY);

			XmlSerializer serializer = new XmlSerializer(typeof(MaintainerSettings));
			Stream stream = new FileStream(PATH, FileMode.Create, FileAccess.Write, FileShare.None);
			serializer.Serialize(stream, settingsInstance);
			stream.Close();
		}

		private static MaintainerSettings LoadInstance()
		{
			MaintainerSettings settingsInstance;

			XmlSerializer serializer = new XmlSerializer(typeof(MaintainerSettings));
			Stream stream = new FileStream(PATH, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

			try
			{
				settingsInstance = serializer.Deserialize(stream) as MaintainerSettings;
			}
			catch (Exception ex)
			{
				Debug.LogWarning(Maintainer.LOG_PREFIX + "Can't read settings, resetting them!\n" + ex);
				settingsInstance = null;
			}
			finally
			{
				stream.Close();
			}

			return settingsInstance;
		}

		private static MaintainerSettings CreateInstance()
		{
			MaintainerSettings newInstance = new MaintainerSettings();
			newInstance.issuesFinderSettings = new IssuesFinderSettings();
			newInstance.projectCleanerSettings = new ProjectCleanerSettings();

			return newInstance;
		}
	}
}

#endif