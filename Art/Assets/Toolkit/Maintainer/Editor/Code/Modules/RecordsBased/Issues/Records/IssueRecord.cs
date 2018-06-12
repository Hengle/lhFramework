#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Text;

namespace CodeStage.Maintainer.Issues
{
	[Serializable]
	public abstract class IssueRecord: RecordBase
	{
		private static readonly Dictionary<RecordType, RecordSeverity> recordTypeSeverity = new Dictionary<RecordType, RecordSeverity>
		{
			{RecordType.EmptyArrayItem, RecordSeverity.Info},
			{RecordType.MissingComponent, RecordSeverity.Error},
			{RecordType.MissingReference, RecordSeverity.Warning},
			{RecordType.DuplicateComponent, RecordSeverity.Warning},
			{RecordType.InconsistentTerrainData, RecordSeverity.Warning},
			{RecordType.MissingPrefab, RecordSeverity.Warning},
			{RecordType.DisconnectedPrefab, RecordSeverity.Info},
			{RecordType.EmptyMeshCollider, RecordSeverity.Info},
			{RecordType.EmptyMeshFilter, RecordSeverity.Info},
			{RecordType.EmptyAnimation, RecordSeverity.Info},
			{RecordType.EmptyRenderer, RecordSeverity.Info},
			{RecordType.EmptySpriteRenderer, RecordSeverity.Info},
			{RecordType.EmptyTerrainCollider, RecordSeverity.Info},
			{RecordType.EmptyAudioSource, RecordSeverity.Info},
			{RecordType.UndefinedTag, RecordSeverity.Warning},
			{RecordType.UnnamedLayer, RecordSeverity.Info},
			{RecordType.HugePosition, RecordSeverity.Warning},
			{RecordType.DuplicateScenesInBuild, RecordSeverity.Info},
			{RecordType.DuplicateTagsAndLayers, RecordSeverity.Info},
			{RecordType.Other, RecordSeverity.Info},
			{RecordType.Error, RecordSeverity.Error}
		};

		public RecordType type;

		// ----------------------------------------------------------------------------
		// base constructors
		// ----------------------------------------------------------------------------

		protected IssueRecord(RecordType type)
		{
			this.type = type;
			severity = recordTypeSeverity[type];
		}

		protected IssueRecord(RecordType type, RecordLocation location):this(type)
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
				case RecordType.MissingComponent:
					header.Append("Missing component");
					break;
				case RecordType.MissingReference:
					header.Append("Missing reference");
					break;
				case RecordType.DuplicateComponent:
					header.Append("Duplicate component");
					break;
				case RecordType.EmptyArrayItem:
					header.Append(string.Format("Array with {0} empty item(s)", headerFormatArgument));
					break;
				case RecordType.MissingPrefab:
					header.Append("Instance of missing prefab");
					break;
				case RecordType.DisconnectedPrefab:
					header.Append("Disconnected prefab instance");
					break;
				case RecordType.EmptyMeshCollider:
					header.Append("MeshCollider without mesh");
					break;
				case RecordType.EmptyMeshFilter:
					header.Append("MeshFilter without mesh");
					break;
				case RecordType.EmptyAnimation:
					header.Append("Animation without any clips");
					break;
				case RecordType.EmptyRenderer:
					header.Append("Renderer without material");
					break;
				case RecordType.EmptySpriteRenderer:
					header.Append("SpriteRenderer without sprite");
					break;
				case RecordType.EmptyTerrainCollider:
					header.Append("TerrainCollider without Terrain Data");
					break;
				case RecordType.EmptyAudioSource:
					header.Append("AudioSource without AudioClip");
					break;
				case RecordType.UndefinedTag:
					header.Append("GameObject with undefined tag");
					break;
				case RecordType.UnnamedLayer:
					header.Append("GameObject with unnamed layer");
					break;
				case RecordType.HugePosition:
					header.Append("GameObject with huge position");
					break;
				case RecordType.InconsistentTerrainData:
					header.Append("Terrain and TerrainCollider with different Terrain Data");
					break;
				case RecordType.DuplicateScenesInBuild:
					header.Append("Same scene added to the Scenes In Build multiple times");
					break;
				case RecordType.DuplicateTagsAndLayers:
					header.Append("Duplicate item(s) found at the Tags and Layers settings");
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
	}
}

#endif