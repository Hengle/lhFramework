#if UNITY_EDITOR

using System;
using System.IO;
using System.Text;
using CodeStage.Maintainer.Settings;
using UnityEditor;
using UnityEngine;

namespace CodeStage.Maintainer.Cleaner
{
	[Serializable]
	public class AssetRecord : CleanerRecord, IShowableRecord
	{
		public string path;
		public string beautyPath;
		public string assetDatabasePath;

		internal static AssetRecord Create(RecordType type, string path)
		{
			return new AssetRecord(type, path);
		}

		protected AssetRecord(RecordType type, string path) :base(type, RecordLocation.Asset)
		{
			this.path = path;

			int index = Application.dataPath.IndexOf("/Assets");

			if (!Path.IsPathRooted(path))
			{
				assetDatabasePath = path;
			}
			else
			{
				assetDatabasePath = path.Replace('\\', '/').Substring(index + 1);
			}
			beautyPath = assetDatabasePath.Substring(7);
		}

		protected override void ConstructBody(StringBuilder text)
		{
			text.Append("<b>Path:</b> ").Append(beautyPath);
		}

		protected override bool PerformClean()
		{
			bool result;

			if (MaintainerSettings.Cleaner.useTrashBin)
			{
				result = AssetDatabase.MoveAssetToTrash(assetDatabasePath);
			}
			else
			{
				switch (type)
				{
					case RecordType.EmptyFolder:
						{

							if (Directory.Exists(path))
							{
								Directory.Delete(path, true);
							}
							break;
						}
					case RecordType.EmptyScene:
						{
							if (File.Exists(path))
							{
								File.Delete(path);
							}
							break;
						}
					case RecordType.Error:
						break;
					case RecordType.Other:
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				// removes corresponding .meta files
				AssetDatabase.DeleteAsset(assetDatabasePath);
				result = !(Directory.Exists(path) || File.Exists(path));
			}
				
			if (!result)
			{
				Debug.LogWarning(Maintainer.LOG_PREFIX + ProjectCleaner.MODULE_NAME + " can't clean asset: " + beautyPath);
			}

			return result;
		}

		public void Show()
		{
			Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(assetDatabasePath);
		}
	}
}

#endif