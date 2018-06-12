#if UNITY_EDITOR

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using CodeStage.Maintainer.Cleaner;
using CodeStage.Maintainer.Issues;
using UnityEngine;

namespace CodeStage.Maintainer
{
	public class SearchResultsStorage
	{
		private const string DIRECTORY = "Temp";
		private const string PATH = DIRECTORY + "/MaintainerSearchResults.bin";

		private static IssueRecord[] issuesSearchResults;
		private static CleanerRecord[] cleanerSearchResults;

		public static IssueRecord[] IssuesSearchResults
		{
			get
			{
				if (issuesSearchResults == null)
				{
					Load();
				}
				return issuesSearchResults;
			}
			set
			{
				issuesSearchResults = value;
				Save();
			}
		}

		public static CleanerRecord[] CleanerSearchResults
		{
			get
			{
				if (cleanerSearchResults == null)
				{
					Load();
				}
				return cleanerSearchResults;
			}
			set
			{
				cleanerSearchResults = value;
				Save();
			}
		}

		public static void Load()
		{
			if (File.Exists(PATH))
			{
				BinaryFormatter bf = new BinaryFormatter();
				Stream stream = new FileStream(PATH, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

				try
				{
					issuesSearchResults = bf.Deserialize(stream) as IssueRecord[];
					cleanerSearchResults = bf.Deserialize(stream) as CleanerRecord[];
				}
				catch (Exception ex)
				{
					Debug.LogWarning(Maintainer.LOG_PREFIX + "Can't read search results!\n" + ex);
				}
				finally
				{
					stream.Close();
				}

				if (issuesSearchResults == null || cleanerSearchResults == null)
				{
					File.Delete(PATH);
				}
			}
			else
			{
				issuesSearchResults = new IssueRecord[0];
				cleanerSearchResults = new CleanerRecord[0];
			}
		}

		public static void Save()
		{
			if (issuesSearchResults == null)
			{
				issuesSearchResults = new IssueRecord[0];
            }

			if (cleanerSearchResults == null)
			{
				cleanerSearchResults = new CleanerRecord[0];
			}

			if (!Directory.Exists(DIRECTORY)) Directory.CreateDirectory(DIRECTORY);
			 
			BinaryFormatter bf = new BinaryFormatter();
			Stream stream = new FileStream(PATH, FileMode.Create, FileAccess.Write, FileShare.None);
			bf.Serialize(stream, issuesSearchResults);
			bf.Serialize(stream, cleanerSearchResults);
			stream.Close();
		}
	}
}

#endif