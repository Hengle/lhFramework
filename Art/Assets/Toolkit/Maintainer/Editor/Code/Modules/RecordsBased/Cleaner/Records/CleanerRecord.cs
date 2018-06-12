#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Text;

namespace CodeStage.Maintainer.Cleaner
{
	[Serializable]
	public abstract class CleanerRecord : RecordBase
	{
		private static readonly Dictionary<RecordType, RecordSeverity> recordTypeSeverity = new Dictionary<RecordType, RecordSeverity>
		{
			{RecordType.EmptyFolder, RecordSeverity.Info},
			{RecordType.EmptyScene, RecordSeverity.Info},
			{RecordType.Other, RecordSeverity.Info},
			{RecordType.Error, RecordSeverity.Error}
		};

		public RecordType type;
		public bool selected = true;
		public bool cleaned;

		internal bool Clean()
		{
			cleaned = PerformClean();
			return cleaned;
		}

		// ----------------------------------------------------------------------------
		// base constructors
		// ----------------------------------------------------------------------------

		protected CleanerRecord(RecordType type)
		{
			this.type = type;
			severity = recordTypeSeverity[type];
		}

		protected CleanerRecord(RecordType type, RecordLocation location):this(type)
		{
			this.location = location;
		}

		// ----------------------------------------------------------------------------
		// issue header generation
		// ----------------------------------------------------------------------------

		protected override void ConstructHeader(StringBuilder header)
		{
			switch (type)
			{
				case RecordType.EmptyFolder:
					header.Append("Empty Folder");
					break;
				case RecordType.EmptyScene:
					header.Append("Empty Scene");
					break;
				case RecordType.Error:
					header.Append("Error!");
					break;
				case RecordType.Other:
					header.Append("Other");
					break;
				default:
					header.Append("Unknown issue!");
					break;
			}
		}

		protected abstract bool PerformClean();
	}
}

#endif